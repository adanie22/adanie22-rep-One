# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#
"""
Asset classes for Blueprints, Workloads, Packages and Scripts; populates objects from REST calls into Agility.
Most code in this module expects json format, for most xml handling, refer to xml module.
"""

from pprint import pprint as pp
import os
import default
import re
import zipfile
import StringIO
from datetime import datetime
from itertools import groupby
from util import *
import requests
requests.packages.urllib3.disable_warnings()
from lxml import etree as et

def get_vars(short_id,s_name,j):
    """
    populate variables given an assets json data;
    return variables as list, empty list if none, in the format:

    [PARENT_SHORT_ID,VAR_NAME,VAR_VALUE,PROPERTY_TYPE,ISDEFAULT]

    NB: Along with PARENT_SHORT_ID, ISDEFAULT is used in simplistic reporting of blueprint variables; intention being "isdefault", will have lower priority 
    in a contained asset (Package, script,etc ) if the parent asset has explicity set the variable value

    """
    l = []

    if j.has_key('variables'):
        for var in j['variables']:
            try:
                if var.has_key('stringValue'):
                    l.append((short_id,s_name,var['name'],var['stringValue'],var['propertyType']['name'],False))
                elif var.has_key('intValue'):
                    l.append((short_id,s_name,var['name'],var['intValue'],var['propertyType']['name'],False))
                elif var.has_key('booleanValue'):
                    l.append((short_id,s_name,var['name'],var['booleanValue'],var['propertyType']['name'],False))
                elif var.has_key('floatValue'):
                    l.append((short_id,s_name,var['name'],var['floatValue'],var['propertyType']['name'],False))
                elif var.has_key('defaultValues'):
                    if var['defaultValues'][0].has_key('booleanValue'):
                        l.append((short_id,s_name,var['name'],'%s'%var['defaultValues'][0]['booleanValue'],var['propertyType']['name'],True))
                    elif var['defaultValues'][0].has_key('intValue'):
                        l.append((short_id,s_name,var['name'],'%s'%var['defaultValues'][0]['intValue'],var['propertyType']['name'],True))
                    elif var['defaultValues'][0].has_key('floatValue'):
                        l.append((short_id,s_name,var['name'],'%s'%var['defaultValues'][0]['floatValue'],var['propertyType']['name'],True))
                    else: 
                        l.append((short_id,s_name,var['name'],'%s'%var['defaultValues'][0]['stringValue'],var['propertyType']['name'],True))
                elif var.has_key('propertyTypeValue'):
                        l.append((short_id,s_name,var['name'],','.join([i['value'] for i in var['propertyTypeValue']['rootValues']]),var['propertyType']['name'],True))
                else:
                    l.append((short_id,s_name,var['name'],'',var['propertyType']['name'],True))
            except KeyError:
                print "************ UNEXPECTED PROBLEM POPULATING VARIABLE **************"
                print short_id,var
                print "*******************************************************************"
    return l

class Resources():
    """
    A utility class for Resources in json
    """
    def __init__(self,x):
        self.json = x
        self.resources = self.f_resources()

    def f_resources(self):
        d = {}
        if self.json.has_key('resources'):
            for res in self.json['resources']:
                if res.has_key('stringValue'):
                    #d.setdefault(res['name'],res['stringValue'])
                    #d.setdefault(res['name'],'(%s) %s: %s'%(res['resourceType'],res['name'],res['stringValue']))
                    d.setdefault(res['name'],(res['resourceType'],res['name'],res['stringValue']))
                if res.has_key('quantity'):
                    #d.setdefault(res['name'],'(%s) %s: %s'%(res['resourceType'],res['name'],str(int(res['quantity']))))
                    d.setdefault(res['name'],(res['resourceType'],res['name'],str(int(res['quantity']))))
        return d

class Script:
    """
    Models an Agility Script asset
    """
    def __init__(self,x_json,isDebug=False):
        self.json = x_json
        self.id = self.json['id']
        self.name,self.parent_id,self.parent_name,self.body = self.json['name'],self.json['parent']['id'],self.json['parent']['name'],self.json['body']
        self.version = self.json['version']
        self.latest = self.json['latest']
        
        self.mtime = self.json['lastModified']
        self.asset_type = 'SCRPT'
        self.sid = short_id(self.asset_type,self.id)
        self.vars = get_vars(self.sid,self.name,x_json)
        self.CID_ = 'CID'
        self.SCID_ = 'SCID'
        self.short_id = 'SCRPT-%s'%self.id

    def _get_cid(self):
        return 'SCRIPT-%s-%s-%s-%s'%(self.CID_,self.SCID_,self.id,normalise_assetname(self.name))

    def __repr__(self):
        return self.short_id

