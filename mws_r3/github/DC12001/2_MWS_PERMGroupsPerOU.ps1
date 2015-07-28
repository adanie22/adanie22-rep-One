<#
.SYNOPSIS
Generate AD delegation groups for a standard MWS domain structure
based on CSC naming standards and OU names

.DESCRIPTION
The script reads an input CSV file and uses it to generate standard OUs
The output file is identified in the script

CSC
MyWorkStyles
Sujit Trivedi

#>


### Define Input & Log files
$Inputfile = ".\MWS_OUimport.csv"
$Delim = ";"
$OUPermgroupFile = ".\MWS_PERMGroups_perOU.csv"
$UniquePermFile = ".\MWS_PERMGroups_unique.csv"
$Logfile = ".\MWS_PERMGroups_perOU_Log.log"
$Blankline = "`r`n"

### Enable error handling by treating all errors as "Terminating"
$ErrorActionPreference = "Stop"

### import AD and QAS modules
import-module activedirectory
add-PSSnapin Quest.ActiveRoles.ADManagement

$ADRootDSE = Get-ADRootDSE -Properties *
$ADRootDSE.defaultNamingContext

$domain=Get-ADdomain -Current LocalComputer
$domainNetbios = $domain.netbiosname

### Log the script execution date and time
$DateRun = Get-date -format g
$Blankline + "------------------------------------------" | Out-File $Logfile -append
$DateRun | Out-File $Logfile -append
"------------------------------------------" | Out-File $Logfile -append


### For each OU in the domain, create a number of permission (PERM) groups
###  for delegation of AD rights to that OU

### The PERM groups will be created in the "Permissions" OU.
$PERMOU=Get-ADOrganizationalUnit -filter 'Name -eq "Permissions"' –properties *
$PERMOU = $PERMOU.DistinguishedName

### Import the CSV files containing the PERM groups mapping to AD rights
###	for unique cases or catch-all instances
### file format: 
### BaseGroupName,Scope,Description
$PermgroupFile = import-csv $UniquePermFile

foreach ($PERMgroupline in $PermgroupFile)
{
	$PERMgroup = $PERMgroupline.BaseGroupName
	"Processing group " + $PERMgroup

	Try 
	{
	Get-ADgroup $PERMgroup
	"Group already exists: " + $PERMgroup | Out-File $Logfile -Append
	}
	Catch
	{
	"Attempting to create Group: " + $PERMgroup | Out-File $Logfile -Append
		Try
		{
		New-ADGroup -Name $PERMgroup -Path $PermOU -GroupScope $PERMgroupline.Scope -Description $PERMgroupline.Description
		"Successfully created Group: " + $PERMgroup | Out-File $Logfile -Append

# "Applying rights for Group: " + $PERMgroup | Out-File $Logfile -Append
		}
		Catch
		{
		"**ERROR: Unable to create Group: " +  $PERMgroup | Out-File $Logfile -Append
		}
	}
# bracket: foreach ($PERMgroupline ...
}

$Blankline | Out-File $Logfile -Append


### Import the CSV files containing the PERM groups mapping to AD rights
###	for user,computer, and group objects
### file format: 
### ObjectType,BaseGroupName,Scope,Description
$PermgroupFile = import-csv $OUPermgroupFile

### Import the CSV file containing the standard MWS OUs
### file format: child_OU;parent_OU_RDN;delegationTarget
$OUlist = import-csv $Inputfile -delimiter $Delim

### Loop through each standard MWS OU
### Read the delegation target: user,computer,group, or other specified target
### If the OU is targeted for delegation,
### Append the OU name to the BaseGroupName to generate the final group name
### Example:  	PERM-D-MWS-DeleteComp for OU "Servers" will become:
###  		PERM-D-MWS-DeleteComp-Servers
### If group does not exist, create it
### Use the Quest commands to apply the ACLs to the appropriate OU

foreach ($OUline in $OUlist){
	$childOU = $OUline.child_OU
	if ($OUline.parent_OU_RDN -eq "")
	{
		$parentOU = $ADRootDSE.defaultNamingContext
	}
	else
	{
		$parentOU = $OUline.parent_OU_RDN  + "," + $ADRootDSE.defaultNamingContext
	}
	"Processing OU: " + $childOU | Out-File $Logfile -Append


	foreach ($PERMgroupline in $PermgroupFile){

		if ($PERMgroupline.ObjectType -eq $OUline.delegationTarget) 
		{
		$PERMgroup = $PERMgroupline.BaseGroupName + "-" + $childOU
		"Processing group " + $PERMgroup

			Try 
			{
			Get-ADgroup $PERMgroup
			"Group already exists: " + $PERMgroup | Out-File $Logfile -Append
			}
			Catch
			{
			"Attempting to create Group: " + $PERMgroup | Out-File $Logfile -Append
				Try
				{
				New-ADGroup -Name $PERMgroup -Path $PermOU -GroupScope $PERMgroupline.Scope -Description $PERMgroupline.Description
				"Successfully created Group: " + $PERMgroup | Out-File $Logfile -Append

# "Applying rights for Group: " + $PERMgroup | Out-File $Logfile -Append

				}
				Catch
				{
				"**ERROR: Unable to create Group: " +  $PERMgroup | Out-File $Logfile -Append
				}
			}
		# bracket: if ($PERMgroupline.ObjectType...
		}

	# bracket: foreach ($PERMgroupline ...
	}

	$Blankline | Out-File $Logfile -Append

# bracket: foreach ($OUline in $OUlist)
}

write-host $Blankline "Script completed. Results logged to " $Logfile

