using System;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Coldwater.Construct.Build.Activities.Test
{
    public class MockWorkspace : IWorkspaceTemplate
    {

        private System.Collections.Generic.List<IWorkspaceMapping> mappings = new System.Collections.Generic.List<IWorkspaceMapping>();

        public IWorkspaceMapping AddMapping(string serverItem, string localItem, WorkspaceMappingType type, WorkspaceMappingDepth depth)
        {
            var mapping = new MockWorkspaceMapping() { LocalItem = localItem, ServerItem = serverItem, Depth = depth, MappingType = type };
            this.mappings.Add(mapping);
            return mapping;
        }

        public IWorkspaceMapping AddMapping(string serverItem, string localItem, WorkspaceMappingType type)
        {
            var mapping = new MockWorkspaceMapping() { LocalItem = localItem, ServerItem = serverItem, MappingType = type };
            this.mappings.Add(mapping);
            return mapping;
        }

        public IWorkspaceMapping AddMapping()
        {
            var mapping = new MockWorkspaceMapping();// { LocalItem = localItem, ServerItem = serverItem, Depth = depth, MappingType = type };
            this.mappings.Add(mapping);
            return mapping;
        }

        public IWorkspaceMapping Cloak(string serverItem)
        {
            throw new NotImplementedException();
        }

        public string LastModifiedBy
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime LastModifiedDate
        {
            get { throw new NotImplementedException(); }
        }

        public IWorkspaceMapping Map(string serverItem, string localItem)
        {
            return this.AddMapping(serverItem, localItem, WorkspaceMappingType.Map);
        }

        public System.Collections.Generic.List<IWorkspaceMapping> Mappings
        {
            get
            {
                return this.mappings;
            }
        }

        public bool RemoveMapping(string serverItem)
        {
            throw new NotImplementedException();
        }

        public bool RemoveMapping(IWorkspaceMapping mapping)
        {
            throw new NotImplementedException();
        }


        public void CopyFrom(IWorkspaceTemplate source)
        {
            throw new NotImplementedException();
        }
    }
}

