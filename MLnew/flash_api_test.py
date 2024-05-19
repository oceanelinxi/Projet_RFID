from flask import Flask, request, jsonify
from analytique import methode_analytique
from RandomForest import RFcross_validation, pretraitement_knn, dataset
from SVM import train_and_evaluate_svm
from knn import knnn
from knn import evaluate_adaboost_rf
from analytique import methode_analytique
from datetime import datetime
import zipfile
import os
app = Flask(__name__)


def unzip_file(file_path, extract_to):
    with zipfile.ZipFile(file_path, 'r') as zip_ref:
        zip_ref.extractall(extract_to)



@app.route('/analytique', methods=['POST'])
def analytique():
    start = datetime.now()
    # Get the input parameters from the request
    input_params = request.get_json()       

    # Call the predict() function to make a prediction
    accuracy = methode_analytique()
    duree_ana = (datetime.now() - start)
    print ("duree methode analytique : {}".format(duree_ana.seconds))
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'duree':str(duree_ana)})

# Chemin du dossier
dossier = 'Uploads/data_anonymous'
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
    
    accuracy = RFcross_validation(data,input_params['n_estimators'], input_params['max_depth'], input_params['min_samples_leaf'])
    duree_rf = datetime.now() - start 
    print('Duree de rf : {}'.format(duree_rf.seconds))
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'duree':str(duree_rf)})
 

@app.route('/SVM', methods=['POST'])
def svm():
    start = datetime.now()
    # Get the input parameters from the request
    input_params = request.get_json()
    print(input_params)
    print((input_params['Gamma'], input_params['C'], input_params['Kernel']))
    # Call the predict() function to make a prediction with SVM
    accuracy = train_and_evaluate_svm(input_params['Gamma'], input_params['C'], input_params['Kernel'])
    duree_svm = (datetime.now()-start)
    print('Duree svm {}'.format(duree_svm.seconds))
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'duree':str(duree_svm)})

@app.route('/Chemin', methods=['POST'])
def chemin():
    # Get the input parameters from the request
    data = request.get_json()
    chemin=data.get('chemin')
    # Remplacer "\\" par "/"
    chemin = chemin.replace("\\", "/")
    print( chemin)
    unzip_file(chemin,"Uploads/data_anonymous")
    return chemin

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


@app.route('/knn', methods=['POST'])
def knn():
    # Get the input parameters from the request
    start_knn = datetime.now()
    input_params = request.get_json()
    hyperparameter1_value = int(input_params['Hyperparameter1'])
    hyperparameter2_value = str(input_params['Hyperparameter2'])
    hyperparameter3_value = str(input_params['Hyperparameter3'])
 
    # Call the predict() function to make a prediction
    accuracy = knnn(data,hyperparameter1_value,hyperparameter2_value,hyperparameter3_value)
    
    duree = (datetime.now()- start_knn)
    print('Duree de knn : {}'.format(duree, duree.seconds))
    print('accuracy', accuracy)
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy, 'duree':str(duree)})

app.run(host='0.0.0.0', port=5000)
