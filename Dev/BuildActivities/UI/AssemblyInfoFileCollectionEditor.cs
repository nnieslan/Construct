//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace Coldwater.Construct.Tfs.Activities.UI
{
    /// <summary>
    /// An <see cref="UITypeEditor"/> implementation that launches the <see cref="AssemblyInfoFilesDialog"/>.
    /// </summary>
    public sealed class AssemblyInfoFileCollectionEditor : UITypeEditor
    {
        /// <summary>
        /// Edits <see cref="AssemblyInfoFileCollection"/>s using the <see cref="AssemblyInfoFilesDialog"/>.
        /// </summary>
        /// <param name="context">The current context.</param>
        /// <param name="provider">The current <see cref="IServiceProvider"/>.</param>
        /// <param name="value">The value to edit.</param>
        /// <returns>The edited Value.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            AssemblyInfoFileCollection list = value as AssemblyInfoFileCollection;
            if (list == null)
            {
                list = new AssemblyInfoFileCollection();
            }

            using (AssemblyInfoFilesDialog control = new AssemblyInfoFilesDialog())
            {
                control.AssemblyInfoFiles = list;
                if (control.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    value = control.AssemblyInfoFiles;
                }
            }

            return value;
        }

        /// <summary>
        /// Denotes that the Edit Style is Modal.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
