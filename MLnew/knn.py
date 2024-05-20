import os
import numpy as np
import pandas as pd
import datetime
from sklearn.ensemble import AdaBoostClassifier
from sklearn.ensemble import RandomForestClassifier
from sklearn.datasets import make_classification
from sklearn.model_selection import cross_val_score
from sklearn.svm import SVC
from sklearn.preprocessing import LabelEncoder
from sklearn.neighbors import KNeighborsClassifier

BEST_PARAMETERS = {'metric': 'manhattan', 'n_neighbors': 8, 'weights': 'distance'}
def pretraitement_knn():
   
   pathfile = r'/home/ken/Téléchargements/data_anonymous'

   # reflist: list of epc in each box
   reflist = pd.DataFrame()
   # 
   files=os.listdir(pathfile)
   for file in files:
       if file.startswith('reflist_'):
           temp=pd.read_csv(os.path.join(pathfile,file),sep=',').reset_index(drop=True)[['Epc']]
           temp['refListId']=file.split('.')[0]
           reflist = pd.concat([reflist, temp], axis = 0)
           #reflist=reflist.append(temp)
   reflist=reflist.rename(columns = {'refListId':'refListId_actual'})
   reflist['refListId_actual'] = reflist['refListId_actual'].apply(lambda x:int(x[8:]))

   Q_refListId_actual=reflist.groupby('refListId_actual')['Epc'].nunique().rename('Q refListId_actual').reset_index(drop=False)
   reflist=pd.merge(reflist,Q_refListId_actual,on='refListId_actual',how='left')
   
   df=pd.DataFrame()
   # 
   #files=os.listdir(pathfile)
   for file in files:
       if file.startswith('ano_APTags'):
      
           temp=pd.read_csv(os.path.join(pathfile,file),sep=',')
           df= pd.concat([df, temp], axis = 0)
   df['LogTime'] = pd.to_datetime (df['LogTime'] ,format='%Y-%m-%d-%H:%M:%S') 
   df['TimeStamp'] = df['TimeStamp'].astype(float)
   df['Rssi'] = df['Rssi'].astype(float)
   df=df.drop(['Reader','EmitPower','Frequency'],axis=1).reset_index(drop=True)
   df=df[['LogTime', 'Epc', 'Rssi', 'Ant']]
   # antennas 1 and 2 are facing the box when photocell in/out 
   Ant_loc=pd.DataFrame({'Ant':[1,2,3,4],'loc':['in','in','out','out']})
   df=pd.merge(df,Ant_loc,on=['Ant'])
   df=df.sort_values('LogTime').reset_index(drop=True)

   tags = df

       # timing: photocells a time window for each box: start/stop (ciuchStart, ciuchStop)
   file=r'ano_supply-process.2019-11-07-CUT.csv'
   timing=pd.read_csv(os.path.join(pathfile,file),sep=',')
   timing['file']=file
   timing['date']=pd.to_datetime(timing['date'],format='%d/%m/%Y %H:%M:%S,%f')
   timing['ciuchStart']=pd.to_datetime(timing['ciuchStart'],format='%d/%m/%Y %H:%M:%S,%f')
   timing['ciuchStop']=pd.to_datetime(timing['ciuchStop'],format='%d/%m/%Y %H:%M:%S,%f')
   timing['timestampStart']=timing['timestampStart'].astype(float)
   timing['timestampStop']=timing['timestampStop'].astype(float)
   timing=timing.sort_values('date')
   timing.loc[:,'refListId']=timing.loc[:,'refListId'].apply(lambda x:int(x[8:]))
   timing=timing[['refListId', 'ciuchStart', 'ciuchStop']]
   
   
       # ciuchStart_up starts upstream ciuchStart, half way in between the previous stop and the actual start
   timing[['ciuchStop_last']]=timing[['ciuchStop']].shift(1)
   timing[['refListId_last']]=timing[['refListId']].shift(1)
   timing['ciuchStartup']=timing['ciuchStart'] - (timing['ciuchStart'] - timing['ciuchStop_last'])/2
   # timing start: 10sec before timing
   timing.loc[0,'refListId_last']=timing.loc[0,'refListId']
   timing.loc[0,'ciuchStartup']=timing.loc[0,'ciuchStart']-datetime.timedelta(seconds=10)
   timing.loc[0,'ciuchStop_last']=timing.loc[0,'ciuchStartup']-datetime.timedelta(seconds=10)
   timing['refListId_last']=timing['refListId_last'].astype(int)
   # 
   timing['ciuchStopdown']= timing['ciuchStartup'].shift(-1)
   timing.loc[len(timing)-1,'ciuchStopdown']=timing.loc[len(timing)-1,'ciuchStop']+datetime.timedelta(seconds=10)
   timing=timing[['refListId', 'refListId_last','ciuchStartup', 'ciuchStart','ciuchStop','ciuchStopdown']]
   
   
       # t0_run = a new run starts when box 0 shows up
   t0_run=timing[timing['refListId']==0] [['ciuchStartup']]
   t0_run=t0_run.rename(columns={'ciuchStartup':'t0_run'})
   t0_run=t0_run.groupby('t0_run').size().cumsum().rename('run').reset_index(drop=False)
   t0_run=t0_run.sort_values('t0_run')
   # 
   # each row in timing is merged with a last row in t0_run where t0_run (ciuchstart) <= timing (ciuchstart)
   timing=pd.merge_asof(timing,t0_run,left_on='ciuchStartup',right_on='t0_run', direction='backward')
   timing=timing.sort_values('ciuchStop')
   timing=timing[['run', 'refListId', 'refListId_last', 'ciuchStartup','ciuchStart','ciuchStop','ciuchStopdown','t0_run']]

   
   slices = pd.DataFrame()
   for i, row in timing.iterrows():
       ciuchStartup = row['ciuchStartup']
       ciuchStart = row['ciuchStart']
       ciuchStop = row['ciuchStop']
       ciuchStopdown = row['ciuchStopdown']
       steps = 3

       # Cr�ation des tranches "up"
       up = pd.DataFrame(index=pd.date_range(start=ciuchStartup, end=ciuchStart, periods=steps)) \
           .reset_index(drop=False).rename(columns={'index': 'slice'})
       up['slice_id'] = ['up_' + str(x) for x in range(steps)]
       slices = pd.concat([slices, up], ignore_index=True)

       # Cr�ation des tranches "mid"
       mid = pd.DataFrame(index=pd.date_range(start=ciuchStart, end=ciuchStop, periods=steps)) \
           .reset_index(drop=False).rename(columns={'index': 'slice'})
       mid['slice_id'] = ['mid_' + str(x) for x in range(steps)]
       slices = pd.concat([slices, mid], ignore_index=True)

       # Cr�ation des tranches "down"
       down = pd.DataFrame(index=pd.date_range(start=ciuchStop, end=ciuchStopdown, periods=steps)) \
           .reset_index(drop=False).rename(columns={'index': 'slice'})
       down['slice_id'] = ['down_' + str(x) for x in range(steps)]
       slices = pd.concat([slices, down], ignore_index=True)

   slices.reset_index(drop=False, inplace=True)

   # 
   timing_slices=pd.merge_asof(slices,timing,left_on='slice',right_on='ciuchStartup',direction='backward')
   timing_slices=timing_slices[['run', 'refListId', 'refListId_last','slice_id','slice',  \
                                'ciuchStartup', 'ciuchStart', 'ciuchStop', 'ciuchStopdown','t0_run']]
   
   # merge between df and timing
   # merge_asof needs sorted df > df_ref
   df=df[ (df['LogTime']>=timing['ciuchStartup'].min()) & (df['LogTime']<=timing['ciuchStopdown'].max())  ]
   df=df.sort_values('LogTime')
   # 
   # each row in df_ref is merged with the last row in timing where timing (ciuchstart_up) < df_ref (logtime)
   # 
   # df_timing=pd.merge_asof(df_ref,timing,left_on=['LogTime'],right_on=['ciuchStartup'],direction='backward')
   # df_timing=df_timing.dropna()
   # df_timing=df_timing.sort_values('LogTime').reset_index(drop=True)
   # df_timing=df_timing[['run', 'Epc','refListId', 'refListId_last', 'ciuchStartup',\
   #                      'LogTime', 'ciuchStop', 'ciuchStopdown','Rssi', 'loc', 'refListId_actual']]
   # 
   # each row in df_ref is merged with the last row in timing_slices where timing (slice) < df_ref (logtime)
   # 
   df_timing_slices=pd.merge_asof(df,timing_slices,left_on=['LogTime'],right_on=['slice'],direction='backward')
   df_timing_slices=df_timing_slices.dropna()
   df_timing_slices=df_timing_slices.sort_values('slice').reset_index(drop=True)
   df_timing_slices=df_timing_slices[['run', 'Epc','refListId', 'refListId_last', 'ciuchStartup','slice_id','slice','LogTime', \
                         'ciuchStart','ciuchStop', 'ciuchStopdown', 'Rssi', 'loc','t0_run']]
   df_timing_slices['reflist_run_id'] = df_timing_slices['refListId'].astype(str) +"_"+ df_timing_slices['run'].astype(str)
   
   # runs 16 23 32 40 have missing boxes: discarded
   # also run 1 is the start, no previous box: discarded
   # run 18: box 0 run at the end
   # 
   timing=timing[~timing['run'].isin([1,18,16,23,32,40])]
   timing_slices=timing_slices[~timing_slices['run'].isin([1,18,16,23,32,40])]
   df_timing_slices=df_timing_slices[~df_timing_slices['run'].isin([1,18,16,23,32,40])]

   df_timing_slices=df_timing_slices.sort_values(['LogTime','Epc'])
   
   # df_timing_slices['dt']=
   df_timing_slices['dt']=(df_timing_slices['LogTime']-df_timing_slices['t0_run']).apply(lambda x:x.total_seconds())
   
   timing['reflist_run_id']= timing['refListId'].astype(str)+"_"+ timing['run'].astype(str)
   timing['window_width']=(timing['ciuchStopdown']-timing['ciuchStartup']).apply(lambda x:x.total_seconds())
   windows=timing[['reflist_run_id', 'window_width']]
   rssi_quantite=1
   return [df_timing_slices, windows,rssi_quantite,reflist]


