import { useEffect, useMemo, useState } from 'react'
import {
  CalendarDays,
  Check,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  Circle,
  Clock3,
  Flame,
  LoaderCircle,
  Pause,
  Play,
  Plus,
  RotateCcw,
  Sparkles,
  Trash2,
  X,
} from 'lucide-react'
import { AuthMascot } from '../../components/auth/AuthMascot'
import {
  completeActivity,
  createMyDayActivity,
  deleteActivity,
  getMyDay,
  pauseActivity,
  startActivity,
  stopActivity,
} from '../../services/myDayApi'
import styles from './MyDayPage.module.css'

function toLocalDateInput(date = new Date()) {
  const offset = date.getTimezoneOffset() * 60_000
  return new Date(date.getTime() - offset).toISOString().slice(0, 10)
}

function moveDate(date, amount) {
  const value = new Date(`${date}T12:00:00`)
  value.setDate(value.getDate() + amount)
  return toLocalDateInput(value)
}

function formatDuration(seconds = 0) {
  const minutes = Math.floor(seconds / 60)
  const hours = Math.floor(minutes / 60)
  const remainingMinutes = minutes % 60
  return hours
    ? `${hours}h ${remainingMinutes.toString().padStart(2, '0')}min`
    : `${remainingMinutes}min`
}

function formatTimer(seconds = 0) {
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const remainingSeconds = Math.floor(seconds % 60)
  return [hours, minutes, remainingSeconds]
    .map((part) => part.toString().padStart(2, '0'))
    .join(':')
}

function formatTime(dateTime) {
  if (!dateTime) return 'Horário livre'
  return new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit' }).format(
    new Date(dateTime),
  )
}

