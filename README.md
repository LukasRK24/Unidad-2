# Unidad 2

Juego de plataformas 2D hecho en Unity 6 para el curso Desarrollo de Videojuegos (Unidad 2).

## Controles

- Moverse: flechas izquierda/derecha o A y D
- Saltar: flecha arriba, W o Espacio
- Dash: Shift o J
- Pausa: Esc o P
- Reiniciar: R

## Objetivo

Recoger las 9 frutas y llegar al trofeo. Los cerdos y los picos quitan vida; se les gana pisándolos o con dash.

## Qué tiene

- Nivel con Tilemap y colisión compuesta
- Cámara que sigue al jugador con zona muerta y suavizado
- Animator del personaje (Idle, Run, Jump, Fall, Dash, Hit)
- Menú principal, HUD, pausa, victoria y game over
- Sonidos, música (AudioMixer) y partículas
- Eventos (Observer) y managers (Singleton)

## Carpetas

- `Assets/_Project/Scripts`: código del juego
- `Assets/_Project/Tests`: pruebas del Test Runner

## Cómo abrirlo

Abrir la carpeta con Unity 6000.6 y cargar la escena `MainMenu`.

## Créditos

Sprites: Pixel Adventure (Pixel Frog). Música y sonidos: reutilizados de Digital_Toy.
