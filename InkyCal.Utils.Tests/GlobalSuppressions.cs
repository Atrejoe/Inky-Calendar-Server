// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "I don't know what to do yet", Scope = "module")]
[assembly: SuppressMessage(
	"Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores",
	Justification = "Test methods require underscores for readability.",
	Scope = "namespaceanddescendants", Target = "InkyCal.Server.Shared")]
