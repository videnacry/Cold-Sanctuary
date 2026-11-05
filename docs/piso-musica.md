# Área de Música: escenarios, estudios y salas de ensayo (2026-11-05)

Berón: «si no existe área de música créala», y después: «si una de las tareas es dar conciertos tendría que haber más de un escenario y estudio de grabación no? para que varios
grupos puedan practicar simultáneamente se pueden aprovechar las paredes transparentes para bloquear sonido». Sin profesión de músico: las tareas van por la rotación normal y todos
pueden hacerlo todo. Solo código: **compila, pero no se ha construido ni probado en Unity**.

## Dónde va y qué tiene

Torre del Entrenamiento, encima del Aula (`SampleSceneBuilder.Musica.cs`; área `Music_Area`, «Música», `SanctuaryAreaType.Music`, al final del enum). Hoy (28 miembros), **3 pisos**:

| Piso | Qué hay | Puestos (`AreaSeat`) |
|---|---|---|
| **Conciertos** (`Music_Area`) | Escenario grande y 5 filas de butacas. Segundo escenario y 2 filas, en su sala de doble cristal. Banco del técnico. Aseos | «concierto» (ocio) 50 + 16; «tocar en concierto» 6 + 4; «sonido» 2 |
| **Estudios 1 y 2** (`Planta_musica_2` y `_3`) | Cada uno: 4 salas de ensayo y 2 estudios (control + sala de grabación). Aseos | «ensayar» (entrenamiento) 5 por sala; «grabar» 4 y «mezcla» 2 por estudio |

Total: **2 escenarios, 4 estudios, 8 salas de ensayo** con 40 plazas: pueden ensayar a la vez los 28 miembros, en 6 grupos de 5 o en 8 grupos más pequeños. Los pisos de estudios salen de
`MusicStudioFloors()` = ⌈miembros / 20⌉: si llega más gente, el constructor añade otro. El microcosmos usa solo el piso de conciertos (las plantas con `TreeFloor` se quitan al clonar).

## Acústica: cuánto bloquea el cristal

**Un paño de cristal suelto no basta.** Los paños que ya había (`GlassWall`: una hoja, puerta abierta, sin tapa) se pensaron para aislar el aire, y para el sonido casi no sirven.

| Dato | Valor | Fuente |
|---|---|---|
| Un cristal acústico de 12,5 mm | ~Rw 40 | glassatwork.co.uk (catálogo) |
| Doble acristalamiento estándar, cámara térmica | Rw 30–33 | ídem |
| Doble cristal de mampara acústica | Rw 44–52 (laboratorio) | catálogos de mamparas (Indeglas FG2: 44 dB; el resto, sin atribución verificada) |
| Pérdida en obra | hasta 8 dB | glassonweb.com |
| Cámara de aire | ≥ 50 mm; 2–3 dB por cada doblado | glassatwork; foros de John Sayers |
| Ventana de control clásica | dos cristales con 200 mm de aire y marcos separados: ~40 dB | artículo de revista de 1984, en muzines.co.uk |
| Meta entre salas de ensayo | STC 50 (uso normal), 55 (silencioso) | guía del sector en build-construct.com |
| Meta en puertas de sala de música | STC 40 | ANSI, citado en esa búsqueda |
| Estudio serio | STC 55–65 («room within a room», ~55–60 dB) | studiomatrx, secondskinaudio (vendedores), sonicscoop |
| El punto débil | el hueco de arriba y las puertas | glassonweb.com |

Son cifras de catálogo y de foros, sin informes de ensayo: valen como orden de magnitud. **No encontré ningún dato de ensayo de la composición exacta que usamos.**

**Diseño (`SoundRoom`, `MusicAcoustics`):** dos cajas de cristal laminado, una dentro de otra, con 200 mm de aire, y **tapa de cristal**. Se asume Rw 45 ya descontada la obra y Rw 40 en la puerta.
Aplicando la ley de la pared compuesta a una sala de 8,4 × 6,2 m:

| Caso | Aislamiento |
|---|---|
| Sala cerrada, tapa y puerta | **~45 dB** |
| Entre dos salas vecinas (dos paredes dobles; se suman +8 a la baja) | **~53 dB**: cumple STC 50, no STC 55 |
| Con la puerta abierta | ~15 dB |
| Sin tapa | ~4 dB |

**Conclusiones:** sirve para que varias bandas ensayen a la vez, pero **una batería seguirá oyéndose bajito en la sala de al lado**: el grave es lo que peor para el cristal, y un estudio serio pide más (STC 55–65).
Grabar en una sala y tocar batería en la contigua no sería limpio. Hay dos límites del modelo:
- **Las puertas son huecos abiertos** (1,8 m), como en todo `GlassWall`: en el juego nadie las cierra, y la tabla supone que sí. El juego no simula el sonido: `MusicAcoustics` es una cuenta.
- **La tapa de cristal es un techo.** La regla S6 dice «sin muros ni techos» y solo permite paredes de cristal. Sin tapa no hay aislamiento (4 dB). **Decide Berón**: se ha puesto porque no tapa la vista.

## Vida en el santuario

