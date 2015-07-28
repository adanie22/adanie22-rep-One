# This module contain functions that will be used by DSConfig powershell script to configure
# domain controller post-promotion tasks
# Created: 24-feb-2015
# Author: Tarun Rajvanshi/Umesh Thakur

# Script level variables
$DSLogFilePath = ""

# -------------------------------------
# This function will set log file path
# -------------------------------------
Function Set-DSLogPath([string]$Path) {
    $script:DSLogFilePath = $Path
}

# -------------------------------------------------
# This function will write given string to log file
# -------------------------------------------------
Function Write-DSLog($Message) {
    $FormattedDateTime = "[" + (Get-Date).ToString("MM-dd-yyyy hh:mm:ss") + "] "
    Out-File -FilePath $Script:DSLogFilePath -InputObject ($FormattedDateTime + $Message) -Append
}

# -------------------------------------------------------------------------------
# This function configures pagefile (if user has said so), as per DSSOE standards
# -------------------------------------------------------------------------------
Function Set-DSPageFile {
	#$ADDrive = $ADDatabasepath.split(":")
    $ntdsRegPath = 'HKLM:\SYSTEM\CurrentControlSet\Services\NTDS\Parameters'
    $ADDrive = (Get-ItemProperty $ntdsRegPath -Name 'DSA Database File').'DSA Database file'
	
    # get a list of physical disks for query
    $ntdsdrivecheck = [string] $(wmic diskdrive get deviceid)
	
    # turn off automatic pagefile management by OS
    Write-DSLog -Message ("[PageFile] turning off automatic management of pagefile")
    $pfOut = (wmic computersystem set AutomaticManagedPagefile=False)
	Write-DSLog -Message ("[PageFile] $pfOut")

    # Get total visible memory for the purpose of setting pagefile
    $RAM = Get-WmiObject Win32_OperatingSystem | select TotalVisibleMemorySize
	$RAM = ($RAM.TotalVisibleMemorySize / 1kb).tostring("F00")

    # check if server has multiple disks as per DSSOE requirements and AD Database is installed to E drive
	if ((($ntdsdrivecheck.Contains("PHYSICALDRIVE2")) -eq $true) -and ($ADDrive[0] -eq "e"))
	{
        # create pagefile on E:\
        Write-DSLog -Message ("[PageFile] Attempting to create e:\pagefile.sys")
		$pfOut = (wmic.exe pagefileset create name="E:\pagefile.sys")
        Write-DSLog -Message ("[PageFile] $pfOut")
		
        # get reference to created page file and set its initial/max size
        $PageFile = Get-WmiObject Win32_PageFileSetting | Where-Object {$_.name -eq "E:\pagefile.sys"}
		$PageFile.InitialSize = [int]$RAM + 300
		$PageFile.MaximumSize = [int]$RAM * 1.5
		$PageFile.Put() | Out-Null

        # delete existing old pagefile
		$PageFile = Get-WmiObject Win32_PageFileSetting | Where-Object {$_.name -eq "C:\pagefile.sys"}
		$PageFile.delete()

        # Write to log file about pagefile set
		Write-DSLog -Message ("[PageFile] PageFile Settings Configuration done, File E:\pagefile.sys" + " Initial =" +$PageFile.InitialSize + " Max =" + $PageFile.MaximumSize)
		
	}
	else {
        # Server doesn't have disk partitions as per DSSOE, set pagefile on C:\
		
        # Create pagefile in C:\
        Write-DSLog -Message ("[PageFile] Attempting to create c:\pagefile.sys")
        $pfOut = (wmic.exe pagefileset create name="C:\pagefile.sys")
		Write-DSLog -Message ("[PageFile] $pfOut")

        # get reference to created page file and set its initial/max size
        $PageFile = Get-WmiObject Win32_PageFileSetting | Where-Object {$_.name -eq "C:\pagefile.sys"}
		$PageFile.InitialSize = [int]$RAM + 300
		$PageFile.MaximumSize = [int]$RAM * 1.5
		$PageFile.Put() | Out-Null

        # Write to log file about pagefile set
		Write-DSLog -Message ("[PageFile] PageFile Settings Configuration done File C:\pagefile.sys" + " Initial =" +$PageFile.InitialSize + " Max =" + $PageFile.MaximumSize)
	}
}

