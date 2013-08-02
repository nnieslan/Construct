//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing.Design;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace Coldwater.Construct.Tfs.Activities
{
 
    /// <summary>
    /// Increments the AssemblyVersion and AssemblyFileVersion attributes in all files
    /// matching the incoming collection.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class IncrementAssemblyVersion : CodeActivity
    {
        #region properties

        /// <summary>
        /// Gets or sets the collection of files to use.
        /// </summary>
        [RequiredArgument()]
        public InArgument<AssemblyInfoFileCollection> AssemblyInfoFiles { get; set; }

        ///// <summary>
        ///// Gets or sets the file mask to use for filtering files.
        ///// </summary>
        [RequiredArgument()]
        public InArgument<string> BuildNumber { get; set; }

        /// <summary>
        /// Gets or sets the workspace to search.
        /// </summary>
        [RequiredArgument()]
        public InArgument<Workspace> Workspace { get; set; }

        /// <summary>
        /// Gets or sets an indicator denoting if the AssemblyVersion attribute should be incremented as part of the action.
        /// </summary>
        public InArgument<bool?> IncludeAssemblyVersion { get; set; }

        /// <summary>
        /// Gets or sets an indicator denoting if the AssemblyFileVersion attribute should be incremented as part of the action.
        /// </summary>
        public InArgument<bool?> IncludeAssemblyFileVersion { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Executes the search for and incrementing of all attributes in matching files.
        /// </summary>
        /// <param name="context">The current workflow context.</param>
        protected override void Execute(CodeActivityContext context)
        {
            Workspace wkspc = context.GetValue(this.Workspace);
            var files = context.GetValue(this.AssemblyInfoFiles);
            string buildNumber = context.GetValue(this.BuildNumber);
            bool? assemblyVersion = context.GetValue(this.IncludeAssemblyVersion);
            bool? assemblyFileVersion = context.GetValue(this.IncludeAssemblyFileVersion);
            
            if (!assemblyVersion.HasValue)
            {
                context.TrackBuildMessage(Strings.DefaultIncludeAssemblyVersionAttribute);
                assemblyVersion = true;
            }

            if (!assemblyFileVersion.HasValue)
            {
                context.TrackBuildMessage(Strings.DefaultIncludeAssemblyFileVersionAttribute);
                assemblyFileVersion = true;
            }

            if (wkspc == null)
            {
                context.TrackBuildError("A Workspace is required for this activity.");
                return;
            }

            if (files == null || files.Count == 0)
            {
                context.TrackBuildMessage("No AssemblyInfo files denoted for version updating.");
                return;
            }


            if (string.IsNullOrWhiteSpace(buildNumber))
            {
                context.TrackBuildError("A valid build number to insert in AssemblyInfo files is required");
                return;
            }

            List<string> attributes = new List<string>();
            if (assemblyFileVersion.Value)
            {
                attributes.Add(VersionHelper.AssemblyFileVersionAttribute);
            }
            if (assemblyVersion.Value)
            {
                attributes.Add(VersionHelper.AssemblyVersionAttribute);
            }

            //update all of the assembly info files with the new version number.
            foreach (string file in files)
            {
                string fullFilePath = string.Empty;
                if (TryFindFile(file, wkspc, out fullFilePath))
                {
                    IncrementAssemblyVersion.ReplaceVersionNumber(
                        fullFilePath,
                        attributes,
                        buildNumber,
                        (s) => { context.TrackBuildMessage(s); });

                }
                else
                {
                    context.TrackBuildError(
                        string.Format(Strings.InvalidFullVersionFilePathError, fullFilePath));
                }
            }
        }

        /// <summary>
        /// Helper method that looks up the AssemblyInfo file in the folders of the current workspace to determine if the file exists.
        /// </summary>
        /// <param name="file">The relative path to the file.</param>
        /// <param name="wkspc">The workspace to interrogate.</param>
        /// <param name="fullFilePath">An out paramter with the file path for the file if it was found.</param>
        /// <returns>True if found, else false.</returns>
        private static bool TryFindFile(string file, Workspace wkspc, out string fullFilePath)
        {
            fullFilePath = string.Empty;
            foreach (var folder in wkspc.Folders)
            {
                fullFilePath = Path.Combine(folder.LocalItem, file);

                if (File.Exists(fullFilePath))
                {
                    return true;
                }
            }   
            return false;
        }

        /// <summary>
        /// Refreshes the AssemblyInfo file attributes for any files that match the regex specified.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="attributes"></param>
        /// <param name="newVersionNumber"></param>
        /// <param name="logMatch"></param>
        private static void ReplaceVersionNumber(
            string file, 
            List<string> attributes, 
            string newVersionNumber, 
            Action<string> logMatch)
        {
            string contents = File.ReadAllText(file);
            foreach (var attribute in attributes)
            {
                Regex versionExpression =
                    new Regex(string.Format(@"{0}\(""{1}""\)", attribute, VersionHelper.VersionNumberRegex));

                Match match = versionExpression.Match(contents);
                if (match.Success)
                {
                    logMatch(string.Format(
                        Strings.VersionIncremented,
                        attribute,
                        file,
                        newVersionNumber));

                    contents = versionExpression.Replace(contents, string.Format("{0}(\"{1}\")", attribute, newVersionNumber));
                }
            }
            var info = new FileInfo(file);
            var oldAttributes = info.Attributes;
            info.Attributes = FileAttributes.Normal;
            File.WriteAllText(file, contents);
            info.Attributes = oldAttributes;
        }

        #endregion
    }
}
