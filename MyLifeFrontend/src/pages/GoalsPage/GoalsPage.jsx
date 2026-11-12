import { useEffect, useMemo, useState } from 'react'
import { createPortal } from 'react-dom'
import {
  ArrowDownRight,
  ArrowRight,
  ArrowUpRight,
  CalendarDays,
  Check,
  LoaderCircle,
  PiggyBank,
  Plus,
  Sparkles,
  Target,
  Trash2,
  WalletCards,
  X,
} from 'lucide-react'
import bubuSaving from '../../assets/bubu-guardando-dinheiro.png'
import { BubuPageHero } from '../../components/layout/BubuPageHero'
import { DataPageSkeleton, ErrorNotice } from '../../components/layout/DataStates'
import { addGoalContribution, createGoal, deleteGoal, getGoalAccountOptions, getGoals } from '../../services/goalsApi'
import styles from './GoalsPage.module.css'

const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const date = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' })

const formatCurrency = (value) => currency.format(value ?? 0)
const formatDate = (value) => value ? date.format(new Date(`${value}T12:00:00`)).replace('.', '') : 'Sem data definida'

export function GoalsPage() {
  const [goals, setGoals] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [reloadKey, setReloadKey] = useState(0)
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [selectedGoal, setSelectedGoal] = useState(null)
  const [accounts, setAccounts] = useState([])

  useEffect(() => {
    const controller = new AbortController()
    Promise.all([getGoals(controller.signal), getGoalAccountOptions(controller.signal)])
      .then(([goalsData, accountsData]) => {
        setGoals(goalsData)
        setAccounts(accountsData)
      })
      .catch((error) => {
        if (error.name !== 'AbortError') setErrorMessage(error.message)
      })
      .finally(() => setIsLoading(false))
    return () => controller.abort()
  }, [reloadKey])

  const summary = useMemo(() => ({
    saved: goals.reduce((total, goal) => total + goal.currentValue, 0),
    target: goals.reduce((total, goal) => total + goal.targetValue, 0),
    active: goals.filter((goal) => goal.status === 'active').length,
    completed: goals.filter((goal) => goal.status === 'completed').length,
  }), [goals])

  const refresh = () => {
    setIsLoading(true)
    setErrorMessage('')
    setReloadKey((current) => current + 1)
  }

  const handleContributionSaved = (updatedGoal) => {
    setGoals((current) => current.map((goal) => goal.id === updatedGoal.id ? updatedGoal : goal))
    setSelectedGoal(null)
  }

  const handleDelete = async (goal) => {
    if (!window.confirm(`Excluir o objetivo “${goal.title}” e todo o histórico dele?`)) return
    try {
      await deleteGoal(goal.id)
      setGoals((current) => current.filter((item) => item.id !== goal.id))
    } catch (error) {
      setErrorMessage(error.message)
    }
  }

  return (
    <div className={styles.page}>
      <BubuPageHero
        eyebrow="SEUS PLANOS GANHAM FORMA"
        title="Objetivos que você consegue"
        highlight="ver crescendo."
        description="Crie um cofre para cada plano. O Bubu acompanha cada passo e mostra o quanto seus sonhos já avançaram."
        image={bubuSaving}
        badge={<><PiggyBank size={17} /> Um pouco por vez!</>}
        stats={[
          { icon: PiggyBank, label: 'Guardado nos cofres', value: formatCurrency(summary.saved), tone: 'lime' },
          { icon: Target, label: 'Soma das metas', value: formatCurrency(summary.target) },
          { icon: Sparkles, label: 'Em andamento', value: summary.active, tone: 'purple' },
          { icon: Check, label: 'Concluídos', value: summary.completed, tone: 'warm' },
        ]}
      >
        <button type="button" onClick={() => setIsCreateOpen(true)}><Plus size={18} /> Criar novo objetivo</button>
      </BubuPageHero>

      <section className={styles.sectionHeader}>
        <div><span>SEUS COFRES</span><h2>Planos em andamento</h2></div>
        {goals.length > 0 && <button type="button" onClick={() => setIsCreateOpen(true)}><Plus size={16} /> Novo objetivo</button>}
      </section>

      {errorMessage && <ErrorNotice message={errorMessage} onRetry={refresh} title="Não conseguimos carregar seus objetivos." />}

      {isLoading ? (
        <DataPageSkeleton cards={0} panels={2} showChart={false} />
      ) : goals.length === 0 ? (
        <div className={styles.empty}>
          <span><PiggyBank size={30} /></span>
          <h2>Seu primeiro cofre começa aqui</h2>
          <p>Pode ser uma viagem, um curso, um celular novo ou qualquer plano importante para você.</p>
          <button type="button" onClick={() => setIsCreateOpen(true)}>Criar meu primeiro objetivo <ArrowRight size={17} /></button>
        </div>
      ) : (
        <div className={styles.goalGrid}>
          {goals.map((goal, index) => (
            <GoalCard goal={goal} index={index} key={goal.id} onMove={() => setSelectedGoal(goal)} onDelete={() => handleDelete(goal)} />
          ))}
        </div>
      )}

      {isCreateOpen && <CreateGoalModal onClose={() => setIsCreateOpen(false)} onCreated={(goal) => { setGoals((current) => [goal, ...current]); setIsCreateOpen(false) }} />}
      {selectedGoal && <ContributionModal goal={selectedGoal} accounts={accounts} onClose={() => setSelectedGoal(null)} onSaved={(updatedGoal) => { handleContributionSaved(updatedGoal); getGoalAccountOptions().then(setAccounts).catch(() => {}) }} />}
    </div>
  )
}

