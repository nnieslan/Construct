using System;
using System.Activities;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.TeamFoundation.Build.Client;
using Coldwater.Construct.Tfs.Activities;
using Resources = Coldwater.Construct.Build.Activities.Test.ResourceFiles;

namespace Coldwater.Construct.Build.Activities.Test
{
    [TestClass]
    public class CopyFilesTest
    {
        private static string sourcesDirectory;
        private static string destinationDirectory;

        // Declare a list of the files we'll be using for the test
        private static List<Tuple<string, string>> ResourceFiles = new List<Tuple<string, string>>() {
                new Tuple<string, string>("AssemblyInfo.cs", Resources.AssemblyInfo_cs),
                new Tuple<String, String>("AssemblyInfo.vb", Resources.AssemblyInfo_vb),
                new Tuple<String, String>("AssemblyInfo.cpp", Resources.CppVersion_rc),
            };

        
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
                if (String.IsNullOrEmpty(CopyFilesTest.sourcesDirectory))
                {
                    var pathSources = Path.Combine(Environment.CurrentDirectory, "Resources");
                    var sourcesDir = new DirectoryInfo(pathSources);

                    if (!sourcesDir.Exists)
                        sourcesDir = Directory.CreateDirectory(pathSources);

                    CopyFilesTest.sourcesDirectory = sourcesDir.FullName;
                }

                return CopyFilesTest.sourcesDirectory;
            }
        }

        private static string DestinationDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(destinationDirectory))
                {
                    var path = Path.Combine(Environment.CurrentDirectory, "Output"); 
                    var outDir = new DirectoryInfo(path);

                    if (!outDir.Exists)
                        outDir = Directory.CreateDirectory(path);

                    CopyFilesTest.destinationDirectory = outDir.FullName;
                }
                return destinationDirectory;
            }
        }

        #endregion Properties
        
        public void InitializeFiles()
        {            
            // Write each file out to the "Sources" directory with a suitable file name
            foreach (Tuple<String, String> tuple in ResourceFiles)
            {

                var file = Path.Combine(SourcesDirectory, tuple.Item1);
                using (StreamWriter writer = File.CreateText(file))
                {
                    writer.Write(tuple.Item2);
                    writer.Flush();
                    writer.Close();
                }
            }

            foreach (var file in Directory.GetFiles(DestinationDirectory))
            {
                File.Delete(file);
            }
        }

        [TestMethod]
        public void CopyFilesToValidLocationTest()
        {
            // Create an instance of our test workflow
            var workflow = new CopyFilesTestWorkflow();

            InitializeFiles();
            
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(new MockBuildDetail());

            // Set the in arguments for the workflow as specified
            workflow.SourceDir = SourcesDirectory;
            workflow.FileSpec = "AssemblyInfo.*";
            workflow.DestinationDir = DestinationDirectory;

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            var outfiles = Directory.GetFiles(DestinationDirectory);

            foreach (var file in ResourceFiles)
            {
                Assert.IsNotNull(outfiles.FirstOrDefault(f => f.EndsWith(file.Item1)));
            }
        }

        [TestMethod]
        public void CopyFilesToInvalidLocationTest()
        {
            // Create an instance of our test workflow
            var workflow = new CopyFilesTestWorkflow();

            InitializeFiles();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(new MockBuildDetail());

            // Set the in arguments for the workflow as specified
            workflow.SourceDir = SourcesDirectory;
            workflow.FileSpec = "AssemblyInfo.*";
            workflow.DestinationDir = string.Empty;

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            //TODO - figure out how to read build errors
        }

        [TestMethod]
        public void CopyFilesFromInvalidLocationTest()
        {
            // Create an instance of our test workflow
            var workflow = new CopyFilesTestWorkflow();

            InitializeFiles();

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(new MockBuildDetail());

            // Set the in arguments for the workflow as specified
            workflow.SourceDir = string.Empty;
            workflow.FileSpec = "AssemblyInfo.*";
            workflow.DestinationDir = DestinationDirectory;

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            var outfiles = Directory.GetFiles(DestinationDirectory);

            Assert.AreEqual(0, outfiles.Length);
        }
    }
}
