// Packages the Claude Skill in `skill/vite-dotnet/` into `public/downloads/vite-dotnet-skill.zip`
// so the "Claude Skill" docs page can serve it as a download.
//
// The skill's markdown files are the source of truth and live in the repository; the zip is a
// build artifact (gitignored) rebuilt by the `predev` / `prebuild` npm scripts, so it can never
// drift from the files.
//
// The archive is written by hand with `zlib` rather than a packaging dependency: a zip is a
// handful of well-documented record structures, and this keeps the docs site dependency-free.
// Entries are stamped with a fixed timestamp so identical inputs produce an identical file.
import { deflateRawSync } from 'node:zlib';
import { readdirSync, readFileSync, mkdirSync, writeFileSync, statSync } from 'node:fs';
import { join, relative, dirname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const docsRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const skillRoot = join(docsRoot, 'skill');
const skillName = 'vite-dotnet';
const outputFile = join(docsRoot, 'public', 'downloads', `${skillName}-skill.zip`);

// DOS date/time for 1980-01-01 00:00 — the earliest the format can express. A fixed value keeps
// the output byte-for-byte reproducible across machines and rebuilds.
const DOS_TIME = 0;
const DOS_DATE = (1 << 5) | 1; // month 1, day 1, year 1980

const crcTable = (() => {
	const table = new Int32Array(256);
	for (let i = 0; i < 256; i++) {
		let c = i;
		for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
		table[i] = c;
	}
	return table;
})();

function crc32(buffer) {
	let crc = -1;
	for (let i = 0; i < buffer.length; i++) crc = crcTable[(crc ^ buffer[i]) & 0xff] ^ (crc >>> 8);
	return (crc ^ -1) >>> 0;
}

function filesUnder(dir) {
	return readdirSync(dir, { withFileTypes: true })
		.sort((a, b) => a.name.localeCompare(b.name))
		.flatMap((entry) => {
			const path = join(dir, entry.name);
			return entry.isDirectory() ? filesUnder(path) : [path];
		});
}

function localHeader(entry) {
	const header = Buffer.alloc(30);
	header.writeUInt32LE(0x04034b50, 0); // local file header signature
	header.writeUInt16LE(20, 4); // version needed to extract (2.0 — deflate)
	header.writeUInt16LE(0, 6); // general purpose flags
	header.writeUInt16LE(8, 8); // compression method: deflate
	header.writeUInt16LE(DOS_TIME, 10);
	header.writeUInt16LE(DOS_DATE, 12);
	header.writeUInt32LE(entry.crc, 14);
	header.writeUInt32LE(entry.compressed.length, 18);
	header.writeUInt32LE(entry.size, 22);
	header.writeUInt16LE(entry.name.length, 26);
	header.writeUInt16LE(0, 28); // extra field length
	return Buffer.concat([header, entry.name, entry.compressed]);
}

function centralHeader(entry) {
	const header = Buffer.alloc(46);
	header.writeUInt32LE(0x02014b50, 0); // central directory header signature
	header.writeUInt16LE(20, 4); // version made by
	header.writeUInt16LE(20, 6); // version needed to extract
	header.writeUInt16LE(0, 8); // general purpose flags
	header.writeUInt16LE(8, 10); // compression method: deflate
	header.writeUInt16LE(DOS_TIME, 12);
	header.writeUInt16LE(DOS_DATE, 14);
	header.writeUInt32LE(entry.crc, 16);
	header.writeUInt32LE(entry.compressed.length, 20);
	header.writeUInt32LE(entry.size, 24);
	header.writeUInt16LE(entry.name.length, 28);
	header.writeUInt16LE(0, 30); // extra field length
	header.writeUInt16LE(0, 32); // file comment length
	header.writeUInt16LE(0, 34); // disk number start
	header.writeUInt16LE(0, 36); // internal file attributes
	// External attributes: regular file, rw-r--r-- in the high word. Multiplied rather than
	// shifted, since `<< 16` would overflow into a negative 32-bit int.
	header.writeUInt32LE(0o100644 * 0x10000, 38);
	header.writeUInt32LE(entry.offset, 42);
	return Buffer.concat([header, entry.name]);
}

function endOfCentralDirectory(count, size, offset) {
	const record = Buffer.alloc(22);
	record.writeUInt32LE(0x06054b50, 0); // end of central directory signature
	record.writeUInt16LE(0, 4); // disk number
	record.writeUInt16LE(0, 6); // disk with the central directory
	record.writeUInt16LE(count, 8); // entries on this disk
	record.writeUInt16LE(count, 10); // total entries
	record.writeUInt32LE(size, 12);
	record.writeUInt32LE(offset, 16);
	record.writeUInt16LE(0, 20); // comment length
	return record;
}

function buildZip(files) {
	const entries = [];
	const chunks = [];
	let offset = 0;

	for (const file of files) {
		const contents = readFileSync(file);
		// Zip paths always use forward slashes, and stay relative to the skill folder so the
		// archive extracts as `vite-dotnet/SKILL.md`.
		const name = Buffer.from(relative(skillRoot, file).split(sep).join('/'), 'utf8');
		const entry = {
			name,
			crc: crc32(contents),
			size: contents.length,
			compressed: deflateRawSync(contents, { level: 9 }),
			offset,
		};
		const local = localHeader(entry);
		chunks.push(local);
		offset += local.length;
		entries.push(entry);
	}

	const central = entries.map(centralHeader);
	const centralSize = central.reduce((total, buffer) => total + buffer.length, 0);
	return Buffer.concat([
		...chunks,
		...central,
		endOfCentralDirectory(entries.length, centralSize, offset),
	]);
}

const skillDir = join(skillRoot, skillName);
if (!statSync(skillDir, { throwIfNoEntry: false })?.isDirectory()) {
	throw new Error(`Skill source not found at ${skillDir}`);
}

const files = filesUnder(skillDir);
mkdirSync(dirname(outputFile), { recursive: true });
writeFileSync(outputFile, buildZip(files));

console.log(
	`Packaged ${files.length} skill files into ${relative(docsRoot, outputFile).split(sep).join('/')}`
);
