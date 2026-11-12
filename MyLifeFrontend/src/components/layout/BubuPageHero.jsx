import styles from './BubuPageHero.module.css'

export function BubuPageHero({
  eyebrow,
  title,
  highlight,
  description,
  image,
  imageAlt = '',
  badge,
  stats = [],
  children,
}) {
  return (
    <section className={`${styles.hero} ${stats.length ? styles.withStats : ''}`}>
      <div className={styles.copy}>
        <span className={styles.eyebrow}>{eyebrow}</span>
        <h1>{title} {highlight && <em>{highlight}</em>}</h1>
        <p>{description}</p>

        {children && <div className={styles.actions}>{children}</div>}

        {stats.length > 0 && (
          <div className={styles.stats} aria-label="Resumo">
            {stats.map(({ icon: Icon, label, value, tone = 'default' }) => (
              <article className={styles[tone]} key={label}>
                <span><Icon size={17} /></span>
                <div><small>{label}</small><strong>{value}</strong></div>
              </article>
            ))}
          </div>
        )}
      </div>

      <div className={styles.mascot} aria-hidden={imageAlt ? undefined : 'true'}>
        <i />
        <img src={image} alt={imageAlt} />
        {badge && <div>{badge}</div>}
      </div>
    </section>
  )
}
