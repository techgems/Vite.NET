// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ViteDotNet.NPM;

/// <summary>
/// Launches the node command (the Vite dev server) from a minimal set of inputs: instead of being
/// handed a working directory and port, it derives the working directory from
/// <see cref="IViteConfigService"/> and <see cref="IWebHostEnvironment"/> resolved from the
/// application's service provider, and it needs no port at all — the Vite plugin picks its own port
/// and records it in <c>manifest.dev.json</c>.
/// </summary>
internal static class ViteDevServerRunner
{
    private const string LogCategoryName = "NodeServices";

    /// <summary>
    /// Resolves configuration from DI and starts the node command. Callers that don't want to await
    /// this (the common case) should launch it fire-and-forget from an application-lifetime callback;
    /// see <see cref="NodeCommandExtensions"/>.
    /// </summary>
    public static async Task Attach(IApplicationBuilder appBuilder, NodeCommandOptions options)
    {
        var services = appBuilder.ApplicationServices;

        var configService = services.GetRequiredService<IViteConfigService>();
        var environment = services.GetRequiredService<IWebHostEnvironment>();
        var applicationStoppingToken = services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
        var logger = LoggerFinder.GetOrCreateLogger(appBuilder, LogCategoryName);
        var diagnosticSource = services.GetRequiredService<DiagnosticSource>();

        var workingDir = ResolveWorkingDirectory(options, configService, environment);

        await ExecuteScript(workingDir, options, logger, diagnosticSource, applicationStoppingToken);
    }

    /// <summary>
    /// Uses the explicit override when provided; otherwise resolves the app directory from the
    /// <c>ViteDotNet</c> configuration and combines it with the content root.
    /// </summary>
    private static string ResolveWorkingDirectory(
        NodeCommandOptions options, IViteConfigService configService, IWebHostEnvironment environment)
    {
        if (!string.IsNullOrWhiteSpace(options.WorkingDirectory))
        {
            return options.WorkingDirectory!;
        }

        var appDirectory = configService.GetAppDirectory(options.AppName);
        return Path.Combine(environment.ContentRootPath, appDirectory);
    }

    private static async Task ExecuteScript(
        string workingDir, NodeCommandOptions options, ILogger logger,
        DiagnosticSource diagnosticSource, CancellationToken applicationStoppingToken)
    {
        var scriptRunner = new ViteProcessRunner(
            workingDir, options, diagnosticSource, applicationStoppingToken);
        scriptRunner.AttachToLogger(logger, true);

        using var stdErrReader = new EventedStreamStringReader(scriptRunner.StdErr);
        try
        {
            await scriptRunner.StdOut.WaitForMatch(
                new Regex(options.ReadyMatch, RegexOptions.None, options.ReadyMatchTimeout));
        }
        catch (EndOfStreamException ex)
        {
            throw new InvalidOperationException(
                $"The npm script '{options.ScriptName}' exited without indicating that the " +
                "Vite Dev Server had started. The error output was: " +
                $"{stdErrReader.ReadAsString()}", ex);
        }
    }
}
