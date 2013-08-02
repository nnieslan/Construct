//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;

namespace Coldwater.Construct.Tfs.Activities
{

    /// <summary>
    /// A strongly-typed list of strings to hold AssemblyInfo version files.
    /// </summary>
    [Editor("Coldwater.Construct.Tfs.Activities.UI.AssemblyInfoFileCollectionEditor, Coldwater.Construct.Tfs.Activities",
            typeof(UITypeEditor)),
            TypeConverter(typeof(AssemblyInfoFileCollectionConverter))]
    [Serializable]
    public sealed class AssemblyInfoFileCollection : Collection<string>
    {
        #region ctor

        public AssemblyInfoFileCollection()
        {

        }

        #endregion
    }

}
