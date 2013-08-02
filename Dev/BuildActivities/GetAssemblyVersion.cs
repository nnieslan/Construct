//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.VersionControl.Client;
          
namespace Construct.Tfs.Activities
{
    /// <summary>
    /// Gets the assembly file version to be used in this build.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Controller)]
    public sealed class GetAssemblyVersion : CodeActivity<string>
    {
        #region properties

        /// <summary>
        /// Gets or sets the file spec to filter by when searching source control.
        /// </summary>
        [RequiredArgument]
        public InArgument<string> VersionFile { get; set; }

        /// <summary>
        /// Gets the <see cref="IBuildDetail"/> for the current build.
        /// </summary>
        [RequiredArgument]
        public InArgument<IBuildDetail> BuildDetail { get; set; }

        /// <summary>
        /// Gets or sets the version number octet to increment.
        /// </summary>
        [RequiredArgument()]
        public InArgument<int> OctetToIncrement { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Executes the look up of the current assembly version number.
        /// </summary>
        /// <param name="context">The current workflow context.</param>
        /// <returns>A string representation of the version number.</returns>
        protected override string Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the input arguments            
            string versionFile = context.GetValue(this.VersionFile);
            IBuildDetail buildDetail = context.GetValue(this.BuildDetail);
            int octetPosition = context.GetValue(this.OctetToIncrement);

            if (octetPosition < 1 || octetPosition > 4)
            {
                context.TrackBuildError(
                    string.Format(CultureInfo.InvariantCulture, Strings.InvalidOctetToIncrementError, octetPosition));

                return Strings.OctetToIncrementFailedReturnValue;
            }

            var workspace = buildDetail.BuildDefinition.Workspace;
            var vc = buildDetail.BuildServer.TeamProjectCollection.GetService<VersionControlServer>();

            // Define the regular expression to find (which is for example 'AssemblyFileVersion("1.0.0.0")' )            
            Regex regex = new Regex(VersionHelper.AssemblyFileVersionAttribute + VersionHelper.VersionNumberRegex);

            // For every workspace folder (mapping)            
            foreach (var folder in workspace.Mappings)
            {
                string serverPath = folder.ServerItem + "/" + versionFile;
                Item item = null;
                bool found = false;
                try
                {
                    item = vc.GetItem(serverPath, VersionSpec.Latest);
                    found = true;
                }
                catch (Exception)
                {
                    context.TrackBuildMessage(
                        string.Format(CultureInfo.InvariantCulture, Strings.InvalidFullVersionFilePathError, serverPath));
                }

                if (found && item != null)
                {
                    context.TrackBuildMessage(
                        string.Format(CultureInfo.InvariantCulture, Strings.DownloadingFile, item.ServerItem));

                    // Download the file                   
                    string localFile = Path.GetTempFileName();
                    item.DownloadFile(localFile);

                    // Read the text from the AssemblyInfo file                
                    string text = File.ReadAllText(localFile);
                    // Search for the first occurrence of the version attribute    
                    Match match = regex.Match(text);

                    if (match.Success)
                    {
                        // Retrieve the version number   
                        string versionNumber = match.Value.Substring(
                            VersionHelper.AssemblyFileVersionAttribute.Length + 2,
                            match.Value.Length - VersionHelper.AssemblyFileVersionAttribute.Length - 4);

                        Version version = VersionHelper.Increment(
                            new Version(versionNumber), octetPosition);

                        context.TrackBuildMessage(
                            string.Format(CultureInfo.InvariantCulture, Strings.VersionFileFound, version));

                        return version.ToString();
                    }
                }
            }

            context.TrackBuildError(
                string.Format(CultureInfo.InvariantCulture, Strings.InvalidFullVersionFilePathError, versionFile));

            return Strings.OctetToIncrementFailedReturnValue;
        }

        #endregion
    }
}
