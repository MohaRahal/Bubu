const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

async function readProblem(response, fallbackMessage) {
  const problem = await response.json().catch(() => null)
  return problem?.title ?? problem?.detail ?? fallbackMessage
}

async function request(path, options = {}) {
  const response = await fetch(`${apiBaseUrl}/api/goals${path}`, {
    credentials: 'include',
    ...options,
    headers: options.body ? { 'Content-Type': 'application/json', ...options.headers } : options.headers,
  })

  if (!response.ok) {
    throw new Error(await readProblem(response, 'Não foi possível concluir a operação.'))
  }

  return response.status === 204 ? null : response.json()
}

export const getGoals = (signal) => request('', { signal })

export const getGoalAccountOptions = (signal) => request('/account-options', { signal })

export const createGoal = (goal) => request('', {
  method: 'POST',
  body: JSON.stringify(goal),
})

export const addGoalContribution = (goalId, contribution) => request(`/${goalId}/contributions`, {
  method: 'POST',
  body: JSON.stringify(contribution),
})

export const deleteGoal = (goalId) => request(`/${goalId}`, { method: 'DELETE' })
