// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "I don't know what to do yet", Scope = "module")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "These are shared components", Scope = "module")]

[assembly: SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "I'm allowing commented code", Scope = "module")]
[assembly: SuppressMessage("Info Code Smell", "S1135:Track uses of \"TODO\" tags", Justification = "I'm allowing todo's", Scope = "module")]
[assembly: SuppressMessage("Major Code Smell", "S3358:Ternary operators should not be nested", Justification = "Yes this needs clarification", Scope = "member", Target = "~M:InkyCal.Utils.Calendar.ICalExtensions.GetEvents(System.Text.StringBuilder,System.Collections.Generic.IEnumerable{System.Uri})~System.Threading.Tasks.Task{System.Collections.Generic.List{InkyCal.Utils.Calendar.Event}}")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Two Test-calender urls are hardcoded for now", Scope = "member", Target = "~F:InkyCal.Utils.TestCalendarPanelRenderer.PublicHolidayCalenderUrl")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Two Test-calender urls are hardcoded for now", Scope = "member", Target = "~F:InkyCal.Utils.TestCalendarPanelRenderer.PhasesOfTheMoonCalenderUrl")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Two Test-calender urls are hardcoded for now", Scope = "member", Target = "~F:InkyCal.Utils.TestCalendarImagePanelRenderer.PublicHolidayCalenderUrl")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Two Test-calender urls are hardcoded for now", Scope = "member", Target = "~F:InkyCal.Utils.TestCalendarImagePanelRenderer.PhasesOfTheMoonCalenderUrl")]
[assembly: SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types", Justification = "For now intended behavior: each generic instance has it's own cache.", Scope = "member", Target = "~F:InkyCal.Utils.PdfRenderer`1._cache")]
[assembly: SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Online test-image is hardcoded for now", Scope = "member", Target = "~F:InkyCal.Utils.TestImagePanelRenderer.DemoImageUrl")]
[assembly: SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Candidate for dependency injection", Scope = "member", Target = "~M:InkyCal.Utils.NewPaperRenderer.FreedomForum.ApiClient.GetNewsPapers(System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.Collections.Generic.Dictionary{System.String,InkyCal.Utils.NewPaperRenderer.FreedomForum.Models.NewsPaper}}")]
[assembly: SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Candidate for dependency injection", Scope = "member", Target = "~M:InkyCal.Utils.NewPaperRenderer.FreedomForum.ApiClient.DownloadAsPDF(InkyCal.Utils.NewPaperRenderer.FreedomForum.Models.NewsPaper,System.Nullable{System.DateTime},System.Threading.CancellationToken)~System.Threading.Tasks.Task{System.Byte[]}")]
