# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#

import os,sys

from nose import with_setup
from nose.plugins.attrib import attr

from pprint import pprint as pp
import pickle,base64
from xml_compare import xml_compare as xc
from lxml import etree as et

sys.path.append('..')
from cdc import blueprints as bps
from cdc import default
from cdc.xml import *
from cdc.util import *

from copy import deepcopy
from datetime import datetime

ns={'ns1':'http://servicemesh.com/agility/api'}

def timestamp2(n):
    return datetime.strftime(n,'%H%M%S')
    
################################################################################################################
# FIXTURES
################################################################################################################
@attr('var_test1')
def var_test1_runner():
    #lfns = ['bp-1431.xml','bp-1431b.xml']
    lfns = ['bp-1431b.xml']
    for fn in lfns:
        yield var_test1,fn

@attr('var_test2')
def var_test2_runner():
    lfns = ['bp-1431c.xml']
    for fn in lfns:
        yield var_test2,fn

@attr('var_test3')
def var_test3_runner():
    lfns = ['bp-1431c.xml']
    for fn in lfns:
        yield var_test3,fn

@attr('bp_test_delete_zzcfg')
def bp_test_delete_zzcfg_runner():
    """

    :ref:`cdc_CIs_json`

    """
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    bps_cfg = d_cfg['blueprints']
    for id,rec in bps_cfg.iteritems():
        yield bp_test_delete_zzcfg,id,rec

@attr('bp_test_update_zzcfg')
def bp_test_update_zzcfg_runner():
    """

    :ref:`cdc_CIs_json`

    """
#    load_assets(asset_types=['BP'],asset_names=None,parent_ids=None,env='CDC_DEV',maxn=999)
    b = bps.get_asset('1431','BP')
    yield bp_test_update_zzcfg,b

@attr('bp_test_upload_zzcfg1')
def bp_test_upload_zzcfg1_runner():
    """

    :ref:`cdc_CIs_json`

    """
#    d_cfg = {u'blueprints': {u'1431': {u'BASELINE': [u'PRY_R3'], u'CID': 1, u'SCID': 2}, u'1432': {u'BASELINE': [u'PRY_R3'], u'CID': 1, u'SCID': 2}}}
    print '\n======================================================================================'
    print 'bp_test_upload_zzcfg1: update assetcfg (CIDs, SCIDs, etc)'
    print '======================================================================================'
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    bps_cfg = d_cfg['blueprints']
    for id,rec in bps_cfg.iteritems():
        yield bp_test_upload_zzcfg1,id,rec

################################################################################################################
def var_test1(fn):
    print '\n======================================================================================'
    print 'var_test1: %s'%fn
    print '======================================================================================'

    xml0 = get_xml(fn)
    xml0_copy = deepcopy(xml0)

    print_dvars(xml0)

    k,v = 'DUMMY_VAR','NEWVAL-%s'%timestamp2(datetime.now())
    print 'Update Var: %s >> %s'%(k,v)
    update_var(xml0,k,v)
    if xc(xml0_copy,xml0,reporter=sys.stderr.write,strip_whitespaces=True) == True:
        print 'NO UPDATE'
    else:
        print '**** ERROR: UPDATED ****'

    k,v = 'zzcfg','NEWVAL-%s'%timestamp2(datetime.now())
    print 'Update Var: %s >> %s'%(k,v)
    update_var(xml0,k,v)
    if xc(xml0_copy,xml0,reporter=sys.stderr.write,strip_whitespaces=True) == False:
        print 'UPDATED'
    else:
        print '**** ERROR: NO UPDATE ****'

    print_dvars(xml0)

def var_test2(fn):
    """
    Create new variable if doesn't exist
    >>> b = bps.get_asset('1431','BP')
    >>> b.get_xml()
    <Element {http://servicemesh.com/agility/api}Blueprint at 0x13133e60>
    """
    print '\n======================================================================================'
    print 'var_test2: %s'%fn
    print 'Creates new variable "z2"'
    print '======================================================================================'

    xml0 = get_xml(fn)
    xml0_copy = deepcopy(xml0)

    print_dvars(xml0)

    k,v = 'z2','z2_NEWVAL-%s'%timestamp2(datetime.now())
    print 'Create Var: %s'%k
    xml0 = get_xml(fn)
    xml0_copy = deepcopy(xml0)
    if not is_var(xml0,k):
        create_var(xml0,k,v)
    assert xc(xml0_copy,xml0,reporter=sys.stderr.write,strip_whitespaces=True) == False

    print_dvars(xml0)
    #pxml(xml0)

