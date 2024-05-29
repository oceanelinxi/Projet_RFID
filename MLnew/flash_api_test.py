from flask import Flask, request, jsonify
from analytique import methode_analytique
from RandomForest import RFcross_validation, pretraitement_knn, dataset
from SVM import train_and_evaluate_xgboost
from SVM import train_and_evaluate_svm
from SVM import train_and_evaluate_dt_xgboost
from knn import knnn
from knn import evaluate_adaboost_rf
from knn import evaluate_adaboost_svm
from knn import evaluate_adaboost_knn
from analytique import methode_analytique
from datetime import datetime
import zipfile,pandas as pd
import os
app = Flask(__name__)

# Chemin du dossier
data = pd.DataFrame()
dossier = 'Uploads/data_anonymous'


def unzip_file(file_path, extract_to):
    with zipfile.ZipFile(file_path, 'r') as zip_ref:
        zip_ref.extractall(extract_to)



@app.route('/analytique', methods=['POST'])
def analytique():
    start = datetime.now()
    # Get the input parameters from the request
    input_params = request.get_json()       
    steps=int(input_params['steps'])
    t0=int(input_params['t0'])
    # Call the predict() function to make a prediction
    accuracy = methode_analytique(steps,t0,pathfile = dossier)
    duree_ana = (datetime.now() - start).seconds
    print ("duree methode analytique : {}".format(duree_ana))
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'duree':str(duree_ana)})



if os.path.exists(dossier):
    start = datetime.now()
    # Le dossier existe, donc tu peux executer la fonction de pretraitement
    pretrait = pretraitement_knn(dossier)
    data = dataset(pretrait[0], pretrait[1], pretrait[2], pretrait[3])

    end = datetime.now()
    print('Duree du pretraitement : {}'.format(end-start))

@app.route('/RandomForest',methods= ['POST'])
def random_forest():
    start = datetime.now()
    # Get the input parameters from the request
    input_params = request.get_json()

    if(input_params['class_weight_index']=='none'):
        input_params['class_weight_index']=None
    if(input_params['max_leaf_nodes_index']==0):
        input_params['max_leaf_nodes_index']=None
    if(input_params['n_jobs_index']==0):
        input_params['n_jobs_index']=None
    if(input_params['random_state_index']==0):
        input_params['random_state_index']=None
    if(input_params['max_samples_index']==0):
        input_params['max_samples_index']=None

    print(input_params)
    
    accuracy = RFcross_validation(data,input_params['n_estimators'], input_params['max_depth'], input_params['min_samples_leaf'],str(input_params['criterion_index']),
                    int(input_params['min_samples_split_index']),
                    float(input_params['min_weight_fraction_leaf_index']),
                    str(input_params['max_features_index']),
                    input_params['max_leaf_nodes_index'],
                    float(input_params['min_impurity_decrease_index']),
                    bool(input_params['bootstrap_index']),
                    bool(input_params['oob_score_index']),
                    input_params['n_jobs_index'],
                    input_params['random_state_index'],
                    int(input_params['verbose_index']),
                    bool(input_params['warm_start_index']),
                    input_params['class_weight_index'],
                    float(input_params['ccp_alpha_index']),
                    input_params['max_samples_index'])



    duree_rf = (datetime.now() - start).seconds
    print('Duree de rf : {}',str(duree_rf))
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'duree':str(duree_rf)})
 

@app.route('/SVM', methods=['POST'])
def svm():
    start = datetime.now()
    # Get the input parameters from the request
    input_params = request.get_json()
    print(input_params)
    print((input_params['Gamma'], float(input_params['C']), input_params['Kernel']))
    

    if(input_params['class_we']=="None"):
        input_params['class_we']=None
    if(input_params['random_s']==0):
        input_params['random_s']=None

    # Call the predict() function to make a prediction with SVM
    accuracy = train_and_evaluate_svm(data,input_params['Gamma'], float(input_params['C']), input_params['Kernel'],
    int(input_params['degre']), float(input_params['coef']), bool(input_params['shrinkings']), 
    bool(input_params['prob']), float(input_params['tols']),
    float(input_params['cach_size']),input_params['class_we'], 
    bool(input_params['verbos']), int(input_params['max_it']),
    str(input_params['decision_func']), bool(input_params['break_t']), input_params['random_s'])


    duree_svm = (datetime.now()-start).seconds
    print('Duree svm {}'.format(duree_svm))
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'duree':str(duree_svm)})

