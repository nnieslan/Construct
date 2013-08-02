//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Construct.Tfs.Activities
{
    /// <summary>
    /// A Workflow activity for building a Version Number based on the current branch name and a branch-based build number stored in a version file.
    /// The Branch name will follow a format of DESC_DESC_1.x.x
    /// The Build Number in the version file will be a single integer.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Controller)]
    public sealed class CreateBranchVersionNumber : CodeActivity<string>
    {
        #region properties

        /// <summary>
        /// Gets or sets the VersionFile to use for the BuildNumber.
        /// </summary>
        [RequiredArgument()]
        public InArgument<string> VersionFile { get; set; }

        /// <summary>
        /// Gets or sets the VersionFile to use for the BuildNumber.
        /// </summary>
        [RequiredArgument()]
        public InArgument<bool> IncrementVersionFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IBuildDetails"/> to use for building the remaining Version Number segments.
        /// </summary>
        [RequiredArgument()]
        public InArgument<IBuildDetail> BuildDetails { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Uses the incoming context to build a Version Number, returning it as a string.
        /// </summary>
        /// <param name="context">
        /// The <see cref="CodeActivityContext"/> containing the current workflow instance values.
        /// </param>
        /// <returns>
        /// A version number based on the current branch's build number and the branch name.
        /// </returns>
        protected override string Execute(CodeActivityContext context)
        {
            var versionFile = context.GetValue(this.VersionFile);
            var increment = context.GetValue(this.IncrementVersionFile);

            if (string.IsNullOrWhiteSpace(versionFile))
            {
                context.TrackBuildError("A version file is required.");
                return string.Empty;
            }
            var details = context.GetValue(this.BuildDetails);
            if (details == null)
            {
                context.TrackBuildError("Unable to determine the build settings.");
                return string.Empty;
            }
            var buildName = details.BuildDefinition.Name;
            int buildNumber = this.GetBranchBuildNumber(versionFile, increment, details, context);

            //split the branch name based on the underscore
            var tokens = buildName.Split('_');
            //get the last token from the split
            string version = tokens[tokens.Length - 1];
            //determine in the token is a verion number prefix (i.e. 1.x.x)
            if (Regex.IsMatch(version, @"(\d+\.?)+"))
            {
                return string.Format("{0}.{1}", version, buildNumber);
            }
            //Default to returning just the build number if we aren't able to attach it to a prefix version.
            return buildNumber.ToString();
        }

        /// <summary>
        /// Private Helper function that fetches the current branch version file and reads it's contents.
        /// </summary>
        /// <param name="versionFile">The relative path to the version file in the current branch.</param>
        /// <param name="details">The current build's <see cref="IBuildDetails"/>.</param>
        /// <param name="context">The current context for logging.</param>
        /// <returns>The parsed version number for the current branch.</returns>
        private int GetBranchBuildNumber(
            string versionFile,
            bool increment,
            IBuildDetail details,
            CodeActivityContext context)
        {
            var workspace = details.BuildDefinition.Workspace;
            var vcs = details.BuildServer.TeamProjectCollection.GetService<VersionControlServer>();

            //loop through the workspace mappings attempting to find the version file 
            foreach (var folder in workspace.Mappings)
            {
                string serverPath = folder.ServerItem + "/" + versionFile;
                Item item = null;
                bool found = false;
                try
                {
                    item = vcs.GetItem(serverPath, VersionSpec.Latest);
                    found = true;
                }
                catch (Exception ex)
                {
                    context.TrackBuildMessage(ex.Message);
                    context.TrackBuildMessage(
                        string.Format("Failed attempt to find Version file {0}", serverPath));
                }

                if (found && item != null)
                {
                    context.TrackBuildMessage(
                        string.Format("Found Version file {0}, downloading...", item.ServerItem));

                    // Download the file to a temp location since this is happening on the Build Controller.          
                    string localFile = Path.GetTempFileName();
                    item.DownloadFile(localFile);

                    return VersionHelper.ReadVersionFile(localFile, false, increment);
                }
            }
            return 0;
        }

        #endregion
    }
}
