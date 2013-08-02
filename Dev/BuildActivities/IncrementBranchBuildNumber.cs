//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Coldwater.Construct.Tfs.Activities
{
    /// <summary>
    /// A Workflow activity for committing the incrementing of a branch-based build number stored in a version file.
    /// The Build Number in the version file will be a single integer.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class IncrementBranchBuildNumber : CodeActivity<int>
    {
        #region properties

        /// <summary>
        /// Gets or sets the relative path to the version file for this branch.
        /// </summary>
        [RequiredArgument()]
        public InArgument<string> VersionFile { get; set; }

        /// <summary>
        /// Gets or sets the local workspace.
        /// </summary>
        [RequiredArgument()]
        public InArgument<Workspace> Workspace { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Opens the version file for this branch by finding it in the local 
        /// workspace and increments the build number contained within it.
        /// </summary>
        /// <param name="context">The current workflow <see cref="CodeActivityContext"/>.</param>
        /// <returns>The incremented build number.</returns>
        protected override int Execute(CodeActivityContext context)
        {
            string versionFile = context.GetValue(this.VersionFile);
            Workspace wkspc = context.GetValue(this.Workspace);

            if (string.IsNullOrWhiteSpace(versionFile))
            {
                context.TrackBuildError("A version file containing a Version number to read and increment is required.");
                return 0;
            }

            foreach (var folder in wkspc.Folders)
            {
                var fullpathVersionFile = Path.Combine(folder.LocalItem, versionFile);
                if (File.Exists(fullpathVersionFile))
                {
                    return VersionHelper.ReadVersionFile(fullpathVersionFile, true, true);
                }
            }
            context.TrackBuildError(
                string.Format("The provided version file '{0}' does not exist.", versionFile));
            return 0;
        }

        #endregion
    }
}
