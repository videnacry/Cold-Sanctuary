# Piso de Música: sala de grabación y escenario de conciertos (2026-11-05)

Berón: «si no existe área de música créala». Es el piso que pedían los microcosmos de la música y Viena («Sala de grabación y escenario de
conciertos»). Solo código: **sin construir ni probar en Unity**.

## Dónde va

**Torre del Entrenamiento, encima del Aula** (`SampleSceneBuilder.Musica.cs`, `BuildMusic`; área `Music_Area`, «Música», `SanctuaryAreaType.Music`, al final del enum para no mover
los valores guardados). Es un piso más de la hélice (`TowerFloor`): 36 × 26 m, sus ramas, puertas y máscaras son las de cualquier piso del árbol; `entLevels` suma este piso. Va en
Entrenamiento porque comparte público y oficio con el ballet y el baile, y arriba del todo para que el ruido quede lejos de las salas de yoga. Queda justo
encima del Aula, así que un concierto y una clase no deben coincidir (el concierto es por la noche, en el horario libre).

## Qué tiene

| Zona | Lado | Piezas | Puestos (`AreaSeat`) |
|---|---|---|---|
| Escenario de conciertos | −X, junto al tronco, sin tocar el umbral de la puerta (F5) | tarima de 10,5 × 4,8 m, batería, piano, amplificador con guitarra, 3 micrófonos, altavoces: un mueble | «tocar en concierto», 6 |
| Butacas | −X | 5 filas de 10, con 2 m de paso | «concierto» (ocio), **50**: J16, todos los miembros a la vez |
| Cabina de voz | +X | 2 micrófonos dentro de una **cabina de cristal** (`GlassWall.Box`: la única pared permitida, en una sala que debe aislarse: S6) | «grabar», 2 |
| Mesa de mezclas | +X, frente a la cabina | faders, monitores y pantalla | «mezcla», 2 |
| Rincón de ensayo | +X | batería, piano y amplificador: un mueble | «ensayar», 4 |
| Banco del técnico | +X | rollos de cable y herramientas | «sonido», 2 |
| Aseos | esquina | los de siempre | — |

Si los miembros pasan de 50, `AreaCapacity` falla: entonces hace falta un segundo piso (como el Aula).

## Cómo entra en la vida del santuario

- **Misiones** (`AreaFunctions`, no obligatorias; 2,5 h): `son_grabar`, `son_ensayo`, `son_equipo`. Los id son `son_` porque `mus_` ya es el Museo. Con sus pasos en `VirtualMissions`, `PlayerTasks` («E») y `Performances`. **Nadie tiene aún la profesión de músico**: las reparte la rotación.
- **Uniforme**: `Uniforms.Kind.Musica`, camiseta y pantalón negros de técnico de escenario.
- **Horario libre**: actividad «concierto» (ocio para todos, con la Cocina de reserva).
- **Máquina de virtualización**: `WireMeditationArea` del área; desde ella se entra a los 8 microcosmos del piso.
- **Energía**: la rama `claude/edif-energia` declara cada piso con `FloorEnergy.Declare` sobre todas las `SanctuaryArea`, así que este piso entra solo y sus mesas solares salen de ahí. Al unirla, **añadir `SanctuaryAreaType.Music` a la línea de 18 W/m² de `EnergyRule.WattsPerM2`** (recreación); sin eso cuenta 16 W/m². No está en `claude/edificios` todavía, por eso no se ha tocado.

## Microcosmos que dependen de él

Vega Baja, Montreal, Gary, Kingston, Belfast, Long Island, Grandes aportes y **Viena 1824** (re-apuntada aquí desde la Sala de Ballet). Todos pasan a representados. **Seúl sigue en la Sala de Ballet**: es una sala de ensayo.

## Qué probar (lo hace el probador)

1. «Build Sample Scene Blockout (solo el santuario, sin microcosmos)» y guardar SampleScene (W5, W6). El orden importa: los microcosmos copian este piso.
2. Grupos 86 (`MusicaStoryTest`: al empezar mira el piso), 75 (capacidad), 57 (árboles: **holgura de 16 m**, puertas y umbrales), 28 (2 m de paso), 84 (la cabina de cristal), 50 y 65 (jornada y horario libre), 96 (pasos con «E»), 97 (uniforme y actuación), 40.
3. `AreaFunctionRegistryTest`: se añadieron los 3 id; sus cuentas fijas de «55 misiones» y «64,5 h» ya estaban desfasadas antes de este cambio (hay misiones de profesiones que no están en su lista) y siguen sin cuadrar.

## Riesgos

- Si la torre sube **un nivel** por este piso, hay que medir de nuevo la holgura con el Árbol de los Campos (16,3 m al medirla).
- Las posiciones del piso se calcularon a mano; el umbral de las puertas y los 2 m, sin comprobar en vivo.
- Encima de un Aula sin muros, el sonido no se simula.
