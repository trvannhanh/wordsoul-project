---
name: vocamon-master
description: "Central Operating System (Central OS) for the Agent working on the Vocamon project. Provides architectural context, development rules, and operational guidelines for both Backend and Frontend."
category: project-management
risk: safe
source: local
date_added: "2026-05-08"
author: Antigravity
tags: [vocamon, central-os, architecture, dotnet, react, vite]
---

# Vocamon Master: Central Operating System

## 1. Project Overview
Vocamon is an intelligent vocabulary learning system, featuring a robust Backend ecosystem and a modern Frontend application. The project is designed following Clean Architecture principles and aims for a premium user experience.

## 2. System Architecture

### 🚀 Backend: `WordSoulApi`
The Backend is built on **.NET** following the **Clean Architecture** pattern.
- **WordSoul.Domain**: Contains Entities, Value Objects, Domain Services, and Interfaces. It has no dependencies on any other layers.
- **WordSoul.Application**: Contains Business Logic (Use Cases), DTOs, and Mapping profiles. Uses MediatR for the CQRS pattern.
- **WordSoul.Infrastructure**: Implements Interfaces from the Application layer (Database, External Services, Logging).
- **WordSoul.Api**: The system's entry point, containing Controllers, Middleware, and Dependency Injection configuration. **Rule: No business logic or direct DB access here.**
- **Admin Dashboard**: Located in `wordsoul-admin` (Next.js). Uses Ant Design 5. Uses Route Groups `(admin)` for shared layouts.
- **WordSoul.Tests / WordSoul.IntegrationTests**: Unit and integration testing systems.

### 🎨 Frontend: `wordsoul-app`
The Frontend is a Single Page Application (SPA) built with **React** and **Vite**.
- **Tech Stack**: React, TypeScript, Vite.
- **Styling**: Prioritize Vanilla CSS or TailwindCSS (if specifically requested).
- **State Management**: Uses modern patterns such as Context API, Redux, or Zustand depending on scale.

## 3. Operational Guidelines

### 🔍 When Starting a Task:
1. **Analyze Context**: Determine if changes are in the Backend, Frontend, or both.
2. **Check Mockups/Designs**: For UI tasks, use `generate_image` to visualize before coding.
3. **Adhere to Clean Architecture**: Do not write database logic in Controllers; do not let the Domain layer depend on Infrastructure.

### 🛠 Development Rules:
- **Backend**:
    - Use `Async/Await` for all I/O operations.
    - Validate data at the Application layer using FluentValidation.
    - Maintain comprehensive logging for significant events.
    - **Architecture Integrity**: `Application` layer MUST NOT reference `Infrastructure`. Use Interfaces for decoupling.
    - **Service Pattern**: Always implement logic in Services (`Application`) rather than using `UnitOfWork` or `DbContext` directly in Controllers.
    - **Entity Verification**: Always verify property names in `WordSoul.Domain.Entities` before using them in DTOs or Services (e.g., check if a property is `XpThreshold` or `RequiredLevel`).
    - **UoW Pattern**: Use `_unitOfWork.SaveChangesAsync()` to persist changes.
    - **JSON Object Cycles**: NEVER return Domain Entities directly in API responses. Always use **DTOs** to prevent circular reference errors (`JsonException: A possible object cycle was detected`).
    - **Auth Claims**: Support both standard JWT and Microsoft identity claim URIs for roles (e.g., `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`) to ensure cross-platform authentication compatibility.
- **Frontend**:
    - Follow Antigravity's **Design Aesthetics**: Vibrant colors, dark mode, glassmorphism, and micro-animations.
    - Break down components for maximum reusability.
    - Optimize Performance (Lazy loading, caching).
    - **API Instance**: Use `authApi` for any request requiring a Bearer token. Use `api` ONLY for public endpoints (Login/Register).
    - **Ant Design 5 Migration**:
        - `Card`: Replace `bordered={false}` with `variant="borderless"`. Use `styles={{ body: { ... } }}` instead of `bodyStyle`.
        - `Statistic`: Use `styles={{ content: { ... } }}` instead of `valueStyle`.
        - `Alert`: Use `title` instead of `message`.
        - `Modal`: Use `open` instead of `visible`. Use `destroyOnHidden` instead of `destroyOnClose`.
        - `Tabs`: Use the `items` prop instead of nested `TabPane`.
        - `Space`: Prefer `Flex` (AntD 5) for more predictable layout and alignment.
    - **AntD App Component**: Always use `App.useApp()` hook to access `message`, `notification`, and `modal` to ensure proper theme context.
    - **Form Connection**: Ensure `Form.useForm()` instances are connected to a visible `<Form>` element in the DOM to avoid "not connected" warnings. Avoid early returns that bypass the `<Form>` rendering while calling form methods.
    - **Next.js Layouts**: Use Route Groups like `(admin)` to wrap multiple pages with a shared `AdminLayout` (SideBar/Header).

## 4. Related Skills
- `@vocamon-master`: This Central OS (active).
- `@logic-lens`: Tailored for Vocamon **Clean Architecture** boundary checks.
- `@design-taste-frontend`: Tailored for Vocamon **Dark Mode & Glassmorphism** aesthetics.
- `@api-endpoint-builder`: Tailored for **.NET Core & MediatR** endpoint construction.
- `@frontend-api-integration-patterns`: Tailored for handling Vocamon's **ApplicationResult** API patterns.
- `@performance-optimizer`: Used for handling performance/caching issues (e.g., Redis).

## 5. Agent System Instructions
When working on this project, the Agent MUST:
1. Prioritize consistency between the Backend (API Contract) and Frontend (Service Integration).
2. Place new files in the correct directories according to the defined architecture.
3. Always check the `implementation_plan.md` file before executing major changes.
4. Communicate professionally and concisely in English.

---
*Last updated: 2026-05-13*
