import { Navigate, NavLink, Outlet, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth'
import Cadastro from './pages/Cadastro'
import Lancamentos from './pages/Lancamentos'
import Login from './pages/Login'
import Relatorios from './pages/Relatorios'

function AreaLogada() {
  const { logado, email, sair } = useAuth()
  if (!logado) return <Navigate to="/login" replace />

  return (
    <div className="app">
      <header className="topo">
        <strong className="marca">FinTrack</strong>
        <nav className="abas">
          <NavLink to="/" end>Início</NavLink>
          <NavLink to="/lancamentos">Lançamentos</NavLink>
        </nav>
        <span className="usuario">{email}</span>
        <button className="secundario" onClick={sair}>Sair</button>
      </header>
      <main className="conteudo">
        <Outlet />
      </main>
    </div>
  )
}

function SoDeslogado({ children }: { children: React.ReactNode }) {
  const { logado } = useAuth()
  return logado ? <Navigate to="/" replace /> : children
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<SoDeslogado><Login /></SoDeslogado>} />
      <Route path="/cadastro" element={<SoDeslogado><Cadastro /></SoDeslogado>} />
      <Route element={<AreaLogada />}>
        <Route path="/" element={<Relatorios />} />
        <Route path="/lancamentos" element={<Lancamentos />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
