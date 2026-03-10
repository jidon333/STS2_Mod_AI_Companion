# ARCHITECTURE

## STS2 native mod

Small in-process shell responsible for loader compatibility, stable state capture points, config loading, and bridge lifecycle hooks.

## external companion host

Out-of-process application that owns AI orchestration, prompt construction, model calls, policy checks, and higher-latency workflows.

## communication bridge

Explicit boundary for serialized state snapshots, event notifications, health checks, and future request-response flows between the native shell and the host.

## data sources

Game-exposed state, local snapshot artifacts, runtime config, and future bridge payloads. No direct multiplayer manipulation or hidden memory scraping is in scope for the initial scaffold.

## safety / rollback boundaries

The native mod must stay removable, bridge failures must degrade safely, and snapshot/restore tooling remains the fallback when experiments affect local modded data.