function GoalCard({ goal, index, onMove, onDelete }) {
  const remaining = Math.max(0, goal.targetValue - goal.currentValue)
  const isCompleted = goal.status === 'completed'
  return (
    <article className={`${styles.goalCard} ${styles[`cardTone${index % 3}`]} ${isCompleted ? styles.completed : ''}`}>
      <header>
        <span>{isCompleted ? <Check size={20} /> : <Target size={20} />}</span>
        <div><small>{isCompleted ? 'OBJETIVO CONCLUÍDO' : 'COFRE ATIVO'}</small><h3>{goal.title}</h3></div>
        <button type="button" onClick={onDelete} aria-label={`Excluir ${goal.title}`}><Trash2 size={16} /></button>
      </header>
      {goal.description && <p className={styles.description}>{goal.description}</p>}
      <div className={styles.amounts}><div><small>Já guardado</small><strong>{formatCurrency(goal.currentValue)}</strong></div><div><small>Meta</small><b>{formatCurrency(goal.targetValue)}</b></div></div>
      <div className={styles.progressHeader}><span>{goal.progressPercentage}% alcançado</span><span>{isCompleted ? 'Você conseguiu!' : `Faltam ${formatCurrency(remaining)}`}</span></div>
      <div className={styles.progress}><i style={{ width: `${goal.progressPercentage}%` }} /></div>
      <div className={styles.goalMeta}><span><CalendarDays size={15} /> {formatDate(goal.targetDate)}</span><span>{goal.recentContributions.length} movimentações recentes</span></div>
      {goal.recentContributions[0] && <div className={styles.lastMove}>{goal.recentContributions[0].contributionType === 'deposit' ? <ArrowUpRight size={16} /> : <ArrowDownRight size={16} />}<span>Último registro<strong>{goal.recentContributions[0].contributionType === 'deposit' ? '+' : '−'} {formatCurrency(goal.recentContributions[0].amount)}</strong></span></div>}
      <button className={styles.moveButton} type="button" onClick={onMove}><PiggyBank size={17} /> {isCompleted ? 'Movimentar cofre' : 'Guardar ou retirar valor'}</button>
    </article>
  )
}

function ModalShell({ children, onClose, title, eyebrow, icon: Icon, busy }) {
  useEffect(() => {
    const previous = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const closeOnEscape = (event) => event.key === 'Escape' && !busy && onClose()
    window.addEventListener('keydown', closeOnEscape)
    return () => { document.body.style.overflow = previous; window.removeEventListener('keydown', closeOnEscape) }
  }, [busy, onClose])

  return createPortal(<div className={styles.backdrop} onMouseDown={(event) => event.target === event.currentTarget && !busy && onClose()}><section className={styles.modal} role="dialog" aria-modal="true"><header><span><Icon size={19} /></span><div><small>{eyebrow}</small><h2>{title}</h2></div><button type="button" onClick={onClose} disabled={busy} aria-label="Fechar"><X size={19} /></button></header>{children}</section></div>, document.body)
}

function CreateGoalModal({ onClose, onCreated }) {
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState('')
  const handleSubmit = async (event) => {
    event.preventDefault(); setIsSaving(true); setError('')
    const data = new FormData(event.currentTarget)
    try {
      const goal = await createGoal({ title: String(data.get('title')).trim(), description: String(data.get('description')).trim() || null, targetValue: Number(data.get('targetValue')), targetDate: data.get('targetDate') || null })
      onCreated(goal)
    } catch (requestError) { setError(requestError.message); setIsSaving(false) }
  }
  return <ModalShell onClose={onClose} busy={isSaving} icon={Target} eyebrow="NOVO COFRE" title="Qual plano vamos realizar?"><form onSubmit={handleSubmit} className={styles.modalForm}><label><span>Nome do objetivo</span><input name="title" minLength={2} maxLength={150} placeholder="Ex.: Viagem para o Japão" autoFocus required /></label><label><span>Quanto você precisa?</span><div className={styles.moneyInput}><small>R$</small><input name="targetValue" type="number" min="0.01" step="0.01" placeholder="10.000,00" required /></div></label><label><span>Data desejada <i>opcional</i></span><input name="targetDate" type="date" min={new Date().toISOString().slice(0, 10)} /></label><label><span>Por que isso é importante? <i>opcional</i></span><textarea name="description" rows={3} maxLength={1000} placeholder="Uma lembrança para manter sua motivação..." /></label>{error && <p className={styles.modalError}>{error}</p>}<footer><button type="button" onClick={onClose} disabled={isSaving}>Cancelar</button><button type="submit" disabled={isSaving}>{isSaving ? <LoaderCircle className={styles.spin} size={17} /> : <Plus size={17} />} Criar objetivo</button></footer></form></ModalShell>
}

