using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace A12.Infrastructure.Configuration;

public static class WebConfigConfigurationExtensions
{
    public const string FileName = "web.config";

    public static IConfigurationBuilder AddA12WebConfig(this IConfigurationBuilder configuration, string contentRoot)
    {
        var path = FindWebConfig(contentRoot)
            ?? throw new FileNotFoundException(
                "Backend web.config was not found. Keep the connection string in A12-Back/web.config.");

        configuration.Add(new WebConfigConfigurationSource(path));
        return configuration;
    }

    private static string? FindWebConfig(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, FileName);
            if (HasConnectionStrings(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        var besideApplication = Path.Combine(AppContext.BaseDirectory, "a12.web.config");
        return HasConnectionStrings(besideApplication) ? besideApplication : null;
    }

    private static bool HasConnectionStrings(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        var document = XDocument.Load(path);
        return document.Descendants("connectionStrings").Elements("add").Any();
    }
}

internal sealed class WebConfigConfigurationSource(string path) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new WebConfigConfigurationProvider(path);
}

internal sealed class WebConfigConfigurationProvider(string path) : ConfigurationProvider
{
    public override void Load()
    {
        var document = XDocument.Load(path);
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var element in document.Descendants("connectionStrings").Elements("add"))
        {
            var name = (string?)element.Attribute("name");
            var value = (string?)element.Attribute("connectionString");
            if (!string.IsNullOrWhiteSpace(name) && value is not null)
            {
                data[$"ConnectionStrings:{name}"] = value;
            }
        }

        foreach (var element in document.Descendants("appSettings").Elements("add"))
        {
            var key = (string?)element.Attribute("key");
            var value = (string?)element.Attribute("value");
            if (!string.IsNullOrWhiteSpace(key))
            {
                data[key] = value;
            }
        }

        Data = data;
    }
}
