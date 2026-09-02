const baseUrl = (import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7090').replace(/\/$/, '')

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

interface RequestOptions {
  /** Bearer access token to send in the Authorization header. */
  token?: string | null
}

/**
 * Thin wrapper around `fetch` for the FairPlay backend. Always sends cookies
 * (`credentials: 'include'`) so the HttpOnly refresh-token cookie rides along,
 * and unwraps the `{ error }` body the API returns on failure.
 */
async function request<T>(
  method: string,
  path: string,
  body?: unknown,
  { token }: RequestOptions = {},
): Promise<T> {
  const headers: Record<string, string> = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  if (token) headers['Authorization'] = `Bearer ${token}`

  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers,
    credentials: 'include',
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  if (response.status === 204) return undefined as T

  const payload = await response.json().catch(() => null)

  if (!response.ok) {
    const message =
      (payload && typeof payload === 'object' && 'error' in payload && String(payload.error)) ||
      `La solicitud ha fallado (${response.status}).`
    throw new ApiError(response.status, message)
  }

  return payload as T
}

export const http = {
  get: <T>(path: string, options?: RequestOptions) => request<T>('GET', path, undefined, options),
  post: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>('POST', path, body, options),
}
