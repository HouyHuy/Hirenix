using Testcontainers.MySql;

namespace Hirenix.SystemTests;

public sealed class TestcontainersReadinessTests
{
    [Fact(Skip = "Readiness sample for Docker-backed MySQL suites. Enable when Docker Desktop is available and isolated DB tests are needed.")]
    public async Task MySql_testcontainer_can_start_for_isolated_business_suites()
    {
        await using var container = new MySqlBuilder()
            .WithDatabase("hirenix_test")
            .WithUsername("hirenix")
            .WithPassword("hirenix")
            .Build();

        await container.StartAsync();
        Assert.False(string.IsNullOrWhiteSpace(container.GetConnectionString()));
    }
}
