//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace Coldwater.Construct.Tfs.Activities
{
    /// <summary>
    /// Checks in pending changes to the workspace provided.
    /// </summary>
    [BuildActivity(HostEnvironmentOption.Agent)]
    public sealed class CheckIn : CodeActivity
    {
        #region properties

        /// <summary>
        /// Gets or sets the workspace to check in changes for.
        /// </summary>
        [RequiredArgument()]
        public InArgument<Workspace> Workspace { get; set; }
        
        #endregion

        #region methods

        /// <summary>
        /// Executes the check in of all pending changes for the workspace and logs the changeset number.
        /// </summary>
        /// <param name="context">The current context.</param>
        protected override void Execute(CodeActivityContext context)
        {

            Workspace wkspc = context.GetValue(this.Workspace);

            var parameters = new WorkspaceCheckInParameters(wkspc.GetPendingChanges(), Strings.NoCICheckInComment) 
            {
               OverrideGatedCheckIn = true,
               PolicyOverride = new PolicyOverrideInfo(Strings.PolicyOverrideCheckInComment, null),
               Author = System.Threading.Thread.CurrentPrincipal.Identity.Name,
               SuppressEvent = true
            };

            int changesetNumber = wkspc.CheckIn(parameters);
            if (changesetNumber < 0)
            {
                //notify of the error and undo the changes to clean up the build changes.
                context.TrackBuildError(Strings.UnableToCheckInError);
                int undoCount = wkspc.Undo("*.*", RecursionType.Full);
                context.TrackBuildMessage(
                    string.Format(CultureInfo.InvariantCulture, Strings.PendingChangesUndoneError, undoCount));
            }
            else
            {
                context.TrackBuildMessage(
                    string.Format(CultureInfo.InvariantCulture, Strings.CheckInCompleted, changesetNumber));
            }
        }
        
        #endregion
    }
}
