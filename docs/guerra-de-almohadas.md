# Camas de 4 y guerra de almohadas en la arboleda residencial

Berón (2026-11-04): «las camas de las residencias del santuario deben ser capaces de dar espacio a 4 personas, se deben crear guerras de
almohadas en un ring que se debe crear en la residencia, el ring debe ser creado con la regla para que tenga las dimensiones necesarias
para que se enfrenten todos los residentes del santuario y debe estar lleno de almohadas y la base es una cama colosal».

Y una regla nueva: los centros de entrenamiento y de ocio deben dar servicio a todos los miembros del santuario a la vez.

## Camas de 4 (casas del árbol)

| | Antes | Ahora |
|---|---|---|
| Colchón | 1,40 × 2,10 m (dos plazas) | **3,60 × 2,10 m**: cuatro plazas de 90 cm |
| Armazón | 1,50 × 2,20 m | 3,70 × 2,30 m, con cabecero y cuatro almohadas |
| Casa | 7 × 6 m, a 5,2 m del tronco | **9 × 7 m**, a 6,2 m del tronco (el borde sigue a 1,7 m del eje) |
| Terraza frutal | 7 × 6 m | 7 × 7 m, pegada a la casa |

Por qué esa medida:
- 90 cm es el ancho de una cama individual europea (90 × 200 cm).
- Cuatro, una junto a otra, es la «cama familiar» que se monta juntando camas individuales.
- La mayor de serie, la Alaskan king, mide 2,74 × 2,74 m.

Distribución de la casa (x de −4,5 a 4,5; z de −3,5 a 3,5; cabecero en +z), con los 2 m de paso (manual S2):
- Cama: x −4,45…−0,75, z 1,15…3,45. La mesilla va pegada a ella (van juntas).
- Armario: x 1,9…3,3, a 2,1 m de la mesilla.
- Alfombra-esterilla (3 × 3,6 m): x 1,15…4,15, z −3,45…0,15. Queda a 2,15 m de la cama y a 2,8 m del armario.

Dormir:
- `Bedroom.sleeps` (4) y `laneWidth` (0,9 m) dan una plaza a cada uno.
- `Bedroom.LieSpot`: el dueño en una plaza del medio y su pareja en la de al lado (Amahuz y Karen).
- `SleepSpell` y «Descansar en casa» tumban a cada uno en su plaza.
- El sitio de la casa en el horario libre es el pie de la cama (`Bedroom.FootSpot`).

## El ring de almohadas: la cama colosal

`ResidentialBuilder.BuildPillowRing`. Es una plataforma 8 m por encima de la plataforma central (a 20 m), sobre sus cuatro columnas.

### La regla del tamaño (manual J15)

- Cada luchador necesita una **celda de 2,5 × 2,5 m**:
  - un brazo mide unos 0,7 m y una almohada unos 0,6;
  - cada uno barre unos 1,3 m a su alrededor;
  - dos que se dan de frente quedan a unos 2,5 m.
- El colchón es una rejilla de **n × n celdas**, con **n = ⌈√residentes⌉**. Caben n² a la vez (`PillowRing.CellsFor`, `Capacity`).
- **Residentes** = dueños de las casas del árbol más sus parejas (`PillowRing.Residents`). Hoy son 19: los 18 dueños y Karen.
  - n = 5: colchón de 12,5 × 12,5 m, caben 25.
- Alrededor del colchón:
  - borde blando de 0,6 m, con una entrada de 3 m al pie;
  - 1,6 m de cabecero con almohadones gigantes;
  - 2 m de paso hasta el borde de la plataforma y hasta la pista de alfombras.
- Plataforma de hoy: 17,7 × 23,9 m.
- Sin escalones: el colchón sube 0,2 m sobre la plataforma, y el borde blando no tiene collider (se pasa por encima).
- **Lleno de almohadas**: dos sueltas por celda (50), más un almohadón gigante por celda contra el cabecero.

### Validador

`PillowRing.Validate` falla si:
- caben menos que los residentes o que el personal del santuario (con la Magnate);
- el colchón no mide lo que pide la regla;
- hay menos de dos almohadas por luchador.

Al reconstruir la escena, el ring **crece solo**: `RingSize` usa los residentes de `ResidentialBuilder.Owners` y `Partners`.

`RingSize` también avisa (`Debug.LogError`) si el ring ya no cabe dentro del anillo de casas, cuyo borde interior está a 17,5 m del centro.
- Hoy la esquina queda a 14,9 m.
- Con más de 25 residentes el colchón pasa a 6 × 6 (15 m) y la esquina quedaría a 16,6 m, más allá del máximo de 16,5 (17,5 − 1 de margen).
- Así que con más de 25 residentes `RingSize` avisa y hay que subir el ring, sacarlo del anillo o abrir el anillo de árboles.

### Cómo se llega

