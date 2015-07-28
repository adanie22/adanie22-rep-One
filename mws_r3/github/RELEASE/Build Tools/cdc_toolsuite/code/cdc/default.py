# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#

"""
Included in all modules that connect to Agility, loads rest api connection details from `settings.json` file
into global variable dictionary `CFG`.
"""

import os,json
fp_settings = 'settings.json'
CFG = json.load(open(fp_settings,'rt'))
ENV=CFG['DEFAULT']
MAXN=CFG[ENV]['MAXN']

