# Peticiones de Berón (PB)

<!-- GENERADO por docs/qa/migracion/generar.pl el 2026-11-13 desde docs/auditoria-peticiones.md, docs/manual-de-diseno.md y los tests. Desde aquí se mantiene a mano. -->

Cada petición de Berón, con su número fijo. **Una petición nueva entra el mismo día** como la siguiente PB libre (ver [README.md](README.md) §Cómo entra una petición).
El **estado no se escribe a mano**: sale de sus casos en [registro.md](registro.md) (lo comprueba el comprobador):
`Hecha` si todos sus casos están `Probado (…)`; `Doc` si solo pedía un doc o una regla; `Sin verificar` si alguno está en `Solo código` o `Sin probar`; `Abierta` si tiene un `HC` o le falta código.

Las PB-001…PB-155 vienen de [auditoria-peticiones.md](../auditoria-peticiones.md) (el número es el mismo). La siguiente libre es **PB-156**.

| PB | Fecha | Petición | Reglas | Casos | Estado | Origen |
|---|---|---|---|---|---|---|
| PB-001 | 2026-10-27 | Sin escaleras, puentes ni barreras de borde; se salta entre plataformas sobre cojines de musgo | S5 | ARB-01 | Hecha | auditoría #1: Verificado en vivo (2026-11-13) |
| PB-002 | 2026-10-27 | Todos los pisos del mismo tamaño (SetBack 0) | — | ARB-02 | Hecha | auditoría #2: Verificado en vivo (2026-11-13) |
| PB-003 | 2026-10-27 | Alfombras de 4 tipos (Edificio/Santuario/Residencia/Habitación) y rutas de NPC por `CarpetPad.Route` | — | ARB-03 | Sin verificar | auditoría #3: Sin anotar |
| PB-004 | 2026-10-27 | Muebles para la persona más alta (1,85 m: camilla 2,00 × 0,70, asientos a 0,45) | — | ARB-04 | Hecha | auditoría #4: Verificado en vivo (2026-11-13) |
| PB-005 | 2026-10-27 | Todo microcosmos sale de la plantilla (fecha, hora, piso) | M1 | MIC-01 | Sin verificar | auditoría #5: Sin anotar |
| PB-006 | 2026-10-27 | Ambrosio en el hospital al alba de la taiga; Sakshi segunda anciana; mariquita de 2,4 m como depredador | — | PX-01 | Hecha | auditoría #6: Verificado en vivo (2026-11-12) |
| PB-007 | 2026-10-28 | 2 m de paso entre lo que no va junto | S7 | ARB-05 | Hecha | auditoría #7: Verificado en vivo (2026-11-12) |
| PB-008 | 2026-10-28 | Anillo (RingDeck) opcional, sin escaleras, unido por alfombras | S5 | ARB-06 | Hecha | auditoría #8: Verificado en vivo (2026-11-12) |
| PB-009 | 2026-10-28 | Octógonos con megacolumna CFST por esquina para 30 pisos y aviso si se pasa | S9 | ARB-07 | Sin verificar | auditoría #9: Sin anotar |
| PB-010 | 2026-10-28 | Plazas reservadas en cada piso para flora, fauna, personajes e historia | S10 | ARB-08 | Hecha | auditoría #10: Verificado en vivo (2026-11-12) |
| PB-011 | 2026-10-28 | Las iniciaciones las da el trabajador de turno (la Magnate solo acompaña) | — | JOR-01 | Hecha | auditoría #11: Verificado en vivo (2026-11-12) |
| PB-012 | 2026-10-29 | El microcosmos es SOLO su piso, amurallado; nada fuera | M5 | MIC-02 | Hecha | auditoría #12: Verificado en vivo (2026-11-12) |
| PB-013 | 2026-10-29 | Iniciaciones como «orquesta» (≤ 2 m, cada uno anda solo, sin tirones ni teletransportes) | — | JOR-02 | Hecha | auditoría #13: Verificado en vivo (2026-11-12) |
| PB-014 | 2026-10-29 | Pocas torres temáticas con muchos pisos (Servicios, Entrenamiento, Ciencias, Producción) | S2 | ARB-09 | Hecha | auditoría #14: Verificado en vivo (2026-11-12) |
| PB-015 | 2026-10-30 | Árbol v2: tronco cuadrado, hélice, primer piso ≥ 50 m, entradas solo abajo y en el techo, nado entre niveles | T1, T2 | ARB-10 | Hecha | auditoría #15: Verificado en vivo (2026-11-12) |
| PB-016 | 2026-10-31 | Ramas 5 × 5 y puertas 3 × 3 → sustituido por puertas de 7 × 7 (8 personas y submarino) | — | ARB-11 | Hecha | auditoría #16: Verificado en vivo (2026-11-12) |
| PB-017 | 2026-10-31 | Migrar todos los edificios al modelo árbol ANTES de arreglar los fallos de pruebas | S3 | ARB-12 | Sin verificar | auditoría #17: Sin anotar |
| PB-018 | 2026-10-31 | Llenar las plataformas grandes de alfombras con un frutal de cada tipo, experta, sala de cuidados y jardinero diario | G1 | ARB-13 | Hecha | auditoría #18: Verificado en vivo (2026-11-12) |
| PB-019 | 2026-10-31 / 11-04 / 11-05 | Árbol guardián (excepción de altura → injerto récord → sin túnel → retirado) | — | ARB-14 | Sin verificar | auditoría #19: Sin verificar en vivo |
| PB-020 | 2026-11-01 | Reglas 1–4 de los micro: paso de 2 personas, sin muros ni techos, muebles detallados con utensilios, necesidades básicas | — | MIC-03 | Hecha | auditoría #20: Verificado en vivo |
| PB-021 | 2026-11-01 | Pueblos de época que llenan el piso; solo insectos-humanos civilizados; fauna solo si la época la tenía | — | MIC-04 | Hecha | auditoría #21: Verificado en vivo |
| PB-022 | 2026-11-01 | 12 figuras: Newton, Einstein, Tesla, Curie, Nietzsche, Bolívar, Pitágoras, Ananenko y su escuadra, Jenner, Patterson, Heródoto, al-Juarismi | — | MIC-05 | Sin verificar | auditoría #22: Sin verificar en vivo |
| PB-023 | 2026-11-02 | Un HOGAR para cada personaje del pueblo, más trabajo y servicios | M18 | MIC-06 | Sin verificar | auditoría #23: Sin anotar |
| PB-024 | 2026-11-02 | Ciclo de vida igual para humanos e insectos-humanos, con prueba como las escenas CicloVida | M19 | MIC-07 | Hecha | auditoría #24: Verificado en vivo (2026-11-12) |
| PB-025 | 2026-11-02 | Escala ×100 (todo relativo a `AreaScale`) | M11 | MIC-08 | Hecha | auditoría #25: Verificado en vivo (2026-11-12) |
| PB-026 | 2026-11-02 | Se permiten los muros históricos que pide la historia (gueto, reactor) | — | MIC-09 | Sin verificar | auditoría #26: Sin anotar |
| PB-027 | 2026-11-02 | Camas y servicios adaptados al insecto-humano y del estilo de la época; catálogo de estilos reutilizable | — | MIC-10 | Hecha | auditoría #27: Verificado en vivo |
| PB-028 | 2026-11-03 | El piso nunca se rompe; el pueblo se adapta | — | MIC-11 | Hecha | auditoría #28: Verificado en vivo (2026-11-12) |
| PB-029 | 2026-11-03 | Los insectos-humanos trepan; los muebles son suelo elevado | — | MIC-12 | Hecha | auditoría #29: Verificado en vivo (2026-11-12) |
| PB-030 | 2026-11-03 | Relieve realista (montes, lagos, acantilados) | — | FAU-01 | Hecha | auditoría #30: Verificado en vivo (2026-11-12) |
| PB-031 | 2026-11-03 | Frutales en las residencias del SANTUARIO | — | ARB-15 | Hecha | auditoría #31: Verificado en vivo (2026-11-12) |
| PB-032 | 2026-11-03 | Pueblos rehechos sobre mapas reales con edificios de actividades | — | MIC-13 | Hecha | auditoría #32: Verificado en vivo |
| PB-033 | 2026-11-03 | Cada estructura con su interacción (trabajo/descanso/recuperación/ocio) y en las actividades semanales | — | JOR-03 | Hecha | auditoría #33: Verificado en vivo (2026-11-12) |
| PB-034 | 2026-11-03 | Horario libre semanal con el reparto rotatorio | — | JOR-04 | Hecha | auditoría #34: Verificado en vivo (2026-11-12) |
| PB-035 | 2026-11-03 | Figuras del mesocosmos: Musk, Goku, Gates, Buffett, MacKenzie Scott, Traoré (luego, a sus microcosmos) | — | MIC-14 | Hecha | auditoría #35: Verificado en vivo (2026-11-12) |
| PB-036 | 2026-11-03 | Microcosmos maya | — | MIC-15 | Hecha | auditoría #36: Verificado en vivo |
| PB-037 | 2026-11-03 | Microcosmos sobre la forma TERMINADA del piso; dos formas de cada piso | — | MIC-16 | Hecha | auditoría #37: Verificado en vivo |
| PB-038 | 2026-11-03 | «Pendiente de piso representativo» → crear esos pisos | — | ARB-16 | Abierta | auditoría #38: Abierto |
| PB-039 | 2026-11-03 | Máscaras de oxígeno en entradas + estaciones; pruebas del jugador y de Kushal bajo el agua | — | ARB-17 | Hecha | auditoría #39: Verificado en vivo (2026-11-12) |
| PB-040 | 2026-11-03 | Tareas del jardinero: revisar máscaras y fitoplancton; reparto regenerado | — | ARB-18 | Hecha | auditoría #40: Verificado en vivo (2026-11-12) |
| PB-041 | 2026-11-03 | Árboles sin suelo, para bajar nadando al mar y que entre la fauna marina | — | FAU-02 | Hecha | auditoría #41: Verificado en vivo (2026-11-12) |
| PB-042 | 2026-11-03 | Puertas y ramas para 8 personas a la vez | — | ARB-19 | Hecha | auditoría #42: Verificado en vivo (2026-11-12) |
| PB-043 | 2026-11-03 | Peces, kril, medusas y abejas como GRUPOS | — | FAU-03 | Hecha | auditoría #43: Verificado en vivo (2026-11-12) |
| PB-044 | 2026-11-03 | Frutales en los hogares de los microcosmos | — | MIC-17 | Hecha | auditoría #44: Verificado en vivo |
| PB-045 | 2026-11-03 | Dejar siempre los docs y el checklist al día | — | OP-09 | Sin verificar | auditoría #45: Sin anotar |
| PB-046 | 2026-11-03 | Jardines frutales en los techos de alfombras; compañeras, insectos, tierra frente a maceta | — | ARB-20 | Hecha | auditoría #46: Verificado en vivo (2026-11-12) |
| PB-047 | 2026-11-03 | Logística de obra: submarino anfibio de obra, tronco y ramas más grandes, garaje, prueba de entrega | — | ARB-21 | Hecha | auditoría #47: Verificado en vivo (2026-11-12) |
| PB-048 | 2026-11-03 | Submarino-hospital para personas y animales grandes | — | HOS-01 | Hecha | auditoría #48: Verificado en vivo (2026-11-12) |
| PB-049 | 2026-11-03 | Atención a ballenas y orcas; hospital submarino colosal | — | HOS-02 | Hecha | auditoría #49: Verificado en vivo (2026-11-12) |
| PB-050 | 2026-11-03 | Banco de sangre | — | HOS-03 | Hecha | auditoría #50: Verificado en vivo (2026-11-12) |
| PB-051 | 2026-11-03 | Paredes de cristal en salas aisladas | — | HOS-04 | Hecha | auditoría #51: Verificado en vivo (2026-11-12) |
| PB-052 | 2026-11-04 | Yoga: sin anillo, pisos Hatha y Ashtanga, occidental/tradicional, inmersión en el río | — | YOG-01 | Hecha | auditoría #52: Verificado en vivo (2026-11-12) |
| PB-053 | 2026-11-04 | La vegetación SIEMPRE encima, nunca debajo (Ambrosio) | — | MIC-18 | Hecha | auditoría #53: Verificado en vivo (2026-11-12) |
| PB-054 | 2026-11-04 | Capa de tierra de 10 cm → luego apagada por pesada | — | MIC-19 | Hecha | auditoría #54: Verificado en vivo (2026-11-12) |
| PB-055 | 2026-11-04 | 3 versiones de piso; elementos meso/micro; muebles como terreno | — | MIC-20 | Hecha | auditoría #55: Verificado en vivo (2026-11-12) |
| PB-056 | 2026-11-04 | Una historia puede rehacer su microcosmos desde la plantilla del piso | — | MIC-21 | Hecha | auditoría #56: Verificado en vivo (2026-11-12) |
| PB-057 | 2026-11-04 | Hospital propio del microcosmos y revisión al cambiar el piso meso; gancho que lo recuerde | M9 | MIC-22 | Hecha | auditoría #57: Verificado en vivo (2026-11-12) |
| PB-058 | 2026-11-04 | Alfombras más gruesas | — | ARB-22 | Hecha | auditoría #58: Verificado en vivo (2026-11-13) |
| PB-059 | 2026-11-04 | Las abejas del santuario deslumbran | — | REN-01 | Sin verificar | auditoría #59: Sin verificar en vivo |
| PB-060 | 2026-11-04 | Plaza de Bolívar: asientos usables, platos y ollas, escala, estatua y fuente | — | MIC-23 | Hecha | auditoría #60: Verificado en vivo (2026-11-12) |
| PB-061 | 2026-11-04 | Versiones insecto-humano de las estatuas | — | MIC-24 | Hecha | auditoría #61: Verificado en vivo (2026-11-12) |
| PB-062 | 2026-11-04 | Conversaciones por las ánimas y escena de prueba | — | PER-01 | Hecha | auditoría #62: Verificado en vivo (2026-11-12) |
| PB-063 | 2026-11-04 | Manual de diseño con todas las reglas | — | — | Doc | auditoría #63: Hecho (doc) |
| PB-064 | 2026-11-04 | Al cambiar piso/área/árbol/microcosmos, revisar jornadas, reparto y horario libre (gancho W9) | — | MIC-25 | Sin verificar | auditoría #64: Sin anotar |
| PB-065 | 2026-11-04 | Una cama por casa: litera de 2 × 4 con tobogán | — | JOR-05 | Hecha | auditoría #65: Verificado en vivo (2026-11-12) |
| PB-066 | 2026-11-04 | Ring de guerra de almohadas para todos los residentes | — | JOR-06 | Hecha | auditoría #66: Verificado en vivo (2026-11-12) |
| PB-067 | 2026-11-04 | Piso de Informática (IA local, agua y energía) | — | ARB-23 | Hecha | auditoría #67: Verificado en vivo (2026-11-12) |
| PB-068 | 2026-11-04 | Aula universitaria | — | ARB-24 | Hecha | auditoría #68: Verificado en vivo (2026-11-12) |
| PB-069 | 2026-11-04 | Lanzadera de cohetes en un casquete y regla de altura | — | NUC-01 | Hecha | auditoría #69: Verificado en vivo (2026-11-12) |
| PB-070 | 2026-11-04 | Alerta de piso nuevo para los microcosmos pendientes | — | MIC-26 | Hecha | auditoría #70: Verificado en vivo (2026-11-12) |
| PB-071 | 2026-11-04 | Capacidad: maquinaria para varios a la vez | — | ARB-25 | Hecha | auditoría #71: Verificado en vivo (2026-11-12) |
| PB-072 | 2026-11-04 | Los centros de entrenamiento sirven a todos a la vez | — | ARB-26 | Hecha | auditoría #72: Verificado en vivo (2026-11-12) |
| PB-073 | 2026-11-04 | Pijamadas con literas | — | JOR-07 | Hecha | auditoría #73: Verificado en vivo (2026-11-12) |
| PB-074 | 2026-11-04 | Flora y fauna nunca bajo el piso: los pobladores sueltan desde el aire | — | MIC-27 | Hecha | auditoría #74: Verificado en vivo (2026-11-12) |
| PB-075 | 2026-11-04 | Mapa hecho a mano por historia; la siembra al azar, solo de respaldo | — | MIC-28 | Hecha | auditoría #75: Verificado en vivo (2026-11-12) |
| PB-076 | 2026-11-04 | La comida, orquestada (utensilios, bocados) | — | JOR-08 | Hecha | auditoría #76: Verificado en vivo (2026-11-12) |
| PB-077 | 2026-11-04 | Toda actividad se HACE en su sitio (humanos e insectos-humanos) | — | JOR-09 | Hecha | auditoría #77: Verificado en vivo (2026-11-12) |
| PB-078 | 2026-11-04 | Tareas e iniciaciones del jugador con E (modelo Ambrosio) | — | PX-02 | Hecha | auditoría #78: Verificado en vivo (2026-11-12) |
| PB-079 | 2026-11-04 | Gimnasio real (series, repeticiones, rutinas semanales) y el entrenador | — | GYM-01 | Hecha | auditoría #79: Verificado en vivo (2026-11-12) |
| PB-080 | 2026-11-04 | Uniformes por tarea | — | JOR-10 | Hecha | auditoría #80: Verificado en vivo (2026-11-12) |
| PB-081 | 2026-11-04 | Lugares de entrenamiento de época en los pueblos y planes para insectos-humanos | — | MIC-29 | Hecha | auditoría #81: Verificado en vivo (2026-11-12) |
| PB-082 | 2026-11-04 | Rellenar huecos históricos con mapas actuales (solo posiciones) | — | MIC-30, HC-02 | Abierta | auditoría #82: Sin verificar; una parte abierta |
| PB-083 | 2026-11-04 | Personajes: parejas, profesiones, Andrea la más fuerte y entrenadora, 6 nuevos, Neo, renombres | — | PER-02 | Hecha | auditoría #83: Verificado en vivo (2026-11-12) |
| PB-084 | 2026-11-04 | Guardería o juego para Neo (con Ivelyna) | — | JOR-11 | Abierta | auditoría #84: Abierto |
| PB-085 | 2026-11-04 | Las obras avanzan a la vista, pieza a pieza | — | ARB-27 | Hecha | auditoría #85: Verificado en vivo (2026-11-12) |
| PB-086 | 2026-11-05 | Planta atómica lo más segura posible (bajo el lecho o el suelo) | — | NUC-02 | Hecha | auditoría #86: Verificado en vivo (2026-11-12) |
| PB-087 | 2026-11-05 | Microcosmos de Eloy Alfaro, Galvarino, Pedro Sánchez, Atahualpa, Cleopatra, Beethoven, Descartes, Che, Lincoln | — | MIC-31 | Hecha | auditoría #87: Verificado en vivo (2026-11-12) |
| PB-088 | 2026-11-05 | Shakira, Bad Bunny, John Lennon, Michael Jackson | — | MIC-32 | Hecha | auditoría #88: Verificado en vivo (2026-11-12) |
| PB-089 | 2026-11-05 | Nativos americanos (Gran Ley de la Paz) y las calculadoras de la NASA | — | MIC-33 | Hecha | auditoría #89: Verificado en vivo (2026-11-12) |
| PB-090 | 2026-11-05 | UI: horario sobre F10, recursos en una ventana, ventana de misiones y jornada | F10 | UI-01 | Hecha | auditoría #90: Verificado en vivo (2026-11-12) |
| PB-091 | 2026-11-05 | Consol (maestra de Ashtanga, 2.º puesto de yoga o más) | — | YOG-02 | Hecha | auditoría #91: Verificado en vivo (2026-11-12) |
| PB-092 | 2026-11-05 | Fernando y las tareas solares en la jornada | — | ENE-01 | Sin verificar | auditoría #92: Sin verificar en vivo |
| PB-093 | 2026-11-05 | Paneles solares en cada piso nuevo según su demanda | — | ENE-02 | Hecha | auditoría #93: Verificado en vivo (2026-11-12) |
| PB-094 | 2026-11-05 | Casquete de energía lejano (solar, eólica, hidráulica seguras) | — | ENE-03 | Hecha | auditoría #94: Verificado en vivo (2026-11-12) |
| PB-095 | 2026-11-05 | Morsa y pingüino emperador | — | FAU-04 | Hecha | auditoría #95: Verificado en vivo (2026-11-12) |
| PB-096 | 2026-11-05 | Lista de reglas para crear animales | — | — | Doc | auditoría #96: Hecho (doc) |
| PB-097 | 2026-11-05 | Casquetes: cuevas, mezcla 50/50 con isla, renos/lobos/conejos, flora de la isla, reubicar edificios | — | FAU-05 | Hecha | auditoría #97: Verificado en vivo (2026-11-12) |
| PB-098 | 2026-11-05 | Reglas para crear personajes; la jerarquía se rehace cuando llega un perfil más fuerte | P1, P10 | — | Doc | auditoría #98: Hecho (doc) |
| PB-099 | 2026-11-05 | Un .md por personaje; los puestos documentados | — | — | Doc | auditoría #99: Hecho (doc) |
| PB-100 | 2026-11-05 | BLACKPINK, una banda de rock, una de rap, «grandes aportes de las canciones» | — | MIC-34 | Hecha | auditoría #100: Verificado en vivo (2026-11-12) |
| PB-101 | 2026-11-05 | Microcosmos de presidentes (hazañas) y Lee Kuan Yew | — | MIC-35 | Hecha | auditoría #101: Verificado en vivo (2026-11-12) |
| PB-102 | 2026-11-05 | Una torre en la isla (transporte aéreo) | — | ARB-28 | Hecha | auditoría #102: Verificado en vivo (2026-11-12) |
| PB-103 | 2026-11-05 | Regla de la cadena alimenticia; especie real para peces y medusas | — | FAU-06 | Hecha | auditoría #103: Verificado en vivo (2026-11-12) |
| PB-104 | 2026-11-05 | Área de música (escenarios, estudios, cristal) | — | ARB-29 | Hecha | auditoría #104: Verificado en vivo (2026-11-12) |
| PB-105 | 2026-11-05 | Atahualpa: la vida inca y la bienvenida | — | MIC-36 | Hecha | auditoría #105: Verificado en vivo (2026-11-12) |
| PB-106 | 2026-11-05 | Regla W10: lo altruista, bello o de superación | — | — | Doc | auditoría #106: Hecho (doc) |
| PB-107 | 2026-11-05 | Ideas para que las turbinas no maten aves (cría de aves, figuras, dragón de drones, aves que avisan) | — | MIC-37 | Abierta | auditoría #107: Abierto |
| PB-108 | 2026-11-05 | Sísmica: no; túnel-tren rápido al lecho | — | NUC-03 | Hecha | auditoría #108: Verificado en vivo (2026-11-12) |
| PB-109 | 2026-11-05 | Deshierbe cuidadoso | — | FAU-07 | Hecha | auditoría #109: Verificado en vivo (2026-11-12) |
| PB-110 | 2026-11-05 | Mejorar los microcosmos viejos | — | MIC-38 | Sin verificar | auditoría #110: Sin verificar en vivo |
| PB-111 | 2026-11-05 | Regla de reglas W11 | — | — | Doc | auditoría #111: Hecho (doc) |
| PB-112 | 2026-11-05 | Profundidad real del lecho y túnel de menos de 1 min | — | NUC-04 | Hecha | auditoría #112: Verificado en vivo (2026-11-12) |
| PB-113 | 2026-11-05 | Columnas y techo en las salas subterráneas | T16 | ARB-30 | Hecha | auditoría #113: Verificado en vivo (2026-11-12) |
| PB-114 | 2026-11-05 | Fuera el comedero del oso; alimentador por rondas con lista compartida | — | FAU-08 | Hecha | auditoría #114: Verificado en vivo (2026-11-12) |
| PB-115 | 2026-11-05 | Las tareas tienen requisitos | — | JOR-12 | Hecha | auditoría #115: Verificado en vivo (2026-11-12) |
| PB-116 | 2026-11-05 | 1.ª hora de entrenamiento y 2.ª opcional → 2 h al día | — | JOR-13 | Hecha | auditoría #116: Verificado en vivo (2026-11-12) |
| PB-117 | 2026-11-05 | Sala de asambleas investigada por época | — | MIC-39 | Hecha | auditoría #117: Verificado en vivo (2026-11-12) |
| PB-118 | 2026-11-05 | Áreas mezcla de lo mejor del mundo (S14/F14) | S14, F14 | ARB-31 | Hecha | auditoría #118: Verificado en vivo (2026-11-12) |
| PB-119 | 2026-11-05 | Frases con fuente (verificada o inventada) | — | PER-03 | Sin verificar | auditoría #119: Hecho (doc); sin verificar |
| PB-120 | 2026-11-05 | Techos de cristal y rampas permitidos | — | ARB-32 | Sin verificar | auditoría #120: Sin verificar en vivo |
| PB-121 | 2026-11-05 | «Hazlo real»: el hielo flota, la roca llega al lecho, refugio en el fondo | — | NUC-05 | Sin verificar | auditoría #121: Sin verificar en vivo |
| PB-122 | 2026-11-05 | Ningún depredador con hambre; la lista se renueva solo cuando está completa | — | FAU-09 | Hecha | auditoría #122: Verificado en vivo (2026-11-12) |
| PB-123 | 2026-11-05 | Producción: la maquinaria crece con lo que se pide | — | ARB-33 | Hecha | auditoría #123: Verificado en vivo (2026-11-12) |
| PB-124 | 2026-11-05 | Lanzadera protegida de icebergs; fauna bajo el hielo y respiraderos | — | FAU-10 | Hecha | auditoría #124: Verificado en vivo (2026-11-12) |
| PB-125 | 2026-11-05 | Refugio para 1 mes (desalinizar, aire, huerto, huevos) | — | NUC-06 | Sin verificar | auditoría #125: Sin verificar en vivo |
| PB-126 | 2026-11-05 | Iniciación como tarea; primero quien menos sabe; nadie tiene hora fija de clase | — | JOR-14 | Hecha | auditoría #126: Verificado en vivo (2026-11-12) |
| PB-127 | 2026-11-05 | Santuario 1 = tierra y mar somero; el mar profundo, al Santuario Marino | — | NUC-07 | Hecha | auditoría #127: Verificado en vivo (2026-11-12) |
| PB-128 | 2026-11-05 | El refugio crece hasta que se pueda vivir en él, con áreas nuevas | — | NUC-08 | Sin verificar | auditoría #128: Sin anotar |
| PB-129 | 2026-11-05 | Icebergs con animales reales y sus motivos | — | FAU-11 | Hecha | auditoría #129: Verificado en vivo (2026-11-12) |
| PB-130 | 2026-11-05 | Documentar el túnel y el anillo por si se vuelven a juntar los santuarios | — | — | Doc | auditoría #130: Hecho (doc) |
| PB-131 | 2026-11-05 | Santuario 3 Aéreo con cohetes y viento/agua | — | NUC-09 | Hecha | auditoría #131: Verificado en vivo (2026-11-12) |
| PB-132 | 2026-11-05 | Fauna fría nativa en los santuarios 2 y 3 | — | FAU-12 | Hecha | auditoría #132: Verificado en vivo (2026-11-12) |
| PB-133 | 2026-11-05 | Guardia en un piso de torre con pantallas (icebergs, sismos, tsunamis) | — | ARB-34 | Hecha | auditoría #133: Verificado en vivo (2026-11-12) |
| PB-134 | 2026-11-05 | Quitar los árboles artificiales (menos insectos) | — | ARB-35 | Sin verificar | auditoría #134: Sin verificar en vivo |
| PB-135 | 2026-11-05 | Casquetes silvestres con estructuras para la fauna polar | — | ARB-36 | Hecha | auditoría #135: Verificado en vivo (2026-11-12) |
| PB-136 | 2026-11-05 | Árbol cazador: residencia → piso de investigación → bajo el agua con abejas y arañas | — | ARB-37 | Hecha | auditoría #136: Verificado en vivo (2026-11-12) |
| PB-137 | 2026-11-05 | Residencia a la isla y en edificio-árbol; casas de árbol guardadas | — | ARB-38 | Hecha | auditoría #137: Verificado en vivo (2026-11-12) |
| PB-138 | 2026-11-05 | Focos brillantes en vivo: revisar medusas y todo, verificado en vivo | — | REN-02 | Sin verificar | auditoría #138: Sin anotar |
| PB-139 | 2026-11-08 | Casa en santuarioN nivelM; solo hacia atrás; reparto diario por sitios | — | JOR-15 | Hecha | auditoría #139: Verificado en vivo (2026-11-12) |
| PB-140 | 2026-11-08 | bonusPacks (Irosene, Lizmibet, Linduarda) | — | HC-01 | Abierta | auditoría #140: Abierto |
| PB-141 | 2026-11-08 | Para subir hay que superar al animal más poderoso; la magia como camino | — | FAU-13 | Hecha | auditoría #141: Verificado en vivo (2026-11-12) |
| PB-142 | 2026-11-08 | Planta nuclear = santuario 2, entera bajo el agua; solo la estación arriba | — | NUC-10 | Hecha | auditoría #142: Verificado en vivo (2026-11-12) |
| PB-143 | 2026-11-08 | Horas a partes iguales, clones, nada a mañana, áreas de varios pisos cuentan como una | — | JOR-16 | Hecha | auditoría #143: Verificado en vivo (2026-11-12) |
| PB-144 | 2026-11-08 | No encoger salas ni maquinaria (fiestas para todos) | J16 | ARB-39 | Hecha | auditoría #144: Verificado en vivo (2026-11-12) |
| PB-145 | 2026-11-08 | Nombres reales ocultos pero documentados | — | PER-04 | Hecha | auditoría #145: Verificado en vivo (2026-11-12) |
| PB-146 | 2026-11-08 | Cocina: Goluis encargado; Maite y Amykhuz los rangos más altos | — | PER-05 | Hecha | auditoría #146: Verificado en vivo (2026-11-12) |
| PB-147 | 2026-11-08 | Segunda fachada de fitoplancton y Huerto LED | — | ARB-40 | Hecha | auditoría #147: Verificado en vivo (2026-11-12) |
| PB-148 | 2026-11-08 | Varias tareas a la vez en un área; las opcionales se saltan si nadie está libre | — | JOR-17 | Hecha | auditoría #148: Verificado en vivo (2026-11-12) |
| PB-149 | 2026-11-09 | Solo lo básico terminado; ciencias, asambleas, estudio… en obra | — | ARB-41 | Hecha | auditoría #149: Verificado en vivo (2026-11-12) |
| PB-150 | 2026-11-09 | Control de misión también en obra | — | ARB-42 | Hecha | auditoría #150: Verificado en vivo (2026-11-12) |
| PB-151 | 2026-11-09 | Fauna = K × humanos (K = 3), por la pirámide trófica | E11 | JOR-18 | Hecha | auditoría #151: Verificado en vivo (2026-11-12) |
| PB-152 | 2026-11-09 | Todo opaco, sin brillo, materiales eficientes | — | REN-03 | Hecha | auditoría #152: Verificado en vivo (2026-11-13) |
| PB-153 | 2026-11-09 | Sin insectos en el santuario 1 por ahora | — | FAU-14 | Hecha | auditoría #153: Verificado en vivo (2026-11-12) |
| PB-154 | 2026-11-09 | Se pueden quitar las sombras o las columnas de los árboles | — | ARB-43 | Sin verificar | auditoría #154: Sin anotar |
| PB-155 | 2026-11-09 | ¿Por qué pesa más que GTA5? (medido: 395 ms/frame de scripts) | — | REN-04 | Abierta | auditoría #155: Abierto |

**Recuento:** Abierta 6 · Doc 7 · Hecha 121 · Sin verificar 21 · total 155.