# -------------------------------------------------------------------------------------------------
# This function will create a reserve file for the purpose of dealing with future space constraints
# -------------------------------------------------------------------------------------------------
Function New-DSReserveFile($ReserveFilePath, $ReserveFileSizeMB) {
    # Delete existing reserve file, if found
	if( (Test-Path $ReserveFilePath) -eq $true)
	{
		[System.IO.File]::delete($ReserveFilePath)
		Write-DSLog -Message ("[ReserveFile] Existing reserve file deleted: " + $ReserveFilePath)
	}

    # create a new reserve file with given size
    try {
	    $file = new-object System.IO.FileStream $ReserveFilePath, Create, ReadWrite
	    $file.SetLength($ReserveFileSizeMB*1MB)
	    $file.Close()
	    Write-DSLog -Message "[ReserveFile] Reserve File created with given size $ReserveFileSizeMB MB"
    }
    catch { # something went wrong with creation of reserve file
        Write-DSLog -Message "[DS] Error while creating reserve file."
        Write-DSLog -Message "[DS] $($_.Exception.Message)"
    }
}

# --------------------------------------------
# Configure external time sync server options
# --------------------------------------------
Function Set-DSExternalTimeServerConfig($ExternalTimeServerName) {
    #Configuring External Time Server parameters
	Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\Parameters" -Name Type -Value NTP -Type String
	Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\Config" -Name AnnounceFlags -Value 5 -Type DWord
	Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\TimeProviders\NtpServer" -Name Enabled -Value 1 -Type DWord
	Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\Parameters" -Name NtpServer -Value "$ExternalTimeServerName,0x8" -Type String
	Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\Config" -Name SpecialPollInterval -Value 3600 -Type DWord
	Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\Config" -Name MaxPosPhaseCorrection -Value 172800 -Type DWord
	Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\W32Time\Config" -Name MaxNegPhaseCorrection -Value 172800 -Type DWord
	Write-DSLog -Message "[ExtTimeServer] External Time Server parameters configured"
}

# ---------------------------------------------------------------------
# Security GPO configuration; Domain policy & Domain Controller Policy
# ---------------------------------------------------------------------
Function Add-DSGPOPolicies(
    [switch] $ServerIsAdditionalDC, 
    [switch] $ConfigureDomainPolicy,
    [switch] $ConfigureDomainControllerPolicy)
{   
    # If this server is NOT additional domain controller, then configure this section
	If($ServerIsAdditionalDC -eq $false) {
        #Creating of GPO for CSC Standard default domain policy and default domain controllers policy
        $lgpOut = Invoke-Command {cmd /c c: "&" cd $PSScriptRoot "&" cscript localgpo.wsf /ConfigSCE}
        Write-DSLog -Message "[DSGPO] $lgpOut"
    
		# Store GPO Backup Path in a variable
		$GPOBackupPath = "$PSScriptRoot\CSC Security Policies"
		
        # configure domain security policy
		if($ConfigureDomainPolicy) { 
			Write-DSLog -Message "[DSGPO] Creating GPO for CSC Standard default domain policy"
			$gpOut = new-gpo -name DO-B-CSCBaselineDomainPolicy -Domain (Get-ADDomain -Server $(hostname)).dnsroot | new-gplink -target ((Get-ADDomain -Server $(hostname)).DistinguishedName) -Order 1 -Domain (Get-ADDomain -Server $(hostname)).dnsroot	
            Write-DSLog -Message "[DSGPO] $gpOut"
			$gpOut = import-gpo -BackupId EC56A14A-A314-472B-8591-9ABF7F8B93EF -TargetName DO-B-CSCBaselineDomainPolicy -path $GPOBackupPath
			Write-DSLog -Message "[DSGPO] $gpOut"
            $gpout = (gpupdate /force /boot)
            Write-DSLog -Message "[DSGPO] $gpOut"
            Write-DSLog -Message "[DSGPO] GPO for CSC Standard default domain policy has been created and applied"
		}
		else {# user has not specified to configure domain security policy
			Write-DSLog -Message "[DSGPO] Creation of GPO for CSC Standard default domain policy is not selected by user"
		}

        # Configure domain controller security policy
		if($ConfigureDomainControllerPolicy) {
			Write-DSLog -Message "[DSGPO] Creating GPO for CSC Standard default domain controller policy"
			$gpOut = new-gpo -name OU-B-CSCBaselineDomainControllerPolicy -Domain (Get-ADDomain -Server $(hostname)).dnsroot | new-gplink -target ((Get-ADDomain -Server $(hostname)).domaincontrollerscontainer) -Order 1 -Domain (Get-ADDomain -Server $(hostname)).dnsroot
            Write-DSLog -Message "[DSGPO] $gpOut"
			$gpOut = import-gpo -BackupId 60913860-13F2-4BD3-948D-888901BC4BAD -TargetName OU-B-CSCBaselineDomainControllerPolicy -path $GPOBackupPath
            Write-DSLog -Message "[DSGPO] $gpOut"
			$gpOut = (gpupdate /force /boot)			
			Write-DSLog -Message "[DSGPO] GPO for CSC Standard default domain controller policy has been created and applied"
		}
		else {
		    Write-DSLog -Message "[DSGPO] Creation of GPO for CSC Standard default domain controller policy is not selected by user"
        }
    }
}

