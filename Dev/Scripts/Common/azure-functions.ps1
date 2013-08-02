$Global:TimeStampFormat = "g"

# load the required dll
[void][System.Reflection.Assembly]::LoadFile("$env:PROGRAMFILES\Microsoft SDKs\Windows Azure\.NET SDK\2012-10\bin\Microsoft.WindowsAzure.StorageClient.dll")
$references = @("$env:PROGRAMFILES\Microsoft SDKs\Windows Azure\.NET SDK\2012-10\bin\Microsoft.WindowsAzure.StorageClient.dll")

Add-Type -ReferencedAssemblies $references `
		 -TypeDefinition "
		using System;
		using System.Collections.ObjectModel;
		using System.Collections.Generic;
		using System.IO;
		using System.Linq;
		using System.Management.Automation;
		using System.Text;
		using System.Threading.Tasks;
		using Microsoft.WindowsAzure;
		using Microsoft.WindowsAzure.StorageClient;

		public class AzureBlobDownloader
	    {
	        public string StorageAccount { get; set; }

	        public string AccountKey { get; set; }

	        public string Container { get; set; }

	        public string Destination { get; set; }

	        public void Execute()
           {
            var dest = System.IO.Path.GetFullPath(this.Destination);
            StorageCredentialsAccountAndKey credentials = new StorageCredentialsAccountAndKey(this.StorageAccount, this.AccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(this.Container);
            foreach (var blob in container.ListBlobs())
            {

                var bRef = container.GetBlobReference(blob.Uri.OriginalString);
                bRef.DownloadToFile(System.IO.Path.Combine(dest, bRef.Name));
            }
        }
	  }

	  public class AzureBlobUploader
	  {
	        public string StorageAccount { get; set; }

	        public string AccountKey { get; set; }

	        public string Container { get; set; }

	        public string File { get; set; }

	        public void Execute()
	        {
	            var blobName = Path.GetFileName(this.File);
	            StorageCredentialsAccountAndKey credentials = new StorageCredentialsAccountAndKey(this.StorageAccount, this.AccountKey);
	            CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, true);
	            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
	            CloudBlobContainer container = blobClient.GetContainerReference(this.Container);
	            container.CreateIfNotExist();
	            var blob = container.GetBlobReference(blobName);
	            blob.UploadFile(this.File);
	        }
	    }"

<#
.SYNOPSIS
Helper function that publishes the Azure package to the service and slot specified.
#>
function Publish {
	param 
	(
		[string]$serviceName,
		[string]$slot,
		[string]$packageLocation = "",
	    [string]$cloudConfigLocation = "",
	    [string]$deploymentLabel = "Automated deploy to $servicename"
	)
    $deployment = Get-AzureDeployment -ServiceName $serviceName -Slot $slot -ErrorVariable a -ErrorAction silentlycontinue 
    if ($a[0] -ne $null)
    {
        write-message -message "$(Get-Date –f $Global:TimeStampFormat) - No deployment is detected. Creating a new deployment. "
    }
    #check for existing deployment and then either upgrade, delete + deploy, or cancel according to $alwaysDeleteExistingDeployments and $enableDeploymentUpgrade boolean variables
    if ($deployment.Name -ne $null)
    {
        if($alwaysDeleteExistingDeployments) {
            if($enableDeploymentUpgrade) {
            	#Update deployment inplace (usually faster, cheaper, won't destroy VIP)
                write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Deployment exists in $servicename.  Upgrading deployment."
                UpgradeDeployment -serviceName $serviceName `
								  -slot $slot `
								  -packageLocation $packageLocation `
								  -cloudConfigLocation $cloudConfigLocation `
								  -deploymentLabel $deploymentLabel
            }
            else  #Delete then create new deployment
            {
                write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Deployment exists in $servicename.  Deleting deployment."
                DeleteDeployment -serviceName $serviceName -slot $slot 
								    
                CreateNewDeployment -serviceName $serviceName `
								    -slot $slot `
								    -packageLocation $packageLocation `
								    -cloudConfigLocation $cloudConfigLocation `
								    -deploymentLabel $deploymentLabel
            }
        } else {
            write-error -message "$(Get-Date –f $Global:TimeStampFormat) - ERROR: Deployment exists in $servicename.  Script execution cancelled."
            exit
        }
    } else {
        CreateNewDeployment -serviceName $serviceName `
						    -slot $slot `
						    -packageLocation $packageLocation `
						    -cloudConfigLocation $cloudConfigLocation `
						    -deploymentLabel $deploymentLabel
    }
}

<#
.SYNOPSIS
Helper function that creates a new deployment at the service and slot specified.
#>
function CreateNewDeployment {
	param 
	(
		[string]$serviceName,
		[string]$slot,
		[string]$packageLocation = "",
	    [string]$cloudConfigLocation = "",
	    [string]$deploymentLabel = "Automated deploy to $servicename"
	)
    write-progress -id 3 -activity "Creating New Deployment" -Status "In progress"
    write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Creating New Deployment: In progress"

    $opstat = New-AzureDeployment -Slot $slot -Package $packageLocation -Configuration $cloudConfigLocation -label $deploymentLabel -ServiceName $serviceName

    $completeDeployment = Get-AzureDeployment -ServiceName $serviceName -Slot $slot
    $completeDeploymentID = $completeDeployment.deploymentid

    write-progress -id 3 -activity "Creating New Deployment" -completed -Status "Complete"
    write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Creating New Deployment: Complete, Deployment ID: $completeDeploymentID"

    StartInstances -serviceName $serviceName -slot $slot 
}

<#
.SYNOPSIS
Helper function that upgrades an existing deployment at the service and slot specified.
#>
function UpgradeDeployment {
	param 
	(
		[string]$serviceName,
		[string]$slot,
		[string]$packageLocation = "",
	    [string]$cloudConfigLocation = "",
	    [string]$deploymentLabel = "Automated deploy to $servicename"
	)
    write-progress -id 3 -activity "Upgrading Deployment" -Status "In progress"
    write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Upgrading Deployment: In progress"

    # perform Update-Deployment
    $setdeployment = Set-AzureDeployment -Upgrade -Slot $slot -Package $packageLocation -Configuration $cloudConfigLocation -label $deploymentLabel -ServiceName $serviceName -Force

    $completeDeployment = Get-AzureDeployment -ServiceName $serviceName -Slot $slot
    $completeDeploymentID = $completeDeployment.deploymentid

    write-progress -id 3 -activity "Upgrading Deployment" -completed -Status "Complete"
    write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Upgrading Deployment: Complete, Deployment ID: $completeDeploymentID"
}

<#
.SYNOPSIS
Helper function that deletes the deployment at the service and slot specified.
#>
function DeleteDeployment {
	param 
	(
		[string]$serviceName,
		[string]$slot
	)

    write-progress -id 2 -activity "Deleting Deployment" -Status "In progress"
    write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Deleting Deployment: In progress"

    #WARNING - always deletes with force
    $removeDeployment = Remove-AzureDeployment -Slot $slot -ServiceName $serviceName -Force

    write-progress -id 2 -activity "Deleting Deployment: Complete" -completed -Status $removeDeployment
    write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Deleting Deployment: Complete"

}

<#
.SYNOPSIS
Helper function used to determine if Azure Cloud Services RoleInstances are running.
#>
function StartInstances {
	param 
	(
		[string]$serviceName,
		[string]$slot
	)
	
    write-progress -id 4 -activity "Starting Instances" -status "In progress"
    write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Starting Instances: In progress"

    $deployment = Get-AzureDeployment -ServiceName $serviceName -Slot $slot
    $runstatus = $deployment.Status

    if ($runstatus -ne 'Running') 
    {
        $run = Set-AzureDeployment -Slot $slot -ServiceName $serviceName -Status Running
    }
    $deployment = Get-AzureDeployment -ServiceName $serviceName -Slot $slot
    $oldStatusStr = @("") * $deployment.RoleInstanceList.Count

    while (-not(AllInstancesRunning($deployment.RoleInstanceList)))
    {
        $i = 1
        foreach ($roleInstance in $deployment.RoleInstanceList)
        {
            $instanceName = $roleInstance.InstanceName
            $instanceStatus = $roleInstance.InstanceStatus

            if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
            {
                $oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
                write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Starting Instance '$instanceName': $instanceStatus"
            }

            write-progress -id (4 + $i) -activity "Starting Instance '$instanceName'" -status "$instanceStatus"
            $i = $i + 1
        }

        sleep -Seconds 1

        $deployment = Get-AzureDeployment -ServiceName $serviceName -Slot $slot
    }

    $i = 1
    foreach ($roleInstance in $deployment.RoleInstanceList)
    {
        $instanceName = $roleInstance.InstanceName
        $instanceStatus = $roleInstance.InstanceStatus

        if ($oldStatusStr[$i - 1] -ne $roleInstance.InstanceStatus)
        {
            $oldStatusStr[$i - 1] = $roleInstance.InstanceStatus
            write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Starting Instance '$instanceName': $instanceStatus"
        }

        $i = $i + 1
    }

    $deployment = Get-AzureDeployment -ServiceName $serviceName -Slot $slot
    $opstat = $deployment.Status 

    write-progress -id 4 -activity "Starting Instances" -completed -status $opstat
    write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Starting Instances: $opstat"
}


<#
.SYNOPSIS
Helper function used to determine if Azure Cloud Services RoleInstances are running.
#>
function AllInstancesRunning($roleInstanceList){
    foreach ($roleInstance in $roleInstanceList)
    {
        if ($roleInstance.InstanceStatus -ne "ReadyRole")
        {
            return $false
        }
    }

    return $true
}

<#
.SYNOPSIS
Creates or Updates an Azure Cloud Services deployment based on the information passed in.
#>
function DeployAndVerifyCloudService {
	param(  
		[string]$serviceName = "",
	    [string]$storageAccountName = "",
	    [string]$packageLocation = "",
	    [string]$cloudConfigLocation = "",
	    [string]$environment = "Staging",
	    [string]$deploymentLabel = "Automated deploy to $servicename",
	    [string]$timeStampFormat = "g",
	    [switch]$alwaysDeleteExistingDeployments,
	    [switch]$enableDeploymentUpgrade,
	    [string]$selectedsubscription = "default",
	    [string]$subscriptionDataFile = ""
		)

	$Global:TimeStampFormat = $timeStampFormat
	
	#configure powershell with publishsettings for your subscription
	$pubsettings = $subscriptionDataFile
	Import-AzurePublishSettingsFile $pubsettings
	Set-AzureSubscription -CurrentStorageAccount $storageAccountName -SubscriptionName $selectedsubscription

	#set remaining environment variables for Azure cmdlets
	$subscription = Get-AzureSubscription $selectedsubscription
	$subscriptionname = $subscription.subscriptionname
	$subscriptionid = $subscription.subscriptionid
	$slot = $environment

	#main driver - publish & write progress to activity log
	write-header -message "$(Get-Date –f $Global:TimeStampFormat) - Azure Cloud Service deploy script started."
	write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Preparing deployment of $deploymentLabel for $subscriptionname with Subscription ID $subscriptionid."
	
	Publish -serviceName $serviceName `
		    -slot $slot `
		    -packageLocation $packageLocation `
		    -cloudConfigLocation $cloudConfigLocation `
		    -deploymentLabel $deploymentLabel
	
	$deployment = Get-AzureDeployment -slot $slot -serviceName $servicename
	$deploymentUrl = $deployment.Url

	write-message -message "$(Get-Date –f $Global:TimeStampFormat) - Created Cloud Service with URL $deploymentUrl."
	write-header -message "$(Get-Date –f $Global:TimeStampFormat) - Azure Cloud Service deploy script finished."
}


function Download-BlobsFromContainer
{
	param(
      [string]$storageAccountName,
      [string]$blobContainer,
      [string]$destination
	) 
	
	$secondaryKey = (Get-AzureStorageKey -StorageAccountName $storageAccountName).Secondary
	$downloader = New-Object AzureBlobDownloader
	$downloader.StorageAccount = $storageAccountName
	$downloader.AccountKey = $secondaryKey
	$downloader.Container = $blobContainer
	$downloader.Destination = $destination
	
	$downloader.Execute()
}

<#
.SYNOPSIS
Uploads a file to the Azure Blob container denoted.
#>
function Upload-BlobToContainer
{
	param(
      [string]$storageAccountName,
      [string]$blobContainer,
      [string]$file
	) 
	
	$secondaryKey = (Get-AzureStorageKey -StorageAccountName $storageAccountName).Secondary
	$uploader = New-Object AzureBlobUploader
	$uploader.StorageAccount = $storageAccountName
	$uploader.AccountKey = $secondaryKey
	$uploader.Container = $blobContainer
	$uploader.File = $file
	
	$uploader.Execute()
}