using System;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Construct.Build.Activities.Test
{
    public class MockWorkspaceMapping : IWorkspaceMapping
    {

        public WorkspaceMappingDepth Depth
        {
            get;
            set;
        }

        public string LocalItem
        {
            get;
            set;
        }

        public WorkspaceMappingType MappingType
        {
            get;
            set;
        }

        public string ServerItem
        {
            get;
            set;
        }
    }

}
