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

## Tras la primera prueba en el móvil (2026-11-13)
- **Joystick de la cámara** (Berón): abajo a la derecha, fijo. Gira a velocidad constante según cuánto se empuja (a tope, ~100 °/s de giro y
  ~60 °/s de cabeceo) y **manda sobre arrastrar** (arrastrar en el resto de la mitad derecha sigue mirando).
  - Los botones **E, F, Saltar y Correr** van en **arco** encima de él (E arriba del todo, Correr a la izquierda).
  - En una pantalla estrecha (vertical), el botón que pisaría el joystick de andar se oculta. E se ve siempre. Si Correr se oculta, deja de
    correr.
  - **Intro** va pequeño arriba en el centro.
  - Todo mide ~13 % del lado corto de la pantalla y se recoloca cada fotograma, así que vale en horizontal y en vertical.
- **Cubo magenta**: los sombreadores de los materiales hechos en juego no iban en el build (nadie los referenciaba). El menú ahora crea, en
  `Assets/Resources/WebLigeraShaders/`, un material de cada sombreador del URP que se usa en juego, con las palabras clave del juego (mate,
  opaco y transparente):
  - Simple Lit, Lit, Unlit y Particles/Unlit.
  - Así el build guarda esos sombreadores y solo esas variantes.
  - Los pequeños (Sprites/Default, Unlit/Color, Unlit/Transparent) van a «Always Included Shaders».
  - Lo registra: `[Web] Sombreadores…`.
- **Plantilla para el móvil**: el menú escribe `Assets/WebGLTemplates/WebLigera/index.html` y la elige (`PROJECT:WebLigera`). Trae:
  - el lienzo al 100 % de ancho y alto (`100dvh`) y el meta viewport sin zoom;
  - la densidad de píxeles con tope 1,5;
  - una barra de carga y un botón ⛶ de pantalla completa (si el navegador la permite; en Safari del iPhone, no).
- **Sin paneles de depuración** en la web:
  - fuera: los rótulos flotantes (`AnimaNameplate`), las lecturas de emoción y de estado de las ánimas, el pensamiento, los bocadillos de
    las charlas, la barra de hechizos, el itinerario de la jornada (arriba a la derecha) y el «F1: ver paneles de prueba»;
  - se quedan: la misión, la voz de Kushal, los subtítulos y las tarjetas de las historias, los diálogos, y las ventanas de Horario,
    Recursos y Misiones;
  - esas tres ventanas salen sin «F2/F3/F4», arriba a la izquierda (bajo el botón «½ res»), y el holograma «Menú» va arriba a la derecha.
- **Texto legible**: la piel IMGUI por defecto (la de todos los paneles que no fijan su tamaño) va a ~3,5 % del lado corto de la pantalla.

## Segunda prueba (build 65aecfe)
- **Bloque magenta junto al jugador**: el `RefugeMarker` de la misión WASD (un `CreatePrimitive`, con el material por defecto del URP que la
  pasada de CheapMaterials pasa a Simple Lit). El menú guarda ahora en `WebLigeraShaders` también el material por defecto de las primitivas,
  tal cual y en barato. Además, en la web, `WebLigera.FixBrokenShaders` busca los materiales que saldrían en magenta (sin sombreador, no
  soportado o el de error):
  - al arrancar, a los 3, 10 y 30 s, y luego cada minuto;
  - **registra** cada uno (`[Web] MAGENTA …: material «…» con el sombreador «…» en «ruta»`), que dice qué sombreador falta añadir;
  - le pone el primer sombreador bueno (Simple Lit, Lit, Unlit o Unlit/Color) con su color.
- **Itinerario en medio de la pantalla**: en la web, «Tu jornada de hoy…» es una sola línea durante 5 s («Hoy: 09:00 … y N más · la jornada
  entera, en «Misiones y jornada»»). La lista entera está en esa ventana. La línea de la misión sigue abajo.
