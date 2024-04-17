from flask import Flask, request, jsonify
from analytique import methode_analytique
app = Flask(__name__)

@app.route('/analytique', methods=['POST'])
def analytique_route():
  prediction_ana =methode_analytique()
  return jsonify({'Analytique': prediction_ana})
app.run(host='0.0.0.0', port=5000)
