//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Construct.Build.TfsServices;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.Lab.Workflow.Activities;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.Framework.Client;

namespace Construct.Tfs.Activities
{
    /// <summary>
    /// A <see cref="CodeActivity"/> to execute a StatLight test run 
    /// and parse the results, sending them to TFS as a TestRun.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class StatLightRunTests : CodeActivity
    {
        #region members

        private IBuildDetail buildDetail;
        private string statlightDirectory;
        private string testDirectory;
        private string[] xapFiles;
        private string resultFile;
        private string resultFileType;
        private string configuration;
        private string platform;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current <see cref="IBuildDetail"/>.
        /// </summary>
        public InArgument<IBuildDetail> BuildDetail { get; set; }

        /// <summary>
        /// Gets or sets the path to the StatLight executable suite.
        /// </summary>
        public InArgument<string> StatLightPath { get; set; }

        /// <summary>
        /// Gets or sets the target test directory
        /// </summary>
        public InArgument<string> TestDirectory { get; set; }

        /// <summary>
        /// Gets or sets the Silverlight XAP file to test.
        /// </summary>
        public InArgument<string[]> XapFiles { get; set; }

        /// <summary>
        /// Gets or sets the test result file name to use when generating output.
        /// </summary>
        public InArgument<string> ResultFile { get; set; }

        /// <summary>
        /// Gets or sets the test result file type to generate.
        /// </summary>
        /// <remarks>
        /// --ReportOutputFileType[=VALUE]
        ///   Specify the type of report output when using the
        ///   -r|--ReportOutputFile=[path]. Possible options
        ///   [StatLight | MSGenericTest | NUnit]
        ///</remarks>
        public InArgument<string> ResultFileType { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the test run based on the workflow parameters.
        /// </summary>
        public InArgument<string> Configuration { get; set; }

        /// <summary>
        /// Gets or sets the parameters for the test run based on the workflow parameters.
        /// </summary>
        public InArgument<string> Platform { get; set; }


        #endregion

        #region methods

        /// <summary>
        /// Reads in all context parameter values and sets them to member variables after validating them.
        /// </summary>
        /// <param name="context">The current workflow activity context.</param>
        /// <returns><b>True</b> if the parameters are valid, else <b>False</b>.</returns>
        private bool ValidateAndPopulateParameters(CodeActivityContext context)
        {
            this.buildDetail = context.GetValue(this.BuildDetail);
            if (this.buildDetail == null)
            {
                context.TrackBuildError(Strings.InvalidBuildDetailError);
                return false;
            }

            this.platform = context.GetValue(this.Platform);
            this.configuration = context.GetValue(this.Configuration);
            if (string.IsNullOrWhiteSpace(this.platform))
            {
                context.TrackBuildError(Strings.InvalidTestParameters);
                return false;
            }

            this.statlightDirectory = context.GetValue(this.StatLightPath);
            if (string.IsNullOrWhiteSpace(this.statlightDirectory) || !File.Exists(this.statlightDirectory))
            {
                context.TrackBuildError(string.Format(Strings.InvalidStatLightExeError, this.statlightDirectory));
                return false;
            }

            this.testDirectory = context.GetValue(this.TestDirectory);
            if (string.IsNullOrWhiteSpace(this.testDirectory) || !Directory.Exists(this.testDirectory))
            {
                context.TrackBuildError(string.Format(Strings.InvalidTestDllError, this.testDirectory));
                return false;
            }

            this.xapFiles = context.GetValue(this.XapFiles);
            if (this.xapFiles == null || this.xapFiles.Length == 0)
            {
                context.TrackBuildError(Strings.InvalidTestXapList);
                return false;
            }
            foreach (string xap in this.xapFiles)
            {
                if (string.IsNullOrWhiteSpace(xap) || !File.Exists(Path.Combine(this.testDirectory, xap)))
                {
                    context.TrackBuildError(string.Format(Strings.InvalidTestDllError, xap));
                    return false;
                }
            }

            this.resultFile = context.GetValue(this.ResultFile);
            if (string.IsNullOrWhiteSpace(this.resultFile))
            {
                context.TrackBuildMessage(string.Format(Strings.UsingDefaultTestResultFile, this.buildDetail.BuildNumber));
                resultFile = this.buildDetail.BuildNumber + ".trx";
            }

            this.resultFileType = context.GetValue(this.ResultFileType);
            if (string.IsNullOrWhiteSpace(resultFileType))
            {
                context.TrackBuildMessage(Strings.UsingDefaultTestResultFileType);
                resultFileType = "StatLight";
            }

            return true;
        }

        /// <summary>
        /// Executes the activity given the current context.
        /// </summary>
        /// <param name="context">The current workflow context.</param>
        protected override void Execute(CodeActivityContext context)
        {
            if (!this.ValidateAndPopulateParameters(context))
            {
                return;
            }
            
            StringBuilder arguments = new StringBuilder();
            foreach (string xap in this.xapFiles)
            {
                //generate the full paths to the xap and results file.
                string fullXap = Path.Combine(this.testDirectory, xap);
                arguments.AppendFormat("--XapPath=\"{0}\" ", fullXap);
            }
            resultFile = Path.Combine(this.testDirectory, resultFile);

            //build testing execution arguments based on the workflow inputs
            //ensure that the testing window is shown so that UI test cases execute correctly.
            arguments.AppendFormat("--ReportOutputFile=\"{0}\" ", this.resultFile);
            arguments.AppendFormat("--ReportOutputFileType=\"{0}\" ", this.resultFileType);
            arguments.Append("--ShowTestingBrowserHost");

            //a logging action with a flag f to denote error vs info.
            Action<string, bool> log = (s, f) =>
            {
                if (f)
                {
                    Debug.WriteLine(s);
                    context.TrackBuildError(s);
                }
                else
                {
                    Debug.WriteLine(s);
                    context.TrackBuildMessage(s);
                }
            };

            ExecuteTests(this.statlightDirectory, arguments.ToString(), log);

            if (this.resultFileType.Equals("StatLight"))
            {
                ParseResults(this.resultFile, this.buildDetail, this.platform, this.configuration, log);
            }
        }

        /// <summary>
        /// Executes StatLight using the parameter set indicated.
        /// </summary>
        /// <param name="statlightExe">
        /// The path to the StatLight executable to run.
        /// </param>
        /// <param name="args">
        /// The arguments to start the process with.
        /// </param>
        /// <param name="logMessage">
        /// An <see cref="Action"/> delegate to use for logging messages and errors.
        /// </param>
        private static void ExecuteTests(
            string statlightExe, 
            string args, 
            Action<string, bool> logMessage)
        {
            ProcessStartInfo statLightInfo = new ProcessStartInfo();
            statLightInfo.FileName = statlightExe;
            statLightInfo.Arguments = args;
            statLightInfo.UseShellExecute = false;
            statLightInfo.RedirectStandardError = true;
            statLightInfo.RedirectStandardOutput = true;

            var testRun = Process.Start(statLightInfo);
            testRun.WaitForExit();
            while (!testRun.StandardOutput.EndOfStream)
            {
                string line = testRun.StandardOutput.ReadLine();
                logMessage(line, false);
            }

            while (!testRun.StandardError.EndOfStream)
            {
                string line = testRun.StandardError.ReadLine();
                logMessage(line, true);
            }
        }

        /// <summary>
        /// Parses the StatLight test run results xml file, reading in the results and storing
        /// them into a <see cref="TestRun"/> on the TFS server using the TFS SOAP web services.
        /// </summary>
        /// <param name="resultFile">
        /// The statlight result file to parse.
        /// </param>
        /// <param name="build">
        /// The <see cref="IBuildDetail"/> to associate the tests with.
        /// </param>
        /// <param name="testParams">
        /// The <see cref="BuildDetails"/> containing the build testing configuration details.
        /// </param>
        /// <param name="logMessage">
        /// An <see cref="Action"/> delegate to use for logging information and errors.
        /// </param>
        private static void ParseResults(
            string resultFile, 
            IBuildDetail build, 
            string platform,
            string configuration,
            Action<string, bool> logMessage)
        {
            if (!File.Exists(resultFile))
            {
                logMessage(
                    string.Format(Strings.InvalidTestResultsFileError, resultFile), true);
                return;
            }

            const string statLightTestRunNameFormat = "{0} StatLight Test Run";

            ServiceClientFactory.TfsUrl =
                build.BuildServer.TeamProjectCollection.ConfigurationServer.Uri;
            ServiceClientFactory.TfsTeamCollectionUrl =
                build.BuildServer.TeamProjectCollection.Uri;

            //TestResultsServiceSoapClient trsClient = null;
            
            string buildPlatform = platform;
            string buildFlavor = configuration;

            ITestManagementService svc = build.BuildServer.TeamProjectCollection.GetService<ITestManagementService>();
            var testTeamProject = svc.GetTeamProject(build.TeamProject);
            
            
            var localTestRunResults = StatLightHelper.Parse(
                resultFile, 
                string.Format(statLightTestRunNameFormat, build.BuildNumber));
            try
            {
                //trsClient = ServiceClientFactory.CreateTestResultsServiceSoapClient();
                var userId = ReadCurrentUsersIdentity(build);

                ITestRun testRun = testTeamProject.TestRuns.Create();
                testRun.Title = localTestRunResults.Name;
                testRun.State = TestRunState.Completed;
                testRun.BuildUri = build.Uri;
                testRun.BuildNumber = build.BuildNumber;
                testRun.BuildPlatform = buildPlatform;
                testRun.BuildFlavor = buildFlavor;
                testRun.DateStarted = localTestRunResults.StartTime;
                testRun.DateCompleted = localTestRunResults.StartTime.AddMinutes(1.0);
                testRun.Iteration = build.TeamProject;
                testRun.IsAutomated = true;
                
                //testRun.Owner = userId;                
                //    StartDate = localTestRunResults.StartTime,
                //    CompleteDate = localTestRunResults.StartTime.AddMinutes(1.0),
                //    PostProcessState = 3,
                //    Iteration = build.TeamProject,
                //    Version = 1000,
                //    TeamProject = build.TeamProject

                //TestRun testRun = new TestRun()
                //{
                //    Title = localTestRunResults.Name,
                //    Owner = userId,
                //    State = 2,
                //    BuildUri = build.Uri.ToString(),
                //    BuildNumber = build.BuildNumber,
                //    BuildPlatform = buildPlatform,
                //    BuildFlavor = buildFlavor,
                //    StartDate = localTestRunResults.StartTime,
                //    CompleteDate = localTestRunResults.StartTime.AddMinutes(1.0),
                //    PostProcessState = 3,
                //    Iteration = build.TeamProject,
                //    Version = 1000,
                //    TeamProject = build.TeamProject
                //};

                //foreach (StatLightTestResult current in localTestRunResults.TestList)
                //{
                //    //testRun.AddTestPoint
                //    StatLightTestPoint point = new StatLightTestPoint();
                //    point.MostRecentResult = current;
                //    testRun.AddTestPoint(point, userId);
                //}


                testRun.Save();

                //testRun = trsClient.CreateTestRun(testRun, build.TeamProject);
                logMessage(string.Format(Strings.StatlightTestRunId, testRun.Id), false);

                //Dictionary<StatLightTestResult, TestCaseResult> resultsByTest = 
                //    new Dictionary<StatLightTestResult, TestCaseResult>();
                //int resultId = 0;
                //foreach (StatLightTestResult current in localTestRunResults.TestList)
                //{                    
                    //TestCaseResult item = new TestCaseResult
                    //{
                    //    ResolutionStateId = -1,
                    //    Owner = userId,
                    //    RunBy = userId,
                    //    TestCaseTitle = current.Name,
                    //    AutomatedTestName = current.Name,
                    //    AutomatedTestType = "Unit Test", //TODO - find a better alternative
                    //    AutomatedTestId = System.Guid.NewGuid().ToString("D"), //TODO - find a better alternative
                    //    Id = new TestCaseResultIdentifier
                    //    {
                    //        TestRunId = testRun.TestRunId,
                    //        TestResultId = resultId++
                    //    }
                    //};

                    //logMessage("Test Case Result: " + current.Name, false);
                    //resultsByTest.Add(current, item);
                //}

                //trsClient.CreateTestResults(resultsByTest.Values.ToArray(), build.TeamProject);

            //    List<ResultUpdateRequest> updateRequests = new List<ResultUpdateRequest>();
            //    foreach (StatLightTestResult current in resultsByTest.Keys)
            //    {
            //        StatLightTestResult test = current;
            //        TestCaseResult result = resultsByTest[current];
            //        updateRequests.Add(new ResultUpdateRequest
            //        {
            //            TestRunId = result.Id.TestRunId,
            //            TestResultId = result.Id.TestResultId,
            //            TestCaseResult = result
            //        });
            //        switch (test.Result)
            //        {
            //            case StatLightTestResultStatus.Pass:
            //                {
            //                    result.Outcome = 2;
            //                    break;
            //                }
            //            case StatLightTestResultStatus.Fail:
            //                {
            //                    result.Outcome = 3;
            //                    result.ErrorMessage = test.Message;
            //                    break;
            //                }
            //            case StatLightTestResultStatus.Inconclusive:
            //                {
            //                    result.Outcome = 4;
            //                    break;
            //                }
            //            case StatLightTestResultStatus.Unknown:
            //                {
            //                    result.Outcome = 0;
            //                    break;
            //                }
            //        }
            //    }
            //    trsClient.UpdateTestResults(updateRequests.ToArray(), build.TeamProject);
            }
            finally
            {
                //if (trsClient != null)
                //{
                //    trsClient.Close();
                //}
            }
        }

        ///// <summary>
        ///// Uses the TFS IdentityManagmentService to resolve the current execution user principal 
        ///// against TFS and get the user's TFS GUID.
        ///// </summary>
        ///// <returns>
        ///// A <see cref="Guid"/> representing the user's TFS ID.
        ///// </returns>
        public static TeamFoundationIdentity ReadCurrentUsersIdentity(IBuildDetail build)
        {
            Guid teamFoundationId = Guid.NewGuid();

            WindowsIdentity current = WindowsIdentity.GetCurrent();
            if (current == null)
            {
                throw new InvalidOperationException(Strings.UnableToFindWindowsUserError);
            }

            var securityService = build.BuildServer.TeamProjectCollection.GetService<IIdentityManagementService>();
            TeamFoundationIdentity[] ids = securityService.ReadIdentities(new IdentityDescriptor[] { 
                IdentityHelper.CreateTeamFoundationDescriptor(current.User) }, 
                Microsoft.TeamFoundation.Framework.Common.MembershipQuery.None,
                Microsoft.TeamFoundation.Framework.Common.ReadIdentityOptions.None);
        //    using (IdentityManagementWebServiceSoapClient imClient = 
        //        ServiceClientFactory.CreateIdentityManagementWebServiceSoapClient())
        //    {
        //        WindowsIdentity current = WindowsIdentity.GetCurrent();
        //        if (current == null)
        //        {
        //            throw new InvalidOperationException(Strings.UnableToFindWindowsUserError);
        //        }

        //        TeamFoundationIdentity[][] array = imClient.ReadIdentities(0, new string[]{ current.Name }, 0, 0);
        //        if (array == null || array.Length != 1 || array[0].Length != 1)
        //        {
        //            throw new InvalidOperationException(
        //                string.Format(Strings.InvalidTfsUserError, current.Name));
        //        }
        //        teamFoundationId = array[0][0].TeamFoundationId;
        //    }
            return ids[0];
        }


       
        #endregion
    }
}
