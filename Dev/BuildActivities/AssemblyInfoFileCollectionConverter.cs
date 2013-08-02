//==============================================================================
// Copyright (c) Coldwater Software. All Rights Reserved.
//==============================================================================

using System;
using System.ComponentModel;
using System.Text;

namespace Coldwater.Construct.Tfs.Activities
{
    /// <summary>
    /// A <see cref="TypeConverter"/> to convert the list of AssemblyInfoFiles into a semi-colon delimited string.
    /// </summary>
    public sealed class AssemblyInfoFileCollectionConverter : TypeConverter
    {
        /// <summary>
        /// Can convert to String or anything the base list type can.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Converts to the semi-colon list or handsoff to base implementation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(
            ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture,
            object value,
            Type destinationType)
        {
            if (destinationType == typeof(string) && value is AssemblyInfoFileCollection)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string current in (AssemblyInfoFileCollection)value)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(";");
                    }
                    stringBuilder.Append(current);
                }
                return stringBuilder.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
