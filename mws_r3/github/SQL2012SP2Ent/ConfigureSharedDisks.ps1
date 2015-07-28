# ConfigureSharedDisks.ps1

# $SHAREDDISKXMLFILENAME='.\Install\Cluster\SharedDiskConfigSharedDB.xml'

#########################################################################
# Author: Stiven Skoklevski,
# Bring Shared disks online, initialised and then offline in preparation to be used by Windows Cluster
#########################################################################

#####################################################
# Configure drive
#####################################################
function ConfigureDrive([object]$disk, [string]$diskLabel, [string]$driveLetter)
{

    log "INFO: Configuring Drive - Label:$diskLabel, Letter: $driveLetter for drive number $($disk.Number) with friendly name '$($disk.FriendlyName)'"
    
    log "INFO: Stop the windows prompt asking to format drive."
    Stop-Service ShellHWDetection

    log "INFO: Setting disk to online"
    $disk | Set-Disk -IsOffline $false

    # once the disk is initialised for the first time this command will throw the exception 
    # 'Initialize-Disk : The disk has already been initialized.'
    # This exception can be ignored.
    log "INFO: Initialising disk"
    $disk | Initialize-Disk -PartitionStyle GPT -ErrorAction SilentlyContinue

    log "INFO: Create new partition"
    New-Partition –DiskNumber $($disk.Number) –DriveLetter $driveLetter -UseMaximumSize 

    # creating the partition takes time which cause the format volume to throw an exception that the disk is read-only,
    # so sleep to allow creation of partition to complete
    Start-Sleep -Seconds 10

    log "INFO: Format the volume"
    Format-Volume –DriveLetter $driveLetter –FileSystem NTFS -NewFileSystemLabel $diskLabel -Confirm:$false -AllocationUnitSize 65536

    log "INFO: Start the windows prompt asking to format drive."
    Start-Service ShellHWDetection

    # The SMS product installs software on all disks that do NOT contain a file with this name.
    # The SMS software should only be installed on the shared disk named PCDEVICES which is why this file is not created on this drive.
    if($diskLabel -ne "PCDEVICES")
    {
        $file = "$($driveLetter):\NO_SMS_ON_DRIVE.sms"
        New-Item $file -Type file -Force
        log "INFO: Created file: '$file'"
    }

    log "INFO: Setting disk to offline"
    $disk | Set-Disk -IsOffline $true

    log "INFO: Configured Drive - Label:$diskLabel, Letter: $driveLetter for drive number $($disk.Number) with friendly name '$($disk.FriendlyName)'"
}

#####################################################
# Manage Disks
#####################################################
function ManageDisks($xmlNodes, $disks, $type)
{
    $xmlNodesCount = $($xmlNodes | measure).Count
    $disksCount = $($disks | measure).Count
    If($xmlNodesCount -ne $disksCount)
    {
        log "ERROR: Number of $type disk nodes in the XML file does not equal number of $type disks available on VM."
        return
    }

    for($i=0; $i -le $xmlNodesCount - 1; $i++)
    {
        $node = $xmlNodes | Select-Object -index $i
        $disk = $disks | Select-Object -Index $i
        
        $diskLabel = $node.attributes['DiskLabel'].value
        $driveLetter = $node.attributes['DriveLetter'].value
        $type = $node.attributes['Type'].value
        if([String]::IsNullOrEmpty($diskLabel))
        {
            log "WARN: $diskLabel is empty."
            continue                            
        }
        if([String]::IsNullOrEmpty($driveLetter))
        {            
            log "WARN: $driveLetter is empty."
            continue                            
        }
        if([String]::IsNullOrEmpty($type))
        {            
            log "WARN: $type is empty."
            continue                            
        }

        ConfigureDrive $disk $diskLabel $driveLetter
    }
}

#####################################################
# This script should only run when the required number of shared disks are available
#####################################################
function CheckPreReqs([string]$numberSharedDisksReqd)
{
    $isSharedDiskConfigured = GetNodeValue $MWSREGISTRYXMLFILENAME 'IsSharedDiskConfigured' 
    # note check for IsClustered to ensure this script cannot be run 2x
    $diskAvailCount = (Get-Disk | Where-Object {($_.Path -like '*scsi*') -and ($_.IsClustered -eq $false)}  | measure).Count
    
    if($diskAvailCount -eq $numberSharedDisksReqd)
    {
        if($isSharedDiskConfigured -eq 'False') # check if disks have already been configured
        {
            log "INFO: Pre-requisistes completed. Number of disks and not already attached to the cluster found were: '$diskAvailCount'. Number of disks required: '$numberSharedDisksReqd'"
            return $true
        }
        else
        {
            log "WARN: Disks have already been configured."
            return $false
        }
    }
    else
    {
        log "ERROR: Pre-requisistes NOT completed. Number of disks found and not already attached to the cluster were: '$diskAvailCount'. Number of disks required: '$numberSharedDisksReqd'"
        SetNodeValue $MWSREGISTRYXMLFILENAME 'IsSharedDiskConfigured' 'False'
        return $false
    }
}

#####################################################
# Main
#####################################################

. .\ConfigureMWS2Registry.ps1

$preReqsCompleted = CheckPreReqs $NUMBERSHAREDDISKS
if(!$preReqsCompleted)
{
    return
}

if([String]::IsNullOrEmpty($SHAREDDISKXMLFILENAME))
{
   log "The SHAREDDISKXMLFILENAME parameter is null or empty."
}
else
{
    # *** configure and validate existence of input file
    $inputFile = "$scriptPath\$SHAREDDISKXMLFILENAME"

    if ((CheckFileExists( $inputFile)) -ne $true)
    {
        log "ERROR: $inputFile is missing, users will not be configured."
        return
    }

    # witness disk is 1GB while data disks are 10 GB, this number falls in between and this will help us distinguish between the 2 types
    $witnessSize = 2000000000

    log "INFO: ***** Executing $SHAREDDISKXMLFILENAME ***********************************************************"

    # Get the xml Data
    $xml = [xml](Get-Content $SHAREDDISKXMLFILENAME)

    $nodes = $xml.SelectNodes("//*[@DiskLabel]")

    # configure the witness disk
    $diskType = "Witness"
    $witnessDiskNodes = $nodes | Where {$_.Type -eq $diskType} |
                Sort-Object Number
                
    $witnessDisks = Get-Disk | Where-Object `
                        {($_.Path -like '*scsi*') -and `
                        ($_.IsClustered -eq $false) -and `
                        ($_.Size -lt $witnessSize)} | `
                    Sort-Object Number


    ManageDisks $witnessDiskNodes $witnessDisks $diskType

    # configure the data disks
    $diskType = "Data"
    $dataDiskNodes = $nodes | Where {$_.Type -eq $diskType} |
                Sort-Object Number

    $dataDisks = Get-Disk | Where-Object `
                        {($_.Path -like '*scsi*') -and `
                        ($_.IsClustered -eq $false) -and `
                        ($_.Size -gt $witnessSize)} | `
                    Sort-Object Number


    ManageDisks $dataDiskNodes $dataDisks $diskType

    # Set registry property to True to allow next script to execute
    SetNodeValue $MWSREGISTRYXMLFILENAME 'IsSharedDiskConfigured' 'True'

}