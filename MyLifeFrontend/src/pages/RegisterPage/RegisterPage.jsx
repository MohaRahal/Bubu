import { lazy, Suspense, useEffect, useState } from 'react'
import {
  ArrowLeft,
  ArrowRight,
  CalendarDays,
  Check,
  Eye,
  EyeOff,
  LockKeyhole,
  Mail,
  UserRound,
} from 'lucide-react'
import { Link } from 'react-router'
import { useNavigate } from 'react-router'
import { BrandMark } from '../../components/brand/BrandMark'
import { createUser } from '../../services/usersApi'
import authStyles from '../LoginPage/LoginPage.module.css'
import styles from './RegisterPage.module.css'

const AuthMascot = lazy(() => import('../../components/auth/AuthMascot').then((module) => ({
  default: module.AuthMascot,
})))

export function RegisterPage() {
  const navigate = useNavigate()
  const [showPassword, setShowPassword] = useState(false)
  const [showPasswordConfirmation, setShowPasswordConfirmation] = useState(false)
  const [submitState, setSubmitState] = useState('idle')
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const previousTitle = document.title
    document.title = 'Criar conta | Bubu'

    return () => {
      document.title = previousTitle
    }
  }, [])

  const handleSubmit = async (event) => {
    event.preventDefault()

    const form = event.currentTarget
    const formData = new FormData(form)
    const password = formData.get('password')
    const passwordConfirmation = formData.get('passwordConfirmation')

    if (password !== passwordConfirmation) {
      setErrorMessage('As duas senhas precisam ser iguais.')
      form.elements.passwordConfirmation.focus()
      return
    }

    setSubmitState('loading')
    setErrorMessage('')

    try {
      await createUser({
        name: formData.get('name'),
        email: formData.get('email'),
        password,
        avatarUrl: null,
        birthDate: formData.get('birthDate') || null,
      })

      setSubmitState('success')

      window.setTimeout(() => {
        navigate('/login', {
          replace: true,
          state: {
            accountCreated: true,
            email: formData.get('email'),
          },
        })
      }, 1050)
    } catch (error) {
      setSubmitState('idle')
      setErrorMessage(
        error instanceof TypeError
          ? 'Não foi possível acessar a API. Confirme se o backend está rodando.'
          : error.message,
      )
    }
  }

  return (
    <main className={`${authStyles.page} ${styles.page}`}>
      <section className={authStyles.welcome} aria-label="Boas-vindas do Bubu">
        <div className={authStyles.pattern} aria-hidden="true" />

        <Link className={authStyles.backLink} to="/">
          <ArrowLeft size={17} />
          Voltar para o início
        </Link>

        <div className={authStyles.welcomeCopy}>
          <span className={authStyles.eyebrow}>COMECE DO SEU JEITO</span>
          <h1>Um lugar para chamar de <em>seu.</em></h1>
          <p>
            Crie sua conta e comece a transformar planos soltos em uma rotina
            mais clara, leve e possível.
          </p>
        </div>

        <div className={authStyles.mascotStage} aria-hidden="true">
          <span className={authStyles.orbit} />
          <span className={authStyles.sparkleOne}>✦</span>
          <span className={authStyles.sparkleTwo}>✦</span>
          <div className={authStyles.message}>Vamos começar juntos?</div>
          <Suspense fallback={<div className={authStyles.mascotPlaceholder} />}>
            <AuthMascot className={authStyles.mascotRive} />
          </Suspense>
        </div>

        <p className={authStyles.sideNote}>Sem pressa e sem cobrança. Só progresso real.</p>
      </section>

      <section
        className={`${authStyles.formSide} ${styles.formSide}`}
        aria-labelledby="register-title"
      >
        <header className={authStyles.mobileHeader}>
          <Link to="/" aria-label="Ir para a página inicial">
            <BrandMark />
          </Link>
        </header>

        <div className={`${authStyles.formWrapper} ${styles.formWrapper}`}>
          <div className={authStyles.formHeading}>
            <span className={authStyles.miniMark}>✦</span>
            <h2 id="register-title">Crie sua conta</h2>
            <p>Leva menos de dois minutos para começar.</p>
          </div>

          <form className={`${authStyles.form} ${styles.form}`} onSubmit={handleSubmit}>
            <div className={authStyles.field}>
              <label htmlFor="name">Como podemos chamar você?</label>
              <div className={authStyles.inputWrapper}>
                <UserRound size={19} aria-hidden="true" />
                <input
                  id="name"
                  name="name"
                  type="text"
                  autoComplete="name"
                  placeholder="Seu nome"
                  minLength={2}
                  maxLength={120}
                  disabled={submitState !== 'idle'}
                  required
                />
              </div>
            </div>

            <div className={authStyles.field}>
              <label htmlFor="register-email">E-mail</label>
              <div className={authStyles.inputWrapper}>
                <Mail size={19} aria-hidden="true" />
                <input
                  id="register-email"
                  name="email"
                  type="email"
                  autoComplete="email"
                  placeholder="voce@exemplo.com"
                  maxLength={255}
                  disabled={submitState !== 'idle'}
                  required
                />
              </div>
            </div>

            <div className={authStyles.field}>
              <div className={authStyles.labelRow}>
                <label htmlFor="birthDate">Data de nascimento</label>
                <span className={styles.optional}>Opcional</span>
              </div>
              <div className={authStyles.inputWrapper}>
                <CalendarDays size={19} aria-hidden="true" />
                <input
                  id="birthDate"
                  name="birthDate"
                  type="date"
                  autoComplete="bday"
                  disabled={submitState !== 'idle'}
                />
              </div>
            </div>

            <div className={styles.passwordGrid}>
              <div className={authStyles.field}>
                <label htmlFor="register-password">Senha</label>
                <div className={authStyles.inputWrapper}>
                  <LockKeyhole size={19} aria-hidden="true" />
                  <input
                    id="register-password"
                    name="password"
                    type={showPassword ? 'text' : 'password'}
                    autoComplete="new-password"
                    placeholder="Mínimo de 8 caracteres"
                    minLength={8}
                    maxLength={100}
                    disabled={submitState !== 'idle'}
                    required
                  />
                  <button
                    className={authStyles.passwordToggle}
                    type="button"
                    aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
                    aria-pressed={showPassword}
                    onClick={() => setShowPassword((current) => !current)}
                  >
                    {showPassword ? <EyeOff size={19} /> : <Eye size={19} />}
                  </button>
                </div>
              </div>

              <div className={authStyles.field}>
                <label htmlFor="passwordConfirmation">Confirme a senha</label>
                <div className={authStyles.inputWrapper}>
                  <LockKeyhole size={19} aria-hidden="true" />
                  <input
                    id="passwordConfirmation"
                    name="passwordConfirmation"
                    type={showPasswordConfirmation ? 'text' : 'password'}
                    autoComplete="new-password"
                    placeholder="Repita sua senha"
                    minLength={8}
                    maxLength={100}
                    disabled={submitState !== 'idle'}
                    required
                  />
                  <button
                    className={authStyles.passwordToggle}
                    type="button"
                    aria-label={showPasswordConfirmation ? 'Ocultar confirmação' : 'Mostrar confirmação'}
                    aria-pressed={showPasswordConfirmation}
                    onClick={() => setShowPasswordConfirmation((current) => !current)}
                  >
                    {showPasswordConfirmation ? <EyeOff size={19} /> : <Eye size={19} />}
                  </button>
                </div>
              </div>
            </div>

            <p className={styles.passwordHint}>
              <Check size={15} aria-hidden="true" /> Pelo menos 8 caracteres.
            </p>

            <label className={`${authStyles.remember} ${styles.terms}`}>
              <input
                name="termsAccepted"
                type="checkbox"
                disabled={submitState !== 'idle'}
                required
              />
              <span>
                Li e aceito os <a href="#termos">Termos de Uso</a> e a{' '}
                <a href="#privacidade">Política de Privacidade</a>.
              </span>
            </label>

            {errorMessage && (
              <p className={styles.formError} role="alert">{errorMessage}</p>
            )}

            <button
              className={`${authStyles.submitButton} ${styles.createButton} ${submitState === 'success' ? styles.success : ''}`}
              type="submit"
              disabled={submitState !== 'idle'}
            >
              <span className={styles.buttonGlow} aria-hidden="true" />
              <span className={styles.buttonLabel}>
                {submitState === 'loading' && <span className={styles.spinner} />}
                {submitState === 'success' && <Check size={20} />}
                {submitState === 'idle' && 'Criar minha conta'}
                {submitState === 'loading' && 'Criando seu espaço...'}
                {submitState === 'success' && 'Conta criada!'}
              </span>
              {submitState === 'idle' && <ArrowRight className={styles.buttonArrow} size={19} />}
            </button>
          </form>

          <p className={authStyles.signUp}>
            Já tem uma conta? <Link to="/login">Entrar agora</Link>
          </p>
        </div>
      </section>
    </main>
  )
}
