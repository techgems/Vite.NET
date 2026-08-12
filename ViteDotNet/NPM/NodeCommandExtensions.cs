// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ViteDotNet.NPM;

/// <summary>
/// Launches the Vite dev server (or another node command) from ASP.NET Core on startup.
/// <para>
/// Two design points are worth calling out:
/// </para>
/// <list type="number">
/// <item>
/// It returns <see cref="IApplicationBuilder"/> instead of a <see cref="Task"/>, so calling it never
/// produces the "because this call is not awaited..." (CS4014) warning. The actual work is launched
/// fire-and-forget internally.
/// </item>
/// <item>
/// Rather than running as the host is being built, it hooks
/// <see cref="IHostApplicationLifetime.ApplicationStarted"/> and only launches the node command once
/// the server is fully up and listening — the same callback approach used by ASP.NET Core's own SPA
/// integrations.
/// </item>
/// </list>
/// </summary>
public static class NodeCommandExtensions
{
    /// <summary>
    /// Launches the Vite dev server for the single configured app once the server has started.
    /// </summary>
    public static IApplicationBuilder RunViteDevServer(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.RunViteDevServer(_ => { });

    /// <summary>
    /// Launches the Vite dev server for the named app once the server has started. Use this overload
    /// when more than one Vite app is configured.
    /// </summary>
    /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
    /// <param name="appName">The directory name of the app whose dev server should be launched.</param>
    public static IApplicationBuilder RunViteDevServer(this IApplicationBuilder applicationBuilder, string appName)
        => applicationBuilder.RunViteDevServer(options => options.AppName = appName);

    /// <summary>
    /// Launches a node command once the server has started, configured via <paramref name="configure"/>.
    /// </summary>
    /// <param name="applicationBuilder">The <see cref="IApplicationBuilder"/>.</param>
    /// <param name="configure">Configures the command that runs. Most defaults are derived from the
    /// existing <c>ViteDotNet</c> configuration, so this is often left empty.</param>
    public static IApplicationBuilder RunViteDevServer(
        this IApplicationBuilder applicationBuilder, Action<NodeCommandOptions> configure)
    {
        if (applicationBuilder == null)
        {
            throw new ArgumentNullException(nameof(applicationBuilder));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var options = new NodeCommandOptions();
        configure(options);

        var lifetime = applicationBuilder.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        // Only start the node command once the server is fully up and listening. The discard makes
        // the fire-and-forget explicit and stops the compiler warning about the unawaited Task.
        lifetime.ApplicationStarted.Register(() => _ = RunAsync(applicationBuilder, options));

        return applicationBuilder;
    }

    /// <summary>
    /// Runs the command and swallows/logs any failure so an unobserved background task never brings
    /// the host down.
    /// </summary>
    private static async Task RunAsync(IApplicationBuilder applicationBuilder, NodeCommandOptions options)
    {
        try
        {
            await ViteDevServerRunner.Attach(applicationBuilder, options);
        }
        catch (Exception ex)
        {
            var logger = LoggerFinder.GetOrCreateLogger(applicationBuilder, "NodeServices");
            logger.LogError(ex, "Failed to launch the Vite dev server via RunViteDevServer.");
        }
    }
}
