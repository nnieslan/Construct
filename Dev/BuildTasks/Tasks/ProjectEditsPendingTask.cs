//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Construct.Build.Tasks
{
    public class ProjectEditsPendingTask : Task
    {
        #region members

        private List<ITaskItem> outputList;
        private List<ITaskItem> inputList;

        #endregion

        #region properties


        /// <summary>
        /// Gets or sets the required TfsServerUrl parameter.
        /// </summary>
        public string TfsServerUrl { get; set; }

        /// <summary>
        /// Gets or sets the optional project extension filter parameter.
        /// </summary>
        public string ProjectExtensionFilter { get; set; }

        /// <summary>
        /// Gets or sets the incoming list of project files to test.
        /// </summary>
        public ITaskItem[] Projects
        {
            get
            {
                return this.inputList.ToArray();
            }
            set
            {
                this.inputList = new List<ITaskItem>(value);
            }
        }

        /// <summary>
        /// Gets or sets the list of updated projects.
        /// </summary>
        [Output]
        public ITaskItem[] OutputItems
        {
            get
            {
                if (this.outputList != null)
                {
                    return this.outputList.ToArray();
                }
                return null;
            }
            set
            {
                this.outputList = new List<ITaskItem>(value);
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Validates required parameters.
        /// </summary>
        /// <returns><b>True</b> if parameters are valid, else <b>False</b>.</returns>
        private bool ValidateParameters()
        {
            if (string.IsNullOrWhiteSpace(this.TfsServerUrl))
            {
                base.Log.LogError(Strings.ProjectEditsPendingTfsMissingError);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes the MSBuild task.
        /// </summary>
        /// <returns>true.</returns>
        public override bool Execute()
        {
            base.Log.LogMessage(MessageImportance.Normal, Strings.ProjectEditsPendingMessage);
            if (this.ValidateParameters() && this.Projects.Length > 0)
            {
                try
                {
                    var tfsUrl = new Uri(this.TfsServerUrl, UriKind.Absolute);
                    var server = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUrl);
                    var vcs = server.GetService<VersionControlServer>();
                    int i = 0;
                    Workspace wkspc = null;

                    while (i < this.Projects.Length)
                    {
                        var path = this.Projects[i].GetMetadata("FullPath");
                        if (string.IsNullOrWhiteSpace(this.ProjectExtensionFilter)
                            || path.EndsWith(this.ProjectExtensionFilter))
                        {
                            path = path.Replace(Path.GetFileName(path), "");
                            if (wkspc == null)
                            {
                                wkspc = vcs.GetWorkspace(path);
                            }
                            var changes = wkspc.GetPendingChanges(new string[] {path}, RecursionType.Full);
                            this.Projects[i].SetMetadata("HasPendingChanges", (changes.Length > 0).ToString());

                            if (changes.Length > 0)
                            {
                                base.Log.LogMessage(MessageImportance.High,
                                                    "{0} has pending changes, CodeAnalysis will run.",
                                                    this.Projects[i].ItemSpec);
                            }
                        }

                        i++;
                    }
                }
                catch (Microsoft.TeamFoundation.TeamFoundationServiceUnavailableException ex)
                {
                    base.Log.LogWarningFromException(ex);
                }

            }
            this.outputList = new List<ITaskItem>(this.Projects);
            return true;
        }

        #endregion
    }
}
