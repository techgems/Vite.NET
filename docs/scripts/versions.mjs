// Reads the package versions out of `docs/.env` for scripts that run outside Astro (which loads
// that file itself). Kept deliberately small: the file holds two unquoted values, not arbitrary
// shell syntax.
import { readFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const envFile = join(dirname(fileURLToPath(import.meta.url)), '..', '.env');

const env = Object.fromEntries(
	readFileSync(envFile, 'utf8')
		.split('\n')
		.map((line) => line.trim())
		.filter((line) => line && !line.startsWith('#'))
		.map((line) => {
			const separator = line.indexOf('=');
			return [line.slice(0, separator).trim(), line.slice(separator + 1).trim()];
		})
);

function required(name) {
	if (!env[name]) throw new Error(`${name} is not set — add it to docs/.env`);
	return env[name];
}

export const nugetVersion = required('PUBLIC_NUGET_VERSION');
export const npmVersion = required('PUBLIC_NPM_VERSION');
