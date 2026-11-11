
export type Tipo = 'Receita' | 'Despesa'

export interface Token {
  accessToken: string
  tokenType: string
  expiraEm: string
}

export interface Categoria {
  id: number
  nome: string
  tipo: Tipo
}

export interface Conta {
  id: number
  nome: string
  saldoInicial: number
}

export interface ContaSaldo {
  contaId: number
  contaNome: string
  saldoInicial: number
  totalReceitas: number
  totalDespesas: number
  saldoAtual: number
}

export interface Transacao {
  id: number
  descricao: string
  valor: number
  data: string
  tipo: Tipo
  categoriaId: number
  categoriaNome: string
  contaId: number
  contaNome: string
}

export interface TransacaoForm {
  descricao: string
  valor: number
  data: string
  tipo: Tipo
  categoriaId: number
  contaId: number
}

export interface Pagina<T> {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export interface RelatorioCategoria {
  categoriaId: number
  categoria: string
  total: number
  percentual: number
}

export interface RelatorioMes {
  mes: number
  receitas: number
  despesas: number
  saldo: number
}

export interface RelatorioMensal {
  ano: number
  mes: number
  totalReceitas: number
  totalDespesas: number
  saldo: number
  despesasPorCategoria: RelatorioCategoria[]
  receitasPorCategoria: RelatorioCategoria[]
}

export interface RelatorioAnual extends Omit<RelatorioMensal, 'mes'> {
  meses: RelatorioMes[]
}

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

const CHAVE_TOKEN = 'fintrack.token'

export const tokenStorage = {
  get: () => localStorage.getItem(CHAVE_TOKEN),
  set: (token: string) => localStorage.setItem(CHAVE_TOKEN, token),
  clear: () => localStorage.removeItem(CHAVE_TOKEN),
}

let aoExpirar: () => void = () => {}
export const definirAoExpirar = (fn: () => void) => {
  aoExpirar = fn
}

async function mensagemDeErro(resposta: Response): Promise<string> {
  try {
    const problema = await resposta.json()
    if (problema.errors) {
      return Object.values(problema.errors as Record<string, string[]>).flat().join(' ')
    }
    return problema.detail ?? problema.title ?? `Erro ${resposta.status}`
  } catch {
    return `Erro ${resposta.status}`
  }
}

async function request<T>(metodo: string, rota: string, corpo?: unknown): Promise<T> {
  const token = tokenStorage.get()
  const headers: Record<string, string> = {}
  if (corpo !== undefined) headers['Content-Type'] = 'application/json'
  if (token) headers.Authorization = `Bearer ${token}`

  const resposta = await fetch(rota, {
    method: metodo,
    headers,
    body: corpo !== undefined ? JSON.stringify(corpo) : undefined,
  })

  if (resposta.status === 401 && token) {
    aoExpirar()
  }
  if (!resposta.ok) {
    throw new ApiError(resposta.status, await mensagemDeErro(resposta))
  }
  return resposta.status === 204 ? (undefined as T) : resposta.json()
}

const query = (params: Record<string, string | number | undefined>) => {
  const qs = new URLSearchParams()
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== '') qs.set(k, String(v))
  })
  return qs.toString()
}

export const api = {
  registrar: (email: string, senha: string) =>
    request<{ id: number; email: string }>('POST', '/api/auth/registrar', { email, senha }),
  login: (email: string, senha: string) => request<Token>('POST', '/api/auth/login', { email, senha }),

  categorias: () => request<Categoria[]>('GET', '/api/categorias'),
  contas: () => request<Conta[]>('GET', '/api/contas'),
  saldo: (contaId: number) => request<ContaSaldo>('GET', `/api/contas/${contaId}/saldo`),

  transacoes: (filtro: { tipo?: string; dataInicio?: string; dataFim?: string; page?: number; pageSize?: number }) =>
    request<Pagina<Transacao>>('GET', `/api/transacoes?${query(filtro)}`),
  criarTransacao: (dados: TransacaoForm) => request<Transacao>('POST', '/api/transacoes', dados),
  atualizarTransacao: (id: number, dados: TransacaoForm) => request<Transacao>('PUT', `/api/transacoes/${id}`, dados),
  excluirTransacao: (id: number) => request<void>('DELETE', `/api/transacoes/${id}`),

  relatorioMensal: (ano: number, mes: number) =>
    request<RelatorioMensal>('GET', `/api/relatorios/mensal?${query({ ano, mes })}`),
  relatorioAnual: (ano: number) => request<RelatorioAnual>('GET', `/api/relatorios/anual?${query({ ano })}`),
}

export const moeda = (valor: number) => valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })

export const hoje = () => {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

export const paraApi = (dia: string) => `${dia}T00:00:00Z`
export const doApi = (data: string) => data.slice(0, 10)
export const dataBr = (data: string) => doApi(data).split('-').reverse().join('/')

export const NOMES_MESES = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]
