import { useState } from 'react'
import { Menu, X } from 'lucide-react'
import { Link } from 'react-router'
import { BrandMark } from '../brand/BrandMark'
import styles from './SiteHeader.module.css'

const links = [
  { href: '#por-que-bubu', label: 'Por que o Bubu?' },
  { href: '#recursos', label: 'O que ele faz' },
  { href: '#faq', label: 'Dúvidas' },
]

export function SiteHeader() {
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const closeMenu = () => setIsMenuOpen(false)

  return (
    <header className={styles.header}>
      <div className={styles.inner}>
        <a className={styles.logoLink} href="#inicio" onClick={closeMenu}>
          <BrandMark />
        </a>

        <nav className={styles.desktopNav} aria-label="Navegação principal">
          {links.map((link) => <a key={link.href} href={link.href}>{link.label}</a>)}
        </nav>

        <div className={styles.actions}>
          <Link className={styles.login} to="/login">Entrar</Link>
          <a className={styles.primaryAction} href="#comece">Conhecer o Bubu</a>
        </div>

        <button
          className={styles.menuButton}
          type="button"
          aria-label={isMenuOpen ? 'Fechar menu' : 'Abrir menu'}
          aria-expanded={isMenuOpen}
          onClick={() => setIsMenuOpen((current) => !current)}
        >
          {isMenuOpen ? <X size={22} /> : <Menu size={22} />}
        </button>
      </div>

      {isMenuOpen && (
        <nav className={styles.mobileNav} aria-label="Navegação móvel">
          {links.map((link) => <a key={link.href} href={link.href} onClick={closeMenu}>{link.label}</a>)}
          <Link to="/login" onClick={closeMenu}>Entrar</Link>
          <a className={styles.mobileCta} href="#comece" onClick={closeMenu}>Conhecer o Bubu</a>
        </nav>
      )}
    </header>
  )
}
