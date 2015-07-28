from pprint import pprint as pp
import os
import default
import re
from datetime import datetime
import xlwt
import requests

def datestamp5():
    return datetime.now().strftime("%Y%m%dT%H%M")


def get_assets(asset_type='compute',env=default.ENV,maxn=default.MAXN):

    cfg = default.CFG[env]
    url,user,passwd = '%s/%s'%(cfg['BASE_URL'],asset_type),'%s'%cfg['USER'],'%s'%cfg['PASSWD']
    print url
    r = requests.get(url, auth=('%s'%user, '%s'%passwd),verify=False,headers={'accept':'application/json'})
    return r.json()['links']

def get_asset(id,asset_type='compute',env=default.ENV,maxn=default.MAXN):
    """
set([u'assetPath',
     u'assetProperties',
     u'assetType',
     u'cloud',
     u'createdOn',
     u'credentials',
     u'description',
     u'detailedAssetPath',
     u'environment',
     u'hostname',
     u'id',
     u'image',
     u'instanceId',
     u'lastStartedOrStoppedBy',
     u'lifecycleVersion',
     u'location',
     u'model',
     u'name',
     u'onboarded',
     u'privateAddress',
     u'publicAddress',
     u'removable',
     u'resources',
     u'scriptstatusLinks',
     u'stack',
     u'startTime',
     u'state',
     u'stopTime',
     u'template',
     u'top',
     u'uuid',
     u'variables',
     u'volumeStorages'])
    """

    cfg = default.CFG[env]
    url,user,passwd = '%s/%s/%s'%(cfg['BASE_URL'],asset_type,id),'%s'%cfg['USER'],'%s'%cfg['PASSWD']
    print url
    r = requests.get(url, auth=('%s'%user, '%s'%passwd),verify=False,headers={'accept':'application/json'})
    return r.json()


def rpt1():
    """
{
  "links" : [ {
    "name" : "Agility Appliance",
    "href" : "https://20.148.76.21:8443/agility/api/v3.2/compute/1",
    "id" : 1,
    "type" : "application/com.servicemesh.agility.api.Instance+xml",
    "position" : 0
  }, {
    """
    l = []
    assets = get_assets()
    [l.append(get_asset(j['id'])) for j in assets]
    #return l
    l2 = []
    for rec in l:
        d = {}
        d['name'],d['privateAddress'],d['state'],d['assetPath'] = rec['name'],rec['privateAddress'],rec['state'],re.sub(r'/Root/MWS2/','',rec['assetPath']) 
        if rec.has_key('lastStartedOrStoppedBy'):
            d['lastStartedOrStoppedBy'] = rec['lastStartedOrStoppedBy']
        else:
            d['lastStartedOrStoppedBy'] = 'n/a'
        if rec.has_key('environment'):
            d['env'] = rec['environment']['name']
        else:
            d['env'] = 'n/a'
        l3 = []
        if rec.has_key('resources'):
            for d2 in rec['resources']:
                if d2.has_key('resourceType') and d2['resourceType'] == 'DISK_DRIVE':
                    l3.append('DISK: %s (%s)'%(d2['name'],d2['quantity']))
            
        d['c_drive'] = '\n'.join(l3)
        l4 = []
        if rec.has_key('volumeStorages'):
            for d2 in rec['volumeStorages']:
                if d2.has_key('resourceType') and d2['resourceType'] == 'DISK_DRIVE':
                    l4.append('VOL: %s'%(d2['name'],))
        d['volumes'] = '\n'.join(l4)
        l2.append(d)
        
    wb2 = xlwt.Workbook(encoding='utf-8')

    style = xlwt.XFStyle()
    style.alignment.wrap = 1

    summ_sht = wb2.add_sheet('Compute')

    hdr = ('name','lastStartedOrStoppedBy','privateAddress','state','assetPath','c_drive','volumes') 

    nrow,ncol = 0,0
    for j in hdr:
        summ_sht.write(nrow,ncol,j)
        ncol += 1

    for rec in l2:
        ncol = 0
        nrow += 1
     #   for j in sorted(rec.keys()):
        for j in hdr:
            summ_sht.write(nrow,ncol,rec[j],style)
            ncol += 1

    summ_sht.col(0).width = 10500
    summ_sht.col(1).width = 3000
    summ_sht.col(2).width = 10500
    summ_sht.col(3).width = 10500
    summ_sht.col(4).width = 3000
    summ_sht.col(5).width = 18000

    dp_root = os.path.join('reports/compute',datestamp5())
    os.makedirs(dp_root)

    dp_rpt = os.path.join(dp_root,'compute.xls')
    print "Saving reports %s",(dp_rpt)
    wb2.save(dp_rpt)



