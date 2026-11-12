import { useEffect, useMemo, useRef, useState } from 'react'
import { ArrowDownLeft, ArrowRightLeft, ArrowUpRight, CalendarDays, CheckCircle2, Download, LoaderCircle, Mail, PiggyBank, Printer, ReceiptText, X } from 'lucide-react'
import bubuImage from '../../assets/bubu-3d.png'
import { emailFinanceReport, getFinanceDashboard } from '../../services/financeApi'
import styles from './FinanceReportModal.module.css'

const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const shortDate = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' })
const generatedAtFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long', timeStyle: 'short' })

const formatCurrency = (value) => currency.format(value ?? 0)
const formatDate = (value) => shortDate.format(new Date(value.includes('T') ? value : `${value}T12:00:00`))
const toLocalDate = (value) => {
  const offset = value.getTimezoneOffset() * 60_000
  return new Date(value.getTime() - offset).toISOString().slice(0, 10)
}
const today = toLocalDate(new Date())
const minimumDate = (() => {
  const value = new Date(`${today}T12:00:00`)
  value.setDate(value.getDate() - 366)
  return toLocalDate(value)
})()

function monthRange(month) {
  const [year, monthNumber] = month.split('-').map(Number)
  const monthStart = `${month}-01`
  const first = monthStart < minimumDate ? minimumDate : monthStart
  const last = toLocalDate(new Date(year, monthNumber, 0, 12))
  return { from: first, to: last > today ? today : last }
}

function currentMonthRange(offset = 0) {
  const value = new Date()
  value.setMonth(value.getMonth() + offset, 1)
  return monthRange(`${value.getFullYear()}-${String(value.getMonth() + 1).padStart(2, '0')}`)
}

function movementPresentation(movement, selectedAccountId) {
  const isIncome = movement.kind === 'income'
  const isOpening = movement.kind === 'opening_balance'
  const isGoalDeposit = movement.kind === 'goal_deposit'
  const isGoalWithdrawal = movement.kind === 'goal_withdrawal'
  const isTransfer = movement.kind === 'transfer'
  const transferIsOutgoing = isTransfer && selectedAccountId && movement.accountId?.toString() === selectedAccountId
  const positive = isIncome || isOpening || isGoalWithdrawal || (isTransfer && selectedAccountId && !transferIsOutgoing)
  const neutral = isTransfer && !selectedAccountId

  if (isGoalDeposit || isGoalWithdrawal) {
    return {
      Icon: PiggyBank,
      tone: 'goal',
      label: isGoalDeposit ? 'Transferência para objetivo' : 'Retorno do objetivo',
      prefix: isGoalDeposit ? '−' : '+',
    }
  }

  if (isTransfer) {
    return { Icon: ArrowRightLeft, tone: 'transfer', label: 'Entre contas', prefix: neutral ? '' : positive ? '+' : '−' }
  }

  return {
    Icon: positive ? ArrowDownLeft : ArrowUpRight,
    tone: positive ? 'income' : 'expense',
    label: movement.categoryName ?? (positive ? 'Entrada' : 'Saída'),
    prefix: positive ? '+' : '−',
  }
}

function paginateMovements(items) {
  if (items.length === 0) return [[]]
  const pages = [items.slice(0, 7)]
  for (let index = 7; index < items.length; index += 9) pages.push(items.slice(index, index + 9))
  return pages
}

