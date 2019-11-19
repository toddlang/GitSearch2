using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LibGit2Sharp;

namespace GitSearch2.Indexer.Tests.Integration {
	internal sealed class LocalNameParser : INameParser {
		RepoProjectName INameParser.Parse( IRepository repository ) {
			DirectoryInfo gitFolder = new DirectoryInfo( repository.Info.Path );
			DirectoryInfo project = gitFolder.Parent;
			DirectoryInfo repo = project.Parent;

			return new RepoProjectName( repo.Name, project.Name );
		}
	}
}
