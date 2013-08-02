//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace Construct.Tfs.Activities
{
    /// <summary>
    /// Executes the PowerShell script indicated on the remote machine denoted.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Controller)]
    public class QueueDeploymentBuildWorkflow : CodeActivity
    {

        private const string ContinuousIntegrationBuildDefinitionProcess_DeploymentBuildParameterName = "DeploymentBuildDefinition";
        
        #region properties

        public InArgument<string> TeamProjectCollectionUrl { get; set; }
        public InArgument<string> BuildDefinitionUri{ get; set; }
        public InArgument<string> BuildDefinitionProcessParameters { get; set; }

        #endregion

        #region methods

        protected override void Execute(CodeActivityContext context)
        {
            var url = new Uri(context.GetValue(this.TeamProjectCollectionUrl));
            var continuousIntegrationBuildDefUri = context.GetValue(this.BuildDefinitionUri);
            var buildParams = context.GetValue(this.BuildDefinitionProcessParameters);
            string buildDefinitionName;
            if (TryGetDeploymentBuildName(buildParams, out buildDefinitionName))
            {
                var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(url);

                IBuildServer buildServer = collection.GetService<IBuildServer>();
                var ciBuildDef = buildServer.GetBuildDefinition(new Uri(continuousIntegrationBuildDefUri));
                IBuildDefinition buildDef = buildServer.GetBuildDefinition(ciBuildDef.TeamProject, buildDefinitionName);
                buildServer.QueueBuild(buildDef);
            }
        }

        private bool TryGetDeploymentBuildName(string continuousBuildDefinitionParameters, out string deploymentBuildDefinitionName)
        {
            var xd = XDocument.Parse(continuousBuildDefinitionParameters);

            foreach (var element in xd.Root.Elements())
            {
                if (element.Attributes().First(x => x.Name.LocalName == "Key").Value == ContinuousIntegrationBuildDefinitionProcess_DeploymentBuildParameterName)
                {
                    deploymentBuildDefinitionName =  element.Value;
                    return true;
                }
            }
            deploymentBuildDefinitionName = string.Empty;
            return false;
        }

        #endregion
    }
}