class Package:
    """
    Models an Agility Package asset
    """

    """
    intersection keys:

        set([u'assetPath', u'assetType', u'checkoutAllowed', u'created', u'creator', u'dependencies', u'description', u'detailedAssetPath', u'domain', u'id', u'installs', u'lastModified', u'latest', u'lifecycleVersion', u'lockType', u'name', u'operatingSystems', u'operationals', u'parent', u'publishComment', u'publisher', u'removable', u'slot', u'slotId', u'startups', u'top', u'uuid', u'version', u'versionStatus'])

    difference keys:

        set([u'dependencies', u'shutdowns', u'startups'])
    """
    def __init__(self,x_json,isDebug=False):
        self.json = x_json
        self.id = self.json['id']
        self.name,self.parent_id,self.parent_name = self.json['name'],self.json['parent']['id'],self.json['parent']['name']
        self.version = self.json['version']
        self.latest = self.json['latest']
        self.mtime = self.json['lastModified']
        self.CID_ = 'CID'
        self.SCID_ = 'SCID'
        self.short_id_ = make_cid('PKG',self.CID_,self.SCID_,self.id,normalise_assetname(self.name))
        self.short_id = 'PKG-%s'%self.id
        self.asset_type = 'PKG'
        self.sid = short_id(self.asset_type,self.id)
        self.vars = get_vars(self.sid,self.name,x_json)
        if len(self.vars) > 0:
            print 'init pkg %s - has vars:'%self.short_id_
        self.scripts = []
        if self.json.has_key('installs'): 
            self.has_installs = True
        else:
            self.has_installs = False

        if self.json.has_key('operationals'): 
            self.has_operationals = True
        else:
            self.has_operationals = False

        self.load_scripts()
        self.s_vars = []

        for s_type,s in self.scripts:
            for s_id,s_name,k,v,ptype,is_default in s.vars:
                self.s_vars.append((s_id,s_name,k,v,ptype,is_default))

    def load_scripts(self):
        if self.has_installs:
            self.scripts.extend([('I',get_asset(i['id'],asset_type='SCRPT')) for i in self.json['installs']])
        if self.has_operationals:

            def f1(x):
                """
                 x = ('O', SCRIPT-CID-SCID-681-MWS2R2___Get_App_Files)

                 only add if operational script NOT already included as install script

                """
                s_type,s = x
                if s.short_id in [j.short_id for m,j in self.scripts]:
                    return False
                else:
                    return True

            self.scripts.extend(filter(f1,[('O',get_asset(i['id'],asset_type='SCRPT')) for i in self.json['operationals']]))
        
    def get_meta(self):
        return [self.id,self.name,self.parent_name]

    def _get_cid(self):
        return 'PKG-%s-%s-%s-%s'%(self.CID_,self.SCID_,self.id,normalise_assetname(self.name))

    def __repr__(self):
        return self.short_id

class Packages():
    """
    Thin wrapper for packages, used in Workloads class
    """
    def __init__(self,x):
        self.json = x
        self.packages = self.f_packages()

    def f_packages(self):
        l = []
        if self.json.has_key('packages'):
            for i in self.json['packages']:
                l.append(i['id'])
        return l

class Workload:
    """
    Models an Agility Workload asset
    """


    def __init__(self,x_json,bp_cid,isDebug=False):
        self.json = x_json
        self.bp_cid = bp_cid
        self.id = self.json['id']
        self.name,self.parent_id,self.parent_name = self.json['name'],self.json['parent']['id'],self.json['parent']['name']
        self.mtime = self.json['lastModified']
        self.type = m_type(self.json['__type'])

        self.ro = Resources(self.json)
        self.resources = self.ro.resources

        self.asset_type = 'WKLD'
        self.sid = short_id(self.asset_type,self.id)
        self.vars = get_vars(self.sid,self.name,x_json)

        self.ap = Packages(self.json)
        self.packages = []
        for i in self.ap.packages:
            self.packages.append(get_asset(i,asset_type='PKG'))

        """
        self.p_vars = set()

        for p in self.packages:
            for s_id,s_name,k,v,ptype,is_default in sorted(p.s_vars,key=lambda x:x[1]):
                self.p_vars.add((self.sid,s_id,s_name,k,v,ptype,is_default)) 
        """

        self.CID_ = 'CID'
        self.SCID_ = 'SCID'
        self.short_id = 'WKLD-%s'%self.id

    def _get_cid(self):
        return 'WKLD-%s-%s-%s-%s'%(self.CID_,self.SCID_,self.id,normalise_assetname(self.name))

    def __repr__(self):
        return self.short_id

