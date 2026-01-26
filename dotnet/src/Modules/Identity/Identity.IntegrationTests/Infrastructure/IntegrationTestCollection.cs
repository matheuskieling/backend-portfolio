using Xunit;

namespace Identity.IntegrationTests.Infrastructure;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<PortfolioWebApplicationFactory>
{
}
