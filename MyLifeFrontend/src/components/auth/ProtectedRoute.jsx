import { Navigate, useLocation } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import { BubuLoadingScreen } from '../layout/BubuLoadingScreen'

export function ProtectedRoute({ children }) {
  const location = useLocation()
  const { isLoading, user } = useAuth()

  if (isLoading) {
    return <BubuLoadingScreen />
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return children
}

export { BubuLoadingScreen as RouteLoading }
