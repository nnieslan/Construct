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
    /// Publishes NuGet packages to the denoted NuGet server feed.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class PublishNuGetPackage : CodeActivity
    {
        private const string ConstructTfsUserAgent = "Coldwater Construct TFS NuGet Activity";

        #region properties

        /// <summary>
        /// Gets or sets the source location to copy from.
        /// </summary>
        public InArgument<string> SourceLocation { get; set; }

        /// <summary>
        /// Gets or sets the filter to use when finding source files.
        /// </summary>
        public InArgument<string> NuGetPackageFileSpec { get; set; }

        /// <summary>
        /// Gets or sets the destination NuGet server to copy to.
        /// </summary>
        public InArgument<string> DestinationNuGetUri { get; set; }

        /// <summary>
        /// Gets or sets the NuGet Server ApiKey to use to push packages.
        /// </summary>
        public InArgument<string> ApiKey { get; set; }

        #endregion

        #region methods
        
        protected override void Execute(CodeActivityContext context)
        {
                string source = context.GetValue(this.SourceLocation);
                string filespec = context.GetValue(this.NuGetPackageFileSpec);
                string dest = context.GetValue(this.DestinationNuGetUri);
                string apiKey = context.GetValue(this.ApiKey);

                context.TrackBuildMessage(
                    string.Format(CultureInfo.InvariantCulture, Strings.NuGetPackageSourceMessage, source), BuildMessageImportance.High);
                context.TrackBuildMessage(
                    string.Format(CultureInfo.InvariantCulture, Strings.NuGetPackagesFileSpecMessage, filespec), BuildMessageImportance.High);
                context.TrackBuildMessage(
                    string.Format(CultureInfo.InvariantCulture, Strings.NuGetServerUriMessage, dest) , BuildMessageImportance.High);

                if(string.IsNullOrWhiteSpace(source))
                {
                    context.TrackBuildError(Strings.NuGetPackageSourceMissingError);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dest))
                {
                    context.TrackBuildError(Strings.NuGetServerUriMissingError);
                    return;
                }

                //default to all nupkg files if no filter is given
                if (string.IsNullOrWhiteSpace(filespec))
                {
                    filespec = "*.nupkg";
                }

            if (Directory.Exists(source))
            {
                var files = Directory.GetFiles(
                    source,
                    filespec,
                    SearchOption.AllDirectories);

                this.PushPackage(files, dest, apiKey, (s) => { context.TrackBuildMessage(s, BuildMessageImportance.High); });

            }
        }

        /// <summary>
        /// Publishes the passed in list of packages to the package server using the API Key given.
        /// </summary>
        /// <param name="packages">The array of packag paths.</param>
        /// <param name="pkgSource">The Nuget feed source to publish to.</param>
        /// <param name="apiKey">The ApiKey to use for that publishing.</param>
        private void PushPackage(string[] packages, string pkgSource, string apiKey, Action<string> logMessage)
        {
            PackageServer packageServer = new PackageServer(pkgSource, ConstructTfsUserAgent);
            
            foreach (string current in packages)
            {
                logMessage(string.Format("Uploading {0} to the package server {1}", current, pkgSource));
                ZipPackage zipPackage = new ZipPackage(current);

                using (Stream stream = zipPackage.GetStream())
                {
                    packageServer.PushPackage(apiKey, stream);
                }
            }
        }

        #endregion
    }
}
