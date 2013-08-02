using System;
using System.Activities;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.TeamFoundation.Build.Client;

namespace Coldwater.Construct.Build.Activities.Test
{
    [TestClass]
    public class GetAssemblyVersionTest
    {
        [TestMethod]
        [Ignore]
        public void GetVersion_DevScenario_Test()
        {
            var workflow = new GetAssemblyersionTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            var detail = new MockBuildDetail();

            detail.BuildDefinition.Workspace.Map("$/Alm/Main", @"c:\src\Alm_Main");

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(detail);

            // Set the in arguments for the workflow as specified
            workflow.VersionFile = @"Test/Coldwater.Construct.Build.Activities.Test/Resources/AssemblyInfo.cs";
            workflow.OctetToIncrement = 4;

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();


            Assert.IsTrue(outputs.ContainsKey("BuildDetail"));
            Assert.AreNotEqual("No version found", ((IBuildDetail)outputs["BuildDetail"]).BuildNumber);


        }
    }
}
