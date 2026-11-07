using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

public class RepositorySafetyTests
{
    [Theory]
    [InlineData("src/HR_Mangment2.HttpApi.Host/appsettings.json")]
    [InlineData("src/HR_Mangment2.DbMigrator/appsettings.json")]
    public void Tracked_settings_hold_no_secrets(string relativePath)
    {
        var text = File.ReadAllText(Path.Combine(TestDatabase.FindRepositoryRoot(), relativePath));

        foreach (var marker in new[] { "Password", "PassPhrase", "ConnectionStrings" })
        {
            text.ShouldNotContain(marker);
        }
    }

    [Fact]
    public void Only_arabic_and_english_resources_exist()
    {
        var folder = Path.Combine(TestDatabase.FindRepositoryRoot(), "src", "HR_Mangment2.Domain.Shared", "Localization", "HR_Mangment2");

        Directory.GetFiles(folder, "*.json").Select(Path.GetFileName).OrderBy(x => x)
            .ShouldBe(new[] { "ar.json", "en.json" });
    }
}
