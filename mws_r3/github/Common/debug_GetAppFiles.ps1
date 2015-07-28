# MWS2 - Get App Files ##############################################################
# Author: Marina Krynina
# Desc:   Deploys installation files onto the target server
############################################################################################

# Mandatory heading
# Load Common functions
. .\GlobalRepository.ps1
. .\Logging.ps1
. .\VariableUtility.ps1
. .\FilesUtility.ps1
. .\LaunchProcess.ps1

# get current script location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$scriptName = $MyInvocation.MyCommand.Name
ConfigureLogging $scriptPath $scriptName

#########################################################################
# Main 
# - Deploys installation files onto the target server
#########################################################################
try
{
    log "INFO: Getting variables values or setting defaults if the variables are not populated."
    $sourceFolder = (Get-VariableValue $SOURCE "" $false) 

    log "INFO: Deploying $sourceFolder folder onto the target VM"
    get-appfiles $sourceFolder    

    log "INFO: Finished $msg."

    exit 0
}
catch
{
    $ex = $_.Exception | format-list | Out-String
    log "ERROR: Exception occurred `nException Message: $ex"

    exit 1
}