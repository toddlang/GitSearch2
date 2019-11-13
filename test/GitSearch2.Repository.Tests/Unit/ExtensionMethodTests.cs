using System;
using Microsoft.AspNetCore.Builder;
using Moq;
using NUnit.Framework;

namespace GitSearch2.Repository.Tests.Unit {
	[TestFixture]
	public class ExtensionMethodTests {

		[Test]
		public void UseRepositories_ValidBuilder_InitializeCalled() {
			var updateRepository = new Mock<IUpdateRepository>( MockBehavior.Strict );
			updateRepository
				.Setup( ur => ur.Initialize() );
			var commitRepository = new Mock<ICommitRepository>( MockBehavior.Strict );
			commitRepository
				.Setup( cr => cr.Initialize() );
			var services = new Mock<IServiceProvider>( MockBehavior.Strict );
			services
				.Setup( s => s.GetService( typeof( ICommitRepository ) ))
				.Returns( commitRepository.Object );
			services
				.Setup( s => s.GetService( typeof( IUpdateRepository ) ) )
				.Returns( updateRepository.Object );
			var builder = new Mock<IApplicationBuilder>( MockBehavior.Strict );
			builder
				.Setup( b => b.ApplicationServices )
				.Returns( services.Object );
			ExtensionMethods.UseRepositories( builder.Object );

			updateRepository.VerifyAll();
			commitRepository.VerifyAll();
		}
	}
}