- **Misiones** (no obligatorias, 2,5 h): `son_grabar`, `son_ensayo`, `son_equipo`, por la rotación normal. Con pasos en `VirtualMissions`, `PlayerTasks` y `Performances`.
- **Uniforme**: `Uniforms.Kind.Musica`. **Horario libre**: «concierto», para todos, con la Cocina de reserva.
- **Máquina de virtualización**: `WireMeditationArea`; se entra a 8 microcosmos (Vega Baja, Montreal, Gary, Kingston, Belfast, Long Island, Grandes aportes y Viena 1824). Seúl sigue en la Sala de Ballet.

## Auditoría de reglas (2026-11-05)

**Qué se comprobó:** el código compila (`verify-wt.sh`, 0 errores) y las medidas se calcularon a mano leyendo el código. **Qué NO:** nada se ha construido en Unity, ninguna prueba ha corrido y no hay fotos. «Por cálculo» no es un aprobado.

| Regla | Resultado | Cómo se comprobó | Se confirma con |
|---|---|---|---|
| S1, T3 elevado, ≥ 50 m | Por diseño | Piso de la hélice (`TowerFloor`) | 57 |
| S2 torre temática | Por diseño | Va en Entrenamiento | 28 |
| S3, T1, T2 edificio-árbol | Por diseño | Mismo `HelixFloor` | 57 |
| S4 ≥ 16 m entre torres | **Sin comprobar: riesgo** | La torre sube hasta 2 niveles; la holgura con el Árbol era 16,3 m | 57 |
| S5 sin escaleras | **Falla en parte** | Las tarimas miden 0,6 m y no tienen rampa: nadie sube | fotos; hay que añadir rampa |
| S6 sin muros ni techos | **Excepción pedida** | Paredes de cristal, permitidas; **las tapas son techo**: decide Berón | — |
| S7 2 m de paso | Por cálculo | Medido a mano: 2,04 m el más justo (banda–cristal) | 28; también la prueba 86 |
| S8 medida humana | Por cálculo | Butaca a 0,45 m | solo en docs |
| S9 30 pisos | Sin comprobar | Suma 3 pisos; el total de la torre no se contó | 28, `StructuralLimits` |
| S10 plazas reservadas | **Sin comprobar** | Las calcula `Slots`; las salas de cristal le quitan sitio | 28, 40 |
| S11 energía solar | Pendiente | `FloorEnergy` no ha llegado a `claude/edificios` | energía (ver abajo) |
| S13 = J16 capacidad | Por cálculo | Ensayo 40 ≥ 28, concierto 66 ≥ 28, trabajo ≥ 2 | 75, 86 |
| F1, F2 formas del piso | Por diseño | `EnsureFloorForms` recorre todas las áreas | 67, 68 |
| F4 misma medida | Por diseño | 36 × 26 m, `TowerFloor` | 57 |
| F5 umbrales | Por cálculo | Zona de 7,4 × 4,4 m libre: x −4,1…3,3 en conciertos; franja de 4,6 m en estudios | 57 |
| F6, §11 revisar microcosmos | Hecho | Tabla del manual actualizada | 43, 67 |
| T4 alfombras | Por diseño | No se añade ninguna | 57 |
| T6 puertas | Tronco 7 × 7 m, por diseño. **Salas de cristal: 1,8 m**, el mismo valor de `GlassWall` en el hospital | **Menos que los 2 m de S7**: dos personas pasan justas | 84 solo mira el hospital |
| T8 máscaras | Por diseño | Por nivel, automático | 70 |
| H5 cristal solo donde aísla | Cumple | Salas de sonido | — |
| J3 horario libre | Por lectura de código | «concierto» en `SanctuaryActivities` | 65 |
| J16 ocio para todos | Por lectura | En `AllMembers` | 89, 75 |
| J19 hacer la tarea | Por lectura | 3 `Performances`; el concierto, «sentado» genérico | 97 |
| J20 uniforme | Por lectura | `Kind.Musica` | 97 |
| J22 pasos con «E» | Por lectura | `PlayerTasks`; los sitios son por nombre | 96 |
| J2 rotación, ≤ 5 h | Por diseño | 2,5 h no obligatorias; sin profesión | 28, 50 |
| J29 nada en obras | Cumple | El piso nace terminado | 65 |
| M9 mapas de sus microcosmos | Parcial | Las órdenes `pared` y `mesa` usan el catálogo; no se han visto | 68, fotos |
| W9 revisar jornadas | Hecho | Sin puestos fijos ni profesión | 50, 65 |

## Qué falta

1. **Rampas de las tarimas** (S5), o decidir que el concierto se toca delante de ellas.
2. **Berón decide las tapas** (S6).
3. Al unir `claude/edif-energia`: `SanctuaryAreaType.Music` en la línea de 18 W/m² de `EnergyRule.WattsPerM2` (sin eso, 16). `FloorEnergy.Declare` recorre todas las áreas y calcula las mesas solares sola: estos pisos piden unos 2800 m² × 18 W/m² ≈ 50 kW de media, sin contrastar con las mesas libres.
4. Reconstruir SampleScene y pasar: 86, 75, 57, 28, 40, 84, 50, 65, 89, 96, 97, 67, 68, 70.
5. `AreaFunctionRegistryTest`: sus cuentas fijas («55 misiones», 64,5 h) parecen desfasadas ya antes de esto; sin ejecutar.
