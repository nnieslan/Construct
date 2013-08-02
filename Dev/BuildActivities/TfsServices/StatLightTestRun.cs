//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Construct.Build.TfsServices
{
    /// <summary>
    /// Represents a TestRun from StatLight.
    /// </summary>
    public class StatLightTestRun
    {
        /// <summary>
        /// Gets or sets the name of test run.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the start time for the test run.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the number of test executed.
        /// </summary>
        public int TestCount { get; set; }

        /// <summary>
        /// Gets or sets the count of failed tests.
        /// </summary>
        public int TestFailed { get; set; }

        /// <summary>
        /// Gets or sets the count of tests skipped.
        /// </summary>
        public int TestSkipped { get; set; }

        /// <summary>
        /// Gets or sets the count of tests successfully executed.
        /// </summary>
        public int TestPassed { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of <see cref="StatLightTestResult"/>s.
        /// </summary>
        //public List<StatLightTestResult> TestList { get; set; }
    }
}
