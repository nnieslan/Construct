using System;
using System.Activities;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;


namespace Coldwater.Construct.Build.Activities.Test
{
    [TestClass]
    public class RunPowerShellScriptTest
    {
        #region Fields

        private static String sourcesDirectory;


        // Declare a list of the AssemblyInfo files we'll be using for the test
        private static List<Tuple<string, byte[]>> Packages = new List<Tuple<string, byte[]>>() {
               new Tuple<string, byte[]>("Simple.ps1", ResourceFiles.simple_ps1)
            };

        #endregion Fields

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get;
            set;
        }

        /// <summary>
        /// Path to a "Sources" directory beneath the working directory
        /// (the TestResults folder in this case). This directory will be used
        /// to store the canonical AssemblyInfo files for testing.
        /// </summary>
        private static String SourcesDirectory
        {
            get
            {
                if (String.IsNullOrEmpty(RunPowerShellScriptTest.sourcesDirectory))
                {
                    var pathSources = Path.Combine(Environment.CurrentDirectory, "Resources");
                    var sourcesDir = new DirectoryInfo(pathSources);

                    if (!sourcesDir.Exists)
                        sourcesDir = Directory.CreateDirectory(pathSources);

                    RunPowerShellScriptTest.sourcesDirectory = sourcesDir.FullName;
                }

                return RunPowerShellScriptTest.sourcesDirectory;
            }
        }

        #endregion Properties

        public List<string> InitializeFiles()
        {
            List<string> files = new List<string>();
            // Write each file out to the "Sources" directory with a suitable file name
            foreach (Tuple<String, byte[]> tuple in Packages)
            {

                var file = Path.Combine(SourcesDirectory, tuple.Item1);
                using (FileStream stream = new FileStream(file, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {

                        writer.Write(tuple.Item2);
                        writer.Flush();
                        writer.Close();
                    }
                    stream.Close();
                }
                files.Add(file);
            }
            return files;
        }


        [TestMethod]
        public void RunPowerShellScript_ExecuteSimpleScriptScenario()
        {
            var files = InitializeFiles();

            // Create an instance of our test workflow
            var workflow = new RunPowerShellScriptTestWorkflow();
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            //File.Copy(files[0], @"d:\sample.ps1", true);

            // Set the in arguments for the workflow as specified
            workflow.ScriptLocation = @"d:\tfs-deploy.ps1";
            workflow.DropFolder = @"\\nick-sp2010\src\sp_dev\bin\debug\";
            workflow.TargetComputerName = "nick-sp2010.cws.coldwatersoftware.com";//string.Format("{0}.{1}", Environment.MachineName, Environment.GetEnvironmentVariable("USERDNSDOMAIN"));
            workflow.TargetComputerPort = 5985;
            workflow.TargetUrl = "http://nick-sp2010/sites/cctb/ccdemo";
            workflow.RunAsAccount=@"CWS\nick";
            workflow.RunAsAccountPassword = @"W0lfLogic*";
            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            Assert.Inconclusive();
        }
    }
}
