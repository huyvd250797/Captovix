Captovix V1.29.1 — Serverless Licensing

The Update-Packages ZIP contains the canonical Setup.exe and update assets.
Before publishing, configure Supabase, keep the signing private key only in Edge
Secrets, embed the PUBLIC app config, build and test on real Windows machines.
The delivered review build has an empty production URL/public key and only
allows manual region capture. It is not a pre-activated customer release.

See docs/SERVERLESS-LICENSING.md in Source ZIP for Dashboard/Cloudflare setup.
Upload the FIVE release assets after configuration and acceptance:
HuyVo.Captovix-1.29.1-full.nupkg
HuyVo.Captovix-win-Setup.exe
RELEASES
assets.win.json
releases.win.json

Source ZIP is for development. Never upload private keys, edge-secrets.env,
Supabase service_role/secret keys, admin passwords or test fixtures to Releases.
