﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NachoCron {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NachoCron.Resources", typeof(Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} returned a non-zero exit code [{1}]..
        /// </summary>
        public static string JobErrored {
            get {
                return ResourceManager.GetString("JobErrored", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is already running. Skipping..
        /// </summary>
        public static string JobIsAlreadyRunning {
            get {
                return ResourceManager.GetString("JobIsAlreadyRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is running longer than specified..
        /// </summary>
        public static string JobIsRunningLongerThanExpectedError {
            get {
                return ResourceManager.GetString("JobIsRunningLongerThanExpectedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is running longer than specified..
        /// </summary>
        public static string JobIsRunningLongerThanExpectedWarning {
            get {
                return ResourceManager.GetString("JobIsRunningLongerThanExpectedWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is starting..
        /// </summary>
        public static string JobIsStarting {
            get {
                return ResourceManager.GetString("JobIsStarting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} ran successfully..
        /// </summary>
        public static string JobRanSuccessfully {
            get {
                return ResourceManager.GetString("JobRanSuccessfully", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} threw an exception..
        /// </summary>
        public static string JobThrewAnException {
            get {
                return ResourceManager.GetString("JobThrewAnException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} timed out..
        /// </summary>
        public static string JobTimedOut {
            get {
                return ResourceManager.GetString("JobTimedOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting NachoCron.
        /// </summary>
        public static string ServiceStarting {
            get {
                return ResourceManager.GetString("ServiceStarting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stopped NachoCron.
        /// </summary>
        public static string ServiceStopped {
            get {
                return ResourceManager.GetString("ServiceStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stopping NachoCron.
        /// </summary>
        public static string ServiceStopping {
            get {
                return ResourceManager.GetString("ServiceStopping", resourceCulture);
            }
        }
    }
}