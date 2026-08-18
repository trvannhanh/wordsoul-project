# WordSoul Project Overview

## Overview

WordSoul (Vocamon) is a gamified English-vocabulary learning ecosystem for long-term retention and repeated practice. It combines guided learning and SM-2-style review with pets, quests, achievements, groups, notifications, pronunciation practice, PvE gym battles, and real-time PvP. The repository contains one API and three clients: learner web, administration web, and mobile.

This file is quick implementation context. Canonical business scope, decisions, tasks, release gates, and evidence remain under `WordSoulApi/docs/`.

## Goals

1. Provide a complete journey from account creation through vocabulary learning, review, progress, rewards, and battle.
2. Keep identity, progress, assets, and administrative changes consistent across API, web, admin, and mobile.
3. Make sensitive actions verifiable through authorization, audit, rate limiting, health signals, and evidence-based release gates.

## Core User Flow

1. A learner registers or signs in directly or through a supported identity provider.
2. The system grants only capabilities allowed by verification, consent, role, account state, and session policy.
3. The learner browses or creates vocabulary sets and starts a learning session.
4. The learner answers staged questions; the API records outcomes and updates progress and review scheduling.
5. Review activity produces idempotent progress, quest, achievement, pet, and inventory outcomes.
6. The learner may enter eligible PvE or PvP battle flows.
7. Notifications and dashboards expose relevant state without becoming sources of truth for domain data.

## Features

### Learning and Retention

- Vocabulary and vocabulary-set management.
- Multi-stage learning sessions, answer recording, spaced review, and progress history.
- Pronunciation practice and listening-oriented exercises where enabled.

### Gamification and Community

- Pets, items, inventory, daily quests, achievements, and rewards.
- Gym progression, arena battles, and SignalR-based PvP.
- User groups, leaderboards, and privacy-governed social presentation.

### Identity and Operations

- JWT authentication, refresh flows, Google OAuth, account/profile state, and device-aware notifications.
- Administrative dashboards, configuration, activity/system logs, health endpoints, and background workers.
- SQL Server, Redis, media storage, Firebase, SendGrid, Azure Speech, and optional AI/media integrations.

## Scope

### In Scope

- `WordSoulApi/`: domain, application, infrastructure, API, SignalR, workers, tests, and canonical docs.
- `wordsoul-app/`: learner-facing React web application.
- `wordsoul-admin/`: Next.js administration application.
- `wordsoul-mobile/`: Expo/React Native learner application.

### Out of Scope

- Replacing canonical business decisions with assumptions in these context files.
- Enabling capabilities held closed by an active CT, REL, gate, or deferred `-A` scope.
- Treating generated artifacts, package internals, or real credentials as project source.
- Claiming production readiness from documentation or partial implementation without passing the executor's selected verification.

## Success Criteria

1. A valid learner can complete registration/sign-in, learning, review, progress, and an eligible battle without duplicate state or unauthorized access.
2. Applicable API/client builds and tests pass, and failures use predictable contracts without leaking secrets or personal data.
3. Sensitive administration enforces role/scope, re-authentication, denial, audit, rollback, and reconciliation rules.
4. Phase A opens only after WSA-7K2 completes the applicable A-G01–A-G06 and REL self-checklists with no severe unresolved blocker.
