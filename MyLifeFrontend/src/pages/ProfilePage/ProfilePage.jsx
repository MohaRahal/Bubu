import { useEffect, useState } from 'react'
import {
  CalendarDays,
  Check,
  Eye,
  EyeOff,
  Image,
  KeyRound,
  LoaderCircle,
  Mail,
  Save,
  ShieldCheck,
  Sparkles,
  UserRound,
} from 'lucide-react'
import bubuImage from '../../assets/bubu-3d.png'
import { useAuth } from '../../hooks/useAuth'
import { changePassword, getAvatars, getProfile, updateProfile } from '../../services/profileApi'
import styles from './ProfilePage.module.css'

const emptyPasswordForm = {
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
}

export function ProfilePage() {
  const { updateUser } = useAuth()
  const [profile, setProfile] = useState(null)
  const [avatars, setAvatars] = useState([])
  const [form, setForm] = useState({ name: '', email: '', avatarKey: '', birthDate: '' })
  const [passwordForm, setPasswordForm] = useState(emptyPasswordForm)
  const [showPasswords, setShowPasswords] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isChangingPassword, setIsChangingPassword] = useState(false)
  const [profileMessage, setProfileMessage] = useState(null)
  const [passwordMessage, setPasswordMessage] = useState(null)

  useEffect(() => {
    const controller = new AbortController()

    Promise.all([getProfile(controller.signal), getAvatars(controller.signal)])
      .then(([data, availableAvatars]) => {
        setProfile(data)
        setAvatars(availableAvatars)
        setForm({
          name: data.name ?? '',
          email: data.email ?? '',
          avatarKey: data.avatarUrl ? decodeURIComponent(data.avatarUrl.split('/').pop()) : '',
          birthDate: data.birthDate ?? '',
        })
      })
      .catch((error) => {
        if (error.name !== 'AbortError') setProfileMessage({ type: 'error', text: error.message })
      })
      .finally(() => setIsLoading(false))

    return () => controller.abort()
  }, [])

  function handleFieldChange(event) {
    const { name, value } = event.target
    setForm((current) => ({ ...current, [name]: value }))
    setProfileMessage(null)
  }

  function handlePasswordChange(event) {
    const { name, value } = event.target
    setPasswordForm((current) => ({ ...current, [name]: value }))
    setPasswordMessage(null)
  }

  async function handleProfileSubmit(event) {
    event.preventDefault()
    setIsSaving(true)
    setProfileMessage(null)

    try {
      const updatedProfile = await updateProfile({
        name: form.name.trim(),
        email: form.email.trim(),
        avatarKey: form.avatarKey || null,
        birthDate: form.birthDate || null,
      })

      setProfile(updatedProfile)
      updateUser(updatedProfile)
      setProfileMessage({ type: 'success', text: 'Seu perfil foi atualizado.' })
    } catch (error) {
      setProfileMessage({ type: 'error', text: error.message })
    } finally {
      setIsSaving(false)
    }
  }

  async function handlePasswordSubmit(event) {
    event.preventDefault()
    setPasswordMessage(null)

    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setPasswordMessage({ type: 'error', text: 'A confirmação da nova senha não confere.' })
      return
    }

    setIsChangingPassword(true)

    try {
      await changePassword({
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
      })
      setPasswordForm(emptyPasswordForm)
      setPasswordMessage({ type: 'success', text: 'Senha alterada com segurança.' })
    } catch (error) {
      setPasswordMessage({ type: 'error', text: error.message })
    } finally {
      setIsChangingPassword(false)
    }
  }

  if (isLoading) {
    return (
      <div className={styles.loadingState} aria-label="Carregando perfil">
        <img src={bubuImage} alt="Bubu" />
        <LoaderCircle size={22} />
        <span>Preparando seu espaço...</span>
      </div>
    )
  }

  const selectedAvatar = avatars.find((avatar) => avatar.key === form.avatarKey)
  const avatarPreview = selectedAvatar?.url ?? profile?.avatarUrl ?? bubuImage

  return (
    <div className={styles.page}>
      <header className={styles.heading}>
        <div>
          <span>
            <Sparkles size={14} /> SEU ESPAÇO
          </span>
          <h1>Perfil e preferências</h1>
          <p>Mantenha suas informações atualizadas e sua conta protegida.</p>
        </div>
        {profile?.createdAt && (
          <small>
            <CalendarDays size={15} /> Com o Bubu desde{' '}
            {new Intl.DateTimeFormat('pt-BR', { month: 'long', year: 'numeric' }).format(
              new Date(profile.createdAt),
            )}
          </small>
        )}
      </header>

      <div className={styles.profileGrid}>
        <aside className={styles.identityCard}>
          <div className={styles.avatarFrame}>
            <span />
            <img
              src={avatarPreview}
              alt={`Avatar de ${form.name || 'usuário'}`}
              onError={(event) => {
                event.currentTarget.src = bubuImage
              }}
            />
          </div>
          <span className={styles.activeBadge}>
            <i /> Conta ativa
          </span>
          <h2>{form.name || 'Seu nome'}</h2>
          <p>{form.email || 'seu@email.com'}</p>
          <div className={styles.bubuNote}>
            <Sparkles size={17} />
            <span>
              <strong>Dica do Bubu</strong>Use uma foto que faça este espaço parecer ainda mais seu.
            </span>
          </div>
        </aside>

        <div className={styles.formsColumn}>
          <form className={styles.formCard} onSubmit={handleProfileSubmit}>
            <header>
              <span>
                <UserRound size={20} />
              </span>
              <div>
                <h2>Informações pessoais</h2>
                <p>Os dados usados para identificar sua conta.</p>
              </div>
            </header>

            <div className={styles.fieldsGrid}>
              <label>
                <span>Nome completo</span>
                <div>
                  <UserRound size={17} />
                  <input
                    name="name"
                    value={form.name}
                    onChange={handleFieldChange}
                    minLength={2}
                    maxLength={120}
                    required
                  />
                </div>
              </label>
              <label>
                <span>E-mail</span>
                <div>
                  <Mail size={17} />
                  <input
                    name="email"
                    type="email"
                    value={form.email}
                    onChange={handleFieldChange}
                    maxLength={255}
                    required
                  />
                </div>
              </label>
              <label>
                <span>Data de nascimento</span>
                <div>
                  <CalendarDays size={17} />
                  <input
                    name="birthDate"
                    type="date"
                    value={form.birthDate}
                    max={new Date().toISOString().slice(0, 10)}
                    onChange={handleFieldChange}
                  />
                </div>
              </label>
              <fieldset className={styles.avatarPicker}>
                <legend>
                  <Image size={16} /> Escolha seu Bubu
                </legend>
                <div>
                  {avatars.map((avatar) => (
                    <button
                      className={form.avatarKey === avatar.key ? styles.selectedAvatar : undefined}
                      type="button"
                      key={avatar.key}
                      aria-pressed={form.avatarKey === avatar.key}
                      onClick={() => {
                        setForm((current) => ({ ...current, avatarKey: avatar.key }))
                        setProfileMessage(null)
                      }}
                    >
                      <span>
                        <img src={avatar.url} alt="" />
                      </span>
                      <strong>{avatar.name}</strong>
                      {form.avatarKey === avatar.key && (
                        <i>
                          <Check size={13} />
                        </i>
                      )}
                    </button>
                  ))}
                </div>
                <small>Você só pode escolher avatares oficiais disponibilizados pelo Bubu.</small>
              </fieldset>
            </div>

            <FormFooter
              message={profileMessage}
              isLoading={isSaving}
              loadingText="Salvando..."
              buttonText="Salvar alterações"
              icon={Save}
            />
          </form>

          <form className={styles.formCard} onSubmit={handlePasswordSubmit}>
            <header>
              <span>
                <ShieldCheck size={20} />
              </span>
              <div>
                <h2>Segurança</h2>
                <p>Troque sua senha sempre que precisar.</p>
              </div>
            </header>

            <div className={styles.passwordGrid}>
              <PasswordField
                label="Senha atual"
                name="currentPassword"
                value={passwordForm.currentPassword}
                onChange={handlePasswordChange}
                visible={showPasswords}
              />
              <PasswordField
                label="Nova senha"
                name="newPassword"
                value={passwordForm.newPassword}
                onChange={handlePasswordChange}
                visible={showPasswords}
              />
              <PasswordField
                label="Confirmar nova senha"
                name="confirmPassword"
                value={passwordForm.confirmPassword}
                onChange={handlePasswordChange}
                visible={showPasswords}
              />
            </div>

            <button
              className={styles.visibilityButton}
              type="button"
              onClick={() => setShowPasswords((current) => !current)}
            >
              {showPasswords ? <EyeOff size={16} /> : <Eye size={16} />}
              {showPasswords ? 'Ocultar senhas' : 'Mostrar senhas'}
            </button>

            <FormFooter
              message={passwordMessage}
              isLoading={isChangingPassword}
              loadingText="Alterando..."
              buttonText="Alterar senha"
              icon={KeyRound}
            />
          </form>
        </div>
      </div>
    </div>
  )
}

function PasswordField({ label, name, value, onChange, visible }) {
  return (
    <label>
      <span>{label}</span>
      <div>
        <KeyRound size={17} />
        <input
          name={name}
          type={visible ? 'text' : 'password'}
          value={value}
          onChange={onChange}
          minLength={8}
          maxLength={100}
          autoComplete={name === 'currentPassword' ? 'current-password' : 'new-password'}
          required
        />
      </div>
    </label>
  )
}

function FormFooter({ message, isLoading, loadingText, buttonText, icon: Icon }) {
  return (
    <footer className={styles.formFooter}>
      <div
        className={message?.type === 'error' ? styles.errorMessage : styles.successMessage}
        role={message ? 'status' : undefined}
      >
        {message && (
          <>
            <Check size={15} /> {message.text}
          </>
        )}
      </div>
      <button type="submit" disabled={isLoading}>
        {isLoading ? <LoaderCircle className={styles.spinner} size={17} /> : <Icon size={17} />}
        {isLoading ? loadingText : buttonText}
      </button>
    </footer>
  )
}
