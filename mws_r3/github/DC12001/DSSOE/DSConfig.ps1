# This script will apply DSSOE configuration to an existing Domain Controller
# Created: 25-feb-2015

# Read DS.XML configuration file and get parameters from it
if(-not (Test-Path "$PSScriptRoot\dsconfig.xml")) { # unable to find DS.XML, exit
    return 'DS_NO_CONFIG_XML_FILE'
    exit
}

# if DSconfig module file not found, exit
if(-not (Test-Path "$PSScriptRoot\dsconfig.psm1")) {
    return 'DS_NO_MODULE_FILE'
    exit
}
else { # import module
    Import-Module "$PSScriptRoot\dsconfig.psm1" -Force 
}

# Check if script is launched using admin privileges.. warn if not
if((Test-IsCurrentUserAdmin) -eq $false) {
    return 'DS_NO_ADMIN_PRIVILEGES'
    exit
}

# read DS.XML file
[xml]$dsXml = Get-Content "$PSScriptRoot\dsconfig.xml"

# Check if DS OutputFolder exists or not.. create if doesn't exist
if(-not (Test-Path ($dsXml.DS.OutputFolder))) {
    New-Item -ItemType Directory -Path $dsXml.DS.OutputFolder -Force | Out-Null
}

# set log file
$fdt = (Get-Date).ToString("MMddyyyy-hhmmss") # formatted datetime to append in file name
Set-DSLogPath -Path "$($dsXml.DS.OutputFolder)\DSConfig-$fdt.log"
Write-DSLog -Message "[DS] Validate whether this server is a properly built domain controller"

# Verify that this is a properly setup domain controller
If((Test-DSSetup) -eq $false) { 
    Write-DSLog -Message "[DS] This server is not a properly built domain controller, script will exit"
    return 'DS_NOT_PROPERLY_SETUP'
    exit
}
else {
    Write-DSLog -Message "[DS] This server is a properly built domain controller"
}

# check to see if required parameters are supplied by user
$dcInNewForest = $dsXml.DS.ServerIsFirstDCInNewForest
$AdditionalDC = $dsXml.DS.ServerIsAdditionalDC
# valid values: (false, false) or (true, false) or (false, true)
#if($dsXml.DS.ServerIsFirstDCInNewForest -eq "" -or $dsXml.DS.ServerIsAdditionalDC -eq "") {
if(-not (($dcInNewForest -in ("true","false")) -and ($AdditionalDC -in ("true","false")))) {
    Write-DSLog -Message "[DS] Please specify valid values for ServerIsFirstDCInNewForest and ServerIsAdditionalDC parameters in DSCONFIG.XML file"
    return 'DS_REQUIRED_PARAMS_MISSING'
    exit
}
elseif($dcInNewForest -eq 'true' -and $AdditionalDC -eq 'true') {
    Write-DSLog -Message "[DS] Please specify valid values for ServerIsFirstDCInNewForest and ServerIsAdditionalDC parameters in DSCONFIG.XML file (both values cannot be 'true'"
    return 'DS_REQUIRED_PARAMS_MISSING'
    exit
}
 
# Configure DS pagefile settings if user has specified to do so
if($dsXml.DS.ConfigurePageFile -eq $true) {
    Write-DSLog -Message "[DS] Calling routine to set DS page file"
    Set-DSPageFile # call routine to configure page file
}
else {
    Write-DSLog -Message "[DS] Parameter to configure pagefile is not specified"
}

# Create reserve file if user has specified to do so
if($dsXml.ds.ReserveFile.Path -eq "" -or $dsXml.ds.ReserveFile.SizeMB -eq "") {
    Write-DSLog -Message "[DS] Parameters for creation of reserve file is not specified"
}
else { # configure reserve file
    Write-DSLog -Message "[DS] Calling routine to create a reserve file"
    New-DSReserveFile -ReserveFilePath $dsXml.ds.ReserveFile.Path -ReserveFileSizeMB ([int]($dsXml.ds.ReserveFile.SizeMB))
}

# Set external time server name if user has specified it
if($dsXml.ds.ExternalTimeServer.Name -eq "") {
    Write-DSLog -Message "[DS] Parameters to configure external time server is not specified"
}
else {
    Write-DSLog -Message "[DS] Calling routine to set external time server"
    Set-DSExternalTimeServerConfig -ExternalTimeServerName $dsXml.ds.ExternalTimeServer.Name
}