def dataset(df_timing_slices, windows, rssi_quantite):
   ds_rssi = df_timing_slices.groupby(['Epc', 'reflist_run_id', 'slice_id', 'loc'])['Rssi'].quantile(rssi_quantite) \
                .unstack(['slice_id', 'loc'], fill_value=-110)
   ds_rssi.columns = [x[0] + '_' + x[1] for x in ds_rssi.columns]
   ds_rssi = ds_rssi.reset_index(drop=False)
   
   ds_rc = df_timing_slices.groupby(['Epc', 'reflist_run_id', 'slice_id', 'loc']).size() \
              .unstack(['slice_id', 'loc'], fill_value=0)
   ds_rc.columns = [x[0] + '_' + x[1] for x in ds_rc.columns]
   ds_rc = ds_rc.reset_index(drop=False)
   
   ds = pd.merge(ds_rssi, ds_rc, on=['Epc', 'reflist_run_id'], suffixes=['_rssi', '_rc'])
   ds = pd.merge(ds, windows, on='reflist_run_id', how='left')
   
   Q_Epcs_window = df_timing_slices.groupby(['reflist_run_id'])['Epc'].nunique().rename('Epcs_window').reset_index(drop=False)
   ds = pd.merge(ds, Q_Epcs_window, on='reflist_run_id', how='left')
   
   Q_reads_window = df_timing_slices.groupby(['reflist_run_id']).size().rename('reads_window').reset_index(drop=False)
   ds = pd.merge(ds, Q_reads_window, on='reflist_run_id', how='left')
   
   ds=pd.merge(ds, pretraitement_knn()[3], on='Epc', how='left')
   ds['actual']=ds['reflist_run_id'].apply(lambda x: x.split ('_')[0]).astype('int64')== ds['refListId_actual']
   ds['actual'] = ds ['actual'].replace({True: 'IN' , False: 'OUT'})

   
   return ds
