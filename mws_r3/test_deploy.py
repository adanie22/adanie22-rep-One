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
from cdc import default
from cdc import deploy
from cdc.xml import *
from cdc.util import *


################################################################################################################
# FIXTURES
################################################################################################################
@attr('process_deploy')
def process_deploy_test_runner():
    """
    """
    fn_rpt = 'install_items-20150526.txt'
    deploy.p1(fn_rpt)


