using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// Display the change log
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
	public partial class Changelog: ComponentBase
	{
		/// <summary>
		/// Gets the content of the changelog.
		/// </summary>
		/// <value>
		/// The content of the changelog.
		/// </value>
		public MarkupString ChangelogContent { get; private set; }

		/// <summary>
		/// Prepares changelogs
		/// </summary>
		/// <returns>
		/// A <see cref="Task" /> representing any asynchronous operation.
		/// </returns>
		protected override async Task OnInitializedAsync()
		{
			var fileName = Path.Combine(
							Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
							@"changelog.md"
							);

			var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
			var markdown = await System.IO.File.ReadAllTextAsync(fileName);
			ChangelogContent = new MarkupString(Markdown.ToHtml(markdown, pipeline));
			
		}
	}
}
