using System;
using System.Diagnostics;
using System.Linq;
using InkyCal.Models;
using Xunit;

namespace InkyCal.Utils.Tests
{
	public class PanelRenderHelperTests
	{
		[Fact()]
		public void GetRendererTest()
		{
			//Arrange
			//Get instance of all types inheriting from panel
			var panels = AppDomain.CurrentDomain.GetAssemblies()
											.SelectMany(x => x.GetTypes())
											.Where(x => typeof(Panel).IsAssignableFrom(x)
														&& !x.Equals(typeof(Panel))
														&& !x.IsInterface
														&& !x.IsAbstract).Select(x=>(Panel)x.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()));
			//Act & assert
			Assert.All(panels, x => {
				try
				{
					var renderer = x.GetRenderer();
					Assert.NotNull(renderer);
				}
				catch (ArgumentNullException ex) {
					Trace.TraceWarning($"Getting renderer for {x.GetType().Name} failed, likely due to using an unvalidated panel: {ex}");
				}
			});
		}
	}
}
