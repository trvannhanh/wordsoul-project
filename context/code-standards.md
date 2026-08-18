# Code Standards

## General

- Keep each module, service, component, route, and commit focused on one coherent responsibility.
- Fix root causes and preserve boundaries; do not layer UI/controller workarounds over domain or data defects.
- Validate untrusted input at boundaries and use explicit failure contracts.
- Prefer small, testable changes and preserve unrelated user work.
- Never commit credentials, tokens, private keys, production personal data, generated output, or dependency directories.

## C# and .NET

- Keep nullable reference types enabled; model nullability instead of suppressing it.
- Use PascalCase for public types/members, camelCase for locals/parameters, and `I`-prefixed interfaces.
- Keep controllers thin: parse/authorize, invoke an application service, and map established responses/errors.
- Define dependencies in Application and implement persistence/integrations in Infrastructure through DI.
- Use async I/O, propagate cancellation where supported, and avoid sync-over-async.
- Use the established transaction/unit-of-work boundary for multi-repository mutations.
- Use established exceptions/error codes and the global Problem Details pipeline.
- Make workers restart-safe and log structured identifiers/results without sensitive payloads.

## TypeScript and React

- Keep strict TypeScript enabled. Avoid `any`; narrow `unknown` and validate external responses.
- Reuse DTO/type and service modules; do not duplicate API shapes in pages or screens.
- Keep network/auth/token-refresh logic in services/helpers, not scattered through components.
- Follow hook lint rules; avoid unstable dependencies and unnecessary derived-state effects.
- Prefer feature-local code; put only genuine cross-feature reuse in shared components.
- Preserve each framework boundary: Vite/React Router, Next App Router, and React Navigation.

## Styling

- Reuse `ui-context.md` tokens and component language before adding colors, radii, shadows, or fonts.
- Learner web uses pixel-art utilities/Tailwind; admin uses CSS variables/Ant Design; mobile uses NativeWind.
- Do not copy web-only CSS/components into React Native.
- Provide focus, contrast, disabled, loading, error, and reduced-motion-safe behavior.

## API Routes

- Validate input before business logic; never trust client identity, role, ownership, totals, rewards, or timestamps.
- Enforce authentication, authorization, account state, ownership, and applicable rate limiting before mutation.
- Use centralized route constants and predictable DTO/Problem Details shapes.
- Define idempotency/concurrency outcomes; never report full success while a required part failed.
- Do not expose entity graphs, secrets, provider payloads, stack traces, or unredacted personal fields.

## Data and Storage

- EF entities/migrations represent durable relational state; repositories/application services own access patterns.
- Redis is cache/coordination with an explicit degradation policy, not accidental durable storage.
- Large media uses the configured asset service; store references and ownership metadata in SQL.
- Schema/config changes need validation, compatibility impact, recovery plan, and tests.
- Do not rewrite applied migrations; add a migration when planned schema evolution requires one.

## File Organization

- `WordSoulApi/WordSoul.Domain/` — entities, enums, domain rules.
- `WordSoulApi/WordSoul.Application/` — DTOs, use cases, interfaces, policies, errors.
- `WordSoulApi/WordSoul.Infrastructure/` — persistence, integrations, limiting, workers.
- `WordSoulApi/WordSoul.Api/` — controllers, middleware, hubs, routing, composition.
- `wordsoul-app/src/features/` — learner features; `shared/` only for cross-feature reuse.
- `wordsoul-admin/src/app/` and `components/` — admin routes and reusable admin UI.
- `wordsoul-mobile/src/` — mobile screens, navigation, services, types, components.
- `context/` — these six context files; `WordSoulApi/docs/` remains canonical for business decisions/evidence.

## Verification Commands

- API: `dotnet test WordSoulApi/WordSoulApi.sln`; compile-only: `dotnet build WordSoulApi/WordSoulApi.sln`.
- Learner web: in `wordsoul-app`, run relevant `npm run lint`, `npm run test`, and `npm run build`.
- Admin: in `wordsoul-admin`, run `npm run lint` and `npm run build`.
- Mobile: in `wordsoul-mobile`, run `npx tsc --noEmit` and the affected Expo journey for runtime/UI changes.
