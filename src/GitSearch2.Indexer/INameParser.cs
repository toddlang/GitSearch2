using System;
using System.Collections.Generic;
using System.Text;

namespace GitSearch2.Indexer {
	internal interface INameParser {

		RepoProjectName Parse( string remoteUrl );
	}
}
