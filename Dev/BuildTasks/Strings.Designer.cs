﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Construct.Build.Tasks {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Construct.Build.Tasks.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempting to copy project outputs to any secondary binplace locations..
        /// </summary>
        internal static string CopyProjectOutputsTaskMessage {
            get {
                return ResourceManager.GetString("CopyProjectOutputsTaskMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attemping to Filter incoming items..
        /// </summary>
        internal static string FilterItemsMessage {
            get {
                return ResourceManager.GetString("FilterItemsMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A list of ItemsToFilter is required..
        /// </summary>
        internal static string ItemsToFilterMissingError {
            get {
                return ResourceManager.GetString("ItemsToFilterMissingError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attempting to determine if edits are pending on each incoming project..
        /// </summary>
        internal static string ProjectEditsPendingMessage {
            get {
                return ResourceManager.GetString("ProjectEditsPendingMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TfsServerUrl is required..
        /// </summary>
        internal static string ProjectEditsPendingTfsMissingError {
            get {
                return ResourceManager.GetString("ProjectEditsPendingTfsMissingError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RegexPattern is required.
        /// </summary>
        internal static string RegexPatternMissingError {
            get {
                return ResourceManager.GetString("RegexPatternMissingError", resourceCulture);
            }
        }
    }
}
