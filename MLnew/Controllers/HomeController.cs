﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MLnew.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using MLnew.Controllers;
using Microsoft.EntityFrameworkCore;
using MLnew.Data;

namespace MLnew.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
   
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Visiteur"))
                {
                    return RedirectToAction("IndexVisiteur");
                }
                else if (User.IsInRole("Expert"))
                {
                    return RedirectToAction("IndexAdmin");
                }
            }

            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        [Authorize(Roles = "Expert")]
        public IActionResult IndexAdmin()
        {
            return View("Index");
        }
        [Authorize(Roles = "Visiteur")]
        public IActionResult IndexVisiteur()
        {
            return View("Image");
        }
        [Authorize(Roles = "Expert")]

        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Upload()
        {
            foreach (var file in Request.Form.Files)
            {
                var fileName = Path.GetFileName(file.FileName);
                var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                var filePath = Path.Combine(uploadDirectory, fileName);

                // Vérifier si le répertoire existe, sinon le créer
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var client = new HttpClient())
                {
                    var requestData = new
                    {
                        chemin = filePath,
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("http://localhost:5000/Chemin", content);

                }

                // Retourner le chemin du fichier téléchargé dans l'en-tête de réponse HTTP
                return View("Index");
            }

            // Si aucun fichier n'a été téléchargé, retourner une réponse BadRequest
            return BadRequest("Aucun fichier téléchargé.");
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
        public async Task<string> Methode_SVM(string gamma_select, float C_input, string kernel_select)
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

                return resultSVM;
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

        public async Task<ActionResult> ExecuteMachineLearningTasks(bool? mainOption1, bool? mainOption2, bool? mainOption3, bool? mainOption4, int n_est, int max_d, int min_samples, string gamma_select, float C_input, string kernel_select, string n_neighbors, string weights, string metric)
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

                
                Methode methode = new Methode
                {
                    
                    Nom = "RandomForest",
                    Param1 = n_est.ToString(),
                    Param2 = max_d.ToString(),
                    Param3 = min_samples.ToString()
                };

                _context.Methode.Add(methode);
                await _context.SaveChangesAsync();


            }


            if (mainOption3 == true)
            {
                var resultSVM = await Methode_SVM(gamma_select, C_input, kernel_select);
                ViewBag.SVMResult = resultSVM;

                Methode methode = new Methode
                {

                    Nom = "Methode_SVM",
                    Param1 = gamma_select.ToString(),
                    Param2 = C_input.ToString(),
                    Param3 = kernel_select.ToString()
                };

                _context.Methode.Add(methode);
                await _context.SaveChangesAsync();
            }

            if (mainOption4 == true)
            {
                var resultKNN = await KNN(n_neighbors, weights, metric);
                ViewBag.KNNResult = resultKNN;

                Methode methode = new Methode
                {

                    Nom = "KNN",
                    Param1 = n_neighbors.ToString(),
                    Param2 = weights.ToString(),
                    Param3 = metric.ToString()
                };

                _context.Methode.Add(methode);
                await _context.SaveChangesAsync();
            }

            // 返回到 Image 视图
            return View("Image");
        }

        //Page de precision et hyperparametre
        public ActionResult Index3()
        {
            ViewBag.Message = "Your contact page.";
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Visiteur"))
                {
                    return RedirectToAction("IndexVisiteur");
                }
                else if (User.IsInRole("Expert"))
                {
                    return View("Index3");
                }
            }
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        public async Task<ActionResult> CourbesPrecision(string ml, string hp)
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    methode = ml,
                    hyperparametre = hp
                };
                 
                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/CourbesPrecision", content);
                var result = await response.Content.ReadAsStringAsync();
                
                ViewBag.path = result;
                return View("ResultSVM");
            }
        }
        public ActionResult Historique()
        {
            ViewBag.Message = "Your contact page.";
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Visiteur"))
                {
                    return RedirectToAction("IndexVisiteur");
                }
                else if (User.IsInRole("Expert"))
                {
                    return View("Historique");
                }
            }
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
        public ActionResult Ensembliste()
        {
            ViewBag.Message = "Your contact page.";
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Visiteur"))
                {
                    return RedirectToAction("IndexVisiteur");
                }
                else if (User.IsInRole("Expert"))
                {
                    return View("Ensembliste");
                }
            }
            return RedirectToPage("/Account/Login", new { area = "Identity" });
            return View();
        }
        public ActionResult ResultSVM()
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
