using Microsoft.ML;
using StayGo.Data;
using StayGo.Models;
using StayGo.Services.ML.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StayGo.Services.ML
{
    public class MLRecommendationService
    {
        private readonly StayGoContext _context;
        private readonly string _modelPath = "MLModels/propiedadModel.zip";
        private readonly MLContext _mlContext;

        public MLRecommendationService(StayGoContext context)
        {
            _context = context;
            _mlContext = new MLContext();
        }

        // Entrena el modelo de recomendación
        public void TrainModel()
        {
            // 1️⃣ Cargar datos desde las reseñas (usuario - propiedad - puntuación)
            var reseñas = _context.Resenas
                .Where(r => r.Puntuacion != null)
                .Select(r => new PropiedadRating
                {
                    userId = r.UsuarioId.ToString(),
                    propiedadId = r.PropiedadId.ToString(),
                    Label = (float)r.Puntuacion
                })
                .ToList();

            if (!reseñas.Any())
            {
                Console.WriteLine("⚠️ No hay reseñas suficientes para entrenar el modelo.");
                return;
            }

            var data = _mlContext.Data.LoadFromEnumerable(reseñas);

            // 2️⃣ Configurar las opciones del algoritmo de factorización matricial
            var options = new Microsoft.ML.Trainers.MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "userIdEncoded",
                MatrixRowIndexColumnName = "propiedadIdEncoded",
                LabelColumnName = "Label",
                NumberOfIterations = 50,
                ApproximationRank = 100
            };

            // 3️⃣ Crear el pipeline de entrenamiento
            var pipeline = _mlContext.Transforms.Conversion
                    .MapValueToKey("userIdEncoded", nameof(PropiedadRating.userId))
                .Append(_mlContext.Transforms.Conversion
                    .MapValueToKey("propiedadIdEncoded", nameof(PropiedadRating.propiedadId)))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

            // 4️⃣ Entrenar el modelo
            var model = pipeline.Fit(data);

            // 5️⃣ Guardar modelo entrenado
            _mlContext.Model.Save(model, data.Schema, _modelPath);

            Console.WriteLine("✅ Modelo de recomendaciones entrenado y guardado correctamente.");
        }

        // Genera recomendaciones para un usuario
        public List<Propiedad> RecommendForUser(Guid userId, int topN = 5)
        {
            if (!_context.Resenas.Any())
            {
                Console.WriteLine("⚠️ No hay reseñas en la base de datos para generar recomendaciones.");
                return new List<Propiedad>();
            }

            // 1️⃣ Cargar el modelo entrenado
            if (!System.IO.File.Exists(_modelPath))
            {
                Console.WriteLine("⚠️ El modelo no existe. Entrénalo primero ejecutando TrainModel().");
                return new List<Propiedad>();
            }

            var model = _mlContext.Model.Load(_modelPath, out var schema);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<PropiedadRating, PropiedadRatingPrediction>(model);

            // 2️⃣ Obtener todas las propiedades disponibles
            var propiedades = _context.Propiedades.ToList();
            var predictions = new List<(Propiedad propiedad, float score)>();

            // 3️⃣ Calcular puntuación predicha para cada propiedad
            foreach (var p in propiedades)
            {
                var prediction = predictionEngine.Predict(new PropiedadRating
                {
                    userId = userId.ToString(),
                    propiedadId = p.Id.ToString()
                });

                predictions.Add((p, prediction.Score));
            }

            // 4️⃣ Devolver las propiedades con mejor puntuación
            return predictions
                .OrderByDescending(p => p.score)
                .Take(topN)
                .Select(p => p.propiedad)
                .ToList();
        }
    }
}