@app.route('/Chemin', methods=['POST'])
def chemin():
    global data, dossier
    # Get the input parameters from the request
    data_ = request.get_json()
    chemin = data_.get('chemin')
    fileName=data_['nomFichier']
    fileName = fileName.rstrip('.zip')
    upl="Uploads/"
    # Remplacer "\\" par "/"
    chemin = chemin.replace("\\", "/")
    print( "fileName")
    print( fileName)
    print( "fileName")
    dossier = upl+fileName
    print("dossier:"+dossier)
    unzip_file(chemin, dossier)
    start = datetime.now()
    # Le dossier existe, donc tu peux executer la fonction de pretraitement
    pretrait = pretraitement_knn(dossier)
    data = dataset(pretrait[0], pretrait[1], pretrait[2], pretrait[3])
    

    end = datetime.now()
    print('Duree du pretraitement : {}'.format(end-start))
    return dossier

@app.route('/CourbesPrecision', methods=['POST'])
def cheminCourbe():
    model_and_hp = request.get_json()
    model, hparam = model_and_hp['methode'], model_and_hp['hyperparametre']
    print(" model, hparam : ",model, hparam.split('-'))
    picpath = "~/images/precision/" + str(model) + "/" + str(hparam.split('-')[1]) + ".png"
    return jsonify({'path' : picpath})


@app.route('/AdaRF', methods=['POST'])
def ada_rf():
    input_params = request.get_json()
    n_estimators = int(input_params['n_estimators'])
    max_depth = int(input_params['max_depth'])
    min_samples_leaf = int(input_params['min_samples_leaf'])
    critere= str(input_params['critere'])
    min_s_s= int(input_params['min_s_s'])
    min_w_f= float(input_params['min_w_f'])
    max_feat= str(input_params['max_feat'])
    max_l_n= int(input_params['max_l_n'])
    min_impurity= float(input_params['min_impurity'])
    boot=bool(input_params['boot'])
    oob=bool(input_params['oob'])
    n_job= int(input_params['n_job'])
    random= int(input_params['random'])
    verbo= int(input_params['verbo'])
    warm= bool(input_params['warm'])
    class_w= str(input_params['class_w'])
    ccp= float(input_params['ccp'])
    max_sample= int(input_params['max_sample'])
    cv=int(input_params['cv'])

    if(class_w=='none'):
        class_w=None
    if(max_depth==0):
        max_depth=None
    if(max_l_n==0):
        max_l_n=None
    if(n_job==0):
        n_job=None
    if(random==0):
        random=None
    if(max_sample==0):
        max_sample=None
    print(n_estimators)
    print(max_depth)
    print(min_samples_leaf)
    print(critere)
    print(min_s_s)
    print(min_w_f)
    print(max_feat)
    print(max_l_n)
    print(min_impurity)
    print(boot)
    print(oob)
    print(n_job)
    print(random)
    print(verbo)
    print(warm)
    print(class_w)
    print(ccp)
    print(max_sample)
    print(cv)

    mean_accuracy=evaluate_adaboost_rf(data,cv,n_estimators,critere,max_depth,min_s_s,min_samples_leaf,min_w_f,max_feat,max_l_n,min_impurity,boot,oob,n_job,random,verbo,warm,class_w,ccp,max_sample)
    return jsonify({'mean_accuracy': mean_accuracy})

@app.route('/AdaSVM',methods=['POST'])
def ada_svm():
    input_params = request.get_json()
    C=float(input_params['C'])
    kernels=str(input_params['kernels'])
    gamma=str(input_params['gamma'])
    degre=int(input_params['degre'])
    coef=float(input_params['coef'])
    shrinkings=bool(input_params['shrinkings'])
    prob=bool(input_params['prob'])
    tols=float(input_params['tols'])
    cach_size=float(input_params['cach_size'])
    max_it=int(input_params['max_it'])
    decision_func=str(input_params['decision_func'])
    break_t=bool(input_params['break_t'])
    class_we=str(input_params['class_we'])
    cv_svm=int(input_params['cv_svm'])
    random_s=int(input_params['random_s'])
    verbos=bool(input_params['verbos'])
    if(class_we=="None"):
        class_we=None
    if(random_s==0):
        random_s=None
    print(C)
    print(kernels)
    print(gamma)
    print(degre)
    print(coef)
    print(shrinkings)
    print(prob)
    print(tols)
    print(cach_size)
    print(max_it)
    print(decision_func)
    print(break_t)
    print(class_we)
    print(cv_svm)
    print(random_s)
    print(verbos)
    mean_accuracy=evaluate_adaboost_svm(data,cv_svm,C,kernels,degre,gamma,coef,shrinkings,prob,tols,cach_size,class_we,verbos,max_it,decision_func,break_t,random_s)
    return jsonify({'mean_accuracy': mean_accuracy})


