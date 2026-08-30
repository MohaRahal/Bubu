import bubuLoading from '../../assets/bubu-3d.png'
import styles from './BubuLoadingScreen.module.css'

export function BubuLoadingScreen({
  title = 'Bem-vindo ao seu espaço.',
  message = 'O Bubu está organizando tudo para você...'
}) {
  return (
    <div className={styles.loadingScreen} role="status" aria-live="polite">
      <div className={styles.transitionGlow} aria-hidden="true" />
      <img className={styles.transitionBubu} src={bubuLoading} alt="" />
      <strong>{title}</strong>
      <p>{message}</p>
      <span className={styles.transitionProgress} aria-hidden="true">
        <i />
      </span>
    </div>
  )
}
