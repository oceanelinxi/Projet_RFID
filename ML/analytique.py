import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import os
import datetime





def reflist(pathfile = r'Uploads/data_anonymous'):
    # pathfile = path_file
    reflist = pd.DataFrame()
    # 
    files=os.listdir(pathfile)
    for file in files:
        # print(file)
        if file.startswith('reflist_'):
            temp=pd.read_csv(os.path.join(pathfile,file),sep=',').reset_index(drop=True)[['Epc']]
            temp['refListId']=file.split('.')[0]
            reflist = pd.concat([reflist, temp], axis = 0)
            #reflist=reflist.append(temp)
    reflist=reflist.rename(columns = {'refListId':'refListId_actual'})
    reflist['refListId_actual'] = reflist['refListId_actual'].apply(lambda x:int(x[8:]))
    Q_refListId_actual=reflist.groupby('refListId_actual')['Epc'].nunique().rename('Q refListId_actual').reset_index(drop=False)
    reflist=pd.merge(reflist,Q_refListId_actual,on='refListId_actual',how='left')
    return reflist

def df_tags(pathfile = r'Uploads/data_anonymous'):
    df=pd.DataFrame()

    files=os.listdir(pathfile)
    for file in files:
        if file.startswith('ano_APTags'):
            # print(file)
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
    return df 

def timing(pathfile = r'Uploads/data_anonymous'):
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
    return timing

def timing_slices(timing):
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

    return timing_slices

def df_timing_slices(timing_slices, timing, df):
    df=df[ (df['LogTime']>=timing['ciuchStartup'].min()) & (df['LogTime']<=timing['ciuchStopdown'].max())  ]
    df=df.sort_values('LogTime')
    # each row in df_ref is merged with the last row in timing where timing (ciuchstart_up) < df_ref (logtime)

    # df_timing=pd.merge_asof(df_ref,timing,left_on=['LogTime'],right_on=['ciuchStartup'],direction='backward')
    # df_timing=df_timing.dropna()
    # df_timing=df_timing.sort_values('LogTime').reset_index(drop=True)
    # df_timing=df_timing[['run', 'Epc','refListId', 'refListId_last', 'ciuchStartup',\
    #                      'LogTime', 'ciuchStop', 'ciuchStopdown','Rssi', 'loc', 'refListId_actual']]
    # each row in df_ref is merged with the last row in timing_slices where timing (slice) < df_ref (logtime)
    df_timing_slices=pd.merge_asof(df,timing_slices,left_on=['LogTime'],right_on=['slice'],direction='backward')
    df_timing_slices=df_timing_slices.dropna()
    df_timing_slices=df_timing_slices.sort_values('slice').reset_index(drop=True)
    df_timing_slices=df_timing_slices[['run', 'Epc','refListId', 'refListId_last', 'ciuchStartup','slice_id','slice','LogTime', \
                          'ciuchStart','ciuchStop', 'ciuchStopdown', 'Rssi', 'loc','t0_run']]
    df_timing_slices['reflist_run_id'] = df_timing_slices['refListId'].astype(str) +"_"+ df_timing_slices['run'].astype(str)

    # runs 16 23 32 40 have missing boxes: discarded
    # also run 1 is the start, no previous box: discarded
    # r
    timing=timing[~timing['run'].isin([1,18,16,23,32,40])]
    timing_slices=timing_slices[~timing_slices['run'].isin([1,18,16,23,32,40])]
    df_timing_slices=df_timing_slices[~df_timing_slices['run'].isin([1,18,16,23,32,40])]

    df_timing_slices=df_timing_slices.sort_values(['LogTime','Epc'])
    # df_timing_slices['dt']=
    df_timing_slices['dt']=(df_timing_slices['LogTime']-df_timing_slices['t0_run']).apply(lambda x:x.total_seconds())

    return df_timing_slices


def analytical(df_timing_slices, timing_slices, reflist):
    ana = df_timing_slices.groupby(['Epc', 'reflist_run_id', 'slice_id', 'loc']) ['Rssi'].max()\
    .unstack('loc', fill_value =- 110).reset_index(drop=False)

    order=pd.DataFrame(timing_slices['slice_id'].unique(), columns=['slice_id'])
    order['order']=order. index

    ana=pd.merge(ana, order, on='slice_id', how='left')
    ana = ana [['Epc', 'reflist_run_id', 'slice_id', 'in', 'out', 'order']]
    # Last subslice_id with out>in
    ana_out =ana [ ana['out']>ana['in'] ] \
    .sort_values(['Epc', 'reflist_run_id', 'order'], ascending=False) \
    .drop_duplicates(['Epc', 'reflist_run_id'])
    # first subslice_id with in/out
    ana_in =ana [ ana['in']>ana['out'] ] \
    .sort_values(['Epc', 'reflist_run_id', 'order'], ascending=True) \
    .drop_duplicates(['Epc', 'reflist_run_id'])

    ana = pd.merge(ana_in, ana_out, on=['Epc', 'reflist_run_id'], suffixes=['_IN', '_OUT'], how='inner')\
    .sort_values(['Epc', 'reflist_run_id'])
    ana = pd.merge(ana, reflist, on='Epc', how='left')

    ana['pred_ana_bool']= ana['reflist_run_id'].apply(lambda x:x.split('_')[0]).astype('int64') == ana['refListId_actual']

    return ana

def methode_analytique():
    pathfile = r'Uploads/data_anonymous'
    Reflist = reflist(pathfile)
    Timing = timing(pathfile)
    df = df_tags(pathfile)
    Timing_slices = timing_slices(Timing)
    Df_timing_slices = df_timing_slices(Timing_slices, Timing, df)
    analytics = analytical(Df_timing_slices, Timing_slices, Reflist)
    trues, total = analytics[analytics['pred_ana_bool']==True].shape[0], analytics.shape[0]
    accur = 100 * trues / total

    # Nombre de tags trouv� par carton par tour

    #  cartons = analytics[['reflist_run_id','refListId_actual','Q refListId_actual', 'pred_ana_bool']]
    #  cartons['n_true'] = cartons['pred_ana_bool'].astype(int)
    #  cartons['run_id'] = cartons['reflist_run_id'].str.split('_').str[1]
    
    #  pred1 = cartons[['refListId_actual','reflist_run_id','Q refListId_actual','n_true','run_id']].groupby(['refListId_actual','Q refListId_actual','run_id'])['n_true'].sum()
    #  pred2 = cartons[['refListId_actual','run_id','pred_ana_bool']].groupby(['refListId_actual','run_id']).count()
    #pred1.merge(pred2 on 

    #  result = pd.merge(pred2.rename(columns = {'reflist_run_id':'Total_tags_et_tour', 'pred_ana_bool':'Nombre de predictions'}), \
    #                pd.DataFrame(pred1), on = ['refListId_actual','run_id'], how='inner')

    #result.sort_values(by=['refListId_actual','run_id'], ascending=True).to_csv('resultats methode anaytique.csv')
    print(accur)

    return accur 