# ----------------------------------------------------------------
# OU Creation in Standard AD or Multi-tenant AD + Delegation model
# ----------------------------------------------------------------
Function New-CSCOUStructure {
	#Creating OU's Under CSC
	$CSCOU = "OU=CSC," + (Get-ADDomain -Server $env:COMPUTERNAME).DistinguishedName
	New-ADOrganizationalUnit -Name Admins -Path $CSCOU -ProtectedFromAccidentalDeletion $true | Out-Null
	New-ADOrganizationalUnit -Name Workstations -Path $CSCOU -ProtectedFromAccidentalDeletion $true | Out-Null
	New-ADOrganizationalUnit -Name Groups -Path $CSCOU -ProtectedFromAccidentalDeletion $true | Out-Null
	New-ADOrganizationalUnit -Name Servers -Path $CSCOU -ProtectedFromAccidentalDeletion $true | Out-Null
	New-ADOrganizationalUnit -Name Users -Path $CSCOU -ProtectedFromAccidentalDeletion $true | Out-Null
}

# -----------------------------------------------------------
# This function will remove security inheritence for given OU
# -----------------------------------------------------------
Function Remove-DSOUInheritence($OUDistinguishedName) {
    $removeinheritance = [adsi]("LDAP://$OUDistinguishedName")
	$removeinheritance.psbase.get_objectsecurity().SetAccessRuleProtection($true, $true)
	$removeinheritance.psbase.CommitChanges()
}

