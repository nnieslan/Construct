using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Lab.Workflow.Activities;

namespace Coldwater.Construct.Tfs.Activities
{
    [Serializable()]
    [Editor(
        "Coldwater.Construct.Build.Tasks.Activities.GciWorkflowEditor, Coldwater.Construct.Build.Tasks", 
        typeof(System.Drawing.Design.UITypeEditor))]
    public class GciWorkflowDetails : IEquatable<GciWorkflowDetails>
    {
        public BuildDetails BuildDetails
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public StringDictionary WorkflowSettings
        {
            get;
            set;
        }

        public GciWorkflowDetails()
        {
            this.BuildDetails = new BuildDetails();
        }

        public bool Equals(GciWorkflowDetails other)
        {
            return !object.ReferenceEquals(other, null) 
                //&& (object.Equals(this.EnvironmentDetails, other.EnvironmentDetails) 
                //&& object.Equals(this.DeploymentDetails, other.DeploymentDetails) 
                //&& object.Equals(this.BuildDetails, other.BuildDetails) 
                //&& object.Equals(this.TestParameters, other.TestParameters)) 
                && this.DoWorkflowSettingsMatch(this.WorkflowSettings, other.WorkflowSettings);
        }

        public override bool Equals(object obj)
        {
            return obj != null 
                && obj.GetType() == typeof(LabWorkflowDetails) 
                && this.Equals(obj as LabWorkflowDetails);
        }
        public static bool operator ==(GciWorkflowDetails details1, GciWorkflowDetails details2)
        {
            if (details1 == null)
            {
                return details2 == null;
            }
            return details1.Equals(details2);
        }
        public static bool operator !=(GciWorkflowDetails details1, GciWorkflowDetails details2)
        {
            return !(details1 == details2);
        }

        private bool DoWorkflowSettingsMatch(
            Dictionary<string, string> source, 
            Dictionary<string, string> other)
        {
            if (object.ReferenceEquals(source, other))
            {
                return true;
            }
            if (source == null != (other == null))
            {
                return false;
            }
            if (source == null)
            {
                return true;
            }
            if (source.Count != other.Count)
            {
                return false;
            }
            
            foreach (KeyValuePair<string, string> current in source)
            {
                try
                {
                    string a = other[current.Key];
                    if (!string.Equals(a, current.Value))
                    {
                        return false;
                    }
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
            }
            return true;
        }
	
    }
}
