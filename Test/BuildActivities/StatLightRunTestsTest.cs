using System;
using System.Activities;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Lab.Workflow.Activities;

namespace Construct.Build.Activities.Test
{
    [TestClass]
    public class StatLightRunTestsTest
    {
        [TestMethod]
        public void StatLightRunTests_ExecuteTest_DefaultResults()
        {
            var workflow = new StatLightRunTestsTestWorkflow();
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(new MockBuildDetail());

            string statlightPath = Path.Combine(Environment.CurrentDirectory, "v1.5.4260.39423");

            workflow.TestDirectory = Environment.CurrentDirectory;
            workflow.StatLightPath = Path.Combine(statlightPath, "statlight.exe");
            //workflow.ResultFileType = "StatLight";
            workflow.XapFiles = new InArgument<string[]>(
                (env) => (new string[] { @"v1.5.4260.39423\SampleSilverlightClassLibrary.Tests.xap", @"v1.5.4260.39423\SampleSilverlightClassLibrary.Tests.xap" }));
            workflow.BuildTestDetails = new InArgument<BuildDetails>(
                (env) => (new BuildDetails() { Configuration = new Microsoft.TeamFoundation.Build.Workflow.Activities.PlatformConfiguration("Debug", "AnyCPU") }));

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();
            string resultsFile = Path.Combine(
                Environment.CurrentDirectory, @"v1.5.4260.39423\SampleSilverlightClassLibrary.Tests.xap.trx");

            Assert.IsTrue(File.Exists(resultsFile));
        }

        [TestMethod]
        public void StatLightRunTests_ExecuteTest_SpecificResults()
        {
            var workflow = new StatLightRunTestsTestWorkflow();
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(new MockBuildDetail());

            string statlightPath = Path.Combine(Environment.CurrentDirectory, "v1.5.4260.39423");

            workflow.TestDirectory = Environment.CurrentDirectory;
            workflow.StatLightPath = Path.Combine(statlightPath, "statlight.exe");
            workflow.XapFiles = new InArgument<string[]>(
                (env) => (new string[] { @"v1.5.4260.39423\SampleSilverlightClassLibrary.Tests.xap", @"v1.5.4260.39423\SampleSilverlightClassLibrary.Tests.xap" }));
            workflow.BuildTestDetails = new InArgument<BuildDetails>(
                (env) => (new BuildDetails() { Configuration = new Microsoft.TeamFoundation.Build.Workflow.Activities.PlatformConfiguration("Debug", "AnyCPU") }));

            //workflow.ResultFileType = "StatLight";
            workflow.ResultsFile = "specific.trx";

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();
            string resultsFile = Path.Combine(Environment.CurrentDirectory, "specific.trx");

            Assert.IsTrue(File.Exists(resultsFile));
        }
    }
}
