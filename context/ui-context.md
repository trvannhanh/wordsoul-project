# UI Context

## Theme

WordSoul has three related but intentionally different surfaces:

- **Learner web:** playful pixel-art/Pokémon-inspired visuals, light/dark support, game backgrounds, retro display fonts, and expressive motion.
- **Admin web:** compact operational workspace using Inter, neutral layered surfaces, indigo accents, and light/dark themes.
- **Mobile:** touch-first learner experience using NativeWind, blue primary actions, violet secondary accents, and orange highlights.

Do not force one surface's style onto another. Share semantic behavior and accessibility expectations, not framework-specific components.

## Colors

### Learner Web Tokens

| Role | CSS Variable | Light | Dark |
|---|---|---|---|
| Page background | `--background-color` | `#FFFFFF` | `rgb(2, 6, 23)` |
| Primary text | `--text-color` | `#1F2937` | `#F3F4F6` |
| Hover surface | `--hover-bg` | `#E5E7EB` | `#374151` |
| Border | `--border-color` | `rgb(2, 6, 23)` | `#374151` |
| Sidebar | `--sidebar-background-color` | `#E5E7EB` | `#101828` |
| Text outline | `--text-stroke-color` | `#FFFFFF` | `#000000` |

### Admin Tokens

| Role | CSS Variable | Light | Dark |
|---|---|---|---|
| Page background | `--bg-base` | `#F8FAFC` | `#0C0E14` |
| Surface | `--bg-surface` | `#FFFFFF` | `#141620` |
| Primary text | `--text-primary` | `#0F172A` | `#F1F5F9` |
| Muted text | `--text-muted` | `#94A3B8` | `#475569` |
| Primary accent | `--accent` | `#4F46E5` | `#6366F1` |
| Border | `--border` | `#E2E8F0` | `#1E2136` |
| Error | `--danger` | `#DC2626` | `#EF4444` |
| Success | `--success` | `#16A34A` | `#22C55E` |

### Mobile Theme Keys

| Role | Tailwind Key | Base Value |
|---|---|---|
| Primary | `primary-500` | `#3B82F6` |
| Secondary | `secondary-500` | `#8B5CF6` |
| Accent | `accent-500` | `#F97316` |
| Surface | `surface` | `#FFFFFF` |
| Dark surface | `surface-dark` | `#1E1E2E` |

## Typography

| Surface | Role | Font |
|---|---|---|
| Learner web | Pixel display | `PixelFont`, `Press Start 2P`, or `VT323` through existing utilities |
| Learner web | Body | `Noto Sans` or system sans |
| Admin | UI text | `Inter`, then system sans |
| Mobile | UI text | Platform `System` font unless an existing loaded asset is required |

Retro fonts are for short labels/headings, not long instructions, tables, forms, or errors.

## Border Radius

| Context | Convention |
|---|---|
| Learner game panels | Use established pixel borders; avoid soft SaaS cards inside game scenes |
| Admin small UI | `--radius-sm: 4px` |
| Admin cards/panels | `--radius-md: 8px` |
| Admin modals/overlays | `--radius-lg: 12px` |
| Mobile | Use the existing component/Tailwind scale consistently within a flow |

## Component Library

- Learner web uses Tailwind 4, project components, Lucide icons, and Framer Motion. Reuse feature/shared components before adding primitives.
- Admin uses Ant Design 6 with project CSS variables and theme context. Configure tokens/overrides centrally.
- Mobile uses React Native, NativeWind, Expo Vector Icons, and Reanimated. Reuse `src/components/ui/` and `src/components/layout/`.

## Layout Patterns

- Learner web: application navigation surrounds feature pages; learning/review and battle may use immersive full-screen scenes.
- Admin: fixed collapsible sidebar (`240px`/`64px`), sticky `56px` header, padded content, tables, and filter bars.
- Mobile: safe-area container, authentication stack, bottom tabs, and nested feature stacks with comfortable touch targets.
- Overlays expose primary action, cancel, loading, failure recovery, and duplicate-submission prevention.
- Essential actions must not depend only on hover or a desktop viewport.

## Icons and Motion

- Learner web: Lucide for conventional actions and pixel assets for game identity.
- Admin: Ant Design icons aligned with Ant Design controls.
- Mobile: Expo Vector Icons with labels for ambiguous actions.
- Motion communicates state/game feedback; respect reduced-motion preferences for nonessential particles, shake, flash, and loops.
