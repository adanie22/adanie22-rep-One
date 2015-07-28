Import-Module ActiveDirectory

$AuditRuleFileName = "AuditRules.clixml"

#NOTE. The assumption is that this listing is definitive apart from the core sites.
If (!(Test-Path -Path $AuditRuleFileName)) {
    Write-Error "Unable to locate $($AuditRuleFileName)"
    exit
}


$DomainRootName = (Get-ADRootDSE).rootDomainNamingContext
$CurrentACL = Get-ACL -Audit -Path "AD:\$DomainRootName"
$ImportedAuditRules = Import-clixml $AuditRuleFileName

$EveryoneIdentity = New-Object System.Security.Principal.SecurityIdentifier "S-1-1-0"
Foreach ($ImportedAuditRule in $ImportedAuditRules) {
	If ($ImportedAuditRule.IdentityReference.Value -eq $EveryoneIdentity.Value) {
		Write-Host "Adding a rule"
		$NewAuditRule = New-Object System.DirectoryServices.ActiveDirectoryAuditRule($EveryoneIdentity,$ImportedAuditRule.ActiveDirectoryRights,$ImportedAuditRule.AuditFlags,$ImportedAuditRule.ObjectType,$ImportedAuditRule.InheritanceType,$ImportedAuditRule.InheritedObjectType)
		$CurrentACL.AddAuditRule($NewAuditRule)
	}
}
Write-Host "Applying updated ACL to top level of domain"
Set-ACL -AclObject $CurrentACL -Path "AD:\$DomainRootName"

