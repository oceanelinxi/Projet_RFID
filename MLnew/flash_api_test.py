from flask import Flask, request, jsonify
from analytique import methode_analytique
from RandomForest import RFcross_validation, pretraitement_knn, dataset
from SVM import train_and_evaluate_svm
from knn import knnn
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
