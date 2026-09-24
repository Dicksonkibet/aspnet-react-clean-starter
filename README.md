# CleanStart

A clean-architecture starter for new projects: ASP.NET Core (Clean Architecture) backend
+ React/TypeScript (Vite) frontend, extracted and generalized from the CampusConnect
codebase. It's opinionated but minimal — pick and delete what you don't need.

## What's included

**Backend** (`/backend`) — 4-project Clean Architecture solution:

- `CleanStart.Domain` — plain entities, zero dependencies. `BaseEntity` gives every
  entity an Id, audit timestamps and soft-delete for free.
- `CleanStart.Application` — CQRS-lite with MediatR: one file per feature holds its
  DTOs, commands/queries, FluentValidation validators and handlers side by side
  (see `TodoItems/TodoItemFeature.cs` for the pattern to copy). A generic
  `IRepository<T>`/`IUnitOfWork` means new entities get full CRUD with no extra
  per-entity repository code, and the specification pattern (`BaseSpecification<T>`)
  covers filtered/paged/sorted queries without leaking EF Core up from Infrastructure.
- `CleanStart.Infrastructure` — EF Core + PostgreSQL, ASP.NET Identity, and the
  concrete `EfRepository<T>` / `UnitOfWork` that implement the Application interfaces.
- `CleanStart.API` — thin controllers (one line each: send to MediatR, return result),
  JWT auth (register/login), global exception-handling middleware that turns any
  unhandled exception into a consistent JSON error, CORS locked to an allow-list in
  non-Development environments, and a `/healthz` endpoint that checks real DB
  connectivity.

**Frontend** (`/frontend`) — Vite + React 19 + TypeScript + Tailwind v4:

- `lib/api/http.ts` — a small `apiFetch` wrapper (attaches the JWT, normalizes error
  bodies, times out).
- `store/authStore.ts` — zustand store for the logged-in user + token, persisted to
  `localStorage`.
- `routes/ProtectedRoute.tsx` — wrap any route that needs a logged-in user.
- Sample pages: Home, Login, Register, and a Dashboard that exercises the sample
  `TodoItems` API end to end (list/add/toggle/delete) so you can see the whole slice
  working before you write your own features.

## Getting started

### Backend

```bash
cd backend
dotnet restore

# set a real JWT signing key (don't commit one to appsettings.json)
dotnet user-secrets init --project CleanStart.API
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)" --project CleanStart.API

# point ConnectionStrings:DefaultConnection at your own Postgres instance,
# then create the initial migration and apply it
dotnet ef migrations add InitialCreate --project CleanStart.Infrastructure --startup-project CleanStart.API
dotnet ef database update --project CleanStart.Infrastructure --startup-project CleanStart.API

dotnet run --project CleanStart.API
```

Swagger UI opens automatically in Development at `/swagger`.

### Frontend

```bash
cd frontend
npm install
cp .env.example .env   # set VITE_API_BASE_URL to your API's URL
npm run dev
```

## Renaming for a new project

Everything is namespaced `CleanStart.*` / `cleanstart-*` so a find-and-replace is
enough:

1. Rename the 4 project folders and `.csproj` files (`CleanStart.Domain` →
   `YourApp.Domain`, etc.), and the `.sln`.
2. Find-and-replace `CleanStart` → `YourApp` across `/backend` (namespaces, project
   references, `UserSecretsId`).
3. Update `frontend/package.json` name and `frontend/index.html` title.
4. Delete the `TodoItems` sample feature (Application + the controller + the
   Dashboard page) once you've copied its pattern for your first real feature.

## Adding a new feature (backend)

1. Add the entity to `CleanStart.Domain/Entities`, inheriting `BaseEntity`.
2. Add a `DbSet<T>` + soft-delete query filter to `AppDbContext`.
3. Create a migration (`dotnet ef migrations add ...`).
4. Add a feature file under `CleanStart.Application/<FeatureName>/` following
   `TodoItemFeature.cs` — DTOs, commands/queries, validators, handlers.
5. Add a thin controller under `CleanStart.API/Controllers` that sends to MediatR.

No DI wiring needed for any of this — `AddApplication()` / `AddInfrastructure()`
pick up new handlers, validators and repositories automatically via assembly scanning
and the generic `IRepository<T>`.

## Design choices worth knowing

- **Soft delete by default.** `Remove()` on the repository never issues a physical
  `DELETE` — it flips `IsDeleted`. Add a query filter per entity in `AppDbContext` so
  deleted rows disappear from normal queries.
- **CORS wide-open only in Development.** Set `Cors:AllowedOrigins` for every other
  environment or the API will reject cross-origin requests.
- **JWT only, no refresh tokens.** Tokens expire after 2 hours (`JwtTokenGenerator`).
  Add a refresh-token flow once you need longer sessions — CampusConnect's
  `AuthController`/`http.ts` pair is a reference if you want to see one.
- **Migrations run on startup.** Fine early on; switch to a separate deploy step once
  you have a real release process.
