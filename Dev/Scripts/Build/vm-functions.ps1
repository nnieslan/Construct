#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#vm-functions - contains functions used to maintain VMs on TFS Lab Management host.

$spsnapin = $(get-pssnapin | where {$_.Name -eq "Microsoft.SystemCenter.VirtualMachineManager" })
if($spsnapin -ne $null) { remove-pssnapin $spsnapin -ErrorAction:SilentlyContinue }
Add-PsSnapin Microsoft.SystemCenter.VirtualMachineManager -ErrorAction:SilentlyContinue

<#
.SYNOPSIS
Moves a vm to a new location on the host to alieviate storage issues.
#>
function move-hosted-vm
{
    param
    (
        [string]$vmname = $(throw 'A VM name to update is required.'),
        [string]$destination = $(throw 'A destination path on the SCVMM host is required.'),
        [string]$hostname = $(throw 'A host computer name is required.'),
        [switch]$isLabManaged
    )
        
    $vmhost = get-vmmserver $hostname
    if(-not $isLabManaged) { $vm = $vmhost | get-vm | ?{$_.Name -eq $vmname} }
    else { $vm = $vmhost | get-vm | ?{$_.Description -match $vmname} }
    
    Move-VM -VM $vm -VMHost $vmhost -Path $destination -RunAsynchronously -UseLAN  
}

<# 
.SYNOPSIS
Gets VM computer names for teh given host.
#>   
function get-vm-computernames
{
   param
    (
        [string]$hostname = $(throw 'A host FQDN is required.')
    )
	
	$vmhost = get-vmmserver $hostname
	
	$vmhost | get-vm | foreach -process { 
		echo $_.ComputerName 
	}
}
