using Microsoft.WizardFramework;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Coldwater.Construct.Tfs.Activities.UI
{
    public class GciWorkflowWizardPage : WizardPage
    {
        private System.Windows.Forms.UserControl userControl;
        private GciWorkflowForm wizardForm;
        
        public override bool CanDeactivate
        {
            get
            {
                return this.wizardForm.CanDeactivatePage(base.Id);
            }
        }

        public GciWorkflowWizardPage(
            GciWorkflowForm wizardForm, 
            UserControl userControl, 
            Bitmap logo, 
            string title, 
            string linkText, 
            string helpTopic) 
            : base(wizardForm)
		{
			this.wizardForm = wizardForm;
			base.Headline = title;
			base.StepTitle = linkText;
			base.HelpKeyword = helpTopic;
			this.MinimumSize = base.Wizard.WizardFormMinSize;
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Skippable = true;
			this.Dock = System.Windows.Forms.DockStyle.None;
			this.AutoScroll = true;
			this.AutoSize = false;
			this.userControl = userControl;
			base.Controls.Add(userControl);
			this.userControl.Dock = System.Windows.Forms.DockStyle.None;
			base.PerformLayout();
			if (logo != null)
			{
				base.Logo = logo;
			}
		}
		public override void OnActivated()
		{
			base.OnActivated();
			this.wizardForm.CheckAndSetButtonState();
		}
    }
}
