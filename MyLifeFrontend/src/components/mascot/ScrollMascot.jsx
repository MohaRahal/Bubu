import { useEffect, useRef, useState } from 'react'
import {
  AnimatePresence,
  motion,
  useReducedMotion,
  useScroll,
  useSpring,
  useTransform,
} from 'motion/react'
import {
  Alignment,
  Fit,
  Layout,
  useRive,
} from '@rive-app/react-webgl2'
import styles from './ScrollMascot.module.css'

const bubuFile = new URL('../../assets/bubu.riv', import.meta.url).href

const messages = [
  'Oi! Eu sou o Bubu. Que bom ter você aqui 👋',
  'Pequenos registros podem revelar grandes mudanças.',
  'Que tal começar pelo que mais importa hoje?',
  'Eu ajudo a organizar, mas sem julgar. Combinado? ☁️',
]

export function ScrollMascot() {
  const [messageIndex, setMessageIndex] = useState(0)
  const [isMessageVisible, setIsMessageVisible] = useState(false)
  const closeTimer = useRef(null)
  const shouldReduceMotion = useReducedMotion()
  const { scrollYProgress } = useScroll()
  const { canvas, RiveComponent } = useRive({
    src: bubuFile,
    stateMachines: 'State Machine 1',
    autoplay: true,
    layout: new Layout({
      fit: Fit.Contain,
      alignment: Alignment.Center,
    }),
  })

  const smoothProgress = useSpring(scrollYProgress, {
    stiffness: 82,
    damping: 24,
    mass: 0.42,
    restDelta: 0.001,
  })

  const y = useTransform(
    smoothProgress,
    [0, 0.2, 0.4, 0.6, 0.8, 1],
    ['14vh', '23vh', '36vh', '48vh', '60vh', '72vh'],
  )

  const rotate = useTransform(
    smoothProgress,
    [0, 0.23, 0.4, 0.58, 0.76, 1],
    [-8, 7, -6, 8, -5, 7],
  )

  useEffect(() => {
    return () => window.clearTimeout(closeTimer.current)
  }, [])

  useEffect(() => {
    if (!canvas) {
      return undefined
    }

    function handleGlobalPointerMove(event) {
      if (!event.isTrusted) {
        return
      }

      const canvasBounds = canvas.getBoundingClientRect()
      const normalizedX = Math.min(1, Math.max(0, event.clientX / window.innerWidth))
      const normalizedY = Math.min(1, Math.max(0, event.clientY / window.innerHeight))

      canvas.dispatchEvent(new PointerEvent('pointermove', {
        bubbles: false,
        clientX: canvasBounds.left + normalizedX * canvasBounds.width,
        clientY: canvasBounds.top + normalizedY * canvasBounds.height,
        pointerId: event.pointerId,
        pointerType: event.pointerType,
      }))
    }

    window.addEventListener('pointermove', handleGlobalPointerMove, {
      passive: true,
    })

    return () => {
      window.removeEventListener('pointermove', handleGlobalPointerMove)
    }
  }, [canvas])

  function handleMascotClick() {
    window.clearTimeout(closeTimer.current)

    setMessageIndex((current) =>
      isMessageVisible ? (current + 1) % messages.length : current,
    )
    setIsMessageVisible(true)

    closeTimer.current = window.setTimeout(() => {
      setIsMessageVisible(false)
    }, 4500)
  }

  return (
    <div className={styles.layer}>
      <motion.div
        className={styles.positioner}
        style={{
          y: shouldReduceMotion ? '58vh' : y,
        }}
      >
        <AnimatePresence>
          {isMessageVisible && (
            <motion.div
              className={styles.message}
              role="status"
              aria-live="polite"
              initial={{ opacity: 0, x: 18, scale: 0.92 }}
              animate={{ opacity: 1, x: 0, scale: 1 }}
              exit={{ opacity: 0, x: 12, scale: 0.95 }}
              transition={{ type: 'spring', stiffness: 320, damping: 24 }}
            >
              <span>BUBU DIZ</span>
              <p>{messages[messageIndex]}</p>
              <i aria-hidden="true" />
            </motion.div>
          )}
        </AnimatePresence>

        <motion.button
          className={styles.mascotButton}
          type="button"
          aria-label="Conversar com o Bubu"
          onClick={handleMascotClick}
          style={{ rotate: shouldReduceMotion ? 0 : rotate }}
          whileHover={shouldReduceMotion ? undefined : { scale: 1.06 }}
          whileTap={shouldReduceMotion ? undefined : { scale: 0.94 }}
        >
          <motion.div
            className={styles.float}
            animate={shouldReduceMotion
              ? undefined
              : { translateY: [0, -9, 0], rotate: [0, 1.2, 0] }}
            transition={{
              duration: 3.2,
              ease: 'easeInOut',
              repeat: Infinity,
            }}
          >
            <RiveComponent className={styles.mascot} aria-label="" />
          </motion.div>
        </motion.button>
      </motion.div>
    </div>
  )
}
