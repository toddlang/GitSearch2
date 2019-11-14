using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitSearch2.Client.Service;
using GitSearch2.Shared;
using Microsoft.AspNetCore.Components;

namespace GitSearch2.Client.Pages {
	public partial class Index {

		public Index() {
			Commits = new List<CommitDetails>();
			FirstSearch = true;
		}

		public string SearchTerm { get; set; }

		public bool Busy { get; set; }

		public bool FirstSearch { get; set; }

		public string Message { get; set; }

		[Inject] protected IGitQueryService GitQueryService { get; set; }

		[Inject] protected NavigationManager UriHelper { get; set; }

		protected IEnumerable<CommitDetails> Commits { get; set; }

		protected async override Task OnParametersSetAsync() {
			string queryTerm = UriHelper.GetParameter( "q" );
			if( !string.IsNullOrWhiteSpace( queryTerm ) ) {
				SearchTerm = queryTerm;
				await PerformSearch( queryTerm );
			}
		}

		public async Task TriggerSearch( EventArgs _ ) {
			await PerformSearch( SearchTerm );
		}

		private async Task PerformSearch( string searchTerm ) {
			Busy = true;
			GitQueryResponse response = await GitQueryService.GitQuery( searchTerm, 0 );
			Commits = response.Commits;
			Message = response.Message;
			FirstSearch = false;
			Busy = false;
		}
	}
}
