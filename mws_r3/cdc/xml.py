# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#

import default
from pprint import pprint as pp
from lxml import etree as et

def get_xml(fn):
    """
    used in test routines :ref:`test_blueprints`, :ref:`test_etl`
    """
    f = open(fn,'rt')
    s_xml = f.read()
    f.close()
    x = et.fromstring(s_xml)
    return x

def build_var_xml(VAR_NAME,VAR_VALUE,VAR_DESC=''):
    return """
    <ns1:variable xmlns:ns1="http://servicemesh.com/agility/api">
       <ns1:name>%s</ns1:name>
       <ns1:description>%s</ns1:description>
       <ns1:top>false</ns1:top>
       <ns1:displayName>%s</ns1:displayName>
       <ns1:readable>true</ns1:readable>
       <ns1:writable>false</ns1:writable>
       <ns1:minRequired>1</ns1:minRequired>
       <ns1:maxAllowed>1</ns1:maxAllowed>
       <ns1:defaultValue>
         <ns1:name/>
         <ns1:encrypted>false</ns1:encrypted>
         <ns1:overridable>true</ns1:overridable>
         <ns1:propertyType>
           <ns1:name>string-any</ns1:name>
           <ns1:href>/propertytype/4</ns1:href>
           <ns1:id>4</ns1:id>
           <ns1:rel>up</ns1:rel>
           <ns1:type>application/com.servicemesh.agility.api.PropertyType+xml</ns1:type>
           <ns1:position>0</ns1:position>
         </ns1:propertyType>
        <ns1:stringValue>%s</ns1:stringValue>
       </ns1:defaultValue>
       <ns1:stringValue>%s</ns1:stringValue>
       <ns1:propertyType>
         <ns1:name>string-any</ns1:name>
         <ns1:href>propertytype/4</ns1:href>
         <ns1:id>4</ns1:id>
         <ns1:type>application/com.servicemesh.agility.api.PropertyType+xml</ns1:type>
       </ns1:propertyType>
     </ns1:variable>
    """%(VAR_NAME,VAR_NAME,VAR_NAME,VAR_VALUE,VAR_VALUE)

def pxml(e):
    """
    pretty print lxml element tree
    """
    print et.tostring(e,pretty_print=True)

