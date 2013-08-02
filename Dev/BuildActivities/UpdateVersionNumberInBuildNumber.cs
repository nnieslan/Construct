//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Construct.Tfs.Activities
{
    /// <summary>
    /// A simple build task to replace a $(Version) placeholder in the current
    /// build's number format with the Version number indicated.
    /// </summary>
    public sealed class UpdateVersionNumberInBuildNumber : CodeActivity<string>
    {
        #region properties

        /// <summary>
        /// Gets or sets the build number format with the placeholder.
        /// </summary>
        public InArgument<string> BuildNumberFormat { get; set; }

        /// <summary>
        /// Gets or sets the version number to insert.
        /// </summary>
        public InArgument<string> VersionNumber { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Executes the version number replacement.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <returns>The build number format with the version placeholder replaced.</returns>
        protected override string Execute(CodeActivityContext context)
        {
            string format = context.GetValue(this.BuildNumberFormat);
            string version = context.GetValue(this.VersionNumber);
            var details = context.GetExtension<IBuildDetail>();
            
            format = format.Replace("$(BuildDefinitionName)", details.BuildDefinition.Name);

            var tokens = format.Split('_');
            StringBuilder newFormat = new StringBuilder(format.Length);
            for (int i = 0; i<tokens.Length; i++)
            {
                var token = tokens[i];
                if (!Regex.IsMatch(token, @"(\d+\.?)+")
                    && !token.Equals("$(Version)", StringComparison.OrdinalIgnoreCase))
                {
                    if (i < tokens.Length - 1)
                        newFormat.AppendFormat("{0}_", token);
                    else
                        newFormat.Append(token);
                }
                else if (token.Equals("$(Version)", StringComparison.OrdinalIgnoreCase))
                {
                    if (i < tokens.Length - 1)
                        newFormat.AppendFormat("{0}_", version);
                    else
                        newFormat.Append(version);
                }
            }
            return newFormat.ToString() ;
        }

        #endregion
    }
}
