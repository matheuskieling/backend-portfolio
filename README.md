# Backend Portfolio

A backend portfolio project demonstrating architecture, domain modeling, and best practices across multiple technology stacks.

## Purpose

This project goes beyond basic CRUD to showcase:

- Clean Architecture principles
- Modular monolith design with clear boundaries
- Real-world concerns: authentication, authorization, validation, business rules
- Consistency across different technology stacks

## Implementations

| Stack | Status | Live Demo |
|-------|--------|-----------|
| .NET 8 | In progress | [API Docs](https://portfolio-dotnet-api.fly.dev/) |
| Java | Planned | - |

Each implementation follows identical business rules and architectural decisions, allowing direct comparison between stacks.

## Architecture

- **Modular Monolith** - Explicit module boundaries without microservices complexity
- **Clean Architecture** - Domain and application layers isolated from infrastructure
- **DDD-inspired** - Lightweight, pragmatic domain modeling

## Modules

- **Identity** - Authentication, JWT, RBAC with roles and permissions
- **Catalog** - Products and categories management
- **Orders** - Order lifecycle with business rule enforcement

## Philosophy

- Clarity over cleverness
- Explicitness over magic
- Maintainability over speed
