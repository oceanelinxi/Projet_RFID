Projet de détection de tag avec Machine Learning. 
La solution fonctionnelle est MLnew.
Les données sont prises en entrée dans un fichier Zip.

L'analyse des tags se fait par quatres méthodes :
 * Méthode analytique ;
 * Trois algorithmes de Machine Learning : Random Forest, SVM et KNN.

Nous avons utilisés deux API, une en python pour interragir avec le code python et une en C#.
Avant de lancer la solution dans VS, et de lancer les simulations, il faut ouvrir python et lancer l'exécution du fichier flash_api_test.py

pour la partie de chatbot
1. télécharger MLnew/Chat sur github - https://github.com/oceanelinxi/Projet_RFID.git

2) Vous pouvez utiliser cmd ou terminal dans vs code pour télécharger l'environnement du chatbot.
pip install -r requirements.txt

3) Editer chat/config.py.
Ajouter mon api personnelle entre les guillemets de openai_api= :
sk-proj-INypDEca2pXr7Vci1xwsT3BlbkFJOCyjC81Srhre2zdhhLK
Sauvegarder

4. enfin, vous pouvez lancer ChatBot depuis le terminal de vs code ou cmd (si vous êtes déjà dans le dossier chat) en tapant
Streamlit run ChatBot.py

5) Exécutez le code et la page du chatbot s'affichera automatiquement.
Vous pouvez entrer votre question en anglais/français/chinois mais seulement des questions liées à notre projet.
Posez des questions sur les mots-clés, svm, knn, random forest, xgboost, adaboost et leurs paramètres, hyperparamètres, et RFID.
