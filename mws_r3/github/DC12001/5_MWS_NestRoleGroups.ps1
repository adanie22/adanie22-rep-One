<#
.SYNOPSIS
Add global ROLE groups from the Broker domain
to domain local PERM groups in the resource domain

.DESCRIPTION
The script prompts for an input file mapping global role groups
for various CSC support teams in the Broker domain to
permissions groups in the Resource domain
and and nests the global groups into the local groups

The script is to be executed in the MWS Resource domain with a user account
with rights to modify group membership in the Resource domain permissions groups
and at least read access to the Broker domain

#>

### Enable error handling by treating all errors as "Terminating"
$ErrorActionPreference = "Stop"

### Define Input & Log files


### $Inputfile = ".\MWS_GroupNesting.csv"
$Inputfile = ".\test2.csv"



$Logfile = ".\MWS_NestGroupsLog.log"
$Blankline = "`r`n"
$Delim = ";"

write-host $Blankline
write-host "Reading input file: " $Inputfile "..."

$FileExists = test-path $Inputfile
if ($FileExists -eq $false)
{
write-host "**ERROR: Cannot locate file $Inputfile"
EXIT
}

### get the naming context of the domain
import-module activedirectory
add-PSSnapin Quest.ActiveRoles.ADManagement

$ADRootDSE = Get-ADRootDSE -Properties *

### Log the script execution date and time
$DateRun = Get-date -format g
$Blankline + "------------------------------------------" | Out-File $Logfile -append
$DateRun | Out-File $Logfile -append
"------------------------------------------" | Out-File $Logfile -append

$grouplist = import-csv $Inputfile -delimiter $Delim
foreach ($groupline in $grouplist){
	$permgroup = $groupline.Permgroup
	$rolegroup = $groupline.Rolegroup

	"Adding " + $rolegroup + " to " + $permgroup  | Out-File $Logfile -append

	Add-QADgroupmember $permgroup $rolegroup | Out-File $Logfile -append

}

write-host "Script complete. Results logged to file: " $Logfile
