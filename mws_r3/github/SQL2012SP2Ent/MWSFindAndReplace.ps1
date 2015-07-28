#########################################################################
# Author: Stiven Skoklevski, CSC
# Find and replace text within files.
# Used to replace IP addresses
#########################################################################

# find and replace text

$readOnly = $false

# eg c:\users\skoklevski
$pathToSearch = "C:\Users\sskoklev"

# CDC IP
$findString = "10.5.7."

# PDC IP
$replaceString = "10.3.41."

$files = Get-ChildItem -Path $pathToSearch -Include "*.ps1", "*.ini", "*.xml" -Recurse
foreach ($file in $files)
{
    if ($file.Directory -like "*InstallMedia_SQL*")
    {
        continue
    }

    if ($file.Directory -like "*SXS*")
    {
        continue
    }

    if ($file.Directory -like "*TestFramework*")
    {
        continue
    }

    if ($readOnly -eq $false)
    {
        (Get-Content $file.PSPath) | 
            Foreach-Object {$_ -replace $findString, $replaceString} | 
            Set-Content $file.PSPath
    }

     Write-Host $file.PSPath
}


