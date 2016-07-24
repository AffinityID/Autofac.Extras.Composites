using System.Reflection;
using System.Runtime.InteropServices;
using Autofac.Extras.Composites.Properties;
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Autofac.Extras.Composites")]
[assembly: AssemblyCompany("Affinity ID")]
[assembly: AssemblyProduct("Autofac.Extras.Composites")]
[assembly: AssemblyCopyright("Copyright © Affinity ID 2016")]
[assembly: AssemblyDescription("Provides registration for composites that would otherwise cause circular reference, for example:\r\npublic class CompositeLogger : ILogger { public CompositeLogger(ILogger[] loggers) {...} }.")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("f720984b-c7a3-4115-9350-e59d08c40a42")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfo.Version)]

namespace Autofac.Extras.Composites.Properties {
    internal static class AssemblyInfo {
        public const string Version = "0.1.0";
    }
}