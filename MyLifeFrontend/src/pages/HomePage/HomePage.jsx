import { lazy, Suspense } from 'react'
import {
  Activity,
  ArrowRight,
  Check,
  ChevronDown,
  CircleDollarSign,
  Clock3,
  Eye,
  Fingerprint,
  Heart,
  Layers3,
  ShieldCheck,
  Sparkles,
  Target,
  TrendingUp,
  WalletCards,
} from 'lucide-react'
import { BrandMark } from '../../components/brand/BrandMark'
import { SiteHeader } from '../../components/layout/SiteHeader'
import styles from './HomePage.module.css'

const ScrollMascot = lazy(() =>
  import('../../components/mascot/ScrollMascot').then((module) => ({
    default: module.ScrollMascot,
  })),
)

const AuthMascot = lazy(() =>
  import('../../components/auth/AuthMascot').then((module) => ({
    default: module.AuthMascot,
  })),
)

const questions = [
  {
    question: 'O Bubu é mais um aplicativo de tarefas?',
    answer:
      'Não. Tarefas são apenas uma parte. O Bubu conecta atividades, objetivos e finanças para mostrar como suas escolhas diárias movimentam a sua vida.',
  },
  {
    question: 'Preciso organizar tudo de uma vez?',
    answer:
      'Nem pensar. Você pode começar registrando apenas uma área e adicionar o restante no seu ritmo. O sistema foi pensado para crescer junto com você.',
  },
  {
    question: 'Meus dados ficam protegidos?',
    answer:
      'A arquitetura separa os registros por usuário e protege credenciais com hash. Privacidade e controle de acesso fazem parte da base técnica do produto.',
  },
  {
    question: 'Vai funcionar no celular?',
    answer:
      'Sim. A experiência está sendo construída de forma responsiva para funcionar bem no computador, tablet e celular.',
  },
]

function ProductWindow() {
  return (
    <div className={styles.productWindow} aria-label="Exemplo do painel do Bubu">
      <div className={styles.windowBar}>
        <span />
        <span />
        <span />
        <p>app.bubu.life/hoje</p>
      </div>
      <div className={styles.windowBody}>
        <aside>
          <BrandMark />
          <nav>
            <span className={styles.navActive}>
              <Sparkles size={16} /> Hoje
            </span>
            <span>
              <Activity size={16} /> Atividades
            </span>
            <span>
              <Target size={16} /> Objetivos
            </span>
            <span>
              <WalletCards size={16} /> Finanças
            </span>
          </nav>
          <div className={styles.profile}>
            <span>MF</span>
            <p>
              Meu espaço<small>Plano inicial</small>
            </p>
          </div>
        </aside>
        <div className={styles.appContent}>
          <header>
            <div>
              <small>VISÃO DE HOJE</small>
              <h3>Bom dia, Maria.</h3>
            </div>
            <button>+ Registrar</button>
          </header>
          <div className={styles.dailySummary}>
            <article>
              <Clock3 />
              <p>
                Tempo em foco<strong>2h 35min</strong>
              </p>
              <em>+18%</em>
            </article>
            <article>
              <Target />
              <p>
                Metas avançando<strong>3 de 5</strong>
              </p>
              <em>no ritmo</em>
            </article>
            <article>
              <CircleDollarSign />
              <p>
                Saldo do mês<strong>R$ 1.840</strong>
              </p>
              <em>positivo</em>
            </article>
          </div>
          <div className={styles.appGrid}>
            <article className={styles.weekCard}>
              <div>
                <strong>Energia da semana</strong>
                <small>Uma visão, sem julgamento</small>
              </div>
              <div className={styles.weekBars}>
                {[48, 72, 58, 86, 67, 92, 74].map((height, index) => (
                  <i key={index} style={{ '--bar': `${height}%` }} />
                ))}
              </div>
              <div className={styles.weekDays}>
                <span>S</span>
                <span>T</span>
                <span>Q</span>
                <span>Q</span>
                <span>S</span>
                <span>S</span>
                <span>D</span>
              </div>
            </article>
            <article className={styles.nextCard}>
              <strong>Próximos passos</strong>
              {['Caminhar 30 min', 'Revisar gastos', 'Ler 20 páginas'].map((item, index) => (
                <div key={item}>
                  <span className={index === 0 ? styles.checked : ''}>
                    {index === 0 && <Check size={11} />}
                  </span>
                  <p>
                    {item}
                    <small>
                      {index === 0 ? 'Saúde' : index === 1 ? 'Finanças' : 'Aprendizado'}
                    </small>
                  </p>
                </div>
              ))}
            </article>
          </div>
        </div>
      </div>
    </div>
  )
}

