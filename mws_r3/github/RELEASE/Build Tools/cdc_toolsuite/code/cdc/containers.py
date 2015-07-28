from pprint import pprint as pp
import re
import requests

import default
from containers import *
from scripts import *

def get_containers_info(containers_unique,env=default.ENV):
    """
    in: 

        blueprints.get_bps() - 

            [(26, 'VMProject', 'MWS2'),
             (27, 'VMEnvironment', 'DEV_R1'),..]

    out:
            (26:VMProject) MWS2     envs: 2         policies: 6     bprints: 23     stacks: 0       scripts: 182
            (62:VMEnvironment) SANDPIT      envs: 0         policies: 4     bprints: 3      stacks: 3       scripts: 18
            (261:VMProject) MWS2 R2 envs: 3         policies: 2     bprints: 12     stacks: 0       scripts: 73

    """
    for i,env_type,k in containers_unique:
        c = Container(i,env)
        print get_container_info(c,env_type)

def get_container_info(c,env_type):
    d = {'environments':0,'policies':0,'blueprints':0,'stacks':0,'scripts':0}
    if c.json.has_key('environments'): d['environments'] = len(c.json['environments'])
    if c.json.has_key('policies'): d['policies'] = len(c.json['policies'])
    if c.json.has_key('blueprints'): d['blueprints'] = len(c.json['blueprints'])
    if c.json.has_key('packages'): d['packages'] = len(c.json['packages'])
    if c.json.has_key('stacks'): d['stacks'] = len(c.json['stacks'])
    if c.json.has_key('scripts'): d['scripts'] = len(c.json['scripts'])
    return '%s\tenvs: %i \tpolicies: %i \tbprints: %i \tstacks: %i \tscripts: %i'%(('(%s:%s) %s'%(c.json['id'],env_type,str(c.json['name']))).ljust(17),d['environments'],d['policies'],d['blueprints'],d['stacks'],d['scripts'])
    
class Container:
    def __init__(self,id,env=default.ENV):
        cfg = default.CFG[env]
        self.id = id
        self.env = env
        self.url,self.user,self.passwd = '%s/container/%s'%(cfg['BASE_URL'],self.id),'%s'%cfg['USER'],'%s'%cfg['PASSWD']
        self.json = self.get_json()

    def get_json(self,hdrs={'accept':'application/json'}):
        r = requests.get(self.url, auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)
        return r.json()

    def get_parent(self):
        return self.json['parent']['name']

    def get_scripts(self):
        l = []
        if self.json.has_key('scripts'):
            [l.append((scripts.Script(i['id']).get_info())) for i in self.json['scripts']]
        return l

    def get_vars(self):
        l = []
        if self.json.has_key('variables'):
            l2 = self.get_meta()
            for var in self.json['variables']:
                if var.has_key('stringValue'):
                    l2.extend((var['name'],var['stringValue']))
                else:
                    l2.append((var['name'],''))
                l.append(l2)
        return {'container_variables':l}


    def __repr__(self):
        return self.json 

    def __repr__(self):
        return self.json

class Containers:
    def __init__(self,env=default.ENV):
        cfg = default.CFG[env]
        self.env = env
        self.url,self.user,self.passwd = '%s/container'%cfg['BASE_URL'],'%s'%cfg['USER'],'%s'%cfg['PASSWD']
        self.json = self.get_json()

    def get_json(self,hdrs={'accept':'application/json'}):
        """
>>> j['links'][0].keys()
[u'name', u'href', u'rel', u'position', u'type', u'id']
        """
        r = requests.get(self.url, auth=('%s'%self.user, '%s'%self.passwd),verify=False,headers=hdrs)
        if r.status_code == 200:
            return r.json()['links']

    def get_container_list(self):
        for i in self.json:
            yield (Container(i['id']).get_parent(),i['id'],i['name'])

    def get_containers(self):
        for i in self.get_container_list():
            container = Container(i[0])
            yield container.json

    def get_container2(self):
        for i in self.get_container_list():
            container = Container(i[0])
            yield (container['name'],container['parent']['name'],container['parent']['href'])

    def __str__(self):
        return '\n'.join([i['name'] for i in self.json])


