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
  
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }

            
        public ActionResult Analytic()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Methode_analytique()
        {
            using (var client = new HttpClient())
            {
                var requestData = new {};

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/analytique", content);
                var result = await response.Content.ReadAsStringAsync();

                ViewBag.Result = result;
            }

            return View("ResultatAnalytique");
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

        public async Task<ActionResult> Methode_analytique()
        {
            using (var client = new HttpClient())
            {
                var requestData = new { };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/analytique", content);
                var result = await response.Content.ReadAsStringAsync();

                ViewBag.Result = result;
            }

            return View("ResultAnalytique");
        }

        //Random Forest

        [HttpPost]
         public async Task<ActionResult> RandomForest(string hyperparameter1, string hyperparameter2, string hyperparameter3)//à remplacer suivant le nom donneé par Randy
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
                 var response = await client.PostAsync("http://localhost:5000/RandomForest", content);//à remplacer suivant le nom donneé par Randy
                 var resultRF = await response.Content.ReadAsStringAsync();

                 ViewBag.Hyperparameter1 = hyperparameter1;
                 ViewBag.Hyperparameter2 = hyperparameter2;
                 ViewBag.Hyperparameter3 = hyperparameter3;
                 ViewBag.RFResult = resultRF; //Resultat Random Forest
             }

             return View("ResultRF");
         }

        //SVM
        [HttpPost]
        public async Task<ActionResult> SVM(float C_input, string kernel_select, string gamma_select)
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    C = C_input,
                    Kernel = kernel_select,
                    Gamma = gamma_select
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/SVM", content);//modifier selon python
                var result = await response.Content.ReadAsStringAsync();

                ViewBag.Result = result;
            }

            return View("Index");
        }

        /*KNN*/
        [HttpPost]
        public async Task<ActionResult> KNN(string hyperparameter1, string selectedItem1, string selectedItem2)
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


        public ActionResult Image()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
<<<<<<< HEAD
       

=======

        public ActionResult Fin()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


>>>>>>> a815e79ab5a8ed6693f0a120594243618750bf01
    }
}