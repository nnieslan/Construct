
function Clean-GitHubIssues{
	param(
		[string]$token = $(throw 'A GitHub OAuth token is required')
	)
	$issues = Get-GitHubIssues -token $token -owner FlowPayCorp -repo flowpay
	
	$issueDict = ConvertFrom-Json $issues
	
	$issueDict | foreach {
		$body = $_["body"]
		
		$body = $body -replace "n###","\n###"
		$body = $body -replace "(###\s(\w+\s?)+?)\sn",'$1 \n'
		$body = $body -replace "`n",'\n'
		$body = $body.Trim('n')
		#$body
		Set-GitHubIssue -token $token -repo flowpay -owner FlowPayCorp -id $_["number"] -body $body
				
	}
}

function Convert-TfsBugs{

	param(
		[string]$token = $(throw 'A GitHub OAuth token is required')
	)

	#Get-GitHubIssues -token $token -repo "flowpay" -owner "FlowPayCorp" 
	
	$bugs = get-activebugs 'FlowPay'
	
	$bugs | foreach {
		$bug = $_
		
		write-host "Converting | " $bug.ID
		
		$body = "#### Converted From TFS \n"
		$body += "## TFS Id - " + $bug.ID + "\n"
		$body += "## Created By - " + $bug.CreatedBy + "\n"
		
		$repro = '';
		$fields = '';
		$bug.Fields | ?{$_.Value -ne $null} | foreach {
			if( $_.Name -eq 'Repro Steps' ){ 
				$repro = '';
				$repro  += "## " + $_.Name + " \n " 
					
				$htmlSteps = $_.Value | Select-String ">([^<>]+?)<" -AllMatches
				$htmlSteps.matches | foreach { 
					if($_ -ne $null -and $_.Value -ne $null ) {
						$line = $_.Value.Trim('>','<', ' ') | ?{$_ -notmatch "FONT" -and $_ -notmatch "&nbsp;" -and $_ -match "[^\s]"} 
						if($line.length -gt 0 ) {
						$line = $line -replace '\s',' '
						$line = $line -replace '\\',' '
						write-message -message $line
						$repro += $line + "\n"
						}
					}
				}
				$repro += "\n"
			} else {
				if($_.Value -ne '' `
					-and $_.Name -ne 'Created By' `
					-and $_.Name -ne 'Work Item Type' `
					-and $_.Name -ne 'Title' `
					-and $_.Name -ne 'ID' `
					-and $_.Name -ne 'History' `
					-and $_.Name -notmatch 'Date') { 
					$fields += "### " + $_.Name + " \\\n" + $_.Value + "\\\n" 
				}
			}
		}
		$fields = $fields -replace '\\',''
		$body += $repro + $fields
		
		#$body = $body -replace '\\',''
		$body = $body -replace '"','&quot;'		
		$body = $body -replace '{','('
		$body = $body -replace '}',')'
		$title = $_.Title -replace '"','&quot;'
		#$body
		New-GitHubIssue -token $token -repo "flowpay" -owner "FlowPayCorp" -title $title  -body $body -labels @("bug")
	}
}