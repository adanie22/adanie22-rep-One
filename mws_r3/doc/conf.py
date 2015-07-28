# -*- coding: utf-8 -*-
import sys, os
sys.path.insert(0, '../')
#sys.path.insert(0, os.getcwd())

sys.path.append(os.path.abspath('_themes'))
from pprint import pprint as pp

#extensions = ['sphinx.ext.autodoc', 'sphinx.ext.viewcode']
extensions = ['sphinx.ext.autodoc', 'sphinx.ext.intersphinx']
templates_path = ['_templates']

source_suffix = '.rst'
master_doc = 'index'

project = u'TxfrTop'
copyright = u'2011, The TxfrTop Team'

version = ''

#from txfrtop import __version__ as release

release = ''
import re
#if 'dev' in release:
#    release = release[:release.find('dev') + 3]
#if release == 'unknown':
#    version = release
#else:
#    version = re.match(r'\d+\.\d+(?:\.\d+)?', release).group()
version = release

exclude_patterns = ['_build']

#pygments_style = 'sphinx'
pygments_style = 'txfrtop_theme_support.TxfrTopStyle'

#html_theme = 'default'

html_theme = 'txfrtop'
html_theme_path = ['_themes']
html_static_path = ['_static']

html_sidebars = {
    'index':    ['sidebarlogo.html', 'sidebarintro.html', 'sourcelink.html',
                 'searchbox.html'],
    '**':       ['sidebarlogo.html', 'localtoc.html', 'relations.html',
                 'sourcelink.html', 'searchbox.html']
}


htmlhelp_basename = 'txfrtopdoc'
latex_elements = {
}
latex_documents = [
  ('index', 'txfrtop.tex', u'txfrtop Documentation',
   u'XX', 'manual'),
]
man_pages = [
    ('index', 'txfrtop', u'txfrtop Documentation',
     [u'XX'], 1)
]
texinfo_documents = [
  ('index', 'txfrtop', u'txfrtop Documentation',
   u'XX', 'txfrtop', 'One line description of project.',
   'Miscellaneous'),
]
epub_title = u'txfrtop'
epub_author = u'XX'
epub_publisher = u'XX'
epub_copyright = u'2013, XX'
autodoc_member_order = 'bysource'
