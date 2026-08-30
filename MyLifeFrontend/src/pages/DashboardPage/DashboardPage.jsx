import { useEffect, useMemo, useState } from 'react'
import {
  Activity,
  ArrowDownRight,
  ArrowUpRight,
  CalendarCheck,
  CheckCircle2,
  ChevronRight,
  Circle,
  CircleDollarSign,
  Clock3,
  Goal,
  LayoutDashboard,
  Lightbulb,
  LogOut,
  Menu,
  PanelLeftClose,
  RefreshCw,
  Settings,
  Target,
  WalletCards,
  X,
} from 'lucide-react'
import { useLocation, useNavigate } from 'react-router'
import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import bubuIcon from '../../assets/bubu-3d.png'
import { LogoutModal } from '../../components/dashboard/LogoutModal'
import { useAuth } from '../../hooks/useAuth'
import { ProfilePage } from '../ProfilePage/ProfilePage'
import { MyDayPage } from '../MyDayPage/MyDayPage'
import { getDashboard } from '../../services/authApi'
import styles from './DashboardPage.module.css'

const menuItems = [
  { label: 'Visão geral', icon: LayoutDashboard, path: '/dashboard' },
  { label: 'Meu dia', icon: CalendarCheck, path: '/dashboard/meu-dia' },
  { label: 'Atividades', icon: Activity },
  { label: 'Objetivos', icon: Target },
  { label: 'Finanças', icon: WalletCards },
]

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

const shortDateFormatter = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: 'short',
})

function formatCurrency(value) {
  return currencyFormatter.format(value ?? 0)
}

function formatDuration(seconds = 0) {
  const totalMinutes = Math.floor(seconds / 60)
  const hours = Math.floor(totalMinutes / 60)
  const minutes = totalMinutes % 60

  if (hours === 0) return `${minutes}min`
  return `${hours}h ${minutes.toString().padStart(2, '0')}min`
}

function formatDate(date) {
  return shortDateFormatter.format(new Date(`${date}T12:00:00`)).replace('.', '')
}

