# Samara Dashboard — Frontend

Next.js 16 (App Router) + TypeScript + MUI v9 + TanStack Query + D3.js.

The home page shows a **profile-distribution chart** (system recommendation vs.
student survey choice) and per-cluster **student tables**. Clicking a student
opens `/students/[id]` with the detailed profile analysis and warnings.

The frontend never talks to the backend directly from the browser — it proxies
`/bff/*` through the Next server to the API gateway (no CORS).

---

## Prerequisites

- The **backend stack must be running** (it provides the API gateway on
  `:8080`). See the repo root `README.md` for the backend. Quick start:
  ```bash
  # from the repo root
  docker compose -f deploy/docker-compose.yml up --build -d
  ```
  > The Analysis service uses a deterministic engine by default, so an OpenAI
  > key is **optional**. After a fresh start, trigger analysis once so
  > recommendations/warnings exist (see root README), e.g.
  > `POST http://localhost:8080/analysis/api/analysis/schools/{schoolId}/run`.

---

## Option A — Run locally (development)

Requires Node.js 20.9+ (22 LTS recommended).

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:3000.

By default the Next server proxies `/bff/*` to `http://localhost:8080`. If your
gateway is elsewhere, set `BACKEND_URL`:

```bash
BACKEND_URL=http://localhost:8080 npm run dev
```

Other scripts: `npm run build` (production build), `npm start` (serve the build),
`npm run lint`.

---

## Option B — Run in Docker (full stack)

The frontend is part of the compose stack and is built automatically:

```bash
# from the repo root
docker compose -f deploy/docker-compose.yml up --build -d
```

Then open:

| Service            | URL                                  |
| ------------------ | ------------------------------------ |
| **Frontend**       | http://localhost:3000                |
| API gateway        | http://localhost:8080                |
| Swagger (Analysis) | http://localhost:5105/swagger        |
| RabbitMQ UI        | http://localhost:15672 (guest/guest) |

Inside the network the frontend proxies `/bff/*` to `http://gateway:8080`
(baked at build time via the `BACKEND_URL` build arg in `deploy/docker-compose.yml`).

Build/run just the frontend image on its own:

```bash
cd frontend
docker build --build-arg BACKEND_URL=http://host.docker.internal:8080 -t samara-frontend .
docker run --rm -p 3000:3000 samara-frontend
```

Stop the stack:

```bash
docker compose -f deploy/docker-compose.yml down       # keep data
docker compose -f deploy/docker-compose.yml down -v    # also drop the DB volume
```

---

## Notes

- `BACKEND_URL` is read by Next at **build time** (rewrite destinations are
  resolved during `next build`). The Docker image is therefore built with
  `http://gateway:8080`; local dev defaults to `http://localhost:8080`.
- The React Query Devtools panel is visible in dev only; it is not in the
  production build.
