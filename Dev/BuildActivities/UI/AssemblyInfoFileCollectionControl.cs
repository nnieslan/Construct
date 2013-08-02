//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System.Windows.Forms;
using System.Linq;

namespace Coldwater.Construct.Tfs.Activities.UI
{
    /// <summary>
    /// A Multi-line <see cref="TextBox"/> control that displays a list of AssemblyInfo files (relative path to current branch).
    /// </summary>
    public sealed class AssemblyInfoFileCollectionControl :  UserControl
    {
        #region members

        private AssemblyInfoFileCollection assemblyInfoFiles;
		private TextBox listFiles;

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the wrapped list of files providing data.
        /// </summary>
        public AssemblyInfoFileCollection AssemblyInfoFiles
		{
			get
			{
                return this.assemblyInfoFiles;
			}
			set
			{
                this.assemblyInfoFiles = value;
			}
		}
		
        #endregion

        #region ctor
        
        /// <summary>
        /// Instantiates a new instance of the <see cref="AssemblyInfoFileCollectionControl"/>.
        /// </summary>
        public AssemblyInfoFileCollectionControl()
		{
			this.InitializeComponent();
        }
        
        #endregion

        #region methods

        /// <summary>
        /// Initializes the control by setting the text box contents to the list of existing files.
        /// </summary>
        public void Initialize()
        {
            this.listFiles.Lines = this.AssemblyInfoFiles.ToArray();
        }

        /// <summary>
        /// Reads the text box, assigning it's content to the list of assembly info files.
        /// </summary>
        public void Parselist()
        {
            this.AssemblyInfoFiles.Clear();
            foreach (string line in this.listFiles.Lines)
            {
                this.AssemblyInfoFiles.Add(line);
            }
        }
		
        /// <summary>
        /// Standard control initialization.
        /// </summary>
		private void InitializeComponent()
		{
			this.listFiles = new TextBox();
			base.SuspendLayout();
            this.listFiles.BorderStyle = BorderStyle.None;
            this.listFiles.Multiline = true;
            this.listFiles.AcceptsReturn = true;
            this.listFiles.AcceptsTab = false;
            
            this.listFiles.Name = "listFiles";
            
            base.AutoScaleMode = AutoScaleMode.Font;
            base.Controls.Add(this.listFiles);
            base.Name = "AssemblyInfoFileCollectionControl";
			base.ResumeLayout(false);
        }

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Flag denoting if currently disposing.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion
    }
}
