import { Alignment, Fit, Layout, useRive } from '@rive-app/react-webgl2'

const bubuFile = new URL('../../assets/bubu.riv', import.meta.url).href

export function AuthMascot({ className }) {
  const { RiveComponent } = useRive({
    src: bubuFile,
    stateMachines: 'State Machine 1',
    autoplay: true,
    layout: new Layout({
      fit: Fit.Contain,
      alignment: Alignment.Center,
    }),
  })

  return <RiveComponent className={className} />
}
