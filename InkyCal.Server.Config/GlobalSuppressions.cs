// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S1118:Utility classes should not have public constructors", Justification = "I have to figure out how to use DI and Configuration", Scope = "type", Target = "~T:InkyCal.Server.Config.GoogleOAuth")]