export function FinanceReportModal({ accountId, accountName, from, to, onClose }) {
  const initialCurrentMonth = currentMonthRange()
  const [report, setReport] = useState(null)
  const [reportFrom, setReportFrom] = useState(from)
  const [reportTo, setReportTo] = useState(to)
  const [reportRequestKey, setReportRequestKey] = useState(0)
  const [periodMode, setPeriodMode] = useState(
    from === initialCurrentMonth.from && to === initialCurrentMonth.to ? 'current' : 'custom',
  )
  const [stage, setStage] = useState('loading')
  const [visibleCount, setVisibleCount] = useState(0)
  const [errorMessage, setErrorMessage] = useState('')
  const [busyAction, setBusyAction] = useState(null)
  const [sentTo, setSentTo] = useState('')
  const pdfDocumentRef = useRef(null)
  const isBusy = busyAction !== null

  useEffect(() => {
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const handleEscape = (event) => event.key === 'Escape' && !isBusy && onClose()
    window.addEventListener('keydown', handleEscape)
    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', handleEscape)
    }
  }, [isBusy, onClose])

  useEffect(() => {
    const controller = new AbortController()
    getFinanceDashboard({ accountId, from: reportFrom, to: reportTo, report: true, signal: controller.signal })
      .then((data) => {
        setReport(data)
        setStage('printing')
      })
      .catch((error) => {
        if (error.name !== 'AbortError') {
          setErrorMessage(error.message)
          setStage('error')
        }
      })
    return () => controller.abort()
  }, [accountId, reportFrom, reportRequestKey, reportTo])

  const previewLimit = Math.min(report?.recentMovements.length ?? 0, 8)
  useEffect(() => {
    if (stage !== 'printing' || !report) return undefined

    if (previewLimit === 0) {
      const timer = window.setTimeout(() => setStage('ready'), 900)
      return () => window.clearTimeout(timer)
    }

    if (visibleCount >= previewLimit) {
      const timer = window.setTimeout(() => {
        setVisibleCount(report.recentMovements.length)
        setStage('ready')
      }, 550)
      return () => window.clearTimeout(timer)
    }

    const timer = window.setTimeout(() => setVisibleCount((current) => current + 1), 310)
    return () => window.clearTimeout(timer)
  }, [previewLimit, report, stage, visibleCount])

  const pages = useMemo(() => paginateMovements(report?.recentMovements ?? []), [report])
  const reportCode = `BUBU-${reportFrom.replaceAll('-', '')}-${reportTo.replaceAll('-', '')}`
  const generatedAt = useMemo(() => new Date(), [])

  function applyRange(nextFrom, nextTo, mode) {
    if (!nextFrom || !nextTo || nextFrom > nextTo) return
    setStage('loading')
    setReport(null)
    setVisibleCount(0)
    setErrorMessage('')
    setSentTo('')
    setPeriodMode(mode)
    setReportFrom(nextFrom)
    setReportTo(nextTo)
    setReportRequestKey((current) => current + 1)
  }

  async function generatePdf() {
    await document.fonts?.ready
    const [{ default: html2canvas }, { jsPDF }] = await Promise.all([
      import('html2canvas'),
      import('jspdf'),
    ])
    const pageNodes = [...pdfDocumentRef.current.querySelectorAll('[data-pdf-page]')]
    const pdf = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4', compress: true })

    for (let index = 0; index < pageNodes.length; index += 1) {
      const canvas = await html2canvas(pageNodes[index], {
        scale: 2,
        backgroundColor: '#f8f7f1',
        useCORS: true,
        logging: false,
      })
      if (index > 0) pdf.addPage('a4', 'portrait')
      pdf.addImage(canvas.toDataURL('image/jpeg', 0.94), 'JPEG', 0, 0, 210, 297, undefined, 'FAST')
    }
    return pdf
  }

  async function downloadPdf() {
    if (!pdfDocumentRef.current || isBusy) return
    setBusyAction('download')
    setErrorMessage('')
    setSentTo('')
    try {
      const pdf = await generatePdf()
      pdf.save(`relatorio-bubu-${reportFrom}-${reportTo}.pdf`)
    } catch {
      setErrorMessage('Não foi possível gerar o PDF. Tente novamente.')
    } finally {
      setBusyAction(null)
    }
  }

  async function sendByEmail() {
    if (!pdfDocumentRef.current || isBusy) return
    setBusyAction('email')
    setErrorMessage('')
    setSentTo('')
    try {
      const pdf = await generatePdf()
      const result = await emailFinanceReport({
        pdf: pdf.output('blob'),
        accountId,
        from: reportFrom,
        to: reportTo,
      })
      setSentTo(result.recipient)
    } catch (error) {
      setErrorMessage(error.message)
    } finally {
      setBusyAction(null)
    }
  }

  const shownMovements = report?.recentMovements.slice(0, visibleCount) ?? []

  return (
    <div className={styles.backdrop} role="presentation">
      <section className={styles.modal} role="dialog" aria-modal="true" aria-labelledby="report-title">
        <header className={styles.modalHeader}>
          <div><span><ReceiptText size={19} /></span><div><small>RELATÓRIO FINANCEIRO</small><h2 id="report-title">Seu período em um recibo</h2></div></div>
          <button type="button" onClick={onClose} disabled={isBusy} aria-label="Fechar"><X size={20} /></button>
        </header>

        <ReportPeriodSelector
          from={reportFrom}
          to={reportTo}
          mode={periodMode}
          disabled={isBusy}
          onApply={applyRange}
        />

        <div className={styles.stage}>
          <div className={`${styles.printer} ${stage === 'printing' ? styles.printerActive : ''}`} aria-hidden="true">
            <div className={styles.printerTop}><span /><span /><i /></div>
            <div className={styles.printerFace}><div className={styles.slot} /><span className={styles.powerLight} /></div>
          </div>

          {stage === 'loading' && <div className={styles.loading}><LoaderCircle size={25} /> Organizando suas movimentações...</div>}
          {stage === 'error' && <div className={styles.errorState}><strong>Não conseguimos preparar o relatório.</strong><p>{errorMessage}</p><button type="button" onClick={onClose}>Voltar</button></div>}

          {report && stage !== 'loading' && stage !== 'error' && (
            <div className={styles.paperViewport}>
              <div className={`${styles.receipt} ${stage === 'printing' ? styles.receiptPrinting : ''}`}>
                <ReceiptHeader accountName={accountName} from={reportFrom} to={reportTo} code={reportCode} />
                <Summary report={report} />
                <div className={styles.receiptDivider}><span>MOVIMENTAÇÕES</span></div>
                <div className={styles.movementList}>
                  {shownMovements.length === 0 && stage === 'ready' ? <p className={styles.emptyReceipt}>Nenhuma movimentação no período.</p> : shownMovements.map((movement) => <ReceiptMovement movement={movement} accountId={accountId} animated={stage === 'printing'} key={movement.key} />)}
                </div>
                {stage === 'ready' && <><FinalTotals report={report} /><ReceiptFooter code={reportCode} generatedAt={generatedAt} /></>}
              </div>
              {stage === 'printing' && shownMovements.slice(-3).map((movement, index) => {
                const presentation = movementPresentation(movement, accountId)
                return <span className={`${styles.valueChip} ${styles[presentation.tone]}`} style={{ '--chip-index': index }} key={`chip-${movement.key}`}>{presentation.prefix}{formatCurrency(movement.amount)}</span>
              })}
            </div>
          )}
        </div>

        <footer className={styles.actions}>
          <div><strong>{stage === 'ready' ? 'Relatório pronto!' : 'O Bubu está imprimindo...'}</strong><span>{report?.recentMovements.length ?? 0} movimentações encontradas</span></div>
          <button type="button" onClick={() => window.print()} disabled={stage !== 'ready' || isBusy}><Printer size={17} /> Imprimir</button>
          <button type="button" onClick={sendByEmail} disabled={stage !== 'ready' || isBusy}>{busyAction === 'email' ? <LoaderCircle className={styles.spinner} size={17} /> : <Mail size={17} />} {busyAction === 'email' ? 'Enviando...' : 'Enviar por e-mail'}</button>
          <button type="button" onClick={downloadPdf} disabled={stage !== 'ready' || isBusy}>{busyAction === 'download' ? <LoaderCircle className={styles.spinner} size={17} /> : <Download size={17} />} {busyAction === 'download' ? 'Gerando PDF...' : 'Baixar PDF'}</button>
        </footer>
        {sentTo && <p className={styles.actionSuccess}><CheckCircle2 size={15} /> Relatório enviado para {sentTo}.</p>}
        {errorMessage && stage !== 'error' && <p className={styles.actionError}>{errorMessage}</p>}
      </section>

      {report && <div className={styles.pdfDocument} ref={pdfDocumentRef} aria-hidden="true">{pages.map((pageMovements, index) => <PdfPage report={report} movements={pageMovements} accountId={accountId} accountName={accountName} from={reportFrom} to={reportTo} code={reportCode} generatedAt={generatedAt} page={index + 1} totalPages={pages.length} key={`pdf-${index}`} />)}</div>}
    </div>
  )
}

