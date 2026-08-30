import { Navigate, useLocation } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import styles from './ProtectedRoute.module.css'

export function ProtectedRoute({ children }) {
  const location = useLocation()
  const { isLoading, user } = useAuth()

  if (isLoading) {
    return <RouteLoading />
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return children
}

export function RouteLoading() {
  return (
    <main className={styles.loadingPage} aria-label="Verificando sua sessão">
      <span className={styles.cloud} aria-hidden="true" />
      <p>Preparando seu espaço...</p>
    </main>
  )
}
