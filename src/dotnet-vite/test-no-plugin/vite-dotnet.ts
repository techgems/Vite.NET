import { writeFileSync, mkdirSync } from 'node:fs'
import { join, resolve } from 'node:path'

export type IntegrationMeta = {
  entrypoint: string
  containerElementId: string
  isReact: boolean
}

export function appFolder(): string {
  return process.cwd().split(/[\\/]/).pop()!
}

export function writeDevManifest(meta: IntegrationMeta): void {
  writeManifest('manifest.dev.json', { ...meta })
}

export function writeProdManifest(meta: IntegrationMeta): void {
  writeManifest('manifest.prod.json', meta)
}

function writeManifest(fileName: string, data: unknown): void {
  const dir = resolve(process.cwd(), '..', 'wwwroot', appFolder())
  mkdirSync(dir, { recursive: true })
  writeFileSync(join(dir, fileName), JSON.stringify(data, null, 2))
}
