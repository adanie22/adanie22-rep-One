# get current script location
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$scriptName = $MyInvocation.MyCommand.Name

. .\LoggingV2.ps1 $true $scriptPath $scriptName


# . .\Install\ApplySCCMRequirements.ps1 $scriptPath "demo3\agilitydeploy"

. .\ExecuteApplySCCMRequirements.ps1 # $scriptPath "demo3\agilitydeploy"


