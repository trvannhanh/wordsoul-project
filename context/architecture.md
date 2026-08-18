# Architecture Context

## Stack

| Layer | Technology | Role |
|---|---|---|
| API | ASP.NET Core Web API on .NET 9 | REST, middleware, OpenAPI/Scalar, health endpoints |
| Domain | C# entities, enums, domain services | Core business concepts and rules |
| Application | C# services, DTOs, interfaces, policies | Use cases and contracts |
| Persistence | EF Core 9 + SQL Server | Durable domain state and relationships |
| Cache/coordination | Redis + in-memory cache | Cache, rate limiting, transient coordination |
| Realtime | ASP.NET Core SignalR | Notifications, matchmaking, PvP |
| Learner web | React 19, Vite 7, TypeScript 5.8, Tailwind 4 | Browser learner experience |
| Admin web | Next.js 16, React 19, Ant Design 6, Zustand | Administration and operations |
| Mobile | Expo 56, React Native 0.85, TypeScript 6, NativeWind | Mobile learner experience |
| External services | Cloudinary/Azure Blob, Firebase, SendGrid, Azure Speech, Google OAuth, Gemini, Unsplash | Media, messaging, identity, optional capabilities |
| Tests | xUnit, FluentAssertions, Moq, SQLite/InMemory EF, Vitest | Unit, integration, frontend verification |

## System Boundaries

- `WordSoulApi/WordSoul.Domain/` — entities, enums, and domain services; no delivery or persistence ownership.
- `WordSoulApi/WordSoul.Application/` — use cases, DTOs, policies, interfaces, and application exceptions.
- `WordSoulApi/WordSoul.Infrastructure/` — EF persistence, repositories, integrations, distributed limiting, workers.
- `WordSoulApi/WordSoul.Api/` — composition root, controllers, middleware, routing, hubs, runtime configuration.
- `WordSoulApi/WordSoul.Tests/` and `WordSoulApi/WordSoul.IntegrationTests/` — automated verification only.
- `wordsoul-app/src/` — learner-web features, pages, shared UI, state, services, i18n, config.
- `wordsoul-admin/src/` — admin routes, components, contexts, services, theme.
- `wordsoul-mobile/src/` — screens, navigation, contexts, services, types, reusable components.
- `WordSoulApi/docs/` — canonical product decisions, Phase A execution, gates, and evidence.
- `context/` — concise developer/AI orientation, synchronized when repository-level context changes.

## Storage Model

- **SQL Server through EF Core**: users, vocabulary, sets, sessions, answers, progress, reviews, pets, inventory, quests, achievements, groups, battles, notifications, configurations, and relational logs.
- **Redis**: distributed cache, rate-limiter state, and transient coordination; never the only durable domain source.
- **Cloudinary/Azure Blob**: media and large assets; the database stores references, ownership, and status metadata.
- **Secure client storage**: mobile auth material uses Expo SecureStore; clients use established auth helpers rather than ad hoc persistence.
- **Logs/evidence**: structured metadata only, never raw credentials, tokens, secret values, personal payloads, or raw media.

## Auth and Access Model

- The API validates JWT issuer, audience, lifetime, and signing key; SignalR uses the established hub token path.
- Direct auth and Google OAuth converge on M01 account-state/linking rules; matching email alone never links accounts.
- Controllers and hubs enforce authentication plus role/scope authorization before protected reads or mutations.
- Sensitive operations require documented scope, reason/case, re-authentication when applicable, and an audit result.
- Refresh, logout, restriction, role change, reset, and deletion honor session revocation across clients.

## Invariants

1. Dependencies point inward: Domain → Application contracts/use cases → Infrastructure implementations → API composition/delivery.
2. Controllers, pages, and screens do not become alternate sources of business truth.
3. Retry, concurrency, worker restart, or provider timeout must not duplicate accounts, rewards, ledger changes, progress, or battle outcomes.
4. Account state, authorization, ownership, consent, and CT/REL gates are enforced server-side; client visibility is not authorization.
5. Scheduled/long work belongs in restart-safe workers with idempotency, checkpoint/compensation, health, and reconciliation.
6. Provider failure follows an explicit fail-open/fail-closed contract and cannot silently lower protections.
7. Secrets and raw sensitive payloads never appear in source, logs, screenshots, docs, or evidence.
