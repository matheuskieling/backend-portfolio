using Xunit;

namespace DocumentManager.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<DocumentManagerWebApplicationFactory>
{
}
