using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.WizardFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Coldwater.Construct.Tfs.Activities.UI
{
    public class GciWorkflowForm : WizardForm
    {
        private IServiceProvider serviceProvider;
        private GciWorkflowWizardData workflowData;
        private bool initializing;
		
        private Dictionary<string, Type> pageTypeMap = new Dictionary<string, Type>();
        private Dictionary<Type, int> errorDictionary = new Dictionary<Type, int>();

        public GciWorkflowDetails WorkflowDetails
        {
            get
            {
                return this.workflowData.GetWorkflowDetails();
            }
        }

        public GciWorkflowForm(IServiceProvider provider, GciWorkflowDetails details)
        {
            if (provider == null)
            {
                throw new System.ArgumentNullException("provider");
            }
            if (details == null)
            {
                details = new GciWorkflowDetails ();
            }

            this.serviceProvider = provider;
            IBuildDefinition buildDefinition = (IBuildDefinition)provider.GetService(typeof(IBuildDefinition));

            this.workflowData = new GciWorkflowWizardData(
                buildDefinition.BuildServer.TeamProjectCollection, 
                buildDefinition.TeamProject, 
                details);
        }

        public override void OnFinish()
        {
            base.OnFinish();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void SetWizardProperties()
        {
            base.HelpButton = true;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            //base.set_Title(WizardResources.WizardTitle);
            this.MinimumSize = new System.Drawing.Size(880, 680);
            this.MaximumSize = new System.Drawing.Size(880, 680);
            //base.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.LabWorkflowWizardForm_HelpButtonClicked);
            //base.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.LabWorkflowWizardForm_HelpRequested);
            base.ShowOrientationPanel = true;
            base.OrientationPanelStepMode =  Microsoft.WizardFramework.OrientationPanelStepMode.AllDisabled;
        }

        private void LoadWizardPages()
        {
            Bitmap wizardLogo = WizardResources.WizardLogo;
            WelcomePageControl welcomeControl = new WelcomePageControl();
            //this.m_labEnvironmentDetailsControl = new LabEnvironmentDetailsControl(this.m_wizardData);
            //this.m_buildDetailsControl = new BuildDetailsControl(this.m_wizardData);
            //this.m_deploymentScriptControl = new DeploymentScriptControl(this.m_wizardData);
            
            GciWorkflowWizardPage welcomePage = new GciWorkflowWizardPage(
                this,
                welcomeControl, 
                wizardLogo, 
                WizardResources.WelcomePage_Title, 
                WizardResources.WelcomePage_StepTitle,
                null);
            base.AddPage(welcomePage);

            //LabWorkflowWizardPage labWorkflowWizardPage2 = new LabWorkflowWizardPage(this, this.m_labEnvironmentDetailsControl, runWorkflow_, WizardResources.EnvironmentLocationPageTitle, WizardResources.EnvironmentLocationPageStepTitle, null);
            //base.AddPage(labWorkflowWizardPage2);
            //this.m_pageTypeMap.Add(labWorkflowWizardPage2.get_Id(), typeof(EnvironmentDetails));
            //LabWorkflowWizardPage labWorkflowWizardPage3 = new LabWorkflowWizardPage(this, this.m_buildDetailsControl, runWorkflow_, WizardResources.BuildDetailsPageTitle, WizardResources.BuildDetailsPageStepTitle, null);
            //base.AddPage(labWorkflowWizardPage3);
            //this.m_pageTypeMap.Add(labWorkflowWizardPage3.get_Id(), typeof(BuildDetails));
            //LabWorkflowWizardPage labWorkflowWizardPage4 = new LabWorkflowWizardPage(this, this.m_deploymentScriptControl, runWorkflow_, WizardResources.DeploymentScriptPageTitle, WizardResources.DeploymentScriptPageStepTitle, null);
            //base.AddPage(labWorkflowWizardPage4);
            //this.m_pageTypeMap.Add(labWorkflowWizardPage4.get_Id(), typeof(DeploymentDetails));
            //this.m_testParametersControl = new LabWorkflowTestParametersControl(this.m_wizardData);
            //LabWorkflowWizardPage labWorkflowWizardPage5 = new LabWorkflowWizardPage(this, this.m_testParametersControl, runWorkflow_, WizardResources.TestingPageTitle, WizardResources.TestingPageStepTitle, null);
            //base.AddPage(labWorkflowWizardPage5);
            //this.m_pageTypeMap.Add(labWorkflowWizardPage5.get_Id(), typeof(RunTestDetails));
        }

        internal bool CanDeactivatePage(string pageId)
        {
            if (this.pageTypeMap.ContainsKey(pageId))
            {
                Type key = this.pageTypeMap[base.ActivePage.Id];
                if (this.errorDictionary.ContainsKey(key))
                {
                    return this.errorDictionary[key] == 0;
                }
            }
            return true;
        }

        internal void CheckAndSetButtonState()
        {
            if (this.initializing)
            {
                return;
            }
            if (this.errorDictionary.Values.All((int val) => val == 0))
            {
                base.EnableButton(ButtonType.Next, true);
            }
            else
            {
                base.EnableButton(ButtonType.Next, false);
            }
            base.EnableButton(ButtonType.Finish, 
                this.CanDeactivatePage(base.ActivePage.Id) && !this.IsLastPage());
        }

        private bool IsLastPage()
        {
            return base.NextPage == null;
        }
    }
}
