using Microsoft.AspNetCore.Mvc;
using StayGo.Services.ML;
using StayGo.Data;
using System.Linq;
using System.Security.Claims;

namespace StayGo.Controllers
{
    public class RecomendacionController : Controller
    {
        private readonly MLRecommendationService _ml;
        private readonly StayGoContext _context;

        public RecomendacionController(MLRecommendationService ml, StayGoContext context)
        {
            _ml = ml;
            _context = context;
        }

        public IActionResult Index()
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // si no hay usuario logueado, usar cualquiera que tenga reseñas
            if (string.IsNullOrEmpty(userId))
            {
                userId = _context.Resenas
                    .Select(r => r.UsuarioId)
                    .FirstOrDefault();
            }

           
            if (string.IsNullOrEmpty(userId))
            {
                ViewBag.Mensaje = "No hay reseñas para generar recomendaciones.";
                return View(Enumerable.Empty<StayGo.Models.Propiedad>());
            }

            //  pedir recomendaciones al servicio
            var recomendaciones = _ml.RecommendForUser(userId, 5);

            
            if (recomendaciones == null || !recomendaciones.Any())
            {
                ViewBag.Mensaje = "No hay recomendaciones disponibles en este momento.";
            }

            return View(recomendaciones);
        }

        // opcional: entrenar desde el navegador
        [HttpGet]
        public IActionResult Entrenar()
        {
            _ml.TrainModel();
            TempData["msg"] = "Modelo entrenado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
