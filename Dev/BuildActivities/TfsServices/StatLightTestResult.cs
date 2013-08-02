using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace Coldwater.Construct.Build.TfsServices
{
    public class StatLightTestPoint : ITestPoint
    {

        public TeamFoundationIdentity AssignedTo
        {
            get;
            set;
        }

        public void Block()
        {
            throw new NotImplementedException();
        }

        public int ConfigurationId { get; set; }

        public string ConfigurationName { get; set; }

        private ReadOnlyObservableCollection<ITestPointProperties> _history = new ReadOnlyObservableCollection<ITestPointProperties>(
            new ObservableCollection<ITestPointProperties>());

        public ReadOnlyObservableCollection<ITestPointProperties> History
        {
            get { return _history; }
        }

        public ITestCaseResult MostRecentResult { get; set; }

        public ITestPlan Plan
        {
            get;
            private set;
        }

        public int[] QueryAssociatedWorkItemsFromResults()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public int SuiteId
        {
            get { throw new NotImplementedException(); }
        }

        public bool TestCaseExists
        {
            get { throw new NotImplementedException(); }
        }

        public int TestCaseId
        {
            get { throw new NotImplementedException(); }
        }

        public ITestCase TestCaseWorkItem
        {
            get { throw new NotImplementedException(); }
        }

        public void Unblock()
        {
            throw new NotImplementedException();
        }

        public object UserData { get; set; }

        public int Id { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Comment { get; set; }

        public DateTime LastUpdated
        {
            get { throw new NotImplementedException(); }
        }

        public TeamFoundationIdentity LastUpdatedBy
        {
            get { throw new NotImplementedException(); }
        }

        public FailureType MostRecentFailureType
        {
            get { throw new NotImplementedException(); }
        }

        public int MostRecentResolutionStateId
        {
            get { throw new NotImplementedException(); }
        }

        public int MostRecentResultId
        {
            get { return this.MostRecentResult.Id.TestResultId; }
        }

        public TestOutcome MostRecentResultOutcome
        {
            get { return this.MostRecentResult.Outcome; }
        }

        public TestResultState MostRecentResultState
        {
            get { return this.MostRecentResult.State; }
        }

        public int MostRecentRunId
        {
            get { return this.MostRecentResult.Id.TestRunId; }
        }

        public int Revision { get; set; }

        public TestPointState State
        {
            get;
            set;
        }
    }
 
    public class StatLightTestResult : ITestCaseResult
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        
        private List<string> collectors = new List<string>();
        public List<string> CollectorsEnabled
        {
            get { return this.collectors; }
        }

        public string Comment {get; set;}
        
        public DateTime DateCompleted {get; set;}
        
        public DateTime DateCreated {get; set;}
        
        public DateTime DateStarted {get; set;}
        
        public TimeSpan Duration {get; set;}
        
        public string ErrorMessage
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Message))
                    return this.Message;

                return string.Empty;
            }
        }

        public TestOutcome Outcome { get; set; }

        public event UploadCompletionEventHandler AttachmentUploadCompleted;
        
        public IAttachmentCollection Attachments {get; set;}
        
        public ITestAttachment CreateAttachment(string sourceFileName, SourceFileAction deleteOnCompletion)
        {
            throw new NotImplementedException();
        }

        public ITestAttachment CreateAttachment(string localFileName)
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ITestAttachment ActionRecording
        {
            get;
            set;
        }

        public Uri ArtifactUri
        {
            get;
            set;
        }

        public void AssociateWorkItem(Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem workItem)
        {
            throw new NotImplementedException();
        }

        public string ComputerName
        {
            get;
            set;

        }

        public ITestAttachment CreateAttachment(byte[] contents, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public ITestIterationResult CreateIteration(int iterationId)
        {
            throw new NotImplementedException();
        }

        public void DisassociateWorkItem(Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem workItem)
        {
            throw new NotImplementedException();
        }

        public FailureType FailureType
        {
            get;
            set;
        }

        public ITestCase GetTestCase()
        {
            throw new NotImplementedException();
        }

        public ITestRun GetTestRun()
        {
            throw new NotImplementedException();
        }

        public ITestImplementation Implementation
        {
            get;
            set;
        }

        public bool IsFinished
        {
            get;
            set;
        }

        public ITestIterationResultCollection Iterations
        {
            get;
            set;
        }

        public DateTime LastUpdated
        {
            get;
            set;
        }

        public Microsoft.TeamFoundation.Framework.Client.TeamFoundationIdentity LastUpdatedBy
        {
            get;
            set;
        }

        public Microsoft.TeamFoundation.Framework.Client.TeamFoundationIdentity Owner
        {
            get;
            set;
        }

        public int Priority
        {
            get;
            set;
        }

        public Microsoft.TeamFoundation.Artifact[] QueryAssociatedWorkItemArtifacts()
        {
            throw new NotImplementedException();
        }

        public int[] QueryAssociatedWorkItems()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public int ResetCount
        {
            get;
            set;
        }

        public int ResolutionStateId
        {
            get;
            set;
        }

        public int Revision
        {
            get;
            set;
        }

        public Microsoft.TeamFoundation.Framework.Client.TeamFoundationIdentity RunBy
        {
            get;
            set;
        }

        public void Save(bool uploadInBackground)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public TestResultState State
        {
            get;
            set;
        }

        public string TestCaseArea
        {
            get;
            set;
        }

        public int TestCaseId
        {
            get;
            set;
        }

        public int TestCaseRevision
        {
            get;
            set;
        }

        public string TestCaseTitle
        {
            get;
            set;
        }

        public int TestConfigurationId
        {
            get;
            set;
        }

        public string TestConfigurationName
        {
            get;
            set;
        }

        public int TestPointId
        {
            get;
            set;
        }

        public int TestResultId
        {
            get;
            set;
        }

        public int TestRunId
        {
            get;
            set;
        }


        string ITestResult.ErrorMessage
        {
            get;
            set;
        }

        public TestCaseResultIdentifier Id
        {
            get;
            set;
        }
    }
}
