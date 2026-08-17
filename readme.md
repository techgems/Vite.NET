# Vite.NET

Vite.NET is a library that allows you to easily implement Vite integrations in your ASP.NET Core Razor Pages and MVC applications.

This offers great advantages in the simplification of SPA authorization integration, allowing the implementation of micro front ends outside of a node ecosystem and in general just giving you more tools and tradeoffs when you're building applications.

The main idea is to bridge the gap between the backend and the front end and allow better integration between the two.

## Upgrading from v0

v1 has breaking changes, but the upgrade is short. The Vite plugin now emits manifest files describing each app, which removes nearly all of the backend configuration v0 required — the `ViteDotNet` setting is just your SPA's folder name, and there is nothing left to keep in sync between `vite.config.ts` and `appsettings.json`.

It comes down to a handful of edits: update both packages together, pass your entrypoint and container element id to the plugin, replace the old config object with the folder name, and rebuild. See [Migrating from v0](https://vite-dotnet.techgems.net/introduction/migrating-from-v0/) for the step-by-step guide.

## Links

- [Documentation](https://vite-dotnet.techgems.net)
- [Nuget Package](https://www.nuget.org/packages/TechGems.ViteDotNet)
- [NPM Package](https://www.npmjs.com/package/vite-dotnet)
