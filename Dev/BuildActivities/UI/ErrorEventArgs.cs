//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coldwater.Construct.Tfs.Activities.UI
{
    public class ErrorEventArgs<T> 
        : EventArgs 
        where T : Exception
    {
        public T Error { get; private set; }
        public ErrorEventArgs(T reason)
        {
            this.Error = reason;
        }

    }
}
