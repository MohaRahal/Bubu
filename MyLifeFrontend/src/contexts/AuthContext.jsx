import { useEffect, useMemo, useState } from 'react'
import * as authApi from '../services/authApi'
import { AuthContext } from './authContext'

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const controller = new AbortController()
    let isCurrentRequest = true

    authApi.getCurrentUser(controller.signal)
      .then((authenticatedUser) => {
        if (isCurrentRequest) setUser(authenticatedUser)
      })
      .catch((error) => {
        if (isCurrentRequest && error.name !== 'AbortError') {
          setUser(null)
        }
      })
      .finally(() => {
        if (isCurrentRequest) setIsLoading(false)
      })

    return () => {
      isCurrentRequest = false
      controller.abort()
    }
  }, [])

  const value = useMemo(() => ({
    user,
    isLoading,
    async login(credentials) {
      const authenticatedUser = await authApi.login(credentials)
      setUser(authenticatedUser)
      return authenticatedUser
    },
    async logout() {
      await authApi.logout()
      setUser(null)
    },
    updateUser(updatedUser) {
      setUser((currentUser) => currentUser ? { ...currentUser, ...updatedUser } : updatedUser)
    },
  }), [isLoading, user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