class Bp:
    """
    Models an Agility Blueprint asset
    """
    def __init__(self,x_json,cfg,isDebug=False):
        self.json = x_json
        self.cfg = cfg
        self.id = self.json['id']
        self.name,self.description,self.creator,self.parent_id,self.parent_name = self.json['name'],self.json['description'],self.json['creator']['name'],self.json['parent']['id'],self.json['parent']['name']
        self.mtime = self.json['lastModified']

        self.asset_type = 'BP'
        self.sid = short_id(self.asset_type,self.id)
        self.vars = get_vars(self.sid,self.name,x_json)

        self.CID_ = 'CID'
        self.SCID_ = 'SCID'

        if self.json.has_key('variables'):
            for j in self.json['variables']:
                if j['name'] == 'CID': self.CID_ = j['stringValue']
                if j['name'] == 'SCID': self.SCID_ = j['stringValue']

        self.short_id = 'BP-%s'%self.id
        self.workloads = self.get_anyorder('Workload')

    def get_xml(self,asset_type='BP',hdrs={'accept':'application/xml'}):
        url,user,passwd = '%s/%s/%s?fields=id'%(self.cfg['BASE_URL'],a_path(asset_type),self.id),'%s'%self.cfg['USER'],'%s'%self.cfg['PASSWD']
        r = requests.get(url, auth=('%s'%user, '%s'%passwd),verify=False,headers=hdrs)
        return et.fromstring(str(r.text.encode('ascii', 'ignore')))

    def write_xml(self,xml0,asset_type='BP',hdrs={'content-type':'application/xml'}):
        url,user,passwd = '%s/%s/%s'%(self.cfg['BASE_URL'],a_path(asset_type),self.id),'%s'%self.cfg['USER'],'%s'%self.cfg['PASSWD']
        resp = requests.put(url, data=et.tostring(xml0),auth=('%s'%user, '%s'%passwd),verify=False,headers=hdrs)
        return resp

    def get_anyorder(self,otype):
        l = []
        if self.json.has_key('anyOrderItems'):
            for x_json in self.json['anyOrderItems']:
                if m_type(x_json['__type']) == otype:
                    l.append(Workload(x_json,self.short_id))
        return l
        
    def get_meta(self):
        return [self.id,self.name,self.parent_name]

    def _get_cid(self):
        return 'BP-%s-%s-%s-%s'%(self.CID_,self.SCID_,self.id,normalise_assetname(self.name))

    def __repr__(self):
        return self.short_id

def get_pack(id,l_asset_cache):
    if len([i for i in l_asset_cache if i['id'] == id]) > 0:
        return [i for i in l_asset_cache if i['id'] == id][0]
    else:
        return None

##########################################################################################################################
# CACHE CLASSES AND FUNCTIONS
##########################################################################################################################

class lP(object):

    def __init__(self):
        self.lp_ = {}

    def is_p(self,id):
        if self.lp_.has_key(id):
            return True
        else:
            return False

    def get_p(self,id):
        if self.lp_.has_key(id):
            return self.lp_[id]
        else:
            return None

    def set_p(self,id_,p):
        self.lp_[id_] = p


def make_cache_id(asset_type,id):
    """
    l_asset_cache variable is an asset cache, ie lookup up keys to find asset json, if not there, make the rest call and then
    add new asset (key + json) into cache, for subsequent lookups
    """
    return  '%s-%s'%(asset_type,id)

def get_asset(id,asset_type='PKG',env=default.ENV,maxn=default.MAXN,isdebug=False):
    """
    load cache for asset, id. 

    in (asset_type [required]): BP, PKG, SCRPT. 
    in (env [required]): used for Agility rest connection
    """
    id_ = '%s-%s'%(asset_type,id)

    if not isdebug and l_asset_cache.is_p(id_):
        return l_asset_cache.get_p(id_)
    else:
        cfg = default.CFG[env]
        url,user,passwd = '%s/%s/%s'%(cfg['BASE_URL'],a_path(asset_type),id),'%s'%cfg['USER'],'%s'%cfg['PASSWD']
        r = requests.get(url, auth=('%s'%user, '%s'%passwd),verify=False,headers={'accept':'application/json'})
        if r.status_code == 200:
            if asset_type=='PKG':
                j = r.json()
                p = Package(j)
                l_asset_cache.set_p(id_,p)
                return p
            elif asset_type=='SCRPT':
                j = r.json()
                p = Script(j)
                l_asset_cache.set_p(id_,p)
                return p
            elif asset_type=='BP':
                j = r.json()
                p = Bp(j,cfg)
                l_asset_cache.set_p(id_,p)
                return p
        else:
            print 'Unable to retrive asset %s, %s; response code %s'%(id,asset_type,r.status_code)
            return None
    return None

