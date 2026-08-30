const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

async function readError(response, fallbackMessage) {
  const problem = await response.json().catch(() => null)
  const validationMessage = problem?.errors
    ? Object.values(problem.errors).flat()[0]
    : null

  return validationMessage ?? problem?.title ?? problem?.detail ?? fallbackMessage
}

export async function getMyDay(date, signal) {
  const response = await fetch(`${apiBaseUrl}/api/my-day?date=${date}`, {
    credentials: 'include',
    signal,
  })

  if (!response.ok) throw new Error(await readError(response, 'Não foi possível carregar seu dia.'))
  return response.json()
}

export async function createMyDayActivity(activity) {
  const response = await fetch(`${apiBaseUrl}/api/my-day/activities`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(activity),
  })

  if (!response.ok) throw new Error(await readError(response, 'Não foi possível criar a atividade.'))
  return response.json()
}

async function runActivityAction(id, action) {
  const response = await fetch(`${apiBaseUrl}/api/my-day/activities/${id}/${action}`, {
    method: 'PATCH',
    credentials: 'include',
  })

  if (!response.ok) throw new Error(await readError(response, 'Não foi possível atualizar a atividade.'))
  return response.json()
}

export const completeActivity = (id) => runActivityAction(id, 'complete')
export const startActivity = (id) => runActivityAction(id, 'start')
export const pauseActivity = (id) => runActivityAction(id, 'pause')
export const stopActivity = (id) => runActivityAction(id, 'stop')

export async function deleteActivity(id) {
  const response = await fetch(`${apiBaseUrl}/api/my-day/activities/${id}`, {
    method: 'DELETE',
    credentials: 'include',
  })

  if (!response.ok) throw new Error(await readError(response, 'Não foi possível remover a atividade.'))
}
