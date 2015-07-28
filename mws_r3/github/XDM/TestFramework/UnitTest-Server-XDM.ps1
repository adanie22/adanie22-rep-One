Param(
    [string] $scriptPath,
    [string] $testFolder
)

############################################################################################
# Author:Stiven Skoklevski
# Desc: Server side Unit testing - XDM  
############################################################################################

function get-WindowsServices()
{
    $services = @()
    log "INFO: about to call get-winServices"
    $services += get-Services @("CloudStorageUploaderSvc", "SCFileCleanSvc", "SCFileCopySvc", "S3Uploader")

    $winS = Build-HTML-Fragment ($services) TABLE "<h2>ShareFile Windows Services</h2>" 

    Write-Output $winS
}

function get-ShareFileWebPage()
{
    $url = "http://XDMIPAddress/zdm "
    $searchString = "Errors"
    $webPageExists = Check-WebPage $url $searchString

    try
    {
        $objects = @()
        $object = New-Object -TypeName PSObject
        $object | Add-Member -Name 'Url' -MemberType Noteproperty -Value $url
        $object | Add-Member -Name 'Search String' -MemberType Noteproperty -Value $searchString
        $object | Add-Member -Name 'Found Errors?' -MemberType Noteproperty -Value (AddColor $webPageExists "True" "Red")  

        $objects += $object
    }
    catch
    {
        $objects = get-Exception $($_.Exception.Message)      
    }
    finally
    {
        Write-Output $objects
    }

}

############################################################################################
# Main
############################################################################################
# \USER_PROFILE
#        \TestResults\SERVER-PRODUCT.html

log "INFO: Script path $scriptPath"
Set-Location -Path $scriptPath 

try
{
    $product = "XDM"
    <#
    . .\LoggingV2.ps1 $true $scriptPath "unitTest-Server-$product.ps1"
    . "$scriptPath\$testFolder\HTMLGenerator.ps1"
    . "$scriptPath\$testFolder\UnitTest-Common-Utilities.ps1"

    $dtStart =  get-date

    log "INFO: about to call getShareFileWebPage"
    $shareFilePage = Build-HTML-Fragment (get-ShareFileWebPage) LIST "<h2>XDM About Web Page</h2>"
    
    # log "INFO: about to call get-WindowsServices"
    # $winServices = get-WindowsServices

 #   $content = "$shareFilePage `
 #               $winServices "

    $content = "$shareFilePage"

    Build-HTML-UnitTestResults $content $dtStart $product "$scriptPath\$testFolder"
    #>


    exit 0
}
catch
{
    log "ERROR: $($_.Exception.Message)"

    # This is done to get an error code from the scheduled task.
    Write-Output  $($_.Exception.Message) | Out-File "$scriptPath\error.txt" -Append
    exit -1
}