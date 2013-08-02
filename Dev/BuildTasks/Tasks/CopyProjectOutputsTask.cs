//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Coldwater.Construct.Build.Tasks
{
    /// <summary>
    /// Copies the incoming project outputs to the denoted locations
    /// </summary>
    public class CopyProjectOutputsTask : Task
    {
        #region consts
        
        private const string DeployDirMetadata = "DeployDir";
        private const string IdentityMetadata = "Identity";
        private const string SourceProjectMetadata = "MSBuildSourceProjectFile";
        private const string FullPathMetdata = "FullPath";
        private const string DebugSymbolExtension = ".pdb";
        private const string DebugConfiguration = "Debug";

        #endregion
        #region members

        private List<ITaskItem> projectTargetsList;
        private List<ITaskItem> inputProjectList;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the OutDir property being used for root bin-place location currently in the build.
        /// </summary>
        public string OutDir { get; set; }

        /// <summary>
        /// Gets or sets the current build configuration.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets the incoming list of project files to evaluate.
        /// </summary>
        public ITaskItem[] Projects
        {
            get
            {
                return this.inputProjectList.ToArray();
            }
            set
            {
                this.inputProjectList = new List<ITaskItem>(value);
            }
        }

        /// <summary>
        /// Gets or sets the list of updated projects.
        /// </summary>
        public ITaskItem[] ProjectTargetOutputItems
        {
            get
            {
                if (this.projectTargetsList != null)
                {
                    return this.projectTargetsList.ToArray();
                }
                return null;
            }
            set
            {
                this.projectTargetsList = new List<ITaskItem>(value);
            }
        }

        #endregion

        /// <summary>
        /// Overrides the base <see cref="Task"/> execute virtual method to implement the copy task for this project.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            base.Log.LogMessage(MessageImportance.Normal, Strings.CopyProjectOutputsTaskMessage);
            foreach (var project in this.Projects)
            {
                if (project.MetadataNames.Cast<string>().Contains(DeployDirMetadata))
                {
                    string identity = project.GetMetadata(IdentityMetadata);
                    string projectFullPath = project.GetMetadata(FullPathMetdata);
                    string relativeDeployDir = project.GetMetadata(DeployDirMetadata);
                    string fullDeployDir = Path.Combine(this.OutDir, relativeDeployDir);
                    
                    base.Log.LogMessage(MessageImportance.Normal, 
                        "Copying outputs from {0} to a deployment location of {1}", 
                        identity, 
                        fullDeployDir);

                    if (!Directory.Exists(fullDeployDir))
                    {
                        base.Log.LogMessage(MessageImportance.Low,
                           "Creating deployment directory {0}",
                           fullDeployDir);

                        Directory.CreateDirectory(fullDeployDir);
                    }
                    
                    foreach (var targetItem in this.ProjectTargetOutputItems)
                    {
                        string sourceProject = targetItem.GetMetadata(SourceProjectMetadata);
                        base.Log.LogMessage(MessageImportance.Low,
                        "Output Item {0} -> Source Project {1}",
                        targetItem.ItemSpec,
                        sourceProject);
                    
                        if (String.Compare(sourceProject, projectFullPath, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            string fullTargetItemPath = targetItem.GetMetadata(FullPathMetdata);
                            string destination = Path.Combine(fullDeployDir, Path.GetFileName(fullTargetItemPath));
                            File.Copy(fullTargetItemPath, destination, true);
                            if (this.Configuration.Equals(
                                DebugConfiguration, StringComparison.OrdinalIgnoreCase))
                            {
                                string ext = Path.GetExtension(fullTargetItemPath);
                                File.Copy(
                                    fullTargetItemPath.Replace(ext, DebugSymbolExtension), 
                                    destination.Replace(ext, DebugSymbolExtension), 
                                    true);
                            }
                        }
                    }

                }

            }
            return true;
        }
    }
}
