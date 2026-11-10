import { useEffect, useState } from 'react'
import { api, moeda, NOMES_MESES, type RelatorioAnual, type RelatorioCategoria, type RelatorioMensal } from '../api'

type Visao = 'mensal' | 'anual'

const AGORA = new Date()
const ANOS = Array.from({ length: 6 }, (_, i) => AGORA.getFullYear() - 4 + i)

function Resumo({ receitas, despesas, saldo }: { receitas: number; despesas: number; saldo: number }) {
  return (
    <div className="resumo">
      <div className="cartao indicador">
        <span>Entradas</span>
        <strong className="receita">{moeda(receitas)}</strong>
      </div>
      <div className="cartao indicador">
        <span>Gastos</span>
        <strong className="despesa">{moeda(despesas)}</strong>
      </div>
      <div className="cartao indicador">
        <span>Saldo</span>
        <strong className={saldo < 0 ? 'despesa' : 'receita'}>{moeda(saldo)}</strong>
      </div>
    </div>
  )
}

function PorCategoria({ titulo, itens, classe }: { titulo: string; itens: RelatorioCategoria[]; classe: string }) {
  return (
    <section className="cartao">
      <h3>{titulo}</h3>
      {itens.length === 0 ? (
        <p className="vazio">Nada no período.</p>
      ) : (
        <ul className="barras">
          {itens.map((c) => (
            <li key={c.categoriaId}>
              <div className="barra-legenda">
                <span>{c.categoria}</span>
                <span>
                  {moeda(c.total)} <small>({c.percentual.toLocaleString('pt-BR')}%)</small>
                </span>
              </div>
              <div className="barra-fundo">
                <div className={`barra ${classe}`} style={{ width: `${c.percentual}%` }} />
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

export default function Relatorios() {
  const [visao, setVisao] = useState<Visao>('mensal')
  const [ano, setAno] = useState(AGORA.getFullYear())
  const [mes, setMes] = useState(AGORA.getMonth() + 1)
  const [mensal, setMensal] = useState<RelatorioMensal | null>(null)
  const [anual, setAnual] = useState<RelatorioAnual | null>(null)
  const [erro, setErro] = useState('')

  useEffect(() => {
    const pedido = visao === 'mensal'
      ? api.relatorioMensal(ano, mes).then(setMensal)
      : api.relatorioAnual(ano).then(setAnual)
    pedido.then(() => setErro('')).catch((e: Error) => setErro(e.message))
  }, [visao, ano, mes])

  const relatorio = visao === 'mensal' ? mensal : anual

  return (
    <>
      <div className="barra-titulo">
        <h2>Relatórios</h2>
        <div className="filtros">
          <div className="alternador">
            <button className={visao === 'mensal' ? 'ativo' : ''} onClick={() => setVisao('mensal')}>Mensal</button>
            <button className={visao === 'anual' ? 'ativo' : ''} onClick={() => setVisao('anual')}>Anual</button>
          </div>
          {visao === 'mensal' && (
            <select value={mes} onChange={(e) => setMes(Number(e.target.value))} aria-label="Mês">
              {NOMES_MESES.map((nome, i) => (
                <option key={nome} value={i + 1}>{nome}</option>
              ))}
            </select>
          )}
          <select value={ano} onChange={(e) => setAno(Number(e.target.value))} aria-label="Ano">
            {ANOS.map((a) => (
              <option key={a} value={a}>{a}</option>
            ))}
          </select>
        </div>
      </div>

      {erro && <p className="erro">{erro}</p>}

      {relatorio && (
        <>
          <Resumo receitas={relatorio.totalReceitas} despesas={relatorio.totalDespesas} saldo={relatorio.saldo} />
          <div className="duas-colunas">
            <PorCategoria titulo="Gastos por categoria" itens={relatorio.despesasPorCategoria} classe="despesa" />
            <PorCategoria titulo="Entradas por categoria" itens={relatorio.receitasPorCategoria} classe="receita" />
          </div>
        </>
      )}

      {visao === 'anual' && anual && (
        <section className="cartao">
          <h3>Mês a mês em {anual.ano}</h3>
          <table>
            <thead>
              <tr>
                <th>Mês</th>
                <th className="num">Entradas</th>
                <th className="num">Gastos</th>
                <th className="num">Saldo</th>
              </tr>
            </thead>
            <tbody>
              {anual.meses.map((m) => (
                <tr key={m.mes}>
                  <td>{NOMES_MESES[m.mes - 1]}</td>
                  <td className="num receita">{moeda(m.receitas)}</td>
                  <td className="num despesa">{moeda(m.despesas)}</td>
                  <td className={`num ${m.saldo < 0 ? 'despesa' : ''}`}>{moeda(m.saldo)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      )}
    </>
  )
}
