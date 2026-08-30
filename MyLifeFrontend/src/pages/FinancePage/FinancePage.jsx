import { useEffect, useMemo, useState } from 'react'
import { createPortal } from 'react-dom'
import {
  ArrowDownLeft,
  ArrowRightLeft,
  ArrowUpRight,
  Building2,
  CalendarDays,
  Check,
  ChevronDown,
  CircleDollarSign,
  CreditCard,
  Landmark,
  LoaderCircle,
  Pause,
  Pencil,
  Play,
  Plus,
  RefreshCw,
  Repeat2,
  Trash2,
  TrendingDown,
  TrendingUp,
  WalletCards,
  X,
} from 'lucide-react'
import {
  Area,
  Bar,
  CartesianGrid,
  ComposedChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { AuthMascot } from '../../components/auth/AuthMascot'
import bubuSavingMoney from '../../assets/bubu-guardando-dinheiro.png'
import bubuSpendingMoney from '../../assets/bubu-tirando-dinheiro.png'
import {
  createFinanceAccount,
  createFinanceCategory,
  createFinanceRecurring,
  createFinanceTransaction,
  createFinanceTransfer,
  deleteFinanceAccount,
  getFinanceDashboard,
  pauseFinanceRecurring,
  resumeFinanceRecurring,
  updateFinanceAccount,
} from '../../services/financeApi'
import styles from './FinancePage.module.css'

const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const compactCurrency = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
  notation: 'compact',
  maximumFractionDigits: 1,
})
const dayMonth = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: 'short' })
const fullDate = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: 'long',
  year: 'numeric',
})

const timezoneOffset = new Date().getTimezoneOffset() * 60_000
const today = new Date(Date.now() - timezoneOffset).toISOString().slice(0, 10)

const accountTypeLabels = {
  checking: 'Conta corrente',
  savings: 'Poupança',
  cash: 'Dinheiro',
  wallet: 'Carteira digital',
  investment: 'Investimento',
  other: 'Outra',
}

const categoryColors = ['#70c52f', '#5f8fe8', '#a16fd0', '#ed9b45', '#e66f68', '#55b7ad']

const recurrenceLabels = {
  daily: 'Todo dia',
  weekly: 'Toda semana',
  monthly: 'Todo mês',
  yearly: 'Todo ano',
}

function monthRange() {
  const date = new Date()
  return {
    from: new Date(date.getFullYear(), date.getMonth(), 1, 12).toISOString().slice(0, 10),
    to: today,
  }
}

function daysRange(days) {
  const date = new Date()
  date.setDate(date.getDate() - days + 1)
  return { from: date.toISOString().slice(0, 10), to: today }
}

const ranges = {
  month: { label: 'Este mês', getValue: monthRange },
  '30days': { label: '30 dias', getValue: () => daysRange(30) },
  '90days': { label: '3 meses', getValue: () => daysRange(90) },
  year: {
    label: 'Este ano',
    getValue: () => ({ from: `${new Date().getFullYear()}-01-01`, to: today }),
  },
}

function formatCurrency(value) {
  return currency.format(value ?? 0)
}

function formatChartDate(value) {
  return dayMonth.format(new Date(`${value}T12:00:00`)).replace('.', '')
}

