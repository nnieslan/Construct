using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Lab.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Coldwater.Construct.Tfs.Activities.UI
{
    internal class GciWorkflowWizardData
    {
        private TfsTeamProjectCollection tfsServer;
        private string teamProject;
        private GciWorkflowDetails gciWorkflowDetails;
        //private EnvironmentDetails m_environmentDetails = new EnvironmentDetails();
        //private BuildDetails m_buildDetails;
        //private DeploymentDetails m_deploymentDetails;
        //private RunTestDetails m_testParameters;
        //private readonly LabEnvironmentDisposition m_defaultDisposition = LabEnvironmentDisposition.Active;
        //private List<IBuildDefinition> m_definitionList;
        //private List<BuildDefinitionEx> m_buildDefinitionList;
        //private ICollection<LabEnvironment> leList;
        //private ICollection<TeamProjectHostGroup> tphgList;
        internal event EventHandler DataLoadCompleted;
        internal event EventHandler<ErrorEventArgs<Exception>> DataLoadFailed;
        internal TfsTeamProjectCollection Server
        {
            get
            {
                return this.tfsServer;
            }
        }
        internal string TeamProject
        {
            get
            {
                return this.teamProject;
            }
        }
  
        //internal System.Collections.Generic.ICollection<LabEnvironment> LabEnvironmentList
        //{
        //    get
        //    {
        //        return this.leList;
        //    }
        //}
        //internal System.Collections.Generic.ICollection<TeamProjectHostGroup> HostGroupList
        //{
        //    get
        //    {
        //        return this.tphgList;
        //    }
        //}
        //internal LabEnvironmentDisposition DefaultDisposition
        //{
        //    get
        //    {
        //        return this.m_defaultDisposition;
        //    }
        //}
        internal GciWorkflowWizardData(TfsTeamProjectCollection server, string teamProject, GciWorkflowDetails workflowDetails)
        {
            if (server == null)
            {
                throw new System.ArgumentNullException("server");
            }
            if (string.IsNullOrEmpty(teamProject))
            {
                throw new System.ArgumentNullException("teamProject");
            }
            this.tfsServer = server;
            this.teamProject = teamProject;
            this.gciWorkflowDetails = workflowDetails;
            if (this.gciWorkflowDetails == null)
            {
                this.gciWorkflowDetails = new GciWorkflowDetails();
            }

            //this.m_buildDetails = new BuildDetails();
            //this.m_deploymentDetails = new DeploymentDetails(this);
            //this.m_testParameters = new RunTestDetails(this.Server, this.TeamProject, this.gciWorkflowDetails.TestParameters);
        }
        private void RaiseEvent(EventHandler eventToRaise)
        {
            if (eventToRaise != null)
            {
                eventToRaise(this, null);
            }
        }
        protected void RaiseLoadFailureEvent(Exception reason)
        {
            if (this.DataLoadFailed != null)
            {
                this.DataLoadFailed(this, new ErrorEventArgs<Exception>(reason));
            }
        }
        private void LoadData()
        {
            System.ComponentModel.BackgroundWorker backgroundWorker = new System.ComponentModel.BackgroundWorker();
            backgroundWorker.DoWork += delegate(object sender, System.ComponentModel.DoWorkEventArgs e)
            {
                
                var service = (IBuildServer)this.Server.GetService(typeof(IBuildServer));
                //TODO - load information about other builds.

                //this.leList = service.QueryLabEnvironments(new LabEnvironmentQuerySpec
                //{
                //    Project = this.TeamProject,
                //    Disposition = LabEnvironmentDisposition.Active
                //});
                //this.tphgList = service.QueryTeamProjectHostGroups(new TeamProjectHostGroupQuerySpec
                //{
                //    Project = this.TeamProject
                //});
                //IBuildServer service2 = this.Server.GetService<IBuildServer>();
                //this.m_definitionList = service2.QueryBuildDefinitions(this.TeamProject).ToList<IBuildDefinition>();
                //this.m_testParameters.LoadTestPlans();
            }
            ;
            backgroundWorker.RunWorkerCompleted += delegate(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    this.RaiseLoadFailureEvent(e.Error);
                    this.OnError(e.Error);
                    return;
                }
                this.InitializeData();
                this.RaiseEvent(this.DataLoadCompleted);
            }
            ;
            backgroundWorker.RunWorkerAsync();
        }
        private void OnError(System.Exception ex)
        {
        }
        private void InitializeData()
        {
            try
            {
                if (this.gciWorkflowDetails != null)
                {
                    //this.InitializeEnvironmentDetails();
                    //this.InitializeBuildDetails();
                    //this.InitializeDeploymentDetails();
                    //this.m_testParameters.InitializeData();
                }
            }
            catch (System.Exception ex)
            {
                this.OnError(ex);
            }
        }
        //private void InitializeEnvironmentDetails()
        //{
        //    this.EnvironmentDetails.LabEnvironment = this.leList.FirstOrDefault((LabEnvironment le) => le.Uri.Equals(this.m_labWorkflowDetails.EnvironmentDetails.LabEnvironmentUri));
        //    this.EnvironmentDetails.EnvironmentType = ((this.EnvironmentDetails.LabEnvironment == null) ? this.m_defaultDisposition : this.EnvironmentDetails.LabEnvironment.Disposition);
        //    if (!string.IsNullOrEmpty(this.m_labWorkflowDetails.EnvironmentDetails.HostGroupName))
        //    {
        //        this.EnvironmentDetails.HostGroup = this.HostGroupList.FirstOrDefault((TeamProjectHostGroup hg) => string.Equals(hg.Name, this.m_labWorkflowDetails.EnvironmentDetails.HostGroupName, System.StringComparison.OrdinalIgnoreCase));
        //    }
        //    this.EnvironmentDetails.NewLabEnvironmentName = this.m_labWorkflowDetails.EnvironmentDetails.NewLabEnvironmentName;
        //    if (this.EnvironmentDetails.LabEnvironment != null)
        //    {
        //        this.EnvironmentDetails.RevertToSnapshot = this.m_labWorkflowDetails.EnvironmentDetails.RevertToSnapshot;
        //        this.EnvironmentDetails.SnapshotName = this.m_labWorkflowDetails.EnvironmentDetails.SnapshotName;
        //    }
        //}
        //private void InitializeBuildDetails()
        //{
        //    this.BuildDetails.IsTeamSystemBuild = this.m_labWorkflowDetails.BuildDetails.IsTeamSystemBuild;
        //    this.BuildDetails.QueueNewBuild = this.m_labWorkflowDetails.BuildDetails.QueueNewBuild;
        //    this.BuildDetails.CustomBuildPath = this.m_labWorkflowDetails.BuildDetails.CustomBuildPath;
        //    if (this.BuildDefinitionList != null)
        //    {
        //        this.BuildDetails.BuildDefinition = this.BuildDefinitionList.FirstOrDefault((BuildDefinitionEx def) => def.Definition.Uri.Equals(this.m_labWorkflowDetails.BuildDetails.BuildDefinitionUri));
        //    }
        //    if (this.BuildDetails.BuildList != null)
        //    {
        //        if (this.m_labWorkflowDetails.BuildDetails.BuildUri == null)
        //        {
        //            this.BuildDetails.BuildDetail = this.BuildDetails.BuildList.FirstOrDefault((BuildDetailEx build) => build.IsLatestOption);
        //        }
        //        else
        //        {
        //            this.BuildDetails.BuildDetail = this.BuildDetails.BuildList.FirstOrDefault((BuildDetailEx build) => !build.IsLatestOption && build.Build.Uri.Equals(this.m_labWorkflowDetails.BuildDetails.BuildUri));
        //            if (this.BuildDetails.BuildDetail == null)
        //            {
        //                this.BuildDetails.BuildDetail = this.BuildDetails.BuildList.FirstOrDefault((BuildDetailEx build) => build.IsLatestOption);
        //            }
        //        }
        //    }
        //    this.BuildDetails.Configuration = ((this.m_labWorkflowDetails.BuildDetails.Configuration == null || this.BuildDetails.ConfigurationList == null) ? null : this.BuildDetails.ConfigurationList.FirstOrDefault((PlatformConfiguration config) => Microsoft.TeamFoundation.Lab.Workflow.Activities.BuildDetails.PlatformConfigurationEquals(config, this.m_labWorkflowDetails.BuildDetails.Configuration)));
        //}
        //private void InitializeDeploymentDetails()
        //{
        //    this.DeploymentDetails.DeploymentNeeded = this.m_labWorkflowDetails.DeploymentDetails.DeploymentNeeded;
        //    this.DeploymentDetails.Scripts = DeploymentScriptUtils.ToDeploymentScript(this.m_labWorkflowDetails.DeploymentDetails.Scripts);
        //    this.DeploymentDetails.PostDeploymentSnapshotNeeded = this.m_labWorkflowDetails.DeploymentDetails.TakePostDeploymentSnapshot;
        //    this.DeploymentDetails.SnapshotName = this.m_labWorkflowDetails.DeploymentDetails.PostDeploymentSnapshotName;
        //    this.DeploymentDetails.SnapshotPath = this.m_labWorkflowDetails.DeploymentDetails.PostDeploymentSnapshotPath;
        //}
        //private void SaveEnvironmentDetails(LabWorkflowDetails workflowDetails)
        //{
        //    workflowDetails.EnvironmentDetails.TfsUrl = this.Server.Uri.AbsoluteUri;
        //    workflowDetails.EnvironmentDetails.ProjectName = this.TeamProject;
        //    workflowDetails.EnvironmentDetails.RevertToSnapshot = this.EnvironmentDetails.RevertToSnapshot;
        //    workflowDetails.EnvironmentDetails.SnapshotName = this.EnvironmentDetails.SnapshotName;
        //    if (this.EnvironmentDetails.LabEnvironment != null)
        //    {
        //        LabEnvironment le = this.EnvironmentDetails.LabEnvironment;
        //        workflowDetails.EnvironmentDetails.Disposition = le.Disposition;
        //        workflowDetails.EnvironmentDetails.LabEnvironmentUri = le.Uri;
        //        workflowDetails.EnvironmentDetails.LabEnvironmentName = le.Name;
        //        if (le.Disposition == LabEnvironmentDisposition.Active)
        //        {
        //            TeamProjectHostGroup teamProjectHostGroup = this.HostGroupList.FirstOrDefault((TeamProjectHostGroup hgroup) => hgroup.Uri.Equals(le.LabLocationUri));
        //            if (teamProjectHostGroup != null)
        //            {
        //                workflowDetails.EnvironmentDetails.HostGroupName = teamProjectHostGroup.Name;
        //            }
        //            workflowDetails.EnvironmentDetails.LabLibraryShareName = null;
        //            workflowDetails.EnvironmentDetails.NewLabEnvironmentName = null;
        //        }
        //    }
        //}
        //private void SaveBuildDetails(LabWorkflowDetails workflowDetails)
        //{
        //    workflowDetails.BuildDetails.IsTeamSystemBuild = this.BuildDetails.IsTeamSystemBuild;
        //    workflowDetails.BuildDetails.QueueNewBuild = this.BuildDetails.QueueNewBuild;
        //    workflowDetails.BuildDetails.CustomBuildPath = this.BuildDetails.CustomBuildPath;
        //    workflowDetails.BuildDetails.BuildDefinitionUri = ((this.BuildDetails.BuildDefinition != null) ? this.BuildDetails.BuildDefinition.Definition.Uri : null);
        //    workflowDetails.BuildDetails.BuildDefinitionName = ((this.BuildDetails.BuildDefinition != null) ? this.BuildDetails.BuildDefinition.Definition.Name : null);
        //    workflowDetails.BuildDetails.BuildUri = ((this.BuildDetails.BuildDetail == null || this.BuildDetails.BuildDetail.IsLatestOption) ? null : this.BuildDetails.BuildDetail.Build.Uri);
        //    workflowDetails.BuildDetails.Configuration = this.BuildDetails.Configuration;
        //}
        //private void SaveDeploymentDetails(LabWorkflowDetails workflowDetails)
        //{
        //    workflowDetails.DeploymentDetails.DeploymentNeeded = this.DeploymentDetails.DeploymentNeeded;
        //    workflowDetails.DeploymentDetails.Scripts = DeploymentScriptUtils.ToStringList(this.DeploymentDetails.Scripts);
        //    workflowDetails.DeploymentDetails.TakePostDeploymentSnapshot = this.DeploymentDetails.PostDeploymentSnapshotNeeded;
        //    workflowDetails.DeploymentDetails.PostDeploymentSnapshotName = this.DeploymentDetails.SnapshotName;
        //    workflowDetails.DeploymentDetails.PostDeploymentSnapshotPath = this.DeploymentDetails.SnapshotPGciWorkflowDetailsath;
        //}
        internal void Initialize()
        {
            this.LoadData();
        }
        internal GciWorkflowDetails GetWorkflowDetails()
        {
            GciWorkflowDetails workflowDetails = new GciWorkflowDetails();
            //this.SaveEnvironmentDetails(labWorkflowDetails);
            //this.SaveBuildDetails(labWorkflowDetails);
            //this.SaveDeploymentDetails(labWorkflowDetails);
            //labWorkflowDetails.TestParameters = this.m_testParameters.GetDetails();
            return workflowDetails;
        }
    }
}
