from flask import Flask, request, jsonify
from prediction_test import predict
app = Flask(__name__)

@app.route('/RFClassifier', methods=['POST'])
def analyticalWithParams_route():
    # Charger les DataFrames a partir des fichiers CSV
    params1 = request.get_json()
    # Call the predict() function to make a prediction
    MLRF = RandomForestML(int(params1['Hyperparameter1']), int(params1['Hyperparameter2']), int(params1['Hyperparameter3']))
    # Return the prediction as JSON
    return jsonify({'MLRF': MLRF})
app.run(host='0.0.0.0', port=5000)