#data = pretraitement_knn()
#colonne=dataset(pretraitement_knn()[0],pretraitement_knn()[1],pretraitement_knn()[2]).columns


def Xcols_func(features, Xcols_all):
   Features=pd.DataFrame(\
       [\
        ['all', True, True, False, True, True, True],\
        ['rssi & rc only', True, True, False, False, False, False],\
        ['rssi & rc_mid', True, True, True, False, False, False],\
        ['rssi only', True, False, True, False, False, False],\
        ['rc only', False, True, False, False, False, False],\
       ], columns=['features', 'rssi', 'rc', 'rc_mid_only', 'Epcs_window', 'reads_window', 'window_width'])
   Features

   Features_temp = Features[Features['features']==features]
   
   X=[]
   rssi = Features_temp ['rssi'].values[0]
   rc = Features_temp['rc'].values[0]
   rc_mid_only = Features_temp['rc_mid_only'].values[0]
   Epcs_window =  Features_temp['Epcs_window'].values[0]
   reads_window =  Features_temp['reads_window'].values[0]
   window_width =  Features_temp['window_width'].values[0]
   
   colonne = Xcols_all
   
   X_rssi = [x for x in colonne if rssi*'rssi' in x.split('_')]
   X_rc = [x for x in colonne if rc*'rc' in x.split('_')]
   
   X = X_rssi + X_rc
   
   if Epcs_window:
       X.append('Epcs_window')
   if reads_window:
       X.append('reads_window')
   if window_width:
       X.append('window_width')
       
   return X


