import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api'

export default function Cadastro() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [confirmacao, setConfirmacao] = useState('')
  const [erro, setErro] = useState('')
  const [enviando, setEnviando] = useState(false)

  const enviar = async (e: FormEvent) => {
    e.preventDefault()
    setErro('')
    if (senha !== confirmacao) {
      setErro('As senhas não conferem.')
      return
    }
    setEnviando(true)
    try {
      const usuario = await api.registrar(email, senha)
      navigate('/login', { state: { cadastrado: true, email: usuario.email } })
    } catch (err) {
      setErro((err as Error).message)
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="pagina-auth">
      <form className="cartao auth" onSubmit={enviar}>
        <h1>FinTrack</h1>
        <h2>Criar conta</h2>
        <label>
          E-mail
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required autoFocus />
        </label>
        <label>
          Senha <small>(mínimo de 8 caracteres)</small>
          <input type="password" value={senha} onChange={(e) => setSenha(e.target.value)} minLength={8} required />
        </label>
        <label>
          Confirmar senha
          <input type="password" value={confirmacao} onChange={(e) => setConfirmacao(e.target.value)} required />
        </label>
        {erro && <p className="erro">{erro}</p>}
        <button type="submit" disabled={enviando}>{enviando ? 'Cadastrando…' : 'Cadastrar'}</button>
        <p className="rodape">
          Já tem conta? <Link to="/login">Entrar</Link>
        </p>
      </form>
    </div>
  )
}
