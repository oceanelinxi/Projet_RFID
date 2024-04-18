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

        /*RANDOM FOREST*/
        /*[HttpPost]
         public async Task<IActionResult> RandomForest(string hyperparameter1, string hyperparameter2, string hyperparameter3)/*à remplacer suivant le nom donneé par Randy
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
                 var response = await client.PostAsync("http://localhost:5000/RandomForest", content);/*à remplacer suivant le nom donneé par Randy
                 var resultRF = await response.Content.ReadAsStringAsync();

                 ViewBag.Hyperparameter1 = hyperparameter1;
                 ViewBag.Hyperparameter2 = hyperparameter2;
                 ViewBag.Hyperparameter3 = hyperparameter3;
                 ViewBag.RFResult = resultRF; //Resultat Random Forest
             }

             return View("ResultRF");
         }*/
        /*KNN*/
        [HttpPost]
        public async Task<IActionResult> KNN(string hyperparameter1, string selectedItem1, string selectedItem2)
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    Hyperparameter1 = hyperparameter1,

                    Hyperparameter2 = selectedItem1,

                    Hyperparameter3 = selectedItem2,

                   
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/KNN", content);
                var resultKNN = await response.Content.ReadAsStringAsync();

                ViewBag.Hyperparameter1 = hyperparameter1;
                ViewBag.Hyperparameter2 = selectedItem1;
                ViewBag.Hyperparameter3 = selectedItem2;
                

                ViewBag.KNNResult = resultKNN;
            }

            return View("ResultKNN");
        }
        

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
        public IActionResult ResultRF()
        {
            return View();

        }
        public IActionResult ResultKNN()
        {
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
