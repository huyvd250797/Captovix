#!/usr/bin/env python3
"""Current-release integrity gate; no historical version or desktop QA assumptions."""
import argparse
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET
import zipfile

APP = 'HuyVo.Captovix'
SETUP = APP + '-win-Setup.exe'


def require(condition, message):
    if not condition:
        raise ValueError(message)


def digest(stream, algorithm='sha256'):
    h = hashlib.new(algorithm)
    for block in iter(lambda: stream.read(1024 * 1024), b''):
        h.update(block)
    return h.hexdigest().upper()


def file_digest(path, algorithm='sha256'):
    with path.open('rb') as stream:
        return digest(stream, algorithm)


def verify(version, release_dir, update_zip, setup_wrapper=None):
    package = f'{APP}-{version}-full.nupkg'
    required = {package, SETUP, 'RELEASES', 'assets.win.json', 'releases.win.json'}
    checks = []

    def check(ok, name):
        require(ok, name)
        checks.append(name)
        print('PASS:', name)

    check(all((release_dir / name).is_file() and (release_dir / name).stat().st_size > 0 for name in required), 'All five required release assets are present')
    full = release_dir / package
    feed = json.loads((release_dir / 'releases.win.json').read_text(encoding='utf-8-sig'))
    assets = [a for a in feed['Assets'] if a['PackageId'] == APP and a['Version'] == version and a['Type'] == 'Full']
    check(len(assets) == 1 and assets[0]['FileName'] == package, 'Current full package identity in Velopack feed')
    asset = assets[0]
    sha256, sha1 = file_digest(full), file_digest(full, 'sha1')
    check(asset['Size'] == full.stat().st_size and asset['SHA256'].upper() == sha256 and asset['SHA1'].upper() == sha1, 'Feed package size and SHA256/SHA1 match bytes')
    lines = [line.split() for line in (release_dir / 'RELEASES').read_text(encoding='utf-8-sig').splitlines() if line.strip()]
    check(any(len(line) == 3 and line[0].upper() == sha1 and line[1] == package and line[2] == str(full.stat().st_size) for line in lines), 'Legacy RELEASES contains current full package')
    manifest = json.loads((release_dir / 'assets.win.json').read_text(encoding='utf-8-sig'))
    check({package, SETUP}.issubset({a['RelativeFileName'] for a in manifest}), 'Assets manifest references current package and setup')
    with zipfile.ZipFile(full) as z:
        check(z.testzip() is None, 'Nupkg CRC integrity')
        nuspecs = [name for name in z.namelist() if name.endswith('.nuspec')]
        check(len(nuspecs) == 1, 'One NuGet manifest')
        fields = {e.tag.rsplit('}', 1)[-1]: e.text for e in ET.fromstring(z.read(nuspecs[0])).iter()}
        check(fields['id'] == APP and fields['version'] == version, 'Nupkg AppId and version match')
        names = z.namelist()
        runtime_files = ('Captovix.exe', 'Captovix.dll', 'Captovix.Recovery.exe', 'ffmpeg.exe', 'ffprobe.exe', 'Velopack.dll', 'update-source.json', 'release-quality.json')
        for name in runtime_files:
            matches = [n for n in names if n.startswith('lib/') and n.endswith('/' + name)]
            check(len(matches) == 1 and z.getinfo(matches[0]).file_size > 0, 'Package contains ' + name)
        stamp_name = next(n for n in names if n.startswith('lib/') and n.endswith('/release-quality.json'))
        stamp = json.loads(z.read(stamp_name))
        check(stamp['version'] == version, 'Release quality stamp matches version')
    with (release_dir / SETUP).open('rb') as setup:
        check(setup.read(2) == b'MZ', 'Setup is a Windows executable (not signature validation)')
    if setup_wrapper is not None:
        check(file_digest(setup_wrapper) == file_digest(release_dir / SETUP), 'Standalone Setup matches canonical update installer')
    expected = {p.name for p in release_dir.iterdir() if p.is_file() and (p.name in required or p.suffix == '.nupkg')}
    with zipfile.ZipFile(update_zip) as z:
        check(set(z.namelist()) == expected | {'README-PUBLISH.txt'} and len(z.namelist()) == len(set(z.namelist())), 'Update ZIP contains exact release assets and publishing instructions')
        check(z.testzip() is None, 'Update ZIP CRC integrity')
        for name in sorted(expected):
            with z.open(name) as stream:
                check(digest(stream) == file_digest(release_dir / name), 'Update ZIP matches release bytes: ' + name)
    return checks


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--version', required=True)
    p.add_argument('--release-dir', type=Path, required=True)
    p.add_argument('--update-zip', type=Path, required=True)
    p.add_argument('--setup-wrapper', type=Path)
    p.add_argument('--report', type=Path, required=True)
    a = p.parse_args()
    result = {'version': a.version, 'result': 'FAIL', 'desktopAcceptance': 'PENDING'}
    try:
        result['checks'] = verify(a.version, a.release_dir, a.update_zip, a.setup_wrapper)
        result['result'] = 'PASS'
    except Exception as error:
        result['error'] = str(error)
        raise
    finally:
        a.report.write_text(json.dumps(result, indent=2), encoding='utf-8')


if __name__ == '__main__':
    main()
