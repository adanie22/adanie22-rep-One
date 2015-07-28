

$Url = "https://10.5.4.40/svn/MWS2/Generic/branches/Current"

$engineers = "adelpopo, `
              ctamondong2, `
              dhatchett, `
              dridley, `
              gpetrides, `
              mkrynina, `
              mfreeman, `
              rsmith272, `
              rsparkes, `
              sskoklevski"



$rootPath = "C:\SVN\"
$username = "demo3\agilitydeploy"
$password = "M3sh@dmin!"

foreach($engineer in $engineers.split(',').Trim())
{
    $Path = "$rootPath$engineer"
    # $svnCommand = "svn.exe checkout $Url $Path --no-auth-cache --non-interactive --username `"$username`" --password `"$Password`""
    $svnCommand = "svn.exe checkout $Url $Path "
    write-host $svnCommand -ForegroundColor Yellow
    Invoke-Expression $svnCommand
    # $log = Invoke-Expression "svn.exe checkout $Url $Path --no-auth-cache --non-interactive --username `"$username`" --password `"$Password`""
}

write-host "End SVN set up" -ForegroundColor Yellow

# need to ensure eveyone has read\write to the folders otherwise TSVN commands will fail
write-host "Assign Read/Write Folder permission to Everyones" -ForegroundColor Yellow

$FilesAndFolders = gci "c:\SVN" -recurse | % {$_.FullName}
foreach($FileAndFolder in $FilesAndFolders)
{
    #using get-item instead because some of the folders have '[' or ']' character and Powershell throws exception trying to do a get-acl or set-acl on them.
    $item = gi -literalpath $FileAndFolder 
    $acl = $item.GetAccessControl() 
    $permission = "Everyone","FullControl","Allow"
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($rule)
    $item.SetAccessControl($acl)
}

write-host "End Assign Read/Write Folder permission to Everyones" -ForegroundColor Yellow