function ReportPeriodSelector({ from, to, mode, disabled, onApply }) {
  const previousMonth = currentMonthRange(-1)
  const thisMonth = currentMonthRange()
  const yearRange = { from: `${today.slice(0, 4)}-01-01`, to: today }

  return (
    <section className={styles.periodSelector} aria-label="Período do relatório">
      <div className={styles.periodTitle}><span><CalendarDays size={17} /></span><div><strong>Escolha o período</strong><small>O relatório será atualizado antes de gerar o PDF.</small></div></div>
      <div className={styles.periodPresets}>
        <button className={mode === 'current' ? styles.selectedPeriod : ''} type="button" disabled={disabled} onClick={() => onApply(thisMonth.from, thisMonth.to, 'current')}>Este mês</button>
        <button className={mode === 'previous' ? styles.selectedPeriod : ''} type="button" disabled={disabled} onClick={() => onApply(previousMonth.from, previousMonth.to, 'previous')}>Mês anterior</button>
        <button className={mode === 'year' ? styles.selectedPeriod : ''} type="button" disabled={disabled} onClick={() => onApply(yearRange.from, yearRange.to, 'year')}>Este ano</button>
        <label className={mode === 'month' ? styles.selectedPeriod : ''}><span>Selecionar mês</span><input type="month" value={mode === 'month' ? from.slice(0, 7) : ''} min={minimumDate.slice(0, 7)} max={today.slice(0, 7)} disabled={disabled} onChange={(event) => { if (event.target.value) { const range = monthRange(event.target.value); onApply(range.from, range.to, 'month') } }} /></label>
        <button className={mode === 'custom' ? styles.selectedPeriod : ''} type="button" disabled={disabled} onClick={() => onApply(from, to, 'custom')}>Personalizado</button>
      </div>
      {mode === 'custom' && <div className={styles.customRange}><label><span>Data inicial</span><input type="date" value={from} min={minimumDate} max={today} disabled={disabled} onChange={(event) => { const value = event.target.value; if (value) onApply(value, value > to ? value : to, 'custom') }} /></label><i>até</i><label><span>Data final</span><input type="date" value={to} min={minimumDate} max={today} disabled={disabled} onChange={(event) => { const value = event.target.value; if (value) onApply(value < from ? value : from, value, 'custom') }} /></label></div>}
    </section>
  )
}

