import { lazy, Suspense, useEffect, useState } from 'react'
import { ArrowLeft, ArrowRight, Check, Eye, EyeOff, KeyRound, LockKeyhole, Mail } from 'lucide-react'
import { Link, useNavigate } from 'react-router'
import { BrandMark } from '../../components/brand/BrandMark'
import {
  completePasswordReset,
  requestPasswordReset,
  validatePasswordResetCode,
} from '../../services/authApi'
import authStyles from '../LoginPage/LoginPage.module.css'
import styles from './ForgotPasswordPage.module.css'

const AuthMascot = lazy(() =>
  import('../../components/auth/AuthMascot').then((module) => ({
    default: module.AuthMascot,
  })),
)

const apiUnavailableMessage =
  'Não foi possível acessar a API. Confirme se o backend está rodando.'

export function ForgotPasswordPage() {
  const navigate = useNavigate()
  const [step, setStep] = useState('request')
  const [email, setEmail] = useState('')
  const [resetToken, setResetToken] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [showPassword, setShowPassword] = useState(false)
  const [message, setMessage] = useState('')
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    const previousTitle = document.title
    document.title = 'Recuperar senha | Bubu'
    return () => { document.title = previousTitle }
  }, [])

  const showError = (error) => {
    setErrorMessage(error instanceof TypeError ? apiUnavailableMessage : error.message)
  }

  const handleRequest = async (event) => {
    event.preventDefault()
    const requestedEmail = String(new FormData(event.currentTarget).get('email')).trim()
    setIsSubmitting(true)
    setErrorMessage('')
    setMessage('')

    try {
      const response = await requestPasswordReset(requestedEmail)
      setEmail(requestedEmail)
      setMessage(`Enviamos um código de 6 dígitos. Ele vale por ${response.codeLifetimeMinutes} minutos.`)
      setStep('validate')
    } catch (error) {
      showError(error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleValidate = async (event) => {
    event.preventDefault()
    const code = String(new FormData(event.currentTarget).get('code')).replace(/\D/g, '')
    setIsSubmitting(true)
    setErrorMessage('')

    try {
      const response = await validatePasswordResetCode({ email, code })
      setResetToken(response.resetToken)
      setStep('password')
    } catch (error) {
      showError(error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleComplete = async (event) => {
    event.preventDefault()
    const formData = new FormData(event.currentTarget)
    const newPassword = String(formData.get('newPassword'))
    const passwordConfirmation = String(formData.get('passwordConfirmation'))

    if (newPassword !== passwordConfirmation) {
      setErrorMessage('As senhas precisam ser iguais.')
      return
    }

    setIsSubmitting(true)
    setErrorMessage('')
    try {
      await completePasswordReset({ email, resetToken, newPassword })
      setStep('success')
    } catch (error) {
      showError(error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleResend = async () => {
    setIsSubmitting(true)
    setErrorMessage('')
    try {
      const response = await requestPasswordReset(email)
      setMessage(`Enviamos um novo código. Ele vale por ${response.codeLifetimeMinutes} minutos.`)
    } catch (error) {
      showError(error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const buttonContent = (loadingText, label) => (
    <>
      <span className={authStyles.buttonGlow} aria-hidden="true" />
      <span className={authStyles.buttonLabel}>
        {isSubmitting && <span className={authStyles.spinner} />}
        {isSubmitting ? loadingText : label}
      </span>
      {!isSubmitting && <ArrowRight className={authStyles.buttonArrow} size={19} />}
    </>
  )

  return (
    <main className={authStyles.page}>
      <section className={authStyles.welcome} aria-label="Recuperação de senha com o Bubu">
        <div className={authStyles.pattern} aria-hidden="true" />
        <Link className={authStyles.backLink} to="/login">
          <ArrowLeft size={17} /> Voltar para entrar
        </Link>

        <div className={authStyles.welcomeCopy}>
          <span className={authStyles.eyebrow}>RECUPERAÇÃO SEGURA</span>
          <h1>Vamos cuidar do seu <em>acesso.</em></h1>
          <p>Confirme o código enviado ao seu e-mail e escolha uma nova senha com segurança.</p>
        </div>

        <div className={authStyles.mascotStage} aria-hidden="true">
          <span className={authStyles.orbit} />
          <span className={authStyles.sparkleOne}>✦</span>
          <span className={authStyles.sparkleTwo}>✦</span>
          <div className={authStyles.message}>Eu ajudo você a voltar!</div>
          <Suspense fallback={<div className={authStyles.mascotPlaceholder} />}>
            <AuthMascot className={authStyles.mascotRive} />
          </Suspense>
        </div>

        <p className={authStyles.sideNote}>Seu código é temporário e protegido.</p>
      </section>

      <section className={authStyles.formSide} aria-labelledby="reset-title">
        <header className={authStyles.mobileHeader}>
          <Link to="/" aria-label="Ir para a página inicial"><BrandMark /></Link>
        </header>

        <div className={authStyles.formWrapper}>
          {step === 'request' && <>
            <div className={authStyles.formHeading}>
              <span className={authStyles.miniMark}><Mail size={19} /></span>
              <h2 id="reset-title">Recupere sua senha</h2>
              <p>Informe o e-mail usado na sua conta Bubu.</p>
            </div>
            <form className={authStyles.form} onSubmit={handleRequest}>
              <div className={authStyles.field}>
                <label htmlFor="reset-email">E-mail</label>
                <div className={authStyles.inputWrapper}>
                  <Mail size={19} aria-hidden="true" />
                  <input id="reset-email" name="email" type="email" autoComplete="email" placeholder="voce@exemplo.com" required disabled={isSubmitting} />
                </div>
              </div>
              {errorMessage && <p className={authStyles.formError} role="alert">{errorMessage}</p>}
              <button className={`${authStyles.submitButton} ${authStyles.loginButton}`} type="submit" disabled={isSubmitting}>
                {buttonContent('Enviando código...', 'Enviar código')}
              </button>
            </form>
          </>}

          {step === 'validate' && <>
            <div className={authStyles.formHeading}>
              <span className={authStyles.miniMark}><KeyRound size={19} /></span>
              <h2 id="reset-title">Confirme o código</h2>
              <p>Digite o código enviado para <strong>{email}</strong>.</p>
            </div>
            {message && <p className={styles.notice} role="status">{message}</p>}
            <form className={authStyles.form} onSubmit={handleValidate}>
              <div className={authStyles.field}>
                <label htmlFor="reset-code">Código de 6 dígitos</label>
                <div className={`${authStyles.inputWrapper} ${styles.codeInput}`}>
                  <KeyRound size={19} aria-hidden="true" />
                  <input id="reset-code" name="code" type="text" inputMode="numeric" autoComplete="one-time-code" pattern="[0-9]{6}" maxLength={6} placeholder="000000" required autoFocus disabled={isSubmitting} />
                </div>
              </div>
              {errorMessage && <p className={authStyles.formError} role="alert">{errorMessage}</p>}
              <button className={`${authStyles.submitButton} ${authStyles.loginButton}`} type="submit" disabled={isSubmitting}>
                {buttonContent('Validando código...', 'Validar código')}
              </button>
            </form>
            <button className={styles.secondaryAction} type="button" onClick={handleResend} disabled={isSubmitting}>Não recebeu? Enviar novamente</button>
          </>}

          {step === 'password' && <>
            <div className={authStyles.formHeading}>
              <span className={authStyles.miniMark}><LockKeyhole size={19} /></span>
              <h2 id="reset-title">Crie uma nova senha</h2>
              <p>Código confirmado. Agora escolha uma senha segura.</p>
            </div>
            <form className={authStyles.form} onSubmit={handleComplete}>
              <PasswordField id="new-password" name="newPassword" label="Nova senha" placeholder="Mínimo de 8 caracteres" showPassword={showPassword} setShowPassword={setShowPassword} disabled={isSubmitting} />
              <PasswordField id="password-confirmation" name="passwordConfirmation" label="Confirme a nova senha" placeholder="Repita a nova senha" showPassword={showPassword} disabled={isSubmitting} />
              {errorMessage && <p className={authStyles.formError} role="alert">{errorMessage}</p>}
              <button className={`${authStyles.submitButton} ${authStyles.loginButton}`} type="submit" disabled={isSubmitting}>
                {buttonContent('Salvando senha...', 'Criar nova senha')}
              </button>
            </form>
          </>}

          {step === 'success' && <div className={`${authStyles.formHeading} ${styles.success}`}>
            <span className={styles.successIcon}><Check size={28} /></span>
            <h2 id="reset-title">Senha redefinida!</h2>
            <p>Sua nova senha já está ativa. Agora você pode entrar novamente.</p>
            <button className={`${authStyles.submitButton} ${authStyles.loginButton} ${styles.successButton}`} type="button" onClick={() => navigate('/login', { state: { email } })}>
              <span className={authStyles.buttonLabel}>Voltar para o login</span><ArrowRight size={19} />
            </button>
          </div>}

          {step !== 'success' && <p className={authStyles.signUp}>Lembrou sua senha? <Link to="/login">Voltar para entrar</Link></p>}
        </div>

        <p className={authStyles.legal}>Seu código expira automaticamente e só pode ser usado uma vez.</p>
      </section>
    </main>
  )
}

function PasswordField({ id, name, label, placeholder, showPassword, setShowPassword, disabled }) {
  return (
    <div className={authStyles.field}>
      <label htmlFor={id}>{label}</label>
      <div className={authStyles.inputWrapper}>
        <LockKeyhole size={19} aria-hidden="true" />
        <input id={id} name={name} type={showPassword ? 'text' : 'password'} autoComplete="new-password" minLength={8} placeholder={placeholder} required disabled={disabled} />
        {setShowPassword && <button className={authStyles.passwordToggle} type="button" aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'} onClick={() => setShowPassword((current) => !current)}>
          {showPassword ? <EyeOff size={19} /> : <Eye size={19} />}
        </button>}
      </div>
    </div>
  )
}
