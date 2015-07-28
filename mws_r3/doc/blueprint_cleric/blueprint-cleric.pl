#!/usr/bin/perl
#-------------------------------------------------------#
#Written by John Betancur, Charlie Roffe SMI
#Published 10/14/2013
#Version 0.91 BETA
#AP Test versions: 9.0.10 GA
#BluePrints must be version 9.0.7 or higher
#No animals were harmed in the creation of this script
#Replaces the base stack in a BluePrint to match that 
#of the target environment
#-------------------------------------------------------#

#-------------------------------------------------------#
#Change Log
#Version 0.91
#Updateded Parsing - Charlie
#Removed need for zip perl libs - requires zip/unzip be installed instead - John
#-------------------------------------------------------#

use strict;

use vars qw( $opt_j );
#use Archive::Zip qw(:ERROR_CODES);
use Getopt::Std;
use File::Path;


if ( @ARGV < 3) {
    die <<EOF
BluePrint Cleric
Version: 	0.91 Beta
Author (s):	John Betancur, Charlie Roffe SMI
No animals were harmed in the creation of this script

usage: perl $0 <blueprint.zip> <TargetBaseStackID> <TargetBaseStackName>

EOF
}

#-------------------------------------------------------#
#Variables
#-------------------------------------------------------#
my $zipName = $ARGV[0];
(my $zipNameFixed = $zipName) =~ s{\.[^.]+$}{};
my $inFile = "$zipNameFixed" . "/envelope.xml";
my $outFile = "$zipNameFixed" . "/envelope.xml";
my $baseID = "$ARGV[1]";
my $baseName = "$ARGV[2]";
#my $zip = Archive::Zip->new();

#-------------------------------------------------------#
#Extract our BluePrint
#-------------------------------------------------------#
#my $status  = $zip->read($zipName);
#my @members = $zip->memberNames();
#die "Read of $zipName failed\n" if $status != AZ_OK;
#foreach (@members) {
#    $zip->extractMember("$_", "$zipNameFixed" ."/" . "$_");
#}
my $command = `if [ ! -d "$zipNameFixed" ]; then mkdir "$zipNameFixed"; fi; unzip -o "$zipName" -d "$zipNameFixed" -x signature`;

#-------------------------------------------------------#
# Read file into one string and do some replacing
#-------------------------------------------------------#
open (ENVELOPE, "<$inFile") or die "could not open $inFile";
my $data = do { local $/; <ENVELOPE>};
close ENVELOPE;

# Replace stack IDs
$data =~ s/(<ns1:baseStack.*?<ns1:id>)(.*?)(<.*?<\/ns1:baseStack>)/$1$baseID$3/gms;

$data =~ s/(<ns1:pendingAsset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?<ns1:id>)(.*?)(<.*<\/ns1:id>)/$1$baseID$3/gms;
$data =~ s/(<ns1:stack.*?<ns1:id>)(.*?)(<.*?<\/ns1:stack>)/$1$baseID$3/gms;
#$data =~ s/(<ns1:Asset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?<ns1:id>)(.*?)(<.*<\/ns1:id>)/$1$baseID$3/gms;

# Replace base stack names
$data =~ s/(<ns1:baseStack.*?<ns1:name>)(.*?)(<.*?<\/ns1:baseStack>)/$1$baseName$3/gms;
$data =~ s/(<ns1:pendingAsset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?<ns1:name>)(.*?)(<.*<\/ns1:name>)/$1$baseName$3/gms;
$data =~ s/(<ns1:stack.*?<ns1:name>)(.*?)(<.*?<\/ns1:stack>)/$1$baseName$3/gms;
#$data =~ s/(<ns1:Asset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?<ns1:name>)(.*?)(<.*<\/ns1:name>)/$1$baseName$3/gms;
$data =~ s/(<ns1:pendingAsset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?)(\n\s*?<ns1:slot>.*?<\/ns1:slot>)(.*?<\/ns1:pendingAsset)/$1$3/gms;
$data =~ s/(<ns1:pendingAsset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?)(\n\s*?<ns1:slotId>.*?<\/ns1:slotId>)(.*?<\/ns1:pendingAsset)/$1$3/gms;
#$data =~ s/(<ns1:Asset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?)(\n\s*?<ns1:slot>.*?<\/ns1:slot>)(.*?<\/ns1:Asset)/$1$3/gms;
#$data =~ s/(<ns1:Asset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?)(\n\s*?<ns1:slotId>.*?<\/ns1:slotId>)(.*?<\/ns1:Asset)/$1$slotID$3/gms;

#Replace some descriptions
$data =~ s/(<ns1:pendingAsset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?<ns1:description>)(.*?)(<.*<\/ns1:description>.*?<\/ns1:pendingAsset)/$1$baseName$3/gms;  
$data =~ s/(<ns1:anyOrderItem xsi:type=\"ns1:Workload\">.*?<ns1:description>)(.*?)(<.*<\/ns1:description>.*?<\/ns1:pendingAsset)/$1$baseName$3/gms;

#Delete SlotID's from the Base Stack
$data =~ s/(<ns1:baseStack.*?)(\n\s*?<ns1:slot>.*?<\/ns1:slot>)(.*?<\/ns1:baseStack>)/$1$3/gms;
$data =~ s/(<ns1:baseStack.*?)(\n\s*?<ns1:slotId>.*?<\/ns1:slotId>)(.*?<\/ns1:baseStack>)/$1$3/gms;

#Delete Clouds
#$data =~ s/(<ns1:cloud>.*?)(\n\s*?<ns1:id>.*?<\/ns1:id>)(.*?<\/ns1:cloud)/$1$3/gms;
#$data =~ s/(<ns1:cloud>.*?)(\n\s*?<ns1:name>.*?<\/ns1:name>)(.*?<\/ns1:cloud)/$1$3/gms;
#$data =~ s/\n\s*?<ns1:Asset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Cloud\">.*?<\/ns1:Asset>//g;
#$data =~ s/(<ns1:cloudType>.*?)(\n\s*?<ns1:id>.*?<\/ns1:id>)(.*?<\/ns1:cloudType)/$1$3/gms;
#$data =~ s/(<ns1:cloudType>.*?)(\n\s*?<ns1:name>.*?<\/ns1:name>)(.*?<\/ns1:cloudType)/$1$3/gms;

#Change Stack import Directives to Continue
$data =~ s/(<ns1:Asset xmlns:xsi=\"http:\/\/www.w3.org\/2001\/XMLSchema-instance\" xsi:type=\"ns1:Stack\">.*?<ns1:onNotFound>)(Fail)(<\/ns1:onNotFound.*?<\/ns1:Asset>)/$1Continue$3/gms;

#-------------------------------------------------------#
# Write string to output file
#-------------------------------------------------------#
open (OUTPUT, ">$outFile");
print OUTPUT $data;
close OUTPUT;

#-------------------------------------------------------#
#Re-Zip our BluePrint
#-------------------------------------------------------#
my $command = `cd "$zipNameFixed" ; zip -r "../fixed-$zipNameFixed.zip" envelope.xml attachment/* `;

#-------------------------------------------------------#
#Cleanup
#-------------------------------------------------------#
rmtree "$zipNameFixed";
