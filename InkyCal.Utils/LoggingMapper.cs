namespace InkyCal.Utils
{
	internal static class LoggingMapper { 
		public static Bugsnag.Severity MapToBugSnag(this Severity severity) 
		=> severity switch
		{
			Severity.Info => Bugsnag.Severity.Info,
			Severity.Warning => Bugsnag.Severity.Warning,
			Severity.Error => Bugsnag.Severity.Error,
			_ => Bugsnag.Severity.Error,
		};
	}
}
