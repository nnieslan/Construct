//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using NuGet;

namespace Coldwater.Construct.Tfs.Activities
{
    /// <summary>
    /// Sets the default NuGet feed for the build process.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class AddDefaultNuGetFeed : CodeActivity
    {
        
        #region properties

        /// <summary>
        /// Gets or sets the destination NuGet server to copy to.
        /// </summary>
        public InArgument<string> DestinationNuGetUri { get; set; }

        #endregion

        #region methods
        protected override void Execute(CodeActivityContext context)
        {
            string dest = context.GetValue(this.DestinationNuGetUri);

            context.TrackBuildMessage(
                string.Format(CultureInfo.InvariantCulture, Strings.NuGetServerUriMessage, dest), BuildMessageImportance.Normal);

            if (string.IsNullOrWhiteSpace(dest))
            {
                context.TrackBuildError(Strings.NuGetServerUriMissingError);
                return;
            }

            Settings.DefaultSettings.SetValue("packageRestore", "enabled", "true");
            Settings.DefaultSettings.SetValue("packageSources", dest, dest);
        }

        #endregion
    }
}
