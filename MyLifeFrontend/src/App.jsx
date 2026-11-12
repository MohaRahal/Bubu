import { lazy, Suspense } from 'react'
import { AnimatePresence, motion, useReducedMotion } from 'motion/react'
import { Route, Routes, useLocation } from 'react-router'
import { ProtectedRoute, RouteLoading } from './components/auth/ProtectedRoute'
import { HomePage } from './pages/HomePage/HomePage'
import { LoginPage } from './pages/LoginPage/LoginPage'
import { ForgotPasswordPage } from './pages/ForgotPasswordPage/ForgotPasswordPage'
import { NotFoundPage } from './pages/NotFoundPage/NotFoundPage'
import { RegisterPage } from './pages/RegisterPage/RegisterPage'

const DashboardPage = lazy(() => import('./pages/DashboardPage/DashboardPage').then((module) => ({
  default: module.DashboardPage,
})))

function App() {
  const location = useLocation()
  const shouldReduceMotion = useReducedMotion()
  const routeKey = location.pathname.startsWith('/dashboard')
    ? '/dashboard'
    : location.pathname

  return (
    <AnimatePresence mode="wait" initial={false}>
      <motion.div
        key={routeKey}
        initial={shouldReduceMotion ? false : { opacity: 0, y: 14, filter: 'blur(5px)' }}
        animate={{ opacity: 1, y: 0, filter: 'blur(0px)' }}
        exit={shouldReduceMotion ? { opacity: 1 } : { opacity: 0, y: -8, filter: 'blur(3px)' }}
        transition={{ duration: shouldReduceMotion ? 0 : 0.34, ease: [0.22, 1, 0.36, 1] }}
        style={{ minHeight: '100vh' }}
      >
        <Routes location={location}>
          <Route index element={<HomePage />} />
          <Route path="login" element={<LoginPage />} />
          <Route path="esqueci-minha-senha" element={<ForgotPasswordPage />} />
          <Route path="cadastro" element={<RegisterPage />} />
          <Route
            path="dashboard/*"
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
      </motion.div>
    </AnimatePresence>
  )
}

export default App