export function FinancePage() {
  const [dashboard, setDashboard] = useState(null)
  const [selectedAccountId, setSelectedAccountId] = useState('')
  const [selectedRange, setSelectedRange] = useState('month')
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [reloadKey, setReloadKey] = useState(0)
  const [modalType, setModalType] = useState(null)
  const [accountToEdit, setAccountToEdit] = useState(null)
  const [accountToDelete, setAccountToDelete] = useState(null)
  const [movementView, setMovementView] = useState('movements')
  const [recurringAction, setRecurringAction] = useState(null)

  const range = useMemo(() => ranges[selectedRange].getValue(), [selectedRange])

  useEffect(() => {
    const controller = new AbortController()
    let isCurrent = true

    getFinanceDashboard({
      accountId: selectedAccountId,
      from: range.from,
      to: range.to,
      signal: controller.signal,
    })
      .then((data) => isCurrent && setDashboard(data))
      .catch((error) => {
        if (isCurrent && error.name !== 'AbortError') setErrorMessage(error.message)
      })
      .finally(() => isCurrent && setIsLoading(false))

    return () => {
      isCurrent = false
      controller.abort()
    }
  }, [range.from, range.to, reloadKey, selectedAccountId])

  const selectedAccount = dashboard?.accounts.find(
    (account) => account.id.toString() === selectedAccountId,
  )
  const chartData = useMemo(
    () =>
      dashboard?.chart.map((point) => ({
        ...point,
        label: formatChartDate(point.date),
      })) ?? [],
    [dashboard],
  )

  function handleCreated() {
    setModalType(null)
    setAccountToEdit(null)
    setReloadKey((current) => current + 1)
  }

  if (isLoading && !dashboard) return <FinanceSkeleton />

  return (
    <div className={styles.page}>
      {modalType &&
        createPortal(
          <FinanceActionModal
            type={modalType}
            accounts={dashboard?.accounts ?? []}
            categories={dashboard?.categories ?? []}
            selectedAccountId={selectedAccountId}
            account={modalType === 'edit-account' ? accountToEdit : null}
            onClose={() => {
              setModalType(null)
              setAccountToEdit(null)
            }}
            onCreated={handleCreated}
          />,
          document.body,
        )}

      {accountToDelete &&
        createPortal(
          <DeleteAccountModal
            account={accountToDelete}
            onClose={() => setAccountToDelete(null)}
            onDeleted={() => {
              if (selectedAccountId === accountToDelete.id.toString()) setSelectedAccountId('')
              setAccountToDelete(null)
              setReloadKey((current) => current + 1)
            }}
          />,
          document.body,
        )}

      {recurringAction &&
        createPortal(
          <RecurringActionModal
            recurrence={recurringAction.recurrence}
            action={recurringAction.action}
            onClose={() => setRecurringAction(null)}
            onConfirmed={() => {
              setRecurringAction(null)
              setReloadKey((current) => current + 1)
            }}
          />,
          document.body,
        )}

      <section className={styles.heading}>
        <div>
          <span className={styles.eyebrow}>SEU DINHEIRO, SEM COMPLICAÇÃO</span>
          <h1>Finanças</h1>
          <p>Acompanhe suas contas juntas ou entenda cada uma separadamente.</p>
        </div>
        <div className={styles.headingActions}>
          <button
            className={styles.secondaryButton}
            type="button"
            onClick={() => setModalType('transfer')}
            disabled={(dashboard?.accounts.length ?? 0) < 2}
          >
            <ArrowRightLeft size={17} /> Transferir
          </button>
          <button
            className={styles.primaryButton}
            type="button"
            onClick={() => setModalType('income')}
          >
            <Plus size={17} /> Nova movimentação
          </button>
        </div>
      </section>

      {errorMessage && (
        <div className={styles.errorState} role="alert">
          <RefreshCw size={19} />
          <span>{errorMessage}</span>
          <button type="button" onClick={() => setReloadKey((current) => current + 1)}>
            Tentar novamente
          </button>
        </div>
      )}

      <section className={styles.controlBar}>
        <label>
          <WalletCards size={17} />
          <select
            value={selectedAccountId}
            onChange={(event) => {
              setSelectedAccountId(event.target.value)
              setErrorMessage('')
            }}
          >
            <option value="">Todas as contas</option>
            {dashboard?.accounts.map((account) => (
              <option value={account.id} key={account.id}>
                {account.name}
              </option>
            ))}
          </select>
          <ChevronDown size={15} />
        </label>
        <div className={styles.rangeTabs}>
          {Object.entries(ranges).map(([key, item]) => (
            <button
              className={selectedRange === key ? styles.activeRange : undefined}
              type="button"
              onClick={() => {
                setSelectedRange(key)
                setErrorMessage('')
              }}
              key={key}
            >
              {item.label}
            </button>
          ))}
        </div>
        <span>
          <CalendarDays size={15} /> {formatChartDate(range.from)} — {formatChartDate(range.to)}
        </span>
      </section>

      <section className={styles.summaryGrid}>
        <SummaryCard
          icon={CircleDollarSign}
          label={selectedAccount ? `Saldo em ${selectedAccount.name}` : 'Patrimônio disponível'}
          value={formatCurrency(dashboard?.summary.balance)}
          note={
            selectedAccount
              ? accountTypeLabels[selectedAccount.type]
              : `${dashboard?.accounts.length ?? 0} contas ativas`
          }
          tone="dark"
        />
        <SummaryCard
          icon={TrendingUp}
          label="Entradas no período"
          value={formatCurrency(dashboard?.summary.income)}
          note="Receitas registradas"
          tone="green"
        />
        <SummaryCard
          icon={TrendingDown}
          label="Saídas no período"
          value={formatCurrency(dashboard?.summary.expense)}
          note="Despesas registradas"
          tone="red"
        />
        <SummaryCard
          icon={ArrowUpRight}
          label="Resultado do período"
          value={formatCurrency(dashboard?.summary.periodResult)}
          note={
            dashboard?.summary.periodResult >= 0
              ? 'Você ficou no positivo'
              : 'Saídas acima das entradas'
          }
          tone={dashboard?.summary.periodResult >= 0 ? 'lime' : 'red'}
        />
      </section>

      <section className={styles.mainGrid}>
        <article className={styles.chartCard}>
          <header>
            <div>
              <span>FLUXO FINANCEIRO</span>
              <h2>{selectedAccount ? selectedAccount.name : 'Todas as contas'}</h2>
            </div>
            <div className={styles.legend}>
              <span>
                <i className={styles.incomeDot} /> Entradas
              </span>
              <span>
                <i className={styles.expenseDot} /> Saídas
              </span>
              <span>
                <i className={styles.balanceDot} /> Saldo
              </span>
            </div>
          </header>
          <div className={styles.chartArea}>
            <ResponsiveContainer width="100%" height="100%">
              <ComposedChart data={chartData} margin={{ top: 18, right: 14, left: 0, bottom: 0 }}>
                <defs>
                  <linearGradient id="financeBalance" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="#8edf3f" stopOpacity=".38" />
                    <stop offset="100%" stopColor="#8edf3f" stopOpacity=".02" />
                  </linearGradient>
                </defs>
                <CartesianGrid stroke="#e8ece8" strokeDasharray="3 4" vertical={false} />
                <XAxis
                  dataKey="label"
                  axisLine={false}
                  tickLine={false}
                  minTickGap={28}
                  tick={{ fill: '#969e98', fontSize: 10 }}
                  dy={9}
                />
                <YAxis
                  axisLine={false}
                  tickLine={false}
                  width={63}
                  tickFormatter={(value) => compactCurrency.format(value)}
                  tick={{ fill: '#969e98', fontSize: 10 }}
                />
                <Tooltip content={<FinanceTooltip />} cursor={{ fill: 'rgba(112,197,47,.045)' }} />
                <Bar
                  name="Entradas"
                  dataKey="income"
                  fill="#b8ec78"
                  radius={[4, 4, 0, 0]}
                  maxBarSize={10}
                />
                <Bar
                  name="Saídas"
                  dataKey="expense"
                  fill="#f3aaa3"
                  radius={[4, 4, 0, 0]}
                  maxBarSize={10}
                />
                <Area
                  name="Saldo"
                  type="monotone"
                  dataKey="balance"
                  stroke="#4f9e2b"
                  strokeWidth={2.5}
                  fill="url(#financeBalance)"
                  dot={false}
                  activeDot={{ r: 5, fill: '#fff', stroke: '#4f9e2b', strokeWidth: 2 }}
                />
              </ComposedChart>
            </ResponsiveContainer>
          </div>
          {selectedAccount &&
            (dashboard.summary.incomingTransfers > 0 ||
              dashboard.summary.outgoingTransfers > 0) && (
              <footer>
                <span>
                  Transferências recebidas{' '}
                  <strong>{formatCurrency(dashboard.summary.incomingTransfers)}</strong>
                </span>
                <span>
                  Transferências enviadas{' '}
                  <strong>{formatCurrency(dashboard.summary.outgoingTransfers)}</strong>
                </span>
              </footer>
            )}
        </article>

        <aside className={styles.accountsPanel}>
          <header>
            <div>
              <span>SUAS CONTAS</span>
              <h2>Onde está seu dinheiro</h2>
            </div>
            <button
              type="button"
              onClick={() => setModalType('account')}
              aria-label="Adicionar conta"
            >
              <Plus size={17} />
            </button>
          </header>
          <button
            className={`${styles.allAccountsCard} ${!selectedAccountId ? styles.selectedAccount : ''}`}
            type="button"
            onClick={() => setSelectedAccountId('')}
          >
            <span>
              <WalletCards size={19} />
            </span>
            <div>
              <small>TODAS JUNTAS</small>
              <strong>
                {formatCurrency(
                  dashboard?.accounts.reduce((sum, account) => sum + account.balance, 0),
                )}
              </strong>
            </div>
            <Check size={17} />
          </button>
          <div className={styles.accountList}>
            {dashboard?.accounts.map((account, index) => (
              <AccountCard
                account={account}
                index={index}
                selected={selectedAccountId === account.id.toString()}
                onSelect={() => setSelectedAccountId(account.id.toString())}
                onEdit={() => {
                  setAccountToEdit(account)
                  setModalType('edit-account')
                }}
                onDelete={() => setAccountToDelete(account)}
                key={account.id}
              />
            ))}
          </div>
          {dashboard?.accounts.length === 0 && (
            <div className={styles.emptyAccounts}>
              <AuthMascot />
              <strong>Vamos criar sua primeira conta?</strong>
              <p>Ela pode ser banco, carteira ou até dinheiro guardado.</p>
              <button type="button" onClick={() => setModalType('account')}>
                Criar conta
              </button>
            </div>
          )}
        </aside>
      </section>

      <section className={styles.movementsCard}>
        <header>
          <div>
            <span>MOVIMENTAÇÕES</span>
            <h2>
              {movementView === 'movements' ? 'O que aconteceu no período' : 'Contas programadas'}
            </h2>
          </div>
          <div className={styles.movementHeaderControls}>
            <div className={styles.movementTabs}>
              <button
                className={movementView === 'movements' ? styles.activeMovementTab : undefined}
                type="button"
                onClick={() => setMovementView('movements')}
              >
                <CreditCard size={14} /> Movimentações
              </button>
              <button
                className={movementView === 'recurring' ? styles.activeMovementTab : undefined}
                type="button"
                onClick={() => setMovementView('recurring')}
              >
                <Repeat2 size={14} /> Recorrentes
              </button>
            </div>
            {movementView === 'movements' ? (
              <div className={styles.currentBalance}>
                <small>SALDO AGORA</small>
                <strong>{formatCurrency(dashboard?.summary.balance)}</strong>
              </div>
            ) : (
              <button
                className={styles.newRecurringButton}
                type="button"
                onClick={() => setModalType('recurring-expense')}
                disabled={(dashboard?.accounts.length ?? 0) === 0}
              >
                <Plus size={15} /> Nova recorrência
              </button>
            )}
          </div>
        </header>
        {movementView === 'movements' ? (
          <div>
            {dashboard?.recentMovements.length === 0 ? (
              <div className={styles.emptyMovements}>
                <span>
                  <CreditCard size={20} />
                </span>
                <strong>Nenhuma movimentação por aqui.</strong>
                <p>Quando você registrar uma entrada, saída ou transferência, ela aparece aqui.</p>
              </div>
            ) : (
              <>
                <div className={styles.movementTableHead}>
                  <i />
                  <span>Lançamento</span>
                  <span>Conta</span>
                  <span>Data</span>
                  <span>Valor</span>
                </div>
                {dashboard?.recentMovements.map((movement) => (
                  <MovementRow
                    movement={movement}
                    selectedAccountId={selectedAccountId}
                    key={movement.key}
                  />
                ))}
              </>
            )}
          </div>
        ) : (
          <div className={styles.recurringList}>
            {dashboard?.recurringTransactions.length ? (
              dashboard.recurringTransactions.map((recurrence) => (
                <article
                  className={!recurrence.isActive ? styles.pausedRecurring : undefined}
                  key={recurrence.id}
                >
                  <span className={styles.recurringIcon}>
                    <Repeat2 size={18} />
                  </span>
                  <div className={styles.recurringName}>
                    <strong>{recurrence.description}</strong>
                    <small>
                      {recurrence.categoryName ?? 'Sem categoria'} · {recurrence.accountName}
                    </small>
                  </div>
                  <div>
                    <small>FREQUÊNCIA</small>
                    <strong>{recurrenceLabels[recurrence.frequency]}</strong>
                  </div>
                  <div>
                    <small>{recurrence.isActive ? 'PRÓXIMO LANÇAMENTO' : 'PAUSADA EM'}</small>
                    <strong>
                      {fullDate.format(new Date(`${recurrence.nextDueDate}T12:00:00`))}
                    </strong>
                  </div>
                  <b>{formatCurrency(recurrence.amount)}</b>
                  <button
                    type="button"
                    className={recurrence.isActive ? styles.pauseRecurring : styles.resumeRecurring}
                    onClick={() =>
                      setRecurringAction({
                        recurrence,
                        action: recurrence.isActive ? 'pause' : 'resume',
                      })
                    }
                  >
                    {recurrence.isActive ? <Pause size={15} /> : <Play size={15} />}
                    {recurrence.isActive ? 'Pausar' : 'Retomar'}
                  </button>
                </article>
              ))
            ) : (
              <div className={styles.emptyRecurring}>
                <span>
                  <Repeat2 size={20} />
                </span>
                <div>
                  <strong>Nenhuma conta se repetindo ainda.</strong>
                  <p>Abra uma nova movimentação, escolha “Dinheiro saiu” e marque “Recorrente”.</p>
                </div>
                <button
                  type="button"
                  onClick={() => setModalType('recurring-expense')}
                  disabled={(dashboard?.accounts.length ?? 0) === 0}
                >
                  Criar a primeira
                </button>
              </div>
            )}
          </div>
        )}
      </section>
    </div>
  )
}