export function MyDayPage() {
  const today = toLocalDateInput()
  const [selectedDate, setSelectedDate] = useState(today)
  const [day, setDay] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [actionId, setActionId] = useState(null)
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [activityToDelete, setActivityToDelete] = useState(null)
  const [reloadKey, setReloadKey] = useState(0)
  const [now, setNow] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    getMyDay(selectedDate, controller.signal)
      .then(setDay)
      .catch((error) => {
        if (error.name !== 'AbortError') setErrorMessage(error.message)
      })
      .finally(() => setIsLoading(false))

    return () => controller.abort()
  }, [reloadKey, selectedDate])

  useEffect(() => {
    if (!day?.activities.some((activity) => activity.status === 'in_progress')) return undefined
    const initialTimer = window.setTimeout(() => setNow(Date.now()), 0)
    const timer = window.setInterval(() => setNow(Date.now()), 1000)
    return () => {
      window.clearTimeout(initialTimer)
      window.clearInterval(timer)
    }
  }, [day])

  const selectedDateLabel = useMemo(
    () =>
      new Intl.DateTimeFormat('pt-BR', {
        weekday: 'long',
        day: '2-digit',
        month: 'long',
      }).format(new Date(`${selectedDate}T12:00:00`)),
    [selectedDate],
  )

  async function handleAction(id, action) {
    setActionId(id)
    setErrorMessage('')

    try {
      if (action === 'start') await startActivity(id)
      if (action === 'pause') await pauseActivity(id)
      if (action === 'stop') await stopActivity(id)
      if (action === 'complete') await completeActivity(id)
      setReloadKey((current) => current + 1)
    } catch (error) {
      setErrorMessage(error.message)
    } finally {
      setActionId(null)
    }
  }

  function selectDate(date) {
    setDay(null)
    setIsLoading(true)
    setErrorMessage('')
    setSelectedDate(date)
  }

  function retryLoading() {
    setIsLoading(true)
    setErrorMessage('')
    setReloadKey((current) => current + 1)
  }

  return (
    <div className={styles.page}>
      <header className={styles.heading}>
        <div>
          <span>
            <Sparkles size={14} /> UM PASSO DE CADA VEZ
          </span>
          <h1>{selectedDate === today ? 'Meu dia' : 'Sua agenda'}</h1>
          <p className={styles.dateLabel}>{selectedDateLabel}</p>
        </div>
        <div className={styles.headingActions}>
          <div className={styles.dateNavigation}>
            <button
              type="button"
              aria-label="Dia anterior"
              onClick={() => selectDate(moveDate(selectedDate, -1))}
            >
              <ChevronLeft size={18} />
            </button>
            <button
              type="button"
              onClick={() => selectDate(today)}
              disabled={selectedDate === today}
            >
              Hoje
            </button>
            <button
              type="button"
              aria-label="Próximo dia"
              onClick={() => selectDate(moveDate(selectedDate, 1))}
            >
              <ChevronRight size={18} />
            </button>
          </div>
          <button className={styles.addButton} type="button" onClick={() => setIsCreateOpen(true)}>
            <Plus size={18} /> Registrar atividade
          </button>
        </div>
      </header>

      {errorMessage && (
        <div className={styles.errorBanner} role="alert">
          {errorMessage}
          <button type="button" onClick={retryLoading}>
            Tentar novamente
          </button>
        </div>
      )}

      {isLoading && !day ? (
        <MyDaySkeleton />
      ) : (
        day && (
          <>
            <section className={styles.summaryGrid} aria-label="Resumo do dia">
              <SummaryCard
                icon={CheckCircle2}
                label="Concluídas"
                value={`${day.summary.completed} de ${day.summary.total}`}
                note={`${day.summary.completionPercentage}% do seu dia`}
                tone="lime"
              />
              <SummaryCard
                icon={Clock3}
                label="Tempo em foco"
                value={formatDuration(day.summary.focusSeconds)}
                note="tempo registrado"
              />
              <SummaryCard
                icon={Flame}
                label="Em andamento"
                value={day.summary.inProgress}
                note={day.summary.inProgress ? 'cronômetro ativo' : 'nenhum cronômetro'}
                tone="dark"
              />
            </section>

            <section className={styles.dayGrid}>
              <div className={styles.timelineCard}>
                <header>
                  <div>
                    <span>AGENDA</span>
                    <h2>O ritmo do seu dia</h2>
                  </div>
                  <strong>{day.activities.length}</strong>
                </header>

                {day.activities.length === 0 ? (
                  <div className={styles.emptyState}>
                    <span>
                      <CalendarDays size={23} />
                    </span>
                    <h3>Seu dia está livre.</h3>
                    <p>Registre uma atividade ou aproveite o espaço para respirar.</p>
                    <button type="button" onClick={() => setIsCreateOpen(true)}>
                      <Plus size={16} /> Criar primeira atividade
                    </button>
                  </div>
                ) : (
                  <div className={styles.timeline}>
                    {day.activities.map((activity) => (
                      <ActivityRow
                        activity={activity}
                        now={now}
                        isLoading={actionId === activity.id}
                        onAction={handleAction}
                        onDelete={setActivityToDelete}
                        key={activity.id}
                      />
                    ))}
                  </div>
                )}
              </div>

              <aside className={styles.companionCard}>
                <div className={styles.mascotStage}>
                  <AuthMascot className={styles.mascot} />
                </div>
                <span>BUBU DIZ</span>
                <h2>
                  {day.summary.total === 0
                    ? 'Sem pressa para começar.'
                    : day.summary.completed === day.summary.total
                      ? 'Você cuidou do seu dia!'
                      : 'Seu ritmo já está tomando forma.'}
                </h2>
                <p>
                  {day.summary.total === 0
                    ? 'Escolha só uma coisa importante. O restante pode encontrar espaço depois.'
                    : `Você concluiu ${day.summary.completed} de ${day.summary.total} atividades. Continue no seu ritmo.`}
                </p>
                <div className={styles.progressTrack}>
                  <i style={{ width: `${day.summary.completionPercentage}%` }} />
                </div>
                <small>{day.summary.completionPercentage}% concluído</small>
              </aside>
            </section>
          </>
        )
      )}

      {isCreateOpen && (
        <QuickActivityModal
          date={selectedDate}
          categories={day?.categories ?? []}
          onClose={() => setIsCreateOpen(false)}
          onCreated={() => {
            setIsCreateOpen(false)
            setReloadKey((current) => current + 1)
          }}
        />
      )}

      {activityToDelete && (
        <DeleteActivityModal
          activity={activityToDelete}
          onClose={() => setActivityToDelete(null)}
          onDeleted={() => {
            setActivityToDelete(null)
            setReloadKey((current) => current + 1)
          }}
        />
      )}
    </div>
  )
}

