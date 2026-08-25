# Combat Refactor Plan

## Objective
Refactor the current local combat implementation into a generic, event-driven foundation before networking is introduced.

## Phase A - Core contracts
- IDamageable
- DamageInfo
- DamageResult
- DamageType

## Phase B - Health migration
- Update Health to consume DamageInfo
- Preserve legacy TakeDamage(float) temporarily as an adapter
- Emit structured damage and death events

## Phase C - Damage producers
- Bullet builds DamageInfo
- Explosion builds DamageInfo
- Future abilities use the same pipeline

## Phase D - Team and ownership
- Add TeamId
- Add CombatIdentity / owner abstraction
- Prevent friendly-fire damage through a central rule layer

## Phase E - Networking preparation
No NetworkBehaviour is introduced in this branch. The goal is to make combat deterministic enough that a future server-authoritative layer can validate inputs and replicate results.

## Safety rule
Existing scenes should continue compiling while migration happens. Legacy public methods should only be removed after all consumers are migrated.
