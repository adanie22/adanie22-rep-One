# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#

"""
"Export", Transform and Load

Designed for batch operations, currently run from test routines in :ref:`test_etl`.

"""
from pprint import pprint as pp
import os
import default
import re
from datetime import datetime
from itertools import groupby
from util import *
import StringIO
import xls
import io
import requests
from lxml import etree as et


import zipfile
#try:
#    from cStringIO import StringIO
#except ImportError:
#    from io import BytesIO as StringIO


class InMemoryZip(object):
    def __init__(self):
        # Create the in-memory file-like object
        self.in_memory_data = io.BytesIO() #StringIO()
        # Create the in-memory zipfile
        self.in_memory_zip = zipfile.ZipFile(
            self.in_memory_data, "w", zipfile.ZIP_DEFLATED, False)
        self.in_memory_zip.debug = 3

    def append(self, filename_in_zip, file_contents):
        '''Appends a file with name filename_in_zip and contents of
        file_contents to the in-memory zip.'''
        self.in_memory_zip.writestr(filename_in_zip, file_contents)
        return self   # so you can daisy-chain

    def writetofile(self, filename):
        '''Writes the in-memory zip to a file.'''
        # Mark the files as having been created on Windows so that
        # Unix permissions are not inferred as 0000
        for zfile in self.in_memory_zip.filelist:
            zfile.create_system = 0
        self.in_memory_zip.close()
        with open(filename, 'wb') as f:
            f.write(self.in_memory_data.getvalue())
#ie, curl:
#curl -u <USER>:<PASSWD> -k -X GET https://10.0.0.158:8443/agility/api/v3.2/container/export > aws_containertree.zip


#etl.export_asset(k,tstamp,asset_type=asset_type,asset_obj=d_assets[k]['asset_details'][0],tdp='../export',tmpdir='../tmp',env='CDC_DEV',isdebug=False)
def export_asset(id,tstamp,asset_type,asset_obj,tdp='../export',env=default.ENV,isdebug=False,export_all=False):
    """
    exports asset by id; rest returns .zip format. extract .zip contents, envelope.xml, into tmp folder first

    query parameters taken from Roffe/Betancur::

        $HOST/agility/api/current/blueprint/${BPIDS[$WHICH]}/export?recursive=false&deep=false

    if export_all = True, unzip all files, not just envelope.xml

    see :func:`test.test_etl.export_all1_test_runner`

    refer SericeMesh code for REST export query parameters :ref:`blueprint_cleric`

    """
    if not os.path.exists(tdp):
        os.makedirs(tdp)

    cfg = default.CFG[env]
    url,user,passwd = '%s/%s/%s/export?recursive=false&deep=false'%(cfg['BASE_URL'],a_path(asset_type),id),'%s'%cfg['USER'],'%s'%cfg['PASSWD']
    r = requests.get(url, auth=('%s'%user, '%s'%passwd),verify=False)

    if r.status_code == 200:
        #export_dp = os.path.join(tdp,normalise_assetname(asset_obj.name))
        export_dp = tdp
        print 'downloading zip %s (%s): %s to %s'%(asset_obj.name,id,url,export_dp)
        z = zipfile.ZipFile(StringIO.StringIO(r.content))
        if export_all == True:
            z.extractall(export_dp)
        else:
            z.extractall(export_dp,['envelope.xml'])

    else:
        print 'problem downloading zip %s (%s): %s'%(asset_obj.name,id,url)

def get_asset_item(dp_root,fi):
    """
        given folder item (fi) "BP-1431", return as short_id, asset_type, id:
            ie, BP_1431, BP, 1431

        folder item must follow pattern [<BP>|<SCRPT>|<PKG>]-nnnn and contain "envelope.xml" within it

        if not, return None
    """
    try:
        if (len(re.match('(BP|SCRPT|PKG)-([0-9]+$)',fi).groups()) == 2) and \
            os.path.isfile(os.path.join(dp_root,fi,'envelope.xml')):
                return re.match('(BP|SCRPT|PKG)-([0-9]+$)',fi).groups()
        else:
            return None
    except AttributeError as e:
        return None

