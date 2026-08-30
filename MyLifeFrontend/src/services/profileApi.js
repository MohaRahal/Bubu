const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

async function readError(response, fallbackMessage) {
  const problem = await response.json().catch(() => null)
  const validationMessage = problem?.errors
    ? Object.values(problem.errors).flat()[0]
    : null

  return validationMessage ?? problem?.title ?? problem?.detail ?? fallbackMessage
}

export async function getProfile(signal) {
  const response = await fetch(`${apiBaseUrl}/api/profile`, {
    credentials: 'include',
    signal,
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Não foi possível carregar seu perfil.'))
  }

  return response.json()
}

export async function getAvatars(signal) {
  const response = await fetch(`${apiBaseUrl}/api/avatars`, { signal })

  if (!response.ok) {
    throw new Error(await readError(response, 'Não foi possível carregar os avatares.'))
  }

  return response.json()
}

export async function updateProfile(profile) {
  const response = await fetch(`${apiBaseUrl}/api/profile`, {
    method: 'PUT',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(profile),
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Não foi possível salvar seu perfil.'))
  }

  return response.json()
}

export async function changePassword(passwords) {
  const response = await fetch(`${apiBaseUrl}/api/profile/password`, {
    method: 'PUT',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(passwords),
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Não foi possível alterar sua senha.'))
  }
}
