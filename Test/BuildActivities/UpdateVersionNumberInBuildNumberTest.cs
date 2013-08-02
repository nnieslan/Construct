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
    public class UpdateVersionNumberInBuildNumberTest 
    {
        [TestMethod()]
        public void ReplacePlaceholderTest()
        {
            // Create an instance of our test workflow
            var workflow = new UpateVersionNumberInBuildNumberTestWorkflow();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            var detail = new MockBuildDetail();

            detail.BuildDefinition.Name = "UD_CI_1.1.00";
            
            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(detail);

            // Set the in arguments for the workflow as specified
            workflow.BuildNumberFormat = "$(BuildDefinitionName)_$(Version)_Something";
            workflow.VersionNumber = "1.2.3.4";

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            Assert.IsTrue(outputs.ContainsKey("UpdatedBuildNumberFormat"));
            Assert.AreEqual("UD_CI_1.2.3.4_Something", outputs["UpdatedBuildNumberFormat"]);
        }
    }
}
