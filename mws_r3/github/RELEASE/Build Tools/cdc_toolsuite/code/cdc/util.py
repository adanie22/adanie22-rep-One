# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#
#########################################################
# maintenance log:
#    date       version      author
#   9/07/15       1.1         A.Daniels
#  added def for translating component ids for compute rept
#  (get_component_id_compute)
#
#########################################################

from datetime import datetime
from lxml import etree as et
import pickle,base64
import re

def m_type(m):
    return {
        'com.servicemesh.agility.api.Workload':'Workload',
        'com.servicemesh.agility.api.DesignContainer': 'DesignContainer',
        'com.servicemesh.agility.api.BlueprintRef': 'BlueprintRef'
    }[m]

def datestamp5():
    return datetime.now().strftime("%Y%m%dT%H%M")

def timestamp_to_date(t):
    try:
        dte = datetime.fromtimestamp(int(t/1000))
#        return dte.strftime('%d/%m/%Y')
        return dte.strftime('%Y%m%d')
    except TypeError:
        return ''

def translate_machine_state(state):
    try:
          return {
                  'RUNNING':'ONLINE',
                  'STOPPED':'OFFLINE'			
	             }[state]
    except KeyError:
	       return state
		
def timestamp_to_date1(t):
    try:
        dte = datetime.fromtimestamp(int(t/1000))
        return dte.strftime('%d/%m/%Y')
    except TypeError:
        return ''

def timestamp2date(x):
    f1 = lambda x:datetime.fromtimestamp(int(x/1000)).strftime('%Y%m%d')
    return f1(x)

def normalise_assetname(s):
    """
    eg, remove non-words, spaces, etc from asset names for easier file handling
    """
    return re.sub('\W','_',s)

def a_path(m):
    """
    expand shortform asset type (BP,PKG,SCRPT) into longer string, mainly for rest calls, ie BP >> blueprint
    """
    return {
        'BP':'blueprint',
        'PKG':'package',
        'SCRPT':'script'
    }[m]

def make_cid(atype,cid,scid,id,name):
    return '%s-%s-%s-%s-%s'%(atype,cid,scid,id,name)

def get_id_from_cid(cid):
    return re.match('(.*)-(.*)-(.*)-.*',cid).groups()[2]  

def short_id(asset_type,id):
    """
    eg used as index hash tag for assets in cache
    """
    return '%s-%s'%(asset_type,id)

def get_component_id(bp_id,bps_cfg):
    """
    matches component_id to blueprint ID, based on entries in cdc_CIs.json file.
    eg: 
    "blueprints": { 
        "1619":{"CID":"SP","creator":"mkrynina","BASELINE":["R3"],"name":""},
        "1632":{"CID":"EM","creator":"skoklevski","BASELINE":["R3"],"name":""},
    """
    try:
        return bps_cfg[str(bp_id)]['CID']
    except KeyError:
        return 'NOT_IN_LOAD_ASSET'

def get_component_id_compute(cmp_id,cmp_cfg):
    """
    matches component_id to blueprint ID, based on entries in cdc_CIs.json file.
    eg: 
    "blueprints": { 
        "1619":{"CID":"SP","creator":"mkrynina","BASELINE":["R3"],"name":""},
        "1632":{"CID":"EM","creator":"skoklevski","BASELINE":["R3"],"name":""},
    """
    try:
        return cmp_cfg[cmp_id]['CMP']
    except KeyError:
        return 'NOT_IN_LOAD_ASSET'		
def get_baseline_versions(bp_id,bps_cfg):
    """
    matches component_id to blueprint ID, based on entries in cdc_CIs.json file.
    """
    try:
        return ','.join(bps_cfg[str(bp_id)]['BASELINE'])
    except KeyError:
        return ''

