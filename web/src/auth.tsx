import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api, definirAoExpirar, tokenStorage } from './api'

interface Sessao {
  email: string | null
  logado: boolean
  entrar: (email: string, senha: string) => Promise<void>
  sair: () => void
}

const AuthContext = createContext<Sessao | null>(null)

/** Lê o e-mail e a expiração direto do JWT (só para exibir; quem valida é a API). */
function lerToken(token: string | null): { email: string; exp: number } | null {
  if (!token) return null
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
    return { email: payload.email, exp: payload.exp }
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => {
    const salvo = tokenStorage.get()
    const dados = lerToken(salvo)
    return dados && dados.exp * 1000 > Date.now() ? salvo : null
  })

  const sair = () => {
    tokenStorage.clear()
    setToken(null)
  }

  useEffect(() => definirAoExpirar(sair), [])

  const entrar = async (email: string, senha: string) => {
    const resposta = await api.login(email, senha)
    tokenStorage.set(resposta.accessToken)
    setToken(resposta.accessToken)
  }

  return (
    <AuthContext.Provider value={{ email: lerToken(token)?.email ?? null, logado: token !== null, entrar, sair }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth precisa estar dentro de <AuthProvider>')
  return ctx
}
