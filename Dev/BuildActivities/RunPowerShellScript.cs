//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace Coldwater.Construct.Tfs.Activities
{
    internal static class Extensions
    {
        public static SecureString ToSecureString(this string password)
        {
            SecureString securePw;
            // Define the string value to assign to a new secure string.
            char[] chars = password.ToCharArray();
            // Instantiate a new secure string.
            unsafe
            {
                fixed (char* pChars = chars)
                {
                    securePw = new SecureString(pChars, chars.Length);
                }
            }
            return securePw;
        }
    }

    /// <summary>
    /// Executes the PowerShell script indicated on the remote machine denoted.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class RunPowerShellScript : CodeActivity
    {

        #region members

        private const string FormattedExceptionMessage = "Powershell Exception : {0} \nStackTrace : {1}";

        private Dictionary<string, object> taskProperties
            = new Dictionary<string, object>();

        private string command;
        private string scriptLoc;
        private string runAsUser;
        private string runAsUserPw;
        private string computerName;
        private int port;
        private bool runLocal;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the powershell script to execute.
        /// </summary>
        public InArgument<string> ScriptLocation { get; set; }

        /// <summary>
        /// Gets or sets a powershell command to execute.
        /// </summary>
        public InArgument<string> CommandToRun { get; set; }

        /// <summary>
        /// Getsor sets a flag indicating if the script should run on a remote machine or locally.
        /// </summary>
        public InArgument<bool> RunLocally { get; set; }

        /// <summary>
        /// Gets or sets the name of the service account to run as remotely (DOMAIN\user).
        /// </summary>
        public InArgument<string> RunAsAccount { get; set; }

        /// <summary>
        /// Gets or sets the password of the service account to run as remotely.
        /// </summary>
        /// <remarks>
        /// TODO - move toa SecureString outside of this scope.
        /// </remarks>
        public InArgument<string> RunAsAccountPassword { get; set; }

        /// <summary>
        /// Gets or sets the target computer name.
        /// </summary>
        public InArgument<string> TargetComputerName { get; set; }

        /// <summary>
        /// Gets or sets the target computer WinRM port.
        /// </summary>
        public InArgument<int> TargetComputerPort { get; set; }

        /// <summary>
        /// Gets or sets the drop location of the build binaries
        /// </summary>
        public InArgument<string> DropFolder { get; set; }

        /// <summary>
        /// Gets or sets the drop location of the build binaries
        /// </summary>
        public InArgument<string> TargetUrl { get; set; }

        #endregion

        protected override void Execute(CodeActivityContext context)
        {
            if (!this.Initialize(context))
            {
                return;
            }

            if (this.runLocal)
            {
                this.RunLocalScript(context);
            }
            else
            {
                this.RunRemoteScript(context);
            }
        }

        private bool Initialize(CodeActivityContext context)
        {
            if (!this.taskProperties.ContainsKey("out"))
            {
                this.taskProperties.Add("out", null);
            }
            if (!this.taskProperties.ContainsKey("messages"))
            {
                this.taskProperties.Add("messages", null);
            }

            this.runLocal = context.GetValue(this.RunLocally);
            this.scriptLoc = context.GetValue(this.ScriptLocation);
            this.command = context.GetValue(this.CommandToRun);
            if (!File.Exists(this.scriptLoc) && string.IsNullOrEmpty(this.command))
            {
                context.TrackBuildError(string.Format("The location '{0}' does not exist and no string command was specified", scriptLoc));
                return false;
            }
            
            if (!this.runLocal)
            {
                this.computerName = context.GetValue(this.TargetComputerName);
                if (string.IsNullOrWhiteSpace(this.computerName))
                {
                    context.TrackBuildError("A target computer to execute on is required.");
                    return false;
                }

                this.port = context.GetValue(this.TargetComputerPort);
                if (this.port <= 0)
                {
                    context.TrackBuildError("The port for the target computer to execute on is required.");
                    return false;
                }

                this.runAsUser = context.GetValue(this.RunAsAccount);
                if (string.IsNullOrEmpty(this.runAsUser))
                {
                    context.TrackBuildError("A service account to use for powershell execution is required.");
                    return false;
                }

                this.runAsUserPw = context.GetValue(this.RunAsAccountPassword);
                if (string.IsNullOrEmpty(this.runAsUserPw))
                {
                    context.TrackBuildError("A service account password to use for powershell execution is required.");
                    return false;
                }
            }

            return true;
        }

        private void RunRemoteScript(CodeActivityContext context)
        {
            PSCredential remoteCredential = new PSCredential(this.runAsUser, this.runAsUserPw.ToSecureString());
            string shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
            Uri connectionUrl = new Uri(string.Format("http://{0}:{1}/wsman", this.computerName, this.port), UriKind.Absolute);
            WSManConnectionInfo connectionInfo = new WSManConnectionInfo(connectionUrl, shellUri, remoteCredential);
            connectionInfo.OperationTimeout = 4 * 60 * 1000; // 4 minutes.
            connectionInfo.OpenTimeout = 1 * 60 * 1000; // 1 minute.
            
            connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Credssp; //use CredSSP for double hop support in remote runspaces.
            //See article for more info: http://blogs.msdn.com/b/powershell/archive/2008/06/05/credssp-for-second-hop-remoting-part-i-domain-account.aspx

            context.TrackBuildError(string.Format("Construct PowerShell: Performing execution remotely on {0}", this.computerName));
                                   

            using (Runspace remoteRunspace = RunspaceFactory.CreateRunspace(connectionInfo))
            {
                remoteRunspace.Open();
                using (Pipeline pipeline = remoteRunspace.CreatePipeline())
                {
                    try
                    {
                        this.SeedPipeline(pipeline);

                        if (!string.IsNullOrWhiteSpace(this.command))
                        {
                            pipeline.Commands.AddScript(this.command);
                        } 
                        if (File.Exists(scriptLoc))
                        {
                            var cmd = new Command(File.ReadAllText(scriptLoc), true);
                            pipeline.Commands.Add(cmd);
                        }
                        
                        pipeline.Invoke();

                        this.ReadPipeline(pipeline);

                        var messages = this.taskProperties["messages"] as PSObject ;
                        if(messages != null) 
                        {
                            var ht = messages.BaseObject as Hashtable;
                            if (ht != null)
                            {
                                foreach (var key in ht.Keys)
                                {
                                    if (((string)ht[key]) == "Error")
                                    {
                                        context.TrackBuildError(string.Format("Construct PowerShell: {0}", key));
                                    }
                                    else
                                    {
                                        context.TrackBuildMessage(string.Format("Construct PowerShell: {0}", key));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        context.TrackBuildError(string.Format(FormattedExceptionMessage, ex.Message, ex.StackTrace));
                    }
                }
                remoteRunspace.Close();
            }
        }

        /// <summary>
        /// Executes the wrapped PowerShell script in a newly seeded <see cref="Pipeline"/> and fetches any state.
        /// </summary>
        /// <returns>An indicator denoting if there are any pipeline errors from the current execution.</returns>
        private void RunLocalScript(CodeActivityContext context)  
        {
            context.TrackBuildMessage("Construct PowerShell: Performing Execution locally.", BuildMessageImportance.High);

            InitialSessionState s = InitialSessionState.CreateDefault();
            s.ImportPSModule(new string[] {"Construct"});
                
            using (Pipeline pipeline = RunspaceFactory.CreateRunspace(s).CreatePipeline())
            {
                pipeline.Runspace.Open();
                try
                {
                    this.SeedPipeline(pipeline);
                    if (!string.IsNullOrWhiteSpace(this.command))
                    {
                        context.TrackBuildMessage(string.Format("Construct PowerShell: Executing Command '{0}'", this.command), BuildMessageImportance.High);
                        pipeline.Commands.AddScript(this.command);
                    }
                    if (File.Exists(scriptLoc))
                    {
                        context.TrackBuildMessage(string.Format("Construct PowerShell: Executing Script File '{0}'", this.scriptLoc), BuildMessageImportance.High);
                        var cmd = new Command(File.ReadAllText(scriptLoc), true);
                        pipeline.Commands.Add(cmd);
                    }
                        
                    pipeline.Invoke();
                    this.ReadPipeline(pipeline);

                    var messages = this.taskProperties["messages"] as PSObject;
                    if (messages != null)
                    {
                        var ht = messages.BaseObject as Hashtable;
                        if (ht != null)
                        {
                            foreach (var key in ht.Keys)
                            {
                                if (((string)ht[key]) == "Error")
                                {
                                    context.TrackBuildError(string.Format("Construct PowerShell: {0}", key));
                                }
                                else
                                {
                                    context.TrackBuildMessage(string.Format("Construct PowerShell: {0}", key), BuildMessageImportance.High);
                                }
                            }
                        }
                    }
                        
                }
                catch(Exception ex)
                {
                    context.TrackBuildError(string.Format(FormattedExceptionMessage, ex.Message, ex.StackTrace));
                }
            }
        }
        /// <summary>
        /// Helper function that seeds a PowerShell <see cref="Pipeline"/> with the session state from this task instance.
        /// </summary>
        /// <param name="pipeline">The PowerShell pipeline runspace to seed.</param>
        private void SeedPipeline(Pipeline pipeline)
        {
            foreach (string key in this.taskProperties.Keys)
            {
                pipeline.Runspace.SessionStateProxy.SetVariable(key, this.taskProperties[key]);
            }
        }

        /// <summary>
        /// Reads session state out from a <see cref="Pipeline"/> back into the property bag for this task instance.
        /// </summary>
        /// <param name="pipeline">The pipeline to read.</param>
        private void ReadPipeline(Pipeline pipeline)
        {
            var keys = this.taskProperties.Keys.ToArray();
            foreach (string key in keys)
            {
                this.taskProperties[key] = pipeline.Runspace.SessionStateProxy.GetVariable(key);
            }
        }

    }
}
