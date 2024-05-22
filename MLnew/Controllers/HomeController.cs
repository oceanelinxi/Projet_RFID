using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MLnew.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using MLnew.Controllers;
using Microsoft.EntityFrameworkCore;
using MLnew.Data;
using Microsoft.AspNetCore.Http;
using NuGet.Protocol;
using System.Security.Claims;
using System.Drawing;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MLnew.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
           
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Visiteur"))
                {
                    return RedirectToAction("IndexHistorique");
                }
                else if (User.IsInRole("Expert") || User.IsInRole("Administrateur"))
                {
                    return RedirectToAction("IndexAdmin");
                }
            }

            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        [Authorize(Roles = "Expert,Administrateur")]
        public IActionResult IndexAdmin()
        {
            return View("Index");
        }

        [Authorize(Roles = "Visiteur")]
        public IActionResult IndexVisiteur()
        {
            return View("Image");
        }

        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> ConnectionHistory()
        {
            var connectionHistories = await _context.ConnectionHistory.ToListAsync();
            return View(connectionHistories);
        }

        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> ManageRole()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Logique pour supprimer l'utilisateur
            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(ManageRole));
        }

        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> AssignRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.UserId = user.Id;
            ViewBag.Email = user.Email;
            ViewBag.Roles = roles.Select(role => new SelectListItem
            {
                Value = role.Name,
                Text = role.Name,
                Selected = userRoles.Contains(role.Name)
            }).ToList();

            return View("AssignRoles");
        }

        [HttpPost]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> AssignRoles(string userId, string selectedRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Contains(selectedRole))
            {
                ViewBag.Message = $"L'utilisateur a déjà le rôle '{selectedRole}'.";
                var roles = await _roleManager.Roles.ToListAsync();
                ViewBag.UserId = user.Id;
                ViewBag.Email = user.Email;
                ViewBag.Roles = roles.Select(role => new SelectListItem
                {
                    Value = role.Name,
                    Text = role.Name,
                    Selected = userRoles.Contains(role.Name)
                }).ToList();
                return View();
            }

            var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles.");
                return View();
            }

            if (!string.IsNullOrEmpty(selectedRole))
            {
                var addResult = await _userManager.AddToRoleAsync(user, selectedRole);
                if (!addResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to add user role.");
                    return View();
                }
            }


            return RedirectToAction(nameof(ManageRole));
        }

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
                client.Timeout = TimeSpan.FromSeconds(200);
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
                client.Timeout = TimeSpan.FromSeconds(200);
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


        //Adaboost
        public async Task<string> AdaBoostRF(int n_est, int max_d, int min_samples, string criterion, int min_samples_split, float min_weight_fraction_leaf, string max_features, int max_leaf_nodes, float min_impurity_decrease, bool bootstrap, bool oob_score, int n_jobs, int random_state, int verbose, bool warm_start, string class_weight, float ccp_alpha, int max_samples, int cv_folds)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    n_estimators = n_est,
                    max_depth = max_d,
                    min_samples_leaf = min_samples,
                    critere = criterion,
                    min_s_s = min_samples_split,
                    min_w_f = min_weight_fraction_leaf,
                    max_feat = max_features,
                    max_l_n = max_leaf_nodes,
                    min_impurity = min_impurity_decrease,
                    boot = bootstrap,
                    oob = oob_score,
                    n_job = n_jobs,
                    random = random_state,
                    verbo = verbose,
                    warm = warm_start,
                    class_w = class_weight,
                    ccp = ccp_alpha,
                    max_sample = max_samples,
                    cv = cv_folds
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/AdaRF", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }

       
        public async Task<string> AdaBoostSVM(string C_input_svm, string kernel_select, string gamma_select, int degree, float coef0, bool shrinking, bool probability, string tol, float cache_size,string class_weight_svm, bool verbose_svm, int max_iter, string decision_function_shape, bool break_ties, int random_state_svm, int cv_folds_svm)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    C = C_input_svm,
                    kernels = kernel_select,
                    gamma = gamma_select,
                    degre = degree,
                    coef = coef0,
                    shrinkings = shrinking,
                    prob = probability,
                    tols = tol,
                    cach_size = cache_size,
                    max_it = max_iter,
                    decision_func = decision_function_shape,
                    random_s = random_state_svm,
                    verbos = verbose_svm,
                    break_t = break_ties,
                    class_we = class_weight_svm,
                    cv_svm = cv_folds_svm,
                   
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/AdaSVM", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }

        public async Task<string> AdaBoostKNN(int n_neighbors, string weights, string metric, string algorithm, int leaf_size, int n_jobs_knn, string p,int cv_folds_knn)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    n_neighbor = n_neighbors,
                    weight = weights,
                    metrics = metric,
                    algo = algorithm,
                    leaf_sizes = leaf_size,
                    n_job_knn = n_jobs_knn,
                    p_knn = p,
                    cv_fold_knn= cv_folds_knn


                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/AdaKNN", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
        //XGBoost
        public async Task<string> XGBoost(string booster, int silent, int verbosity, string objective, string eval_metric, int n_estimators, int early_stopping_rounds, int seed, int nthread)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    Booster = booster,
                    Silent = silent,
                    Verbosity = verbosity,
                    Objective = objective,
                    EvalMetric = eval_metric,
                    N_estimators = n_estimators,
                    EarlyStopping = early_stopping_rounds,
                    Seed = seed,
                    Nthread = nthread
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/XGBoost", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
        public async Task<string> XGBoostKNN(int knn_neighbors, string booster, int silent, int verbosity, string objective, string eval_metric, int n_estimators, int early_stopping_rounds, int seed, int nthread)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    Knn_neighbors = knn_neighbors,
                    Booster = booster,
                    Silent = silent,
                    Verbosity = verbosity,
                    Objective = objective,
                    EvalMetric = eval_metric,
                    N_estimators = n_estimators,
                    EarlyStopping = early_stopping_rounds,
                    Seed = seed,
                    Nthread = nthread
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/XGBoostKNN", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
        public async Task<string> XGBoostSVM(string svm_kernel, string booster, int silent, int verbosity, string objective, string eval_metric, int n_estimators, int early_stopping_rounds, int seed, int nthread)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    Svm_kernel=svm_kernel,
                    Booster = booster,
                    Silent = silent,
                    Verbosity = verbosity,
                    Objective = objective,
                    EvalMetric = eval_metric,
                    N_estimators = n_estimators,
                    EarlyStopping = early_stopping_rounds,
                    Seed = seed,
                    Nthread = nthread
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/XGBoostSVM", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
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

        public async Task<ActionResult> ExecuteMachineLearningTasks(bool? mainOption1, bool? mainOption2, bool? mainOption3, bool? mainOption4,
                     int n_est, int max_d, int min_samples,
                     string gamma_select, float C_input, string kernel_select,
                     string n_neighbors, string weights, string metric)
        {
            //Création de la simulation en  BDD
            Historique hist = new Historique
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                DateSimulation = DateTime.Now,
            };
            _context.Historique.Add(hist);
            await _context.SaveChangesAsync();
            var all_hists = await _context.Historique.ToListAsync();
            int n_hist = all_hists.Count;

            if (mainOption1 == true)
            {
                var resultAnalytique = await Methode_analytique();
                ViewBag.AnalytiqueResult = resultAnalytique;

                dynamic jsonResult = JsonConvert.DeserializeObject(resultAnalytique);
                var acc = 98.2;
                if (jsonResult != null && jsonResult.accuracy != null)
                {
                    acc = (float)jsonResult.accuracy;
                }

                Methode methode = new Methode
                {
                    Nom = "MethodeAnalytique",
                    Param1 = "aucun",
                    Param2 = "aucun",
                    Param3 = "aucun"
                };
                _context.Methode.Add(methode);

                await _context.SaveChangesAsync();
                var all_methods = await _context.Methode.ToListAsync();
                int size = all_methods.Count;
                Simulation simulation = new Simulation
                {
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    MethodeId = all_methods[size - 1].Id,
                    Accuracy = (float)acc,
                    DateSimulation = DateTime.Now,
                    Duree = jsonResult.duree
                };

                _context.Simulation.Add(simulation);


                await _context.SaveChangesAsync();

            }

            if (mainOption2 == true)
            {
                // Obtention des resultats python
                var resultRandomForest = await RandomForest(n_est, max_d, min_samples);
                ViewBag.RandomForestResult = resultRandomForest;

                dynamic jsonResult = JsonConvert.DeserializeObject(resultRandomForest);
                var acc = (float)0.00; int duree = -1;
                if (jsonResult != null && jsonResult.accuracy != null)
                {
                    acc = (float)jsonResult.accuracy;
                }
                if (jsonResult != null && jsonResult.duree != null)
                {
                    duree = jsonResult.duree;
                }

                //Enrgistrement dans la base de données
                Modele model = new Modele
                {
                    HistoriqueID = all_hists[n_hist - 1].HistoriqueID,
                    DureeSec = duree,
                    Nom = "RandomForest",
                    Accuracy = (float)acc
                };
                _context.Modele.Add(model);
                await _context.SaveChangesAsync();
                var all_models = await _context.Modele.ToListAsync();
                int n_model = all_models.Count;

                // n_estimators
                Parametre param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "n_estimators",
                    Valeur = Convert.ToString(n_est)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // max_depth
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "max_depth",
                    Valeur = Convert.ToString(max_d)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // min_samples_leaf
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "min_samples_leaf",
                    Valeur = Convert.ToString(min_samples)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

            }

            if (mainOption3 == true)
            {
                var resultSVM = await Methode_SVM(gamma_select, C_input, kernel_select);
                ViewBag.SVMResult = resultSVM;

                dynamic jsonResult = JsonConvert.DeserializeObject(resultSVM);
                var acc = 98.2; int duree = -1;
                if (jsonResult != null && jsonResult.accuracy != null)
                {
                    acc = (float)jsonResult.accuracy;
                }
                if (jsonResult != null && jsonResult.duree != null)
                {
                    duree = jsonResult.duree;
                }

                //Enrgistrement dans la base de données
                Modele model = new Modele
                {
                    HistoriqueID = all_hists[n_hist - 1].HistoriqueID,
                    DureeSec = duree,
                    Nom = "SVM",
                    Accuracy = (float)acc
                };
                _context.Modele.Add(model);
                await _context.SaveChangesAsync();
                var all_models = await _context.Modele.ToListAsync();
                int n_model = all_models.Count;

                // Gamma
                Parametre param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "gamma",
                    Valeur = Convert.ToString(gamma_select)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // C
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "C",
                    Valeur = Convert.ToString(C_input)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // kernel
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "kernel",
                    Valeur = Convert.ToString(kernel_select)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();
            }

            if (mainOption4 == true)
            {
                var resultKNN = await KNN(n_neighbors, weights, metric);
                ViewBag.KNNResult = resultKNN;

                dynamic jsonResult = JsonConvert.DeserializeObject(resultKNN);
                var acc = 98.2; int duree = -1;
                if (jsonResult != null && jsonResult.accuracy != null)
                {
                    acc = (float)jsonResult.accuracy;
                }
                if (jsonResult != null && jsonResult.duree != null)
                {
                    duree = jsonResult.duree;
                }

                //Enrgistrement dans la base de données
                Modele model = new Modele
                {
                    HistoriqueID = all_hists[n_hist - 1].HistoriqueID,
                    DureeSec = duree,
                    Nom ="KNN",
                    Accuracy = (float)acc
                };
                _context.Modele.Add(model);
                await _context.SaveChangesAsync();
                var all_models = await _context.Modele.ToListAsync();
                int n_model = all_models.Count;

                // n_neighbors
                Parametre param4 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "n_neighbors",
                    Valeur = Convert.ToString(n_neighbors)
                };
                _context.Parametre.Add(param4);
                await _context.SaveChangesAsync();

                // metric
                param4 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "metric",
                    Valeur = Convert.ToString(metric)
                };
                _context.Parametre.Add(param4);
                await _context.SaveChangesAsync();

                // weights
                param4 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "weights",
                    Valeur = Convert.ToString(weights)
                };
                _context.Parametre.Add(param4);
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
                else if (User.IsInRole("Expert") || User.IsInRole("Administrateur"))
                {
                    return View("Index3");
                }
            }
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }


        public async Task<ActionResult> MethodesEnsemblistes(bool? mainOption1, bool? mainOption2, int n_est, int max_d, int min_samples, string criterion, int min_samples_split, float min_weight_fraction_leaf, string max_features, int max_leaf_nodes, float min_impurity_decrease, bool bootstrap, bool oob_score, int n_jobs, int random_state, int verbose, bool warm_start, string class_weight, float ccp_alpha, int max_samples, int cv_folds, string algorithmSelect,
            string C_input_svm, string kernel_select, string gamma_select, int degree, float coef0, bool shrinking, bool probability, string tol, float cache_size, string class_weight_svm, bool verbose_svm, int max_iter, string decision_function_shape, bool break_ties, int random_state_svm, int cv_folds_svm,
            int n_neighbors,string weights,string metric,string algorithm,int leaf_size,int n_jobs_knn,string p, int cv_folds_knn, string booster, int silent, int verbosity, string objective, string eval_metric, int n_estimators, int early_stopping_rounds, int seed, int nthread, int knn_neighbors, string svm_kernel)
        {
            if (mainOption1 == true)
            {
                if (algorithmSelect == "Random Forest")
                {

                    var resultXGBoost = await  XGBoost(booster, silent, verbosity, objective, eval_metric, n_estimators, early_stopping_rounds, seed, nthread);
                    ViewBag.XGBoost = resultXGBoost;

                    dynamic jsonResult = JsonConvert.DeserializeObject(resultXGBoost);
                    var acc = 98.2;
                    if (jsonResult != null && jsonResult.mean_accuracy != null)
                    {
                        acc = (float)jsonResult.mean_accuracy;
                    }
                }
                if (algorithmSelect == "KNN")
                {
                    var resultXGBoostKNN = await XGBoostKNN(knn_neighbors,booster,silent,verbosity,objective,eval_metric,n_estimators,early_stopping_rounds,seed,nthread);
                    ViewBag.XGBoostKNN = resultXGBoostKNN;

                    dynamic jsonResult = JsonConvert.DeserializeObject(resultXGBoostKNN);
                    var acc = 98.2;
                    if (jsonResult != null && jsonResult.mean_accuracy != null)
                    {
                        acc = (float)jsonResult.mean_accuracy;
                    }
                }
                if (algorithmSelect == "SVM")
                {
                    var resultXGBoostSVM = await XGBoostSVM(svm_kernel, booster, silent, verbosity, objective, eval_metric, n_estimators, early_stopping_rounds, seed, nthread);
                    ViewBag.XGBoostSVM = resultXGBoostSVM;

                    dynamic jsonResult = JsonConvert.DeserializeObject(resultXGBoostSVM);
                    var acc = 98.2;
                    if (jsonResult != null && jsonResult.mean_accuracy != null)
                    {
                        acc = (float)jsonResult.mean_accuracy;
                    }

                }



            }

            if (mainOption2 == true)
            {
                if (algorithmSelect == "Random Forest")
                {

                    var resultAdaRF = await AdaBoostRF(n_est, max_d, min_samples, criterion, min_samples_split, min_weight_fraction_leaf, max_features, max_leaf_nodes, min_impurity_decrease, bootstrap, oob_score, n_jobs, random_state, verbose, warm_start, class_weight, ccp_alpha, max_samples, cv_folds);
                    ViewBag.AdaBoostRF = resultAdaRF;

                    dynamic jsonResult = JsonConvert.DeserializeObject(resultAdaRF);
                    var acc = 98.2;
                    if (jsonResult != null && jsonResult.mean_accuracy != null)
                    {
                        acc = (float)jsonResult.mean_accuracy;
                    }
                }
                if (algorithmSelect == "SVM")
                {
                    var resultAdaSVM = await AdaBoostSVM(C_input_svm, kernel_select, gamma_select, degree, coef0, shrinking, probability, tol, cache_size, class_weight_svm, verbose_svm, max_iter, decision_function_shape, break_ties, random_state_svm, cv_folds_svm);
                    ViewBag.AdaBoostSVM = resultAdaSVM;

                    dynamic jsonResult = JsonConvert.DeserializeObject(resultAdaSVM);
                    var acc = 98.2;
                    if (jsonResult != null && jsonResult.mean_accuracy != null)
                    {
                        acc = (float)jsonResult.mean_accuracy;
                    }
                }
                if(algorithmSelect =="KNN")
                {
                    var resultAdaKNN = await AdaBoostKNN( n_neighbors,  weights,  metric,  algorithm,  leaf_size,  n_jobs_knn, p, cv_folds_knn);
                    ViewBag.AdaBoostKNN = resultAdaKNN;

                    dynamic jsonResult = JsonConvert.DeserializeObject(resultAdaKNN);
                    var acc = 98.2;
                    if (jsonResult != null && jsonResult.mean_accuracy != null)
                    {
                        acc = (float)jsonResult.mean_accuracy;
                    }

                }

            }





            // 返回到 Image 视图
            return View("Image2");
        }
        public async Task<ActionResult> CourbesPrecision(string ml, string hp)
        {
            using (var client = new HttpClient())
            {
               ViewBag.MLmodel = ml;
               ViewBag.hparam = hp;
                //ViewBag.path = result;
               return View("ResultSVM");
            }
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
                else if (User.IsInRole("Expert") || User.IsInRole("Administrateur"))
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
        public async Task<ActionResult> Historique(string start, string end, string userIds,
                 string modelName)
        {
            dynamic users = GetAllUsers();
            
            DateTime startT = DateTime.Parse(start);
            DateTime endT = DateTime.Parse(end);

            var simulations = await _context.Simulation.Include(b => b.Methode)
                .Where(s => s.DateSimulation >= startT && s.DateSimulation <= endT)
                .Where(s => s.UserId == userIds)
                .Where(j => j.Methode.Nom == modelName)
                .ToListAsync();

            ViewBag.simulation = simulations;
            ViewBag.users = users;
            return View("Historique");
        }
        public List<dynamic> GetAllUsers()
        {
            // Define the SQL query to fetch only "Visiteur" Roles
            string sql = @"SELECT * FROM [dbo].[AspNetUsers] 
                            WHERE Id IN (SELECT DISTINCT UserId FROM [dbo].[AspNetUserRoles] WHERE RoleId = 1)";

            // Execute the SQL query using FromSql and map to dynamic objects
            dynamic users = _context.Users.FromSqlRaw(sql).ToList<dynamic>();

            return users; // Or return the data as needed
        }

        public async Task<ActionResult> IndexHistorique()
        {
            /* Liste des utilisteurs 
            var userList = GetAllUsers();

            ViewBag.users = userList;*/
            var modeles = await _context.Modele.Include(b => b.Historique)
                .ToListAsync();
            ViewBag.modeles = modeles;
            return View("NouvelHistorique");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
