import styles from './BrandMark.module.css'

export function BrandMark() {
  return (
    <span className={styles.brand} aria-label="Bubu">
      <span className={styles.mark} aria-hidden="true">
        <span />
        <span />
        <span />
      </span>
      <span className={styles.name}>bubu</span>
    </span>
  )
}
