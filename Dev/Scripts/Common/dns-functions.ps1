﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

##############################################################
# new-dnsrecord
#  Adds the DNS record denoted (such as a CNAME entry)
##############################################################
function new-dnsrecord {
param(
    [string]$server,
    [string]$fzone,
    [string]$rzone,
    [string]$computer,
    [string]$address,
    [string]$alias,
    [string]$maildomain,
    [int]$priority,
    [switch]$arec,
    [switch]$ptr,
    [switch]$cname,
    [switch]$mx
)
## check DNS server contactable
    if (-not (Test-Connection -ComputerName $server)){Throw "DNS server not found"}
## split the server fqdn and address
    $srvr = $server -split "\." 
    $addr = $address -split "\." 

    $rec = [WmiClass]"\\$($srvr[0])\root\MicrosoftDNS:MicrosoftDNS_ResourceRecord"  
##
## create records
##  
## A
    if ($arec){
        $text = "$computer IN A $address"  
        $rec.CreateInstanceFromTextRepresentation($server, $fzone, $text)  
    }
## CNAME
    if ($cname){
        $text = "$alias IN CNAME $computer"  
        $rec.CreateInstanceFromTextRepresentation($server, $fzone, $text)  
    }
## PTR
    if ($ptr){
        $text = "$($addr[3]).$rzone IN PTR $computer"  
        $rec.CreateInstanceFromTextRepresentation($server, $rzone, $text)  
    }
## MX
    if ($mx){
        $text = "$maildomain IN MX $priority $computer"  
        $rec.CreateInstanceFromTextRepresentation($server, $fzone, $text)  
    }
} 