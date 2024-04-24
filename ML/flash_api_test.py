from flask import Flask, request, jsonify
from analytique import methode_analytique
from RandomForest import RFcross_validation, pretraitement_knn, dataset
from SVM import train_and_evaluate_svm
from knn import knnn
from analytique import methode_analytique
import zipfile
app = Flask(__name__)


def unzip_file(file_path, extract_to):
    with zipfile.ZipFile(file_path, 'r') as zip_ref:
        zip_ref.extractall(extract_to)



@app.route('/analytique', methods=['POST'])
def analytique():
    # Get the input parameters from the request
    input_params = request.get_json()
    
    

    # Call the predict() function to make a prediction
    accuracy = methode_analytique()
    #prediction = 0.75
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy})

pretrait = pretraitement_knn(r'Uploads/data_anonymous')
data = dataset(pretrait[0], pretrait[1], pretrait[2], pretrait[3])
@app.route('/RandomForest',methods= ['POST'])
def random_forest():
    # Get the input parameters from the request
    input_params = request.get_json()

    # Call the predict() function to make a prediction with SVM
    accuracy = RFcross_validation(input_params['n_estimators'], input_params['max_depth'], input_params['min_samples_leaf'],data)
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy})
 

@app.route('/SVM', methods=['POST'])
def svm():
    # Get the input parameters from the request
    input_params = request.get_json()
    print(input_params)
    print((input_params['Gamma'], input_params['C'], input_params['Kernel']))
    # Call the predict() function to make a prediction with SVM
    accuracy = train_and_evaluate_svm(input_params['Gamma'], input_params['C'], input_params['Kernel'])
    
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy})

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
   
  


@app.route('/knn', methods=['POST'])
def knn():
    # Get the input parameters from the request
    input_params = request.get_json()
    hyperparameter1_value = int(input_params['Hyperparameter1'])
    hyperparameter2_value = str(input_params['Hyperparameter2'])
    hyperparameter3_value = str(input_params['Hyperparameter3'])
 
 
    # Call the predict() function to make a prediction
    accuracy = knnn(hyperparameter1_value,hyperparameter2_value,hyperparameter3_value)
    #prediction = 0.75
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy})

app.run(host='0.0.0.0', port=5000)