def get_assets(asset_type,asset_ids=None):
    """

    return assets from (cache must already be loaded), of type asset_type (BP, PKG, SCRPT)

    :ref:`cdc_CIs_json`

    """

    def f1(x):
        if asset_ids == None: return True
        else:
            if x['id'] in asset_ids:
                return True
        return False

    l = []
    [l.append(l_asset_cache.lp_[i]) for i in l_asset_cache.lp_.keys() if re.match('(.*)-.*',i).groups()[0] == asset_type]
    return l

def load_assets(asset_types=['SCRPT','PKG','BP','TOP'],asset_names=None,env=default.ENV,maxn=default.MAXN):
    """"
    load assets into cache; can specify asset_type. For more granular loading of assets, 
    use repeated calls to get_asset(id,asset_type) instead.
    """

    def f1(x):
        if parent_ids == None: return True
        else:
            if x['parent']['id'] in parent_ids:
                return True
        return False

    l_asset_cache.lp_ = {}
    cfg = default.CFG[env]
    for asset_type in asset_types:
        url,user,passwd = '%s/%s/search?limit=%s'%(cfg['BASE_URL'],a_path(asset_type),maxn),'%s'%cfg['USER'],'%s'%cfg['PASSWD']

        print url
        r = requests.get(url, auth=('%s'%user, '%s'%passwd),verify=False,headers={'accept':'application/json'})
        if asset_type=='BP':
            def update_lp(l_asset_cache,asset_type,bp):
                l_asset_cache.set_p(make_cache_id(asset_type,bp.id),bp)
            for j in r.json()['assets']:
                if not asset_names is None: 
                    cid = make_cid(a_path(asset_type),'CID','SCID',j['id'],normalise_assetname(j['name']))
                    if cid in asset_names: 
                        print 'processing for cid %s..'%cid
                        bp = Bp(j,cfg)
                        update_lp(l_asset_cache,asset_type,bp)
                else:
                    bp = Bp(j,cfg)
                    update_lp(l_asset_cache,asset_type,bp)

        print 'Number of assets returned from REST call: %i'%len(r.json()['assets'])
        """
        elif asset_type=='PKG':
            l = [Package(p,isDebug=False) for p in filter(f1,r.json()['assets'])]
            [l_asset_cache.set_p(make_cache_id(asset_type,i.id),i) for i in l]
        elif asset_type=='SCRPT':
            l = [Script(p,isDebug=False) for p in filter(f1,r.json()['assets'])]
            [l_asset_cache.set_p(make_cache_id(asset_type,i.id),i) for i in l]
        elif asset_type=='TOP':
            l = [Script(p) for p in filter(f1,r.json()['assets'])]
            [l_asset_cache.set_p(make_cache_id(asset_type,i.id),i) for i in l]
        """
    print 'Number of assets loaded into cache (will include contained assets): %i'%len(l_asset_cache.lp_)

#***********************************************************************************
l_asset_cache = lP()
#***********************************************************************************

############################################################################################################
# misc functions below
############################################################################################################
def get_environments(env=default.ENV):
    def get_json():
        if r.status_code == 200:
            return [(i['id'],i['name'],'parent: %s, %s'%(i['parent']['id'],i['parent']['name'])) for i in r.json()['assets']]
        else:
            return None
    cfg = default.CFG[env]
    url,user,passwd = '%s/environment/search?fields=id,name,parent'%(cfg['BASE_URL'],),'%s'%cfg['USER'],'%s'%cfg['PASSWD']
    r = requests.get(url, auth=('%s'%user, '%s'%passwd),verify=False,headers={'accept':'application/json'})
    return  get_json()

def update_bps(bps,container_id,env):
    """
    Update ram, cpu values
    """
    l_bps = bps.get_bps(container_id)
    for i in bps.d_bps.keys():
        b = Bp(i,cfg) #env=env)
        print i
        resp = b.update_ram_cpu2(2,2)
        print b.json['name'],b.json['id'],resp


