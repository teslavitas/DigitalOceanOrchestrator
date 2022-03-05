using Renci.SshNet;
using System.Diagnostics;

namespace DigitalOceanOrchestrator;

internal class SshService
{
    const int MaxCommandWaitMs = 600_000;
    private readonly string _sshFilePath;
    private readonly string[] _commands;
    public SshService(string sshFilePath, string[] commands)
    {
        _sshFilePath = sshFilePath;
        _commands = commands;
    }

    internal async Task RunCommands(string ipAddress)
    {
        var connectionInfo = new ConnectionInfo(ipAddress, "root",
            new PrivateKeyAuthenticationMethod("root", new PrivateKeyFile(_sshFilePath)));


        using var client = new SshClient(connectionInfo);
        client.Connect();
        using ShellStream shell = client.CreateShellStream("dumb", 80, 24, 800, 600, 1024);
        using var reader = new StreamReader(shell);
        using var writer = new StreamWriter(shell);
        writer.AutoFlush = true;

        for (int i = 0; i < _commands.Length; i++)
        {
            await RunCommand(_commands[i], i == _commands.Length - 1, reader, writer);
        }

        // closing shell causing an exception
        //shell.Close();
        //client.Disconnect();


    }

    private async Task RunCommand(string command, bool isLastCommand, StreamReader reader, StreamWriter writer)
    {
        writer.WriteLine(command);

        if (!isLastCommand)
        {
            // don't wait for the last command
            var timer = new Stopwatch();
            timer.Start();

            while (true)
            {
                await Task.Delay(1000);
                var output = reader.ReadToEnd();
                //Console.Write(output);
                if (output.TrimEnd().EndsWith("#"))
                {
                    break;
                }

                if (timer.ElapsedMilliseconds > MaxCommandWaitMs)
                {
                    throw new Exception($"Timeout for command {command}");
                }
            }
        }

    }
}