function ActivityRow({ activity, now, isLoading, onAction, onDelete }) {
  const isCompleted = activity.status === 'completed'
  const isRunning = activity.status === 'in_progress'
  const runningSeconds =
    isRunning && activity.startedAt
      ? Math.max(0, Math.floor((now - new Date(activity.startedAt).getTime()) / 1000))
      : activity.durationSeconds

  return (
    <article className={`${styles.activityRow} ${isCompleted ? styles.completedActivity : ''}`}>
      <div className={styles.timeColumn}>
        <strong>{formatTime(activity.startedAt)}</strong>
        <span>{formatDuration(runningSeconds)}</span>
      </div>
      <span
        className={styles.timelineDot}
        style={{ '--category-color': activity.categoryColor || '#70c52f' }}
      >
        {isCompleted ? <Check size={14} /> : isRunning ? <Pause size={13} /> : <Circle size={13} />}
      </span>
      <div className={styles.activityContent}>
        <span>{activity.categoryName ?? 'Sem categoria'}</span>
        <h3>{activity.title}</h3>
        {activity.description && <p>{activity.description}</p>}
        {isRunning && (
          <strong className={styles.liveTimer}>
            <i /> {formatTimer(runningSeconds)}
          </strong>
        )}
      </div>
      <div className={styles.activityActions}>
        {isCompleted ? (
          <span className={styles.doneBadge}>
            <Check size={14} /> Concluída
          </span>
        ) : isRunning ? (
          <>
            <button
              className={styles.secondaryAction}
              type="button"
              onClick={() => onAction(activity.id, 'pause')}
              disabled={isLoading}
            >
              {isLoading ? (
                <LoaderCircle className={styles.spinner} size={16} />
              ) : (
                <Pause size={16} />
              )}{' '}
              Pausar
            </button>
            <button
              type="button"
              onClick={() => onAction(activity.id, 'stop')}
              disabled={isLoading}
            >
              <Check size={16} /> Finalizar
            </button>
          </>
        ) : (
          <>
            <button
              className={styles.secondaryAction}
              type="button"
              onClick={() => onAction(activity.id, 'complete')}
              disabled={isLoading}
            >
              <Check size={16} /> Concluir
            </button>
            <button
              type="button"
              onClick={() => onAction(activity.id, 'start')}
              disabled={isLoading}
            >
              {isLoading ? (
                <LoaderCircle className={styles.spinner} size={16} />
              ) : activity.isPaused ? (
                <RotateCcw size={16} />
              ) : (
                <Play size={16} />
              )}{' '}
              {activity.isPaused ? 'Retomar' : 'Iniciar'}
            </button>
          </>
        )}
        <button
          className={styles.deleteAction}
          type="button"
          aria-label={`Remover ${activity.title}`}
          onClick={() => onDelete(activity)}
          disabled={isLoading}
        >
          <Trash2 size={15} />
        </button>
      </div>
    </article>
  )
}

