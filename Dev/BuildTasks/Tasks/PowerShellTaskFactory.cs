//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;

namespace Coldwater.Construct.Build.Tasks
{
    /// <summary>
    /// An implementation of <see cref="ITaskFactory"/> that creates <see cref="PowerShellTask"/>s.
    /// </summary>
    public class PowerShellTaskFactory : ITaskFactory
    {
        #region members

        /// <summary>
        /// The current task's set of task parameters.
        /// </summary>
        private IDictionary<string, TaskPropertyInfo> parameters;
        
        private string script;

        #endregion

        #region properties

        /// <summary>
        /// Gets the name of the TaskFactory.
        /// </summary>
        public string FactoryName
        {
            get
            {
                return base.GetType().Name;
            }
        }

        /// <summary>
        /// Gets the type of <see cref="ITask"/> this factory creates.
        /// </summary>
        public Type TaskType
        {
            get
            {
                return typeof(PowerShellTask);
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Initializes the factory using the input data provided.
        /// </summary>
        /// <param name="taskName">The name of the task.</param>
        /// <param name="parameterGroup">The set of parameters for this task.</param>
        /// <param name="taskBody">The PowerShell script content of task.</param>
        /// <param name="taskFactoryLoggingHost">The logging build engine host.</param>
        /// <returns>True.</returns>
        public bool Initialize(
            string taskName, 
            IDictionary<string, TaskPropertyInfo> parameterGroup, 
            string taskBody, 
            IBuildEngine taskFactoryLoggingHost)
        {
            this.parameters = parameterGroup;
            
            this.script = taskBody;

            return true;
        }
        
        /// <summary>
        /// Creates a new task instance based on the initialization parameters.
        /// </summary>
        /// <param name="taskFactoryLoggingHost">The logging build engine host.</param>
        /// <returns>The newly created <see cref="ITask"/>.</returns>
        public ITask CreateTask(IBuildEngine taskFactoryLoggingHost)
        {
            return new PowerShellTask(this.script);
        }
        
        /// <summary>
        /// If the task is <see cref="IDisposable"/>, it cleans it up.
        /// </summary>
        /// <param name="task">The task to dispose of.</param>
        public void CleanupTask(ITask task)
        {
            IDisposable disposableTask = task as IDisposable;
            if (disposableTask != null)
            {
                disposableTask.Dispose();
            }
        }
        
        /// <summary>
        /// Returns the task parameters.
        /// </summary>
        /// <returns>An array of <see cref="TaskPropertyInfo"/>.</returns>
        public TaskPropertyInfo[] GetTaskParameters()
        {
            return this.parameters.Values.ToArray<TaskPropertyInfo>();
        }

        #endregion
    }
}
