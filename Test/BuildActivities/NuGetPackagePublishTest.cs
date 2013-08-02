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

namespace Coldwater.Construct.Build.Activities.Test
{
    [TestClass]
    public class NuGetPackagePublishTest
    {
         #region ctor

        public NuGetPackagePublishTest()
        {
        }
        
        #endregion

        #region Fields

        private static String sourcesDirectory;

      
        // Declare a list of the AssemblyInfo files we'll be using for the test
        private static List<Tuple<string, byte[]>> Packages = new List<Tuple<string, byte[]>>() {
               new Tuple<string, byte[]>("SampleNuGetPackage_1.0.0.nupkg", ResourceFiles.SampleNuGetPackage)
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
                if (String.IsNullOrEmpty(NuGetPackagePublishTest.sourcesDirectory))
                {
                    var pathSources = Path.Combine(Environment.CurrentDirectory, "Resources");
                    var sourcesDir = new DirectoryInfo(pathSources);

                    if (!sourcesDir.Exists)
                        sourcesDir = Directory.CreateDirectory(pathSources);

                    NuGetPackagePublishTest.sourcesDirectory = sourcesDir.FullName;
                }

                return NuGetPackagePublishTest.sourcesDirectory;
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
                using(FileStream stream = new FileStream(file,FileMode.Create))
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
        public void PublishNuGetPackage_ValidNugetPackageUploadScenario()
        {
            var files = InitializeFiles();
            
            // Create an instance of our test workflow
            var workflow = new NuGetPackagePublishTestWorkflow();
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // Set the in arguments for the workflow as specified
            workflow.NuGetServerApiKey = "cwsoftR0x!";
            workflow.NuGetServerUrl = "http://tfsbldsvc.cws.coldwatersoftware.com/NuGetServer/";
            workflow.PublishNuGetPackages = true;
            workflow.SourceLocation = SourcesDirectory;
            workflow.BuildNumber = "1.2.3.4";
            
            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            //TODO - validate files.
        }

        [TestMethod]
        public void PublishNuGetPackage_AdhocUploadTest()
        {
            var files = InitializeFiles();

            // Create an instance of our test workflow
            var workflow = new NuGetPackagePublishTestWorkflow();
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // Set the in arguments for the workflow as specified
            workflow.NuGetServerApiKey = "cwsoftR0x!";
            workflow.NuGetServerUrl = "http://tfsbldsvc.cws.coldwatersoftware.com/NuGetServer/";
            workflow.PublishNuGetPackages = true;
            workflow.SourceLocation = @"d:\src\construct-2012\bin\Debug";
            workflow.BuildNumber = "1.2.3.4";

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            //TODO - validate files.
        }

    }
}