function ContributionModal({ goal, accounts, onClose, onSaved }) {
  const [type, setType] = useState('deposit')
  const [accountId, setAccountId] = useState(accounts[0]?.id?.toString() ?? '')
  const [useAccount, setUseAccount] = useState(accounts.length > 0)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState('')
  const handleSubmit = async (event) => {
    event.preventDefault(); setIsSaving(true); setError('')
    const data = new FormData(event.currentTarget)
    try {
      const updated = await addGoalContribution(goal.id, { contributionType: type, accountId: useAccount && accountId ? Number(accountId) : null, amount: Number(data.get('amount')), note: String(data.get('note')).trim() || null })
      onSaved(updated)
    } catch (requestError) { setError(requestError.message); setIsSaving(false) }
  }
  const selectedAccount = accounts.find((account) => account.id.toString() === accountId)
  return <ModalShell onClose={onClose} busy={isSaving} icon={PiggyBank} eyebrow="MOVIMENTAR COFRE" title={goal.title}><form onSubmit={handleSubmit} className={styles.modalForm}><div className={styles.typeTabs}><button className={type === 'deposit' ? styles.selectedType : ''} type="button" onClick={() => setType('deposit')}><ArrowUpRight size={17} /> Guardar</button><button className={type === 'withdrawal' ? styles.selectedType : ''} type="button" onClick={() => setType('withdrawal')}><ArrowDownRight size={17} /> Retirar</button></div><div className={styles.balanceLine}><span>Saldo atual do cofre</span><strong>{formatCurrency(goal.currentValue)}</strong></div><div className={styles.originChoice}><button className={useAccount ? styles.selectedOrigin : ''} type="button" onClick={() => setUseAccount(true)} disabled={accounts.length === 0}><WalletCards size={17} /><span>{type === 'deposit' ? 'Usar uma conta' : 'Devolver para conta'}<small>Atualiza o saldo em Finanças</small></span></button><button className={!useAccount ? styles.selectedOrigin : ''} type="button" onClick={() => setUseAccount(false)}><Sparkles size={17} /><span>Movimento externo<small>Altera somente o cofre</small></span></button></div>{useAccount && <label><span>{type === 'deposit' ? 'Conta de origem' : 'Conta de destino'}</span><select value={accountId} onChange={(event) => setAccountId(event.target.value)} required>{accounts.map((account) => <option value={account.id} key={account.id}>{account.name} · {formatCurrency(account.balance)}</option>)}</select>{selectedAccount && type === 'deposit' && <small className={styles.availableBalance}>Disponível: {formatCurrency(selectedAccount.balance)}</small>}</label>}<label><span>Valor para {type === 'deposit' ? 'guardar' : 'retirar'}</span><div className={styles.moneyInput}><small>R$</small><input name="amount" type="number" min="0.01" max={type === 'withdrawal' ? goal.currentValue : useAccount ? selectedAccount?.balance : undefined} step="0.01" autoFocus required /></div></label><label><span>Observação <i>opcional</i></span><input name="note" maxLength={300} placeholder={type === 'deposit' ? 'Ex.: Economia deste mês' : 'Ex.: Imprevisto'} /></label><p className={styles.explanation}><WalletCards size={16} /> {useAccount ? type === 'deposit' ? 'O valor sairá da conta e aparecerá em Finanças como transferência para o objetivo.' : 'O valor voltará para a conta e aparecerá em Finanças como transferência do objetivo.' : 'Este registro altera somente o cofre e não interfere nas suas contas.'}</p>{error && <p className={styles.modalError}>{error}</p>}<footer><button type="button" onClick={onClose} disabled={isSaving}>Cancelar</button><button type="submit" disabled={isSaving || (useAccount && !accountId)}>{isSaving ? <LoaderCircle className={styles.spin} size={17} /> : type === 'deposit' ? <ArrowUpRight size={17} /> : <ArrowDownRight size={17} />} {type === 'deposit' ? 'Guardar valor' : 'Retirar valor'}</button></footer></form></ModalShell>
}
