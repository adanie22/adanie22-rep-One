# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#

"""
test data:

../test_data
../test_data/export
../test_data/export/BP
../test_data/export/BP/MWS2P___Lync_Premium
../test_data/export/BP/MWS2P___Lync_Premium/envelope.xml
../test_data/export/BP/MWS2P___Shared_SQL
../test_data/export/BP/MWS2P___Shared_SQL/envelope.xml
../test_data/transformed
../test_data/transformed/BP

"""

import os,sys

from nose import with_setup
from nose.plugins.attrib import attr

from pprint import pprint as pp
import pickle
from xml_compare import xml_compare as xc

sys.path.append('..')
from cdc import blueprints as bps
from cdc import report as rpt
from cdc import etl
from cdc import default
from cdc.xml import *
from cdc.util import *

from copy import deepcopy

def load_bps():
    """
    load up set of blueprints from :ref:`cdc_CIs_json`. Processing on subsequent Blueprints, Packages, Scripts will 
    be determined by the loaded blueprints.

    :ref:`cdc_CIs_json`

    """
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    bps_cfg = d_cfg['blueprints']
    
    lids = []
    [lids.append(id) for id,rec in bps_cfg.iteritems() if 'R3' in rec['BASELINE']]
    [bps.get_asset(id,asset_type='BP',env='CDC_DEV',maxn=default.MAXN,isdebug=True) for id in lids]
    b = bps.get_assets('BP')
    return b

################################################################################################################
# FIXTURES
################################################################################################################
@attr('bp_export_scrpt1')
def export_scrpt1_test_runner():
    """
    export scripts to target directory, tdp
    """
    b = load_bps()
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)

    tstamp = datestamp5()
    export_rootdp = '../export/%s'%tstamp
    exportAllZip = True
    yield export_asset1,tstamp,'SCRPT',l_wkld_pkgs,export_rootdp,exportAllZip

@attr('bp_export_pkg1')
def export_pkg1_test_runner():
    """
    export packages to target directory, export_rootdp
    """
    b = load_bps()
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)

    tstamp = datestamp5()
    export_rootdp = '../export/%s'%tstamp
    exportAllZip = True
    yield export_asset1,tstamp,'PKG',l_wkld_pkgs,export_rootdp,exportAllZip

@attr('bp_export_bp1')
def export_bp1_test_runner():
    """
    export blueprints to target directory, export_rootdp
    """
    b = load_bps()
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)

    tstamp = datestamp5()
    export_rootdp = '../export/%s'%tstamp
    exportAllZip = True
    yield export_asset1,tstamp,'BP',l_wkld_pkgs,export_rootdp,exportAllZip

@attr('bp_export_R3')
def export_R3_test_runner():
    """
    export blueprints,packages,scripts to target directory, export_rootdp
    """
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    bps_cfg = d_cfg['blueprints']
    
    ######### differs from xls_baseline_R3 above ###########################
    # will only load into the cache selected assets 
    lids = []
    [lids.append(id) for id,rec in bps_cfg.iteritems() if 'R3' in rec['BASELINE']]
    [bps.get_asset(id,asset_type='BP',env='CDC_DEV',maxn=default.MAXN,isdebug=True) for id in lids]
    #######################################################################
    b = bps.get_assets('BP',asset_ids=lids)
    #b = load_bps()
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)

    tstamp = datestamp5()
    export_rootdp = '../export/%s'%tstamp
    exportAllZip = False

    yield export_asset1,tstamp,'SCRPT',l_wkld_pkgs,export_rootdp,exportAllZip
    yield export_asset1,tstamp,'PKG',l_wkld_pkgs,export_rootdp,exportAllZip
    yield export_asset1,tstamp,'BP',l_wkld_pkgs,export_rootdp,exportAllZip

