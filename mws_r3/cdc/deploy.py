# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#


import re
from pprint import pprint as pp

def get_component(s):
    try:
        return re.match('(\w+)\W',s).groups()[0]
    except:
        print s
        xxxx
    

def p1(fn_rpt):
    l = open(fn_rpt,'r').readlines()[0:10]
    l2 = []
    for i in l:
        print i
        recs = [j.rstrip().encode('ascii',errors='ignore') for j in i.rstrip().decode('utf16',errors='ignore').split('##')]
        print recs
        recs.insert(0,get_component(recs[0]))
        #recs.insert(0,get_component(recs[0]))
        l2.append(recs)
    return l2
