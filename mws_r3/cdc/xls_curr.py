# -*- coding: utf-8 -*-
# 
# Computer Sciences Corporation
#
# MWS SAS Deployment Code
#
# Author: dhatchett2
#

from itertools import groupby
import datetime, time
import util

import xlrd
import xlwt

#start a.ad
import xlsxwriter
#end   a.ad
from pprint import pprint as pp

def get_bold_font():
    fnt = Font()
    fnt.name = 'Arial'
    fnt.colour_index = 4
    fnt.bold = True
    return fnt

def get_style(wrap,color,isBold,isBorders):
    """ 
        patterns may be: NO_PATTERN, SOLID_PATTERN, or 0x00 through 0x12

        colors May be: 8 through 63. 0 = Black, 1 = White, 2 = Red, 3 = Green, 4 = Blue, 5 = Yellow, 6 = Magenta, 7 = Cyan, 16 = Maroon, 17 = Dark Green, 18 = Dark Blue, 19 = Dark Yellow , almost brown), 20 = Dark Magenta, 21 = Teal, 22 = Light Gray, 23 = Dark Gray, the list goes on...
    """

    style = xlwt.XFStyle()

    fnt = xlwt.Font()
    fnt.name = 'Arial'
    if isBold == True:
        fnt.bold = True
    else:
        fnt.bold = False

    style.font = fnt

    algn1 = xlwt.Alignment()
    algn1.wrap = wrap 
    style.alignment = algn1

    pattern = xlwt.Pattern() 
    pattern.pattern = xlwt.Pattern.SOLID_PATTERN 
    if color == 'default':
        pattern.pattern_fore_colour = 1 #color
    else:
        pattern.pattern_fore_colour = color

    style.pattern = pattern

    borders = xlwt.Borders() 
    borders.left = xlwt.Borders.HAIR 
    borders.right = xlwt.Borders.HAIR
    borders.top = xlwt.Borders.HAIR
    borders.bottom = xlwt.Borders.HAIR
    borders.left_colour = 0x40
    borders.right_colour = 0x40
    borders.top_colour = 0x40
    borders.bottom_colour = 0x40

    if isBorders:
        borders.left = 6
        borders.right = 6
        borders.top = 6
        borders.bottom = 6
    
    style.borders = borders
    algn1 = xlwt.Alignment()
    algn1.wrap = 1
    algn1.horz = algn1.HORZ_LEFT #HORZ_CENTER
    style.alignment = algn1

    return style

def write_hdr(sht,hdr,nrow,style):
    # write header
    for j,k in enumerate(hdr):
        h,colwidth = k
        sht.write(nrow,j,h,style)

def adjust_colwidths(sht,hdr):
    # adjust column widths
    for j,k in enumerate(hdr):
        h,colwidth = k
        sht.col(j).width = colwidth

