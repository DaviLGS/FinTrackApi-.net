import { useCallback, useEffect, useState, type FormEvent } from 'react'
import {
  api, dataBr, doApi, hoje, moeda, paraApi,
  type Categoria, type Conta, type ContaSaldo, type Pagina, type Tipo, type Transacao,
} from '../api'

interface Form {
  descricao: string
  valor: string
  data: string
  tipo: Tipo
  categoriaId: string
  contaId: string
}

const formVazio = (): Form => ({ descricao: '', valor: '', data: hoje(), tipo: 'Despesa', categoriaId: '', contaId: '' })

export default function Lancamentos() {
  const [categorias, setCategorias] = useState<Categoria[]>([])
  const [contas, setContas] = useState<Conta[]>([])
  const [saldos, setSaldos] = useState<ContaSaldo[]>([])
  const [pagina, setPagina] = useState<Pagina<Transacao> | null>(null)
  const [page, setPage] = useState(1)
  const [filtroTipo, setFiltroTipo] = useState('')
  const [form, setForm] = useState<Form>(formVazio)
  const [editandoId, setEditandoId] = useState<number | null>(null)
  const [erro, setErro] = useState('')
  const [enviando, setEnviando] = useState(false)

  useEffect(() => {
    api.categorias().then(setCategorias).catch((e: Error) => setErro(e.message))
    api.contas().then(setContas).catch((e: Error) => setErro(e.message))
  }, [])

  const carregar = useCallback(async () => {
    try {
      setPagina(await api.transacoes({ tipo: filtroTipo || undefined, page, pageSize: 10 }))
      setSaldos(await Promise.all(contas.map((c) => api.saldo(c.id))))
    } catch (e) {
      setErro((e as Error).message)
    }
  }, [filtroTipo, page, contas])

  useEffect(() => {
    carregar()
  }, [carregar])

  const categoriasDoTipo = categorias.filter((c) => c.tipo === form.tipo)
  const alterar = (campo: keyof Form, valor: string) =>
    setForm((f) => ({ ...f, [campo]: valor, ...(campo === 'tipo' ? { categoriaId: '' } : {}) }))

  const cancelar = () => {
    setForm(formVazio())
    setEditandoId(null)
    setErro('')
  }

  const enviar = async (e: FormEvent) => {
    e.preventDefault()
    setErro('')
    setEnviando(true)
    const dados = {
      descricao: form.descricao,
      valor: Number(form.valor),
      data: paraApi(form.data),
      tipo: form.tipo,
      categoriaId: Number(form.categoriaId),
      contaId: Number(form.contaId),
    }
    try {
      if (editandoId) await api.atualizarTransacao(editandoId, dados)
      else await api.criarTransacao(dados)
      cancelar()
      await carregar()
    } catch (err) {
      setErro((err as Error).message)
    } finally {
      setEnviando(false)
    }
  }

  const editar = (t: Transacao) => {
    setEditandoId(t.id)
    setErro('')
    setForm({
      descricao: t.descricao,
      valor: String(t.valor),
      data: doApi(t.data),
      tipo: t.tipo,
      categoriaId: String(t.categoriaId),
      contaId: String(t.contaId),
    })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const excluir = async (t: Transacao) => {
    if (!confirm(`Excluir "${t.descricao}" (${moeda(t.valor)})?`)) return
    try {
      await api.excluirTransacao(t.id)
      if (editandoId === t.id) cancelar()
      await carregar()
    } catch (err) {
      setErro((err as Error).message)
    }
  }

  return (
    <>
      <h2>Lançamentos</h2>

      <div className="resumo">
        {saldos.map((s) => (
          <div key={s.contaId} className="cartao indicador">
            <span>{s.contaNome}</span>
            <strong className={s.saldoAtual < 0 ? 'despesa' : 'receita'}>{moeda(s.saldoAtual)}</strong>
          </div>
        ))}
      </div>

      <form className="cartao form-lancamento" onSubmit={enviar}>
        <h3>{editandoId ? 'Editar lançamento' : 'Novo lançamento'}</h3>
        <div className="alternador">
          {(['Despesa', 'Receita'] as Tipo[]).map((t) => (
            <button type="button" key={t} className={form.tipo === t ? 'ativo' : ''} onClick={() => alterar('tipo', t)}>
              {t === 'Despesa' ? 'Gasto' : 'Entrada'}
            </button>
          ))}
        </div>
        <div className="grade">
          <label className="largo">
            Descrição
            <input value={form.descricao} onChange={(e) => alterar('descricao', e.target.value)} maxLength={200} required />
          </label>
          <label>
            Valor (R$)
            <input type="number" min="0.01" step="0.01" value={form.valor} onChange={(e) => alterar('valor', e.target.value)} required />
          </label>
          <label>
            Data
            <input type="date" value={form.data} max={hoje()} onChange={(e) => alterar('data', e.target.value)} required />
          </label>
          <label>
            Categoria
            <select value={form.categoriaId} onChange={(e) => alterar('categoriaId', e.target.value)} required>
              <option value="">Selecione…</option>
              {categoriasDoTipo.map((c) => (
                <option key={c.id} value={c.id}>{c.nome}</option>
              ))}
            </select>
          </label>
          <label>
            Conta
            <select value={form.contaId} onChange={(e) => alterar('contaId', e.target.value)} required>
              <option value="">Selecione…</option>
              {contas.map((c) => (
                <option key={c.id} value={c.id}>{c.nome}</option>
              ))}
            </select>
          </label>
        </div>
        {erro && <p className="erro">{erro}</p>}
        <div className="acoes">
          <button type="submit" disabled={enviando}>{editandoId ? 'Salvar alterações' : 'Adicionar'}</button>
          {editandoId && <button type="button" className="secundario" onClick={cancelar}>Cancelar</button>}
        </div>
      </form>

      <section className="cartao">
        <div className="barra-titulo">
          <h3>Histórico</h3>
          <select value={filtroTipo} onChange={(e) => { setFiltroTipo(e.target.value); setPage(1) }} aria-label="Filtrar por tipo">
            <option value="">Todos</option>
            <option value="Despesa">Só gastos</option>
            <option value="Receita">Só entradas</option>
          </select>
        </div>
        {pagina && pagina.items.length === 0 ? (
          <p className="vazio">Nenhum lançamento ainda.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Data</th>
                <th>Descrição</th>
                <th>Categoria</th>
                <th>Conta</th>
                <th className="num">Valor</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {pagina?.items.map((t) => (
                <tr key={t.id} className={editandoId === t.id ? 'selecionada' : ''}>
                  <td>{dataBr(t.data)}</td>
                  <td>{t.descricao}</td>
                  <td>{t.categoriaNome}</td>
                  <td>{t.contaNome}</td>
                  <td className={`num ${t.tipo === 'Despesa' ? 'despesa' : 'receita'}`}>
                    {t.tipo === 'Despesa' ? '−' : '+'} {moeda(t.valor)}
                  </td>
                  <td className="acoes-linha">
                    <button className="link" onClick={() => editar(t)}>Editar</button>
                    <button className="link perigo" onClick={() => excluir(t)}>Excluir</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {pagina && pagina.totalPages > 1 && (
          <div className="paginacao">
            <button className="secundario" disabled={page <= 1} onClick={() => setPage(page - 1)}>Anterior</button>
            <span>Página {pagina.page} de {pagina.totalPages}</span>
            <button className="secundario" disabled={page >= pagina.totalPages} onClick={() => setPage(page + 1)}>Próxima</button>
          </div>
        )}
      </section>
    </>
  )
}
