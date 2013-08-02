//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace Construct.Tfs.Activities
{
    /// <summary>
    /// Checks out all files in the workspace that match the wildcard expression provided.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class Checkout : CodeActivity
    {
        #region properties

        /// <summary>
        /// Gets or sets the file mask to use for filtering.
        /// </summary>
        [RequiredArgument()]
        public InArgument<string> FileToCheckout { get; set; }

        /// <summary>
        /// Gets or sets the Workspace to use for checking files out.
        /// </summary>
        [RequiredArgument()]
        public InArgument<Workspace> Workspace { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Executes the check out of the file denoted.
        /// </summary>
        /// <param name="context">The current context.</param>
        protected override void Execute(CodeActivityContext context)
        {
            string file = context.GetValue(this.FileToCheckout);
            Workspace wkspc = context.GetValue(this.Workspace);

            if (string.IsNullOrWhiteSpace(file))
            {
                context.TrackBuildError("The relative path to a file to checkout is required.");
                return;
            }

            bool found = false;
            foreach (var folder in wkspc.Folders)
            {
                var fullFilePath = Path.Combine(folder.LocalItem, file);

                if (File.Exists(fullFilePath))
                {
                    wkspc.PendEdit(fullFilePath);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                context.TrackBuildError(
                    string.Format("Unable to find a local file in the workspace that matches '{0}'", file));

            }
        }

        #endregion
    }
}
