import { lazy, Suspense } from 'react'
import { Route, Routes } from 'react-router'
import { ProtectedRoute, RouteLoading } from './components/auth/ProtectedRoute'
import { HomePage } from './pages/HomePage/HomePage'
import { LoginPage } from './pages/LoginPage/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage/NotFoundPage'
import { RegisterPage } from './pages/RegisterPage/RegisterPage'

const DashboardPage = lazy(() => import('./pages/DashboardPage/DashboardPage').then((module) => ({
  default: module.DashboardPage,
})))

function App() {
  return (
    <Routes>
      <Route index element={<HomePage />} />
      <Route path="login" element={<LoginPage />} />
      <Route path="cadastro" element={<RegisterPage />} />
      <Route
        path="dashboard"
        element={(
          <ProtectedRoute>
            <Suspense fallback={<RouteLoading />}>
              <DashboardPage />
            </Suspense>
          </ProtectedRoute>
        )}
      />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

export default App
