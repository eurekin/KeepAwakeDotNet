using Microsoft.AspNetCore.Mvc; 
 

namespace KeepAwakeServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly KeepAwakeService _keepAwakeService;

        public HomeController(KeepAwakeService keepAwakeService)
        {
            _keepAwakeService = keepAwakeService;
        }

        public IActionResult Index()
        {
            ViewBag.CurrentTime = DateTime.Now.ToString("T");
            ViewBag.TimeLeft = _keepAwakeService.TimeLeft();
            return View();
        }

        [HttpPost("/extend")]
        public IActionResult Extend([FromBody] DurationModel model)
        {
            TimeSpan duration;
            if (TimeSpan.TryParse(model.Duration, out duration))
            {
                _keepAwakeService.Extend(duration);
                return Json(new { timeLeft = _keepAwakeService.TimeLeft() });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("/reset")]
        public IActionResult Reset()
        {
            _keepAwakeService.Stop();
            return Json(new { timeLeft = "" });
        }
        
 
    }

    public class DurationModel
    {
        public string? Duration { get; set; }
    }
}
