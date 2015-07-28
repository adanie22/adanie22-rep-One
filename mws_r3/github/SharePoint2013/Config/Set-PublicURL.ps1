$curDomain = $env:USERDNSDOMAIN
$clientDomain = "mwsaust.net"

$siteCols = @("communities", "teams", "dev", "my", "search")

foreach($siteCol in $siteCols)
{
    $site = get-spsite "https://$siteCol.$curDomain"
    
    set-spsiteurl -identity $site -url "https://$siteCol.$clientDomain" -zone Intranet

    get-spsiteurl -identity $site
}

