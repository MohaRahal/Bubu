import { useEffect, useMemo, useState } from 'react'
import {
  Activity,
  ArrowDownRight,
  ArrowLeft,
  ArrowUpRight,
  Check,
  Goal,
  LoaderCircle,
  Plus,
  Sparkles,
  X,
} from 'lucide-react'
import { createMyDayActivity } from '../../services/myDayApi'
import {
  createQuickGoal,
  createQuickTransaction,
  getQuickRegisterOptions,
} from '../../services/quickRegisterApi'
import styles from './QuickRegisterModal.module.css'

const timezoneOffset = new Date().getTimezoneOffset() * 60_000
const today = new Date(Date.now() - timezoneOffset).toISOString().slice(0, 10)

const registerTypes = [
  { key: 'activity', title: 'Atividade', description: 'Planeje algo para o seu dia', icon: Activity, tone: 'green' },
  { key: 'goal', title: 'Objetivo', description: 'Defina algo que quer alcançar', icon: Goal, tone: 'purple' },
  { key: 'income', title: 'Receita', description: 'Registre um dinheiro que entrou', icon: ArrowUpRight, tone: 'lime' },
  { key: 'expense', title: 'Despesa', description: 'Registre um gasto realizado', icon: ArrowDownRight, tone: 'red' },
]

const initialForm = {
  title: '',
  description: '',
  date: today,
  time: '',
  duration: '30',
  activityCategoryId: '',
  goalType: 'quantity',
  targetValue: '',
  unit: '',
  targetDate: '',
  amount: '',
  accountId: '',
  financialCategoryId: '',
  notes: '',
}

export function QuickRegisterModal({ onClose, onRegistered }) {
  const [selectedType, setSelectedType] = useState(null)
  const [options, setOptions] = useState({ accounts: [], financialCategories: [], activityCategories: [] })
  const [form, setForm] = useState(initialForm)
  const [isLoadingOptions, setIsLoadingOptions] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const [isComplete, setIsComplete] = useState(false)

  useEffect(() => {
    const controller = new AbortController()
    getQuickRegisterOptions(controller.signal)
      .then((data) => {
        setOptions(data)
        setForm((current) => ({ ...current, accountId: data.accounts[0]?.id?.toString() ?? '' }))
      })
      .catch((error) => {
        if (error.name !== 'AbortError') setErrorMessage(error.message)
      })
      .finally(() => setIsLoadingOptions(false))
    return () => controller.abort()
  }, [])

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

  const selectedConfig = registerTypes.find((type) => type.key === selectedType)
  const transactionType = selectedType === 'income' ? 'income' : 'expense'
  const availableFinancialCategories = useMemo(() => options.financialCategories.filter(
    (category) => category.type === transactionType,
  ), [options.financialCategories, transactionType])

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
    setErrorMessage('')
  }

  function chooseType(type) {
    setSelectedType(type)
    setErrorMessage('')
    if (type === 'income' || type === 'expense') {
      const category = options.financialCategories.find((item) => item.type === type)
      setForm((current) => ({ ...current, financialCategoryId: category?.id?.toString() ?? '' }))
    }
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setIsSaving(true)
    setErrorMessage('')

    try {
      if (selectedType === 'activity') {
        await createMyDayActivity({
          title: form.title.trim(),
          description: form.description.trim() || null,
          activityDate: form.date,
          startTime: form.time ? `${form.time}:00` : null,
          durationMinutes: form.duration ? Number(form.duration) : null,
          categoryId: form.activityCategoryId ? Number(form.activityCategoryId) : null,
        })
      } else if (selectedType === 'goal') {
        await createQuickGoal({
          title: form.title.trim(),
          description: form.description.trim() || null,
          goalType: form.goalType,
          targetValue: form.goalType === 'boolean' ? 1 : Number(form.targetValue),
          unit: form.unit.trim() || null,
          startDate: form.date,
          targetDate: form.targetDate || null,
        })
      } else {
        if (!form.accountId) throw new Error('Crie ou selecione uma conta antes de registrar a movimentação.')
        await createQuickTransaction({
          transactionType,
          accountId: Number(form.accountId),
          categoryId: form.financialCategoryId ? Number(form.financialCategoryId) : null,
          description: form.title.trim(),
          amount: Number(form.amount),
          transactionDate: form.date,
          notes: form.notes.trim() || null,
        })
      }

      setIsComplete(true)
      window.setTimeout(onRegistered, 850)
    } catch (error) {
      setErrorMessage(error.message)
      setIsSaving(false)
    }
  }

  return (
    <div className={styles.backdrop} onMouseDown={(event) => event.target === event.currentTarget && !isSaving && onClose()}>
      <section className={styles.modal} role="dialog" aria-modal="true" aria-labelledby="register-title">
        {isComplete ? (
          <div className={styles.successState}>
            <span><Check size={28} /></span>
            <small>TUDO CERTO</small>
            <h2>Registro salvo!</h2>
            <p>O Bubu já colocou essa informação no lugar certo.</p>
          </div>
        ) : (
          <>
            <header>
              <div>
                {selectedType && <button type="button" aria-label="Voltar" onClick={() => setSelectedType(null)} disabled={isSaving}><ArrowLeft size={18} /></button>}
                <span><Sparkles size={18} /></span>
                <div><small>REGISTRO RÁPIDO</small><h2 id="register-title">{selectedConfig ? selectedConfig.title : 'O que aconteceu?'}</h2></div>
              </div>
              <button type="button" aria-label="Fechar" onClick={onClose} disabled={isSaving}><X size={19} /></button>
            </header>

            {!selectedType ? (
              <div className={styles.typeSelection}>
                <p>Escolha o tipo de informação que deseja registrar.</p>
                <div>
                  {registerTypes.map(({ key, title, description, icon: Icon, tone }) => (
                    <button type="button" onClick={() => chooseType(key)} className={styles[tone]} key={key}>
                      <span><Icon size={21} /></span><div><strong>{title}</strong><small>{description}</small></div><Plus size={16} />
                    </button>
                  ))}
                </div>
              </div>
            ) : (
              <form onSubmit={handleSubmit}>
                {isLoadingOptions ? <div className={styles.loadingState}><LoaderCircle size={22} /> Preparando opções...</div> : (
                  <div className={styles.fields}>
                    <label className={styles.full}><span>{selectedType === 'activity' ? 'Atividade' : selectedType === 'goal' ? 'Objetivo' : 'Descrição'}</span><input name="title" value={form.title} onChange={handleChange} minLength={2} maxLength={selectedType === 'goal' ? 150 : 200} placeholder={selectedType === 'income' ? 'Ex.: Projeto freelance' : selectedType === 'expense' ? 'Ex.: Supermercado' : selectedType === 'goal' ? 'Ex.: Ler 12 livros este ano' : 'Ex.: Caminhar por 30 minutos'} autoFocus required /></label>
                    <label><span>Data</span><input name="date" type="date" value={form.date} onChange={handleChange} required /></label>

                    {selectedType === 'activity' && <ActivityFields form={form} options={options} onChange={handleChange} />}
                    {selectedType === 'goal' && <GoalFields form={form} onChange={handleChange} />}
                    {(selectedType === 'income' || selectedType === 'expense') && <TransactionFields form={form} options={options} categories={availableFinancialCategories} onChange={handleChange} />}

                    {(selectedType === 'activity' || selectedType === 'goal') && <label className={styles.full}><span>Observação <i>opcional</i></span><textarea name="description" value={form.description} onChange={handleChange} rows={3} maxLength={1000} /></label>}
                    {(selectedType === 'income' || selectedType === 'expense') && <label className={styles.full}><span>Observação <i>opcional</i></span><textarea name="notes" value={form.notes} onChange={handleChange} rows={3} maxLength={1000} /></label>}
                  </div>
                )}

                {errorMessage && <p className={styles.errorMessage} role="alert">{errorMessage}</p>}
                <footer><button type="button" onClick={onClose} disabled={isSaving}>Cancelar</button><button type="submit" disabled={isSaving || isLoadingOptions}>{isSaving ? <LoaderCircle className={styles.spinner} size={17} /> : <Plus size={17} />}{isSaving ? 'Salvando...' : `Registrar ${selectedConfig.title.toLowerCase()}`}</button></footer>
              </form>
            )}

            {!selectedType && errorMessage && <p className={styles.errorMessage} role="alert">{errorMessage}</p>}
          </>
        )}
      </section>
    </div>
  )
}

