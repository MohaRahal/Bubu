const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

async function readError(response, fallbackMessage) {
  const problem = await response.json().catch(() => null)
  const validationMessage = problem?.errors
    ? Object.values(problem.errors).flat()[0]
    : null
  return validationMessage ?? problem?.title ?? problem?.detail ?? fallbackMessage
}

export async function getQuickRegisterOptions(signal) {
  const response = await fetch(`${apiBaseUrl}/api/quick-register/options`, {
    credentials: 'include',
    signal,
  })
  if (!response.ok) throw new Error(await readError(response, 'Não foi possível preparar o registro.'))
  return response.json()
}

async function create(path, payload) {
  const response = await fetch(`${apiBaseUrl}/api/quick-register/${path}`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!response.ok) throw new Error(await readError(response, 'Não foi possível salvar o registro.'))
  return response.json()
}

export const createQuickGoal = (goal) => create('goals', goal)
export const createQuickTransaction = (transaction) => create('transactions', transaction)
