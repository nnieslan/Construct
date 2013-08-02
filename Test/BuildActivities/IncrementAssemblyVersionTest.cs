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
using Resources = Coldwater.Construct.Build.Activities.Test.ResourceFiles;

namespace Coldwater.Construct.Build.Activities.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class IncrementAssemblyVersionTest
    {
        #region ctor

        public IncrementAssemblyVersionTest()
        {
        }
        
        #endregion

        #region Fields

        private static String sourcesDirectory;

        private const string VersionNumberPattern = @"\d+\.\d+\.\d+\.\d+";

        // Declare a list of the AssemblyInfo files we'll be using for the test
        private static List<Tuple<string, string>> ResourceFiles = new List<Tuple<string, string>>() {
                new Tuple<string, string>("AssemblyInfo.cs", Resources.AssemblyInfo_cs),
                new Tuple<string, string>("Version.cs", Resources.version)//,
                //TODO - consider other languages.
                //new Tuple<String, String>("AssemblyInfo.vb", Resources.AssemblyInfo_vb),
                //new Tuple<String, String>("AssemblyInfo.cpp", Resources.AssemblyInfo_cpp),
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
                if (String.IsNullOrEmpty(IncrementAssemblyVersionTest.sourcesDirectory))
                {
                    var pathSources = Path.Combine(Environment.CurrentDirectory, "Resources");
                    var sourcesDir = new DirectoryInfo(pathSources);

                    if (!sourcesDir.Exists)
                        sourcesDir = Directory.CreateDirectory(pathSources);

                    IncrementAssemblyVersionTest.sourcesDirectory = sourcesDir.FullName;
                }

                return IncrementAssemblyVersionTest.sourcesDirectory;
            }
        }

        #endregion Properties
        
        public Dictionary<string,Version> InitializeFiles()
        {
            Dictionary<string, Version> currentVersions = new Dictionary<string, Version>();
            Regex versionEx = new Regex(VersionNumberPattern);

            // Write each file out to the "Sources" directory with a suitable file name
            foreach (Tuple<String, String> tuple in ResourceFiles)
            {

                Match match = versionEx.Match(tuple.Item2);
                if (match.Success)
                {
                    currentVersions.Add(tuple.Item1, new Version(match.Value));
                }
                var file = Path.Combine(SourcesDirectory, tuple.Item1);
                using (StreamWriter writer = File.CreateText(file))
                {
                    writer.Write(tuple.Item2);
                    writer.Flush();
                    writer.Close();
                }
            }

            return currentVersions;

        }
        
       
        [TestMethod]
        public void MainlineScenario()
        {
            // Create an instance of our test workflow
            var workflow = new IncrementAssemblyersionTestWorkflow();

            Dictionary<string, Version> currentVersions = InitializeFiles();
            Regex versionEx = new Regex(@"\d+\.\d+\.\d+\.\d+");


            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            var details = new MockBuildDetail();
            details.BuildDefinition.Workspace.Map("$/Alm/Dev/VS2012", @"d:\src\construct-2012");
            var vcs = details.BuildServer.TeamProjectCollection.GetService<VersionControlServer>();

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(details);
            
            // Set the in arguments for the workflow as specified
            workflow.Workspace = new InArgument<Workspace>((env) => vcs.GetWorkspace(Environment.CurrentDirectory));
            workflow.BuildNumber = "1.2.3.4";
            workflow.AssemblyFilesToUpdate = new InArgument<Tfs.Activities.AssemblyInfoFileCollection>(
                (env) => new Tfs.Activities.AssemblyInfoFileCollection()
            {
                @"Test\BuildActivities\Resources\AssemblyInfo.cs",
                @"Test\BuildActivities\Resources\AssemblyInfo.vb"
            });
            workflow.CppFilesToUpdate = new InArgument<Tfs.Activities.AssemblyInfoFileCollection>(
                (env) => new Tfs.Activities.AssemblyInfoFileCollection()
            {
                @"Test\BuildActivities\Resources\CppVersion.rc"
            });

            workflow.NuspecFilesToUpdate = new InArgument<Tfs.Activities.AssemblyInfoFileCollection>(
                (env) => new Tfs.Activities.AssemblyInfoFileCollection()
            {
                @"Test\BuildActivities\Resources\Sample.nuspec"
            });

            workflow.SharePointAppManifestFilesToUpdate = new InArgument<Tfs.Activities.AssemblyInfoFileCollection>(
                (env) => new Tfs.Activities.AssemblyInfoFileCollection()
            {
                @"Test\BuildActivities\Resources\SampleAppManifest.xml"
            });
            

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            //TODO - validate files.
        }

        [TestMethod]
        public void OmitAssemblyVersionScenario()
        {
            // Create an instance of our test workflow
            var workflow = new IncrementAssemblyersionTestWorkflow();

            Dictionary<string, Version> currentVersions = InitializeFiles();
            Regex versionEx = new Regex(@"\d+\.\d+\.\d+\.\d+");
            string attributeVersionEx = @"{0}\(""\d+\.\d+\.\d+\.\d+""\)";
            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(new MockBuildDetail());

            // Set the in arguments for the workflow as specified
            workflow.BuildNumber = "1.2.3.4";
            workflow.AssemblyFilesToUpdate = new Tfs.Activities.AssemblyInfoFileCollection()
            {
                @"Test\BuildTasks\Resources\AssemblyInfo.cs",
                @"Test\BuildTasks\Resources\AssemblyInfo.vb"
            };
            workflow.IncludeAssemblyVersion = false;

            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            Regex fileVersionEx = new Regex(string.Format(attributeVersionEx, "AssemblyFileVersion"));
            Regex assemblyVersionEx = new Regex(string.Format(attributeVersionEx, "AssemblyVersion"));

            // Write each file out to the "Sources" directory with a suitable file name
            foreach (Tuple<String, String> tuple in ResourceFiles.Where(i => !i.Item1.Equals("Version.cs")))
            {
                string text = File.ReadAllText(
                    Path.Combine(SourcesDirectory, tuple.Item1));

                Match filever = fileVersionEx.Match(text);
                Assert.IsTrue(filever.Success);

                Match assemblyVer = assemblyVersionEx.Match(text);
                Assert.IsTrue(assemblyVer.Success);

                Match matchInc = versionEx.Match(filever.Value);
                Assert.IsTrue(matchInc.Success);

                var v = new Version(matchInc.Value);

                Assert.AreEqual(currentVersions[tuple.Item1].Major, v.Major);
                Assert.AreEqual(currentVersions[tuple.Item1].Minor, v.Minor);
                Assert.AreEqual(currentVersions[tuple.Item1].Build, v.Build);
                Assert.AreEqual(currentVersions[tuple.Item1].Revision + 1, v.Revision);

                Match matchSame = versionEx.Match(assemblyVer.Value);
                Assert.IsTrue(matchSame.Success);
                v = new Version(matchSame.Value);
                Assert.AreEqual(currentVersions[tuple.Item1].Major, v.Major);
                Assert.AreEqual(currentVersions[tuple.Item1].Minor, v.Minor);
                Assert.AreEqual(currentVersions[tuple.Item1].Build, v.Build);
                Assert.AreEqual(currentVersions[tuple.Item1].Revision, v.Revision);
            }
        }

        [TestMethod]
        public void InvalidDirectoryTest()
        {
            // Create an instance of our test workflow
            var workflow = new IncrementAssemblyersionTestWorkflow();

            Dictionary<string, Version> currentVersions = InitializeFiles();
            Regex versionEx = new Regex(@"\d+\.\d+\.\d+\.\d+");

            // Create the workflow run-time environment
            var workflowInvoker = new WorkflowInvoker(workflow);

            // New up an instance of our mock BuildDetail object to add to the Workflow
            // environment's extensions
            workflowInvoker.Extensions.Add(new MockBuildDetail());

            // Set the in arguments for the workflow as specified
            workflow.BuildNumber = "1.2.3.4";
            workflow.AssemblyFilesToUpdate = new Tfs.Activities.AssemblyInfoFileCollection()
            {
                @"Test\BuildTasks\Resources\AssemblyInfo.cs",
                @"Test\BuildTasks\Resources\AssemblyInfo.blah"
            };
            // Invoke the workflow and capture the outputs
            IDictionary<String, Object> outputs = workflowInvoker.Invoke();

            // Write each file out to the "Sources" directory with a suitable file name
            foreach (Tuple<String, String> tuple in ResourceFiles)
            {
                string text = File.ReadAllText(
                    Path.Combine(SourcesDirectory, tuple.Item1));

                Match match = versionEx.Match(text);
                Assert.IsTrue(match.Success);

                var v = new Version(match.Value);

                Assert.AreEqual(currentVersions[tuple.Item1].Major, v.Major);
                Assert.AreEqual(currentVersions[tuple.Item1].Minor, v.Minor);
                Assert.AreEqual(currentVersions[tuple.Item1].Build, v.Build);
                Assert.AreEqual(currentVersions[tuple.Item1].Revision, v.Revision);
            }
        }
    }
}
