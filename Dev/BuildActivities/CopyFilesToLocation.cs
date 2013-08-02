//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace Construct.Tfs.Activities
{
    /// <summary>
    /// Copies files from a source location to a destination location
    /// optionally filtering based on a file mask.  
    /// File searches are done recursively.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class CopyFilesToLocation : CodeActivity
    {
        #region properties

        /// <summary>
        /// Gets or sets the source location to copy from.
        /// </summary>
        public InArgument<string> SourceLocation { get; set; }
        
        /// <summary>
        /// Gets or sets the filter to use when finding source files.
        /// </summary>
        public InArgument<string> SourceFileSpec { get; set; }

        /// <summary>
        /// Gets or sets the the destination folder to copy to.
        /// </summary>
        public InArgument<string> DestinationLocation { get; set; }
        
        #endregion

        #region methods
        
        /// <summary>
        /// Executes the copy action, failures occur if the source location
        /// doesn't exist.
        /// </summary>
        /// <param name="context">The current <see cref="CodeActivityContext"/>.</param>
        protected override void Execute(CodeActivityContext context)
        {
            string source = context.GetValue(this.SourceLocation);
            string filespec = context.GetValue(this.SourceFileSpec);
            string dest = context.GetValue(this.DestinationLocation);

            context.TrackBuildMessage(
                string.Format(CultureInfo.InvariantCulture, Strings.CopyLocationSourceMessage, source), BuildMessageImportance.Normal);
            context.TrackBuildMessage(
                string.Format(CultureInfo.InvariantCulture, Strings.CopyLocationFileSpecMessage, filespec), BuildMessageImportance.Normal);
            context.TrackBuildMessage(
                string.Format(CultureInfo.InvariantCulture, Strings.CopyLocationDestinationMessage, dest) , BuildMessageImportance.Normal);

            if(string.IsNullOrWhiteSpace(source))
            {
                context.TrackBuildError(Strings.CopyLocationSourceMissingError);
                return;
            }

            if (string.IsNullOrWhiteSpace(dest))
            {
                context.TrackBuildError(Strings.CopyLocationDestinationMissingError);
                return;
            }

            //default to all files if no filter is given
            if (string.IsNullOrWhiteSpace(filespec))
            {
                filespec = "*.*";
            }

            if (Directory.Exists(source))
            {
                var files = Directory.GetFiles(
                    source, 
                    filespec, 
                    SearchOption.AllDirectories);

                if (!Directory.Exists(dest))
                {
                    context.TrackBuildMessage(
                        string.Format(CultureInfo.InvariantCulture, Strings.CreateDestinationLocation, dest), 
                        BuildMessageImportance.High);

                    Directory.CreateDirectory(dest);
                }

                foreach (var file in files)
                {
                    string destFile = Path.Combine(dest, Path.GetFileName(file));
                    context.TrackBuildMessage(
                        string.Format(CultureInfo.InvariantCulture, Strings.CopyingFiles, file, destFile));

                    File.Copy(file, destFile, true);
                }
            }
            else
            {
                context.TrackBuildError(
                    string.Format(CultureInfo.InvariantCulture, Strings.CopySourceLocationMissingError, source));
            }
        }

        #endregion
    }
}
