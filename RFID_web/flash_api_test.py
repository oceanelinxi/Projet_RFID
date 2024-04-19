<<<<<<< HEAD
from flask import Flask, request, jsonify
from analytique import methode_analytique
app = Flask(__name__)

@app.route('/analytique', methods=['POST'])
def analytique():
    # Get the input parameters from the request
    input_params = request.get_json()
    
    # Call the predict() function to make a prediction
    accuracy = methode_analytique()
    #prediction = 0.75
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy})

app.run(host='0.0.0.0', port=5000)

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

app.run(host='0.0.0.0', port=5000)
=======
from flask import Flask, request, jsonify
from analytique import methode_analytique
app = Flask(__name__)

@app.route('/analytique', methods=['POST'])
def analytique():
    # Get the input parameters from the request
    input_params = request.get_json()
    
    # Call the predict() function to make a prediction
    accuracy = methode_analytique()
    #prediction = 0.75
    # Return the prediction as JSON
    return jsonify({'accuracy': accuracy})

app.run(host='0.0.0.0', port=5000)
>>>>>>> a815e79ab5a8ed6693f0a120594243618750bf01
