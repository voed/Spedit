﻿using System.Reflection;
using System.Runtime.InteropServices;
using System.Resources;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Spedit")]
[assembly: AssemblyDescription("SPEdit - a lightweight sourcepawn editor")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SPEdit")]
[assembly: AssemblyCopyright("Copyright © Julien Kluge 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en-US")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("11db2f22-8d7c-4a10-a940-b103e4e3bdf2")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
#if (DEBUG)
[assembly: AssemblyVersion("1.13.*")]
#else
[assembly: AssemblyVersion("1.2.0.3")]
#endif