El ring tiene su **pista de alfombras**:
- red de edificio de la Residencia, piso 2 (`Residencia#ring`);
- vuela a la plataforma central (piso 1) y al suelo de hielo (piso 0);
- está a 2 m del pie de la cama.

## La actividad: todos a la vez

- `LeisureRoster.Activity.together`: una franja fija a la semana para todo el que quepa. Fuera de esa franja no entra en el reparto.
- La guerra es el **sábado (día 5 de la semana) a las 20 h** (`PillowRing.Day`, `Slot`).
- La pone en el catálogo del santuario el propio ring, como **etiqueta de actividad** (`LeisureTags.Providers`).
- Su capacidad es la del ring.
- Cada uno va a su celda (`PillowRing.Spot`).

`PillowFight`, en la franja:
1. **Coge una almohada**: la suelta más cercana desaparece del colchón y aparece en su mano derecha.
2. Cada 1,1–1,7 s busca al luchador más cercano a su alcance (≤ 2,9 m: los de las celdas de al lado), se gira y le da un
   **almohadazo** (la almohada barre en arco).
3. **Ríen** los dos (cara alegre).
   - El vínculo de amistad sube 0,4 en cada sentido.
   - El estrés baja 0,01.
   - Sueltan algo de serotonina.
4. **Acotado**: como mucho 40 golpes por persona y franja. Nadie sale de su celda: no hay empujones ni saltos.
5. Al acabar la franja, o si le llama una necesidad, deja la almohada en el ring.

## Prueba: `PillowFightTest` (grupo 89, SampleScene)

1. **El ring**:
   - cumple la regla (`Validate`) y tiene ⌈√residentes⌉ celdas por lado;
   - la regla crece con uno más;
   - tiene pista de alfombras a ≥ 2 m de la cama, a la que se llega desde la plataforma central;
   - todas las celdas están sobre el colchón.
2. **Las camas**: todas de 4 plazas (≥ 3,5 m de ancho), y la pareja en plazas distintas.
3. **El horario**:
   - la guerra está en el catálogo como actividad de todos a la vez;
   - en su franja la tienen todos y caben;
   - fuera de su franja no se reparte.
4. **En vivo** (la franja del sábado a las 20 h, reloj ×6):
   - ≥ 80 % llegan, todos andando y en alfombra;
   - ≥ 80 % están a la vez sobre el colchón y cogen almohada;
   - hay golpes y risas, acotados;
   - el vínculo sube;
   - sin saltos ni teletransportes;
   - al acabar dejan las almohadas.

## Regla J16: todos los miembros a la vez (hecha: ver [centros-para-todos.md](centros-para-todos.md))

Berón (2026-11-04): los centros de entrenamiento y de ocio dan servicio a todos los miembros del santuario a la vez.
- El ring cumple: caben 25 y son 19 residentes.
- El resto de centros también cumple ya: más pisos donde hacían falta, clases que se reparten por salas y ocio para todos.
  Detalle en [centros-para-todos.md](centros-para-todos.md).
- Lo comprueba la regla de capacidad (`AreaCapacity`, grupo 75), que cuenta el ring; `PillowFightTest` llama a `AreaCapacity.CheckActivities` para el ring y el ocio «para todos».
- Las actividades por turnos se apuntan en el registro.

## Las alfombras no atraviesan el ring

El ring está justo encima de la plataforma central, y las alfombras que salían de ella lo cruzaban.

Ahora el ring registra su caja (`PillowRing.Box`: plataforma, cama y cabecero, sin columnas ni alfombras) en
`FlyingCarpet.NoFlyProviders`, y `FlyingCarpet.Legs` arma el vuelo por tramos:
- desde debajo del ring: sale en recto, por debajo, hasta fuera de su huella (2 m de margen);
- desde encima (su propia pista): sube en vertical hasta por encima;
- entre esos puntos, el arco de siempre;
- si aún lo cruzara, sube por fuera, cruza en recto por encima y baja.

## Ronda 5 del probador (2026-11-04)

- **8 de 10 llegaban al ring y como mucho 8 a la vez sobre el colchón** (faltaban Adaguchi y Panterilia). Tres cambios:
  - las celdas se llenan **desde el fondo** (el cabecero) hacia la entrada del pie, así los primeros no tapan el paso a los de detrás;
  - el colchón es **blando**: sin collider, se anda sobre la plataforma y uno se hunde 0,2 m en él, porque el escalón de 0,2 m frenaba
    a algunos al entrar;
  - quien ya pisa el colchón a menos de celda y media de la suya cuenta como llegado y pelea desde ahí.
- **3 de 57 vuelos cruzaban el ring** (Plataforma central → Ring de almohadas): esa pasada era anterior a `e4f3221`.
  - `FlyingCarpet.Legs` cubre ese caso: sale en recto por debajo del ring hasta fuera de su huella, luego hace el arco o, si aún lo
    cruzara, sube por fuera y cruza por encima hasta la pista del ring, a la que baja en vertical.
  - `PillowFightTest` muestrea esos mismos tramos.