@attr('bp_export_R3_all')
def export_cdcCIs2_test_runner():
    """
    as per export_cdcCIs1 :func:`export_cdcCIs1_test_runner`, but will export all files, eg signature.xml and anything else. Useful to 
    for one-time check that export_asset1 does not miss some export/zip files
    """
    
    d_cfg = json.loads(open('cdc_CIscfg.json','rt').read())
    bps_cfg = d_cfg['blueprints']
    
    ######### differs from xls_baseline_R3 above ###########################
    # will only load into the cache selected assets 
    lids = []
    [lids.append(id) for id,rec in bps_cfg.iteritems() if 'R3' in rec['BASELINE']]
    [bps.get_asset(id,asset_type='BP',env='CDC_DEV',maxn=default.MAXN,isdebug=True) for id in lids]
    #######################################################################
    b = bps.get_assets('BP',asset_ids=lids)
    #b = load_bps()
    l_wkld_pkgs = rpt.rpt_wkld_pkgs(b)

    tstamp = datestamp5()
    export_rootdp = '../export/%s'%tstamp
    exportAllZip = True

    yield export_asset1,tstamp,'SCRPT',l_wkld_pkgs,export_rootdp,exportAllZip
    yield export_asset1,tstamp,'PKG',l_wkld_pkgs,export_rootdp,exportAllZip
    yield export_asset1,tstamp,'BP',l_wkld_pkgs,export_rootdp,exportAllZip

@attr('bp_transform_assetpath1')
def bp_transform_assetpath1_test_runner():
    """
    transforms asset path xml, validates it
    """
    fp = '../test_data/export/BP/MWS2P___Lync_Premium/envelope.xml'
    yield bp_transform_assetpath1,fp

@attr('import_assets1')
def import_assets1_test():
    """
    """
    sdp_root = '../export/20150512T1653'
    tdp_root = '../import'
    env = 'CDC_DEV'
    l_short_ids = None
    etl.import_assets1(sdp_root,tdp_root,env,l_short_ids,prj_cntainr_id='9',env_cntainr_id='217')#,environment)

################################################################################################################

def export_asset1(tstamp,asset_type,l_wkld_pkgs,export_rootdp,exportAllZip):
    """
    exports all assets; read items (ids) in from cdc_CIscfg.json, where baseline == "R3"

    """
    print '\n======================================================================================'
    print 'export_asset1, reads items in from cdc_CIscfg.json, where baseline == "R3"'
    print '======================================================================================'

    """
    eg. d_assets: {'SCRPT-129': {'p_details': [(BP-1150, WKLD-1151, PKG-32)],'s_details': (SCRPT-129, 'I')},
    """
    if asset_type == 'BP':
        d_assets = rpt.rpt_bp_bps(l_wkld_pkgs)
    elif asset_type == 'PKG':
        d_assets = rpt.rpt_bp_pkgs(l_wkld_pkgs)
    elif asset_type == 'SCRPT':
        d_assets = rpt.rpt_bp_scrpts(l_wkld_pkgs)

    for asset_id in d_assets.keys():
        tdp = os.path.join(export_rootdp,'%s-%s'%(asset_type,str(asset_id)))
        etl.export_asset(asset_id,tstamp,asset_type=asset_type,asset_obj=d_assets[asset_id]['asset_details'][0],tdp=tdp,env='CDC_DEV',isdebug=False,export_all=exportAllZip)
    

def bp_transform_assetpath1(fp):
    print '\n======================================================================================'
    print 'bp_transform_assetpath1: %s'%fp
    print '======================================================================================'

    #xml.update_assetpath(x,k,v,ns={'ns1':'http://servicemesh.com/agility/api'}):

    fp = 'c:/mws_r3/export/20150511T0025/BP/BP/Lync_Test_DA1/envelope.xml'
    xml0 = get_xml(fp)

    #pxml(xml0)

    xml0_copy = deepcopy(xml0)

    update_assetpath(xml0,'k','v',ns={'ns1':'http://servicemesh.com/agility/api'})

    if xc(xml0_copy,xml0,reporter=sys.stderr.write,strip_whitespaces=True) == False:
        print 'UPDATED'
    else:
        print '**** ERROR: NO UPDATE ****'
    
    fp = open('c:/mws_r3/transformed/envelope.xml','wt')
    fp.write(et.tostring(xml0))

    """
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
    """
