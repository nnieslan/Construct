using Coldwater.Construct.Build.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Coldwater.Construct.Tfs.Activities;

namespace Coldwater.Construct.Build.Activities.Test
{
    
    
    /// <summary>
    ///This is a test class for VersionHelperTest and is intended
    ///to contain all VersionHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VersionHelperTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Increment
        ///</summary>
        [TestMethod()]
        public void IncrementMajorTest()
        {
            int position = 1; 
            Version expected = new Version("1.2.3.4"); 
            Version actual;
            actual = VersionHelper.Increment(expected, position);
            Assert.AreEqual(expected.Major+1, actual.Major);
            Assert.AreEqual(0, actual.Minor);
            Assert.AreEqual(0, actual.Build);
            Assert.AreEqual(0, actual.Revision);

        }
        /// <summary>
        ///A test for Increment
        ///</summary>
        [TestMethod()]
        public void IncrementMinorTest()
        {
            int position = 2;
            Version expected = new Version("1.2.3.4");
            Version actual;
            actual = VersionHelper.Increment(expected, position);
            Assert.AreEqual(expected.Major, actual.Major);
            Assert.AreEqual(expected.Minor+1, actual.Minor);
            Assert.AreEqual(0, actual.Build);
            Assert.AreEqual(0, actual.Revision);

        }

        /// <summary>
        ///A test for Increment
        ///</summary>
        [TestMethod()]
        public void IncrementBuildTest()
        {
            int position = 3;
            Version expected = new Version("1.2.3.4");
            Version actual;
            actual = VersionHelper.Increment(expected, position);
            Assert.AreEqual(expected.Major, actual.Major);
            Assert.AreEqual(expected.Minor, actual.Minor);
            Assert.AreEqual(expected.Build + 1, actual.Build);
            Assert.AreEqual(0, actual.Revision);

        }

        /// <summary>
        ///A test for Increment
        ///</summary>
        [TestMethod()]
        public void IncrementRevisionTest()
        {
            int position = 4;
            Version expected = new Version("1.2.3.4");
            Version actual;
            actual = VersionHelper.Increment(expected, position);
            Assert.AreEqual(expected.Major, actual.Major);
            Assert.AreEqual(expected.Minor, actual.Minor);
            Assert.AreEqual(expected.Build, actual.Build);
            Assert.AreEqual(expected.Revision + 1, actual.Revision);

        }
    }
}
