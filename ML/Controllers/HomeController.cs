using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ML.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing.Drawing2D;
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

        public async Task<string> Methode_analytique()
        {
            using (var client = new HttpClient())
            {
                var requestData = new { };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/analytique", content);
                var result = await response.Content.ReadAsStringAsync();

              

                return result;
            }
     
        }

        //Random_Forest
        public async Task<string> RandomForest(int n_est, int max_d, int min_samples)
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
                var response = await client.PostAsync("http://localhost:5000/RandomForest", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }





        //SVM
        public async Task<string> Methode_SVM(string gamma_select,float C_input, string kernel_select )
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

                return  resultSVM;
            }

     
        }

        //KNN
        public async Task<string> KNN(string n_neighbors, string weights, string metric)
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


                return resultKNN;
            }

          
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


        public async Task<ActionResult>  ExecuteMachineLearningTasks(bool? mainOption1, bool? mainOption2, bool? mainOption3, bool? mainOption4, int n_est, int max_d, int min_samples, string gamma_select, float C_input, string kernel_select, string n_neighbors, string weights, string metric)
        {
         
            if (mainOption1 == true)
            {
                var resultAnalytique = await Methode_analytique();
                ViewBag.AnalytiqueResult = resultAnalytique;
            }


            if (mainOption2 == true)
            {
                var resultRandomForest = await RandomForest(n_est, max_d, min_samples);
                ViewBag.RandomForestResult = resultRandomForest;
            }

            if (mainOption3 == true)
            {
                var resultSVM = await Methode_SVM(gamma_select, C_input, kernel_select);
                ViewBag.SVMResult = resultSVM;
            }

            if (mainOption4 == true)
            {
                var resultKNN = await KNN(n_neighbors, weights, metric);
                ViewBag.KNNResult = resultKNN;
            }

            // 返回到 Image 视图
            return View("Image");
        }




    }
}


