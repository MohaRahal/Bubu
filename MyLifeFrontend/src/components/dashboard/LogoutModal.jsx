import { useEffect, useRef } from 'react'
import { LogOut, X } from 'lucide-react'
import { AuthMascot } from '../auth/AuthMascot'
import styles from './LogoutModal.module.css'

export function LogoutModal({ isLoading, errorMessage, onConfirm, onStay }) {
  const stayButtonRef = useRef(null)

  useEffect(() => {
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    stayButtonRef.current?.focus()

    function handleKeyDown(event) {
      if (event.key === 'Escape' && !isLoading) onStay()
    }

    window.addEventListener('keydown', handleKeyDown)

    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', handleKeyDown)
    }
  }, [isLoading, onStay])

  return (
    <div className={styles.backdrop} role="presentation" onMouseDown={(event) => {
      if (event.target === event.currentTarget && !isLoading) onStay()
    }}>
      <section className={styles.modal} role="dialog" aria-modal="true" aria-labelledby="logout-title" aria-describedby="logout-description">
        <button className={styles.closeButton} type="button" aria-label="Fechar" onClick={onStay} disabled={isLoading}>
          <X size={18} />
        </button>

        <div className={styles.mascotStage} aria-hidden="true">
          <span className={styles.orbit} />
          <AuthMascot className={styles.mascot} />
        </div>

        <span className={styles.eyebrow}>O BUBU FICOU TRISTE</span>
        <h2 id="logout-title">Você já vai?</h2>
        <p id="logout-description">O Bubu vai sentir sua falta por aqui. Tem certeza de que deseja sair da sua conta?</p>

        {errorMessage && <p className={styles.errorMessage} role="alert">{errorMessage}</p>}

        <div className={styles.actions}>
          <button ref={stayButtonRef} className={styles.stayButton} type="button" onClick={onStay} disabled={isLoading}>
            Ficar com o Bubu
          </button>
          <button className={styles.leaveButton} type="button" onClick={onConfirm} disabled={isLoading}>
            <LogOut size={17} />
            {isLoading ? 'Saindo...' : 'Sair mesmo'}
          </button>
        </div>
      </section>
    </div>
  )
}