# ----------------------------------------------------------------------
# This function will create OU structure for individual customer/account
# ----------------------------------------------------------------------
Function New-DSCustomerOUStructure(
    [ValidateSet("Standard", "MultiTenant")]$CustomerType,
    [string] $CustomerCode
) 
{
    # Save DN of this DC in a variable for below function calls
    $thisDC = (Get-ADDomain -Server $env:COMPUTERNAME).DistinguishedName

    # Creation of CSC Individual Customer OU Structure for 'Standard' Customer type
	if($CustomerType -eq 'Standard') {
		Write-DSLog -Message "[OUStructure] Creating CSC Standard Customer OU Structure"
					
		# Creating Top level OU's
		New-ADOrganizationalUnit -Name CSC -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		New-ADOrganizationalUnit -Name Admins -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		New-ADOrganizationalUnit -Name Groups -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		New-ADOrganizationalUnit -Name Servers -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		New-ADOrganizationalUnit -Name Workstations -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		New-ADOrganizationalUnit -Name "Service Accounts" -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		$WorkstationOU = "OU=Workstations,$thisDC"
		New-ADOrganizationalUnit -Name VDI -Path $WorkstationOU -ProtectedFromAccidentalDeletion $true | Out-Null
		New-ADOrganizationalUnit -Name MAC -Path $WorkstationOU -ProtectedFromAccidentalDeletion $true | Out-Null				
		New-ADOrganizationalUnit -Name "$CustomerCode Users" -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
					
		Write-DSLog -Message "[OUStructure] Calling Function New-CSCOUStructure" 
		New-CSCOUStructure
        
	}

    # Creation of CSC Multi-Tenant OU Structure for 'MultiTenant' Customer type
	if ($CustomerType -eq 'MultiTenant') {
		Write-DSLog -Message "[OUStructure] Creating CSC MultiTenant Customer OU Structure"
			
        #Installing Quest CMDlets
        Write-DSLog -Message "[OUStructure] Installing Quest cmdlets"
		$questcmdletspath = ($PSScriptRoot + "\Cmdlets.msi")
		$questcmdlets = [System.Diagnostics.Process]::Start($questcmdletspath,"/quiet") 
		$questcmdlets.WaitForExit()

		#Changing AD Object Mode to List Object
		#Write-DSLog -Message "[OUStructure] Changing AD Object Mode to List Object"
		#$Admode = [adsi]("LDAP://CN=Directory Service,CN=Windows NT,CN=Services,CN=Configuration," + $thisDC)
		#$Admode.dSHeuristics = "001"
		#$Admode.setinfo()
					
		# Creating Top level OU's and set their inheritence to not to inherit from parent
		New-ADOrganizationalUnit -Name CSC -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null 
        Remove-DSOUInheritence -OUDistinguishedName "ou=CSC,$thisDC"

		New-ADOrganizationalUnit -Name Admins -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		Remove-DSOUInheritence -OUDistinguishedName "ou=Admins,$thisDC"
	       	
		New-ADOrganizationalUnit -Name Accounts -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		Remove-DSOUInheritence -OUDistinguishedName "ou=Accounts,$thisDC"
	       	
		New-ADOrganizationalUnit -Name Resellers -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		Remove-DSOUInheritence -OUDistinguishedName "ou=Resellers,$thisDC"
	       	
		New-ADOrganizationalUnit -Name Groups -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		Remove-DSOUInheritence -OUDistinguishedName "ou=Groups,$thisDC"
	       	
		New-ADOrganizationalUnit -Name Servers -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		Remove-DSOUInheritence -OUDistinguishedName "ou=Servers,$thisDC"
	       	
		New-ADOrganizationalUnit -Name Workstations -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		Remove-DSOUInheritence -OUDistinguishedName "ou=Workstations,$thisDC"
	       	
        $WorkstationOU = "OU=Workstations,$thisDC"
		New-ADOrganizationalUnit -Name VDI -Path $WorkstationOU -ProtectedFromAccidentalDeletion $true | Out-Null
	       	
		New-ADOrganizationalUnit -Name MAC -Path $WorkstationOU -ProtectedFromAccidentalDeletion $true | Out-Null
	       	
		New-ADOrganizationalUnit -Name "Service Accounts" -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		Remove-DSOUInheritence -OUDistinguishedName "ou=Service Accounts,$thisDC"
	       	
		New-ADOrganizationalUnit -Name "$CustomerCode Users" -Path $thisDC -ProtectedFromAccidentalDeletion $true | Out-Null
		Remove-DSOUInheritence -OUDistinguishedName "ou=$CustomerCode Users,$thisDC"
	       	
		# Call function to create CSC standard OU structure		
		Write-DSLog -Message "[OUStructure] Calling Function New-CSCOUStructure" 
		New-CSCOUStructure
					
		#Creating Shared OU's
		$AccountsOUDN = "OU=Accounts,$thisDC"
		$ResellersOUDN = "OU=Resellers,$thisDC"
			
								
		#Group Creation
		$OUAdministrationRoles = "OU=Admins,$thisDC"
		New-ADGroup -Path $AccountsOUDN -name AllAccountUsers -samAccountName AllAccountUsers -GroupCategory  "security" -groupscope "DomainLocal" | Out-Null
		New-ADGroup -Path $ResellersOUDN -name AllResellerUsers -samAccountName AllResellerUsers -GroupCategory "security" -groupscope "DomainLocal" | Out-Null
		New-ADGroup -Path $OUAdministrationRoles -name Role-D-3rdLine-Wintel -samAccountName Role-D-3rdLine-Wintel -GroupCategory "security" -groupscope "Global" | Out-Null
					
		#Adding Role-D-3rdLine-Wintel group to domain admins
		#$root = [adsi]""
		#$rootdn = $root.distinguishedName
		$group = [adsi]("LDAP://cn=Domain Admins, cn=Users," + $thisDC)
		$wintelgroup = [adsi]("LDAP://cn=Role-D-3rdLine-Wintel," + $OUAdministrationRoles)
		$members = $group.member
		$group.member = $members + $wintelgroup.distinguishedName
		$group.setinfo()
					
		#ACL Changes
		Add-PSSnapin Quest.ActiveRoles.ADManagement
		Add-QADPermission $AccountsOUDN -Account AllAccountUsers -Rights "ReadProperty,ListObject" -ApplyTo "ThisObjectOnly" | Out-Null
	    Get-QADPermission $AccountsOUDN -account "Authenticated Users" -schemadefault -inherited | Remove-QADPermission | Out-Null
		Add-QADPermission $ResellersOUDN -Account AllResellerUsers -Rights "ReadProperty,ListObject" -ApplyTo "ThisObjectOnly" | Out-Null
	    Get-QADPermission $ResellersOUDN -account "Authenticated Users" -schemadefault -inherited | Remove-QADPermission | Out-Null
					
			
		$AllAccountusers = "CN=AllAccountUsers," + $AccountsOUDN
		$AllResellerusers = "CN=AllResellerUsers," + $ResellersOUDN
		$Role3rdline = "CN=Role-D-3rdLine-Wintel," + $OUAdministrationRoles
		Get-QADPermission -identity $AllAccountusers -account "Authenticated Users","Self"  -schemadefault  | Remove-QADPermission | Out-Null
		Get-QADPermission  -identity $AllResellerusers -account "Authenticated Users","Self"  -schemadefault  | Remove-QADPermission | Out-Null
		Get-QADPermission -identity $Role3rdline -account "Authenticated Users","Self"  -schemadefault  | Remove-QADPermission | Out-Null
					
	    Remove-DSOUInheritence -OUDistinguishedName "CN=Builtin,$thisDC"
			
        Get-QADPermission $("CN=Builtin," + $thisDC) -account "Authenticated Users" -schemadefault -inherited | Remove-QADPermission | Out-Null
	    Remove-DSOUInheritence -OUDistinguishedName "CN=Computers,$thisDC"
			
        Get-QADPermission $("CN=Computers," + $thisDC) -account "Authenticated Users" -schemadefault -inherited | Remove-QADPermission | Out-Null
	    Remove-DSOUInheritence -OUDistinguishedName "OU=Domain Controllers,$thisDC"
			
        Get-QADPermission $("OU=Domain Controllers," + $thisDC) -account "Authenticated Users" -schemadefault -inherited | Remove-QADPermission | Out-Null
	    Remove-DSOUInheritence -OUDistinguishedName "CN=Users,$thisDC"
			
        Get-QADPermission $("CN=Users," + $thisDC) -account "Authenticated Users" -schemadefault -inherited | Remove-QADPermission | Out-Null

		$CSCOU = "OU=CSC," + $thisDC 
		$AdministrationOU = "OU=Admins," + $thisDC
					
        $CSCObjects = Get-ADObject -SearchBase $CSCOU -Filter *
		foreach ($CSCObject in $CSCObjects)
		{
			Get-QADPermission $CSCObject.DistinguishedName -account "Authenticated Users" -schemadefault -inherited | Remove-QADPermission | Out-Null
		}
					
		$AdminObjects = (Get-ADObject -SearchBase $AdministrationOU -Filter *)
		foreach ($AdminObject in $AdminObjects)
		{
			Get-QADPermission $AdminObject.DistinguishedName -account "Authenticated Users" -schemadefault -inherited | Remove-QADPermission | Out-Null
		}
	}
}


