using System;
using System.Collections.Generic;

namespace GitSearch2.Shared {
	public sealed record CommitDetails {

		public CommitDetails(
			IEnumerable<string> description,
			string repo,
			string authorEmail,
			string authorName,
			string date,
			IEnumerable<string> files,
			string commitId,
			string project,
			string pr,
			IEnumerable<string> commits,
			bool isMerge,
			string originId
		) {
			if( string.IsNullOrWhiteSpace( repo ) ) {
				throw new ArgumentException( "Repo name not specified", nameof( repo ) );
			}

			if( string.IsNullOrWhiteSpace( authorName ) ) {
				throw new ArgumentException( "Author name not specified", nameof( authorName ) );
			}

			if( string.IsNullOrWhiteSpace( date ) ) {
				throw new ArgumentException( "Date not specified", nameof( date ) );
			}

			if( string.IsNullOrWhiteSpace( commitId ) ) {
				throw new ArgumentException( "CommitId not specified.", nameof( commitId ) );
			}

			if( string.IsNullOrWhiteSpace( originId ) ) {
				throw new ArgumentException( "Origin not specified.", nameof( originId ) );
			}

			Description = description ?? throw new ArgumentException( "Description not specified.", nameof( description ) );
			Repo = repo;
			AuthorEmail = authorEmail;
			AuthorName = authorName;
			Date = date;
			Files = files ?? throw new ArgumentException( "Files not specified.", nameof( files ) );
			CommitId = commitId;
			Project = project;
			PR = pr;
			Commits = commits ?? throw new ArgumentException( "Commits not specified.", nameof( commits ) );
			IsMerge = isMerge;
			OriginId = originId;
		}

		public IEnumerable<string> Description { get; }

		public string Repo { get; }

		public string AuthorEmail { get; }

		public string AuthorName { get; }

		public string Date { get; }

		public IEnumerable<string> Files { get; }

		public string CommitId { get; }

		public string Project { get; }

		public string PR { get; }

		public IEnumerable<string> Commits { get; }

		public bool IsMerge { get; }

		public string OriginId { get; }
	}
}
