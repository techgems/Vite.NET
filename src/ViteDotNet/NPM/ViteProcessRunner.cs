// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ViteDotNet.NPM;

/// <summary>
/// Launches the node command as a child process, streams its output to the logger, and terminates
/// the process (and its whole tree) when the host stops.
/// </summary>
internal sealed class ViteProcessRunner : IDisposable
{
    private static readonly Regex AnsiColorRegex =
        new("\x001b\\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1));

    private Process? _process;

    public EventedStreamReader StdOut { get; }
    public EventedStreamReader StdErr { get; }

    public ViteProcessRunner(
        string workingDirectory, NodeCommandOptions options,
        DiagnosticSource diagnosticSource, CancellationToken applicationStoppingToken)
    {
        var exeToRun = options.PackageManager;
        var completeArguments = $"run {options.ScriptName}";
        if (OperatingSystem.IsWindows())
        {
            // On Windows the package manager is a .cmd file, so it has to be invoked via "cmd /c"
            // (UseShellExecute=true would work but would prevent capturing stdio).
            exeToRun = "cmd";
            completeArguments = $"/c {options.PackageManager} {completeArguments}";
        }

        var processStartInfo = new ProcessStartInfo(exeToRun)
        {
            Arguments = completeArguments,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory
        };

        foreach (var keyValuePair in options.EnvironmentVariables)
        {
            processStartInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
        }

        _process = LaunchProcess(processStartInfo, options.PackageManager);

        StdOut = new EventedStreamReader(_process.StandardOutput);
        StdErr = new EventedStreamReader(_process.StandardError);

        // Graceful-shutdown cleanup, and the primary termination mechanism on non-Windows platforms.
        applicationStoppingToken.Register(Dispose);

        if (diagnosticSource.IsEnabled("ViteDotNet.NPM.ViteProcessStarted"))
        {
            diagnosticSource.Write(
                "ViteDotNet.NPM.ViteProcessStarted",
                new { processStartInfo, process = _process });
        }
    }

    private static string StripAnsiColors(string line) => AnsiColorRegex.Replace(line, string.Empty);

    public void AttachToLogger(ILogger logger, bool treatErrorsAsInfo = false)
    {
        StdOut.OnReceivedLine += line =>
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                logger.LogInformation(StripAnsiColors(line));
            }
        };

        StdErr.OnReceivedLine += line =>
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                if (treatErrorsAsInfo)
                    logger.LogInformation(StripAnsiColors(line));
                else
                    logger.LogError(StripAnsiColors(line));
            }
        };

        // Incomplete lines are treated as progress information and passed straight through.
        StdErr.OnReceivedChunk += chunk =>
        {
            Debug.Assert(chunk.Array != null);

            var containsNewline = Array.IndexOf(
                chunk.Array, '\n', chunk.Offset, chunk.Count) >= 0;
            if (!containsNewline)
            {
                Console.Write(chunk.Array, chunk.Offset, chunk.Count);
            }
        };
    }

    private static Process LaunchProcess(ProcessStartInfo startInfo, string commandName)
    {
        try
        {
            var process = Process.Start(startInfo)!;
            process.EnableRaisingEvents = true;
            return process;
        }
        catch (Exception ex)
        {
            var message = $"Failed to start '{commandName}'. To resolve this:.\n\n"
                        + $"[1] Ensure that '{commandName}' is installed and can be found in one of the PATH directories.\n"
                        + $"    Current PATH enviroment variable is: {Environment.GetEnvironmentVariable("PATH")}\n"
                        + "    Make sure the executable is in one of those directories, or update your PATH.\n\n"
                        + "[2] See the InnerException for further details of the cause.";
            throw new InvalidOperationException(message, ex);
        }
    }

    public void Dispose()
    {
        if (_process != null && !_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            _process.WaitForExit();
            _process = null;
        }
    }
}
