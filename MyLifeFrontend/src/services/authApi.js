const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

async function readProblem(response, fallbackMessage) {
  const problem = await response.json().catch(() => null)
  return problem?.title ?? problem?.detail ?? fallbackMessage
}

export async function login(credentials, signal) {
  const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials),
    signal,
  })

  if (!response.ok) {
    throw new Error(await readProblem(response, 'Não foi possível entrar.'))
  }

  return response.json()
}

export async function getCurrentUser(signal) {
  const response = await fetch(`${apiBaseUrl}/api/auth/me`, {
    credentials: 'include',
    signal,
  })

  if (response.status === 401) {
    return null
  }

  if (!response.ok) {
    throw new Error(await readProblem(response, 'Não foi possível verificar sua sessão.'))
  }

  return response.json()
}

export async function logout() {
  const response = await fetch(`${apiBaseUrl}/api/auth/logout`, {
    method: 'POST',
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error(await readProblem(response, 'Não foi possível sair.'))
  }
}

export async function requestPasswordReset(email, signal) {
  const response = await fetch(`${apiBaseUrl}/api/auth/password-reset/request`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email }),
    signal,
  })

  if (!response.ok) {
    throw new Error(await readProblem(response, 'Não foi possível solicitar a recuperação.'))
  }

  return response.json()
}

export async function validatePasswordResetCode(payload, signal) {
  const response = await fetch(`${apiBaseUrl}/api/auth/password-reset/validate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
    signal,
  })

  if (!response.ok) {
    throw new Error(await readProblem(response, 'Não foi possível validar o código.'))
  }

  return response.json()
}

export async function completePasswordReset(payload, signal) {
  const response = await fetch(`${apiBaseUrl}/api/auth/password-reset/confirm`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
    signal,
  })

  if (!response.ok) {
    throw new Error(await readProblem(response, 'Não foi possível redefinir sua senha.'))
  }
}

export async function getDashboard(signal) {
  const response = await fetch(`${apiBaseUrl}/api/dashboard`, {
    credentials: 'include',
    signal,
  })

  if (!response.ok) {
    throw new Error(await readProblem(response, 'Não foi possível carregar sua dashboard.'))
  }

  return response.json()
}
