from pprint import pprint as pp
import requests
import default
import re
from datetime import datetime
from containers import *

from lxml import etree as et
class Workload:
    def __init__(self,id,env=default.ENV):
        cfg = default.CFG[env]
        self.id = id
        self.env = env
        self.url,self.user,self.passwd = '%s/workload/%s'%(cfg['BASE_URL'],self.id),'%s'%cfg['USER'],'%s'%cfg['PASSWD']
        self.json = self.get_json()
        self.name,self.parent = self.json['name'],self.json['parent']['name']

    def get_meta(self):
        return [self.id,self.name,self.parent]

    def get_json(self,hdrs={'accept':'application/json'}):
        r = requests.get(self.url, auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)
        return r.json()

    def get_vars(self,hdrs={'accept':'application/json'}):
        """
u'resources': [{
                 u'name': u'Min GB RAM',
                 u'quantity': 0.0,
                 u'resourceType': u'MEMORY',
                {
                 u'name': u'Min CPU',
                 u'quantity': 2.0,
                 u'resourceType': u'PROCESSOR',
        """
        l = []
        if self.json.has_key('variables'):
            l2 = self.get_meta()
            for var in self.json['variables']:
                if var.has_key('stringValue'):
                    l2.extend((var['name'],var['stringValue']))
                else:
                    print var
                    xxx
                l.append(l2)
        return {'bp_variables':l}

    def get_xml(self,hdrs={'accept':'application/xml'}):
        r = requests.get(self.url, auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)
        return et.fromstring(str(r.text.encode('ascii', 'ignore')))

    def get_info(self):
        f1 = lambda x:datetime.fromtimestamp(int(x/1000)).strftime('%Y%m%d')
        j = self.json
        return (j['id'],j['parent']['name'],j['name'],j['creator']['name'],j['version'],f1(j['lastModified']))

    def get_detailed_info(self):
        id,p_name,name,creator,version,mtime = self.get_info()
        j = self.json
        return id,p_name,j['description'],name,creator,version,mtime,j['body']

    def update_ram_cpu(self,n_ram,n_cpu,isReplace=False):
        hdrs={'content-type':'application/xml'}
        try:
            e0 = self.get_xml()

            for e2_p in e0.findall('./{http://servicemesh.com/agility/api}anyOrderItem'):

                e_new_ram = et.fromstring(ram_xml(n_ram))
                e_new_cpu = et.fromstring(cpu_xml(n_cpu))

                e_ram_found,e_cpu_found = False,False

                e2_rp = get_resource_parent(e2_p,'Min CPU')
                if not e2_rp is None:
                    if isReplace:
                        e2_p.replace(e2_rp,e_new_cpu)
                else:
                        e2_p.append(e_new_cpu)

                e2_rp = get_resource_parent(e2_p,'Min GB RAM')
                if not e2_rp is None:
                    if isReplace:
                        e2_p.replace(e2_rp,e_new_ram)
                else:
                        e2_p.append(e_new_ram)

            resp = requests.put(self.url, data=et.tostring(e0),auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)

            return resp

        except AttributeError:

            print 'unable to update ram, cpu for %s'%self.url
            return None

    def update_cid_scid(self,hdrs={'content-type':'application/xml'}):
        e0 = self.get_xml()
        l_resources = e0.findall('.//{http://servicemesh.com/agility/api}variables')
        for e in e0.findall('.//{http://servicemesh.com/agility/api}variables'):
            e2_p = e.getparent()
            if e.find('{http://servicemesh.com/agility/api}name').text == 'Min GB RAM':
                e2_new = et.fromstring(ram_xml(4096))
                e2_p.replace(e,e2_new)
            
            if e.find('{http://servicemesh.com/agility/api}name').text == 'Min CPU':
                e2_new = et.fromstring(cpu_xml(2))
                e2_p.replace(e,e2_new)

        resp = requests.put(self.url, data=et.tostring(e0),auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)
        return resp

    def get_vars1(self,hdrs={'accept':'application/json'}):
        """
u'resources': [{
                 u'name': u'Min GB RAM',
                 u'quantity': 0.0,
                 u'resourceType': u'MEMORY',
                {
                 u'name': u'Min CPU',
                 u'quantity': 2.0,
                 u'resourceType': u'PROCESSOR',
        """
        d2 = {}
        if self.json.has_key('anyOrderItems'):
            l2 = []
            for i in self.json['anyOrderItems']:
                d_resources, d_vars = [], []
                if i.has_key('resources'):
                    for j in i['resources']:
                        d_resources.append(('%s:%s'%(j['resourceType'],j['name']),j['quantity']))
                if i.has_key('variables'):
                    for j in i['variables']:
                        if j.has_key('stringValue'):
                            d_vars.append((j['name'],j['stringValue']))
                l2.append((i['name'],{'resources':d_resources},{'variables':d_vars}))
            d2.setdefault('workloads',l2)
        return d2 

    def get_bp(self):
        for i in self.json:
            yield i

    def get_workloads(self):
        d2 = {}
        if self.json.has_key('anyOrderItems'):
            l2 = []
            for i in self.json['anyOrderItems']:
                if i.has_key('assetType'):
                    d2.setdefault(i['id'],i['assetType']['name'])
        return d2 

    def get_workload(self,workload_id,hdrs={'accept':'application/json'}):
        url = '%s/workload/%s'%(self.url,workload_id)
        r = requests.get(url,user='%s'%self.user,passwd='%s'%self.passwd)
        return r.json()

    def __repr__(self):
        return '%s (%s)'%(self.name,self.id)

    def __str__(self):
        return '%s (%s)'%(self.name,self.id)

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

    def update_ram_cpu2(self,n_ram,n_cpu,isReplace=False):
        hdrs={'content-type':'application/xml'}
        e0 = self.get_xml()
        e0 = update_resource_xml(e0,'Min CPU',2,'Processor')
        e0 = update_resource_xml(e0,'Min GB RAM',2,'Memory')
        resp = requests.put(self.url, data=et.tostring(e0),auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)
        return resp

def update_wkloads(wkloads,container_id,env):
    l_wkloads = wkloads.get_wkloads(container_id)
    for i in wkloads.d_wkloads.keys():
        b = Workload(i,env=env)
        print i
        resp = b.update_ram_cpu2(2,2)
        print b.json['name'],b.json['id'],resp

 
