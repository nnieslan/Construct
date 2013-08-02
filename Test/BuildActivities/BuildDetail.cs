using System;
using Microsoft.TeamFoundation.Build.Client;

namespace Coldwater.Construct.Build.Activities.Test
{
      
    public class MockBuildDetail : IBuildDetail
    {
        private string buildNumber;
        private IBuildServer server = new MockBuildServer();
        private IBuildDefinition defn = new MockBuildDefinition();
        #region Events
        #pragma warning disable 0067

        public event PollingCompletedEventHandler PollingCompleted;

        public event StatusChangedEventHandler StatusChanged;

        public event StatusChangedEventHandler StatusChanging;

        #pragma warning restore 0067
        #endregion Events

        /// <summary>
        /// Override the BuildNumber property to return a value for test purposes
        /// </summary>
        public string BuildNumber
        {
            get
            {
                if (string.IsNullOrEmpty(buildNumber))
                {
                    buildNumber = String.Format("{0}_{1}.{2}.{3}.{4}",
                    Environment.MachineName,
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    DateTime.Now.Day,
                    Convert.ToInt16(DateTime.Now.TimeOfDay.TotalMinutes));
                }
                return buildNumber;
            }
            set
            {
                this.buildNumber = value;
            }
        }

        public IBuildServer BuildServer
        {
            get { return this.server; }
        }

        public IBuildDefinition BuildDefinition
        {
            get { return this.defn; }
        }

        public string TeamProject
        {
            get { return "ALM"; }
        }

        public Uri Uri
        {
            get { return new Uri("http://localhost/somebuild/"); }
        }

        #region not implemented

        #region Properties

        public IBuildAgent BuildAgent
        {
            get { throw new NotImplementedException(); }
        }

        public Uri BuildAgentUri
        {
            get { throw new NotImplementedException(); }
        }

        public IBuildController BuildController
        {
            get { throw new NotImplementedException(); }
        }

        public Uri BuildControllerUri
        {
            get { throw new NotImplementedException(); }
        }

        

        public Uri BuildDefinitionUri
        {
            get { throw new NotImplementedException(); }
        }

        public bool BuildFinished
        {
            get { throw new NotImplementedException(); }
        }

        

        public string CommandLineArguments
        {
            get { throw new NotImplementedException(); }
        }

        public BuildPhaseStatus CompilationStatus
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

        public string ConfigurationFolderPath
        {
            get { throw new NotImplementedException(); }
        }

        public Uri ConfigurationFolderUri
        {
            get { throw new NotImplementedException(); }
        }

        public string DropLocation
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

        public string DropLocationRoot
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime FinishTime
        {
            get { throw new NotImplementedException(); }
        }

        public IBuildInformation Information
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsDeleted
        {
            get { throw new NotImplementedException(); }
        }

        public bool KeepForever
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

        public string LabelName
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

        public string LastChangedBy
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime LastChangedOn
        {
            get { throw new NotImplementedException(); }
        }

        public string LogLocation
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
            get { throw new NotImplementedException(); }
        }

        public string Quality
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

        public BuildReason Reason
        {
            get { throw new NotImplementedException(); }
        }

        public string RequestedBy
        {
            get { throw new NotImplementedException(); }
        }

        public string RequestedFor
        {
            get { throw new NotImplementedException(); }
        }

        public string ShelvesetName
        {
            get { throw new NotImplementedException(); }
        }

        public string SourceGetVersion
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

        public DateTime StartTime
        {
            get { throw new NotImplementedException(); }
        }

        public BuildStatus Status
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

        public BuildPhaseStatus TestStatus
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

        

        #endregion Properties

        #region Methods

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Connect(int pollingInterval, System.ComponentModel.ISynchronizeInvoke synchronizingObject)
        {
            throw new NotImplementedException();
        }

        public IBuildDeletionResult Delete(DeleteOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildDeletionResult Delete()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {

        }

        public void FinalizeStatus(BuildStatus status)
        {

        }

        public void FinalizeStatus()
        {

        }

        public string GetServerItemForLocalItem(Guid outputGuid, int outputAge, string localPath)
        {
            throw new NotImplementedException();
        }

        public string GetServerItemForLocalItem(Guid outputGuid, int outputAge, string localPath, bool refresh)
        {
            throw new NotImplementedException();
        }

        public void Refresh(string[] informationTypes, QueryOptions queryOptions)
        {
            throw new NotImplementedException();
        }

        public void RefreshAllDetails()
        {

        }

        public void RefreshMinimalDetails()
        {

        }

        public void Save()
        {

        }

        public void Stop()
        {

        }

        public void Wait()
        {

        }

        #endregion Methods




        #endregion



        public void Connect(int pollingInterval, int timeout, System.ComponentModel.ISynchronizeInvoke synchronizingObject)
        {
            throw new NotImplementedException();
        }

        public string LastChangedByDisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<int> RequestIds
        {
            get { throw new NotImplementedException(); }
        }

        public Guid RequestIntermediateLogs()
        {
            throw new NotImplementedException();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<IQueuedBuild> Requests
        {
            get { throw new NotImplementedException(); }
        }

        public bool Wait(TimeSpan pollingInterval, TimeSpan timeout, System.ComponentModel.ISynchronizeInvoke synchronizingObject)
        {
            throw new NotImplementedException();
        }

        public bool Wait(TimeSpan pollingInterval, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}