//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation.Runspaces;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Construct.Build.Tasks
{
    public sealed class PowerShellTask : Task, IGeneratedTask, ITask, IDisposable
    {
        #region members

        private const string FormattedExceptionMessage = "Powershell Exception : {0} \nStackTrace : {1}";
        
        private Dictionary<string, object> taskProperties 
            = new Dictionary<string, object>();

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the powershell script snippet to execute.
        /// </summary>
        private string Script { get; set; }

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new <see cref="PowerShellTask"/> based on the script passed in.
        /// </summary>
        /// <param name="script">The PowerShell script snippet to execute.</param>
        internal PowerShellTask(string script) 
        {
            if (string.IsNullOrEmpty(script))
            {
                throw new ArgumentNullException("script");
            }

            this.Script = script;
        }

        #endregion

        #region methods

        /// <summary>
        /// Gets the value for the <see cref="TaskPropertyInfo"/> indicated.
        /// </summary>
        /// <param name="property">The property to fetch a value for.</param>
        /// <returns>The property value if it exists, else null.</returns>
        public object GetPropertyValue(TaskPropertyInfo property)
        {
            object value = null;
            if (this.taskProperties.TryGetValue(property.Name, out value))
            {
                return value;
            }

            return null;
        }
        
        /// <summary>
        /// Sets a value to the <see cref="TaskPropertyInfo"/> indicated in the task's property bag.
        /// </summary>
        /// <param name="property">The property to set.</param>
        /// <param name="value">The value to set it to.</param>
        public void SetPropertyValue(TaskPropertyInfo property, object value)
        {
            string key = property.Name;
            if (!this.taskProperties.ContainsKey(key))
            {
                this.taskProperties.Add(key, null);
            }
            this.taskProperties[key] = value;
        }

        /// <summary>
        /// Helper function that seeds a PowerShell <see cref="Pipeline"/> with the session state from this task instance.
        /// </summary>
        /// <param name="pipeline">The PowerShell pipeline runspace to seed.</param>
        private void SeedPipeline(Pipeline pipeline)
        {
            foreach (string key in this.taskProperties.Keys)
            {
                pipeline.Runspace.SessionStateProxy.SetVariable(key, this.taskProperties[key]);
            }

            pipeline.Runspace.SessionStateProxy.SetVariable("msbuildLog", this.Log);
        }
        
        /// <summary>
        /// Reads session state out from a <see cref="Pipeline"/> back into the property bag for this task instance.
        /// </summary>
        /// <param name="pipeline">The pipeline to read.</param>
        private void ReadPipeline(Pipeline pipeline)
        {
            var keys = this.taskProperties.Keys.ToArray();
            foreach (string key in keys)
            {
                this.taskProperties[key] = pipeline.Runspace.SessionStateProxy.GetVariable(key);
            }

        }

        /// <summary>
        /// Executes the wrapped PowerShell script in a newly seeded <see cref="Pipeline"/> and fetches any state.
        /// </summary>
        /// <returns>An indicator denoting if there are any pipeline errors from the current execution.</returns>
        public override bool Execute()
        {
            InitialSessionState s = InitialSessionState.CreateDefault();

            object constructPath = this.GetPropertyValue(new TaskPropertyInfo("ConstructDir", typeof(string), false, true));

            string constructModulePath = (string)constructPath;
            if (!string.IsNullOrWhiteSpace(constructModulePath))
            {
                constructModulePath = System.IO.Path.Combine(constructModulePath, "construct.psd1");
                
                s.ImportPSModule(new string[] {constructModulePath});
                
            }
            using (Pipeline pipeline = RunspaceFactory.CreateRunspace(s).CreatePipeline())
            {
                try
                {
                    pipeline.Commands.AddScript(this.Script);
                    pipeline.Runspace.Open();

                    this.SeedPipeline(pipeline);

                    pipeline.Invoke();

                    this.ReadPipeline(pipeline);
                }
                catch(Exception ex)//TODO - catch the right crap here.
                {
                    this.Log.LogError(string.Format(FormattedExceptionMessage, ex.Message, ex.StackTrace));
                }
            }
            return (!this.Log.HasLoggedErrors);
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing){ }
        }

        #endregion
    }
}
