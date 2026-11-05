# La pila de botones y sus ventanas (2026-11-05)

Berón (2026-11-05): el botón del horario libre tapaba el lanzador de hechizos, abajo a la izquierda; el panel de recursos estaba
siempre a la vista, arriba a la izquierda; y faltaba dónde ver las misiones aceptadas y la jornada del día.

Código: `Assets/Scripts/UI/HudModals.cs` (la pila, las teclas, Esc y la ventana de misiones), `HorarioPanel.cs` y
`SanctuaryResourceHUD.cs` (sus ventanas). Se instala solo con `[RuntimeInitializeOnLoadMethod]`: **no hace falta reconstruir ninguna
escena**. Prueba: `HudModalsTest` (grupo 93).

## La pila

Abajo a la derecha, con el ancho del botón de F10 y de abajo arriba:

| Hueco | Botón | Tecla | Consola | Ventana |
|---|---|---|---|---|
| (fijo) | F10: pausa · Inicio | F10 | — | Pausa (`SceneLauncher`) |
| 0 | F2: Horario libre | F2 | `horario` | La semana de ocio y cuidado ([horario-libre.md](horario-libre.md)) |
| 1 | F3: Recursos | F3 | `recursos` | Los recursos del santuario y las margas de Kushal |
| 2 | F4: Misiones y jornada | F4 | `misiones` | Las misiones aceptadas y la jornada de hoy |

- Si una escena no tiene el sistema de un botón, ese botón no sale y los de encima bajan un hueco.
- Las pruebas en marcha y la pausa ocultan la pila, como ocultan el botón de F10.
- F2, F3 y F4 estaban libres: en el código solo se usaban F1 (`HudGate`) y F10 (`SceneLauncher`).

## Las ventanas

- **De una en una**: `HudModals.Current` es la única fuente; `HorarioPanel.Show` la lee y la escribe. Abrir una cierra la otra.
- **Se cierran** con Esc, con su botón de la pila, con su tecla o con «cerrar». Esc no cierra la ventana si lo necesita otro: el
  lanzador abierto (`GameKeys` lo enmudece) o un diálogo abierto (`GameKeys.DialogOpen`).
- **Siempre al día**, porque no guardan copia de nada:
  - Recursos lee `SanctuaryResources` y `CharacterLevel` en cada repintado.
  - La jornada sale de `WorkdayDirector.Itinerary`, la misma lista del recuadro de arriba a la derecha, que sigue donde estaba; en la
    ventana lleva además ✓ o ☐ de `PlayerWorkday.Done`, como el panel de Progreso.
  - Las misiones aceptadas se releen cada segundo: la historia en curso (`StoryMission`), la tarea en marcha (`PlayerTaskRunner.Active`)
    y las aceptadas del libro de misiones (`MissionTracker.Accepted`).
- El panel de recursos ya no empuja al de estado de Kushal (`AnimaStatusHUD`), que sale en su sitio, a 180 px.