def print_vars(lx,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    pretty print lxml elements in list lx
    """
    for e in lx:
        print '%s'%e
        print et.tostring(e,pretty_print=True)

def print_dvars(x):
    lxvars = get_lxvars(x)
    f1 = lambda e0:','.join([(re.sub('{.*}','',i.tag),i.text) for i in e0.iterchildren()])
    print 'Variables:'
    [pp(f1(i)) for i in lxvars]

############################################################################################################
    
def update_container_xml(xml0,newcontainer):

    fxml = os.path.join(sdp,bp,fn)
    d = et.parse(open(fxml,'rt'))
    r = d.getroot()
    print sdp,bp,fn
    return r
    e_asset = r.find('.//{http://servicemesh.com/agility/api}Asset')
    try:
        print e_asset,e_asset.find('.//{http://servicemesh.com/agility/api}parent')
        idx = e_asset.index(e_asset.find('.//{http://servicemesh.com/agility/api}parent'))

        print idx
        """
        e_asset.remove(e_asset.find('.//{http://servicemesh.com/agility/api}parent'))
        e_new = et.Element('ABC')
        e_asset.insert(idx,e_new)
        fxml2 = os.path.join(tdp,bp,fn)
        f2 = open(fxml2,'wt')
        print 'write %s'%fxml2
        f2.write(et.tostring(r))
        #pp(et.tostring(r))
        f2.close()
        """
    except TypeError:
        print 'could not write %s'%os.path.join(newDir,fn)

def update_resource_xml(self,e0,r_var_label,r_value,r_type,isReplace=False):
    hdrs={'content-type':'application/xml'}
    try:

        for e2_p in e0.findall('./{http://servicemesh.com/agility/api}anyOrderItem'):

            e_new_xml = et.fromstring(resource_xml(r_var_label,r_value,r_type))

            e2_rp = get_resource_parent(e2_p,r_var_label)
            if not e2_rp is None:
                if isReplace:
                    e2_p.replace(e2_rp,e_new_xml)
            else:
                    e2_p.append(e_new_xml)

        return e0
        #resp = requests.put(self.url, data=et.tostring(e0),auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)
        #return resp

    except AttributeError:

        print 'unable to update ram, cpu for %s'%self.url
        return None

    hdrs={'content-type':'application/xml'}
    e0 = self.get_xml()
    e0 = update_resource_xml(e0,'Min CPU',2,'Processor')
    e0 = update_resource_xml(e0,'Min GB RAM',2,'Memory')
    resp = requests.put(self.url, data=et.tostring(e0),auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)
    return resp

############################################################################################################

def get_lxvars(x,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    x = <blueprint> elementTree
    return all children variable elements of <blueprint> parent
    """
    return x.findall('.//ns1:variable',ns)

def is_var(x,k,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    x = <blueprint> elementTree
    return true if variable exists
    """
    lxvars = x.findall('./ns1:variable/ns1:name',ns)
    if (len(lxvars) > 0) and (k in [i.text for i in lxvars]):
        return True
    else:
        return False

def get_var(x,k,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    x = <blueprint> elementTree
    return variable element if variable <name> exists
    """
    lxvars = x.findall('./ns1:variable/ns1:name',ns)
    if (len(lxvars) > 0) and (k in [i.text for i in lxvars]):
        return [i.getparent() for i in lxvars if i.text == k][0]
    else:
        return False

def delete_var(x,k,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    x = <blueprint> elementTree
    if variable name k exists as a child of the <blueprint> elementtree, delete the child 
    """
    if is_var(x,k):
        e = get_var(x,k)
        x.remove(e)

def update_assetpath(x,k,v,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    x = <blueprint>...
    if variable name k exists as a child of the <blueprint> elementtree, update child text << v


    <ns1:assetPath>
    <ns1:detailedAssetPath>

    """

    l_assetpaths = x.xpath('.//ns1:detailedAssetPath',namespaces=ns)
    for i in l_assetpaths:
        print i
        i.getparent().remove(i)
    return x

def update_var(x,k,v,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    x = <blueprint>...
    if variable name k exists as a child of the <blueprint> elementtree, update child text << v
    """
    if is_var(x,k):
        e = get_var(x,k)
        e.find('./ns1:stringValue',ns).text = v
#        e_val0 = x.find('./ns1:variable/ns1:stringValue',ns)
#        e_val0.text = v

def create_var(x,k,v,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    x = <blueprint>...
    if variable name k does not exist as a child of the <blueprint> elementtree, create child with name, value: k,v respectively
    """
    if not is_var(x,k):
        e_str = build_var_xml(k,v,'%s_%s'%(k,v))
        x.append(et.fromstring(e_str))    

def get_dvars(lx,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
    lx = list of <variable> elements
    return dictionaries of variables given a list of variable elements (will have been derived from a blueprint parent)
    """
    d = {}
    for e in lx:
        name = e.xpath('./ns1:name',namespaces=ns)[0].text
        ptype = e.xpath('./ns1:propertyType/ns1:name',namespaces=ns)[0].text
        if ptype == 'string-any':
            val = e.xpath('./ns1:stringValue',namespaces=ns)[0].text
        else:
            val = ''
        d.setdefault(name,(ptype,val))
    return d

def print_xml_vars(x,ns={'ns1':'http://servicemesh.com/agility/api'}):
    """
        x = <blueprint>...
        <ns1:Blueprint xmlns:ns1="http://servicemesh.com/agility/api">
          <ns1:name>Damon test1</ns1:name>
          <ns1:variable>
            <ns1:id>14396</ns1:id>
            <ns1:name>zzcfg</ns1:name>
            ...
          </ns1:variable>
          <ns1:variable>
            ...
          </ns1:variable>
        </ns1:Blueprint>

    """
    for e in x.findall('.//ns1:variable',ns):
        print '%s'%e
        print et.tostring(e,pretty_print=True)

#curl -m 6000 -s -k -o sandpit-test1.zip -u dhatchett-admin:Ag1l1ty23 -X GET https://20.148.76.21:8443/agility/api/current/blueprint/385/export?recursive=false&deep=false
#curl -m 6000 -s -k -o sandpit-test1.zip -u dhatchett-admin:Ag1l1ty23 -X GET https://20.148.76.21:8443/agility/api/current/blueprint/385/export?recursive=false&deep=false


#curl -u dhatchett-admin:Ag1l1ty23 -k -X GET     https://20.148.76.21:8443/agility/api/v3.2/blueprint/171 -H "accept:application/json"

def ram_xml(RAM):
    return """
    <ns1:resources xmlns:ns1="http://servicemesh.com/agility/api">
      <ns1:name>Min GB RAM</ns1:name>
      <ns1:top>false</ns1:top>
      <ns1:assetPath/>
      <ns1:detailedAssetPath/>
      <ns1:applicationType>application/com.servicemesh.agility.api.Resource+xml</ns1:applicationType>
      <ns1:lifecycleVersion>-1</ns1:lifecycleVersion>
      <ns1:removable>true</ns1:removable>
      <ns1:resourceType>Memory</ns1:resourceType>
      <ns1:quantity>%s</ns1:quantity>
    </ns1:resources>
    """%(RAM,)

def resource_xml(r_var_label,r_value,r_type='Memory'):
    """
        r_type = Memory|Processor
    """
    return """
    <ns1:resources xmlns:ns1="http://servicemesh.com/agility/api">
      <ns1:name>%s</ns1:name>
      <ns1:top>false</ns1:top>
      <ns1:assetPath/>
      <ns1:detailedAssetPath/>
      <ns1:applicationType>application/com.servicemesh.agility.api.Resource+xml</ns1:applicationType>
      <ns1:lifecycleVersion>-1</ns1:lifecycleVersion>
      <ns1:removable>true</ns1:removable>
      <ns1:resourceType>%s</ns1:resourceType>
      <ns1:quantity>%s</ns1:quantity>
    </ns1:resources>
    """%(r_var_label,r_type,r_value)

def cpu_xml(CPU):
    return """
    <ns1:resources xmlns:ns1="http://servicemesh.com/agility/api">
      <ns1:name>Min CPU</ns1:name>
      <ns1:top>false</ns1:top>
      <ns1:assetPath/>
      <ns1:detailedAssetPath/>
      <ns1:applicationType>application/com.servicemesh.agility.api.Resource+xml</ns1:applicationType>
      <ns1:lifecycleVersion>-1</ns1:lifecycleVersion>
      <ns1:removable>true</ns1:removable>
      <ns1:resourceType>Processor</ns1:resourceType>
      <ns1:quantity>%s</ns1:quantity>
    </ns1:resources>
    """%(CPU,)

def var_xml(VAR,VAL):
    return """
   <ns1:variable>
     <ns1:name>%s</ns1:name>
     <ns1:encrypted>false</ns1:encrypted>
     <ns1:overridable>true</ns1:overridable>
     <ns1:propertyType>
       <ns1:name>string-any</ns1:name>
       <ns1:href>/propertytype/4</ns1:href>
       <ns1:rel>up</ns1:rel>
       <ns1:type>application/com.servicemesh.agility.api.PropertyType+xml</ns1:type>
       <ns1:position>0</ns1:position>
     </ns1:propertyType>
     <ns1:stringValue>%s</ns1:stringValue>
     <ns1:booleanValue>false</ns1:booleanValue>
   </ns1:variable>
    """%(VAR,VAL)
def new_parent(x='ADSANDPIT',y='envronment/57',z='57'):
    return et.fromstring("""
        <parent>
            <name>%s</name>
            <href>%s</href>
            <id>%s</id>
            <rel>up</rel>
            <type>application/com.servicemesh.agility.api.Environment+xml</type>
            <position>0</position>
        </parent>
"""%(x,y,z))

import urllib2, base64
import json

def get_resource_parent(e2_p,text):
    e = [i.getparent() for i in e2_p.findall('ns1:resources/ns1:name',e2_p.nsmap) if i.text == text]
    if len(e) > 0:
        return e[0]
    else:
        return None

def get_project():
	url = '%s/project'%BASE_URL
	request = urllib2.Request(url)
	base64string = base64.encodestring('%s:%s' % ('%s'%USER, '%s'%PASSWD)).replace('\n', '')
	request.add_header("Authorization", "Basic %s" % base64string)   
	request.add_header("accept", "application/json")
	result = urllib2.urlopen(request)
	return json.read(result.read())

def get_blueprint1():
	url = '%s/blueprint'%BASE_URL
	request = urllib2.Request(url)
	base64string = base64.encodestring('%s:%s' % ('%s'%USER, '%s'%PASSWD)).replace('\n', '')
	request.add_header("Authorization", "Basic %s" % base64string)   
	request.add_header("accept", "application/json")
	result = urllib2.urlopen(request)
	return json.read(result.read())

import os,zipfile
import zipfile,os.path
from pprint import pprint as pp
import shutil
def _mkdir(dp):
    if os.path.exists(dp):
        shutil.rmtree(dp)
        os.mkdir(dp)
    else:
        os.mkdir(dp)
        
def unzip(zipFilePath, tdp):
    zfile = zipfile.ZipFile(zipFilePath)
    for name in zfile.namelist():
        (dirName, fileName) = os.path.split(name)
        #    print dirName, fileName
        newDir = tdp + '/' + os.path.basename(zipFilePath).split('.')[0]
        _mkdir(newDir)
        if not name == 'signature':
            fd = open(newDir + '/' + name, 'wb')
            fd.write(zfile.read(name))
            fd.close()
    zfile.close()

import report
import requests
def p1a():
#	request = urllib2.Request("%s/385/export?recursive=false&deep=false")%BASE_URL
    request = urllib2.Request("%s/blueprint/171")%BASE_URL
    base64string = base64.encodestring('%s:%s' % ('%s'%USER, '%s'%PASSWD)).replace('\n', '')
    request.add_header("Authorization", "Basic %s" % base64string)   
    result = urllib2.urlopen(request)
    return result

import re,StringIO
f_fn = lambda x:re.sub('\W','_',os.path.basename(x).split('.')[0])

def p1b(tdp='import/p0',hdrs={'accept':'application/json'}):
    """
    OLD, see cdc.etl
    """
    if not os.path.exists(tdp):
        os.mkdir(tdp)
    bps = report.Blueprints()
#    pp(bps.get_bps(fldr='ADSANDPIT (sandpit.local)'))
    for id,name,workloads in bps.get_bps(fldr='ADSANDPIT (sandpit.local)'):
        url = '%s/blueprint/%s/export?recursive=false&deep=false'%(BASE_URL,id)
        r = requests.get(url, auth=('%s'%USER, '%s'%PASSWD),verify=False)
        if r.status_code == 200:
            tfp = os.path.join(tdp,f_fn(name))
            print 'downloading zip %s (%s): %s to %s'%(name,id,url,tfp)
            z = zipfile.ZipFile(StringIO.StringIO(r.content))
            z.extractall(tfp,['envelope.xml'])
        else:
            print 'problem downloading zip %s (%s): %s'%(name,id,url)

def p2a(sdp='import/p0',tdp='import/p2',newxml='',ns='{http://servicemesh.com/agility/api}'):
    """
    OLD, see cdc.etl
    """
    for bp in os.listdir(sdp):
        fi = os.path.join(sdp,bp)
        if os.path.isdir(fi):
            tp = os.path.join(tdp,os.path.basename(fi))
            for fn in os.listdir(fi):
                if fn == 'envelope.xml':
                    yield (sdp,bp,fn)#,t.getroot())

from pprint import pprint as pp
def p2b(sdp,bp,fn,tdp='import/p2'):
    """
    OLD, see cdc.etl
    """
    if not os.path.exists(tdp):
        os.mkdir(tdp)

    newDir = os.path.join(tdp,bp)
    _mkdir(newDir)
    
    fxml = os.path.join(sdp,bp,fn)
    d = et.parse(open(fxml,'rt'))
    r = d.getroot()
    print sdp,bp,fn
    return r
    e_asset = r.find('.//{http://servicemesh.com/agility/api}Asset')
    try:
        print e_asset,e_asset.find('.//{http://servicemesh.com/agility/api}parent')
        idx = e_asset.index(e_asset.find('.//{http://servicemesh.com/agility/api}parent'))

        print idx
        """
        e_asset.remove(e_asset.find('.//{http://servicemesh.com/agility/api}parent'))
        e_new = et.Element('ABC')
        e_asset.insert(idx,e_new)
        fxml2 = os.path.join(tdp,bp,fn)
        f2 = open(fxml2,'wt')
        print 'write %s'%fxml2
        f2.write(et.tostring(r))
        #pp(et.tostring(r))
        f2.close()
        """
    except TypeError:
        print 'could not write %s'%os.path.join(newDir,fn)

def p2c():
#curl --connect-timeout 3 -s -k -u %s:Ag1l1ty23 -X PUT --data-binary @"import.zip" https://20.148.76.21:8443/agility/api/current/project/9/import --header "Content-Type:application/x-zip"
	url = '%USER%s/project/9/import'%BASE_URL
	request = urllib2.Request(url)
	base64string = base64.encodestring('%s:%s' % ('dhatchett-admin', '%PASSWD'%s)).replace('\n', '')
	request.add_header("Authorization", "Basic %s" % base64string)   
	request.add_header("accept", "application/json")
	result = urllib2.urlopen(request)
	return json.read(result.read())


