-- Run as database owner, once, before starting the API. Dedicated private schema.
begin;
create schema if not exists captovix_license;
create table if not exists captovix_license.licenses (
 id text primary key, key_hash text unique not null, key_hint text not null,
 customer text not null, seats integer not null check(seats between 1 and 100),
 revoked boolean not null default false, created_at timestamptz not null default now(),
 recovered_at timestamptz, recovery_hash text not null
);
create table if not exists captovix_license.devices (
 id text primary key, license_id text references captovix_license.licenses(id),
 fingerprint text not null, public_hash text not null, name text not null,
 trial_until timestamptz, released boolean not null default false,
 created_at timestamptz not null default now(), seen_at timestamptz not null default now(),
 check((license_id is null)=(trial_until is not null))
);
create unique index if not exists one_trial_per_machine on captovix_license.devices(fingerprint) where license_id is null;
create unique index if not exists one_active_device_per_license on captovix_license.devices(license_id,fingerprint) where license_id is not null and not released;
create table if not exists captovix_license.nonces (nonce text primary key, expires_at timestamptz not null);
create table if not exists captovix_license.audit (
 id bigint generated always as identity primary key, created_at timestamptz not null default now(),
 action text not null, subject text not null, detail text not null
);
-- APIs use a server-only database role; never expose these tables via PostgREST.
revoke all on schema captovix_license from public;
revoke all on all tables in schema captovix_license from public;
alter table captovix_license.licenses enable row level security;
alter table captovix_license.devices enable row level security;
alter table captovix_license.nonces enable row level security;
alter table captovix_license.audit enable row level security;
commit;
