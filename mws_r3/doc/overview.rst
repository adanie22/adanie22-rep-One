===================
Tool Suite Overview
===================

This tool suite provides:

* .xls and .docx reports on Agility assets
* export/import functionality for assets

Installing the tools
--------------------

Python 2.7.3
~~~~~~~~~~~~

Python 2.7.3 with requests and lxml packages is built on build machine that includes the following packages (RHEL 2.6.18-398.el5)

Build Machine requirements:

* openssl-devel.i386 0.9.8e-27.el5_10.4 
* gcc.x86_64 4.1.2-55.el5 
* Python-2.7.3.tar.bz2
* urllib3
* requests-2.5.3.tar.gz
* lxml-3.2.4.tgz
* xlrd
* xlwt
* python-docx

Targe Machine requirements:

* libxslt-1.1.17-4.el5_8.3.x86_64.rpm

Compiling on build machine:
Extract Python and run configure::

    cd sw/tmp
    bzip2 -dc ../Python-2.7.3.tar.bz2  |tar x
    cd Python-2.7.3
    mkdir ../Python-2.7.3
    ./configure --prefix=/home/dhatchett/sw/Python-2.7.3
    make install

Above will give you::

    file /home/dhatchett/Python-2.7.3/bin/python2.7: ELF 64-bit LSB executable, AMD x86-64, version 1 (SYSV), for GNU/Linux 2.6.9, dynamically linked (uses shared libs), not stripped

Install package pre-requisites::

    cd ~/sw/tmp
    export PATH=~/Python-2.7.3/bin:$PATH
    tar -xf ../urllib3-1.10.2.tar.gz
    cd urllib3-1.10.2
    python setup.py install --home=~/Python-2.7.3/

    export PATH=~/Python-2.7.3/bin:$PATH
    tar -xf ../requests-2.5.3.tar.gz
    cd requests-2.5.3/
    python setup.py install --home=~/Python-2.7.3/

    cd ~/sw/tmp
    tar -xzf ../lxml-3.2.4.tgz
    cd lxml-3.2.4
    python setup.py install --home=~/Python-2.7.3/

    cd ~/sw/tmp
    tar -xf ~/sw/xlrd-0.9.2.tar.gz
    cd xlrd-0.9.2
    python setup.py install --home=~/Python-2.7.3/

    cd ~/sw/tmp
    unzip ~/sw/xlwt-master.zip
    cd xlwt-master
    python setup.py install --home=~/Python-2.7.3/

    cd ~/sw/tmp
    tar -xf ../python-docx-0.8.5.tar.gz
    cd python-docx-0.8.5
    python setup.py install --home=~/Python-2.7.3/



Package Python-2.7.3::

    cd ~
    tar -czf sw/Python-2.7.3-`date +%Y%m%d`.tgz Python-2.7.3/


Verify Package::

    tar -tvf Python-2.7.3-20150523.tgz |grep "lxml.*egg"

        Python-2.7.3/lib/python2.7/site-packages/lxml-3.2.4-py2.7.egg-info
        Python-2.7.3/lib/python/lxml-3.2.4-py2.7.egg-info

Above will create python 2.7.3 package suitable for running on RHEL 2.6.18-398.el5. To extract package, Python will be installed into users home directory on target machine::

    cd ~
    tar -xf ../sw/Python-2.7.3-20150523.tgz
    cd Python-2.7.3/
    ls
        bin  include  lib  share

Install libxslt on Target. As root::

    rpm -i /home/dhatchett/sw/libxslt-1.1.17-4.el5_8.3.x86_64.rpm

To run Python (reports, import/export tools etc), first add following to shell PATH:

    export PATH=~/Python-2.7.3/bin:$PATH

CDC Import/Export Tools
~~~~~~~~~~~~~~~~~~~~~~~

Package tools on build machine::

    cd ~/mws_r3/dev
    ls 
        cdc  export  import  test

    tar -czf cdc-`date +%Y%m%d`.tgz cdc test
    
Install on target::

    cd ~
    mkdir -p mws_r3/dev
    cd mws_r3/dev
    tar -xf ~/sw/cdc-20150523.tgz

    #verify
    find ./test/test_*
    ./test/test_blueprints.py
    ./test/test_blueprints.pyc
    ./test/test_etl.py
    ./test/test_etl.pyc
    ./test/test_report.py
    ./test/test_report.pyc
    
    


