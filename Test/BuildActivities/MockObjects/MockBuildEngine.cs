using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;

namespace Coldwater.Construct.Build.Activities.Test.MockObjects
{
    public class MockBuildEngine : IBuildEngine
    {
        private List<CustomBuildEventArgs> customEvents = new List<CustomBuildEventArgs>();
        private List<BuildErrorEventArgs> errors = new List<BuildErrorEventArgs>();
        private List<BuildMessageEventArgs> messages = new List<BuildMessageEventArgs>();
        private List<BuildWarningEventArgs> warnings = new List<BuildWarningEventArgs>();

        public bool BuildProjectFile(
            string projectFileName, 
            string[] targetNames, 
            System.Collections.IDictionary globalProperties, 
            System.Collections.IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public int ColumnNumberOfTaskNode
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContinueOnError
        {
            get { throw new NotImplementedException(); }
        }

        public int LineNumberOfTaskNode
        {
            get { throw new NotImplementedException(); }
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            this.customEvents.Add(e);

            System.Diagnostics.Debug.WriteLine(e.Message);
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            this.errors.Add(e);

            System.Diagnostics.Debug.WriteLine(e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            this.messages.Add(e);
            System.Diagnostics.Debug.WriteLine(e.Message);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            this.warnings.Add(e);

            System.Diagnostics.Debug.WriteLine(e.Message);
        }

        public string ProjectFileOfTaskNode
        {
            get { throw new NotImplementedException(); }
        }
    }
}
