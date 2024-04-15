using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RFID_web.Controllers
{
    /*[HttpPost]
      public async Task<IActionResult> Upload(List<IFormFile> files)
      {
          long size = files.Sum(f => f.Length);

          foreach (var formFile in files)
          {
              if (formFile.Length > 0)
              {
                  // Chemin où le fichier sera sauvegardé
                  var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", formFile.FileName);

                  using (var stream = new FileStream(filePath, FileMode.Create))
                  {
                      await formFile.CopyToAsync(stream);
                  }
              }
          }

          // Redirigez ou affichez un message de succès
          return RedirectToAction("Index"); // Ou retournez un message de succès
      }*/
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Select()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Image()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    
    }
}