function ReceiptHeader({ accountName, from, to, code, page, totalPages }) {
  return <header className={styles.receiptHeader}><div className={styles.receiptBrand}><img src={bubuImage} alt="" /><div><strong>BuBu</strong><small>RELATÓRIO FINANCEIRO</small></div></div>{page && <span className={styles.pageNumber}>{page}/{totalPages}</span>}<div className={styles.period}><span>{accountName || 'Todas as contas'}</span><strong>{formatDate(from)} a {formatDate(to)}</strong></div><small className={styles.reportCode}>{code}</small></header>
}

function Summary({ report }) {
  return <div className={styles.receiptSummary}><div><span>Saldo disponível</span><strong>{formatCurrency(report.summary.balance)}</strong></div><div><span>Entradas</span><b className={styles.incomeText}>+{formatCurrency(report.summary.income)}</b></div><div><span>Saídas</span><b className={styles.expenseText}>−{formatCurrency(report.summary.expense)}</b></div><div><span>Resultado</span><b>{report.summary.periodResult >= 0 ? '+' : '−'}{formatCurrency(Math.abs(report.summary.periodResult))}</b></div></div>
}

function ReceiptMovement({ movement, accountId, animated = false }) {
  const { Icon, tone, label, prefix } = movementPresentation(movement, accountId)
  return <article className={`${styles.receiptMovement} ${styles[`${tone}Row`]} ${animated ? styles.movementPrinted : ''}`}><span className={styles[tone]}><Icon size={15} /></span><div><strong>{movement.description}</strong><small>{label} · {movement.accountName}</small></div><time>{formatDate(movement.date)}</time><b className={styles[`${tone}Text`]}>{prefix}{formatCurrency(movement.amount)}</b></article>
}

function ReceiptFooter({ code, generatedAt }) {
  return <footer className={styles.receiptFooter}><p>Gerado em {generatedAtFormatter.format(generatedAt)}</p><div className={styles.barcode} /><strong>{code}</strong><small>Transferências não alteram os totais de entradas e saídas.</small></footer>
}

function FinalTotals({ report }) {
  return <section className={styles.finalTotals}><small>FECHAMENTO DO PERÍODO</small><div><span><i>Entradas</i><strong className={styles.incomeText}>+{formatCurrency(report.summary.income)}</strong></span><span><i>Saídas</i><strong className={styles.expenseText}>−{formatCurrency(report.summary.expense)}</strong></span><span><i>Resultado</i><strong>{report.summary.periodResult >= 0 ? '+' : '−'}{formatCurrency(Math.abs(report.summary.periodResult))}</strong></span></div></section>
}

function PdfPage({ report, movements, accountId, accountName, from, to, code, generatedAt, page, totalPages }) {
  return <section className={styles.pdfPage} data-pdf-page><ReceiptHeader accountName={accountName} from={from} to={to} code={code} page={page} totalPages={totalPages} />{page === 1 && <Summary report={report} />}<div className={styles.receiptDivider}><span>MOVIMENTAÇÕES</span></div><div className={styles.movementList}>{movements.length ? movements.map((movement) => <ReceiptMovement movement={movement} accountId={accountId} key={movement.key} />) : <p className={styles.emptyReceipt}>Nenhuma movimentação no período.</p>}</div>{page === totalPages && <FinalTotals report={report} />}<ReceiptFooter code={code} generatedAt={generatedAt} /></section>
}
