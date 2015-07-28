Param(
    [string] $scriptPath,
    [string] $clusterXMLfile,
    [string] $registryXMLfile,
    [string] $currentUser
)


#########################################################################
# Author: Stiven Skoklevski,
# Create and configure the Windows Cluster
#########################################################################

Import-Module FailoverClusters

###########################################
# Create Cluster with Primary Node only
###########################################
function CreateCluster()
{
    log "INFO: Creating cluster $clusterName utilising nodes $primaryNode on static IP $clusterIP."

    # done in 2 steps to ensure that the primary  node is assigned a cluster ID of 1.
    # A cluster ID of 1 ensures that this node is considered the primary node          
    New-Cluster -Name $clusterName  -Node $primaryNode  -StaticAddress $clusterIP -NoStorage
    Start-Sleep -Seconds 10
    log "INFO: Primary node: '$primaryNode' has been added."

    log "INFO: Updating Cluster description to $clusterDescription."
    $c = Get-Cluster
    $c.Description = $clusterDescription

    log "INFO: Created cluster $clusterName utilising nodes $primaryNode on static IP $clusterIP."
}

###########################################
# Create Cluster with Secondary Node only
###########################################
function AddSecondaryNode()
{
    log "INFO: Adding node '$secondaryNode' to cluster '$clusterName'."
        
    Get-Cluster | Add-ClusterNode -Name $secondaryNode -NoStorage
    Start-Sleep -Seconds 10

    log "INFO: Added node '$secondaryNode' to cluster '$clusterName'."
}

###########################################
# Grant Cluster Permissions
###########################################
function GrantClusterPermissions()
{

    $domain = get-domainshortname
    $adminUser = (Get-VariableValue $ADMIN "agilitydeploy" $true)

    log "INFO: Granting $domain\$adminUser full permissions to the cluster '$clusterName'."
    Grant-ClusterAccess -User $domain\$adminUser -Full
    log "INFO: Granted $domain\$adminUser full permissions to the cluster '$clusterName'."
    
    log "INFO: Granting $currentUser full permissions to the cluster '$clusterName'."
    Grant-ClusterAccess -User $currentUser -Full
    log "INFO: Granted $currentUser full permissions to the cluster '$clusterName'."
}

###########################################
# Configure Cluster Network
#
# PDC has 2 NICs. A management NIC and a Resource stack NIC
# This functions enables the Resource Stack NIC for Cluster and Client communnications
# and disables the Management NIC from being used by the Cluster
###########################################
function ConfigureClusterNetwork()
{
    log "INFO: Configuring cluster."

    log "INFO: Get Default IP Gateway."
    $networkAdapter = Get-WmiObject win32_networkAdapterConfiguration | 
        Select index,description,ipaddress, defaultipgateway | where-object{$_.defaultipgateway -ne $null}

    $defaultGateway = $networkAdapter.defaultipgateway
    if($defaultGateway -eq $null)
    {
        log "ERROR: Default IP Gateway was not found."
        return
    }

    $defaultGatewayPrefix = $defaultGateway.split('.')[0] + '.' + `
            $defaultGateway.split('.')[1] + '.' + `
            $defaultGateway.split('.')[2]

    log "INFO: Default IP Gateway of '$defaultGateway' was found with prefix of '$defaultGatewayPrefix'"

    $networks = Get-ClusterNetwork

    foreach($network in $networks)
    {
        $address = $network.Address
        $addressPrefix = $address.split('.')[0] + '.' + `
            $address.split('.')[1] + '.' + `
            $address.split('.')[2]

        if($defaultGatewayPrefix -eq $addressPrefix)
        {
            # found Resource Stack address
            $network.Name = $clusterNetwork
            $network.Role = 3 # Allow cluster and client network communications
            
            log "INFO: Set network to $clusterNetwork and Role = 3 (Allow cluster and client network communications)."
        }
        else
        {
            # found Management Stack address
            $networkName = "Management Network"
            $network.Name = $networkName
            $network.Role = 0 # Do NOT allow cluster or client network communications

            log "INFO: Set network to $networkName and Role = 0 (Do NOT allow cluster or client network communications)."
        }

    }

    log "INFO: Updating Cluster IP Address Name from 'Cluster IP Address' to $clusterIPName."
    $ipResource = Get-ClusterResource | Where-Object {$_.ResourceType -eq 'IP Address'}
    $ipResource.Name = $clusterIPName

    log "INFO: Configured cluster."
}

###########################################
# Configure Disks
###########################################
function ConfigureDisks()
{
    log "INFO: Configuring clustered available disks."

    # Add all available disks
    Get-ClusterAvailableDisk | Add-ClusterDisk

    log "INFO: Renaming cluster disks to be the same as the volume labels."
    $ClusterDisks =  (Get-CimInstance -ClassName MSCluster_Resource -Namespace root/mscluster -Filter "type = 'Physical Disk'") | Sort-Object Name
    foreach ($Disk in $ClusterDisks) 
    {
        $DiskResource = Get-CimAssociatedInstance -InputObject $Disk -ResultClass MSCluster_DiskPartition

        if (-not ($DiskResource.VolumeLabel -eq $Disk.Name)) 
        {
            log "INFO: Renaming $($Disk.Name) to $($DiskResource.VolumeLabel)."
            Invoke-CimMethod -InputObject $Disk -MethodName Rename -Arguments @{newName = $DiskResource.VolumeLabel}
        }

    }

    log "INFO: Configured clustered available disks."
}

