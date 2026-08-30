const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

function getErrorMessage(problem) {
  if (problem?.errors) {
    const firstValidationError = Object.values(problem.errors).flat()[0]

    if (firstValidationError) {
      return firstValidationError
    }
  }

  return problem?.title ?? problem?.detail ?? 'Não foi possível criar sua conta.'
}

export async function createUser(user, signal) {
  const response = await fetch(`${apiBaseUrl}/api/users`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(user),
    signal,
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new Error(getErrorMessage(problem))
  }

  return response.json()
}
