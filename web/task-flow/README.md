# TaskFlow — Frontend (Web)

[← Back to project overview](../../README.md)

Angular 22 SPA with Signals-based state management and Tailwind CSS. Communicates with the [TaskFlow API](../../api/TaskFlow/README.md).

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [Running Locally](#running-locally)
- [Running the Tests](#running-the-tests)
- [Building for Production](#building-for-production)

---

## Prerequisites

| Tool | Version | Download |
| --- | --- | --- |
| Node.js | **22 or later** | <https://nodejs.org/en/download> |
| npm | **11 or later** _(bundled with Node 22)_ | — |
| Git | Any recent | <https://git-scm.com> |

Verify your installation:

```bash
node --version
# Expected: v22.x.x

npm --version
# Expected: 11.x.x
```

> **No global Angular CLI required** — the project uses the version pinned in `devDependencies` and all scripts run through `npm run`.

---

## Running Locally

> The API must be running before starting the frontend. See [api/TaskFlow/README.md](../../api/TaskFlow/README.md).

```bash
# 1. Navigate to the Angular workspace
cd web/task-flow

# 2. Install dependencies (first time or after package.json changes)
npm install

# 3. Start the development server
npm start
```

The app is served at `http://localhost:4200` and reloads automatically on file changes.

### Environment configuration

The dev build points to the API via `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7045',
};
```

No changes are needed for local development. If the API runs on a different port, update `apiUrl` here.

---

## Running the Tests

The project uses **Vitest 4** as the test runner with **jsdom** as the DOM environment.

```bash
# From the Angular workspace
cd web/task-flow

# Install dependencies if you have not already
npm install

# Run tests once (CI mode)
npm test

# Run tests in watch mode (re-runs on file changes)
npx ng test --watch
```

### What is covered

| Area | Test file |
| --- | --- |
| `Task` model mapping functions | `task.model.spec.ts` |
| `TaskService` signal-based state | `task.service.spec.ts` |
| Root `AppComponent` bootstrap | `app.spec.ts` |

---

## Building for Production

```bash
npm run build
```

Output is written to `dist/task-flow/`. To serve the SSR bundle:

```bash
npm run serve:ssr:task-flow
```

---

[← Back to project overview](../../README.md)