def knnn(ds:pd.DataFrame,k_neighbors = 8, weight = 'distance', metrics = 'manhattan' ):
   from sklearn.preprocessing import LabelEncoder
   from sklearn.neighbors import KNeighborsClassifier
   from sklearn.model_selection import cross_val_score
   from sklearn.model_selection import cross_val_predict
   import matplotlib.pyplot as plt


   label_encoder = LabelEncoder()
   # data = pretraitement_knn() 
   # ds = dataset(data[0],data[1],data[2])
   X = ds[Xcols_func('rssi & rc only',ds.columns)]
   y = LabelEncoder().fit_transform(ds['actual'])
   #Cr�ation de l'instance du classificateur KNN
   knn = KNeighborsClassifier(n_neighbors = k_neighbors, weights=weight,metric=metrics)
   
   y_pred = cross_val_predict(knn, X, y, cv=20)
   accuracies=cross_val_score(knn,X,y,cv=5,scoring='accuracy')
   accuracy= accuracies.mean()
   
   
   # Créer un boxplot pour les valeurs prédites
   plt.figure(figsize=(10, 6))
   plt.boxplot(accuracies, labels=['Accuracy'], patch_artist=True)
   plt.title('Distribution des Accuracies pour le KNN')
   plt.ylabel('Accuracy')
   plt.savefig('courbes/boxplot/knn/knn_accuracy.png')
   
   return accuracy*100




def evaluate_adaboost_rf(ds:pd.DataFrame,cv_folds,n_estimators=100,criterion='gini',max_depth=None,min_samples_split=2,min_samples_leaf=1,min_weight_fraction_leaf=0.0,max_features='sqrt',max_leaf_nodes=None,min_impurity_decrease=0.0,bootstrap=True,oob_score=False,n_jobs=None,random_state=None,verbose=0,warm_start=False,class_weight=None,ccp_alpha=0.0,max_samples=None):
   label_encoder = LabelEncoder()
   # data = pretraitement_knn() 
   # ds = dataset(data[0],data[1],data[2])
   X = ds[Xcols_func('rssi & rc only',ds.columns)]
   y = LabelEncoder().fit_transform(ds['actual'])
   # Création du modèle de base (arbre de décision)
   base_estimator = RandomForestClassifier(n_estimators=n_estimators,criterion=criterion,max_depth=max_depth,min_samples_split=min_samples_split,min_samples_leaf=min_samples_leaf,min_weight_fraction_leaf=min_weight_fraction_leaf,max_features=max_features,max_leaf_nodes=max_leaf_nodes,min_impurity_decrease=min_impurity_decrease,bootstrap=bootstrap,oob_score=oob_score,n_jobs=n_jobs,random_state=random_state,verbose=verbose,warm_start=warm_start,class_weight=class_weight,ccp_alpha=ccp_alpha,max_samples=max_samples)
   
   # Création du modèle AdaBoost avec les hyperparamètres spécifiés
   ada = AdaBoostClassifier( estimator=base_estimator,n_estimators=50, learning_rate=1.0, random_state=42,algorithm='SAMME')
   # Validation croisée pour évaluer l'accuracy
   accuracies = cross_val_score(ada, X, y, cv=cv_folds, scoring='accuracy')
   # Calcul de la moyenne des scores de précision
   mean_accuracy = accuracies.mean()
   return mean_accuracy
