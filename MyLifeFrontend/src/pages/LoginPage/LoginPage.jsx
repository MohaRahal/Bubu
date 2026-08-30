import { lazy, Suspense, useEffect, useState } from 'react'
import { ArrowLeft, ArrowRight, Eye, EyeOff, LockKeyhole, Mail } from 'lucide-react'
import { Link, useLocation, useNavigate } from 'react-router'
import { BrandMark } from '../../components/brand/BrandMark'
import { BubuLoadingScreen } from '../../components/layout/BubuLoadingScreen'
import { useAuth } from '../../hooks/useAuth'
import styles from './LoginPage.module.css'

const AuthMascot = lazy(() =>
  import('../../components/auth/AuthMascot').then((module) => ({
    default: module.AuthMascot,
  })),
)

export function LoginPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const { login } = useAuth()
  const [showPassword, setShowPassword] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isLoginComplete, setIsLoginComplete] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const previousTitle = document.title
    document.title = 'Entrar | Bubu'

    return () => {
      document.title = previousTitle
    }
  }, [])

  const handleSubmit = async (event) => {
    event.preventDefault()
    const formData = new FormData(event.currentTarget)

    setIsSubmitting(true)
    setErrorMessage('')

    try {
      await login({
        email: formData.get('email'),
        password: formData.get('password'),
      })

      setIsLoginComplete(true)

      const transitionTime = window.matchMedia('(prefers-reduced-motion: reduce)').matches
        ? 120
        : 950

      await new Promise((resolve) => window.setTimeout(resolve, transitionTime))
      navigate(location.state?.from ?? '/dashboard', { replace: true })
    } catch (error) {
      setErrorMessage(
        error instanceof TypeError
          ? 'Não foi possível acessar a API. Confirme se o backend está rodando.'
          : error.message,
      )
      setIsSubmitting(false)
    }
  }

  return (
    <main className={styles.page}>
      {isLoginComplete && (
        <BubuLoadingScreen />
      )}

      <section className={styles.welcome} aria-label="Boas-vindas do Bubu">
        <div className={styles.pattern} aria-hidden="true" />

        <Link className={styles.backLink} to="/">
          <ArrowLeft size={17} />
          Voltar para o início
        </Link>

        <div className={styles.welcomeCopy}>
          <span className={styles.eyebrow}>SEU CANTINHO ESTÁ AQUI</span>
          <h1>
            Que bom ter você <em>de volta.</em>
          </h1>
          <p>
            Entre para organizar seus dias, acompanhar seus objetivos e cuidar da sua vida
            financeira em um só lugar.
          </p>
        </div>

        <div className={styles.mascotStage} aria-hidden="true">
          <span className={styles.orbit} />
          <span className={styles.sparkleOne}>✦</span>
          <span className={styles.sparkleTwo}>✦</span>
          <div className={styles.message}>Eu guardei tudo para você!</div>
          <Suspense fallback={<div className={styles.mascotPlaceholder} />}>
            <AuthMascot className={styles.mascotRive} />
          </Suspense>
        </div>

        <p className={styles.sideNote}>Um passo de cada vez. O Bubu vai com você.</p>
      </section>

      <section className={styles.formSide} aria-labelledby="login-title">
        <header className={styles.mobileHeader}>
          <Link to="/" aria-label="Ir para a página inicial">
            <BrandMark />
          </Link>
        </header>

        <div className={styles.formWrapper}>
          <div className={styles.formHeading}>
            <span className={styles.miniMark}>✦</span>
            <h2 id="login-title">Entre na sua conta</h2>
            <p>Seu dia fica mais leve quando tudo está no lugar.</p>
          </div>

          {location.state?.accountCreated && (
            <p className={styles.successNotice} role="status">
              <span>✓</span> Sua conta foi criada. Agora é só entrar!
            </p>
          )}

          <form className={styles.form} onSubmit={handleSubmit}>
            <div className={styles.field}>
              <label htmlFor="email">E-mail</label>
              <div className={styles.inputWrapper}>
                <Mail size={19} aria-hidden="true" />
                <input
                  id="email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  defaultValue={location.state?.email ?? ''}
                  placeholder="voce@exemplo.com"
                  disabled={isSubmitting}
                  required
                />
              </div>
            </div>

            <div className={styles.field}>
              <div className={styles.labelRow}>
                <label htmlFor="password">Senha</label>
                <span className={styles.comingSoon}>Recuperação em breve</span>
              </div>

              <div className={styles.inputWrapper}>
                <LockKeyhole size={19} aria-hidden="true" />
                <input
                  id="password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  placeholder="Digite sua senha"
                  minLength={8}
                  disabled={isSubmitting}
                  required
                />
                <button
                  className={styles.passwordToggle}
                  type="button"
                  aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
                  aria-pressed={showPassword}
                  onClick={() => setShowPassword((current) => !current)}
                >
                  {showPassword ? <EyeOff size={19} /> : <Eye size={19} />}
                </button>
              </div>
            </div>

            {errorMessage && (
              <p className={styles.formError} role="alert">
                {errorMessage}
              </p>
            )}

            <button
              className={`${styles.submitButton} ${styles.loginButton}`}
              type="submit"
              disabled={isSubmitting}
            >
              <span className={styles.buttonGlow} aria-hidden="true" />
              <span className={styles.buttonLabel}>
                {isSubmitting && <span className={styles.spinner} />}
                {isSubmitting ? 'Validando acesso...' : 'Entrar na minha conta'}
              </span>
              {!isSubmitting && <ArrowRight className={styles.buttonArrow} size={19} />}
            </button>
          </form>

          <p className={styles.signUp}>
            Ainda não tem uma conta? <Link to="/cadastro">Começar agora</Link>
          </p>
        </div>

        <p className={styles.legal}>
          Ao continuar, você concorda com nossos <a href="#termos">Termos</a> e nossa{' '}
          <a href="#privacidade">Política de Privacidade</a>.
        </p>
      </section>
    </main>
  )
}
