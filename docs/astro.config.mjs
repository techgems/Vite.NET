// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import starlightLinksValidator from 'starlight-links-validator';

// https://astro.build/config
export default defineConfig({
	site: 'https://vite-dotnet.techgems.net',
	integrations: [
		starlight({
			// The skill archive is a build artifact in `public/`, not a page, so the link
			// validator can't resolve it.
			plugins: [starlightLinksValidator({ exclude: ['/downloads/**'] })],
			title: 'Vite.NET',
			description:
				'Integrate Vite SPAs with ASP.NET Core Razor Pages and MVC — a single build tool, shared authentication, and micro-frontends without leaving .NET.',
			favicon: './favicon.svg',
			logo: {
				// Placeholder wordmark — swap src/assets/logo.svg for the final Vite.NET logo.
				// `replacesTitle` hides the text title since the logo already includes it.
				src: './src/assets/logo.svg',
				replacesTitle: true,
			},
			social: [
				{ icon: 'github', label: 'GitHub', href: 'https://github.com/techgems/Vite.NET' },
			],
			components: {
				// Force the light theme and remove the theme switcher.
				ThemeProvider: './src/components/DefaultLight.astro',
				ThemeSelect: './src/components/DisableThemeColor.astro',
				// Add a header link out to the previous (v0) documentation site.
				SocialIcons: './src/components/HeaderLinks.astro',
			},
			sidebar: [
				{
					label: 'Introduction',
					items: [
						{ label: 'Overview', slug: 'introduction/overview' },
						{ label: 'Installation', slug: 'introduction/installation' },
						{ label: 'Quick Start', slug: 'introduction/quick-start' },
					],
				},
				{
					label: 'Guides',
					items: [
						{ label: 'How It Works', slug: 'guides/how-it-works' },
						{ label: 'The Vite Plugin', slug: 'guides/the-vite-plugin' },
						{ label: 'Using the Plugin Without npm', slug: 'guides/plugin-without-npm' },
						{ label: 'Backend Configuration', slug: 'guides/backend-configuration' },
						{ label: 'Rendering a SPA', slug: 'guides/rendering-a-spa' },
						{ label: 'Development Workflow', slug: 'guides/development-workflow' },
						{ label: 'Production Builds', slug: 'guides/production-builds' },
						{ label: 'Multiple SPAs', slug: 'guides/multiple-spas' },
					],
				},
				{
					label: 'Reference',
					items: [
						{ label: 'ViteDotNetPlugin', slug: 'reference/vite-plugin' },
						{ label: 'AddViteIntegration', slug: 'reference/add-vite-integration' },
						{ label: 'dev-vite-scripts', slug: 'reference/dev-vite-scripts' },
						{ label: 'prod-vite-scripts', slug: 'reference/prod-vite-scripts' },
						{ label: 'Manifest Files', slug: 'reference/manifest-files' },
					],
				},
				{
					label: 'Miscellaneous',
					items: [
						{ label: 'Migrating from v0', slug: 'miscellaneous/migrating-from-v0' },
						{ label: 'Claude Skill', slug: 'miscellaneous/claude-skill' },
					],
				},
			],
		}),
	],
});
