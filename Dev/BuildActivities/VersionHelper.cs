//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Coldwater.Construct.Tfs.Activities
{
    /// <summary>
    /// Helper class used for Version number incrementing and related values.
    /// </summary>
    public class VersionHelper
    {
        #region consts

        public const string AssemblyFileVersionAttribute = "AssemblyFileVersion";

        public const string AssemblyVersionAttribute = "AssemblyVersion";

        public const string CppFileVersionAttribute = "FileVersion";

        public const string CppProductVersionAttribute = "ProductVersion";

        public const string VersionNumberRegex = @"\d+\.\d+\.\d+\.\d+";

        public const string CppVersionNumberRegex = @"(\d+),(\d+),(\d+),(\d+)";

        public const string NuspecVersionNumberRegex = @"<version>\d+\.\d+\.\d+\.\d+</version>";

        #endregion

        #region ctor

        /// <summary>
        /// Default private constructor.
        /// </summary>
        private VersionHelper() { }

        #endregion

        #region methods

        /// <summary>
        /// Increments the build octet specified by the position.
        /// </summary>
        /// <param name="value">The version number to increment.</param>
        /// <param name="position">The octet position to increment.</param>
        /// <returns>The newly incremented <see cref="Version"/>.</returns>
        public static Version Increment(Version value, int position)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            switch (position)
            {
                case 1:
                    return new Version(value.Major + 1, 0, 0, 0);
                case 2:
                    return new Version(value.Major, value.Minor + 1, 0, 0);
                case 3:
                    return new Version(value.Major, value.Minor, value.Build + 1, 0);
                case 4:
                    return new Version(value.Major, value.Minor, value.Build, value.Revision + 1);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads and optionally increments & writes back the build number in the version file passed in.
        /// </summary>
        /// <param name="file">The version file to read in.</param>
        /// <param name="writeUpdate">A switch to denote if any incrementing should be written back to the file.</param>
        /// <param name="incrementNumber">A switch indicating if the build number should be incremented prior to returning.</param>
        /// <returns>The build number from the file, incremented by one optionally.</returns>
        public static int ReadVersionFile(string file, bool writeUpdate, bool incrementNumber)
        {
            int nextNumber = 0;
            using (FileStream fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                StreamReader streamReader = new StreamReader(fileStream);
                long num = 0L;
                string text = streamReader.ReadLine();
                while (text != null && text.StartsWith("#"))
                {
                    num = num + (long)text.Length + (long)Environment.NewLine.Length;
                    text = streamReader.ReadLine();
                }
                int num2;
                if (!int.TryParse(text, out num2))
                    num2 = -1;
                if (incrementNumber)
                    nextNumber = num2 + 1;
                else
                    nextNumber = num2;

                if (writeUpdate)
                {
                    fileStream.Position = num;
                    StreamWriter streamWriter = new StreamWriter(fileStream);
                    streamWriter.WriteLine(nextNumber.ToString());
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            return nextNumber;
        }
        #endregion
    }
}
