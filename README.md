# Unidad 2 – Arquitectura de Sistemas Integrados

Nivel 2D de plataformas hecho en **Unity 6 (6000.6) con URP 2D** para el curso *Desarrollo de Videojuegos*
(Universidad Continental). El objetivo no es solo terminar el nivel, sino demostrar que los sistemas
(Input, Animación, Audio, UI, VFX) están **desacoplados** y se comunican por eventos.

**Autor:** Christian Manuel Cueva Chambilla (trabajo individual)

## Cómo jugar

| Acción | Teclado | Mando |
|---|---|---|
| Mover | A / D o flechas | Stick izquierdo |
| Saltar (mantener = más alto) | Espacio / W / ↑ | A (sur) |
| Dash (derrota enemigos) | Shift / J | X (oeste) / RB |
| Pausa | Esc / P | Start |
| Reiniciar | R | – |

Recoge las **9 frutas** y llega a la meta (trofeo). Pisa a los cerdos o hazles dash para derrotarlos; los picos
y los cerdos quitan vida; caer al vacío es muerte inmediata.

## Requisitos de la rúbrica y dónde están

| Requisito | Implementación |
|---|---|
| Mundo 2D con Tilemap y colisionadores compuestos | `Level_01`: Tilemap + `TilemapCollider2D` + `CompositeCollider2D` (Merge) + `Rigidbody2D` estático |
| Cámara dinámica | `CameraFollow`: dead zone, damping (`SmoothDamp`), look-ahead, límites del nivel y shake por evento |
| Personaje animado | `Animator Controller` con Idle, Run, Jump, Fall, Dash y Hit; transiciones por parámetros sin *Has Exit Time* |
| UI/UX reactiva | `HUDController` (barra de vida que parpadea, contador, mensajes), `PauseMenu`, `EndScreenPanel`, `MainMenu`; Canvas Scaler 1920x1080 |
| Juice | `VFXManager` (ParticleSystem + Object Pool) y `AudioManager` (SFX + BGM + `AudioMixer` con grupos Music/SFX) |
| Patrones | Singleton (`Singleton<T>`: GameManager, AudioManager, VFXManager), Observer (`GameEvents`), Object Pool |
| SOLID | SRP: el jugador se divide en `PlayerInputHandler`, `PlayerMovement`, `PlayerHealth`, `PlayerCombat`, `PlayerAnimator`, `PlayerFeedback`; `IDamageable` como abstracción |

## Arquitectura

```
                    GameEvents (static, Observer)
   publica ──────────────┬──────────────────────── escucha
 PlayerMovement          │            HUDController, PauseMenu, EndScreenPanel
 PlayerHealth            │            AudioManager, VFXManager
 Collectible, Goal       │            GameManager, CameraFollow, PlayerAnimator
 EnemyPatrol             │
```

Ningún script de gameplay conoce a la UI: el jugador solo anuncia "cambió mi vida" y quien quiera reaccionar
se suscribe (`OnEnable` suscribe, `OnDisable` desuscribe). Cada evento documenta quién lo publica y quién lo escucha
en `Assets/_Project/Scripts/Architecture/GameEvents.cs`.

## Estructura

```
Assets/_Project/
  Scripts/{Architecture,Player,World,Cameras,Audio,VFX,UI}   código de juego (asmdef Unidad2.Runtime)
  Editor/                                                     generador de nivel/escenas (menú "Unidad 2")
  Tests/PlayMode/                                             18 pruebas automáticas
  Art/ Audio/ Animations/ Prefabs/ Scenes/
```

## Pruebas

`Window > General > Test Runner > PlayMode` (18 pruebas): sistemas presentes, aterrizaje, correr/saltar, dash, HUD por eventos,
daño e invulnerabilidad, picos, Game Over, vacío, pausa, pisotón, victoria, cámara, Animator, menú/mixer y un
**bot que recorre todo el nivel** hasta la meta para garantizar que es completable.

## Regenerar el proyecto

`Unidad 2 > 0) Importar TMP Essentials` y luego `Unidad 2 > 1) Generar proyecto completo` recrea tiles, animaciones,
partículas, mixer, prefabs y las escenas `MainMenu` y `Level_01`.

## Créditos

- Sprites (personajes, enemigos, frutas, terreno, trampas): *Pixel Adventure* de Pixel Frog (CC0).
- Música, efectos de sonido y fondo: reutilizados del proyecto *Digital_Toy* (Unidad 1) del mismo autor.
