<#
.SYNOPSIS
Creates OU structure to reflect standard MWS OU structure per Architecture docs

.DESCRIPTION
The script reads a CSV file and uses it to generate standard OUs
The input is a CSV file. The output file is identified in the script

#>

### Enable error handling by treating all errors as "Terminating"
$ErrorActionPreference = "Stop"

### Define Input & Log files
$Inputfile = ".\MWS_OUimport.csv"
$Delim = ";"
$Logfile = ".\MWS_OUimportLog.log"
$Blankline = "`r`n"


### get the naming context of the domain
import-module activedirectory
$ADRootDSE = Get-ADRootDSE -Properties *

### Log the script execution date and time
$DateRun = Get-date -format g
$Blankline + "------------------------------------------" | Out-File $Logfile -append
$DateRun | Out-File $Logfile -append
"------------------------------------------" | Out-File $Logfile -append


### Import csv file with OU names into child and parent OU
### The child & parent fields are separated by semicolon ";"
### Append the domain DN to the path to the OU names
$NewOUlist = import-csv $Inputfile -delimiter $Delim
foreach ($OUline in $NewOUlist){
	$childOU = $OUline.child_OU
	$parentOU = $OUline.parent_OU_RDN
	if ($parentOU -eq "")
	{
		$parentOU = $ADRootDSE.defaultNamingContext
	}
	else
	{
		$parentOU = $parentOU  + "," + $ADRootDSE.defaultNamingContext
	}

### If child OU does not exist, create it
	"Processing OU=" + $childOU + "," + $parentOU | Out-File $Logfile -Append
	Try 
	{
		Get-ADOrganizationalUnit -Identity $childOU + "," + $parentOU
		"OU already exists: OU=" + $childOU + "," + $parentOU | Out-File $Logfile -Append
	}
	Catch
	{
		"Attempting to create OU: OU=" + $childOU + "," + $parentOU | Out-File $Logfile -Append
		Try
		{
			New-ADOrganizationalUnit -Name $ChildOU -Path $ParentOU
			"Successfully created OU: OU=" + $childOU + "," + $parentOU | Out-File $Logfile -Append
		}
		Catch
		{
			"**ERROR: Unable to create OU: OU=" +  $childOU + "," + $parentOU | Out-File $Logfile -Append
		}
	}
	$Blankline | Out-File $Logfile -Append
}

write-host "Script complete. Results logged to file: " $Logfile