export function HomePage() {
  return (
    <div className={styles.page}>
      <SiteHeader />
      <Suspense fallback={null}>
        <ScrollMascot />
      </Suspense>

      <main>
        <section className={styles.hero} id="inicio">
          <div className={styles.heroNoise} />
          <div className={styles.container}>
            <div className={styles.heroCopy}>
              <div className={styles.kicker}>
                <span>
                  <Sparkles size={14} />
                </span>{' '}
                Seu copiloto para a vida real
              </div>
              <h1>
                A vida não cabe numa <em>planilha.</em>
                <br />
                Ainda bem.
              </h1>
              <p>
                O Bubu junta rotina, sonhos e dinheiro em uma visão que faz sentido — para você
                cuidar da vida sem viver cuidando de aplicativo.
              </p>
              <div className={styles.heroActions}>
                <a href="#comece">
                  Quero conhecer o Bubu <ArrowRight size={18} />
                </a>
                <span>
                  <Check size={15} /> Comece pelo que importa hoje
                </span>
              </div>
            </div>
            <div className={styles.heroStage} aria-hidden="true">
              <div className={styles.orbit} />
              <div className={styles.heroMascot}>
                <Suspense fallback={<span className={styles.heroMascotPlaceholder} />}>
                  <AuthMascot className={styles.heroMascotRive} />
                </Suspense>
              </div>
              <div className={`${styles.floatingNote} ${styles.noteOne}`}>
                <Heart size={18} />
                <span>7 dias cuidando de você</span>
              </div>
              <div className={`${styles.floatingNote} ${styles.noteTwo}`}>
                <TrendingUp size={18} />
                <span>Meta avançou 18%</span>
              </div>
              <div className={`${styles.floatingNote} ${styles.noteThree}`}>
                <WalletCards size={18} />
                <span>Contas em dia</span>
              </div>
            </div>
          </div>
        </section>

        <div className={styles.marquee} aria-hidden="true">
          <div className={styles.marqueeTrack}>
            <div className={styles.marqueeGroup}>
              <span>ROTINA</span>
              <i>✦</i>
              <span>OBJETIVOS</span>
              <i>✦</i>
              <span>FINANÇAS</span>
              <i>✦</i>
              <span>CLAREZA</span>
              <i>✦</i>
            </div>
            <div className={styles.marqueeGroup}>
              <span>ROTINA</span>
              <i>✦</i>
              <span>OBJETIVOS</span>
              <i>✦</i>
              <span>FINANÇAS</span>
              <i>✦</i>
              <span>CLAREZA</span>
              <i>✦</i>
            </div>
          </div>
        </div>

        <section className={styles.manifesto} id="por-que-bubu">
          <div className={styles.container}>
            <span className={styles.sectionLabel}>POR QUE BUBU?</span>
            <div className={styles.manifestoGrid}>
              <h2>
                Você não precisa de mais cobrança.
                <br />
                <em>Precisa enxergar melhor.</em>
              </h2>
              <div>
                <p>
                  Aplicativos tradicionais fragmentam sua vida: um para hábitos, outro para metas,
                  uma planilha para dinheiro e tudo dependendo da sua memória.
                </p>
                <p>
                  O Bubu aproxima essas partes. Não para controlar cada minuto, mas para revelar
                  padrões e ajudar você a escolher com mais intenção.
                </p>
              </div>
            </div>
          </div>
        </section>

        <section className={styles.features} id="recursos">
          <div className={styles.container}>
            <div className={styles.featureHeading}>
              <span className={styles.sectionLabel}>O QUE ELE FAZ</span>
              <h2>
                Três áreas.
                <br />
                Uma vida só.
              </h2>
              <p>O essencial para entender seu presente e construir o próximo passo.</p>
            </div>
            <div className={styles.bento}>
              <article className={styles.routineCard}>
                <div className={styles.cardIcon}>
                  <Activity />
                </div>
                <span>ROTINA</span>
                <h3>Descubra para onde seu tempo está indo.</h3>
                <p>
                  Atividades, duração e categorias mostram seu ritmo sem transformar o dia em
                  cronômetro.
                </p>
                <div className={styles.timeline}>
                  <i />
                  <div>
                    <strong>Estudo focado</strong>
                    <small>08:30 · 45 minutos</small>
                  </div>
                  <em>Concluído</em>
                </div>
                <div className={styles.timeline}>
                  <i />
                  <div>
                    <strong>Treino funcional</strong>
                    <small>18:00 · Planejado</small>
                  </div>
                  <em>Hoje</em>
                </div>
              </article>
              <article className={styles.goalsCard}>
                <div className={styles.cardIcon}>
                  <Target />
                </div>
                <span>OBJETIVOS</span>
                <h3>Metas que não somem depois de janeiro.</h3>
                <p>
                  Conecte ações aos seus objetivos e veja progresso em tempo, quantidade, dinheiro
                  ou conclusão.
                </p>
                <div className={styles.goalRing}>
                  <div>
                    <strong>68%</strong>
                    <small>do objetivo</small>
                  </div>
                </div>
              </article>
              <article className={styles.moneyCard}>
                <div>
                  <div className={styles.cardIcon}>
                    <WalletCards />
                  </div>
                  <span>FINANÇAS</span>
                  <h3>Dinheiro com contexto, não culpa.</h3>
                  <p>
                    Organize contas, transações, recorrências e transferências. Saiba o que
                    aconteceu e o que vem depois.
                  </p>
                </div>
                <div className={styles.balance}>
                  <small>SALDO DISPONÍVEL</small>
                  <strong>R$ 4.280,00</strong>
                  <span>
                    <TrendingUp size={14} /> mês positivo
                  </span>
                </div>
              </article>
              <article className={styles.connectionCard}>
                <Layers3 />
                <div>
                  <span>TUDO CONECTADO</span>
                  <h3>Quando as áreas conversam, suas escolhas ganham sentido.</h3>
                </div>
                <ArrowRight />
              </article>
            </div>
          </div>
        </section>

        <section className={styles.product}>
          <div className={styles.container}>
            <div className={styles.productIntro}>
              <span className={styles.sectionLabel}>SEM POLUIÇÃO VISUAL</span>
              <h2>
                Abra. Entenda.
                <br />
                Siga o seu dia.
              </h2>
              <p>Informação suficiente para decidir, sem dashboards que parecem cabine de avião.</p>
            </div>
            <ProductWindow />
          </div>
        </section>

        <section className={styles.moments}>
          <div className={styles.container}>
            <div className={styles.momentsIntro}>
              <span className={styles.sectionLabel}>BUBU NO SEU DIA</span>
              <h2>
                Útil nos momentos
                <br />
                que realmente contam.
              </h2>
            </div>
            <div className={styles.momentList}>
              <article>
                <span>01</span>
                <div>
                  <Clock3 />
                  <h3>Quando o dia começa</h3>
                  <p>Veja o que merece atenção sem abrir cinco aplicativos.</p>
                </div>
              </article>
              <article>
                <span>02</span>
                <div>
                  <Eye />
                  <h3>Quando algo sai do ritmo</h3>
                  <p>Perceba padrões antes que eles virem uma surpresa.</p>
                </div>
              </article>
              <article>
                <span>03</span>
                <div>
                  <Sparkles />
                  <h3>Quando você quer avançar</h3>
                  <p>Transforme intenção em um próximo passo concreto.</p>
                </div>
              </article>
            </div>
          </div>
        </section>

        <section className={styles.privacy}>
          <div className={styles.container}>
            <div className={styles.privacyVisual}>
              <div>
                <Fingerprint />
              </div>
              <span />
              <span />
              <span />
            </div>
            <div className={styles.privacyCopy}>
              <span className={styles.sectionLabel}>PRIVACIDADE É BASE</span>
              <h2>Sua vida continua sendo sua.</h2>
              <p>
                O Bubu está sendo construído com separação de dados por usuário, credenciais
                protegidas e uma arquitetura que trata segurança como requisito — não como promessa
                de rodapé.
              </p>
              <ul>
                <li>
                  <ShieldCheck /> Senhas nunca são armazenadas em texto puro
                </li>
                <li>
                  <Check /> Você controla seus próprios registros
                </li>
              </ul>
            </div>
          </div>
        </section>

        <section className={styles.faq} id="faq">
          <div className={styles.container}>
            <div>
              <span className={styles.sectionLabel}>PERGUNTAS HONESTAS</span>
              <h2>Antes de deixar o Bubu entrar na sua rotina.</h2>
            </div>
            <div className={styles.questions}>
              {questions.map((item) => (
                <details key={item.question}>
                  <summary>
                    {item.question}
                    <ChevronDown />
                  </summary>
                  <p>{item.answer}</p>
                </details>
              ))}
            </div>
          </div>
        </section>

        <section className={styles.finalCta} id="comece">
          <div className={styles.ctaGlow} />
          <div className={styles.container}>
            <span>UM PASSO DE CADA VEZ</span>
            <h2>Uma vida mais clara começa com um registro.</h2>
            <p>O Bubu cuida da organização. Você continua cuidando da vida.</p>
            <a href="mailto:contato@bubu.life">
              Quero ser avisado <ArrowRight />
            </a>
          </div>
        </section>
      </main>

      <footer className={styles.footer}>
        <div className={styles.container}>
          <BrandMark />
          <p>Feito para vidas reais, não rotinas perfeitas.</p>
          <div>
            <a href="#inicio">Voltar ao topo</a>
            <span>© 2026 Bubu</span>
          </div>
        </div>
      </footer>
    </div>
  )
}