###########################################
# Configure Witness
###########################################
function ConfigureWitness()
{
    log "INFO: Configuring Witness/Quorum on Disk named $witnessDiskName."
    # Configure Witness/Quorum
    Start-Sleep -Seconds 5
    Set-ClusterQuorum -NodeAndDiskMajority $witnessDiskName

    log "INFO: Configured Witness/Quorum on Disk named $witnessDiskName."
}

###########################################
# Validate Cluster - The SQL Cluster install will not pass pre-reqs if the cluster is not validated
###########################################
function ValidateCluster()
{
    log "INFO: Validating the cluster: '$clusterName'."
    # Configure Witness/Quorum
    Start-Sleep -Seconds 5
    $currentDate = get-date -Format yyyyMMddHHmm
    $testReport = "$scriptPath\Logs\ClusterValidation_$($clusterName)_$currentDate.html"
    Test-Cluster -ReportName $testReport
    Start-Sleep -Seconds 20

    log "INFO: Validated the cluster: '$clusterName'. See validation report: '$testReport'"
}

#####################################################
# This script should only run when the required number of shared disks are available
#####################################################
function CheckPreReqs([string]$propertyName)
{
    $isSharedDiskConfigured = GetNodeValue $registryXMLfile 'IsSharedDiskConfigured' 
    if($isSharedDiskConfigured -eq 'True')
    {
        log "INFO: Pre-requisistes completed. Shared Disks are available."
        return $true
    }
    else
    {
        log "ERROR: Pre-requisistes NOT completed. Shared disks are NOT available"
        SetNodeValue $registryXMLfile $propertyName 'False'
        return $false
    }
}

###########################################
# Main
###########################################

Set-Location -Path $scriptPath 

 # Logging must be configured here. otherwise it gets lost in the nested calls# 
 . .\LoggingV2.ps1 $true $scriptPath "ConfigureWindowsCluster.ps1"

. .\ConfigureMWS2Registry.ps1
. .\PlatformUtils.ps1
. .\VariableUtility.ps1

$preReqsCompleted = CheckPreReqs 'IsSharedDiskConfigured'
if(!$preReqsCompleted)
{
    return
}

if([String]::IsNullOrEmpty($clusterXMLfile))
{
   log "ERROR: The clusterXMLfile parameter is null or empty."
}
else
{
    # *** configure and validate existence of input file
    $inputFile = "$scriptPath\$clusterXMLfile"

    if ((CheckFileExists( $inputFile)) -ne $true)
    {
        log "ERROR: $inputFile is missing, users will not be configured."
        return
    }

    log "INFO: ***** Executing $clusterXMLfile ***********************************************************"

    # Get the xml Data
    $xml = [xml](Get-Content $clusterXMLfile)
 
    $nodes = $xml.SelectNodes("//doc/WindowsCluster")
    
    if (([string]::IsNullOrEmpty($nodes)))
    {
        log "No cluster settings to configure in: '$clusterXMLfile'"
        return
    }


    foreach ($node in $nodes) 
    {
        $clusterName = ([string](Get-ServerName $node.GetAttribute("ClusterName"))).ToUpper() 
        $clusterDescription = $node.GetAttribute('ClusterDescription') 
        $primaryNode = ([string](Get-ServerName $node.GetAttribute("PrimaryNode"))).ToUpper() 
        $secondaryNode = ([string](Get-ServerName $node.GetAttribute('SecondaryNode'))).ToUpper()
        $clusterIP = $node.GetAttribute('ClusterIP')
        $clusterNetwork = ([string](Get-ServerName $node.GetAttribute('ClusterNetwork'))).ToUpper()
        $clusterIPName = ([string](Get-ServerName $node.GetAttribute('ClusterIPName'))).ToUpper()
        $witnessDiskName = $node.GetAttribute('WitnessDiskName')

        if([String]::IsNullOrEmpty($clusterName))
        {
            log "ERROR: clusterName is empty."
            return                            
        }


        if([String]::IsNullOrEmpty($primaryNode))
        {
            log "ERROR: primaryNode is empty."
            return                            
        }


        if([String]::IsNullOrEmpty($secondaryNode))
        {
            log "ERROR: secondaryNode is empty."
            return                            
        }


        if([String]::IsNullOrEmpty($clusterIP))
        {
            log "ERROR: clusterIP is empty."
            return                            
        }


        if([String]::IsNullOrEmpty($clusterNetwork))
        {
            log "ERROR: clusterNetwork is empty."
            return                            
        }


        if([String]::IsNullOrEmpty($clusterIPName))
        {
            log "ERROR: clusterIPName is empty."
            return                            
        }


        if([String]::IsNullOrEmpty($witnessDiskName))
        {
            log "ERROR: witnessDiskName is empty."
            return                            
        }

        $clusterExists = Get-Cluster
        if($clusterExists -ne $null)
        {
            log "INFO: The cluster '$clusterName' already exists and will not be recreated."
        }
        else
        {
            CreateCluster

            GrantClusterPermissions

            ConfigureClusterNetwork

            ConfigureDisks

            ConfigureWitness

            # Add secondary node at the end to ensure drive letters arer assigned to cluster disk 
            # are in accordance with what is assigned to actual disks
            AddSecondaryNode

            ValidateCluster

            # Set registry property to True to allow next script to execute
            SetNodeValue $registryXMLfile 'IsWindowsClusterConfigured' 'True'
        }
    }

}
