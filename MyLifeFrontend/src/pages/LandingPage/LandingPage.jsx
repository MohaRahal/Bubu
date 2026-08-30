import { lazy, Suspense } from 'react'
import {
  Activity,
  ArrowRight,
  CalendarCheck,
  Check,
  CircleDollarSign,
  Clock3,
  ShieldCheck,
  Sparkles,
  Target,
  TrendingUp,
  WalletCards,
} from 'lucide-react'
import { BrandMark } from '../../components/brand/BrandMark'
import { SiteHeader } from '../../components/layout/SiteHeader'
import styles from './LandingPage.module.css'

const ScrollMascot = lazy(() =>
  import('../../components/mascot/ScrollMascot').then((module) => ({
    default: module.ScrollMascot,
  })),
)

const features = [
  {
    icon: Activity,
    title: 'Rotina sem atrito',
    description: 'Registre atividades, acompanhe seu tempo e entenda onde seus dias realmente acontecem.',
    className: styles.featureRoutine,
  },
  {
    icon: Target,
    title: 'Objetivos visíveis',
    description: 'Transforme planos em metas mensuráveis e acompanhe cada avanço sem perder o contexto.',
    className: styles.featureGoals,
  },
  {
    icon: WalletCards,
    title: 'Finanças no controle',
    description: 'Contas, lançamentos, recorrências e transferências organizadas em uma visão simples.',
    className: styles.featureFinance,
  },
]

const steps = [
  ['01', 'Organize seu espaço', 'Crie categorias, contas e metas que combinam com a sua realidade.'],
  ['02', 'Registre o que importa', 'Adicione atividades e movimentações em poucos segundos.'],
  ['03', 'Enxergue seus padrões', 'Use uma visão integrada para tomar decisões melhores todos os dias.'],
]

function DashboardPreview() {
  return (
    <div className={styles.previewShell} aria-label="Prévia do painel MyLife">
      <div className={styles.previewTopbar}>
        <div className={styles.miniBrand}><BrandMark /></div>
        <div className={styles.previewSearch}>Buscar no MyLife</div>
        <div className={styles.avatar}>ML</div>
      </div>

      <div className={styles.previewBody}>
        <aside className={styles.previewSidebar}>
          <span className={styles.activeNav}><Sparkles size={15} /> Visão geral</span>
          <span><CalendarCheck size={15} /> Atividades</span>
          <span><Target size={15} /> Objetivos</span>
          <span><WalletCards size={15} /> Finanças</span>
        </aside>

        <div className={styles.previewContent}>
          <div className={styles.previewHeading}>
            <div>
              <small>SEGUNDA, 29 DE AGOSTO</small>
              <h3>Olá, sua semana está no ritmo.</h3>
            </div>
            <span>+ Novo registro</span>
          </div>

          <div className={styles.metricGrid}>
            <article>
              <div className={styles.metricIcon}><Clock3 size={17} /></div>
              <small>FOCO ESTA SEMANA</small>
              <strong>12h 40min</strong>
              <span className={styles.positive}><TrendingUp size={13} /> 18% acima</span>
            </article>
            <article>
              <div className={styles.metricIcon}><Target size={17} /></div>
              <small>METAS EM ANDAMENTO</small>
              <strong>4 de 6</strong>
              <div className={styles.progress}><span /></div>
            </article>
            <article>
              <div className={styles.metricIcon}><CircleDollarSign size={17} /></div>
              <small>SALDO DO MÊS</small>
              <strong>R$ 2.480</strong>
              <span className={styles.positive}>Tudo em dia</span>
            </article>
          </div>

          <div className={styles.previewLower}>
            <article className={styles.chartCard}>
              <div className={styles.cardHeading}><strong>Seu ritmo</strong><small>Últimos 7 dias</small></div>
              <div className={styles.chart} aria-hidden="true">
                {[42, 68, 55, 82, 62, 90, 76].map((height, index) => (
                  <span key={index} style={{ '--height': `${height}%` }} />
                ))}
              </div>
              <div className={styles.chartLabels}><span>S</span><span>T</span><span>Q</span><span>Q</span><span>S</span><span>S</span><span>D</span></div>
            </article>
            <article className={styles.todayCard}>
              <div className={styles.cardHeading}><strong>Hoje</strong><small>3 atividades</small></div>
              {['Treino matinal', 'Revisar orçamento', 'Estudar 45 minutos'].map((item, index) => (
                <div className={styles.task} key={item}>
                  <span className={index < 2 ? styles.taskDone : ''}>{index < 2 && <Check size={11} />}</span>
                  <p>{item}<small>{index === 0 ? 'Saúde' : index === 1 ? 'Finanças' : 'Desenvolvimento'}</small></p>
                </div>
              ))}
            </article>
          </div>
        </div>
      </div>
    </div>
  )
}