# --------------------------------------------------------------------------------------------------
# Schedule the backup task configuration; Enable schedule backup and enable backup maintenance task
# EnableBackupSchedule param in xml is needed to call this function
# --------------------------------------------------------------------------------------------------
Function Set-DSBackupConfiguration([switch] $EnableBackupMaintenance, $BackupVolumePath) {
    Write-DSLog -Message "[DSBackupConfig] Installing Windows Server Backup role"
    Add-WindowsFeature Windows-Server-Backup | Out-Null
    New-Item -Path HKLM:\SYSTEM\CurrentControlSet\Services\wbengine\SystemStateBackup  -Force | Out-Null
	Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\wbengine\SystemStateBackup -Name AllowSSBToAnyVolume -Value 1 -Type DWord | Out-Null
			
	# Creating Backup policy
    Write-DSLog -Message "[DSBackupConfig] Configuring DC System State Backup Schedule"
	$policy = New-WBPolicy 
	Set-WBSchedule –Policy $policy –Schedule 22:00 | Out-Null
	$BackupTargetVolume = New-WBbackupTarget –VolumePath $BackupVolumePath
	Add-WBBackupTarget –Policy $policy –Target $BackupTargetVolume  | Out-Null
	Add-WBSystemState –Policy $policy | Out-Null
	Set-WBPolicy –Policy $policy | Out-Null
	Write-DSLog -Message "[DSBackupConfig] DC System State Backup Schedule created"

    # If user has selected to enable backup maintenance then configure it
	if($EnableBackupMaintenance) {
		Write-DSLog -Message "[DSBackupConfig] Configuring DC System State Backup Maintenance job"
		(schtasks.exe /create /sc DAILY  /TN "System State Backup Cleanup" /NP /F /TR "%windir%\System32\wbadmin.exe delete systemstatebackup -keepVersions:3" /ST 21:00) | Out-Null
	}
	else
	{
		Write-DSLog -Message "[DSBackupConfig] No Configuration of DC System State Backup Maintenance required"
    }
}

# ------------------------------------------------------------------------
# Configure DNS forwarders for DSSOE (requires DNSConfiguration parameter)
# ------------------------------------------------------------------------
Function Set-DSDNSForwarders([string]$ForwardersIP) {				
    if($ForwardersIP -ne "") {
	    [string] $ForwardersIP = $ForwardersIP.split(",")
	    Write-DSLog -Message "[DSForwardersConfig] Creating DNS Forwarders $ForwardersIP"
	    $command = "C:\Windows\System32\dnscmd.exe $(hostname) /resetforwarders $ForwardersIP"
        Write-DSLog -Message "[DSForwardersConfig] Creating DNS Forwarders"	    
        $fwdOut = Invoke-Expression -Command $command 
        Write-DSLog -Message "[DSForwardersConfig] $fwdOut"
    }
}


