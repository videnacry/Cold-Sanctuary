# Reglas para crear personajes del mesocosmos

Berón (2026-11-05): «debe existir una lista de reglas para la creación de personajes en el mesocosmos, entre ellos estaría la
reestructuración de puestos … recopilar información de cada personaje al detalle en un .md y la regla de actualizar los docs».

Son las reglas **P1–P10** del [manual de diseño](manual-de-diseno.md) (§9.1). La comprobación automática es
`PersonajesReglasTest` (grupo 95) y `tools/check-personajes.sh`, que no necesita Unity.

## Las reglas

| # | Regla | Dónde se mira |
|---|---|---|
| P1 | **Fuentes de un personaje, por este orden**: lo que dice Berón, su foto, su ficha `character-*.md` y los docs de diseño. **No se inventa**: lo que falta se escribe «pendiente de Berón». | Su ficha |
| P2 | **Cada persona del mesocosmos tiene su ficha** `docs/personajes/<nombre>.md` con lo que se sabe y sus huecos. | 95, `check-personajes.sh` |
| P3 | **El aspecto sale solo de la foto**: rasgos visibles (pelo, piel, ropa, complementos, complexión), con respeto y sin caricatura. Nunca datos personales que se vean en la foto, ni edades ni géneros supuestos (J13). `Fem()` solo para quien Berón nombra en femenino. Sin foto, aspecto provisional distinto del de los demás y «pendiente de foto» en el rótulo. | Galería de retratos |
| P4 | **Profesión en la tabla** `Professions.All`: quién, título y áreas o misiones. Con ella: área de salida (`SanctuaryDirector.HomeArea`, `PlaceStaff`), rótulo y, si da una clase, `ClassLeads`. | 95, 28 |
| P5 | **Parejas e hijos comparten casa** (J14; `Partners`, `Kids`). Quien llega solo tiene casa propia (`Owners`). Si no hay casa libre, se añade **una tanda de 6 casas**, una por árbol, y los árboles crecen lo que haga falta. | 65, 89 |
| P6 | **Reestructurar al llegar alguien más capaz**: cuando llega una persona con más habilidad documentada para un puesto, se revisa la jerarquía de ese puesto ese mismo día: quién dirige, quién apoya y quién lo deja. | Su ficha y la de los afectados |
| P7 | **Ningún puesto sin base documentada**: cada puesto tiene su frase de origen en `Professions.Basis`. Lo deducido por nosotros va en `Professions.Unconfirmed` hasta que Berón lo confirme. Quien no tiene base deja el puesto. | 95 |
| P8 | **W9 al llegar o irse alguien**: se revisan jornadas, reparto y horario libre (`WorkdayDirector.Staff`, `LeisureDirector.DefaultStaff`, listas de las pruebas y `SampleSceneBuilder.TestScenes.Staff`). | 28, 50, 65 |
| P9 | **J16 al llegar alguien**: se recuentan miembros y residentes frente a cada centro (`AreaCapacity`, `TrainingCentres.FloorsFor`), el ring y la cama-litera; si no llegan, se añade un piso o se agranda. | 75, 89 |
| P10 | **Los docs van con el cambio**: su ficha, [personajes-mesocosmos.md](personajes-mesocosmos.md), [diseno-personajes.md](diseno-personajes.md), el manual, el checklist y los docs del área que toca. Un cambio de personaje sin docs no está terminado. | `check-personajes.sh` |

## Lista para añadir a alguien

1. Fuentes (P1) y ficha (P2).
2. `SampleSceneBuilder.FamilyCharacters` (id, rótulo, x libre, ropa, stats cercanas a 1) y `PlaceStaff`.
3. `CubeHair.For`, por el id **exacto** (el nombre puede salir en un microcosmos).
4. `WorkdayDirector.Staff`, `LeisureDirector.DefaultStaff`, `SampleSceneBuilder.TestScenes.Staff` y las listas de las pruebas.
5. `Professions.All`, `Professions.Basis` y, si hace falta, `Unconfirmed` y `ClassLeads`; `SanctuaryDirector.HomeArea`.
6. Su plan de gimnasio en `GymProgramming` (sin él, cuerpo entero de salud).
7. Casa (P5), recuento (P9) y jerarquía (P6).
8. Docs (P10).

Si alguien se va a un microcosmos (J12), se le quita de todo eso, pero se conservan su aspecto y sus datos.

## Auditoría de puestos (2026-11-05)

**Hecho en esta auditoría:**
- **Panterilia deja el yoga**. Fue segunda maestra por deducción («la de la percepción»), sin base de Berón.
- **Consol, jefa de Ashtanga; Josedrilo, jefe de Hatha.** Berón: «ahora que ha llegado Consol el segundo puesto se le pondría a ella siendo
  que existe documentación de su gran habilidad, de hecho su descripción la hace candidata a ser superior a Josedrilo».

**Pendiente de Berón** (`Professions.Unconfirmed`):

| Persona | Lo que no tiene base | Qué hay |
|---|---|---|
| Gohageneis | **Artes marciales** | Salió del reparto provisional («ágil y atlético, y ya es de la Guardia»), al irse Goku. Berón solo dijo guardia y maestro de baile. La experta documentada es Luciana: por P6 podría dirigirlas ella. Se turnan por días. |
| Goluis | **Jefe de cocina** | Lo dicen su ficha y `world-simulation.md`, pero Berón nombró jefes a Maite y Amykhuz. No tiene entrada en `Professions`. |
| Hospital | **Los 4 puestos veterinarios** (H2) | Berón pidió 4 humanos y 4 veterinarios. Los humanos tienen 4 médicas (Lizmibet, Linduarda, Ivelyna, Malena). Ninguna persona está documentada como veterinaria: los cubre la rotación. |
| Gallarle | **Sin profesión** | Pendiente desde el 2026-11-04. |
| Panterilia | **Limpieza y farmacia**; sale en el huerto | Lo dice su ficha antigua; Berón solo citó baile y finanzas. Sale en el huerto por la farmacia, que no tiene área. |
| Luciana | **Control de misión de la lanzadera** | Deducido de «piloto». |
| Maite | **Curas del hospital** (`enf_visita`) | Deducido de «curandera»; `hos_plantas` también es misión nuestra. |
| Shalva | **Jardinero frutal** (`jar_frutales`) | Deducido de «cultivo». |
| Yulo, Lale | **Biología y fotobiorreactores** | Deducido de «biotecnología». |
| Adaguchi | **Toda el área de Construcción** | Deducido de «submarinismo»: los submarinos salen de allí. |

**Textos viejos sin efecto en el juego:** las razones de los planes de gimnasio de Nesvitor («el mayor de las cuentas»), Ducristin («mecánico»),
Maite («registros y vacunación») y Alicita («huerto») y varios comentarios en `SanctuaryDirector` son del reparto provisional.

**Arreglado de paso:** el plan de gimnasio de Amykhuz seguía bajo su nombre viejo, «Amahuz», y por eso iba con el plan genérico; la prueba 87
también lo usaba.
