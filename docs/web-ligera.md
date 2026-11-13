# Web ligera: dos builds WebGL para el móvil (2026-11-13)

Berón quiere abrir el juego en el móvil. Dos builds WebGL, cada uno con **solo sus escenas** y un símbolo de compilación puesto **solo en ese
build** (`BuildPlayerOptions.extraScriptingDefines`: el editor y el Play no cambian).

| Menú | Escenas | Símbolos | Sale a |
|---|---|---|---|
| `Tools/Cold Sanctuary/Web/Build WebGL: Santuario ligero` | SampleScene | `WEB_LIGERA` | `Builds/Web/SantuarioLigero/` |
| `Tools/Cold Sanctuary/Web/Build WebGL: Microcosmos ligero` | SampleScene + Microcosmos_Buda | `WEB_LIGERA`, `WEB_LIGERA_MICRO` | `Builds/Web/MicrocosmosLigero/` |

`Builds/` está junto a `Assets/` en el proyecto Unity. Necesita el módulo WebGL del Hub. El menú registra el tamaño al terminar.

## Lo que cambia en el juego (`WebLigera`, `Assets/Scripts/Web/WebLigera.cs`)
- **Kushal ya es miembro**: sin examen ni iniciaciones (lo mismo que la casilla «Kushal iniciado» de Inicio y el hechizo `miembro`), así
  que ninguna iniciación carga escenas de microcosmos.
- **Solo el santuario 1**: ni `Santuario_Marino` (`SiteLine`) ni `Santuario_Aereo` (`AereoLink`) se cargan, y sus escenas no van en el
  build. La gente de los santuarios remotos sigue oculta por el reloj, como ahora.
- **Fauna**: tope duro de **30 animales** (`PopulationPlan.AnimalCap`; antes K × humanos ≈ 90). Lo que sobra se quita al cargar, por igual
  entre especies (cada una guarda al menos su pareja), y el plan no deja criar por encima. **Insectos fuera** (ya lo estaban).
- **Calidad barata**: sin sombras (el URP en una copia con distancia de sombra 0 y 1 cascada, ninguna luz con sombra), sesgo de LOD 0,4,
  sin MSAA ni HDR, sin reflejos en tiempo real, 30 fps. **Media resolución** (render scale 0,5): por defecto en el móvil; el botón «½ res /
  1× res» de arriba a la derecha la cambia, y se guarda.
- **Sin pruebas**: el `TestRunner` se quita (en un build ya no corría: solo corre en el editor si se piden). TmpCheck no está en el repo.
- **Controles táctiles** (`TouchControls`), solo con pantalla táctil. El proyecto no tenía entrada táctil: el juego lee el teclado por
  `GameKeys`, y ahora `GameKeys` suma unos valores virtuales:
  - un joystick (el dedo que empieza en el 40 % izquierdo), que vale como WASD;
  - arrastrar en la derecha para mirar (como el botón derecho del ratón);
  - botones **E** (hablar, atender), **F** (usar), **Intro** (seguir una tarjeta), **Saltar** y **Correr** (se queda puesto).

  Los menús IMGUI se tocan como clics.
- **Microcosmos ligero**: al arrancar, Kushal entra directo en **Siddhārtha en Kapilavastu** (`Microcosmos_Buda`, por `MobWorldLoader`,
  como Inicio). Mientras está dentro, la fauna del santuario (de fondo) se pone en pausa; vuelve al salir por el portal.
  - **Por qué Buda:** es el microcosmos con más sistemas probados en vivo. Tiene su historia y misión, la gente que anda a sus puestos por
    el NavMesh (prueba 65), el piso ×100 con muros (ronda 30), el mapa a mano HECHO, y la jornada con franjas libres y charlas.
  - **Por qué no Ambrosio:** es un escenario imaginado (sin mapa real) y su historia no usa `GameKeys` (no la tocarían los botones).
- Un microcosmos necesita el santuario de anfitrión (el jugador, la cámara y el reloj viven en SampleScene): por eso el build lleva las
  dos escenas.

## Ajustes de jugador WebGL (`WebLigeraBuild.ApplyPlayerSettings`)
- Compresión **Disabled** (sin Brotli ni gzip: vale cualquier servidor estático) y Decompression Fallback **off**.
- Memoria: 512 MB al empezar, hasta 1024 MB, crecimiento geométrico.
- IL2CPP, stripping **High**, generación de código **para tamaño** (OptimizeSize, Master), `stripEngineCode`.
- **WebGL 2** solo (OpenGLES3). Excepciones: solo las lanzadas explícitamente.
- Con stripping alto, el ensamblado del juego se guarda entero (usa reflexión): el menú escribe `Assets/link.xml` en el proyecto si falta.

## Tamaño esperado (sin comprimir)
Estimación, sin medir. El último build web (junio, proyecto mucho menor) ocupaba 38 MB.
- **Santuario ligero:** ~45–65 MB.
  - Código IL2CPP (`.wasm`): ~20–30 MB, porque son ~990 scripts.
  - Datos de SampleScene: ~15–25 MB (el `.unity` de texto pesa 71 MB; en binario y con los cubos compartidos, mucho menos).
  - El framework: ~1 MB.
- **Microcosmos ligero:** ~60–85 MB. Suma los datos de Kapilavastu (su `.unity` pesa 59 MB de texto).

Si pasa de 60 MB, el paso siguiente es subir un servidor con Brotli. Lo dirá el registro `[Web] … MB en disco` del menú.

## Comprobado sin Unity
`verify-wt.sh` compila el editor y el runtime. Además, una compilación de **jugador** del runtime: sin `UNITY_EDITOR` ni los ensamblados
`UnityEditor*`, con `UNITY_WEBGL` y con `WEB_LIGERA` (y `WEB_LIGERA_MICRO`). Da 0 errores: todo el código de runtime que usa UnityEditor
ya estaba tras `#if UNITY_EDITOR` (CubeCreature, CheapMaterials, los menús de las pruebas).
