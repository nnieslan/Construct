﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

##############################################################
# add-domain-users
#  Adds the domain users with the passwords specified,
#  Reads in the CSV file denoted to populate users on the
#  DC indicated using the admin creds provided.
##############################################################

param
(
    [string]$UserListCSV = $(throw 'The path to a Username/Password CSV file is required.'),
    [string]$AdminUser = $(throw 'An admin username (Domain\User) is required.'),
    [string]$AdminPassword = $(throw 'An admin user password is required.'),
    [string]$ldapUri = $(throw 'An LDAP Uri such as LDAP://cn=Users,dc=domain,dc=local is required.'),
    [string]$targetComputer = $(throw 'The target DC to execute the script on.')

)
begin
{     
    $scriptDir = Split-Path $MyInvocation.MyCommand.Path
    . (Join-Path $scriptDir sharepoint-functions.ps1)
}

process
{

    $pw = convertto-securestring -AsPlainText -Force -String $AdminPassword
    $adminCred = new-object -typename System.Management.Automation.PSCredential -argumentlist $AdminUser,$pw
    $dcsession = new-pssession -computername $targetComputer -credential $adminCred;
    #Create domain users
    write-host -ForegroundColor Green "Running AD User Creation..."
    $users = import-csv $UserListCSV
    $users | foreach-object -process {  
        write-host -ForegroundColor Green "- Adding $_ to domain.."; 
        invoke-command -session $dcsession -scriptblock {
            param([string]$UserName,[string]$Password)
            $container = [ADSI] $ldapUri;
            $newUser = $container.Create("User", "cn=" + $UserName);
            $newUser.Put("sAMAccountName", $UserName);
            $newUser.SetInfo();
            $newUser.psbase.InvokeSet('AccountDisabled', $false);
            $newUser.SetInfo();
            $newUser.SetPassword($Password); } -argumentList $_.Username,$_.Password ;
        }
}