# -----------------------------------------------
# This function will enable DNS scavenging
# parameter for whether it is first DC of forest
# TODO: check in the caller code to see if this box is ADC. If ADC then this function shouldn't be called
# -----------------------------------------------
function Enable-DNSScavenging([switch] $FirstDCInForest)
{
	# $DNSFQDN = $(hostname) + "." + (Get-ADDomain -Server $(hostname)).dnsroot
    $DNSFQDN = (Get-ADComputer -Identity $env:COMPUTERNAME).DNSHostName
	$DNSroot = (Get-ADDomain -Server  $env:COMPUTERNAME).dnsroot
	$DNSMSDCSroot = "_msdcs." + $DNSroot

	# Enabling Scavenging at zone level
    Write-DSLog -Message "[DNSScavenging] Enabling scavenging at zone level"
	$dnsCmdOut = (DNScmd /config $DNSroot /aging 1)
    Write-DSLog -Message "[DNSScavenging] $dnsCmdOut"
	
	#_Msdcs Scavenging will be enabled on the first DC in forest only
	if($FirstDCInForest) { 
        Write-DSLog -Message "[DNSScavenging] FirstDCInForect is true, enabling _msdcs scavenging "
        $dnsCmdOut = (DNScmd /config $DNSMSDCSroot /aging 1) 
        Write-DSLog -Message "[DNSScavenging] $dnsCmdOut"
    }

	# Getting IP address info of the localmachine 
	$NetworkAdpaters = @(Get-wmiobject -class "Win32_NetworkAdapter" -computername (hostname) | Where {$_.netEnabled -eq "True"})
	for($i = 0;$i -lt $NetworkAdpaters.count; $i++)
	{
		$NetInterface = Get-WmiObject Win32_NetworkAdapterConfiguration | where-object {$_.InterfaceIndex -eq $NetworkAdpaters[$i].InterfaceIndex}
			if(($netinterface.ipaddress -ne $null) -and ($netinterface.ipaddress -notlike "169.254.*"))
			{
			   $ip = $netinterface.IPaddress
			}
		}

	#enabling Scavenging which server will do for the zones
    Write-DSLog -Message "[DNSScavenging] Resetting scavenging for this server"
	$dnsCmdOut = (dnscmd . /zoneresetscavengeservers $DNSroot $ip)
    Write-DSLog -Message "[DNSScavenging] $dnsCmdOut"

	#_Msdcs Scavenging will be enabled on the first DC in forest only
	if ($FirstDCInForest) { 
        Write-DSLog -Message "[DNSScavenging] FirstDCInForect is true, resetting scavenging on _msdcs zone"
        $dnsCmdOut = (dnscmd . /zoneresetscavengeservers $DNSMSDCSroot $ip) 
        Write-DSLog -Message "[DNSScavenging] $dnsCmdOut"
    }
	
	#enabling Scavenging at server level
    Write-DSLog -Message "[DNSScavenging] Enabling scavenging at server level"
	$dnsCmdOut = (dnscmd . /config /defaultagingstate 1 )
    Write-DSLog -Message "[DNSScavenging] $dnsCmdOut"
	$dnsCmdOut = (dnscmd . /config /scavenginginterval 168)
    Write-DSLog -Message "[DNSScavenging] $dnsCmdOut"
}


# --------------------------------------------------------------------------------------------------
# This function will verify required AD registry/services to flag as success/failure of installation
# --------------------------------------------------------------------------------------------------
function Test-DSSetup {
	#$LogFile = "C:\DSVerificationLog-" + (Get-Date).ToString("MMddyyyy-hhmmss") + ".txt"
	
    #Checking Existence of 29223 event ID 
	$elog = Get-EventLog -LogName System -ComputerName $env:COMPUTERNAME -InstanceId 29223 -ErrorAction SilentlyContinue
	if ($elog -ne $null) { 
        $event29223 = $true
		Write-DSLog -Message "[TestDSSetup] Event ID 29223 - This server is a Domain Controller" 
	}
	else { 
        Write-DSLog -Message "[TestDSSetup] Event ID 29223 not found - This server is NOT a Domain Controller"
        $event29223 = $false 
    }
	
	#Checking SYSVOL ready entry
	if (((Get-ItemProperty -Path HKLM:\System\CurrentControlSet\Services\Netlogon\Parameters -name Sysvolready).Sysvolready) -eq 1) {
		$SYSVOLready = $true
		Write-DSLog -Message "[TestDSSetup] SysvolReady - SYSVOL is ready"
	}
	else { 
        Write-DSLog -Message "[TestDSSetup] SYSVOL is NOT ready"
        $SYSVOLready = $false 
    }
	
	#Checking SYSVOL share
	if ((((Get-WmiObject win32_share -computer $env:COMPUTERNAME | Where-Object {$_.name -eq "sysvol"}).name)).tolower() -eq "sysvol") {
		$SYSVOLshare = $true
		Write-DSLog -Message "[TestDSSetup] SysvolShare - SYSVOL is shared"
	}
	else { 
        Write-DSLog -Message "[TestDSSetup] SYSVOL is NOT shared"
        $SYSVOLshare = $false 
    }
	
	#Checking NETLOGON share
	if (((Get-WmiObject win32_share -computer $env:COMPUTERNAME | Where-Object {$_.name -eq "netlogon"}).name).tolower() -eq "netlogon") {
		$netlogonshare = $true
		Write-DSLog -Message "[TestDSSetup] NetlogonShare - Netlogon is shared"
	}
	else { 
        Write-DSLog -Message "[TestDSSetup] Netlogon is NOT shared"
        $netlogonshare = $false 
    }
	
	#Checking ADWS
	[string] $ADWSEvent = (Get-EventLog -LogName "Active Directory Web Services" -ComputerName $env:COMPUTERNAME -Source "ADWS" -InstanceId 1073743024)
	if ($ADWSEvent -eq "") { 
        Write-DSLog -Message "[TestDSSetup] ADWS - Active Directory Web Services is NOT installed"
        $event1200 = $False 
    }
	else { 
        $event1200 = $true
		Write-DSLog -Message "[TestDSSetup] ADWS - Active Directory Web Services is installed" 
	}

    # return current status of Sysvol share and Netlogon share
	if($SYSVOLshare -eq $true -and $netlogonshare -eq $true -and $event29223 -eq $true -and $SYSVOLready -eq $true) {
		return $true
	}
	else { 
        Write-DSLog -Message "[TestDSSetup] Not all required tests were successful"
        return $false 
    }
}