# Create OU structure for given customer type and code
if($dsXml.DS.ServerIsAdditionalDC -eq $true) { # this server is ADC, OU structure will not be created
    Write-DSLog -Message "[DS] Skipping OU structure creation on Additional DC"
}
else { # server is not ADC, proceed with OU structure creation
    if($dsXml.ds.OUStructure.CustomerCode -eq "") {
        Write-DSLog -Message "[DS] Required parameter (CustomerCode) is not specified, OU structure will not be created"
    }
    else { # check to see CustomerType is specified
        $ct = $dsXml.ds.OUStructure.CustomerType
        if($ct -eq "Standard" -or $ct -eq "MultiTenant") {
            Write-DSLog -Message "[DS] Parameter to create $ct OU structure is specified, calling routine to create it"
            New-DSCustomerOUStructure -CustomerType $ct -CustomerCode $dsXml.ds.OUStructure.CustomerCode
        }
        else {
            Write-DSLog -Message "[DS] An invalid CustomerType to create OU structure is specified: $ct"
        }
    }
}

# security baseline policies configuration
$sadc = $dsXml.DS.ServerIsAdditionalDC
$cdp = $dsXml.DS.SecurityBaselinePolicies.ConfigureDomainPolicy
$cdcp = $dsXml.DS.SecurityBaselinePolicies.ConfigureDomainControllerPolicy
Add-DSGPOPolicies -ServerIsAdditionalDC:($sadc -eq $true) -ConfigureDomainPolicy:($cdp -eq $true) `
-ConfigureDomainControllerPolicy:($cdcp -eq $true)

# configure DNS scavenging if server is not ADC
if($sadc -eq $true) {
    Write-DSLog -Message "[DS] This server is ADC (as per parameter value), DNS scavenging will not be configured"
}
else {
    Write-DSLog -Message "Calling routine to configure DNS Scavenging" 
    Enable-DNSScavenging -FirstDCInForest:($dsXml.DS.ServerIsFirstDCInNewForest -eq $true)
}

# configure DS backup if parameter is specified in xml
if($dsXml.DS.DSBackup.Configure -eq $true) { 
    Write-DSLog -Message "[DS] Parameter to configure DS backup is specified"
    $ebm = $dsXml.DS.DSBackup.EnableBackupMaintenance
    $bvp = $dsXml.DS.DSBackup.BackupVolumePath
    if($bvp -eq "" -or ((test-path $bvp) -eq $false) -or $bvp.length -gt 2) {
        Write-DSLog -Message "[DS] Backup volume path is not specified or invalid, backup won't be configured"
    }
    else {
        # call routine
        Write-DSLog -Message "[DS] Calling routine to configure backup"
        Set-DSBackupConfiguration -EnableBackupMaintenance:($ebm -eq $true) -BackupVolumePath $bvp
    }
}
else {
    Write-DSLog -Message "[DS] Parameter to configure DS backup is NOT specified, skipping it"
}

# Configure DNS forwarders if user has specified it
if($dsXml.DS.DNSForwarders.ForwardersIP -eq "") {
    Write-DSLog -Message "[DS] DNS forwarders IP not specified, skipping its configuration"
}
else {
    Write-DSLog -Message "[DS] Calling routine to configure DNS forwarders"
    Set-DSDNSForwarders -ForwardersIP $dsXml.DS.DNSForwarders.ForwardersIP
}

# Setting up DS repair mode
Write-DSLog -Message "[DS] Setting up DS repair mode"
Set-DSRepairMode

# Add DFS tools
Write-DSLog -Message "[DS] Adding DFS tools"
Add-DFSTools

# Save DS config file
Write-DSLog -Message "[DS] Saving AD server information to $($dsXml.DS.OutputFolder)\DSServerConfig-$fdt.txt"
Save-ADServerConfig -ADConfigFilePath "$($dsXml.DS.OutputFolder)\DSServerConfig-$fdt.txt"

# if DS specific hotfixes are present, install them
Write-DSLog -Message "[DS] Installing DS specific hotfixes (if any)"
Install-DSHotfix

# Generate best practices analyzer report and save it to user specified location
Write-DSLog -Message "[DS] Generating BPA report and saving them to $($dsXml.DS.OutputFolder)"
New-DSBPAReport -HTMLReportPath $dsXml.DS.OutputFolder

# McAfee antivirus exclusions configuration (if McAfee is installed)

Write-DSLog -Message "[DS] DS Server configuration script finished execution, server will reboot now"
#Shutdown -r -t 15 /c "Directory Services configuration script need to restart this server to finish the configuration process. This server will restart in 15 seconds."

