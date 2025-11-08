using Microsoft.AspNetCore.Hosting;
using Microsoft.ML;
using StayGo.Data;
using StayGo.Models;
using StayGo.Services.ML.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StayGo.Services.ML
{
    public class MLRecommendationService
    {
        private readonly StayGoContext _context;

        private readonly MLContext _mlContext;
        private readonly string _modelPath;

        
        public MLRecommendationService(StayGoContext context, IWebHostEnvironment env)
        {
            _context = context;
            _mlContext = new MLContext();

            
            _modelPath = Path.Combine(env.ContentRootPath, "MLModels", "propiedadModel.zip");
        }

        // Entrena el modelo de recomendación
        public void TrainModel()
        {
            // Cargar datos desde las reseñas (usuario - propiedad - puntuación)
            var resenas = _context.Resenas
                .Select(r => new PropiedadRating
                {
                    userId = r.UsuarioId,                   // string (de Identity)
                    propiedadId = r.PropiedadId.ToString(), // Guid -> string
                    Label = (float)r.Puntuacion
                })
                .ToList();

            if (!resenas.Any())
            {
                Console.WriteLine("⚠️ No hay reseñas suficientes para entrenar el modelo.");
                return;
            }

            var data = _mlContext.Data.LoadFromEnumerable(resenas);

            // Configurar las opciones del algoritmo de factorización matricial
            var options = new Microsoft.ML.Trainers.MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "userIdEncoded",
                MatrixRowIndexColumnName = "propiedadIdEncoded",
                LabelColumnName = "Label",
                NumberOfIterations = 50,
                ApproximationRank = 100
            };

            // Crear el pipeline de entrenamiento
            var pipeline = _mlContext.Transforms.Conversion
                    .MapValueToKey("userIdEncoded", nameof(PropiedadRating.userId))
                .Append(_mlContext.Transforms.Conversion
                    .MapValueToKey("propiedadIdEncoded", nameof(PropiedadRating.propiedadId)))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

            // Entrenar el modelo
            var model = pipeline.Fit(data);

            // Guardar modelo entrenado
            var dir = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _mlContext.Model.Save(model, data.Schema, _modelPath);

            Console.WriteLine("✅ Modelo de recomendaciones entrenado y guardado correctamente. Ruta: " + _modelPath);
        }

        public List<Propiedad> RecommendForUser(string userId, int topN = 5)
        {
            // 1) sin propiedades => nada que hacer
            if (!_context.Propiedades.Any())
                return new List<Propiedad>();
                

            // 2) si no hay reseñas o no hay modelo, devolvemos algo para no dejar vacío
            if (!_context.Resenas.Any() || !File.Exists(_modelPath))
            {
                return _context.Propiedades.Take(topN).ToList();
            }

            // 3) cargar modelo
            var model = _mlContext.Model.Load(_modelPath, out var schema);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<PropiedadRating, PropiedadRatingPrediction>(model);

            // propiedades que el modelo conoce (las que tienen reseñas)
            var propIdsEntrenadas = _context.Resenas
                .Select(r => r.PropiedadId)
                .Distinct()
                .ToHashSet();

            // propiedades que ESTE usuario ya calificó
            var propIdsUsuario = _context.Resenas
                .Where(r => r.UsuarioId == userId)
                .Select(r => r.PropiedadId)
                .ToHashSet();

            // candidatos = conocidas por el modelo y NO calificadas por este usuario
            var candidatos = _context.Propiedades
                .Where(p => propIdsEntrenadas.Contains(p.Id) && !propIdsUsuario.Contains(p.Id))
                .ToList();

            // si no hay candidatos (porque el user ya calificó todo lo entrenado), devolvemos algo
            if (!candidatos.Any())
            {
                return _context.Propiedades
                    .Where(p => !propIdsUsuario.Contains(p.Id))
                    .Take(topN)
                    .ToList();
            }

            var predictions = new List<(Propiedad prop, float score)>();

            foreach (var p in candidatos)
            {
                var pred = predictionEngine.Predict(new PropiedadRating
                {
                    userId = userId,
                    propiedadId = p.Id.ToString()
                });

                // solo agregamos si no es NaN
                if (!float.IsNaN(pred.Score))
                {
                    predictions.Add((p, pred.Score));
                }
            }


            if (!predictions.Any())
            {
                // al menos devolvemos las propiedades que el modelo sí conoce y el user no ha calificado
                return candidatos.Take(topN).ToList();
            }

            
            return predictions
                .OrderByDescending(x => x.score)
                .Take(topN)
                .Select(x => x.prop)
                .ToList();
        }

    }
}
