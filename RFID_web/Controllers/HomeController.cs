using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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


        [HttpPost]
        public async Task<ActionResult> SVM(float C_input, string kernel_select, int degree_input, string gamma_select)
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    C = C_input,
                    Kernel = kernel_select,
                    Degree = degree_input,
                    Gamma = gamma_select
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/SVM", content);//modifier selon python
                var result = await response.Content.ReadAsStringAsync();

                ViewBag.Result = result;
            }

            return View("Index");
        }

        public ActionResult Image()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Fin()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


    }
}