#-------------------------------------------------------------------------------------
# This Funciton will configure the DS Recovery Mode option, available at the boot time
#-------------------------------------------------------------------------------------
Function Set-DSRepairMode { 
    Write-DSLog -Message "[DSRepairMode] Configuring DS repair mode"
    $CopyBCD = bcdedit /copy '{current}' /d "Directory Services Restore Mode"
	$guid = $CopyBCD.Substring($CopyBCD.IndexOf("{"),$copybcd.indexof("}") - $copybcd.indexof("{") + 1)
	$bcdOut = bcdedit /set $guid safeboot dsrepair
    Write-DSLog -Message "[DSRepairMode] $bcdOut"
    Write-DSLog -Message "[DSRepairMode] Directory Services Repair Mode configuration complete."
}

 
#------------------------------------------------------------------------------
# Function to Add DFS Tools feature for management of SYSVOL folder replication
#------------------------------------------------------------------------------
Function Add-DFSTools {
    Add-WindowsFeature RSAT-DFS-Mgmt-Con | Out-Null
    Write-DSLog -Message "[DFSTools] DFS Tools feature for SYSVOL folder replication management are added."
}
 
#--------------------------------------------------------------------------------
# This function will record the AD Server configuration in a user given text file
#--------------------------------------------------------------------------------
Function Save-ADServerConfig($ADConfigFilePath) {
	$DomainInfo = Get-ADDomain -Server ($env:COMPUTERNAME)
	$IPdetails = (gwmi Win32_NetworkAdapterConfiguration | ? { $_.IPAddress -ne $null })
	$RecordAD = "
	Directory Services Server Configuration`n`r`n`r`n`r`n`r
	Configuration Date and Time = $(Get-date)`n`r
	Domain DNS Root             = $($DomainInfo.DNSRoot)`n`r
	DistinguishedName           = $($DomainInfo.DistinguishedName)`n`r
	NetBIOSName                 = $($DomainInfo.NetBIOSName)`n`r
	DomainMode                  = $($DomainInfo.DomainMode)`n`r
	Domain Controller Name      = $(hostname)`n`r
	IPAddress                   = $($IPdetails.IPAddress)`n`r
	SubNet Mask                 = $($IPdetails.IPSubnet)`n`r
	GateWay                     = $($IPdetails.DefaultIPGateway)`n`r
	DNS Address                 = $($IPdetails.DNSServerSearchOrder)`n`r
	OperatingSystem             = $((Get-ADDomainController).OperatingSystem)`n`r
	OperatingSystemServicePack  = $((Get-ADDomainController).OperatingSystemServicePack)`n`r
	OperationMasterRoles        = $((Get-ADDomainController).OperationMasterRoles)`n`r
	Database file               = $((Get-ItemProperty -Path registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\NTDS\Parameters -Name "DSA Database file")."DSA Database file")`n`r" 
	New-Item $ADConfigFilePath -type file -force | Out-Null
	Set-Content -Path $ADConfigFilePath  $RecordAD

	Write-DSLog -Message "[DSServerConfiguration] Configuration of AD Server is saved in the Text File : $ADConfigFilePath"
}

 
# ----------------------------------------------------------------------------------------------------------------------------------------------------
# This function installs a security update to cater to the Group policy preference vulnerability defined in MS security bulletin MS14-025
# ----------------------------------------------------------------------------------------------------------------------------------------------------
Function Install-DSHotfix {
    #Determining the OS version
    $osver = (Get-WmiObject Win32_OperatingSystem).caption

	if (($osver.substring(0,32) -eq "Microsoft Windows Server 2012 R2") -eq $true)
	{
        #Determining if KB2919355 is already installed or not		
		Get-HotFix -Id "KB2919355" | Out-Null
        if($? -eq $false)
		{ 
            Write-DSLog -Message "[HotFix Installation] Pre-requisite security update KB2919355 is not installed"  
        }
		Else
		{
            # Store hotfix path in a variable
            $HotFixFolderPath = "$PSScriptRoot\Hotfix"

            #list of all hotfixes
		    $HotFixes = Get-ChildItem -Path $HotFixFolderPath -Include *.msu -Recurse 

            # enumerate through all hotfixes and install them after verifying that they're not already installed
		    foreach($HF in $HotFixes) {
                #replace extension with blank in hotfix name, to make it hotfix id
                $HFID = $HF.Name -replace $HF.Extension,'' 

               if((Get-HotFix -Id $HFID -ErrorAction SilentlyContinue)) {
       			    Write-DSLog -Message "[HotFix Installation] Security update $HFID is already installed, thus skiping it."
                }	
    	       else { 
                # The hotfix is not installed, installing the hofix.
                Write-DSLog -Message "[HotFix Installation] Installing security update $HFID."
                Start-Process -FilePath WUSA.EXE -ArgumentList "$($hf.FullName) /quiet /norestart" -Wait                                
				}
			}
            Write-DSLog -Message "[HotFix Installation] Hotfix installation routine is finished running"  
		}
     }
 }   


