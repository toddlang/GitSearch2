using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitSearch2.Repository;
using GitSearch2.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GitSearch2.Server.Controllers {
	[Route( "api/[controller]" )]
	public class GitQueryController : Controller {

		private readonly ILogger<GitQueryController> _logger;
		private readonly ICommitRepository _commit;

		public GitQueryController(
			ILogger<GitQueryController> logger,
			ICommitRepository commitRepository
		) {
			_logger = logger;
			_commit = commitRepository ?? throw new ArgumentException( "Commit repository not specified.", nameof( commitRepository ) );
		}


		[HttpPost( "[action]" )]
		[ResponseCache( Duration = 0, Location = ResponseCacheLocation.None, NoStore = true )]
		public async Task<ActionResult<GitQueryResponse>> SearchAsync( [FromBody] GitQuery query ) {

			if( query == default ) {
				return BadRequest();
			}

			try {
				IEnumerable<CommitDetails> result = await _commit.SearchAsync( query.SearchTerm, query.MaximumRecords );

				if( result == default ) {
					return new StatusCodeResult( 500 );
				}

				return Ok( new GitQueryResponse( result, "" ) );

			} catch( Exception ex ) {

				_logger.LogError( ex, "Error performing query." );

				return Ok( new GitQueryResponse( Enumerable.Empty<CommitDetails>(), ex.Message ) );
			}

		}
	}
}