@app.route('/AdaKNN',methods=['POST'])
def ada_knn():
    input_params = request.get_json()
    n_neighbor=int(input_params['n_neighbor'])
    weight=str(input_params['weight'])
    metrics=str(input_params['metrics'])
    algo=str(input_params['algo'])
    leaf_sizes=int(input_params['leaf_sizes'])
    n_job_knn=int(input_params['n_job_knn'])
    p_knn=float(input_params['p_knn'])
    cv_fold_knn=int(input_params['cv_fold_knn'])
   
    if(n_job_knn==0):
        n_job_knn=None
   
    print(n_neighbor)
    print(weight)
    print(metrics)
    print(algo)
    print(leaf_sizes)
    print(n_job_knn)
    print(p_knn)
    print(cv_fold_knn)
    
    mean_accuracy=evaluate_adaboost_knn(data,cv_fold_knn,n_neighbor, weight, algo, leaf_sizes, p_knn, metrics, n_job_knn)
    return jsonify({'mean_accuracy': mean_accuracy})

@app.route('/XGBoost', methods=['POST'])
def xgboost():
     input_params = request.get_json()
     nestimators=int(input_params['nestimators'])
     mx_depth=int(input_params['mx_depth'])
     lrn_rate=float(input_params['lrn_rate'])
     subsample=float(input_params['subsample'])
     colsample_bynode=float(input_params['colsample_bynode'])
     rd_state=int(input_params['rd_state'])
    
     
     mean_accuracy=train_and_evaluate_xgboost(nestimators,mx_depth, lrn_rate,subsample,colsample_bynode, rd_state)
     return jsonify({'mean_accuracy': mean_accuracy})
    
@app.route('/XGBoostKNN', methods=['POST'])
def xgboostknn():
    input_params = request.get_json()
    knn_neighbors=int(input_params['Knn_neighbors'])
    booster=str(input_params['Booster'])
    n_estimators=int(input_params['N_estimators'])
    verbosity=int(input_params['Verbosity'])
    objective=str(input_params['Objective'])
    eval_metric=str(input_params['EvalMetric'])
    
    early_stopping_rounds=int(input_params['EarlyStopping'])
    seed=int(input_params['Seed'])
    nthread=int(input_params['Nthread'])
    
    mean_accuracy=train_and_evaluate_knn_xgboost(knn_neighbors,booster,n_estimators,verbosity,objective,eval_metric,early_stopping_rounds,seed,nthread)
    return jsonify({'mean_accuracy': mean_accuracy})
    
@app.route('/XGBoostDT', methods=['POST'])
def xgboostsvm():
    input_params = request.get_json()
    learning_rate=float(input_params['learning_rate'])
    booster=str(input_params['booster'])
    n_estimator3=int(input_params['n_estimator3'])
    objective=str(input_params['objective'])
    sample_type=str(input_params['sample_type'])
    early_stopping_rounds=int(input_params['early_stopping_rounds'])
    gamma3=float(input_params['gamma3'])
    colsample_bylevel=float(input_params['colsample_bylevel'])
    
    mean_accuracy=train_and_evaluate_dt_xgboost(learning_rate,booster,n_estimator3,objective)
    print(mean_accuracy)
    return jsonify({'mean_accuracy': mean_accuracy})
 


@app.route('/knn', methods=['POST'])
def knn():
    # Get the input parameters from the request
    start_knn = datetime.now()
    input_params = request.get_json()
    hyperparameter1_value = int(input_params['Hyperparameter1'])
    hyperparameter2_value = str(input_params['Hyperparameter2'])
    hyperparameter3_value = str(input_params['Hyperparameter3'])
    hyperparameter4_value = str(input_params['Hyperparameter4'])
    hyperparameter5_value = int(input_params['Hyperparameter5'])
    hyperparameter6_value = int(input_params['Hyperparameter6'])
    hyperparameter7_value = float(input_params['Hyperparameter7'])

    
    print(hyperparameter7_value)

    if(hyperparameter6_value==0):
        hyperparameter6_value=None
 

    
    # Call the predict() function to make a prediction
    accuracy = knnn(data,hyperparameter1_value,hyperparameter2_value,hyperparameter3_value,hyperparameter4_value,hyperparameter5_value,hyperparameter7_value,hyperparameter6_value)
    
    duree = (datetime.now()- start_knn).seconds
    print('Duree de knn : {}'.format(duree))
    print('accuracy', accuracy)
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'duree':str(duree)})

app.run(host='0.0.0.0', port=5000)
