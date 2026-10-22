# BeautyBeast

BeautyBeast is my final year project for my BSc in Computer Science. It's a social media platform built specifically for the beauty industry — think something between Instagram, LinkedIn, and a booking system, but for makeup artists, hairdressers, nail techs, etc.

The idea is that artists get a proper profile to show off their work and credentials, and clients can find them, book appointments, and leave reviews. There's also a resource library, consent form templates, and a feed for posts and updates.

## What it's built with

- **ASP.NET Core 9** — REST API (Minimal API style)
- **Blazor Server** — frontend, runs on .NET 9
- **PostgreSQL** — database
- **Entity Framework Core** — ORM, handles migrations and seeding
- **JWT** — authentication
- **Docker / Docker Compose** — for running everything together

## Features

- Artist profiles (LinkedIn-style — bio, experience, education, skills, achievements)
- Client profiles (more of a social timeline)
- Booking system — clients book treatments, artists manage their calendar
- Treatment menu — artists list their services with prices and duration
- Posts — articles, photos, videos, comments and likes
- Status updates — short posts with reactions
- Educational resource library
- Consent form templates — artists create them, clients sign before a treatment
- Reviews — only available after a completed booking
- Search — find artists or treatments
- Media uploads

## Running it with Docker (easiest way)

You'll need Docker Desktop installed and running.

```bash
cp .env.example .env
```

Open the `.env` file and set `JWT_KEY` to any random string that's at least 32 characters long. Then:

```bash
docker compose up --build
```

That's it. It'll pull PostgreSQL, build both projects, run the migrations, and seed some demo data automatically.

- Web app → http://localhost:5100
- API → http://localhost:5200
- Swagger → http://localhost:5200/swagger

To stop it: `Ctrl+C` then `docker compose down`. If you want to wipe the database too, add `-v`.

## Demo accounts

| Username | Password | Role |
|---|---|---|
| `ana.beauty` | `password123` | Artist |
| `mia.client` | `password123` | Client |

To test the review flow you need a completed booking — log in as `mia.client`, book something with `ana.beauty`, then switch to the artist account and mark it as completed.

## Running locally (without Docker)

You'll need the [.NET 9 SDK](https://dotnet.microsoft.com/download) and a PostgreSQL instance running.

Add an `appsettings.Development.json` file inside `BeautyBeast.Api/` with your connection string and a JWT key:

```json
{
  "ConnectionStrings": {
    "BeautyBeast": "Host=localhost;Port=5432;Database=beautybeast;Username=youruser;Password=yourpassword"
  },
  "Jwt": {
    "Key": "some-long-random-secret-at-least-32-chars"
  }
}
```

Then open two terminals:

```bash
# Terminal 1
cd BeautyBeast.Api
dotnet run

# Terminal 2
cd BeautyBeast.Web
dotnet run
```

The API will create and seed the database on first run.

## Project structure

```
BeautyBeast.Api/
├── Data/           — DbContext and seeder
├── Dtos/           — request/response shapes
├── Endpoints/      — all the API routes
├── Entities/       — database models
├── Helpers/        — JWT helper, password hashing
├── Migrations/     — EF Core migrations
└── wwwroot/uploads/— where uploaded images live

BeautyBeast.Web/
├── Auth/           — JWT auth state provider
├── Components/
│   ├── Layout/     — main layout, nav menu
│   ├── Pages/      — all the pages
│   └── Shared/     — reusable bits (cards, uploader, etc.)
├── Models/         — mirrors of the API DTOs
└── Services/       — wrappers around the API calls
```

## How it works

The app is split into two separate projects. The API handles all the data and business logic, and the Blazor frontend calls it over HTTP. Because Blazor Server runs on the server (not in the browser), all the API calls happen server-side — the browser just gets the rendered HTML and stays connected via SignalR.

JWT tokens are issued by the API on login and stored in the browser's session storage. The API validates them on every request.

I split them into two projects partly because it made sense architecturally, and partly because it means the API could be reused by a mobile app down the line.

## Environment variables

If you're using Docker, these all go in your `.env` file (copy from `.env.example`):

| Variable | What it's for |
|---|---|
| `POSTGRES_DB` | database name |
| `POSTGRES_USER` | database user |
| `POSTGRES_PASSWORD` | database password |
| `JWT_KEY` | signing key for JWT tokens (min 32 chars) |
| `JWT_ISSUER` | defaults to `BeautyBeast.Api` |
| `JWT_AUDIENCE` | defaults to `BeautyBeast.Web` |

Don't commit your `.env` file — it's already in `.gitignore`.