function SummaryCard({ icon: Icon, label, value, note, tone }) {
  return (
    <article className={`${styles.summaryCard} ${styles[tone]}`}>
      <div>
        <span>
          <Icon size={18} />
        </span>
        <small>{label}</small>
      </div>
      <strong>{value}</strong>
      <p>{note}</p>
    </article>
  )
}

function AccountCard({ account, index, selected, onSelect, onEdit, onDelete }) {
  const Icon =
    account.type === 'cash' ? WalletCards : account.type === 'investment' ? Landmark : Building2
  return (
    <article
      className={`${styles.accountCard} ${styles[`accountTone${index % 4}`]} ${selected ? styles.selectedAccount : ''}`}
    >
      <button className={styles.accountSelect} type="button" onClick={onSelect}>
        <span>
          <Icon size={18} />
        </span>
        <div>
          <small>{accountTypeLabels[account.type] ?? 'Conta'}</small>
          <strong>{account.name}</strong>
          <b>{formatCurrency(account.balance)}</b>
        </div>
      </button>
      <div className={styles.accountActions}>
        <button type="button" onClick={onEdit} aria-label={`Editar ${account.name}`}>
          <Pencil size={14} />
        </button>
        <button
          type="button"
          onClick={onDelete}
          aria-label={`Excluir ${account.name}`}
          title={
            account.hasMovements
              ? 'Contas com movimentações não podem ser excluídas'
              : 'Excluir conta'
          }
          disabled={account.hasMovements}
        >
          <Trash2 size={14} />
        </button>
      </div>
    </article>
  )
}

function MovementRow({ movement, selectedAccountId }) {
  const isTransfer = movement.kind === 'transfer'
  const isOpeningBalance = movement.kind === 'opening_balance'
  const isIncome = movement.kind === 'income'
  const isOutgoingTransfer =
    isTransfer && selectedAccountId && movement.accountId.toString() === selectedAccountId
  const prefix =
    isOpeningBalance || isIncome || (isTransfer && selectedAccountId && !isOutgoingTransfer)
      ? '+'
      : isTransfer && !selectedAccountId
        ? ''
        : '−'
  const Icon = isOpeningBalance
    ? Landmark
    : isTransfer
      ? ArrowRightLeft
      : isIncome
        ? ArrowDownLeft
        : ArrowUpRight
  const iconClass = isOpeningBalance
    ? styles.openingIcon
    : isTransfer
      ? styles.transferIcon
      : isIncome
        ? styles.incomeIcon
        : styles.expenseIcon
  const valueClass = isOpeningBalance
    ? styles.openingValue
    : isTransfer
      ? styles.transferValue
      : isIncome
        ? styles.incomeValue
        : styles.expenseValue
  const detail = isOpeningBalance
    ? 'Ponto de partida da conta'
    : isTransfer
      ? 'Transferência entre contas'
      : (movement.categoryName ?? (isIncome ? 'Entrada' : 'Saída'))
  const accountLabel = isTransfer
    ? `${movement.accountName} → ${movement.destinationAccountName}`
    : movement.accountName

  return (
    <article className={styles.movementRow}>
      <span className={iconClass}>
        <Icon size={18} />
      </span>
      <div>
        <strong>{movement.description}</strong>
        <small>{detail}</small>
      </div>
      <span className={styles.movementAccount}>{accountLabel}</span>
      <time>{fullDate.format(new Date(movement.date))}</time>
      <b className={valueClass}>
        {prefix}
        {formatCurrency(movement.amount)}
      </b>
    </article>
  )
}

function FinanceTooltip({ active, payload, label }) {
  if (!active || !payload?.length) return null
  return (
    <div className={styles.tooltip}>
      <span>{label}</span>
      {payload.map((item) => (
        <p key={item.dataKey}>
          <i style={{ background: item.color ?? item.fill }} />
          {item.name}
          <strong>{formatCurrency(item.value)}</strong>
        </p>
      ))}
    </div>
  )
}

