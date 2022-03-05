namespace DigitalOceanOrchestrator;

internal static class LogHelper
{
    public static void Log(string s)
    {
        Console.WriteLine($"{DateTime.Now:dd.MM HH:mm:ss} {s}");
    }
}
