#!/usr/bin/env python3
"""Synthetic fixtures for the integrity verifier. Never writes installable artifacts."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
import zipfile

spec = importlib.util.spec_from_file_location('verify_ci', Path(__file__).with_name('Verify-CI-Packages.py'))
verify_ci = importlib.util.module_from_spec(spec)
spec.loader.exec_module(verify_ci)


class PackageIntegrityTests(unittest.TestCase):
    version = '1.68.3'

    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self.tmp.cleanup)
        self.root = Path(self.tmp.name)
        self.rel = self.root / 'releases'
        self.rel.mkdir()
        self.full = self.rel / f'{verify_ci.APP}-{self.version}-full.nupkg'
        self.setup = self.rel / verify_ci.SETUP
        self.update = self.root / 'fixture.zip'
        with zipfile.ZipFile(self.full, 'w') as z:
            z.writestr('test.nuspec', f'<package><metadata><id>{verify_ci.APP}</id><version>{self.version}</version></metadata></package>')
            for name in ('Captovix.exe','Captovix.dll','Captovix.Recovery.exe','ffmpeg.exe','ffprobe.exe','Velopack.dll','update-source.json'):
                z.writestr('lib/test/' + name, 'synthetic test fixture only')
            z.writestr('lib/test/release-quality.json', json.dumps({'version': self.version}))
        self.setup.write_bytes(b'MZ synthetic test fixture, not an executable')
        self.feed = self.rel / 'releases.win.json'
        self.asset = dict(PackageId=verify_ci.APP, Version=self.version, Type='Full', FileName=self.full.name, Size=self.full.stat().st_size, SHA256=verify_ci.file_digest(self.full), SHA1=verify_ci.file_digest(self.full,'sha1'))
        self.write_feed()
        (self.rel/'RELEASES').write_text(f"{self.asset['SHA1']} {self.full.name} {self.asset['Size']}\n")
        (self.rel/'assets.win.json').write_text(json.dumps([{'RelativeFileName':self.full.name},{'RelativeFileName':self.setup.name}]))
        self.pack()

    def write_feed(self):
        self.feed.write_text(json.dumps({'Assets':[self.asset]}))

    def pack(self, omit=None):
        with zipfile.ZipFile(self.update, 'w') as z:
            for p in self.rel.iterdir():
                if p.name != omit:
                    z.write(p,p.name)
            z.writestr('README-PUBLISH.txt','test fixture')

    def verify(self):
        return verify_ci.verify(self.version,self.rel,self.update)

    def test_valid(self):
        self.assertGreaterEqual(len(self.verify()),20)

    def test_bad_hash(self):
        self.asset['SHA256']='0'*64
        self.write_feed(); self.pack()
        with self.assertRaisesRegex(ValueError,'SHA256'): self.verify()

    def test_missing_asset(self):
        self.setup.unlink()
        with self.assertRaisesRegex(ValueError,'five required'): self.verify()

    def test_wrong_version(self):
        self.asset['Version']='1.68.2'
        self.write_feed(); self.pack()
        with self.assertRaisesRegex(ValueError,'identity'): self.verify()

    def test_zip_missing_entry(self):
        self.pack(omit='RELEASES')
        with self.assertRaisesRegex(ValueError,'exact release assets'): self.verify()

    def test_zip_stale_bytes(self):
        self.setup.write_bytes(b'MZ changed after packaging')
        with self.assertRaisesRegex(ValueError,'matches release bytes'): self.verify()

    def test_wrong_setup_wrapper(self):
        bad=self.root/'wrapper.exe';bad.write_bytes(b'MZ mismatching fixture')
        with self.assertRaisesRegex(ValueError,'Standalone Setup'): verify_ci.verify(self.version,self.rel,self.update,bad)

if __name__ == '__main__':
    unittest.main()
