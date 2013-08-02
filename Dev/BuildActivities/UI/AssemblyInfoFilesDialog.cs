//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Coldwater.Construct.Tfs.Activities.UI
{
    /// <summary>
    /// A Modal Dialog for editing <see cref="AssemblyInfoFileCollection"/>s.
    /// </summary>
    public sealed class AssemblyInfoFilesDialog : Form
    {
        #region members

        private AssemblyInfoFileCollectionControl control;
		private TableLayoutPanel tlpMain;
		private TableLayoutPanel tlpButtons;
		private Button buttonOk;
		private Button buttonCancel;

        #endregion
        
        #region properties

        /// <summary>
        /// Gets or sets the <see cref="AssemblyInfoFileCollection"/> this dialog is editing.
        /// </summary>
        public AssemblyInfoFileCollection AssemblyInfoFiles
		{
			get
			{
				return this.control.AssemblyInfoFiles;
			}
			set
			{
				this.control.AssemblyInfoFiles = value;
			}
		}

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new <see cref="AssemblyInfoFilesDialog"/>
        /// </summary>
        public AssemblyInfoFilesDialog()
		{
			this.InitializeComponent();
            this.control = new AssemblyInfoFileCollectionControl();
			this.tlpMain.Controls.Add(this.control, 0, 0);
		}

        #endregion

        #region methods

        /// <summary>
        /// Initializes the wrapped controls on load.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);
			
            this.control.Initialize();

			System.Drawing.Size size = base.Size;
            this.AutoSize = false;
			this.tlpMain.AutoSize = false;
			this.tlpMain.Dock = DockStyle.Fill;
			this.control.AutoSize = false;
			this.control.Dock = DockStyle.Fill;
			this.MinimumSize = size;
		}
		
        /// <summary>
        /// Parses the wrapped control's data into a list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOk_Click(object sender, System.EventArgs e)
		{
            this.control.Parselist();


		}

        /// <summary>
        /// Standard control initialization.
        /// </summary>
        private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = 
                new ComponentResourceManager(typeof(AssemblyInfoFilesDialog));

			this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
			this.tlpButtons = new System.Windows.Forms.TableLayoutPanel();
			this.buttonOk = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.tlpMain.SuspendLayout();
			this.tlpButtons.SuspendLayout();
			base.SuspendLayout();
			
            //componentResourceManager.ApplyResources(this.tlpMain, "tlpMain");
			
            this.tlpMain.Controls.Add(this.tlpButtons, 0, 1);
			this.tlpMain.Name = "tlpMain";
			
            //componentResourceManager.ApplyResources(this.tlpButtons, "tlpButtons");
			this.tlpButtons.Controls.Add(this.buttonOk, 0, 0);
			this.tlpButtons.Controls.Add(this.buttonCancel, 1, 0);
			this.tlpButtons.Name = "tlpButtons";
			
            //componentResourceManager.ApplyResources(this.buttonOk, "buttonOk");
			this.buttonOk.DialogResult = DialogResult.OK;
			this.buttonOk.MinimumSize = new System.Drawing.Size(75, 23);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new EventHandler(this.buttonOk_Click);
			
            //componentResourceManager.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.MinimumSize = new System.Drawing.Size(75, 23);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			base.AcceptButton = this.buttonOk;
			
            //componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.buttonCancel;
			base.Controls.Add(this.tlpMain);
			base.HelpButton = true;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "AssemblyInfoFilesDialog";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			this.tlpMain.ResumeLayout(false);
			this.tlpMain.PerformLayout();
			this.tlpButtons.ResumeLayout(false);
			this.tlpButtons.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
        }

        #region IDisposable Implementation

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}
