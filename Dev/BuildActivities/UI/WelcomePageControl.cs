using Microsoft.TeamFoundation.Client;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Coldwater.Construct.Tfs.Activities.UI
{
    internal class WelcomePageControl : UserControl
    {
        #region members

        private IContainer components;
        private Label welcomeLabel;
        private TableLayoutPanel masterLayoutPanel;

        #endregion

        #region ctor

        public WelcomePageControl()
        {
            this.InitializeComponent();
        }

        #endregion

        #region methods

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager =
                new ComponentResourceManager(typeof(WelcomePageControl));

            this.welcomeLabel = new Label();
            this.masterLayoutPanel = new TableLayoutPanel();
            this.masterLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            this.welcomeLabel.Dock = DockStyle.Fill;
            this.welcomeLabel.Font = new Font(
                "Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            this.welcomeLabel.Location = new Point(3, 3);
            this.welcomeLabel.Margin = new Padding(3);
            this.welcomeLabel.Name = "welcomeLabel";
            this.welcomeLabel.Size = new Size(524, 193);
            this.welcomeLabel.TabIndex = 0;
            this.welcomeLabel.Text = componentResourceManager.GetString("welcomeLabel.Text");

            this.masterLayoutPanel.ColumnCount = 1;
            this.masterLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.masterLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20f));
            this.masterLayoutPanel.Controls.Add(this.welcomeLabel, 0, 0);
            this.masterLayoutPanel.Location = new Point(0, 0);
            this.masterLayoutPanel.Name = "masterLayoutPanel";
            this.masterLayoutPanel.RowCount = 1;
            this.masterLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.masterLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 193f));
            this.masterLayoutPanel.Size = new Size(530, 199);
            this.masterLayoutPanel.TabIndex = 0;

            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSize = true;
            base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.Controls.Add(this.masterLayoutPanel);
            base.Name = "WelcomePageControl";
            base.Size = new Size(533, 202);
            this.masterLayoutPanel.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

    }
}
