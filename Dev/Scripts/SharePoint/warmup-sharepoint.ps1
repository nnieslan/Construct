#==============================================================================
# Copyright (c) Coldwater Software. All Rights Reserved.
#==============================================================================

################################################################
# warmup-sharepoint
#   A website warm-up script that hits all urls passed in, 
#   loading them once to ensure they can be initialized.
################################################################
param
(
    [string[]]$urls= @("http://localhost")
)

begin
{
    New-EventLog -LogName "Application" -Source "SharePoint Warmup Script" -ErrorAction SilentlyContinue | Out-Null
}

process
{
    $urls | % {
        $url = $_
        try {
            $wc = New-Object System.Net.WebClient
            $wc.Credentials = [System.Net.CredentialCache]::DefaultCredentials
            $ret = $wc.DownloadString($url)
            if( $ret.Length -gt 0 ) {
                $s = "Last run successful for url ""$($url)"": $([DateTime]::Now.ToString('yyyy.dd.MM HH:mm:ss'))"
                $filename=((Split-Path ($MyInvocation.MyCommand.Path))+"\lastrunlog.txt")
                if( Test-Path $filename -PathType Leaf ) {
                    $c = Get-Content $filename
                    $cl = $c -split '`n'
                    $s = ((@($s) + $cl) | select -First 200)
                }
                Out-File -InputObject ($s -join "`r`n") -FilePath $filename
            }
        } catch {
              Write-EventLog -Source "SharePoint Warmup Script"  -Category 0 -ComputerName "." -EntryType Error -LogName "Application" `
                -Message "SharePoint Warmup failed for url ""$($url)""." -EventId 1001

            $s = "Last run failed for url ""$($url)"": $([DateTime]::Now.ToString('yyyy.dd.MM HH:mm:ss')) : $($_.Exception.Message)"
            $filename=((Split-Path ($MyInvocation.MyCommand.Path))+"\lastrunlog.txt")
            if( Test-Path $filename -PathType Leaf ) {
              $c = Get-Content $filename
              $cl = $c -split '`n'
              $s = ((@($s) + $cl) | select -First 200)
            }
            Out-File -InputObject ($s -join "`r`n") -FilePath $filename
        }
    }
}