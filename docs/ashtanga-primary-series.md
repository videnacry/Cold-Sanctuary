# Ashtanga — Serie Primaria (Yoga Chikitsa) = "nivel 1"

Ashtanga Vinyasa tiene **4 series** (niveles): **Primaria** (Yoga Chikitsa, terapia), **Intermedia** (Nadi Shodhana,
purificación nerviosa) y **Avanzada A/B** (Sthira Bhaga). Este doc detalla la **Primaria** (nivel 1). Es larga (~75 min),
**vinyasa** (cada movimiento ligado a una respiración; respiración **Ujjayi**), con **tristana** (respiración + postura +
*drishti* mirada) y **bandhas** (mula + uddiyana). Entre asanas sentadas se intercala un **vinyasa** (medio saludo) para
enlazar. Las posturas principales se mantienen **5 respiraciones** ("panca").

Código: `Assets/Scripts/Asana/AshtangaPrimary.cs` implementa el **núcleo runnable** (apertura + posturas sagitales +
cierre) sobre el modelo por respiraciones; las laterales/complejas (triángulo, torsiones, invertidas) se listan aquí y se
añaden con validación visual en Unity. Registrada en `YogaLibrary` y con su escena `Yoga_Ashtanga1`.

## Apertura
- **Surya Namaskara A** ×5
- **Surya Namaskara B** ×5 (añade Utkatasana + Virabhadrasana I)

## Secuencia de pie (standing)
1. Padangusthasana — dedos gordos (pies anchura de cadera, pliegue, agarre de los dedos). *drishti:* nariz.
2. Pada Hastasana — manos bajo los pies.
3. Utthita Trikonasana — triángulo extendido (+ Parivrtta, revertido).
4. Utthita Parsvakonasana — ángulo lateral extendido (+ Parivrtta).
5. Prasarita Padottanasana A·B·C·D — pliegue de piernas abiertas.
6. Parsvottanasana — estiramiento lateral intenso (manos en rezo a la espalda).
7. Utthita Hasta Padangusthasana — mano al dedo gordo de pie, equilibrio.
8. Ardha Baddha Padmottanasana — medio loto de pie + pliegue.
9. Utkatasana — silla.
10. Virabhadrasana A y B — guerrero I y II.

## Secuencia sentada (Yoga Chikitsa)
- Dandasana — bastón (base de las sentadas).
- Paschimottanasana A·B·C — pinza sentada.
- Purvottanasana — plancha hacia arriba.
- Ardha Baddha Padma Paschimottanasana.
- Triang Mukhaikapada Paschimottanasana.
- Janu Sirsasana A·B·C — cabeza a la rodilla.
- Marichyasana A·B·C·D — torsiones.
- Navasana — barca (×5, con elevaciones).
- Bhujapidasana — presión de hombros (brazo-balance).
- Kurmasana / Supta Kurmasana — tortuga.
- Garbha Pindasana + Kukkutasana — embrión + gallo.
- Baddha Konasana — ángulo atado (mariposa).
- Upavistha Konasana — ángulo abierto sentado.
- Supta Konasana · Supta Padangusthasana · Ubhaya Padangusthasana.
- Urdhva Mukha Paschimottanasana · Setu Bandhasana — puente.

## Cierre (finishing)
- Urdhva Dhanurasana — rueda ×3 (+ pinza de compensación).
- Salamba Sarvangasana — vela (sobre hombros).
- Halasana — arado · Karnapidasana — presión de orejas.
- Urdhva Padmasana · Pindasana.
- Matsyasana — pez · Uttana Padasana.
- Sirsasana — parada de cabeza.
- Balasana — descanso.
- Baddha Padmasana · Yoga Mudra · Padmasana — loto · Utpluthih (Tolasana).
- **Savasana** — cadáver (relajación final, varios minutos).

## Núcleo implementado en código (sagital, bajo riesgo)
Surya A (reutiliza `SunSalutation`) → Padangusthasana → Utkatasana → Virabhadrasana I → Prasarita Padottanasana →
Dandasana → Paschimottanasana → Navasana → Baddha Konasana → Setu Bandhasana → Savasana. Mantenimiento 5 respiraciones.
Las laterales/invertidas (triángulo, torsiones, vela, parada de cabeza) quedan documentadas aquí para añadir con la vista
en Unity (requieren flexión lateral de columna y apoyos que conviene validar visualmente).
