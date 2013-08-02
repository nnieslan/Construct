using System;
using System.Activities;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.TeamFoundation.Build.Client;

namespace Construct.Build.Activities.Test
{
    [TestClass]
    public class CreateBranchVersionNumberTest
    {
        [TestMethod]
        public void GetVersion_DevScenario_Test()
        {
            var workflow = new CreateBranchVersionNumberTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            var detail = new MockBuildDetail();

            detail.BuildDefinition.Name = "UD_CI_1.1.00";
            detail.BuildDefinition.Workspace.Map("$/Alm/Releases/Travelport", @"d:\src\construct-tp");

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(detail);

            // Set the in arguments for the workflow as specified
            workflow.VersionFile = @"Test/BuildTasks/Resources/BuildNumber.txt";
            
            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            Assert.IsTrue(outputs.ContainsKey("BuildDetail"));
            Assert.AreEqual("1.1.00.1235", ((IBuildDetail)outputs["BuildDetail"]).BuildNumber);


        }
    }
}