function ActivityFields({ form, options, onChange }) {
  return <><label><span>Horário</span><input name="time" type="time" value={form.time} onChange={onChange} /></label><label><span>Duração</span><div className={styles.suffixedInput}><input name="duration" type="number" value={form.duration} onChange={onChange} min="1" max="1440" /><small>min</small></div></label><label><span>Categoria</span><select name="activityCategoryId" value={form.activityCategoryId} onChange={onChange}><option value="">Sem categoria</option>{options.activityCategories.map((category) => <option value={category.id} key={category.id}>{category.name}</option>)}</select></label></>
}

function GoalFields({ form, onChange }) {
  return <><label><span>Tipo de meta</span><select name="goalType" value={form.goalType} onChange={onChange}><option value="quantity">Quantidade</option><option value="duration">Tempo</option><option value="money">Dinheiro</option><option value="boolean">Sim ou não</option></select></label>{form.goalType !== 'boolean' && <label><span>Valor desejado</span><input name="targetValue" type="number" value={form.targetValue} onChange={onChange} min="0.01" step="0.01" required /></label>}<label><span>Unidade</span><input name="unit" value={form.unit} onChange={onChange} maxLength={30} placeholder={form.goalType === 'money' ? 'R$' : form.goalType === 'duration' ? 'horas' : 'vezes'} /></label><label><span>Data-alvo</span><input name="targetDate" type="date" value={form.targetDate} min={form.date} onChange={onChange} /></label></>
}

function TransactionFields({ form, options, categories, onChange }) {
  return <><label><span>Valor</span><div className={styles.prefixedInput}><small>R$</small><input name="amount" type="number" value={form.amount} onChange={onChange} min="0.01" step="0.01" required /></div></label><label><span>Conta</span><select name="accountId" value={form.accountId} onChange={onChange} required><option value="">Selecione</option>{options.accounts.map((account) => <option value={account.id} key={account.id}>{account.name}</option>)}</select></label><label><span>Categoria</span><select name="financialCategoryId" value={form.financialCategoryId} onChange={onChange}><option value="">Sem categoria</option>{categories.map((category) => <option value={category.id} key={category.id}>{category.name}</option>)}</select></label></>
}
