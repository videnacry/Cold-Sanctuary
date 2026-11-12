# Kushal (el jugador)

> Ficha con la plantilla de [README.md](README.md). Id en el código: `Player`. Revisada: 2026-11-08.
> Veracidad de cada dato (regla M39, [veracidad-frases.md](../veracidad-frases.md)): **documentada** = lo dijo Berón (sus mensajes o un
> doc de diseño que él dictó); **atestiguada** = se ve en su hoja de diseño; **recreada** = decisión de juego hecha a partir de un dato
> suyo; **inventada** = la puso el juego o un agente, sin base. ⚠️ = el doc lo marca «confirmar». **Falta** = no se sabe.

## 1. Identidad y aspecto

- Hombre: los docs y el código hablan de él en masculino («él contesta»). *documentada · [KushalVoice.cs:5](../../Assets/Scripts/Dialogue/KushalVoice.cs#L5)*
- 1,75 m. *documentada (DEVLOG) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [medidas-muebles.md](../medidas-muebles.md)*
- Media melena negra, gorro de lana rojo, gafas redondas, barba de unos días, piel clara rosada; chaleco morado con cuello de borrego y chapas sobre camiseta azul marino, vaqueros verde oscuro con cadena, muñequera, zapatillas grises de bota, tatuaje en el brazo y un rifle colgado a la espalda. *atestiguada (su hoja de diseño) · [diseno-personajes.md §Fichas](../diseno-personajes.md) · [CubeHair.cs:143](../../Assets/Scripts/Avatar/CubeHair.cs#L143)*
- Nombre «de origen nepalí/indio». *inventada (inferido) · [CubeHair.cs:139](../../Assets/Scripts/Avatar/CubeHair.cs#L139)*. Nacionalidad y edad: **Falta** ([diseno-personajes.md §Pendiente](../diseno-personajes.md)).

## 2. Origen e historia

- Llega al santuario con otros recién llegados; la Enfermería lo recibe y, por la máquina de avatares, baja sin saberlo al Microcosmos. *documentada · [novela.md §Prólogo](../novela.md)*
- Su primer viaje: un gusano que defiende a la cría Sakshi en una orilla lejana. *documentada · [microcosmos-sakshi-origin.md §1-§2](../microcosmos-sakshi-origin.md) · [novela.md §Nivel 0](../novela.md)*
- Quiere darle consciencia a Sakshi; Linduarda le deja volver al Microcosmos a cambio de tareas. *documentada · [microcosmos-eras-and-observation.md §5](../microcosmos-eras-and-observation.md) · [character-linduarda.md §Rol narrativo](../character-linduarda.md)*
- La Cría es «la razón por la que Kushal está aquí». *documentada · [cria-simulation.md](../cria-simulation.md), cabecera*
- Su vida antes del santuario: **Falta**.

## 3. Personalidad y frases

- Humilde y siempre dispuesto: cuando le hablan contesta aceptando ayudar (Berón 2026-10-29). *documentada · [KushalVoice.cs:5](../../Assets/Scripts/Dialogue/KushalVoice.cs#L5)*
- Frases de juego por contexto (ayuda, instrucción, misión, gracias…) y pensamientos al llegar, en la primera iniciación, al entrar al Microcosmos y en el duelo. *inventada (voz de juego) · [KushalVoice.cs:25-31](../../Assets/Scripts/Dialogue/KushalVoice.cs#L25)*
- El jugador no es Kushal: es el conector entre sus pensamientos y los de los seres; Kushal propone sus propias «automisiones». *documentada · [microcosmos-eras-and-observation.md §5](../microcosmos-eras-and-observation.md)*
- Gustos, miedos y aficiones: **Falta**.

## 4. Stats, margas y aptitudes

- Aptitudes «algo por encima de la media»: fuerza 1,2 · resistencia 1,2 · agilidad 1,1. *inventada · [SampleSceneBuilder.cs:270](../../Assets/Editor/SampleSceneBuilder.cs#L270)*
- Su `CharacterLevel` gana XP del farming; sus pools salen de las aptitudes. *código · ídem*
- La autorregulación es el cuidado que el jugador hace de sus stats; falta un HUD completo de ellas. *documentada · [consciousness-mechanics.md §2](../consciousness-mechanics.md)*
- Margas propias: **Falta** (las genéricas del alma).

## 5. Profesión y lo que sabe hacer

- Sin profesión: recién llegado; aprende todo por la rotación. *recreada · [jornada-y-rotacion.md](../jornada-y-rotacion.md)*
- En los mitos tempranos es «recolector estrella» con Nasatya y luego guardián de La Sembradora. *documentada · [founding-trio-stories.md §2-§3](../founding-trio-stories.md)*
- Al subir de nivel crea quimeras y acaba siendo la mano derecha de la Magnate. *documentada · [magic-metabolism-progression.md §5](../magic-metabolism-progression.md) · [sanctuary-second-lap-and-fear.md §6](../sanctuary-second-lap-and-fear.md)*

## 6. Puesto y áreas

- Hace sus misiones con «E» en los sitios marcados (modelo de Ambrosio); si las ignora, caducan. *documentada · manual J22 · [tareas-del-jugador.md](../tareas-del-jugador.md) · [PlayerTasks.cs:16](../../Assets/Scripts/Progression/PlayerTasks.cs#L16) · [PlayerWorkday.cs:25](../../Assets/Scripts/Progression/PlayerWorkday.cs#L25)*
- Su primer día son 5 h de iniciaciones. *documentada · [jornada-y-rotacion.md](../jornada-y-rotacion.md)*
- No da clases (J32, [WorkRoster.cs:274](../../Assets/Scripts/Progression/WorkRoster.cs#L274)); no le toca la ronda de frutales ([WorkRoster.cs:418](../../Assets/Scripts/Progression/WorkRoster.cs#L418)) ni a las 12 un sitio lejano (J31, [WorkRoster.cs:409](../../Assets/Scripts/Progression/WorkRoster.cs#L409)). Son límites técnicos frente a la regla de oro: [reglas-personajes.md §La regla de oro](../reglas-personajes.md).
- Empieza sin vacuna ni análisis: no sale de la enfermería hasta acabar la iniciación. *código · [HealthRecord.cs:6](../../Assets/Scripts/World/HealthRecord.cs#L6) · [jornada-y-rotacion.md §Iniciación](../jornada-y-rotacion.md)*

## 7. Relaciones

- Sakshi: la defendió de cría. *documentada · [microcosmos-sakshi-origin.md §1](../microcosmos-sakshi-origin.md)*
- Linduarda: le da permiso para el Microcosmos. *documentada · [character-linduarda.md §Rol narrativo](../character-linduarda.md)*
- La Magnate: lo elige para crear quimeras; en el final la derrota y Leo se la lleva. *documentada · [sanctuary-second-lap-and-fear.md §6](../sanctuary-second-lap-and-fear.md) · [world-topology-and-planes.md §5](../world-topology-and-planes.md)*
- Nasatya y La Sembradora (mitos de la era temprana). *documentada · [founding-trio-stories.md](../founding-trio-stories.md)*
- Familia y pareja: **Falta**. Casa propia en la arboleda ([ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16)).

## 8. Horario y costumbres

- Jornada del personal ([WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19)); entra cuando ya es miembro.
- Gimnasio: principiante, cuerpo entero 3 días. *recreada · [GymProgramming.cs:201](../../Assets/Scripts/Progression/GymProgramming.cs#L201)*
- Horario libre: es libre; el horario no lo mueve. *documentada · [horario-libre.md](../horario-libre.md)*

## 9. Microcosmos y santuario

- Entra como gusano (primer viaje) y luego como hormiga (el mirmidón de Egina). *documentada · [microcosmos-sakshi-origin.md](../microcosmos-sakshi-origin.md) · [microcosmos-civilizaciones.md](../microcosmos-civilizaciones.md)*
- Arco: Microcosmos → Macrocosmos y la primera guerra del santuario 1; final contra los socios y la Magnate en el Núcleo. *documentada («idea inicial, puede cambiar») · [world-topology-and-planes.md §5](../world-topology-and-planes.md)*
- Santuario de salida: el 1, como recién llegado. Nivel: **Falta**.

## 10. En el código

| Qué | Dónde |
|---|---|
| Nombre en escena | [SampleSceneBuilder.cs:2076](../../Assets/Editor/SampleSceneBuilder.cs#L2076) |
| Aspecto | [CubeHair.cs:143](../../Assets/Scripts/Avatar/CubeHair.cs#L143) |
| Su voz | [KushalVoice.cs](../../Assets/Scripts/Dialogue/KushalVoice.cs) |
| Jornada con E | [PlayerWorkday.cs](../../Assets/Scripts/Progression/PlayerWorkday.cs) · [PlayerTasks.cs](../../Assets/Scripts/Progression/PlayerTasks.cs) |
| Personal y gimnasio | [WorkdayDirector.cs:19](../../Assets/Scripts/Progression/WorkdayDirector.cs#L19) · [GymProgramming.cs:201](../../Assets/Scripts/Progression/GymProgramming.cs#L201) |
| Casa | [ResidentialBuilder.cs:16](../../Assets/Editor/ResidentialBuilder.cs#L16) |

## 11. Preguntas para Berón

1. ¿Su nacionalidad y su edad? El código supone un nombre nepalí o indio.
2. ¿El rifle es de dardos tranquilizantes? El santuario no mata animales.
3. ¿Qué hacía antes de llegar y por qué vino?
4. ¿Debe hacer también frutales, misiones de grupo y dar clases (regla de oro), aunque haya que resolver la técnica?
5. ¿El final de [world-topology-and-planes.md §5](../world-topology-and-planes.md) sigue en pie?