# Exemple d'utilisation
#data = pretraitement_knn() 
#ds = dataset(data[0],data[1],data[2])
#mean_accuracy = evaluate_adaboost_rf(ds,cv_folds=5,n_estimators=100,criterion='gini',max_depth=None,min_samples_split=2,min_samples_leaf=1,min_weight_fraction_leaf=0.0,max_features='sqrt',max_leaf_nodes=None,min_impurity_decrease=0.0,bootstrap=True,oob_score=False,n_jobs=None,random_state=None,verbose=0,warm_start=False,class_weight=None,ccp_alpha=0.0,max_samples=None)
#print(f"Mean accuracy of the model across all folds: {mean_accuracy}")


def evaluate_adaboost_knn(ds:pd.DataFrame,cv_folds,n_neighbors=5, weights='uniform', algorithm='auto', leaf_size=30, p=2, metric='minkowski', n_jobs=None):
   label_encoder = LabelEncoder()
   # data = pretraitement_knn() 
   # ds = dataset(data[0],data[1],data[2])
   X = ds[Xcols_func('rssi & rc only',ds.columns)]
   y = LabelEncoder().fit_transform(ds['actual'])
   # Création du modèle de base (KNN)
   base_estimator = KNeighborsClassifier(n_neighbors=n_neighbors, weights=weights, algorithm=algorithm, leaf_size=leaf_size, p=p, metric=metric, n_jobs=n_jobs)
   # Création du modèle AdaBoost avec les hyperparamètres spécifiés
   ada = AdaBoostClassifier(estimator=base_estimator, n_estimators=50, learning_rate=1.0, random_state=42,algorithm='SAMME')
   # Validation croisée pour évaluer l'accuracy
   accuracies = cross_val_score(ada, X, y, cv=cv_folds, scoring='accuracy')
   # Calcul de la moyenne des scores de précision
   mean_accuracy = accuracies.mean()
   return mean_accuracy




def evaluate_adaboost_svm(ds:pd.DataFrame,cv_folds,C=1.0, kernel='rbf', degree=3, gamma='scale', coef0=0.0, shrinking=True, probability=False, tol=0.001, cache_size=200, class_weight=None, verbose=False, max_iter=-1, decision_function_shape='ovr', break_ties=False, random_state=None):
    label_encoder = LabelEncoder()
    # data = pretraitement_knn() 
    # ds = dataset(data[0],data[1],data[2])
    X = ds[Xcols_func('rssi & rc only',ds.columns)]
    y = LabelEncoder().fit_transform(ds['actual'])
    # Création du modèle de base (KNN)
    base_estimator = SVC(C=C, kernel=kernel, degree=degree, gamma=gamma, coef0=coef0, shrinking=shrinking, probability=probability, tol=tol, cache_size=cache_size, class_weight=class_weight, verbose=verbose, max_iter=max_iter, decision_function_shape=decision_function_shape, break_ties=break_ties, random_state=random_state)
    # Création du modèle AdaBoost avec les hyperparamètres spécifiés
    ada = AdaBoostClassifier(estimator=base_estimator, n_estimators=5, learning_rate=1.0, random_state=42,algorithm='SAMME')
    # Validation croisée pour évaluer l'accuracy
    accuracies = cross_val_score(ada, X, y, cv=cv_folds, scoring='accuracy')
    # Calcul de la moyenne des scores de précision
    mean_accuracy = accuracies.mean()
    return mean_accuracy