function QuickActivityModal({ date, categories, onClose, onCreated }) {
  const [form, setForm] = useState({
    title: '',
    description: '',
    activityDate: date,
    startTime: '',
    durationMinutes: '30',
    categoryId: '',
  })
  const [isSaving, setIsSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

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

  function handleChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
    setErrorMessage('')
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setIsSaving(true)

    try {
      await createMyDayActivity({
        title: form.title.trim(),
        description: form.description.trim() || null,
        activityDate: form.activityDate,
        startTime: form.startTime ? `${form.startTime}:00` : null,
        durationMinutes: form.durationMinutes ? Number(form.durationMinutes) : null,
        categoryId: form.categoryId ? Number(form.categoryId) : null,
      })
      onCreated()
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
      <form
        className={styles.quickModal}
        onSubmit={handleSubmit}
        role="dialog"
        aria-modal="true"
        aria-labelledby="quick-activity-title"
      >
        <header>
          <div>
            <span>
              <Plus size={19} />
            </span>
            <div>
              <small>NOVO REGISTRO</small>
              <h2 id="quick-activity-title">O que você quer fazer?</h2>
            </div>
          </div>
          <button type="button" aria-label="Fechar" onClick={onClose} disabled={isSaving}>
            <X size={19} />
          </button>
        </header>
        <div className={styles.modalMascotIntro}>
          <div>
            <AuthMascot className={styles.modalMascot} />
          </div>
          <p>
            <strong>Vamos dar espaço para algo importante?</strong>Conte para o Bubu e ele organiza
            essa atividade no seu dia.
          </p>
        </div>
        <div className={styles.modalFields}>
          <label className={styles.fullField}>
            <span>Título</span>
            <input
              name="title"
              value={form.title}
              onChange={handleChange}
              minLength={2}
              maxLength={150}
              placeholder="Ex.: Caminhar por 30 minutos"
              autoFocus
              required
            />
          </label>
          <label>
            <span>Data</span>
            <input
              name="activityDate"
              type="date"
              value={form.activityDate}
              onChange={handleChange}
              required
            />
          </label>
          <label>
            <span>Horário</span>
            <input name="startTime" type="time" value={form.startTime} onChange={handleChange} />
          </label>
          <label>
            <span>Duração estimada</span>
            <div className={styles.durationInput}>
              <input
                name="durationMinutes"
                type="number"
                value={form.durationMinutes}
                onChange={handleChange}
                min="1"
                max="1440"
              />
              <small>min</small>
            </div>
          </label>
          <label>
            <span>Categoria</span>
            <select name="categoryId" value={form.categoryId} onChange={handleChange}>
              <option value="">Sem categoria</option>
              {categories.map((category) => (
                <option value={category.id} key={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </label>
          <label className={styles.fullField}>
            <span>
              Uma observação <i>opcional</i>
            </span>
            <textarea
              name="description"
              value={form.description}
              onChange={handleChange}
              maxLength={1000}
              rows={3}
              placeholder="Algum detalhe importante para lembrar?"
            />
          </label>
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
            {isSaving ? <LoaderCircle className={styles.spinner} size={17} /> : <Plus size={17} />}
            {isSaving ? 'Salvando...' : 'Adicionar ao meu dia'}
          </button>
        </footer>
      </form>
    </div>
  )
}

function DeleteActivityModal({ activity, onClose, onDeleted }) {
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
      await deleteActivity(activity.id)
      onDeleted()
    } catch (error) {
      setErrorMessage(error.message)
      setIsDeleting(false)
    }
  }

  return (
    <div
      className={styles.confirmBackdrop}
      onMouseDown={(event) => event.target === event.currentTarget && !isDeleting && onClose()}
    >
      <section
        className={styles.confirmModal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="delete-activity-title"
      >
        <button
          className={styles.confirmClose}
          type="button"
          aria-label="Fechar"
          onClick={onClose}
          disabled={isDeleting}
        >
          <X size={18} />
        </button>
        <div className={styles.confirmMascotStage}>
          <AuthMascot className={styles.confirmMascot} />
        </div>
        <span>BUBU QUER TER CERTEZA</span>
        <h2 id="delete-activity-title">Tiramos isso do seu dia?</h2>
        <p>
          A atividade <strong>“{activity.title}”</strong> será removida. Tudo bem mudar de ideia —
          só não dá para desfazer depois.
        </p>
        {errorMessage && (
          <p className={styles.confirmError} role="alert">
            {errorMessage}
          </p>
        )}
        <footer>
          <button type="button" onClick={onClose} disabled={isDeleting}>
            Manter atividade
          </button>
          <button type="button" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting ? (
              <LoaderCircle className={styles.spinner} size={17} />
            ) : (
              <Trash2 size={17} />
            )}
            {isDeleting ? 'Removendo...' : 'Remover do dia'}
          </button>
        </footer>
      </section>
    </div>
  )
}

function SummaryCard({ icon: Icon, label, value, note, tone = '' }) {
  return (
    <article className={`${styles.summaryCard} ${styles[tone] ?? ''}`}>
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

function MyDaySkeleton() {
  return (
    <div className={styles.skeleton}>
      <div>
        <i />
        <i />
        <i />
      </div>
      <span />
      <section>
        <i />
        <i />
      </section>
    </div>
  )
}