function formatTime(dateTime) {
  if (!dateTime) return 'Sem horário'

  return new Intl.DateTimeFormat('pt-BR', {
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(dateTime))
}

function ChartTooltip({ active, payload, label, metric }) {
  if (!active || !payload?.length) return null

  return (
    <div className={styles.chartTooltip}>
      <span>{label}</span>
      {payload.map((item) => (
        <p key={item.dataKey}>
          <i style={{ background: item.color }} />
          {item.name}
          <strong>
            {metric === 'focus' && formatDuration(item.value)}
            {metric === 'activities' && `${item.value} concluídas`}
            {metric === 'finance' && formatCurrency(item.value)}
          </strong>
        </p>
      ))}
    </div>
  )
}

export function DashboardPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { logout, user } = useAuth()
  const isProfileView = location.pathname === '/dashboard/perfil'
  const isMyDayView = location.pathname === '/dashboard/meu-dia'
  const isOverviewView = !isProfileView && !isMyDayView
  const [dashboard, setDashboard] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const [isLogoutModalOpen, setIsLogoutModalOpen] = useState(false)
  const [logoutError, setLogoutError] = useState('')
  const [errorMessage, setErrorMessage] = useState('')
  const [reloadKey, setReloadKey] = useState(0)
  const [chartMetric, setChartMetric] = useState('focus')
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  useEffect(() => {
    const previousTitle = document.title
    document.title = isProfileView
      ? 'Meu perfil | Bubu'
      : isMyDayView ? 'Meu dia | Bubu' : 'Dashboard | Bubu'

    return () => {
      document.title = previousTitle
    }
  }, [isMyDayView, isProfileView])

  useEffect(() => {
    if (!isOverviewView) return undefined

    const controller = new AbortController()
    let isCurrentRequest = true

    getDashboard(controller.signal)
      .then((dashboardData) => {
        if (isCurrentRequest) setDashboard(dashboardData)
      })
      .catch((error) => {
        if (isCurrentRequest && error.name !== 'AbortError') {
          setErrorMessage(error.message)
        }
      })
      .finally(() => {
        if (isCurrentRequest) setIsLoading(false)
      })

    return () => {
      isCurrentRequest = false
      controller.abort()
    }
  }, [isOverviewView, reloadKey])

  const chartData = useMemo(() => dashboard?.lastSevenDays.map((point) => ({
    ...point,
    label: formatDate(point.date),
    focus: point.focusSeconds,
  })) ?? [], [dashboard])

  async function handleLogout() {
    setIsLoggingOut(true)
    setLogoutError('')

    try {
      await logout()
      navigate('/login', { replace: true })
    } catch (error) {
      setLogoutError(error.message)
      setIsLoggingOut(false)
    }
  }

  function openLogoutModal() {
    setLogoutError('')
    setIsMenuOpen(false)
    setIsLogoutModalOpen(true)
  }

  function closeLogoutModal() {
    if (isLoggingOut) return
    setLogoutError('')
    setIsLogoutModalOpen(false)
  }

  function handleRetry() {
    setIsLoading(true)
    setErrorMessage('')
    setReloadKey((key) => key + 1)
  }

  return (
    <div className={styles.shell}>
      {isLogoutModalOpen && (
        <LogoutModal
          isLoading={isLoggingOut}
          errorMessage={logoutError}
          onConfirm={handleLogout}
          onStay={closeLogoutModal}
        />
      )}

      {isMenuOpen && (
        <button
          className={styles.menuBackdrop}
          type="button"
          aria-label="Fechar menu"
          onClick={() => setIsMenuOpen(false)}
        />
      )}

      <aside className={`${styles.sidebar} ${isMenuOpen ? styles.sidebarOpen : ''}`}>
        <div className={styles.sidebarHeader}>
          <div className={styles.dashboardBrand} aria-label="Bubu">
            <span><img src={bubuIcon} alt="" /></span>
            <strong>BuBu</strong>
          </div>
          <button type="button" aria-label="Fechar menu" onClick={() => setIsMenuOpen(false)}>
            <X size={19} />
          </button>
        </div>

        <nav className={styles.navigation} aria-label="Menu do dashboard">
          <span>SEU ESPAÇO</span>
          {menuItems.map(({ label, icon: Icon, path }) => {
            const isActive = location.pathname === path || (path === '/dashboard' && location.pathname === '/dashboard/')

            return (
              <button
                className={isActive ? styles.activeNav : undefined}
                type="button"
                key={label}
                disabled={!path}
                title={!path ? 'Disponível em breve' : undefined}
                onClick={() => {
                  navigate(path)
                  setIsMenuOpen(false)
                }}
              >
                <Icon size={18} />
                {label}
                {isActive && <i />}
              </button>
            )
          })}
        </nav>

        <div className={styles.sidebarBottom}>
          <button className={isProfileView ? styles.profileNav : undefined} type="button" onClick={() => { navigate('/dashboard/perfil'); setIsMenuOpen(false) }}>
            <Settings size={18} /> Meu perfil
            {isProfileView && <i />}
          </button>
          <button className={styles.logoutButton} type="button" onClick={openLogoutModal}>
            <LogOut size={18} /> Sair da conta
          </button>
          <button className={styles.sidebarProfile} type="button" onClick={() => { navigate('/dashboard/perfil'); setIsMenuOpen(false) }} aria-label="Abrir meu perfil">
            <span className={user.avatarUrl ? styles.selectedUserAvatar : undefined}>{user.avatarUrl ? <img src={user.avatarUrl} alt="" /> : user.name.slice(0, 1).toUpperCase()}</span>
            <div><strong>{user.name}</strong><small>{user.email}</small></div>
          </button>
        </div>
      </aside>

      <main className={styles.main}>
        <header className={styles.topbar}>
          <button className={styles.mobileMenu} type="button" aria-label="Abrir menu" onClick={() => setIsMenuOpen(true)}>
            <Menu size={21} />
          </button>
          <div>
            {isProfileView ? <Settings size={17} /> : isMyDayView ? <CalendarCheck size={17} /> : <LayoutDashboard size={17} />}
            <span>{isProfileView ? 'Meu perfil' : isMyDayView ? 'Meu dia' : 'Visão geral'}</span>
          </div>
          <button className={styles.collapseButton} type="button" disabled aria-label="Recolher menu">
            <PanelLeftClose size={18} />
          </button>
          <button className={styles.topProfile} type="button" onClick={() => navigate('/dashboard/perfil')} aria-label="Abrir meu perfil">
            <span className={user.avatarUrl ? styles.selectedUserAvatar : undefined}>{user.avatarUrl ? <img src={user.avatarUrl} alt="" /> : user.name.slice(0, 1).toUpperCase()}</span>
            <strong>{user.name.split(' ')[0]}</strong>
          </button>
        </header>

        <div className={styles.dashboardContent}>
          {isProfileView && <ProfilePage />}
          {isMyDayView && <MyDayPage />}
          {isOverviewView && (
            <>
          <section className={styles.welcome}>
            <div>
              <span className={styles.eyebrow}>SUA VIDA, EM UM SÓ LUGAR</span>
              <h1>Bom dia, {user.name.split(' ')[0]}.</h1>
              <p>Acompanhe seu ritmo e escolha o próximo passo com calma.</p>
            </div>
            <span className={styles.datePill}>
              <CalendarCheck size={17} />
              {new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long' }).format(new Date())}
            </span>
          </section>

          {errorMessage && !dashboard && (
            <section className={styles.errorState} role="alert">
              <RefreshCw size={22} />
              <div><strong>Não conseguimos carregar seus dados.</strong><p>{errorMessage}</p></div>
              <button type="button" onClick={handleRetry}>Tentar novamente</button>
            </section>
          )}

          {isLoading && !dashboard ? (
            <DashboardSkeleton />
          ) : dashboard && (
            <>
              <section className={styles.summaryGrid} aria-label="Resumo do dia">
                <SummaryCard icon={CheckCircle2} label="Atividades de hoje" value={`${dashboard.summary.completedActivitiesToday} de ${dashboard.summary.totalActivitiesToday}`} note="concluídas" tone="lime" />
                <SummaryCard icon={Clock3} label="Tempo registrado" value={formatDuration(dashboard.summary.focusSecondsToday)} note="hoje" />
                <SummaryCard icon={Goal} label="Objetivos ativos" value={dashboard.summary.activeGoals} note="em andamento" />
                <SummaryCard icon={CircleDollarSign} label="Saldo total" value={formatCurrency(dashboard.summary.totalBalance)} note="contas ativas" tone="dark" />
              </section>

              <section className={styles.chartCard}>
                <header>
                  <div><span>ÚLTIMOS 7 DIAS</span><h2>Seu ritmo</h2></div>
                  <div className={styles.chartTabs} role="group" aria-label="Métrica do gráfico">
                    <button className={chartMetric === 'focus' ? styles.activeTab : undefined} type="button" onClick={() => setChartMetric('focus')}>Tempo</button>
                    <button className={chartMetric === 'activities' ? styles.activeTab : undefined} type="button" onClick={() => setChartMetric('activities')}>Atividades</button>
                    <button className={chartMetric === 'finance' ? styles.activeTab : undefined} type="button" onClick={() => setChartMetric('finance')}>Finanças</button>
                  </div>
                </header>

                <div className={styles.chartArea}>
                  <ResponsiveContainer width="100%" height="100%">
                    <AreaChart data={chartData} margin={{ top: 12, right: 10, left: -18, bottom: 0 }}>
                      <defs>
                        <linearGradient id="limeArea" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor="#8de33d" stopOpacity={0.55} /><stop offset="100%" stopColor="#c7ff38" stopOpacity={0.03} /></linearGradient>
                        <linearGradient id="expenseArea" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopColor="#f6a19a" stopOpacity={0.35} /><stop offset="100%" stopColor="#f6a19a" stopOpacity={0.02} /></linearGradient>
                      </defs>
                      <CartesianGrid stroke="#e8ece8" strokeDasharray="3 4" vertical={false} />
                      <XAxis dataKey="label" tickLine={false} axisLine={false} tick={{ fill: '#929991', fontSize: 11 }} dy={10} />
                      <YAxis tickLine={false} axisLine={false} tick={{ fill: '#929991', fontSize: 10 }} tickFormatter={(value) => chartMetric === 'focus' ? `${Math.round(value / 3600)}h` : value} />
                      <Tooltip content={<ChartTooltip metric={chartMetric} />} cursor={{ stroke: '#b9c2b9', strokeDasharray: '3 3' }} />
                      {chartMetric === 'focus' && <Area name="Tempo" type="monotone" dataKey="focus" stroke="#70c52f" strokeWidth={2.5} fill="url(#limeArea)" dot={{ r: 3, fill: '#fff', stroke: '#70c52f', strokeWidth: 2 }} activeDot={{ r: 5 }} animationDuration={800} />}
                      {chartMetric === 'activities' && <Area name="Atividades" type="monotone" dataKey="completedActivities" stroke="#70c52f" strokeWidth={2.5} fill="url(#limeArea)" dot={{ r: 3, fill: '#fff', stroke: '#70c52f', strokeWidth: 2 }} activeDot={{ r: 5 }} animationDuration={800} />}
                      {chartMetric === 'finance' && <Area name="Receitas" type="monotone" dataKey="income" stroke="#70c52f" strokeWidth={2.5} fill="url(#limeArea)" dot={{ r: 3, fill: '#fff', stroke: '#70c52f', strokeWidth: 2 }} animationDuration={800} />}
                      {chartMetric === 'finance' && <Area name="Despesas" type="monotone" dataKey="expense" stroke="#e57b72" strokeWidth={2} fill="url(#expenseArea)" dot={{ r: 3, fill: '#fff', stroke: '#e57b72', strokeWidth: 2 }} animationDuration={800} />}
                    </AreaChart>
                  </ResponsiveContainer>
                </div>

                <footer className={styles.financeStrip}>
                  <div><ArrowUpRight size={17} /><span>Receitas no mês<strong>{formatCurrency(dashboard.summary.monthIncome)}</strong></span></div>
                  <div><ArrowDownRight size={17} /><span>Despesas no mês<strong>{formatCurrency(dashboard.summary.monthExpense)}</strong></span></div>
                  <div><WalletCards size={17} /><span>Resultado do mês<strong>{formatCurrency(dashboard.summary.monthIncome - dashboard.summary.monthExpense)}</strong></span></div>
                </footer>
              </section>

              <section className={styles.insightCard}>
                <span><Lightbulb size={21} /></span>
                <div><small>BUBU PERCEBEU</small><p>{dashboard.insight}</p></div>
              </section>

              <section className={styles.contentGrid}>
                <DataCard title="Próximas atividades" icon={CalendarCheck} count={dashboard.upcomingActivities.length}>
                  {dashboard.upcomingActivities.length === 0
                    ? <EmptyState text="Nenhuma atividade planejada para os próximos dias." />
                    : dashboard.upcomingActivities.map((activity) => (
                      <article className={styles.activityRow} key={activity.id}>
                        <span className={styles.activityCheck}><Circle size={16} /></span>
                        <div><strong>{activity.title}</strong><small>{formatDate(activity.activityDate)} · {formatTime(activity.startedAt)}{activity.categoryName ? ` · ${activity.categoryName}` : ''}</small></div>
                        <ChevronRight size={17} />
                      </article>
                    ))}
                </DataCard>

                <DataCard title="Objetivos em andamento" icon={Target} count={dashboard.activeGoals.length}>
                  {dashboard.activeGoals.length === 0
                    ? <EmptyState text="Crie um objetivo para acompanhar sua evolução aqui." />
                    : dashboard.activeGoals.map((goal) => (
                      <article className={styles.goalRow} key={goal.id}>
                        <div><strong>{goal.title}</strong><span>{goal.progressPercentage}%</span></div>
                        <div className={styles.progressTrack}><i style={{ width: `${goal.progressPercentage}%` }} /></div>
                        <small>{goal.totalActivities === 0 ? 'Ainda sem atividades vinculadas' : `${goal.completedActivities} de ${goal.totalActivities} atividades concluídas`}</small>
                      </article>
                    ))}
                </DataCard>

                <div className={styles.transactionsCard}>
                  <header><div><WalletCards size={18} /><h2>Movimentações recentes</h2></div><span>{dashboard.recentTransactions.length}</span></header>
                  {dashboard.recentTransactions.length === 0
                    ? <EmptyState text="Suas receitas e despesas aparecerão aqui." />
                    : dashboard.recentTransactions.map((transaction) => (
                      <article className={styles.transactionRow} key={transaction.id}>
                        <span className={transaction.transactionType === 'income' ? styles.incomeIcon : styles.expenseIcon}>{transaction.transactionType === 'income' ? <ArrowUpRight size={17} /> : <ArrowDownRight size={17} />}</span>
                        <div><strong>{transaction.description}</strong><small>{transaction.categoryName ?? transaction.accountName} · {formatDate(transaction.transactionDate)}</small></div>
                        <b className={transaction.transactionType === 'income' ? styles.incomeValue : styles.expenseValue}>{transaction.transactionType === 'income' ? '+' : '−'} {formatCurrency(transaction.amount)}</b>
                      </article>
                    ))}
                </div>
              </section>
            </>
          )}
            </>
          )}
        </div>
      </main>
    </div>
  )
}

function SummaryCard({ icon: Icon, label, value, note, tone }) {
  return <article className={`${styles.summaryCard} ${tone ? styles[tone] : ''}`}><div><span><Icon size={18} /></span><small>{label}</small></div><strong>{value}</strong><p>{note}</p></article>
}

function DataCard({ title, icon: Icon, count, children }) {
  return <div className={styles.dataCard}><header><div><Icon size={18} /><h2>{title}</h2></div><span>{count}</span></header><div>{children}</div></div>
}

function EmptyState({ text }) {
  return <div className={styles.emptyState}><span>✦</span><p>{text}</p></div>
}

function DashboardSkeleton() {
  return <div className={styles.skeleton} aria-label="Carregando dados da dashboard"><div className={styles.skeletonCards}>{[1, 2, 3, 4].map((item) => <i key={item} />)}</div><div className={styles.skeletonChart} /><div className={styles.skeletonGrid}><i /><i /></div></div>
}
