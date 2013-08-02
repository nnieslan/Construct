//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Coldwater.Construct.Build.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> implementation that filters incoming lists by a regex.
    /// </summary>
    public class RegexFilterTask : Task
    {
        #region members

        private List<ITaskItem> outputList;
        private List<ITaskItem> inputList;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the Regex pattern to use for filtering.
        /// </summary>
        public string RegexPattern { get; set; }

        /// <summary>
        /// Gets or sets the name of a Metadata item to use in filtering.  Optional.
        /// </summary>
        public string ItemMetadata { get; set; }

        /// <summary>
        /// Gets the number of matched items.
        /// </summary>
        [Output]
        public int ItemCount { get; set; }

        /// <summary>
        /// Gets or sets the incoming list of items to filter.
        /// </summary>
        public ITaskItem[] ItemsToFilter
        {
            get
            {
                return this.inputList.ToArray();
            }
            set
            {
                this.inputList = new List<ITaskItem>(value);
            }
        }
		
        /// <summary>
        /// Gets or sets the list of matched items.
        /// </summary>
        [Output]
        public ITaskItem[] OutputItems
        {
            get
            {
                if (this.outputList != null)
                {
                    return this.outputList.ToArray();
                }
                return null;
            }
            set
            {
                this.outputList = new List<ITaskItem>(value);
            }
        }


        #endregion

        #region methods

        /// <summary>
        /// Executes the task by attempting to filter the incoming list of items.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            this.FilterItems();
            return (!this.Log.HasLoggedErrors);
        }

        /// <summary>
        /// The actual filtering implementation.  
        /// Uses the supplied Regex pattern to filter the list of items and return the matches.
        /// </summary>
        private void FilterItems()
        {
            base.Log.LogMessage(Strings.FilterItemsMessage);

            if (this.inputList == null)
            {
                base.Log.LogError(Strings.ItemsToFilterMissingError);
                return;
            }
            if (string.IsNullOrEmpty(this.RegexPattern))
            {
                base.Log.LogError(Strings.RegexPatternMissingError, new object[0]);
                return;
            }
            Regex parseRegex = new Regex(this.RegexPattern, RegexOptions.Compiled);
            
            this.outputList = new List<ITaskItem>();
            
            ITaskItem[] array = this.ItemsToFilter;
            for (int j = 0; j < array.Length; j++)
            {
                ITaskItem item = array[j];
                Match i = (string.IsNullOrEmpty(this.ItemMetadata) 
                        ? parseRegex.Match(item.ItemSpec) 
                        : parseRegex.Match(item.GetMetadata(this.ItemMetadata)));

                if (i.Success)
                {
                    this.outputList.Add(item);
                }
            }
            this.ItemCount = this.outputList.Count;
        }

        #endregion
    }
}
