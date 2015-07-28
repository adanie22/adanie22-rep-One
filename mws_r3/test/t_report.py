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
import pickle
from xml_compare import xml_compare as xc

sys.path.append('..')
from cdc import blueprints as bps
from cdc import report as rpt
from cdc import default
from cdc.xml import *
from cdc.util import *

ns={'ns1':'http://servicemesh.com/agility/api'}

################################################################################################################
# FIXTURES
################################################################################################################
@attr('list_assets_project1')
def list_assets_projects_test_runner():
    """
    """
    rpt.list_assets(asset_type='project',env='CDC_DEV') 

@attr('list_assets_env1')
def list_assets_envs_test_runner():
    """
    """
    rpt.list_assets(asset_type='environment',env='CDC_DEV') 

@attr('list_compute1')
def list_compute1_test_runner():
    """
    """
    l = rpt.list_compute(asset_type='compute',env='CDC_DEV') 
    rpt.create_xls_compute(l)

@attr('CDC')
@attr('xls_baseline_all')
def xls_baseline_all_test_runner():
    """
    creates baseline/xls report on all blueprints in the Agility appliance

    :ref:`cdc_CIs_json`

    """
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    #bps_cfg = d_cfg['blueprints']
    bps_cfg = d_cfg['CDC']['blueprints']

    bps.load_assets(asset_types=['BP'],asset_names=None,maxn=999)
    b = bps.get_assets('BP')
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)
    l_wkld_pkg_pvars = rpt.rpt_wkld_pkg_pvars(l_wkld_pkgs)

    d_pkgs = rpt.rpt_bp_pkgs(l_wkld_pkgs)
    d_scrpts = rpt.rpt_bp_scrpts(l_wkld_pkgs)

    yield xls_baseline,l_wkld_pkg_pvars,d_pkgs,d_scrpts,bps_cfg


@attr('PDC')
@attr('xls_baseline_all')
def xls_baseline_all_test_runner():
    """
    creates baseline/xls report on all blueprints in the Agility appliance

    :ref:`cdc_CIs_json`

    """
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    bps_cfg = d_cfg['PDC']['blueprints']

    bps.load_assets(asset_types=['BP'],asset_names=None,maxn=999)
    b = bps.get_assets('BP')
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)
    l_wkld_pkg_pvars = rpt.rpt_wkld_pkg_pvars(l_wkld_pkgs)

    d_pkgs = rpt.rpt_bp_pkgs(l_wkld_pkgs)
    d_scrpts = rpt.rpt_bp_scrpts(l_wkld_pkgs)

    yield xls_baseline,l_wkld_pkg_pvars,d_pkgs,d_scrpts,bps_cfg

@attr('xls_baseline_R3')
def xls_baseline_R3_test_runner():
    """
    creates baselins/xls report on (selected) blueprint ids read from json config file

    :ref:`cdc_CIs_json`

    """
    print '\n======================================================================================'
    print 'xls_baseline_test_runner'
    print '======================================================================================'
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    bps_cfg = d_cfg['blueprints']
    
    ######### differs from xls_baseline_R3 above ###########################
    # will only load into the cache selected assets 
    lids = []
    [lids.append(id) for id,rec in bps_cfg.iteritems() if 'R3' in rec['BASELINE']]
    [bps.get_asset(id,asset_type='BP',env='CDC_DEV',maxn=default.MAXN,isdebug=True) for id in lids]
    #######################################################################
    b = bps.get_assets('BP',asset_ids=lids)
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)
    l_wkld_pkg_pvars = rpt.rpt_wkld_pkg_pvars(l_wkld_pkgs)
    d_pkgs = rpt.rpt_bp_pkgs(l_wkld_pkgs)
    d_scrpts = rpt.rpt_bp_scrpts(l_wkld_pkgs)
    yield xls_baseline,l_wkld_pkg_pvars,d_pkgs,d_scrpts,bps_cfg

@attr('xls_baseline_X')
def xls_baseline_X_test_runner():
    print '\n======================================================================================'
    print 'xls_baseline_X_test_runner'
    print '======================================================================================'
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    bps_cfg = d_cfg['blueprintsX']
    
    ######### differs from xls_baseline_R3 above ###########################
    # will only load into the cache selected assets 
    lids = []
    [lids.append(id) for id,rec in bps_cfg.iteritems() if 'R3' in rec['BASELINE']]
    [bps.get_asset(id,asset_type='BP',env='CDC_DEV',maxn=default.MAXN,isdebug=True) for id in lids]
    #######################################################################
    b = bps.get_assets('BP',asset_ids=lids)
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)
    l_wkld_pkg_pvars = rpt.rpt_wkld_pkg_pvars(l_wkld_pkgs)
    d_pkgs = rpt.rpt_bp_pkgs(l_wkld_pkgs)
    d_scrpts = rpt.rpt_bp_scrpts(l_wkld_pkgs)
    yield xls_baseline,l_wkld_pkg_pvars,d_pkgs,d_scrpts,bps_cfg

################################################################################################################
def xls_baseline(l4,d_pkgs,d_scrpts,bps_cfg):
    print '\n======================================================================================'
    print 'xls_baseline'
    print '======================================================================================'

    fp = rpt.create_xls(l4,d_pkgs,d_scrpts,bps_cfg)
    print 'report created: %s'%fp