def import_assets1(dp_root,tdp_root,env,l_short_ids,prj_cntainr_id,env_cntainr_id=None):
    """
    copy exported assets (exploded zip) into import folder. 
    import into agility project and optionally, an environment container

    import algorithm
    ----------------

     Given below folder organisation; 

    [dhatchett@localhost test]$ find ../export/
    ../export/
    ../export/20150512T1653
    ../export/20150512T1653/BP-1431
    ../export/20150512T1653/BP-1431/envelope.xml
    ../export/20150512T1653/SCRPT-143
    ../export/20150512T1653/SCRPT-143/envelope.xml
    ../export/20150512T1653/PKG-33
    ../export/20150512T1653/PKG-33/envelope.xml

    For each asset in folder root dp_root (../export/20150512T1653):
        if l_short_ids is None
            import all  
        else 
            if asset_id in l_short_ids:
                import asset_id
    
    """

    def build_zip_package(import_dp,fp,short_id,asset_type,id):
        try:
            tfp_zip = os.path.join(import_dp,'%s.zip'%short_id)
            instr = open(os.path.join(fp,'envelope.xml'),'rb').read()
            imz = InMemoryZip()
            imz.append("envelope.xml", instr)
            imz.writetofile(tfp_zip)
            print 'zip created: %s'%tfp_zip
            return True
        except Exception as e:
            print e
            return False

    # create zip for import
    ts = datestamp5()
    #tdp = os.path.join(tdp_root,'../import',ts)
    tdp = os.path.join(tdp_root,ts)
    os.makedirs(tdp)

    print 'creating zip files for import'
    print 'source export folder: %s'%dp_root
    print 'target import folder: %s'%tdp
    cfg = default.CFG[env]
    try:
        for fi in os.listdir(dp_root):
            fp = os.path.join(dp_root,fi)
            if os.path.isdir(fp):
                asset_item = get_asset_item(dp_root,fi)
                short_id = fi
                if not asset_item is None:
                    asset_type,asset_id = asset_item[0],asset_item[1]
                    if l_short_ids is None or short_id in l_short_ids:
                        print 'building zipfile for %s'%short_id
                        if not build_zip_package(tdp,fp,short_id,asset_type,id):
                            print '**** problem building zip %s ****'%short_id
                else:
                    print 'skipping %s'%fi
    except OSError as e:
        print e
        return

    print 'importing assets'
    for fi in os.listdir(tdp):
        fp = os.path.join(tdp,fi)
        print fp
        
        if env_cntainr_id is None:
            import_id_str = '%s'%prj_cntainr_id
        else:
            import_id_str = 'environment/%s'%env_cntainr_id

        url,user,passwd = '%s/%s/import'%(cfg['BASE_URL'],import_id_str),'%s'%cfg['USER'],'%s'%cfg['PASSWD']
        d = open(fp,'rb').read()
        r = requests.put(url, data=d, auth=('%s'%user, '%s'%passwd),headers={'content-type':'application/x-zip'},verify=False)

        if r.status_code == 200:
            print 'asset %s imported into %s'%(fi,import_id_str)
        else:
            print '**** asset %s NOT imported into %s ****'%(fi,import_id_str)
            
#../cdc/xml.py:#curl --connect-timeout 3 -s -k -u %s:Ag1l1ty23 -X PUT --data-binary @"import.zip" https://20.14
#8.76.21:8443/agility/api/current/project/9/import --header "Content-Type:application/x-zip"
##    
##[dhatchett@localhost 20150511T1518]$ curl -u dhatchett-admin:Ag1l1ty23 -k -X  PUT --data-binary @"xx.zip" https://20.148.76.21:8443/agility/api/v3.2/environment/164/import --header "Content-Type:application/x-zip"
#
#
