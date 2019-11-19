using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace GitSearch2.Indexer {
	internal interface INameParser {

		RepoProjectName Parse( IRepository repository );
	}
}
