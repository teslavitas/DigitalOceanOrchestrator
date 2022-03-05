using DigitalOceanOrchestrator;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
var configuration = builder.Build();

var settings = configuration.Get<Settings>();
var sshService = new SshService(settings.SshPrivateKeyFilePath, settings.SshCommands);
var dropletService = new DropletService(settings, sshService);

LogHelper.Log($"start with tag {settings.Tag}");
await dropletService.PrepareTags();

while (true)
{
    try
    {
        await dropletService.Run();
        await Task.Delay(TimeSpan.FromMinutes(1));
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}