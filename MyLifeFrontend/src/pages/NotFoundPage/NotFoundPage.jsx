import { useEffect } from 'react'
import { ArrowLeft, Home } from 'lucide-react'
import { Link } from 'react-router'
import bubu3d from '../../assets/bubu-3d.png'
import { BrandMark } from '../../components/brand/BrandMark'
import styles from './NotFoundPage.module.css'

export function NotFoundPage() {
  useEffect(() => {
    const previousTitle = document.title
    document.title = 'Página não encontrada | Bubu'

    return () => {
      document.title = previousTitle
    }
  }, [])

  return (
    <main className={styles.page}>
      <div className={styles.pattern} aria-hidden="true" />

      <header className={styles.header}>
        <Link to="/" aria-label="Ir para a página inicial">
          <BrandMark />
        </Link>
      </header>

      <section className={styles.content}>
        <div className={styles.visual} aria-hidden="true">
          <span className={styles.code}>404</span>
          <span className={`${styles.orbitDot} ${styles.dotOne}`} />
          <span className={`${styles.orbitDot} ${styles.dotTwo}`} />
          <img src={bubu3d} alt="" />
        </div>

        <div className={styles.copy}>
          <span className={styles.eyebrow}>OPA, UM DESVIO NO CAMINHO</span>
          <h1>Nem o Bubu encontrou essa página.</h1>
          <p>
            Parece que este endereço mudou de lugar ou nunca existiu. Está tudo bem — vamos voltar
            para uma parte conhecida.
          </p>

          <div className={styles.actions}>
            <Link className={styles.primaryAction} to="/">
              <Home size={18} />
              Voltar ao início
            </Link>
            <button
              className={styles.backAction}
              type="button"
              onClick={() => window.history.back()}
            >
              <ArrowLeft size={17} />
              Página anterior
            </button>
          </div>
        </div>
      </section>

      <p className={styles.footerNote}>BUBU · ORGANIZE A VIDA DO SEU JEITO</p>
    </main>
  )
}
