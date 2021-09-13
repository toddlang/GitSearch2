using GitSearch2.Repository;
using LibGit2Sharp;

namespace GitSearch2.Indexer {
	internal sealed class LoopingExecutor : IExecutor {

		private readonly ICommitWalker _walker;
		private readonly IUpdateRepository _updateRepository;
		private readonly IGitRepoProvider _gitRepoProvider;
		private readonly INameParser _nameParser;

		public LoopingExecutor(
			ICommitWalker walker,
			IUpdateRepository updateRepository,
			IGitRepoProvider gitRepoProvider,
			INameParser nameParser
		) {
			_walker = walker;
			_updateRepository = updateRepository;
			_gitRepoProvider = gitRepoProvider;
			_nameParser = nameParser;
		}

		void IExecutor.Run() {
			IRepository repository = _gitRepoProvider.GetRepo();
			RepoProjectName name = _nameParser.Parse( repository );
			do {
				UpdateSession session = _updateRepository.GetScheduledUpdate( name.Repo, name.Project );
				if( _updateRepository.UpdateInProgress( name.Repo, name.Project ) ) {
					if( session is null ) {
						_updateRepository.ScheduleUpdate( Guid.NewGuid(), name.Repo, name.Project );
					}
					return;
				} else {
					if( session is null ) {
						session = _updateRepository.Begin( Guid.NewGuid(), name.Repo, name.Project, DateTime.Now );
					} else {
						_updateRepository.Resume( session.Session, DateTime.Now );
					}
				}


				int commitsWritten = _walker.Run();

				_updateRepository.End( session.Session, DateTime.Now, commitsWritten );

			} while( _updateRepository.GetScheduledUpdate( name.Repo, name.Project ) != null );
		}
	}
}
