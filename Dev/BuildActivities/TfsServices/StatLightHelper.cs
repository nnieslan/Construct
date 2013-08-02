//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace Construct.Build.TfsServices
{
    /// <summary>
    /// Helper object used for parsing and translating statlight result files.
    /// </summary>
    public  class StatLightHelper
    {
        #region consts

        const string TestResultDocumentElement = "StatLightTestResults";
        const string TestResultDateElement = "dateRun";
        const string TestResultTotalElement = "total";
        const string TestResultFailedElement = "failed";
        const string TestResultIgnoredElement = "ignored";
        const string TestElement = "test";
        const string TestNameElement = "name";
        const string TestResultTypeElement = "resulttype";
        const string TestTimeToCompleteElement = "timeToComplete";
        const string TestExceptionInfoElement = "exceptionInfo";
        const string TestExceptionMessageElement = "message";
        const string TestExceptionStackElement = "stackTrace";

        #endregion

        #region ctor

        /// <summary>
        /// A private constructor to stop autogeneration of a public one since all method are static.
        /// </summary>
        private StatLightHelper() { }

        #endregion

        /// <summary>
        /// Parse the output StatLight result file into a an object graph.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal static StatLightTestRun Parse(string filename, string testRunName)
        {
            XDocument xDocument = XDocument.Load(filename);
            XElement xElement = xDocument.Element(TestResultDocumentElement);
            StatLightTestRun testRun = new StatLightTestRun()
            {
                Name = testRunName,
                StartTime = DateTime.Parse(xElement.Attribute(TestResultDateElement).Value),
                TestCount = int.Parse(xElement.Attribute(TestResultTotalElement).Value),
                TestFailed = int.Parse(xElement.Attribute(TestResultFailedElement).Value),
                TestSkipped = int.Parse(xElement.Attribute(TestResultIgnoredElement).Value),
            };
            testRun.TestPassed = testRun.TestCount - testRun.TestFailed - testRun.TestSkipped;


            //List<StatLightTestResult> list = new List<StatLightTestResult>();
            //foreach (XElement current in xElement.Descendants(TestElement))
            //{
            //    StatLightTestResult test = new StatLightTestResult
            //    {
            //        Name = current.Attribute(TestNameElement).Value,
            //        Outcome = Map(current.Attribute(TestResultTypeElement).Value),
            //        Duration = TimeSpan.Parse(current.Attribute(TestTimeToCompleteElement).Value)
            //    };

            //    if (test.Outcome == TestOutcome.Failed)
            //    {
            //        XElement exception = current.Element(TestExceptionInfoElement);
            //        if (exception != null)
            //        {
            //            XElement message = exception.Element(TestExceptionMessageElement);
            //            if (message != null)
            //            {
            //                test.Message = message.Value;
            //            }
            //            XElement stack = exception.Element(TestExceptionStackElement);
            //            if (stack != null)
            //            {
            //                test.StackTrace = stack.Value;
            //            }
            //        }
            //    }
            //    list.Add(test);
            //}
            //testRun.TestList = list;
            return testRun;
        }

        /// <summary>
        /// String to <see cref="TestOutcome"/> conversion.
        /// </summary>
        /// <param name="text">The string to transform.</param>
        /// <returns>The analogous enum value.</returns>
        internal static TestOutcome Map(string text)
        {
            if (text == "Passed")
            {
                return TestOutcome.Passed;
            }
            if (text == "Failed")
            {
                return TestOutcome.Failed;
            }
            return TestOutcome.Unspecified;
        }
    }
}
