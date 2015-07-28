Param(
    [string] $scriptPath
)


#################################################################################################
# Author: Marina Krynina
# Desc:   Functions to support using people picker over a one-way trust
#################################################################################################

function CheckForError
{
    # check if error.txt exists. if yes, read it and throw exception
    # This is done to get an error code from the scheduled task.
    $errorFile = "$scriptPath\error.txt"
    if (CheckFileExists($errorFile))
    {
        $error = Get-Content $errorFile
        Remove-Item $errorFile
   
        throw $error
    }
}

############################################################################################
# Main
############################################################################################
# Load Common functions
. .\FilesUtility.ps1
. .\VariableUtility.ps1
. .\PlatformUtils.ps1
. .\LaunchProcess.ps1
. .\Construct-URL.ps1

Set-Location -Path $scriptPath 

try
{
    $msg = "Start People picker configuration - Set app password"
    log "INFO: Starting $msg"
    log "INFO: Getting variables values or setting defaults if the variables are not populated."

    # *** setup account 
    $domain = get-domainshortname
    $domainFull = get-domainname
    $user = (Get-VariableValue $ADMIN "agilitydeploy" $true)
    $password = get-serviceAccountPassword -username $user

    $server = ([string](Get-ServerName (Get-VariableValue $WEB_APP_SERVER "APP-001" $true))).ToUpper() 
    $URL = (ConstructURL $server $domainFull $true)
    log "INFO: URL = $URL"
 
    # *** configuration input file
    $config_xml = (Get-VariableValue $TRUSTEDDOMAINS_XML "TrustedDomainConfig.xml" $true)    
    $inputFile = "$scriptPath\$config_xml"

    if ((CheckFileExists( $inputFile)) -ne $true)
    {
        log "WARNING: Config $inputFile file is missing, people picker will not be configured for one way trust."
        return
    }

    # Get the xml Data
    $xml = [xml](Get-Content $inputFile)

    $nodes = $xml.SelectNodes("//TrustedDomains/Domain")

    if (([string]::IsNullOrEmpty($nodes)))
    {
        log "INFO: No trusted domains configured in: '$inputFile'"
        return
    }

    $trustedDomainsList = ""
    foreach ($node in $nodes) 
    {
        # Agility Variables support only 1 domain
        $trustedDomain = (Get-VariableValue $TRUSTED_DOMAIN ($node.GetAttribute("FQDN")) $true) 
        $trustedAccount = (Get-VariableValue $TRUSTED_ACCOUNT ($node.GetAttribute("Account")) $true) 
        $trustedPassword = (Get-VariableValue $TRUSTED_PASSWORD ($node.GetAttribute("Password")) $true) 

        log "INFO: trustedDomain = $trustedDomain; trustedAccount = $trustedAccount"

        if (([string]::IsNullOrEmpty($trustedDomain)))
        {
            throw "ERROR: trustedDomain is missing, skipping the record"
        }
        if (([string]::IsNullOrEmpty($trustedAccount)))
        {
            throw "ERROR: trustedAccount is missing, skipping the record"
        }
        if (([string]::IsNullOrEmpty($trustedPassword)))
        {
            throw "ERROR: trustedPassword is missing, skipping the record"
        }

        [string]$trustedDomainsList += "domain:$trustedDomain,$trustedDomain\$trustedAccount,$trustedPassword;"
    }

    if ($trustedDomainsList.EndsWith(";"))
    {
        $trustedDomainsList = $trustedDomainsList.Remove(($trustedDomainsList.Length -1), 1)
    }

    log "INFO: $trustedDomainsList"

    $process = "$PSHOME\powershell.exe"
    $argument = "-file $scriptPath\Config\Set-PeoplePickerSearchADForests.ps1 -scriptPath $scriptPath -url $URL -trustingDomain $domainFull -trustedDomainsList $trustedDomainsList; exit `$LastExitCode"

    log "INFO: Calling $process under identity $domain\$user"
    log "INFO: Arguments $argument"

    $Result = LaunchProcessWithHighestPrivAsUser $process $argument "$domain\$user" $password

    # DEBUG
    # . .\Config\Set-PeoplePickerSearchADForests.ps1 $scriptPath $url $trustingDomain $trustedDomain $account $APP_PASSWORD
    
    CheckForError

    log "INFO: Finished $msg."
    return 0
}
catch
{
    throw "ERROR: $($_.Exception.Message)"
}