export function LandingPage() {
  return (
    <div className={styles.page}>
      <SiteHeader />
      <Suspense fallback={null}>
        <ScrollMascot />
      </Suspense>

      <main>
        <section className={styles.hero} id="inicio">
          <div className={styles.heroGlow} />
          <div className={styles.container}>
            <div className={styles.eyebrow}><Sparkles size={15} /> Sua vida, com mais clareza</div>
            <h1>Menos abas abertas.<br /><span>Mais vida acontecendo.</span></h1>
            <p className={styles.heroCopy}>Rotina, objetivos e finanças finalmente conversando no mesmo lugar. O MyLife ajuda você a organizar hoje sem perder de vista o amanhã.</p>
            <div className={styles.heroActions}>
              <a className={styles.primaryButton} href="#comece">Começar gratuitamente <ArrowRight size={18} /></a>
              <a className={styles.secondaryButton} href="#recursos">Conhecer recursos</a>
            </div>
            <p className={styles.heroNote}><Check size={14} /> Gratuito para começar <span /> Sem cartão de crédito</p>
            <DashboardPreview />
          </div>
        </section>

        <section className={styles.promise} aria-label="Proposta do MyLife">
          <div className={styles.container}>
            <p>Um sistema simples para cuidar do que sustenta uma vida inteira.</p>
            <div className={styles.promiseItems}>
              <span><Activity /> Sua rotina</span>
              <i />
              <span><Target /> Seus objetivos</span>
              <i />
              <span><WalletCards /> Seu dinheiro</span>
            </div>
          </div>
        </section>

        <section className={styles.features} id="recursos">
          <div className={styles.container}>
            <div className={styles.sectionIntro}>
              <span>UMA VISÃO COMPLETA</span>
              <h2>Tudo que importa.<br />Sem complicar o que é simples.</h2>
              <p>Cada área funciona muito bem sozinha. Juntas, elas mostram a história completa.</p>
            </div>

            <div className={styles.featureGrid}>
              {features.map(({ icon: Icon, title, description, className }) => (
                <article className={`${styles.featureCard} ${className}`} key={title}>
                  <div className={styles.featureIcon}><Icon size={22} /></div>
                  <div>
                    <h3>{title}</h3>
                    <p>{description}</p>
                  </div>
                  <a href="#comece">Explorar recurso <ArrowRight size={16} /></a>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section className={styles.howItWorks} id="como-funciona">
          <div className={styles.container}>
            <div className={styles.sectionIntro}>
              <span>DO SEU JEITO</span>
              <h2>Comece pequeno.<br />Evolua no seu ritmo.</h2>
            </div>
            <div className={styles.steps}>
              {steps.map(([number, title, description]) => (
                <article key={number}>
                  <span>{number}</span>
                  <h3>{title}</h3>
                  <p>{description}</p>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section className={styles.security} id="seguranca">
          <div className={styles.container}>
            <div className={styles.securityCard}>
              <div className={styles.securityIcon}><ShieldCheck size={30} /></div>
              <div>
                <span>SEUS DADOS SÃO SEUS</span>
                <h2>Privacidade não é recurso premium.</h2>
                <p>O MyLife foi pensado para manter suas informações pessoais protegidas, com boas práticas desde a primeira linha de código.</p>
              </div>
              <ul>
                <li><Check size={16} /> Senhas protegidas por hash</li>
                <li><Check size={16} /> Dados isolados por usuário</li>
                <li><Check size={16} /> Controle sobre seus registros</li>
              </ul>
            </div>
          </div>
        </section>

        <section className={styles.finalCta} id="comece">
          <div className={styles.ctaPattern} />
          <div className={styles.ctaContent}>
            <span>COMECE HOJE</span>
            <h2>Sua vida já tem história.<br />Dê clareza ao próximo capítulo.</h2>
            <p>Crie seu espaço no MyLife e organize o que realmente importa.</p>
            <a href="mailto:contato@mylife.app">Quero começar <ArrowRight size={18} /></a>
          </div>
        </section>
      </main>

      <footer className={styles.footer}>
        <div className={styles.container}>
          <BrandMark />
          <p>Vida organizada, sem deixar de ser vida.</p>
          <span>© 2026 MyLife</span>
        </div>
      </footer>
    </div>
  )
}
