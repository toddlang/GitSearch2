using System;
using NUnit.Framework;

namespace GitSearch2.Repository.Tests.Unit {

	[TestFixture]
	public sealed class UpdateSessionTests {

		[Test]
		public void Equals_SameObject_ReturnsTrue() {
			UpdateSession session = new UpdateSession( Guid.NewGuid(), "repo", "project", DateTime.Now, DateTime.Now, 1 );

			Assert.IsTrue( session.Equals( session ) );
		}

		[Test]
		public void Equals_CompareToNull_ReturnsFalse() {
			UpdateSession session = new UpdateSession( Guid.NewGuid(), "repo", "project", DateTime.Now, DateTime.Now, 1 );

			Assert.IsFalse( session.Equals( default(UpdateSession) ) );
		}

		[Test]
		public void Equals_SameValues_ReturnsTrue() {
			DateTime timestamp = new DateTime( 2019, 12, 13, 10, 45, 30 );
			Guid sessionId = Guid.NewGuid();

			UpdateSession session1 = new UpdateSession( sessionId, "repo", "project", timestamp, timestamp, 1 );
			UpdateSession session2 = new UpdateSession( sessionId, "repo", "project", timestamp, timestamp, 1 );

			Assert.IsTrue( session1.Equals( session2 ) );
		}

		[Test]
		public void Equals_GuidDiffers_ReturnsFalse() {
			DateTime timestamp = DateTime.Now;
			UpdateSession session1 = new UpdateSession( Guid.NewGuid(), "repo", "project", timestamp, timestamp, 1 );
			UpdateSession session2 = new UpdateSession( Guid.NewGuid(), "repo", "project", timestamp, timestamp, 1 );

			Assert.IsFalse( session1.Equals( session2 ) );
		}

		[Test]
		public void Equals_RepoDiffers_ReturnsFalse() {
			DateTime timestamp = DateTime.Now;
			Guid sessionId = Guid.NewGuid();
			UpdateSession session1 = new UpdateSession( sessionId, "repo1", "project", timestamp, timestamp, 1 );
			UpdateSession session2 = new UpdateSession( sessionId, "repo2", "project", timestamp, timestamp, 1 );

			Assert.IsFalse( session1.Equals( session2 ) );
		}

		[Test]
		public void Equals_ProjectDiffers_ReturnsFalse() {
			DateTime timestamp = DateTime.Now;
			Guid sessionId = Guid.NewGuid();
			UpdateSession session1 = new UpdateSession( sessionId, "repo", "project1", timestamp, timestamp, 1 );
			UpdateSession session2 = new UpdateSession( sessionId, "repo", "project2", timestamp, timestamp, 1 );

			Assert.IsFalse( session1.Equals( session2 ) );
		}

		[Test]
		public void Equals_StartedDiffers_ReturnsFalse() {
			DateTime timestamp1 = new DateTime( 2019, 12, 13, 10, 45, 30 );
			DateTime timestamp2 = new DateTime( 2019, 12, 13, 10, 46, 30 );
			Guid sessionId = Guid.NewGuid();
			UpdateSession session1 = new UpdateSession( sessionId, "repo", "project", timestamp1, timestamp1, 1 );
			UpdateSession session2 = new UpdateSession( sessionId, "repo", "project", timestamp2, timestamp1, 1 );

			Assert.IsFalse( session1.Equals( session2 ) );
		}

		[Test]
		public void Equals_FinishedDiffers_ReturnsFalse() {
			DateTime timestamp1 = new DateTime( 2019, 12, 13, 10, 45, 30 );
			DateTime timestamp2 = new DateTime( 2019, 12, 13, 10, 46, 30 );
			Guid sessionId = Guid.NewGuid();
			UpdateSession session1 = new UpdateSession( sessionId, "repo", "project", timestamp1, timestamp1, 1 );
			UpdateSession session2 = new UpdateSession( sessionId, "repo", "project", timestamp1, timestamp2, 1 );

			Assert.IsFalse( session1.Equals( session2 ) );
		}

		[Test]
		public void Equals_CommitsWrittenDiffers_ReturnsFalse() {
			DateTime timestamp = new DateTime( 2019, 12, 13, 10, 45, 30 );
			Guid sessionId = Guid.NewGuid();
			UpdateSession session1 = new UpdateSession( sessionId, "repo", "project", timestamp, timestamp, 1 );
			UpdateSession session2 = new UpdateSession( sessionId, "repo", "project", timestamp, timestamp, 2 );

			Assert.IsFalse( session1.Equals( session2 ) );
		}
	}
}
