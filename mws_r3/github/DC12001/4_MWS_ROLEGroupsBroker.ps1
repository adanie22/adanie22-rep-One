<#
.SYNOPSIS
Generate AD delegation role groups for a standard MWS Broker domain structure

.DESCRIPTION
The script reads an input file and uses it to generate standard role groups
for various CSC support teams in the Broker domain

The script is to be executed in the MWS Resource domain with a user account
with rights to create groups in the Broker domain Roles OUs

CSC
MyWorkStyles
Sujit Trivedi

#>


### Define Input & Log files
$Delim = ";"
$RoleFile = ".\MWS_RoleGroups_Broker.csv"
$Logfile = ".\MWS_RoleGroups_BrokerLog.log"
$Blankline = "`r`n"

### Enable error handling by treating all errors as "Terminating"
$ErrorActionPreference = "Stop"

### import AD and QAS modules
import-module activedirectory
add-PSSnapin Quest.ActiveRoles.ADManagement

$ADRootDSE = Get-ADRootDSE -Properties *
### $ADRootDSE.defaultNamingContext

$domain=Get-ADdomain -Current LocalComputer
$domainNetbios = $domain.netbiosname

### Log the script execution date and time
$DateRun = Get-date -format g
$Blankline + "------------------------------------------" | Out-File $Logfile -append
$DateRun | Out-File $Logfile -append
"------------------------------------------" | Out-File $Logfile -append

write-host $blankline
### Confirm path for creation of ROLE groups:
$ROLEOU = "ou=roles,ou=administration,ou=global,dc=sitmws,dc=cscmws,dc=com"

write-host "The Role groups will be created in the following OU:"
$ROLEOU
write-host $blankline
write-host "If this is correct, press ENTER to continue."
$response = read-host "Any other response will terminate the script"
if ($response -ne "") {EXIT}


### Import the CSV files containing the ROLE groups mapping to app & support teams
###	for unique cases or catch-all instances
### file format: 
### BaseGroupName,Scope,Description
$ROLEgroupFile = import-csv $ROLEFile

foreach ($ROLEgroupline in $ROLEgroupFile)
{
	$ROLEgroup = $ROLEgroupline.BaseGroupName
	"Processing group " + $ROLEgroup

	Try 
	{
	Get-ADgroup $ROLEgroup
	"Group already exists: " + $ROLEgroup | Out-File $Logfile -Append
	}
	Catch
	{
	"Attempting to create Group: " + $ROLEgroup | Out-File $Logfile -Append
		Try
		{
		New-ADGroup -Name $ROLEgroup -Path $ROLEOU -GroupScope $ROLEgroupline.Scope -Description $ROLEgroupline.Description
		"Successfully created Group: " + $ROLEgroup | Out-File $Logfile -Append

		}
		Catch
		{
		"**ERROR: Unable to create Group: " +  $ROLEgroup | Out-File $Logfile -Append
		}
	}
# bracket: foreach ($ROLEgroupline ...
}

$Blankline | Out-File $Logfile -Append


write-host $Blankline "Script completed. Results logged to " $Logfile

