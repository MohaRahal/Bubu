const apiBaseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

async function readError(response, fallbackMessage) {
  const problem = await response.json().catch(() => null)
  const validationMessage = problem?.errors ? Object.values(problem.errors).flat()[0] : null
  return validationMessage ?? problem?.title ?? problem?.detail ?? fallbackMessage
}

export async function getFinanceDashboard({ accountId, from, to, signal }) {
  const params = new URLSearchParams({ from, to })
  if (accountId) params.set('accountId', accountId)

  const response = await fetch(`${apiBaseUrl}/api/finance?${params}`, {
    credentials: 'include',
    signal,
  })
  if (!response.ok) throw new Error(await readError(response, 'Não foi possível carregar suas finanças.'))
  return response.json()
}

async function create(path, payload) {
  const response = await fetch(`${apiBaseUrl}/api/finance/${path}`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!response.ok) throw new Error(await readError(response, 'Não foi possível salvar essa movimentação.'))
  return response.status === 204 ? null : response.json().catch(() => null)
}

export const createFinanceAccount = (payload) => create('accounts', payload)
export const createFinanceTransaction = (payload) => create('transactions', payload)
export const createFinanceTransfer = (payload) => create('transfers', payload)
export const createFinanceCategory = (payload) => create('categories', payload)
export const createFinanceRecurring = (payload) => create('recurring-transactions', payload)

async function patch(path, fallbackMessage) {
  const response = await fetch(`${apiBaseUrl}/api/finance/${path}`, {
    method: 'PATCH',
    credentials: 'include',
  })
  if (!response.ok) throw new Error(await readError(response, fallbackMessage))
}

export const pauseFinanceRecurring = (id) =>
  patch(`recurring-transactions/${id}/pause`, 'Não foi possível pausar essa mensalidade.')

export const resumeFinanceRecurring = (id) =>
  patch(`recurring-transactions/${id}/resume`, 'Não foi possível retomar essa mensalidade.')

export async function updateFinanceAccount(id, payload) {
  const response = await fetch(`${apiBaseUrl}/api/finance/accounts/${id}`, {
    method: 'PUT',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  if (!response.ok) throw new Error(await readError(response, 'Não foi possível editar essa conta.'))
}

export async function deleteFinanceAccount(id) {
  const response = await fetch(`${apiBaseUrl}/api/finance/accounts/${id}`, {
    method: 'DELETE',
    credentials: 'include',
  })
  if (!response.ok) throw new Error(await readError(response, 'Não foi possível excluir essa conta.'))
}
