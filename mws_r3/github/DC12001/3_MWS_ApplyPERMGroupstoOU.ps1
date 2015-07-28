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
$OUInputfile = ".\MWS_OUimport.csv"
$Delim = ";"
$OUPermgroupFile = ".\MWS_PERMGroups_perOU.csv"
$UniquePermFile = ".\MWS_PERMGroups_unique.csv"
$Logfile = ".\MWS_PERMGroups_AppliedtoOU_Log.log"
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
###	for user,computer, and group objects
### file format: 
### ObjectType,BaseGroupName,Scope,Description
$PermgroupFile = import-csv $OUPermgroupFile

### Import the CSV file containing the standard MWS OUs
### Note: delimiter is a semicolon
### file format: child_OU;parent_OU_RDN;delegationTarget
$OUlist = import-csv $OUInputfile -delimiter $Delim

### Loop through each standard MWS OU
### Read the delegation target: user,computer,group, or other specified target
### delegationTarget indicates the type of object in the OU to be delegated


foreach ($OUline in $OUlist){
	$childOU = $OUline.child_OU
	if ($OUline.parent_OU_RDN -eq "")
	{ 
	$parentOU = $ADRootDSE.defaultNamingContext 
	}
	else
	{$parentOU = $OUline.parent_OU_RDN  + "," + $ADRootDSE.defaultNamingContext
	}

	$TargetOU = "OU=" + $childOU + "," + $parentOU
#	$Blankline | Out-File $Logfile -Append
	$Blankline + "------- Processing " + $TargetOU | Out-File $Logfile -Append


### Append the OU name to the BaseGroupName to generate the final group name
### Example:  	PERM-D-MWS-DeleteComp for OU "Servers" will become:
###  		PERM-D-MWS-DeleteComp-Servers
### If group exists, to apply the ACLs to the appropriate OU using/QAS commands


	foreach ($PERMgroupline in $PermgroupFile)
	{
	   if ($PERMgroupline.ObjectType -eq $OUline.delegationTarget) 
	   {
	   $PERMgroup = $PERMgroupline.BaseGroupName + "-" + $childOU

   	   "Processing group " + $PERMgroup
	
	   Try 
	   {
	   Get-ADgroup $PERMgroup
#	   "Group exists: " + $PERMgroup | Out-File $Logfile -Append

    	   $Accountname = $domainNetbios + "\" + $PERMgroup
  	   "Applying rights for Group: " + $PERMgroup | Out-File $Logfile -Append

  	   	switch ($OUline.delegationTarget)
   		{

		"user" {

			switch -wildcard ($PERMgroup)
			{
			"*CreateUser*" 	{
					Add-QADPermission $TargetOU -Account $AccountName -Rights "CreateChild" -ChildType "user" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					}

			"*DeleteUser*" 	{
					Add-QADPermission $TargetOU -Account $AccountName -Rights "DeleteChild" -ChildType "user" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "GenericAll" -ApplyTo "ChildObjects" -ApplyToType "User" | Out-File $Logfile -append		
					}

			"*FullControlUser*" {
					Add-QADPermission $TargetOU -Account $AccountName -Rights "CreateChild" -ChildType "user" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "DeleteChild" -ChildType "user" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "GenericAll" -ApplyTo "ChildObjects" -ApplyToType "User" | Out-File $Logfile -append					
					}

			"*UnlockUserAcct*" {
					Add-QADPermission $TargetOU -Account $AccountName -ExtendedRight "User-Change-Password" -ApplyTo "ChildObjects" -ApplyToType "User" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -ExtendedRight "User-Force-Change-Password" -ApplyTo "ChildObjects" -ApplyToType "User" | Out-File $Logfile -append					
					}
			}

			# bracket: "user"
			}
				
		"computer" {

			switch -wildcard ($PERMgroup)
			{
			"*CreateComp*" 	{
					add-QADPermission $TargetOU -Account $AccountName -Rights "CreateChild" -ChildType "computer" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					}

			"*DeleteComp*" 	{
					Add-QADPermission $TargetOU -Account $AccountName -Rights "DeleteChild" -ChildType "computer" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "GenericAll" -ApplyTo "ChildObjects" -ApplyToType "computer" | Out-File $Logfile -append		
					}

			"*FullControlComp*" {
					Add-QADPermission $TargetOU -Account $AccountName -Rights "CreateChild" -ChildType "computer" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "DeleteChild" -ChildType "computer" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "GenericAll" -ApplyTo "ChildObjects" -ApplyToType "computer" | Out-File $Logfile -append					
					}

			}

			# bracket: "computer"
			}
					
		"group" {

			switch -wildcard ($PERMgroup)
			{
			"*Creategroup*" {
					Add-QADPermission $TargetOU -Account $AccountName -Rights "CreateChild" -ChildType "group" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					}

			"*Deletegroup*" {
					Add-QADPermission $TargetOU -Account $AccountName -Rights "DeleteChild" -ChildType "group" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "GenericAll" -ApplyTo "ChildObjects" -ApplyToType "group" | Out-File $Logfile -append		
					}

			"*FullControlGroup*" {
					Add-QADPermission $TargetOU -Account $AccountName -Rights "CreateChild" -ChildType "group" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "DeleteChild" -ChildType "group" -ApplyTo "ThisObjectAndImmediateChildObjects" -ApplyToType "organizationalUnit" | Out-File $Logfile -append
					Add-QADPermission $TargetOU -Account $AccountName -Rights "GenericAll" -ApplyTo "ChildObjects" -ApplyToType "group" | Out-File $Logfile -append					
					}

			"*GroupMembers*" {
					Add-QADPermission $TargetOU -Account $AccountName -Rights "ReadProperty,WriteProperty" -Property "member" -ApplyToType "group" | Out-File $Logfile -append
					}
			}

			# bracket: "group"
			}

		default {"NO ACTION !"  | Out-File $Logfile -Append}
		
		# bracket: switch ($OUline.delegationTarget)
		}

	   # bracket: Try...
	   }	
	   Catch
	   {
	 	"**ERROR: Unable to find Group: " +  $PERMgroup | Out-File $Logfile -Append
	   }

	# end if block ...
	}

	
	# bracket: foreach ($PERMgroupline ...
	}

# bracket: foreach ($OUline in $OUlist)
}

write-host $Blankline "Script completed. Results logged to " $Logfile

