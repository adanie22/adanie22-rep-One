#!/bin/bash

#-------------------------------------------------------#
#Written by Charlie Roffe
#Published 10/8/2013
#Version 0.91 BETA
#AP Test versions: 9.0.11 GA
#BluePrints must be version 9.0.7 or higher
#Process:
# -Lists and allows selection of blueprint
# -Asks for output filename
# -Confirms you want to export
# -Export blueprint to given filename
#-------------------------------------------------------#

# USAGE
if [[ $# -lt 1 ]]; then
  echo "usage: $0 AGILITY_HOST"
  echo "e.g.,"
  echo "    $0  https://x.x.x.x:8443"
  exit 1
fi

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

echo "Querying blueprints"

response=$(curl --write-out "\n%{http_code}\n" -o blueprints.xml --connect-timeout 3 -s -k -u$USERID:$PASS "$HOST/agility/api/current/blueprint/search?fields=id,assetPath,name,version&versionOpt=VERSION_INPROGRESS_HEAD_OPT&limit=10000")
status_code=$(echo "$response" | sed -n '$p')
if [[ "$status_code" != "200" ]]; then
  echo "HTTP status code is $status_code, exiting..."
  exit 1
fi

tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:id/text()" | xmllint --shell blueprints.xml | egrep '^\w'`
read -a BPIDS <<<$tmp

IFS=$(echo -en "\n\b")

tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:assetPath/text()" | xmllint --shell blueprints.xml | egrep '^/\w'`
declare -a PATHS
I=0
for path in $tmp
do
   PATHS[$I]="$path"     
   ((I++))
done

tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:version/text()" | xmllint --shell blueprints.xml | egrep '^-?[0-9]' | sed s/-1/IN\ PROGRESS/`
declare -a VER
I=0
for version in $tmp
do
   VER[$I]="$version"
   ((I++))
done

tmp=`echo -e "setrootns\n cat //ns1:Asset/ns1:name/text()" | xmllint --shell blueprints.xml | egrep '^\w'`
declare -a BPS
I=0
echo "****** BLUEPRINTS ******"
for name in $tmp
do
     BPS[$I]="$name"
     echo $I: \"${BPS[$I]}\" \(${BPIDS[$I]}\) version ${VER[$I]} at ${PATHS[$I]}
     ((I++))
done

WHICH=-1
echo -e "\nPlease select a blueprint:"
while [[ ! ($WHICH =~ ^[0-9]+$) ]] || [ "$WHICH" -lt "0" -o "$WHICH" -gt $[ $I - 1 ] ]; do
  read -p "? " WHICH
done

echo -e "\nChoose a filename to export (i.e. myBlueprint.zip):"
while [ "$FILENAME" = "" -o "${FILENAME: -4}" != ".zip" -o "$(echo ${#FILENAME})" -lt "5" -o -f $FILENAME ]; do
   read -p "? " FILENAME
   if [ -f $FILENAME ]; then echo "File exists, choose another file name"; fi
   if [ "$(echo ${#FILENAME})" -lt "5" ]; then echo "Filename too short, choose another file name"; fi
done

echo -e "\nYou selected:"
echo "blueprint: ${BPS[$WHICH]} (${BPIDS[$WHICH]})"
echo " filename: $FILENAME"

#Export selected blueprint
echo -e "\nAre you sure you want to export the blueprint? (Enter 'yes')"
read -p "? " REPLY
if [ "$REPLY" != "yes" ];
then
    echo Cancelled operation.
    exit 1
fi

echo "Downloading blueprint"
response=$(curl --write-out "\n%{http_code}\n" --connect-timeout 3 -s -k -u$USERID:$PASS "$HOST/agility/api/current/blueprint/${BPIDS[$WHICH]}?r=GET")
status_code=$(echo "$response" | sed -n '$p')
if [[ $status_code != 200 ]] && [[ $status_code != 202 ]]; then
  echo "HTTP status code is $status_code, exiting..."
  exit 1
fi

sleep 2

response=$(curl --write-out "\n%{http_code}\n" --connect-timeout 3 -s -k -u$USERID:$PASS "$HOST/agility/api/current/blueprint/${BPIDS[$WHICH]}?r=GET&deep=false")
status_code=$(echo "$response" | sed -n '$p')
if [[ $status_code != 200 ]] && [[ $status_code != 202 ]]; then
  echo "HTTP status code is $status_code, exiting..."
  exit 1
fi

sleep 2

response=$(curl --write-out "\n%{http_code}\n" --connect-timeout 3 -s -k -u$USERID:$PASS "$HOST/agility/api/current/security/blueprint/${BPIDS[$WHICH]}?r=GET")
status_code=$(echo "$response" | sed -n '$p')
if [[ $status_code != 200 ]] && [[ $status_code != 202 ]]; then
  echo "HTTP status code is $status_code, exiting..."
  exit 1
fi

sleep 2

response=$(curl --write-out "\n%{http_code}\n" -m 6000 -s -k -o $FILENAME -u$USERID:$PASS "$HOST/agility/api/current/blueprint/${BPIDS[$WHICH]}/export?recursive=false&deep=false")
status_code=$(echo "$response" | sed -n '$p')
if [[ $status_code != 200 ]] && [[ $status_code != 202 ]]; then
  echo "HTTP status code is $status_code, exiting..."
  exit 1
fi

echo "Downloaded blueprint to file: $FILENAME"
rm -f projects.xml stacks.xml blueprints.xml envelope.xml

exit 0
