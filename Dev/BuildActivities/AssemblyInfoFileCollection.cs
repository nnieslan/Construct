//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;

namespace Construct.Tfs.Activities
{

    /// <summary>
    /// A strongly-typed list of strings to hold AssemblyInfo version files.
    /// </summary>
    [Editor("Construct.Tfs.Activities.UI.AssemblyInfoFileCollectionEditor, Construct.Tfs.Activities",
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
