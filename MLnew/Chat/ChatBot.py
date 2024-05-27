import os
from config import OPENAI_API_KEY, PINECONE_API_KEY, PINECONE_API_ENV
os.environ["OPENAI_API_KEY"]=OPENAI_API_KEY # Remplissez votre clé API OpenAI, ou entrez "export OPENAI_API_KEY='sk-...'" dans le terminal pour la définir comme variable d'environnement
os.environ["PINECONE_API_KEY"] = PINECONE_API_KEY
os.environ["PINECONE_API_ENV"] = PINECONE_API_ENV
from langchain_community.document_loaders import PyPDFLoader
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain.embeddings import OpenAIEmbeddings
from langchain_community.vectorstores import Pinecone
from langchain_openai import OpenAI
from langchain.chains.question_answering import load_qa_chain
from pinecone import Pinecone as PineconeClient,ServerlessSpec
import streamlit as st

# Chargement et découpe du document en petits morceaux de texte (chunks)
def load_and_split(path: str):
    # Chargement du document PDF
    loader = PyPDFLoader(path)
    docs = loader.load() # Lors du chargement du document avec pypdf, le document est divisé par défaut par page
    print(f"Votre document a été divisé en {len(docs)} parties, la première partie contient {len(docs[0].page_content)} caractères.\n")


    # Découpe du document en petits morceaux de texte
    text_splitter = RecursiveCharacterTextSplitter(chunk_size=200, chunk_overlap=100)
    texts = text_splitter.split_documents(docs)
    print(f"Votre document a été (davantage) divisé en {len(texts)} parties, la première partie contient {len(texts[0].page_content)} caractères.\n")

   
    return texts




# Utilisation de la base de données vectorielle Pinecone
index_name = "finalbot"
embeddings = OpenAIEmbeddings()

# # Création d'une nouvelle base de données vectorielle (à exécuter une seule fois)
# texts = load_and_split("data/data.pdf") # Remplacez par votre fichier

# pc = PineconeClient( api_key=os.environ.get("PINECONE_API_KEY") ) 

# if index_name not in pc.list_indexes().names(): 
#     pc.create_index( 
#         name=index_name, 
#         dimension=1536, 
#         metric='euclidean', 
#         spec=ServerlessSpec( 
#             cloud='aws', 
#             region='us-east-1' 
#             ) 
#         )


# # Initialisation du stockage vectoriel Pinecone dans Langchain
# vectordb = Pinecone.from_texts([t.page_content for t in texts], embeddings, index_name=index_name) # Remplacez par le nom de votre index Pinecone
# print("***Mise à jour des vecteurs complétée (upsert)***")



# # # Cas 2: Chargement direct d'une base de données vectorielle existante
pc = PineconeClient( api_key=os.environ.get("PINECONE_API_KEY") ) 
vectordb = Pinecone.from_existing_index(index_name, OpenAIEmbeddings())


# # # Cas 3: Ajout de nouvelles données à la base de données vectorielle
# new_texts = ""
# index = pinecone.Index(index_name)
# # Initialisation de Pinecone
# pinecone.init(
#     api_key=PINECONE_API_KEY, # Trouvable dans la page "API Keys" de app.pinecone.io
#     environment=PINECONE_API_ENV # À côté de la clé API
# )
# vectordb = Pinecone(index, OpenAIEmbeddings().embed_query, "text")
# vectordb.add_texts(new_texts)



#  Questions-réponses utilisateur

# Utilisation de Streamlit pour générer l'interface web
st.title('ChatBot') # Définit le titre
user_input = st.text_input('Entrez votre question') # Définit la question par défaut dans la zone de saisie

# Génération de la réponse en fonction de l'entrée de l'utilisateur
if user_input:
    print(f"user input：{user_input}")
    # Recherche des textes les plus similaires dans la base de données vectorielle en fonction de l'entrée de l'utilisateur
   # most_relevant_texts = vectordb.similarity_search(user_input, k=5) # k est le nombre de textes à retourner, par défaut 4


    most_relevant_texts = vectordb.max_marginal_relevance_search(user_input, k=8, fetch_k=20, lambda_mult=1)
    print("plus près：")
    # print(most_relevant_texts[0].page_content[:200])
    # print(most_relevant_texts[1].page_content[:200])
    for i, text in enumerate(most_relevant_texts):
        print(f"{i+1}. {text.page_content}")


    # Initialisation de l'IA GPT-3 dans Langchain
    llm = OpenAI(temperature=0.5)
    chain = load_qa_chain(llm, chain_type="stuff")
    answer = chain.run(input_documents=most_relevant_texts, question=user_input)

    st.write(answer)
    
