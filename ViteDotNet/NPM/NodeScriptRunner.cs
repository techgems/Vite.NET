using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ViteDotNet.NPM;

internal class NodeScriptRunner : IDisposable
{
    private Process? _npmProcess;
    public EventedStreamReader StdOut { get; }
    public EventedStreamReader StdErr { get; }

    private static readonly Regex AnsiColorRegex = new Regex("\x001b\\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1));


    public NodeScriptRunner(string workingDirectory, string scriptName, IDictionary<string, string>? envVars, string pkgManagerCommand, DiagnosticSource diagnosticSource, CancellationToken applicationStoppingToken)
    {
        if (string.IsNullOrEmpty(workingDirectory))
        {
            throw new ArgumentException("Cannot be null or empty.", nameof(workingDirectory));
        }

        if (string.IsNullOrEmpty(scriptName))
        {
            throw new ArgumentException("Cannot be null or empty.", nameof(scriptName));
        }

        if (string.IsNullOrEmpty(pkgManagerCommand))
        {
            throw new ArgumentException("Cannot be null or empty.", nameof(pkgManagerCommand));
        }

        var exeToRun = pkgManagerCommand;
        var completeArguments = $"run {scriptName}";
        if (OperatingSystem.IsWindows())
        {
            // On Windows, the node executable is a .cmd file, so it can't be executed
            // directly (except with UseShellExecute=true, but that's no good, because
            // it prevents capturing stdio). So we need to invoke it via "cmd /c".
            exeToRun = "cmd";
            completeArguments = $"/c {pkgManagerCommand} {completeArguments}";
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

        if (envVars != null)
        {
            foreach (var keyValuePair in envVars)
            {
                processStartInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        _npmProcess = LaunchNodeProcess(processStartInfo, pkgManagerCommand);
        StdOut = new EventedStreamReader(_npmProcess.StandardOutput);
        StdErr = new EventedStreamReader(_npmProcess.StandardError);

        applicationStoppingToken.Register(this.Dispose);

        if (diagnosticSource.IsEnabled("ViteDotNet.NPM.NpmStarted"))
        {
            diagnosticSource.Write(
                "ViteDotNet.NPM.NpmStarted",
                new
                {
                    processStartInfo = processStartInfo,
                    process = _npmProcess
                });
        }
    }

    private static string StripAnsiColors(string line) => AnsiColorRegex.Replace(line, string.Empty);

    public void AttachToLogger(ILogger logger, bool treatErrorsAsInfo = false)
    {
        // When the node task emits complete lines, pass them through to the real logger
        StdOut.OnReceivedLine += line =>
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                // Node tasks commonly emit ANSI colors, but it wouldn't make sense to forward
                // those to loggers (because a logger isn't necessarily any kind of terminal)
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

        // But when it emits incomplete lines, assume this is progress information and
        // hence just pass it through to StdOut regardless of logger config.
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

    private static Process LaunchNodeProcess(ProcessStartInfo startInfo, string commandName)
    {
        try
        {
            var process = Process.Start(startInfo)!;

            // See equivalent comment in OutOfProcessNodeInstance.cs for why
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
        if (_npmProcess != null && !_npmProcess.HasExited)
        {
            _npmProcess.Kill(entireProcessTree: true);
            _npmProcess.WaitForExit();
            _npmProcess = null;
        }
    }
}