function RecurringTransactionModal({
  accounts,
  categories,
  selectedAccountId,
  onClose,
  onCreated,
}) {
  const [form, setForm] = useState({
    description: '',
    amount: '',
    accountId: selectedAccountId || accounts[0]?.id?.toString() || '',
    categoryId: '',
    frequency: 'monthly',
    firstDueDate: today,
    endDate: '',
  })
  const [localCategories, setLocalCategories] = useState(categories)
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false)
  const [isReviewing, setIsReviewing] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isComplete, setIsComplete] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const expenseCategories = localCategories.filter((category) => category.type === 'expense')
  const account = accounts.find((item) => item.id.toString() === form.accountId)
  const category = localCategories.find((item) => item.id.toString() === form.categoryId)

  useEffect(() => {
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const handleEscape = (event) =>
      event.key === 'Escape' && !isSaving && !isCategoryModalOpen && onClose()
    window.addEventListener('keydown', handleEscape)
    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', handleEscape)
    }
  }, [isCategoryModalOpen, isSaving, onClose])

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
    setErrorMessage('')
  }

  async function save() {
    setIsSaving(true)
    setErrorMessage('')
    try {
      await createFinanceRecurring({
        accountId: Number(form.accountId),
        categoryId: form.categoryId ? Number(form.categoryId) : null,
        description: form.description.trim(),
        amount: Number(form.amount),
        frequency: form.frequency,
        firstDueDate: form.firstDueDate,
        endDate: form.endDate || null,
      })
      setIsComplete(true)
      window.setTimeout(onCreated, 800)
    } catch (error) {
      setErrorMessage(error.message)
      setIsSaving(false)
    }
  }

  return (
    <div
      className={styles.modalBackdrop}
      onMouseDown={(event) => event.target === event.currentTarget && !isSaving && onClose()}
    >
      <section
        className={styles.financeModal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="recurring-title"
      >
        {isCategoryModalOpen &&
          createPortal(
            <CategorySubmodal
              transactionType="expense"
              onClose={() => setIsCategoryModalOpen(false)}
              onCreated={(newCategory) => {
                setLocalCategories((current) => [...current, newCategory])
                setForm((current) => ({ ...current, categoryId: newCategory.id.toString() }))
                setIsCategoryModalOpen(false)
              }}
            />,
            document.body,
          )}

        {isComplete ? (
          <div className={styles.modalSuccess}>
            <div>
              <AuthMascot />
            </div>
            <span>
              <Check size={17} />
            </span>
            <h2>O Bubu vai lembrar!</h2>
            <p>A próxima cobrança será lançada automaticamente no vencimento.</p>
          </div>
        ) : isReviewing ? (
          <>
            <button
              className={styles.modalClose}
              type="button"
              onClick={onClose}
              disabled={isSaving}
              aria-label="Fechar"
            >
              <X size={18} />
            </button>
            <div className={styles.recurringReview}>
              <div className={styles.reviewMascot}>
                <img src={bubuSpendingMoney} alt="Bubu organizando uma despesa recorrente" />
              </div>
              <small>CONFERE COM O BUBU</small>
              <h2 id="recurring-title">Essa conta vai se repetir</h2>
              <p>A cada vencimento, uma saída real será criada automaticamente.</p>
              <div className={styles.recurringReviewAmount}>
                <span>{recurrenceLabels[form.frequency]}</span>
                <strong>−{formatCurrency(Number(form.amount))}</strong>
              </div>
              <div className={styles.transactionReviewGrid}>
                <div>
                  <span>DESCRIÇÃO</span>
                  <strong>{form.description}</strong>
                </div>
                <div>
                  <span>CONTA</span>
                  <strong>{account?.name}</strong>
                </div>
                <div>
                  <span>CATEGORIA</span>
                  <strong>{category?.name ?? 'Sem categoria'}</strong>
                </div>
                <div>
                  <span>PRIMEIRO VENCIMENTO</span>
                  <strong>{fullDate.format(new Date(`${form.firstDueDate}T12:00:00`))}</strong>
                </div>
              </div>
            </div>
            {errorMessage && (
              <p className={styles.modalError} role="alert">
                {errorMessage}
              </p>
            )}
            <footer className={styles.reviewFooter}>
              <button type="button" onClick={() => setIsReviewing(false)} disabled={isSaving}>
                Voltar e revisar
              </button>
              <button type="button" onClick={save} disabled={isSaving}>
                {isSaving ? (
                  <LoaderCircle className={styles.spinner} size={17} />
                ) : (
                  <Check size={17} />
                )}
                {isSaving ? 'Criando...' : 'Confirmar recorrência'}
              </button>
            </footer>
          </>
        ) : (
          <form
            onSubmit={(event) => {
              event.preventDefault()
              setIsReviewing(true)
            }}
          >
            <button
              className={styles.modalClose}
              type="button"
              onClick={onClose}
              aria-label="Fechar"
            >
              <X size={18} />
            </button>
            <div className={styles.modalIntro}>
              <div>
                <img src={bubuSpendingMoney} alt="Bubu organizando dinheiro" />
              </div>
              <span>
                <Repeat2 size={17} />
              </span>
              <small>CONTA AUTOMÁTICA</small>
              <h2 id="recurring-title">Nova despesa recorrente</h2>
              <p>Aluguel, streaming ou academia: o Bubu lança para você no dia certo.</p>
            </div>
            <div className={styles.modalFields}>
              <label className={styles.fullField}>
                <span>Descrição</span>
                <input
                  name="description"
                  value={form.description}
                  onChange={handleChange}
                  placeholder="Ex.: Assinatura da academia"
                  minLength="2"
                  maxLength="200"
                  required
                />
              </label>
              <label>
                <span>Valor</span>
                <input
                  name="amount"
                  value={form.amount}
                  onChange={handleChange}
                  type="number"
                  min="0.01"
                  step="0.01"
                  placeholder="0,00"
                  required
                />
              </label>
              <label>
                <span>Conta</span>
                <select name="accountId" value={form.accountId} onChange={handleChange} required>
                  {accounts.map((item) => (
                    <option value={item.id} key={item.id}>
                      {item.name}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                <span>
                  Categoria{' '}
                  <button
                    className={styles.inlineAddButton}
                    type="button"
                    onClick={() => setIsCategoryModalOpen(true)}
                  >
                    + criar
                  </button>
                </span>
                <select name="categoryId" value={form.categoryId} onChange={handleChange}>
                  <option value="">Sem categoria</option>
                  {expenseCategories.map((item) => (
                    <option value={item.id} key={item.id}>
                      {item.name}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                <span>Frequência</span>
                <select name="frequency" value={form.frequency} onChange={handleChange}>
                  {Object.entries(recurrenceLabels).map(([value, label]) => (
                    <option value={value} key={value}>
                      {label}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                <span>Primeiro vencimento</span>
                <input
                  name="firstDueDate"
                  value={form.firstDueDate}
                  onChange={handleChange}
                  type="date"
                  min={today}
                  required
                />
              </label>
              <label>
                <span>
                  Data final <i>opcional</i>
                </span>
                <input
                  name="endDate"
                  value={form.endDate}
                  onChange={handleChange}
                  type="date"
                  min={form.firstDueDate}
                />
              </label>
            </div>
            {errorMessage && (
              <p className={styles.modalError} role="alert">
                {errorMessage}
              </p>
            )}
            <footer className={styles.formFooter}>
              <button type="button" onClick={onClose}>
                Agora não
              </button>
              <button type="submit">
                <Repeat2 size={17} /> Revisar recorrência
              </button>
            </footer>
          </form>
        )}
      </section>
    </div>
  )
}

function RecurringActionModal({ recurrence, action, onClose, onConfirmed }) {
  const [isSaving, setIsSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const isPause = action === 'pause'

  useEffect(() => {
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const handleEscape = (event) => event.key === 'Escape' && !isSaving && onClose()
    window.addEventListener('keydown', handleEscape)
    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', handleEscape)
    }
  }, [isSaving, onClose])

  async function confirm() {
    setIsSaving(true)
    setErrorMessage('')
    try {
      if (isPause) await pauseFinanceRecurring(recurrence.id)
      else await resumeFinanceRecurring(recurrence.id)
      onConfirmed()
    } catch (error) {
      setErrorMessage(error.message)
      setIsSaving(false)
    }
  }

  return (
    <div
      className={styles.modalBackdrop}
      onMouseDown={(event) => event.target === event.currentTarget && !isSaving && onClose()}
    >
      <section
        className={styles.recurringActionModal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="recurring-action-title"
      >
        <button
          className={styles.modalClose}
          type="button"
          onClick={onClose}
          disabled={isSaving}
          aria-label="Fechar"
        >
          <X size={18} />
        </button>
        <div>
          <AuthMascot />
        </div>
        <small>{isPause ? 'UMA PAUSINHA?' : 'DE VOLTA À ROTINA'}</small>
        <h2 id="recurring-action-title">
          {isPause ? `Pausar “${recurrence.description}”?` : `Retomar “${recurrence.description}”?`}
        </h2>
        <p>
          {isPause
            ? 'Nenhuma nova cobrança será lançada enquanto estiver pausada. O histórico continua guardado.'
            : 'O Bubu seguirá a agenda a partir do próximo vencimento, sem cobrar os períodos que ficaram pausados.'}
        </p>
        {errorMessage && (
          <p className={styles.modalError} role="alert">
            {errorMessage}
          </p>
        )}
        <footer>
          <button type="button" onClick={onClose} disabled={isSaving}>
            {isPause ? 'Manter ativa' : 'Deixar pausada'}
          </button>
          <button type="button" onClick={confirm} disabled={isSaving}>
            {isSaving ? (
              <LoaderCircle className={styles.spinner} size={17} />
            ) : isPause ? (
              <Pause size={16} />
            ) : (
              <Play size={16} />
            )}
            {isSaving ? 'Salvando...' : isPause ? 'Sim, pausar' : 'Sim, retomar'}
          </button>
        </footer>
      </section>
    </div>
  )
}

function FinanceActionModal({
  type,
  accounts,
  categories,
  selectedAccountId,
  account,
  onClose,
  onCreated,
}) {
  const defaultAccountId = selectedAccountId || accounts[0]?.id?.toString() || ''
  const [form, setForm] = useState({
    name: account?.name ?? '',
    accountType: account?.type ?? 'checking',
    initialBalance: '',
    transactionType: type === 'expense' || type === 'recurring-expense' ? 'expense' : 'income',
    isRecurring: type === 'recurring-expense',
    frequency: 'monthly',
    firstDueDate: today,
    endDate: '',
    accountId: defaultAccountId,
    categoryId: '',
    description: '',
    amount: '',
    date: today,
    notes: '',
    fromAccountId: defaultAccountId,
    toAccountId:
      accounts.find((account) => account.id.toString() !== defaultAccountId)?.id?.toString() ?? '',
  })
  const [isSaving, setIsSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const [isComplete, setIsComplete] = useState(false)
  const [isReviewingTransfer, setIsReviewingTransfer] = useState(false)
  const [isReviewingTransaction, setIsReviewingTransaction] = useState(false)
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false)
  const [localCategories, setLocalCategories] = useState(categories)
  const isAccount = type === 'account' || type === 'edit-account'
  const isEditingAccount = type === 'edit-account'
  const isTransfer = type === 'transfer'
  const transactionType = form.transactionType
  const isRecurring = transactionType === 'expense' && form.isRecurring
  const transactionMascotImage = transactionType === 'income' ? bubuSavingMoney : bubuSpendingMoney
  const availableCategories = localCategories.filter(
    (category) => category.type === transactionType,
  )
  const transactionAccount = accounts.find((item) => item.id.toString() === form.accountId)
  const selectedCategory = localCategories.find((item) => item.id.toString() === form.categoryId)
  const originAccount = accounts.find((item) => item.id.toString() === form.fromAccountId)
  const destinationAccount = accounts.find((item) => item.id.toString() === form.toAccountId)
  const title = isEditingAccount
    ? 'Editar conta'
    : isAccount
      ? 'Nova conta'
      : isTransfer
        ? 'Transferir dinheiro'
        : isRecurring
          ? 'Registrar uma saída recorrente'
          : transactionType === 'expense'
            ? 'Registrar uma saída'
            : 'Registrar uma entrada'
  const phrase = isEditingAccount
    ? 'Um nome novo, a mesma história bem guardada.'
    : isAccount
      ? 'Mais um cantinho para organizar seu dinheiro.'
      : isTransfer
        ? 'O dinheiro só vai trocar de lugar. O Bubu cuida do caminho.'
        : isRecurring
          ? 'Você escolhe a agenda e o Bubu dá baixa no saldo em cada vencimento.'
          : transactionType === 'expense'
            ? 'Sem julgamentos. Entender para onde foi já deixa tudo mais leve.'
            : 'Boa! Vamos guardar essa entrada no lugar certo.'

  useEffect(() => {
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const handleEscape = (event) =>
      event.key === 'Escape' && !isSaving && !isCategoryModalOpen && onClose()
    window.addEventListener('keydown', handleEscape)
    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', handleEscape)
    }
  }, [isCategoryModalOpen, isSaving, onClose])

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
    setErrorMessage('')
  }

  function handleFromAccountChange(event) {
    const fromAccountId = event.target.value
    const destinationIsValid = form.toAccountId && form.toAccountId !== fromAccountId
    setForm((current) => ({
      ...current,
      fromAccountId,
      toAccountId: destinationIsValid
        ? current.toAccountId
        : (accounts.find((account) => account.id.toString() !== fromAccountId)?.id?.toString() ??
          ''),
    }))
    setErrorMessage('')
  }

  async function saveRecord() {
    setIsSaving(true)
    setErrorMessage('')
    try {
      if (isEditingAccount)
        await updateFinanceAccount(account.id, {
          name: form.name.trim(),
          accountType: form.accountType,
        })
      else if (isAccount)
        await createFinanceAccount({
          name: form.name.trim(),
          accountType: form.accountType,
          initialBalance: Number(form.initialBalance || 0),
        })
      else if (isTransfer)
        await createFinanceTransfer({
          fromAccountId: Number(form.fromAccountId),
          toAccountId: Number(form.toAccountId),
          amount: Number(form.amount),
          transferDate: `${form.date}T12:00:00`,
          description: form.description.trim() || null,
        })
      else if (isRecurring)
        await createFinanceRecurring({
          accountId: Number(form.accountId),
          categoryId: form.categoryId ? Number(form.categoryId) : null,
          description: form.description.trim(),
          amount: Number(form.amount),
          frequency: form.frequency,
          firstDueDate: form.firstDueDate,
          endDate: form.endDate || null,
        })
      else
        await createFinanceTransaction({
          transactionType,
          accountId: Number(form.accountId),
          categoryId: form.categoryId ? Number(form.categoryId) : null,
          description: form.description.trim(),
          amount: Number(form.amount),
          transactionDate: form.date,
          notes: form.notes.trim() || null,
        })
      setIsComplete(true)
      window.setTimeout(onCreated, 700)
    } catch (error) {
      setErrorMessage(error.message)
      setIsSaving(false)
    }
  }

  async function handleSubmit(event) {
    event.preventDefault()

    if (isTransfer) {
      const amount = Number(form.amount)
      if (!originAccount || !destinationAccount) {
        setErrorMessage('Selecione as contas de origem e destino.')
        return
      }
      if (amount > originAccount.balance) {
        setErrorMessage(
          `Saldo insuficiente. Você tem ${formatCurrency(originAccount.balance)} disponível nessa conta.`,
        )
        return
      }
      setErrorMessage('')
      setIsReviewingTransfer(true)
      return
    }

    if (!isAccount) {
      setErrorMessage('')
      setIsReviewingTransaction(true)
      return
    }

    await saveRecord()
  }

  return (
    <div
      className={styles.modalBackdrop}
      onMouseDown={(event) => event.target === event.currentTarget && !isSaving && onClose()}
    >
      <section
        className={styles.financeModal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="finance-modal-title"
      >
        {isCategoryModalOpen &&
          createPortal(
            <CategorySubmodal
              transactionType={transactionType}
              onClose={() => setIsCategoryModalOpen(false)}
              onCreated={(category) => {
                setLocalCategories((current) => [...current, category])
                setForm((current) => ({ ...current, categoryId: category.id.toString() }))
                setIsCategoryModalOpen(false)
              }}
            />,
            document.body,
          )}
        {isComplete ? (
          <div className={styles.modalSuccess}>
            <div>
              <AuthMascot />
            </div>
            <span>
              <Check size={17} />
            </span>
            <h2>Tudo no lugar!</h2>
            <p>O Bubu já atualizou seus números.</p>
          </div>
        ) : isReviewingTransaction ? (
          <>
            <button
              className={styles.modalClose}
              type="button"
              onClick={onClose}
              disabled={isSaving}
              aria-label="Fechar"
            >
              <X size={18} />
            </button>
            <div className={styles.transactionReview}>
              <div className={styles.reviewMascot}>
                <img
                  src={transactionMascotImage}
                  alt={
                    transactionType === 'income'
                      ? 'Bubu guardando dinheiro'
                      : 'Bubu tirando dinheiro'
                  }
                />
              </div>
              <small>ÚLTIMA CONFERIDA</small>
              <h2 id="finance-modal-title">Podemos registrar?</h2>
              <p>
                {isRecurring
                  ? 'Essa saída será criada automaticamente em cada vencimento.'
                  : 'O Bubu separou os detalhes para você confirmar com calma.'}
              </p>
              <div
                className={`${styles.reviewKind} ${transactionType === 'income' ? styles.reviewIncome : styles.reviewExpense}`}
              >
                <span>
                  {transactionType === 'income' ? (
                    <ArrowDownLeft size={17} />
                  ) : (
                    <ArrowUpRight size={17} />
                  )}
                  {transactionType === 'income'
                    ? 'DINHEIRO ENTROU'
                    : isRecurring
                      ? `SAÍDA · ${recurrenceLabels[form.frequency].toUpperCase()}`
                      : 'DINHEIRO SAIU'}
                </span>
                <strong>
                  {transactionType === 'income' ? '+' : '−'}
                  {formatCurrency(Number(form.amount))}
                </strong>
              </div>
              <div className={styles.transactionReviewGrid}>
                <div>
                  <span>DESCRIÇÃO</span>
                  <strong>{form.description}</strong>
                </div>
                <div>
                  <span>CONTA</span>
                  <strong>{transactionAccount?.name}</strong>
                </div>
                <div>
                  <span>CATEGORIA</span>
                  <strong>{selectedCategory?.name ?? 'Sem categoria'}</strong>
                </div>
                <div>
                  <span>{isRecurring ? 'PRIMEIRO VENCIMENTO' : 'DATA'}</span>
                  <strong>
                    {fullDate.format(
                      new Date(`${isRecurring ? form.firstDueDate : form.date}T12:00:00`),
                    )}
                  </strong>
                </div>
              </div>
              {form.notes && <p className={styles.reviewDescription}>“{form.notes}”</p>}
            </div>
            {errorMessage && (
              <p className={styles.modalError} role="alert">
                {errorMessage}
              </p>
            )}
            <footer className={styles.reviewFooter}>
              <button
                type="button"
                onClick={() => {
                  setIsReviewingTransaction(false)
                  setErrorMessage('')
                }}
                disabled={isSaving}
              >
                Voltar e revisar
              </button>
              <button type="button" onClick={saveRecord} disabled={isSaving}>
                {isSaving ? (
                  <LoaderCircle className={styles.spinner} size={17} />
                ) : (
                  <Check size={17} />
                )}
                {isSaving ? 'Registrando...' : 'Sim, pode registrar'}
              </button>
            </footer>
          </>
        ) : isReviewingTransfer ? (
          <>
            <button
              className={styles.modalClose}
              type="button"
              onClick={onClose}
              disabled={isSaving}
              aria-label="Fechar"
            >
              <X size={18} />
            </button>
            <div className={styles.transferReview}>
              <div className={styles.reviewMascot}>
                <AuthMascot />
              </div>
              <small>CONFERE COM O BUBU</small>
              <h2 id="finance-modal-title">Está tudo certinho?</h2>
              <p>Uma última olhada antes do dinheiro trocar de lugar.</p>
              <div className={styles.transferRoute}>
                <div>
                  <span>SAI DE</span>
                  <strong>{originAccount?.name}</strong>
                  <small>Saldo: {formatCurrency(originAccount?.balance)}</small>
                </div>
                <i>
                  <ArrowRightLeft size={18} />
                </i>
                <div>
                  <span>VAI PARA</span>
                  <strong>{destinationAccount?.name}</strong>
                  <small>Saldo: {formatCurrency(destinationAccount?.balance)}</small>
                </div>
              </div>
              <div className={styles.reviewAmount}>
                <span>VALOR DA TRANSFERÊNCIA</span>
                <strong>{formatCurrency(Number(form.amount))}</strong>
                <small>
                  Depois, ficam{' '}
                  {formatCurrency((originAccount?.balance ?? 0) - Number(form.amount))} na conta de
                  origem.
                </small>
              </div>
              {form.description && <p className={styles.reviewDescription}>“{form.description}”</p>}
            </div>
            {errorMessage && (
              <p className={styles.modalError} role="alert">
                {errorMessage}
              </p>
            )}
            <footer className={styles.reviewFooter}>
              <button
                type="button"
                onClick={() => {
                  setIsReviewingTransfer(false)
                  setErrorMessage('')
                }}
                disabled={isSaving}
              >
                Voltar e revisar
              </button>
              <button type="button" onClick={saveRecord} disabled={isSaving}>
                {isSaving ? (
                  <LoaderCircle className={styles.spinner} size={17} />
                ) : (
                  <ArrowRightLeft size={17} />
                )}
                {isSaving ? 'Transferindo...' : 'Sim, pode transferir'}
              </button>
            </footer>
          </>
        ) : (
          <>
            <button
              className={styles.modalClose}
              type="button"
              onClick={onClose}
              disabled={isSaving}
              aria-label="Fechar"
            >
              <X size={18} />
            </button>
            <div className={styles.modalIntro}>
              <div>
                {!isAccount && !isTransfer ? (
                  <img
                    className={styles.transactionMascotImage}
                    src={transactionMascotImage}
                    alt={
                      transactionType === 'income'
                        ? 'Bubu guardando dinheiro'
                        : 'Bubu tirando dinheiro'
                    }
                  />
                ) : (
                  <AuthMascot />
                )}
              </div>
              <span>
                <small>BUBU ORGANIZA COM VOCÊ</small>
                <h2 id="finance-modal-title">{title}</h2>
                <p>{phrase}</p>
              </span>
            </div>
            <form onSubmit={handleSubmit}>
              <div className={styles.modalFields}>
                {isAccount ? (
                  <>
                    <label>
                      <span>Nome da conta</span>
                      <input
                        name="name"
                        value={form.name}
                        onChange={handleChange}
                        minLength={2}
                        maxLength={100}
                        placeholder="Ex.: Banco principal"
                        autoFocus
                        required
                      />
                    </label>
                    <label>
                      <span>Tipo</span>
                      <select name="accountType" value={form.accountType} onChange={handleChange}>
                        {Object.entries(accountTypeLabels).map(([value, label]) => (
                          <option value={value} key={value}>
                            {label}
                          </option>
                        ))}
                      </select>
                    </label>
                    {!isEditingAccount && (
                      <label className={styles.fullField}>
                        <span>Saldo atual</span>
                        <div className={styles.moneyInput}>
                          <small>R$</small>
                          <input
                            name="initialBalance"
                            type="number"
                            min="0"
                            step="0.01"
                            value={form.initialBalance}
                            onChange={handleChange}
                            placeholder="0,00"
                          />
                        </div>
                      </label>
                    )}
                    {isEditingAccount && (
                      <p className={`${styles.fullField} ${styles.balanceHint}`}>
                        Para manter seu histórico correto, o saldo é alterado por uma entrada, saída
                        ou transferência.
                      </p>
                    )}
                  </>
                ) : isTransfer ? (
                  <>
                    <label>
                      <span>Conta de origem</span>
                      <select
                        name="fromAccountId"
                        value={form.fromAccountId}
                        onChange={handleFromAccountChange}
                        required
                      >
                        <option value="">Selecione</option>
                        {accounts.map((account) => (
                          <option value={account.id} key={account.id}>
                            {account.name} · {formatCurrency(account.balance)}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label>
                      <span>Conta de destino</span>
                      <select
                        name="toAccountId"
                        value={form.toAccountId}
                        onChange={handleChange}
                        required
                      >
                        <option value="">Selecione</option>
                        {accounts
                          .filter((account) => account.id.toString() !== form.fromAccountId)
                          .map((account) => (
                            <option value={account.id} key={account.id}>
                              {account.name}
                            </option>
                          ))}
                      </select>
                    </label>
                    <label>
                      <span>Valor</span>
                      <div className={styles.moneyInput}>
                        <small>R$</small>
                        <input
                          name="amount"
                          type="number"
                          min="0.01"
                          step="0.01"
                          value={form.amount}
                          onChange={handleChange}
                          autoFocus
                          required
                        />
                      </div>
                    </label>
                    <label>
                      <span>Data</span>
                      <input
                        name="date"
                        type="date"
                        value={form.date}
                        onChange={handleChange}
                        required
                      />
                    </label>
                    <label className={styles.fullField}>
                      <span>
                        Descrição <i>opcional</i>
                      </span>
                      <input
                        name="description"
                        value={form.description}
                        onChange={handleChange}
                        maxLength={200}
                        placeholder="Ex.: Guardar na poupança"
                      />
                    </label>
                  </>
                ) : (
                  <>
                    <label className={styles.fullField}>
                      <span>O que aconteceu?</span>
                      <div className={styles.typeSwitch}>
                        <button
                          className={
                            form.transactionType === 'income' ? styles.activeIncome : undefined
                          }
                          type="button"
                          onClick={() =>
                            setForm((current) => ({
                              ...current,
                              transactionType: 'income',
                              categoryId: '',
                              isRecurring: false,
                            }))
                          }
                        >
                          <ArrowDownLeft size={16} /> Dinheiro entrou
                        </button>
                        <button
                          className={
                            form.transactionType === 'expense' ? styles.activeExpense : undefined
                          }
                          type="button"
                          onClick={() =>
                            setForm((current) => ({
                              ...current,
                              transactionType: 'expense',
                              categoryId: '',
                            }))
                          }
                        >
                          <ArrowUpRight size={16} /> Dinheiro saiu
                        </button>
                      </div>
                    </label>
                    {transactionType === 'expense' && (
                      <label className={styles.fullField}>
                        <span>Essa saída vai acontecer novamente?</span>
                        <div className={styles.recurrenceSwitch}>
                          <button
                            className={
                              !form.isRecurring ? styles.activeRecurrenceOption : undefined
                            }
                            type="button"
                            onClick={() =>
                              setForm((current) => ({ ...current, isRecurring: false }))
                            }
                          >
                            <CreditCard size={15} /> Apenas uma vez
                          </button>
                          <button
                            className={form.isRecurring ? styles.activeRecurrenceOption : undefined}
                            type="button"
                            onClick={() =>
                              setForm((current) => ({ ...current, isRecurring: true }))
                            }
                          >
                            <Repeat2 size={15} /> Recorrente
                          </button>
                        </div>
                      </label>
                    )}
                    <label className={`${styles.fullField} ${styles.transactionAmountField}`}>
                      <span>Qual foi o valor?</span>
                      <div className={styles.transactionAmountInput}>
                        <small>R$</small>
                        <input
                          name="amount"
                          type="number"
                          min="0.01"
                          step="0.01"
                          value={form.amount}
                          onChange={handleChange}
                          placeholder="0,00"
                          autoFocus
                          required
                        />
                      </div>
                    </label>
                    <label className={styles.fullField}>
                      <span>Descrição</span>
                      <input
                        name="description"
                        value={form.description}
                        onChange={handleChange}
                        minLength={2}
                        maxLength={200}
                        placeholder={
                          transactionType === 'income'
                            ? 'Ex.: Salário de agosto'
                            : 'Ex.: Compras do mercado'
                        }
                        required
                      />
                    </label>
                    <label>
                      <span>Conta</span>
                      <select
                        name="accountId"
                        value={form.accountId}
                        onChange={handleChange}
                        required
                      >
                        <option value="">Selecione</option>
                        {accounts.map((account) => (
                          <option value={account.id} key={account.id}>
                            {account.name}
                          </option>
                        ))}
                      </select>
                      {transactionAccount && (
                        <small className={styles.accountBalanceHint}>
                          Saldo atual: {formatCurrency(transactionAccount.balance)}
                        </small>
                      )}
                    </label>
                    <label>
                      <span>{isRecurring ? 'Primeiro vencimento' : 'Data da movimentação'}</span>
                      <input
                        name={isRecurring ? 'firstDueDate' : 'date'}
                        type="date"
                        value={isRecurring ? form.firstDueDate : form.date}
                        onChange={handleChange}
                        min={isRecurring ? today : undefined}
                        required
                      />
                    </label>
                    {isRecurring && (
                      <>
                        <label>
                          <span>Frequência</span>
                          <select name="frequency" value={form.frequency} onChange={handleChange}>
                            {Object.entries(recurrenceLabels).map(([value, label]) => (
                              <option value={value} key={value}>
                                {label}
                              </option>
                            ))}
                          </select>
                        </label>
                        <label>
                          <span>
                            Data final <i>opcional</i>
                          </span>
                          <input
                            name="endDate"
                            type="date"
                            value={form.endDate}
                            onChange={handleChange}
                            min={form.firstDueDate}
                          />
                        </label>
                      </>
                    )}
                    <label>
                      <span className={styles.categoryLabel}>
                        Categoria
                        <button type="button" onClick={() => setIsCategoryModalOpen(true)}>
                          <Plus size={13} /> Nova categoria
                        </button>
                      </span>
                      <select name="categoryId" value={form.categoryId} onChange={handleChange}>
                        <option value="">Sem categoria</option>
                        {availableCategories.map((category) => (
                          <option value={category.id} key={category.id}>
                            {category.name}
                          </option>
                        ))}
                      </select>
                    </label>
                    {!isRecurring && (
                      <label className={styles.fullField}>
                        <span>
                          Observação <i>opcional</i>
                        </span>
                        <textarea
                          name="notes"
                          value={form.notes}
                          onChange={handleChange}
                          rows={2}
                          maxLength={1000}
                        />
                      </label>
                    )}
                  </>
                )}
              </div>
              {errorMessage && (
                <p className={styles.modalError} role="alert">
                  {errorMessage}
                </p>
              )}
              <footer>
                <button type="button" onClick={onClose} disabled={isSaving}>
                  Agora não
                </button>
                <button type="submit" disabled={isSaving}>
                  {isSaving ? (
                    <LoaderCircle className={styles.spinner} size={17} />
                  ) : (
                    <Check size={17} />
                  )}
                  {isSaving
                    ? 'Salvando...'
                    : isTransfer
                      ? 'Confirmar transferência'
                      : isEditingAccount
                        ? 'Salvar alterações'
                        : isAccount
                          ? 'Criar conta'
                          : 'Salvar movimentação'}
                </button>
              </footer>
            </form>
          </>
        )}
      </section>
    </div>
  )
}

function CategorySubmodal({ transactionType, onClose, onCreated }) {
  const [name, setName] = useState('')
  const [color, setColor] = useState(categoryColors[0])
  const [isSaving, setIsSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const handleEscape = (event) => event.key === 'Escape' && !isSaving && onClose()
    window.addEventListener('keydown', handleEscape)
    return () => window.removeEventListener('keydown', handleEscape)
  }, [isSaving, onClose])

  async function handleSubmit(event) {
    event.preventDefault()
    setIsSaving(true)
    setErrorMessage('')
    try {
      const category = await createFinanceCategory({
        name: name.trim(),
        categoryType: transactionType,
        color,
      })
      onCreated(category)
    } catch (error) {
      setErrorMessage(error.message)
      setIsSaving(false)
    }
  }

  return (
    <div
      className={styles.submodalBackdrop}
      onMouseDown={(event) => event.target === event.currentTarget && !isSaving && onClose()}
    >
      <form
        className={styles.categorySubmodal}
        onSubmit={handleSubmit}
        role="dialog"
        aria-modal="true"
        aria-labelledby="category-modal-title"
      >
        <button
          className={styles.modalClose}
          type="button"
          onClick={onClose}
          disabled={isSaving}
          aria-label="Fechar"
        >
          <X size={17} />
        </button>
        <div className={styles.categoryMascot}>
          <AuthMascot />
        </div>
        <small>BUBU ABRIU UMA GAVETINHA NOVA</small>
        <h2 id="category-modal-title">
          Nova categoria de {transactionType === 'income' ? 'entrada' : 'saída'}
        </h2>
        <p>Dê um nome fácil de reconhecer. Assim seus próximos registros ficam bem mais rápidos.</p>
        <label>
          <span>Nome da categoria</span>
          <input
            value={name}
            onChange={(event) => {
              setName(event.target.value)
              setErrorMessage('')
            }}
            minLength={2}
            maxLength={80}
            placeholder={transactionType === 'income' ? 'Ex.: Freelance' : 'Ex.: Alimentação'}
            autoFocus
            required
          />
        </label>
        <fieldset>
          <legend>Escolha uma cor</legend>
          <div>
            {categoryColors.map((item) => (
              <button
                className={color === item ? styles.selectedColor : undefined}
                type="button"
                onClick={() => setColor(item)}
                aria-label={`Usar a cor ${item}`}
                key={item}
              >
                <i style={{ background: item }} />
                {color === item && <Check size={13} />}
              </button>
            ))}
          </div>
        </fieldset>
        {errorMessage && (
          <p className={styles.submodalError} role="alert">
            {errorMessage}
          </p>
        )}
        <footer>
          <button type="button" onClick={onClose} disabled={isSaving}>
            Cancelar
          </button>
          <button type="submit" disabled={isSaving}>
            {isSaving ? <LoaderCircle className={styles.spinner} size={16} /> : <Plus size={16} />}
            {isSaving ? 'Criando...' : 'Criar categoria'}
          </button>
        </footer>
      </form>
    </div>
  )
}

function DeleteAccountModal({ account, onClose, onDeleted }) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const handleEscape = (event) => event.key === 'Escape' && !isDeleting && onClose()
    window.addEventListener('keydown', handleEscape)
    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', handleEscape)
    }
  }, [isDeleting, onClose])

  async function handleDelete() {
    setIsDeleting(true)
    setErrorMessage('')
    try {
      await deleteFinanceAccount(account.id)
      onDeleted()
    } catch (error) {
      setErrorMessage(error.message)
      setIsDeleting(false)
    }
  }

  return (
    <div
      className={styles.modalBackdrop}
      onMouseDown={(event) => event.target === event.currentTarget && !isDeleting && onClose()}
    >
      <section
        className={styles.deleteModal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="delete-account-title"
      >
        <button
          className={styles.modalClose}
          type="button"
          onClick={onClose}
          disabled={isDeleting}
          aria-label="Fechar"
        >
          <X size={18} />
        </button>
        <div className={styles.deleteMascot}>
          <AuthMascot />
        </div>
        <small>BUBU QUER TER CERTEZA</small>
        <h2 id="delete-account-title">Vamos tirar essa conta daqui?</h2>
        <p>
          A conta <strong>“{account.name}”</strong> ainda não possui movimentações e será excluída
          definitivamente.
        </p>
        <div className={styles.deleteBalance}>
          <span>Saldo atual</span>
          <strong>{formatCurrency(account.balance)}</strong>
        </div>
        {errorMessage && (
          <p className={styles.modalError} role="alert">
            {errorMessage}
          </p>
        )}
        <footer>
          <button type="button" onClick={onClose} disabled={isDeleting}>
            Manter conta
          </button>
          <button type="button" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting ? (
              <LoaderCircle className={styles.spinner} size={17} />
            ) : (
              <Trash2 size={17} />
            )}
            {isDeleting ? 'Verificando...' : 'Excluir conta'}
          </button>
        </footer>
      </section>
    </div>
  )
}

function FinanceSkeleton() {
  return (
    <div className={styles.skeleton}>
      <i />
      <div>
        {[1, 2, 3, 4].map((item) => (
          <i key={item} />
        ))}
      </div>
      <section>
        <i />
        <i />
      </section>
    </div>
  )
}