import base64
def var_test3(fn):
    """
    Create new cfg, read it back
    """
    print '\n======================================================================================'
    print 'var_test3: %s'%fn
    print '======================================================================================'

    xml0 = get_xml(fn)
    xml0_copy = deepcopy(xml0)

    print_dvars(xml0)

    new_config = AssetCfg({'CID':1,'SCID':2,'BASELINE':['PRY_R3']})

    k,v = 'zzcfg',new_config
    print 'Create Cfg: %s'%k
    xml0 = get_xml(fn)
    xml0_copy = deepcopy(xml0)
    if is_var(xml0,k):
        print '**** UPDATING %s **** '%k
        update_var(xml0,k,v.dumps())
    else:
        print '**** CREATING %s **** '%k
        create_var(xml0,k,v.dumps())
    assert xc(xml0_copy,xml0,reporter=sys.stderr.write,strip_whitespaces=True) == False

    print_dvars(xml0)

def update_create_var(xml0,k,v):
    if is_var(xml0,k):
        print '**** UPDATING %s **** '%k
        update_var(xml0,k,v)
    else:
        print '**** CREATING %s **** '%k
        create_var(xml0,k,v)

def bp_test_update_zzcfg(b):
    print '\n======================================================================================'
    print 'bp_test_update_zzcfg: %s'%b
    print 'requires connection to Agility, ie b instantiated using cfg read from settings.json'
    print '======================================================================================'

    xml0 = b.get_xml()
    xml0_copy = deepcopy(xml0)
    print_dvars(xml0)

    new_config = AssetCfg({'CID':1,'SCID':2,'BASELINE':['PRY_R3']})
    k,v = 'zzcfg',new_config
    update_create_var(xml0,k,v.dumps())

    resp = b.write_xml(xml0)
    print resp
    xml2 = b.get_xml()
    assert xc(xml0_copy,xml2,reporter=sys.stderr.write,strip_whitespaces=True) == False
    print_dvars(xml0)

def bp_test_delete_zzcfg(id,rec):
    """
    "blueprints": {
        "143a": {"CID":1,"SCID":2,"BASELINE":["PRY_R3"]},
        "1431": {"CID":"TEST","SCID":"TEST_SCID","BASELINE":["PRY_R3"]}
    }
    """
    
    logw = open('logw.txt','wt')
    print '-------------------------------------'
    print 'deleting for asset id: %s'%id
    b = bps.get_asset(id,'BP',isdebug=True)
    if not b is None:
        xml0 = b.get_xml()
        xml0_copy = deepcopy(xml0)
        print 'cid: %s'%b.short_id
        print_dvars(xml0)
        delete_var(xml0,'CID')
        delete_var(xml0,'SCID')

        resp = b.write_xml(xml0)
        print resp
        if resp.status_code == 200:
            b2 = bps.get_asset(id,'BP',isdebug=True)
            xml2 = b2.get_xml()
            print_dvars(xml2)
            assert xc(xml0_copy,xml2,reporter=sys.stderr.write,strip_whitespaces=True) == False
            print 'cid: %s'%b2.short_id
        else:
            logw.write('\ncid failed: %s, code: %i'%(b.short_id,resp.status_code))
    logw.close()

def bp_test_upload_zzcfg1(id,rec):
    """
    "blueprints": {
        "143a": {"CID":1,"SCID":2,"BASELINE":["PRY_R3"]},
        "1431": {"CID":"TEST","SCID":"TEST_SCID","BASELINE":["PRY_R3"]}
    }
    """
    
    logw = open('logw.txt','wt')
    print '-------------------------------------'
    print 'updating for asset id: %s'%id
    b = bps.get_asset(id,'BP',isdebug=True)
    if not b is None:
        xml0 = b.get_xml()
        xml0_copy = deepcopy(xml0)
        print 'cid: %s'%b.short_id
        update_create_var(xml0,'CID',rec['CID'])
        update_create_var(xml0,'SCID',rec['SCID'])

        resp = b.write_xml(xml0)
        print resp
        if resp.status_code == 200:
            b2 = bps.get_asset(id,'BP',isdebug=True)
            xml2 = b2.get_xml()
            assert xc(xml0_copy,xml2,reporter=sys.stderr.write,strip_whitespaces=True) == False
            print 'cid: %s'%b2.short_id
        else:
            logw.write('\ncid failed: %s, code: %i'%(b.short_id,resp.status_code))
    logw.close()


