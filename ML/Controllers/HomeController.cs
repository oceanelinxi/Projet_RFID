using Microsoft.AspNetCore.Mvc;
using ML.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;

namespace ML.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        /*[HttpPost]
      public async Task<IActionResult> Analytical()
       {
           using (var client = new HttpClient())
           {

               var content = new StringContent(JsonConvert.SerializeObject(null), System.Text.Encoding.UTF8, "application/json");
               var response = await client.PostAsync("http://localhost:5200/analytical", content);
               var result = await response.Content.ReadAsStringAsync();
               var meth = "Tabry1";

               ViewBag.Result = result;
               ViewBag.Metho = meth;
           }

           return View("Page2");
       }*/
        [HttpPost]
         public async Task<IActionResult> RFClassifier(string hyperparameter1, string hyperparameter2, string hyperparameter3 )
         {
             using (var client = new HttpClient())
             {
                 var requestData = new
                 {
                     Hyperparameter1 = hyperparameter1,

                     Hyperparameter2 = hyperparameter2,

                     Hyperparameter3 = hyperparameter3

                 };

                 var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                 var response = await client.PostAsync("http://localhost:5000/RFClassifier", content);
                 var result1 = await response.Content.ReadAsStringAsync();

                 ViewBag.Hyperparameter1 = hyperparameter1;
                 ViewBag.Hyperparameter2 = hyperparameter2;
                 ViewBag.Hyperparameter3 = hyperparameter3;
                 ViewBag.ResultRF = result1; //Resultat Random Forest
             }

             return View("ResultRF");
         }

        /*[HttpPost]
        public async Task<IActionResult> KNNClassifier(string hyperparameter1, string selectedItem1, string selectedItem2, string selectedItem3)
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    Hyperparameter1 = hyperparameter1,

                    Hyperparameter2 = selectedItem1,

                    Hyperparameter3 = selectedItem2,

                    Hyperparameter4 = selectedItem3
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/KNNClassifier", content);
                var result4 = await response.Content.ReadAsStringAsync();

                ViewBag.Hyperparameter1 = hyperparameter1;
                ViewBag.Hyperparameter2 = selectedItem1;
                ViewBag.Hyperparameter3 = selectedItem2;
                ViewBag.Hyperparameter4 = selectedItem3;

                ViewBag.Result4 = result4;
            }

            return View("ResultMethode4");
        }
        */

        /*[HttpPost("upload")]
        public IActionResult Upload(IFormFile file)
        {
            // Vérifier si le fichier est présent
            if (file == null || file.Length == 0)
                return BadRequest("Un fichier doit être fourni.");

            // Vérifier l'extension du fichier
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (fileExtension != ".zip")
                return BadRequest("Le fichier doit être au format ZIP.");

            // Vérifier le type MIME du fichier
            if (file.ContentType != "application/zip" && file.ContentType != "application/x-zip-compressed" &&
                file.ContentType != "application/x-compressed" && file.ContentType != "multipart/x-zip")
                return BadRequest("Le fichier doit être au format ZIP.");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);

            // Sauvegarder le fichier
            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Extraire le contenu du fichier ZIP
            var extractPath = Path.Combine(Directory.GetCurrentDirectory(), "extracted");
            try
            {
                ZipFile.ExtractToDirectory(path, extractPath);
            }
            catch (InvalidDataException)
            {
                return BadRequest("Le fichier fourni n'est pas un fichier ZIP valide.");
            }

            // Ici, vous pouvez ajouter votre logique pour traiter les fichiers extraits
            // ...

            return Ok(new { message = "Fichier téléchargé et extrait avec succès." });
        }
        */
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
