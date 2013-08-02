using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Construct.Build.Tasks;
using Construct.Build.Tasks.Test.MockObjects;

namespace Construct.Build.Tasks.Test
{
    [TestClass]
    public class PowerShellTaskTest
    {
     
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PowerShellTaskFactory_CreateTaskNullTest()
        {
            MockBuildEngine engine = new MockBuildEngine();
            PowerShellTaskFactory factory = new PowerShellTaskFactory();

            ITask task = factory.CreateTask(engine);

            Assert.IsNotNull(task);
            Assert.AreEqual(typeof(PowerShellTask), task.GetType());

        }
        [TestMethod]
        public void PowerShellTaskFactory_CreateTaskTest()
        {
            MockBuildEngine engine = new MockBuildEngine();
            PowerShellTaskFactory factory = new PowerShellTaskFactory();

            Dictionary<string,TaskPropertyInfo> properties = new Dictionary<string,TaskPropertyInfo>();
            properties.Add("Input1", new TaskPropertyInfo("Input1", typeof(string), false, true));
            properties.Add("Output1", new TaskPropertyInfo("Output1", typeof(string), true, false));

            factory.Initialize("TestTask", properties, "$output1 = $input1;", engine);
            ITask task = factory.CreateTask(engine);

            Assert.IsNotNull(task);
            Assert.AreEqual(typeof(PowerShellTask), task.GetType());

        }

        [TestMethod]
        public void PowerShellTaskFactory_RunTaskTest()
        {
            MockBuildEngine engine = new MockBuildEngine();
            PowerShellTaskFactory factory = new PowerShellTaskFactory();

            Dictionary<string, TaskPropertyInfo> properties = new Dictionary<string, TaskPropertyInfo>();
            properties.Add("Input1", new TaskPropertyInfo("Input1", typeof(string), false, true));
            properties.Add("Output1", new TaskPropertyInfo("Output1", typeof(string), true, false));

            factory.Initialize("TestTask", properties, "$output1 = $input1;", engine);
            ITask task = factory.CreateTask(engine);

            Assert.IsNotNull(task);
            Assert.AreEqual(typeof(PowerShellTask), task.GetType());

            var psTask = task as PowerShellTask;
            psTask.SetPropertyValue(properties["Input1"], "my test value");
            psTask.SetPropertyValue(properties["Output1"], null); 
            Assert.IsTrue(psTask.Execute());

            var output = psTask.GetPropertyValue(properties["Output1"]);

            Assert.IsNotNull(output);
            Assert.AreEqual("my test value", output);
        }


        [TestMethod]
        public void PowerShellTaskFactory_ValidatePipelineTest()
        {
            string expectedoutputfile = Path.Combine(System.Environment.CurrentDirectory, "BuildScripts.zip");
            string taskScript = @"
            $msbuildLog.LogMessage([Microsoft.Build.Framework.MessageImportance]""High"", `
                              ""Construct PowerShell : Now adding files matching the pattern '{0}' to {1}."", $inputfilepattern, $outputfilename)
            $msbuildLog.LogMessage([Microsoft.Build.Framework.MessageImportance]""High"", `
                              ""Construct PowerShell : ENLISTROOT -> '{0}' "", $env:ENLISTROOT)
            $msbuildLog.LogMessage([Microsoft.Build.Framework.MessageImportance]""High"", `
                              ""Construct PowerShell : HostName -> '{0}' "", $host.Name)
            #create zip
            new-zip $outputfilename
            #add files to zip
            $files = @()
            get-childitem $inputfilepattern | foreach{
                write-message $_.FullName
                $files += $_.FullName
            }
            add-zip -zipFileName $outputfilename -filesToAdd $files
            $outputfile = $outputfilename
            ";

            if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("ENLISTROOT")))
            {
                System.Environment.SetEnvironmentVariable("ENLISTROOT", @"D:\src\alm\Main\");
            }

            MockBuildEngine engine = new MockBuildEngine();
            PowerShellTaskFactory factory = new PowerShellTaskFactory();

            Dictionary<string, TaskPropertyInfo> properties = new Dictionary<string, TaskPropertyInfo>();
            properties.Add("inputfilepattern", new TaskPropertyInfo("inputfilepattern", typeof(string), false, true));
            properties.Add("outputfilename", new TaskPropertyInfo("outputfilename", typeof(string), false, true));
            properties.Add("outputfile", new TaskPropertyInfo("outputfile", typeof(string), true, false));

            factory.Initialize("TestTask", properties, taskScript, engine);
            ITask task = factory.CreateTask(engine);

            Assert.IsNotNull(task);
            Assert.AreEqual(typeof(PowerShellTask), task.GetType());

            var psTask = task as PowerShellTask;
            psTask.SetPropertyValue(properties["inputfilepattern"], @"D:\src\alm\Main\bin\Debug\Build\*.*");
            psTask.SetPropertyValue(properties["outputfilename"], expectedoutputfile);
            psTask.SetPropertyValue(properties["outputfile"], null);
            psTask.BuildEngine = engine;
            Assert.IsTrue(psTask.Execute());

            var output = psTask.GetPropertyValue(properties["outputfile"]);

            Assert.IsNotNull(output);
            Assert.AreEqual(expectedoutputfile, output);
            Assert.IsTrue(File.Exists(expectedoutputfile));
        }
    }
}
