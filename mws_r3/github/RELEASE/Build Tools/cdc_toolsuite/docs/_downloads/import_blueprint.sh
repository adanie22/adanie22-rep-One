#!/bin/bash

#-------------------------------------------------------#
#Written by Charlie Roffe
#Published 10/8/2013
#Version 0.91 BETA
#AP Test versions: 9.0.11 GA
#BluePrints must be version 9.0.7 or higher
#Process:
# -Prompts for filename to import
# -Lists and allows selection of project
# -Lists and allows selection of stack
# -Fixes stacks using blueprint cleric
# -Confirms you want to import
# -Imports blueprint into selected project
#-------------------------------------------------------#

#"$HOST/agility/api/current/project/${PJIDS[$WHCH2]}/import"
#fixed-$
# USAGE
if [[ $# -lt 1 ]]; then
  echo "usage: $0 AGILITY_HOST"
  echo "e.g.,"
  echo "    $0  https://x.x.x.x:8443"
  exit 1
fi

STACKUPD=""
USERID=""
PASS=""
HOST=$1
FILENAME=notInitialized

echo -e "\nAgility Username: "
while [ "$USERID" = "" ]; do
  read -p "? " USERID
done

echo -e "\nAgility Password: "
while [ "$PASS" = "" ]; do
  read -p "? " PASS
done

echo -e "\nAre you importing a Blueprint that was exported from the same Agility Platform instance (Enter 'yes' or 'no')? "
while [ "$STACKUPD" = "" ]; do 
  read -p "? " STACKUPD
done

echo -e "\nChoose a filename to import (i.e. myBlueprint.zip):"
while [ "$FILENAME" = "" -o "${FILENAME: -4}" != ".zip" -o "$(echo ${#FILENAME})" -lt "5" -o ! -f $FILENAME ]; do
   read -p "? " FILENAME
   if [ ! -f $FILENAME ]; then echo "File does not exist, choose another file name"; fi
   if [ "$(echo ${#FILENAME})" -lt "5" ]; then echo "Filename too short, choose another file name"; fi

done

if [ "$STACKUPD" = "no" ]; then
echo "Querying stacks"

response=$(curl --write-out "\n%{http_code}\n" -o stacks.xml --connect-timeout 3 -s -k -u$USERID:$PASS "$HOST/agility/api/current/stack/search?fields=id,name&limit=10000&versionOpt=VERSION_INPROGRESS_HEAD_OPT")
status_code=$(echo "$response" | sed -n '$p')
if [[ "$status_code" != "200" ]]; then
	echo "HTTP status code is $status_code, exiting..."
	exit 1
fi

fi
echo "Querying projects"

response=$(curl --write-out "\n%{http_code}\n" -o projects.xml --connect-timeout 3 -s -k -u$USERID:$PASS "$HOST/agility/api/current/project/search?fields=id,name&limit=10000")
status_code=$(echo "$response" | sed -n '$p')
if [[ "$status_code" != "200" ]]; then
	echo "HTTP status code is $status_code, exiting..."
	exit 1
fi

tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:id/text()" | xmllint --shell projects.xml | egrep '^\w'`
read -a PJIDS <<<$tmp

tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:id/text()" | xmllint --shell stacks.xml | egrep '^\w'`
read -a STIDS <<<$tmp

IFS=$(echo -en "\n\b")

tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:name/text()" | xmllint --shell projects.xml | egrep '^\w'`
declare -a PROJS
I=0
echo "****** PROJECTS ******"
for proj in $tmp
do
   PROJS[$I]="$proj"     
   echo $I: \"${PROJS[$I]}\" \(${PJIDS[$I]}\)
   ((I++))
done

WHCH2=-1
echo -e "\nPlease select a project:"
while [[ ! ($WHCH2 =~ ^[0-9]+$) ]] || [ "$WHCH2" -lt "0" -o "$WHCH2" -gt $[ $I - 1 ] ]; do
  read -p "? " WHCH2
done

#Extract name from BP
unzip -oq $FILENAME envelope.xml

echo "Querying blueprint names from project"
response=$(curl --write-out "\n%{http_code}\n" -o blueprints.xml --connect-timeout 3 -s -k -u$USERID:$PASS "$HOST/agility/api/current/project/${PJIDS[$WHCH2]}/blueprint?fields=id,assetPath,name,version&versionOpt=VERSION_INPROGRESS_HEAD_OPT")
status_code=$(echo "$response" | sed -n '$p')
if [[ "$status_code" != "200" ]]; then
	echo "HTTP status code is $status_code, exiting..."
	exit 1
fi

NAMEOFBP=`echo -e 'setrootns\n cat //ns1:Asset/ns1:assetType[ns1:href="assettype/6"]/../ns1:name/text()' | xmllint --shell envelope.xml | egrep '^\w'`
tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:name/text()" | xmllint --shell blueprints.xml | egrep '^\w'`
declare -a BPS
for name in $tmp
do
     if [ "$name" = "$NAMEOFBP" ];
     then
        echo "Blueprint \"$NAMEOFBP\" already exists in project ${PROJS[$WHCH2]}. Not uploading"
        exit 1
     fi
     ((I++))
done

if [ "$STACKUPD" = "no" ]; then

echo "Querying stacks"

IFS=$(echo -en "\n\b")

tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:name/text()" | xmllint --shell stacks.xml | egrep '^\w'`
declare -a STKS
IFS=$(echo -en "\n\b")

I=0
echo "****** STACKS ******"
for stacks in $tmp
do
   STKS[$I]="$stacks"     
   echo $I: \"${STKS[$I]}\" \(${STIDS[$I]}\)
   ((I++))
done

WHCH3=-1
echo -e "\nPlease select a stack:"
while [[ ! ($WHCH3 =~ ^[0-9]+$) ]] || [ "$WHCH3" -lt "0" -o "$WHCH3" -gt $[ $I - 1 ] ]; do
  read -p "? " WHCH3
done
fi

echo -e "\nYou selected:"
echo "  project: ${PROJS[$WHCH2]} (${PJIDS[$WHCH2]})"
echo " filename: $FILENAME"
echo "blueprint: $NAMEOFBP"
if [ "$STACKUPD" = "no" ]; then
echo "    stack: ${STKS[$WHCH3]} (${STIDS[$WHCH3]})"
fi

#Import selected blueprint
echo -e "\nAre you sure you want to import the blueprint? (Enter 'yes')"
read -p "? " REPLY
if [ "$REPLY" != "yes" ];
then
    echo Cancelled operation.
    exit 1
fi

# Blueprint cleric - convert to use existing stack
if [ "$STACKUPD" = "no" ]; then
echo "Performing Blueprint Cleric tasks..."
perl ./blueprint-cleric.pl $FILENAME ${STIDS[$WHCH3]} "${STKS[$I]}"
FILENAME=fixed-$FILENAME
fi


# Upload zip
echo "Uploading zip file..."
response=$(curl --write-out "\n%{http_code}\n" --connect-timeout 3 -s -k -u$USERID:$PASS -X PUT --data-binary @"$FILENAME" "$HOST/agility/api/current/project/${PJIDS[$WHCH2]}/import" --header "Content-Type:application/x-zip")
status_code=$(echo "$response" | sed -n '$p')
if [[ "$status_code" != "200" ]]; then
	echo "HTTP status code is $status_code, exiting..."
	exit 1
fi

echo "Uploading complete"
rm -f projects.xml stacks.xml blueprints.xml envelope.xml

exit 0
