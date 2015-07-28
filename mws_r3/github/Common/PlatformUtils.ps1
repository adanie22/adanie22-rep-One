. .\GlobalRepository.ps1

function get-serviceAccountPassword($username) {
    (get-globalvariable("ServiceAccount\$username")).value
}

function get-domainname {
    (get-globalvariable("Global\DomainFQDN")).value
}

function get-domainshortname {
    (get-globalvariable("Global\DomainNetBIOS")).value
}

function get-Computername([string]$ComponentID,[string]$InstanceID){
    $c = (get-globalvariable("Global\CustomerID")).value

    $l = (get-globalvariable("Global\LocationID")).value
    $name = "$c$($ComponentID)$l$($InstanceID)W"
    $name
}

########################################################################################
# Author: Marina Krynina
# $serverShortName is in ComponentId-InstanceId format, e.g. DBS-003
#########################################################################################
function Get-ServerName([string]$serverShortName)
{
    ($componentId, $instanceId) = $serverShortName.Split("-") 
    $servername = (get-Computername $componentId $instanceId)

    return $servername
}

