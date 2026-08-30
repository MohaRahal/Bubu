import bubuIcon from '../../assets/bubu-3d.png'
import styles from './BrandMark.module.css'

export function BrandMark() {
  return (
    <span className={styles.brand} aria-label="Bubu">
      <span className={styles.mark} aria-hidden="true">
        <img src={bubuIcon} alt="" />
      </span>
      <span className={styles.name}>bubu</span>
    </span>
  )
}
