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

        public async Task<ActionResult> Methode_analytique()
        {
            using (var client = new HttpClient())
            {
                var requestData = new { };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/analytique", content);
                var result = await response.Content.ReadAsStringAsync();

                ViewBag.AnalytiqueResult = result;
            }

            return View("Index");
        }
        //Random_Forest
        public async Task<ActionResult> RandomForest( int n_est, int max_d, int min_samples)
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                  
                    n_estimators = n_est,
                    max_depth = max_d,
                    min_samples_leaf = min_samples
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/RandomForest'", content);
                var result = await response.Content.ReadAsStringAsync();

                ViewBag.Result = result;
                ViewBag.method = "Random Forest";
            }

            return View("Image");
        }

        //SVM
        public async Task<ActionResult> Methode_SVM(string gamma_select,float C_input, string kernel_select )
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    Gamma = gamma_select,
                    C = C_input,
                    Kernel = kernel_select
                    
                };
            
                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/SVM", content);//modifier selon python
                var resultSVM = await response.Content.ReadAsStringAsync();

                ViewBag.SVMResult = resultSVM;
            }

            return View("Image");
        }

        //KNN
        public async Task<ActionResult> KNN(string n_neighbors, string weights, string metric)
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    Hyperparameter1 = n_neighbors,

                    Hyperparameter2 = weights,

                    Hyperparameter3 = metric,


                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/knn", content);
                var resultKNN = await response.Content.ReadAsStringAsync();

                ViewBag.Hyperparameter1 = n_neighbors;
                ViewBag.Hyperparameter2 = weights;
                ViewBag.Hyperparameter3 = metric;


                ViewBag.KNNResult = resultKNN;
            }

            return View("Image");
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
