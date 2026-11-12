import { AlertTriangle, RefreshCw } from 'lucide-react'
import bubuImage from '../../assets/bubu-3d.png'
import styles from './DataStates.module.css'

export function CardSkeletonGrid({ count = 4, columns = count }) {
  return (
    <div className={styles.cardGrid} style={{ '--skeleton-columns': Math.min(columns, count) }} role="status" aria-label="Carregando informações">
      {Array.from({ length: count }, (_, index) => (
        <article className={styles.cardSkeleton} key={index}>
          <div><i /><span /></div>
          <strong />
          <small />
        </article>
      ))}
    </div>
  )
}

export function PanelSkeleton({ rows = 4 }) {
  return (
    <section className={styles.panelSkeleton} role="status" aria-label="Carregando conteúdo">
      <header><div><i /><span /></div><b /></header>
      <div>{Array.from({ length: rows }, (_, index) => <span key={index}><i /><b /><small /></span>)}</div>
    </section>
  )
}

export function BubuChartLoading({ title = 'Preparando o gráfico...', message = 'O Bubu está organizando seus dados.' }) {
  return (
    <div className={styles.chartLoading} role="status" aria-live="polite">
      <div className={styles.glow} aria-hidden="true" />
      <img src={bubuImage} alt="" />
      <strong>{title}</strong>
      <p>{message}</p>
      <span className={styles.progress} aria-hidden="true"><i /></span>
    </div>
  )
}

export function DataPageSkeleton({ cards = 4, panels = 2, showChart = true }) {
  return (
    <div className={styles.pageSkeleton}>
      {cards > 0 && <CardSkeletonGrid count={cards} />}
      {showChart && <BubuChartLoading />}
      {panels > 0 && <div className={styles.panelGrid}>{Array.from({ length: panels }, (_, index) => <PanelSkeleton rows={3} key={index} />)}</div>}
    </div>
  )
}

export function ErrorNotice({ message, onRetry, title = 'Não conseguimos carregar esta parte.' }) {
  return (
    <div className={styles.errorNotice} role="alert" aria-live="assertive">
      <span><AlertTriangle size={20} /></span>
      <div><strong>{title}</strong><p>{message}</p></div>
      {onRetry && <button type="button" onClick={onRetry}><RefreshCw size={16} /> Tentar novamente</button>}
    </div>
  )
}
