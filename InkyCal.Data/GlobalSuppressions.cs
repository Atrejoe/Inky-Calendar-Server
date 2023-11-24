// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "I don't know what to do yet", Scope = "module")]

[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Migrations pass in builders.", Scope = "namespaceanddescendants", Target = "~N:InkyCal.Data.Migrations")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Preparing for injectable repository pattern.", Scope = "type", Target = "~T:InkyCal.Data.GoogleOAuthRepository")]
[assembly: SuppressMessage("Compiler", "CS1591:Missing XML comment for publicly visible type or member", Justification = "<Pending>", Scope = "namespaceanddescendants", Target = "~N:InkyCal.Data.Migrations")]
[assembly: SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "Not a true performance issue here", Scope = "namespaceanddescendants", Target = "~N:InkyCal.Data.Migrations")]
