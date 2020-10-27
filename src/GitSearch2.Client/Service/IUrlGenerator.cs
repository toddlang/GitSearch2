using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitSearch2.Shared;

namespace GitSearch2.Client.Service {
	public interface IUrlGenerator {

		string CommitUrl( CommitDetails details );

		string PrUrl( string project, string repo, CommitDetails details );

		string MergeUrl( CommitDetails details, string commitId );

		string FileUrl( CommitDetails details, string fileName );
	}
}
