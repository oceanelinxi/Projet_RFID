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
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            ViewBag.Users = users;
            ViewBag.UserRoles = userRoles;
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

            ViewBag.UserId = user.Id;
            ViewBag.Email = user.Email;
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");


            return View("AssignRoles");
        }

        [HttpPost]
        [Authorize(Roles = "Administrateur")]
        [HttpPost]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> AssignRoles(string userId, string selectedRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(selectedRole))
            {
                ModelState.AddModelError(string.Empty, "Invalid user or role.");
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                return View(new { userId = userId });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(selectedRole))
            {
                ModelState.AddModelError(string.Empty, "User already has this role.");
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                return View(new { userId = userId });
            }

            // Remove all existing roles
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!removeRolesResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to remove user roles.");
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                return View(new { userId = userId });
            }

            // Add the new selected role
            var addRoleResult = await _userManager.AddToRoleAsync(user, selectedRole);
            if (!addRoleResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to add user role.");
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                return View(new { userId = userId });
            }

            return RedirectToAction(nameof(ManageRole));
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Upload(string btnradio)
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
                        nomFichier= fileName,
                        btnradio= btnradio
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
        public async Task<string> Methode_analytique(int step,int t0_run)
        {
            using (var client = new HttpClient())
            {
                var requestData = new { 
                steps=step,
                t0=t0_run
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/analytique", content);
                var result = await response.Content.ReadAsStringAsync();



                return result;
            }

        }

        //Random_Forest
        public async Task<string> RandomForest(int n_est, int max_d, int min_samples,
            
                     
                     string criterion_index,
                     int min_samples_split_index,
                     float min_weight_fraction_leaf_index,
                     string max_features_index,
                     int max_leaf_nodes_index,
                     float min_impurity_decrease_index,
                     bool bootstrap_index,
                     bool oob_score_index,
                     int n_jobs_index,
                     int random_state_index,
                     int verbose_index,
                     bool warm_start_index,
                     string class_weight_index,
                     float ccp_alpha_index,
                     int max_samples_index)
        {
            using (var client = new HttpClient())
            {
                var requestData = new
                {
                    n_estimators = n_est,
                    max_depth = max_d,
                    min_samples_leaf = min_samples,
                    criterion_index= criterion_index,
                    min_samples_split_index= min_samples_split_index,
                    min_weight_fraction_leaf_index = min_weight_fraction_leaf_index,
                    max_features_index= max_features_index,
                    max_leaf_nodes_index= max_leaf_nodes_index,
                    min_impurity_decrease_index= min_impurity_decrease_index,
                    bootstrap_index= bootstrap_index,
                    oob_score_index= oob_score_index,
                    n_jobs_index= n_jobs_index,
                    random_state_index= random_state_index,
                    verbose_index= verbose_index,
                    warm_start_index= warm_start_index,
                    class_weight_index= class_weight_index,
                    ccp_alpha_index= ccp_alpha_index,
                    max_samples_index= max_samples_index








                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/RandomForest", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }

        //SVM
        public async Task<string> Methode_SVM(string gamma_select, float C_input, string kernel_select, int degree, float coef0, bool shrinking, bool probability, string tol, float cache_size, string class_weight_svm, bool verbose_svm, int max_iter, string decision_function_shape, bool break_ties, int random_state_svm)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    Gamma = gamma_select,
                    C = C_input,
                    Kernel = kernel_select,
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
                    class_we = class_weight_svm
                    
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/SVM", content);//modifier selon python
                var resultSVM = await response.Content.ReadAsStringAsync();

                return resultSVM;
            }

        }

        //KNN
        public async Task<string> KNN(string n_neighbors, string weights, string metric,string algorithm_index,
                     int leaf_size_index,
                     int n_jobs_knn_index,
                     string p_index)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    Hyperparameter1 = n_neighbors,

                    Hyperparameter2 = weights,

                    Hyperparameter3 = metric,
                    Hyperparameter4= algorithm_index,
                    Hyperparameter5= leaf_size_index,
                    Hyperparameter6= n_jobs_knn_index,
                    Hyperparameter7 = p_index
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/knn", content);
                var resultKNN = await response.Content.ReadAsStringAsync();

                ViewBag.Hyperparameter1 = n_neighbors;
                ViewBag.Hyperparameter2 = weights;
                ViewBag.Hyperparameter3 = metric;
                ViewBag.Hyperparameter4 = algorithm_index;
                ViewBag.Hyperparameter5 = leaf_size_index;
                ViewBag.Hyperparameter6 = n_jobs_knn_index;
                ViewBag.Hyperparameter7 = p_index;
                


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
        public async Task<string> XGBoost(int nestimators, int mx_depth, string lrn_rate, string subsample, string colsample_bynode, int rd_state)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    nestimators = nestimators,
                    mx_depth = mx_depth,
                    lrn_rate = lrn_rate,
                    subsample = subsample,
                    colsample_bynode = colsample_bynode,

                    rd_state = rd_state
                   
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/XGBoost", content);
                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
        public async Task<string> XGBoostKNN(int knn_neighbors, string booster, int n_estimators, int verbosity, string objective, string eval_metric, int early_stopping_rounds, int seed, int nthread)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    Knn_neighbors = knn_neighbors,
                    Booster = booster,
                    N_estimators = n_estimators,
                    Verbosity = verbosity,
                    Objective = objective,
                    EvalMetric = eval_metric,
                 
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
        public async Task<string> XGBoostDT(string learning_rate, string booster, int n_estimator3, string objective, string sample_type, int early_stopping_rounds, string gamma3, string colsample_bylevel)
        {
            using (var client = new HttpClient())
            {
                // Définir le timeout du client HTTP à 200 secondes
                client.Timeout = TimeSpan.FromSeconds(200);
                var requestData = new
                {
                    learning_rate = learning_rate,
                    booster = booster,
                    n_estimator3 = n_estimator3,
                    objective = objective,
                    sample_type = sample_type,
                    early_stopping_rounds = early_stopping_rounds,

                    gamma3 = gamma3,
                    colsample_bylevel = colsample_bylevel
                 
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:5000/XGBoostDT", content);
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
                     string n_neighbors, string weights, string metric,
                     int step,int t0_run,
                     string criterion_index,
                     int min_samples_split_index,
                     float min_weight_fraction_leaf_index,
                     string max_features_index,
                     int max_leaf_nodes_index,
                     float min_impurity_decrease_index, 
                     bool bootstrap_index,
                     bool oob_score_index,
                     int n_jobs_index,
                     int random_state_index,
                     int verbose_index,
                     bool warm_start_index,
                     string class_weight_index,
                     float ccp_alpha_index,
                     int max_samples_index,
                     int degree_index,
                     float coef0_index,
                     bool shrinking_index,
                     bool probability_index,
                     string tol_index,
                     float cache_size_index,
                     string class_weight_svm_index,
                     bool verbose_svm_index,
                     int max_iter_index,
                     string decision_function_shape_index,
                     bool break_ties_index,
                     int random_state_index_2,
                     string algorithm_index,
                     int leaf_size_index,
                     int n_jobs_knn_index,
                     string p_index)
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
                var resultAnalytique = await Methode_analytique(step, t0_run);
                ViewBag.AnalytiqueResult = resultAnalytique;

                dynamic jsonResult = JsonConvert.DeserializeObject(resultAnalytique);
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
                    Nom = "Analytique",
                    Accuracy=(float)acc
                };
                _context.Modele.Add(model);
                await _context.SaveChangesAsync();
                var all_models = await _context.Modele.ToListAsync();
                int n_model = all_models.Count;

                // step
                Parametre param = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "step",
                    Valeur = Convert.ToString(step)
                };
                _context.Parametre.Add(param);
                await _context.SaveChangesAsync();

                // t0_run
                param = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "t0_run",
                    Valeur = Convert.ToString(t0_run)
                };
                _context.Parametre.Add(param);
                await _context.SaveChangesAsync();

                


            }

            if (mainOption2 == true)
            {
                // Obtention des resultats python
                var resultRandomForest = await RandomForest(n_est, max_d, min_samples,  criterion_index,
                     min_samples_split_index,
                      min_weight_fraction_leaf_index,
                      max_features_index,
                      max_leaf_nodes_index,
                      min_impurity_decrease_index,
                      bootstrap_index,
                      oob_score_index,
                     n_jobs_index,
                      random_state_index,
                      verbose_index,
                      warm_start_index,
                      class_weight_index,
                      ccp_alpha_index,
                      max_samples_index);
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

                // criterion
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "criterion",
                    Valeur = Convert.ToString(criterion_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // min_samples_split
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "min_samples_split",
                    Valeur = Convert.ToString(min_samples_split_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // min_weight_fraction_leaf
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "min_weight_fraction_leaf",
                    Valeur = Convert.ToString(min_weight_fraction_leaf_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();


                // max_features
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "max_features",
                    Valeur = Convert.ToString(max_features_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // max_leaf_nodes
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "max_leaf_nodes",
                    Valeur = Convert.ToString(max_leaf_nodes_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // min_impurity_decrease
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "min_impurity_decrease",
                    Valeur = Convert.ToString(min_impurity_decrease_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                
                // bootstrap
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "bootstrap",
                    Valeur = Convert.ToString(bootstrap_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // oob_score
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "oob_score",
                    Valeur = Convert.ToString(oob_score_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

              
                // n_jobs
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "n_jobs",
                    Valeur = Convert.ToString(n_jobs_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // random_state
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "random_state_index",
                    Valeur = Convert.ToString(random_state_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // verbose
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "verbose",
                    Valeur = Convert.ToString(verbose_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // warm_start
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "warm_start",
                    Valeur = Convert.ToString(warm_start_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // class_weight
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "class_weight",
                    Valeur = Convert.ToString(class_weight_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // ccp_alpha
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "ccp_alpha",
                    Valeur = Convert.ToString(ccp_alpha_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

                // max_samples
                param2 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "max_samples",
                    Valeur = Convert.ToString(max_samples_index)
                };
                _context.Parametre.Add(param2);
                await _context.SaveChangesAsync();

            }

            if (mainOption3 == true)
            {
                var resultSVM = await Methode_SVM(gamma_select, C_input, kernel_select, degree_index,
                     coef0_index,
                      shrinking_index,
                      probability_index,
                      tol_index,
                     cache_size_index,
                     class_weight_svm_index,
                     verbose_svm_index,
                      max_iter_index,
                      decision_function_shape_index,
                     break_ties_index,
                      random_state_index_2
                      );
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
                
                   
                // degree
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "degree",
                    Valeur = Convert.ToString(degree_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // coef0
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "coef0",
                    Valeur = Convert.ToString(coef0_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // shrinking
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "shrinking",
                    Valeur = Convert.ToString(shrinking_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // probability
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "probability",
                    Valeur = Convert.ToString(probability_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // tol
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "tol",
                    Valeur = Convert.ToString(tol_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // cache_size
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "cache_size",
                    Valeur = Convert.ToString(cache_size_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // class_weight
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "class_weight",
                    Valeur = Convert.ToString(class_weight_svm_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // verbose
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "verbose",
                    Valeur = Convert.ToString(verbose_svm_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // max_iter
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "max_iter",
                    Valeur = Convert.ToString(max_iter_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // decision_function_shape
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "decision_function_shape",
                    Valeur = Convert.ToString(decision_function_shape_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // break_ties
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "break_ties",
                    Valeur = Convert.ToString(break_ties_index)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();

                // random_state
                param3 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "random_state",
                    Valeur = Convert.ToString(random_state_index_2)
                };
                _context.Parametre.Add(param3);
                await _context.SaveChangesAsync();


            }

            if (mainOption4 == true)
            {
                var resultKNN = await KNN(n_neighbors, weights, metric, algorithm_index,
                    leaf_size_index,
                     n_jobs_knn_index, p_index);
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

                // algorithm
                param4 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "algorithm",
                    Valeur = Convert.ToString(algorithm_index)
                };
                _context.Parametre.Add(param4);
                await _context.SaveChangesAsync();

                // leaf_size
                param4 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "leaf_size",
                    Valeur = Convert.ToString(leaf_size_index)
                };
                _context.Parametre.Add(param4);
                await _context.SaveChangesAsync();

                // n_jobs
                param4 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "n_jobs",
                    Valeur = Convert.ToString(n_jobs_knn_index)
                };
                _context.Parametre.Add(param4);
                await _context.SaveChangesAsync();

                // p
                param4 = new Parametre
                {
                    ModeleID = all_models[n_model - 1].ModeleID,
                    Nom = "p",
                    Valeur = Convert.ToString(p_index)
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

        public async Task<ActionResult> MethodesEnsemblistes(bool? mainOption1, bool? mainOption2, int n_est, int max_d, int min_samples, string criterion, int min_samples_split, float min_weight_fraction_leaf, string max_features, int max_leaf_nodes, float min_impurity_decrease, bool bootstrap, bool oob_score, int n_jobs, int random_state, int verbose, bool warm_start, string class_weight, float ccp_alpha, int max_samples, int cv_folds, string algorithmSelect, string algorithmSelect1,
            string C_input_svm, string kernel_select, string gamma_select, int degree, float coef0, bool shrinking, bool probability, string tol, float cache_size, string class_weight_svm, bool verbose_svm, int max_iter, string decision_function_shape, bool break_ties, int random_state_svm, int cv_folds_svm,
            int n_neighbors,string weights,string metric,string algorithm,int leaf_size,int n_jobs_knn,string p, int cv_folds_knn, int nestimators, int mx_depth, string lrn_rate, string subsample, string colsample_bynode, int rd_state, string learning_rate, string booster, int n_estimator3, string objective,string sample_type,int early_stopping_rounds,string gamma3,string colsample_bylevel)
        {
            if (mainOption1 == true)
            {
                if (algorithmSelect1 == "Random Forest")
                {

                    var resultXGBoost = await  XGBoost(nestimators, mx_depth, lrn_rate, subsample, colsample_bynode, rd_state);
                    ViewBag.XGBoost = resultXGBoost;
                    
                    dynamic jsonResult = JsonConvert.DeserializeObject(resultXGBoost);
                    var acc = 98.2;
                    if (jsonResult != null && jsonResult.mean_accuracy != null)
                    {
                        acc = (float)jsonResult.mean_accuracy;
                    }
                }
                if (algorithmSelect1 == "KNN")
                {
                    //var resultXGBoostKNN = await XGBoostKNN(knn_neighbors,booster, n_estimators, verbosity,objective,eval_metric,early_stopping_rounds,seed,nthread);
                   // ViewBag.XGBoostKNN = resultXGBoostKNN;

                    //dynamic jsonResult = JsonConvert.DeserializeObject(resultXGBoostKNN);
                    var acc = 98.2;
                    //if (jsonResult != null && jsonResult.mean_accuracy != null)
                    {
                      //  acc = (float)jsonResult.mean_accuracy;
                    }
                }
                if (algorithmSelect1 == "DT")
                {
                    var resultXGBoostDT = await XGBoostDT(learning_rate, booster, n_estimator3, objective,  sample_type, early_stopping_rounds, gamma3, colsample_bylevel);
                   ViewBag.XGBoostDT = resultXGBoostDT;

                    dynamic jsonResult = JsonConvert.DeserializeObject(resultXGBoostDT);
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
            // Liste des utilisteurs 
            var userList = GetAllUsers();
            ViewBag.users = userList;
            
            List<Modele> modeles = await _context.Modele.Include(b => b.Historique).Include(h => h.Historique.User)
                .ToListAsync();
            ViewBag.modeles = modeles;
                        
            return View("NouvelHIstorique");
        }
       
        public async Task<ActionResult> FilterbyModeleName(bool user, bool modele, bool datesim, DateTime start, DateTime end, String userSelect, String modeleSelect)
        {
            var userList = GetAllUsers();
            ViewBag.users = userList;

            List<Modele> modeles = await _context.Modele.Include(b => b.Historique)
                    .Where(b => b.Nom == modeleSelect)
                    .Where(b => b.Historique.UserId == userSelect)
                    .Where(b => b.Historique.DateSimulation> start && b.Historique.DateSimulation < end)
                     .ToListAsync();
            if (datesim)
            {
                if (!user && modele)
                {
                    modeles = await _context.Modele.Include(b => b.Historique)
                        .Where(b => b.Nom == modeleSelect)
                        .Where(b => b.Historique.DateSimulation > start && b.Historique.DateSimulation < end)
                         .ToListAsync();
                }
                else if (user && !modele)
                {
                    modeles = await _context.Modele.Include(b => b.Historique)
                        .Where(b => b.Historique.UserId == userSelect)
                        .Where(b => b.Historique.DateSimulation > start && b.Historique.DateSimulation < end)
                         .ToListAsync();
                }
                else if (!user && !modele)
                {
                    modeles = await _context.Modele.Include(b => b.Historique)
                        .Where(b => b.Historique.DateSimulation > start && b.Historique.DateSimulation < end)
                         .ToListAsync();
                }
            }
            else
            {
                modeles = await _context.Modele.Include(b => b.Historique)
                    .Where(b => b.Nom == modeleSelect).Where(b => b.Historique.UserId == userSelect)
                     .ToListAsync();

                if (!user && modele)
                {
                    modeles = await _context.Modele.Include(b => b.Historique)
                        .Where(b => b.Nom == modeleSelect)
                         .ToListAsync();
                }
                else if (user && !modele)
                {
                    modeles = await _context.Modele.Include(b => b.Historique)
                        .Where(b => b.Historique.UserId == userSelect)
                         .ToListAsync();
                }
                else if (!user && !modele)
                {
                    modeles = await _context.Modele.Include(b => b.Historique)
                         .ToListAsync();
                }
            }
            
            ViewBag.modeles = modeles;

            return View("NouvelHistorique");
        }

        public  async Task<ActionResult> Details(int Id)
        {
            List<Modele> m = await _context.Modele
                .Include(b => b.Parametres)
                .Where(b => b.ModeleID == Id)
                 .ToListAsync();
            ViewBag.OneModele = m[0];
            if(ViewBag.OneModele == null)
            {
                return NotFound();
            }
            return View("DetailsModele");
        }
        
        [Authorize(Roles = "Administrateur")]
        public ActionResult ClearHistory() {
            return View("ClearHistory");
        }
        [Authorize(Roles="Adminiatrateur")]
        public ActionResult DeleteHistory() {
            string sql = @"Drop * FROM Historique";
            _context.Historique.FromSqlRaw(sql);
            return View("IndexHIstorique");
        }

        public async Task<ActionResult> RunAgain(int Id)
        {
            List<Modele> m = await _context.Modele
                .Include(b => b.Parametres)
                .Where(b => b.ModeleID == Id)
                 .ToListAsync();
            ViewBag.OneModele = m[0];
            if (ViewBag.OneModele == null)
            {
                return NotFound();
            }
            return View("RunAgain");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

         public ActionResult ChatBot()
         {
             return View();
         }
    }
}
