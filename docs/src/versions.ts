// The published package versions, sourced from `docs/.env` so a release only edits one file.
//
// Astro inlines `PUBLIC_`-prefixed variables at build time, so pages can import these directly.
// Code samples can't interpolate inside a markdown fence — use `<Code>` from `astro:components`
// with a template literal instead.

function required(name: string, value: string | undefined): string {
	if (!value) throw new Error(`${name} is not set — add it to docs/.env`);
	return value;
}

/** Version of the `TechGems.ViteDotNet` NuGet package. */
export const nugetVersion = required(
	'PUBLIC_NUGET_VERSION',
	import.meta.env.PUBLIC_NUGET_VERSION
);

/** Version of the `vite-dotnet` npm package. */
export const npmVersion = required('PUBLIC_NPM_VERSION', import.meta.env.PUBLIC_NPM_VERSION);
