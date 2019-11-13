using System;
using System.Collections.Generic;
using GitSearch2.Repository;
using GitSearch2.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GitSearch2.Server.Controllers {
	[Route( "api/[controller]" )]
	public class GitQueryController : Controller {

		private readonly ICommitRepository _commit;

		public GitQueryController(
			ICommitRepository commitRepository
		) {
			_commit = commitRepository ?? throw new ArgumentException( nameof( commitRepository ) );
		}


		[HttpPost( "[action]" )]
		[ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
		public ActionResult<IEnumerable<CommitDetails>> Search( [FromBody] GitQuery query ) {

			if (query == default) {
				return BadRequest();
			}

			IEnumerable<CommitDetails> result = _commit.Search( query.SearchTerm, query.MaximumRecords );

			if (result == default) {
				return StatusCode( 500 );
			}

			return Ok( result );
		}
	}
}