# ---------------------------------------------------------------------------
# This function will generate BPA report and save to html file at given path
# ---------------------------------------------------------------------------
Function New-DSBPAReport($HTMLReportPath, [switch]$ReportAllSevereties) {
    # Getting a list of models to generate report for
    $ModelsToRun = @() 
    Write-DSLog -Message "[DSBPAReport] Identifying BPA models to run"
    if ((Get-WindowsFeature AD-Domain-Services).Installed) { $ModelsToRun += "Microsoft/Windows/DirectoryServices" } 
    if ((Get-WindowsFeature DNS).Installed) { $ModelsToRun += "Microsoft/Windows/DNSServer" } 
    if ((Get-WindowsFeature File-Services).Installed) { $ModelsToRun += "Microsoft/Windows/FileServices" } 

    # Log which all severity will be included in the report
    if ($ReportAllSevereties) { 
        Write-DSLog -Message "[DSBPAReport] All severity will be included in the report"
    }
    else {
        Write-DSLog -Message "[DSBPAReport] Only errors and warnings will be included in the report"
    }

    # enumerate thru the models and generate reports for them
    foreach ($BestPracticesModelId in $ModelsToRun) { 
        # construct BPA name and html paths 
        $date = (Get-Date).ToString("MMddyyyy-hhmmss")
        $BPAName = $BestPracticesModelId.Replace("Microsoft/Windows/","") 
        $HTMLPath = $HTMLReportPath + '\' + $env:COMPUTERNAME + "-" + $BPAName + "-" + $date + ".html" 

        #HTML-header 
        $Head = " 
        <title>BPA Report for $BestPracticesModelId on $($env:COMPUTERNAME)</title> 
        <style type='text/css'>  
            table  { border-collapse: collapse; width: 700px }  
            body   { font-family: Verdana, Arial }  
            td, th { border-width: 1px; border-style: solid; text-align: left; padding: 2px 4px; border-color: black }  
            th     { background-color: grey; }  
            td.Red { color: Red }  
        </style>"  

        #Invoke BPA Model 
        Write-DSLog -Message "[DSBPAReport] Invoking BPA report generation for model $BestPracticesModelId"
        Invoke-BpaModel -BestPracticesModelId $BestPracticesModelId | Out-Null 

        #Include all severeties in BPA Report if enabled. If not, only errors and warnings are reported. 
        if ($ReportAllSevereties) { 
            $BPAResults = Get-BpaResult -BestPracticesModelId $BestPracticesModelId 
        } 
        else { 
            $BPAResults = Get-BpaResult -BestPracticesModelId $BestPracticesModelId | Where-Object {$_.Severity -eq "Error" -or $_.Severity -eq “Warning” } 
        } 

        #Send BPA Results to HTML-file if enabled 
        if ($BPAResults) { 
            $BPAResults | ConvertTo-Html -Property Severity,Category,Title,Problem,Impact,Resolution,Help `
            -Title "BPA Report for $BestPracticesModelId on $($env:COMPUTERNAME)" -Body "BPA Report for `
            $BestPracticesModelId on server $($env:COMPUTERNAME) <HR>" -Head $head | Out-File -FilePath $HTMLPath 
            Write-DSLog -Message "[DSBPAReport] Report is saved to $HTMLPath"
        } 
    } 
} 

# --------------------------------------------------------
# function to return whether current user is admin or not
# --------------------------------------------------------
Function Test-IsCurrentUserAdmin {
    $myIdentity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $wp = New-Object Security.Principal.WindowsPrincipal($myIdentity)
    if (-not $wp.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)) {
        return $false # current user has no administrative rights
    }
    else { return $true } # current user is admin
}

