using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Forms;
using Coldwater.Construct.Tfs.Activities.UI;

namespace Coldwater.Construct.Tfs.Activities
{
    public class GciWorkflowEditor : UITypeEditor
    {
        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public override object EditValue(
            ITypeDescriptorContext context,
            IServiceProvider provider,
            object value)
        {
            if (provider != null)
            {
                GciWorkflowDetails labWorkflowDetails = value as GciWorkflowDetails;
                //bool flag = true;
                //if (labWorkflowDetails != null 
                //    && labWorkflowDetails.WorkflowSettings != null 
                //    && labWorkflowDetails.WorkflowSettings.ContainsKey("Version"))
                //{
                //    flag = (System.Windows.Forms.MessageBox.Show(
                //        LabActivitiesResources.Get("NewClientWarning"), 
                //        WizardResources.WizardTitle, 
                //        MessageBoxButtons.YesNo,
                //        MessageBoxIcon.Exclamation,
                //        MessageBoxDefaultButton.Button2) == DialogResult.Yes);
                //}
                //if (flag)
                //{
                using (GciWorkflowForm gciWorkflowWizardForm = new GciWorkflowForm(provider, labWorkflowDetails))
                {
                    if (gciWorkflowWizardForm != null)
                    {
                        gciWorkflowWizardForm.Start();
                        if (gciWorkflowWizardForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            value = gciWorkflowWizardForm.WorkflowDetails;
                        }
                    }
                }
                //}
            }
            return value;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            foreach (System.Attribute attribute in context.PropertyDescriptor.Attributes)
            {
                if (attribute is ReadOnlyAttribute && ((ReadOnlyAttribute)attribute).IsReadOnly)
                {
                    return System.Drawing.Design.UITypeEditorEditStyle.None;
                }
            }
            return UITypeEditorEditStyle.Modal;
        }
    }
}
