using System;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Coldwater.Construct.Build.Activities.Test
{
    public class MockBuildDefinition : IBuildDefinition
    {
        private IBuildServer server = new MockBuildServer();
        private IWorkspaceTemplate wkspc = new MockWorkspace();

        public IWorkspaceTemplate Workspace
        {
            get { return this.wkspc; }
        }


        public IBuildServer BuildServer
        {
            get { return this.server; }
        }

        #region not implemented


        public IRetentionPolicy AddRetentionPolicy(BuildReason reason, BuildStatus status, int numberToKeep, DeleteOptions deleteOptions)
        {
            throw new NotImplementedException();
        }

        public ISchedule AddSchedule()
        {
            throw new NotImplementedException();
        }

        public IBuildController BuildController
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Uri BuildControllerUri
        {
            get { throw new NotImplementedException(); }
        }

        public string ConfigurationFolderPath
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int ContinuousIntegrationQuietPeriod
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ContinuousIntegrationType ContinuousIntegrationType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IBuildRequest CreateBuildRequest()
        {
            throw new NotImplementedException();
        }

        public IBuildDetail CreateManualBuild(string buildNumber, string dropLocation, BuildStatus buildStatus, IBuildController controller, string requestedFor)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail CreateManualBuild(string buildNumber, string dropLocation)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail CreateManualBuild(string buildNumber)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method has been deprecated. Please remove all references.", true)]
        public IProjectFile CreateProjectFile()
        {
            throw new NotImplementedException();
        }


        public IBuildDefinitionSpec CreateSpec()
        {
            throw new NotImplementedException();
        }

        public IBuildAgent DefaultBuildAgent
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Uri DefaultBuildAgentUri
        {
            get { throw new NotImplementedException(); }
        }

        public string DefaultDropLocation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Enabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Id
        {
            get { throw new NotImplementedException(); }
        }

        public Uri LastBuildUri
        {
            get { throw new NotImplementedException(); }
        }

        public string LastGoodBuildLabel
        {
            get { throw new NotImplementedException(); }
        }

        public Uri LastGoodBuildUri
        {
            get { throw new NotImplementedException(); }
        }

        public IProcessTemplate Process
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ProcessParameters
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IBuildDetail[] QueryBuilds()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.Dictionary<BuildStatus, IRetentionPolicy> RetentionPolicies
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.List<IRetentionPolicy> RetentionPolicyList
        {
            get { throw new NotImplementedException(); }
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.List<ISchedule> Schedules
        {
            get { throw new NotImplementedException(); }
        }

        
        public string FullPath
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get;
            set;
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public string TeamProject
        {
            get { throw new NotImplementedException(); }
        }

        public Uri Uri
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
