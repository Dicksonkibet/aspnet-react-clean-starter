// Thin fetch wrapper for talking to the CleanStart API. No mock-data fallback —
// if VITE_API_BASE_URL is unset, every request throws immediately with a clear
// message instead of the app silently behaving as if a backend were there.

export const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, "");

const DEFAULT_TIMEOUT_MS = 15000;

export class HttpError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

function authHeader(): Record<string, string> {
  const token = useAuthStoreToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

// Reads the token straight out of the persisted zustand store without importing the
// store module directly, so this file has zero dependency cycle risk with app state.
function useAuthStoreToken(): string | null {
  try {
    const raw = localStorage.getItem("cleanstart:auth");
    const token = raw ? JSON.parse(raw)?.state?.token : null;
    return token ?? null;
  } catch {
    return null;
  }
}

function extractErrorMessage(data: unknown, fallback: string): string {
  if (typeof data === "string") return data;
  if (data && typeof data === "object") {
    const obj = data as Record<string, unknown>;
    if (typeof obj.detail === "string" && obj.detail) return obj.detail;
    if (typeof obj.message === "string") return obj.message;
    if (obj.errors && typeof obj.errors === "object") {
      for (const value of Object.values(obj.errors as Record<string, unknown>)) {
        if (Array.isArray(value) && typeof value[0] === "string") return value[0];
      }
    }
    if (typeof obj.title === "string") return obj.title;
  }
  return fallback;
}

export async function apiFetch<T>(
  path: string,
  options: { method?: string; body?: unknown; timeoutMs?: number } = {},
): Promise<T> {
  if (!API_BASE_URL) {
    throw new HttpError("The app isn't connected to a backend (VITE_API_BASE_URL is unset).", 0);
  }

  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), options.timeoutMs ?? DEFAULT_TIMEOUT_MS);

  try {
    const res = await fetch(`${API_BASE_URL}${path}`, {
      method: options.method ?? "GET",
      headers: {
        "Content-Type": "application/json",
        ...authHeader(),
      },
      body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
      signal: controller.signal,
    });

    if (!res.ok) {
      let message = `Request failed (${res.status})`;
      try {
        message = extractErrorMessage(await res.json(), message);
      } catch {
        // response wasn't JSON — keep the generic message
      }
      throw new HttpError(message, res.status);
    }

    if (res.status === 204) return undefined as T;
    const text = await res.text();
    return text ? (JSON.parse(text) as T) : (undefined as T);
  } finally {
    clearTimeout(timeout);
  }
}
