using UnityEngine;

/// <summary>
/// ÁREAS DE ENTRENAMIENTO (docs/jornada-y-rotacion.md): las 2 h diarias de entrenamiento son virtualizaciones de estas
/// áreas. Al empezar el juego solo están abiertos el YOGA y el GIMNASIO; la sala de BALLET, el DOJO de artes marciales y
/// el parque de CALISTENIA están EN OBRAS: cada uno tiene a pie de obra el material de un día de trabajo, andamio y
/// paredes a medio levantar (<see cref="ConstructionProject"/>), y el equipo de Construcción los va terminando.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color TRubber = new Color(0.18f, 0.18f, 0.2f), TIron = new Color(0.35f, 0.36f, 0.4f), TChrome = new Color(0.78f, 0.8f, 0.84f),
        TRed = new Color(0.75f, 0.2f, 0.2f), TWood = new Color(0.7f, 0.52f, 0.34f);

    /// <summary>GIMNASIO abierto: suelo de caucho, 10 tipos de máquina (una por grupo muscular) × 3 de cada = 30 máquinas
    /// (docs/gimnasio-maquinas.md), 15 abajo y 15 en la entreplanta, cada una con su <see cref="GymStation"/>, y una ZONA
    /// LIBRE (esterillas, kettlebells, mancuernas, barra de dominadas y sacos).</summary>
    static void FurnishGym(Transform area, Vector2 size)
    {
        Transform r = new GameObject("Gimnasio").transform; r.SetParent(area, false); r.localPosition = new Vector3(0f, 0.1f, 0f);
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        KitchenProps.C(r, new Vector3(0f, 0.01f, 0f), new Vector3(size.x - 0.4f, 0.02f, size.y - 0.4f), TRubber, 0f, "Suelo_caucho");
        // DOS PISOS DE MÁQUINAS (Berón 2026-10-22): 10 tipos × 3 = 30 máquinas, 15 abajo y 15 en una ENTREPLANTA de 4 m
        // sobre la mitad izquierda, con escalera inclinada. Así cada máquina tiene su pasillo (2,6 × 3,2 m de paso, más
        // que el mínimo de 0,6–0,9 m entre máquinas de las guías de gimnasios) y nadie tiene que pasar por encima.
        // (Las estatuas de gorila se quitaron: eran adorno y quitaban sitio.)
        string[] types = { "Press de pecho", "Press de hombros", "Jalón al pecho (espalda)", "Remo sentado", "Prensa de piernas",
                           "Extensión de cuádriceps", "Curl femoral", "Hip thrust (glúteos)", "Gemelos", "Curl de bíceps y tríceps" };
        Vector3 deckC = new Vector3(-4.6f, 0f, -3f);   // de z = −8 a +2: la escalera (4 m de alto, más larga) cabe antes del borde
        Transform upper = Deck(r, "Entreplanta_maquinas", deckC, new Vector2(12.4f, 10f));
        InclinedStair(r, "Escalera_entreplanta", new Vector3(-3f, 0f, deckC.z + 5f + StairRun()), 180f);   // a un lado de la escalera de mano (x = 0)
        int k = 0;
        for (int t = 0; t < types.Length; t++)
            for (int c = 0; c < 3; c++, k++)
            {
                int slot = k % 15, col = slot % 5, row = slot / 5;
                Vector3 p = new Vector3(-10f + col * 2.6f, 0f, -6f + row * 3.2f);
                float side = col == 0 ? 1f : -1f;   // la primera columna está junto al borde: se llega por su derecha
                if (k < 15) GymMachine(r, $"{types[t]} {c + 1}", t, c, p, side);
                else GymMachine(upper, $"{types[t]} {c + 1}", t, c, p - deckC, side);   // arriba, mismas posiciones sobre la entreplanta
            }
        upper.gameObject.AddComponent<WorldLabel>().text = "Máquinas (piso de arriba)";

        // ZONA LIBRE: esterillas, kettlebells, mancuernas, barra de dominadas, sacos.
        float fx = hx - 4f;
        for (int i = 0; i < 4; i++) KitchenProps.C(r, new Vector3(fx - 1.5f + (i % 2) * 2.2f, 0.03f, -hz + 3f + (i / 2) * 3.2f), new Vector3(1f, 0.02f, 2f), new Color(0.3f, 0.45f, 0.4f), 0f, "Esterilla");
        Transform kb = KitchenProps.Root(r, "Kettlebells", new Vector3(fx + 2.4f, 0f, -hz + 1.2f));
        for (int i = 0; i < 6; i++) KitchenProps.C(kb, new Vector3(-1f + i * 0.4f, 0.12f + i * 0.01f, 0f), new Vector3(0.2f + i * 0.02f, 0.22f + i * 0.02f, 0.2f + i * 0.02f), Color.black, 0f, "Kettlebell");
        KitchenProps.Fit(kb); kb.gameObject.AddComponent<WorldLabel>().text = "Kettlebells";
        Transform dbs = KitchenProps.Root(r, "Mancuernas", new Vector3(fx, 0f, -hz + 0.9f));
        KitchenProps.C(dbs, new Vector3(0f, 0.4f, 0f), new Vector3(3f, 0.06f, 0.5f), TIron, 0f, "Estante");
        for (int i = 0; i < 10; i++) KitchenProps.C(dbs, new Vector3(-1.35f + i * 0.3f, 0.5f, 0f), new Vector3(0.08f + i * 0.008f, 0.12f + i * 0.008f, 0.3f), Color.black, 0f, "Mancuerna");
        KitchenProps.Fit(dbs); dbs.gameObject.AddComponent<WorldLabel>().text = "Mancuernas";
        Transform pull = KitchenProps.Root(r, "Barra_dominadas", new Vector3(fx, 0f, hz - 3f));
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(pull, new Vector3(s * 0.8f, 1.2f, 0f), new Vector3(0.08f, 2.4f, 0.08f), TIron, 0f, "Poste");
        KitchenProps.C(pull, new Vector3(0f, 2.3f, 0f), new Vector3(1.7f, 0.05f, 0.05f), TChrome, 0f, "Barra");
        KitchenProps.Fit(pull); pull.gameObject.AddComponent<WorldLabel>().text = "Barra de dominadas";
        for (int i = 0; i < 2; i++)
        {
            Transform bag = KitchenProps.Root(r, "Saco", new Vector3(fx + 2.6f, 0f, hz - 4.5f + i * 2f));
            KitchenProps.C(bag, new Vector3(0f, 1.1f, 0f), new Vector3(0.4f, 1.1f, 0.4f), TRed, 0f, "Saco_boxeo");
            KitchenProps.C(bag, new Vector3(0f, 2.2f, 0f), new Vector3(0.02f, 0.5f, 0.02f), TIron, 0f, "Cadena");
            KitchenProps.Fit(bag);
        }
        WorldLabel zl = KitchenProps.Root(r, "Zona_libre", new Vector3(fx, 0f, 0f)).gameObject.AddComponent<WorldLabel>(); zl.text = "Zona libre"; zl.height = 2.6f;
        area.gameObject.AddComponent<WorldLabel>().text = "Gimnasio (abierto)";
    }


    /// <summary>Una máquina de gimnasio COMO LAS DE VERDAD (docs/gimnasio-maquinas.md, «Qué cambiar» de cada una): cada tipo
    /// tiene su bastidor, su asiento o banco, sus palancas, rodillos y almohadillas, y su pila de placas en una torre con
    /// carcasa (el cable va DENTRO) o discos si es de carga libre. Nada cruza el sitio de la cabeza de quien la usa. Lleva un
    /// hijo vacío «Puesto» (dónde va la raíz del usuario y hacia dónde mira: −Z) y su <see cref="GymStation"/>.
    /// Origen en el centro de la base, a ras de suelo; X a la derecha, Y arriba, −Z es el frente.</summary>
    static void GymMachine(Transform parent, string name, int type, int copy, Vector3 pos, float approachSide = -1f)
    {
        Transform m = KitchenProps.Root(parent, "Maquina_" + name, pos);
        Color F = TIron, pad = new Color(0.12f, 0.12f, 0.14f), acc = type % 2 == 0 ? new Color(0.85f, 0.65f, 0.15f) : new Color(0.75f, 0.2f, 0.2f);
        Color disc = new Color(0.08f, 0.08f, 0.09f);
        Vector3 seat; float root; Vector3 shiftB = Vector3.zero;
        switch (type)
        {
            case 0:   // PRESS DE PECHO: respaldo reclinado 10°, palancas que pivotan arriba y detrás, torre baja (1,5 m)
                GFrame(m, 0.9f, -0.45f, 0.72f);
                GSeat(m, 0.05f, 0.48f, 0.45f, 0.42f, pad);
                GBack(m, new Vector3(0f, 0.55f, 0.24f), 0.75f, 10f, 0.42f, pad);
                GStack(m, new Vector3(0f, 0f, 0.55f), 1.5f, 0.40f, 0.25f, 1f);
                GBar(m, new Vector3(-0.38f, 1.45f, 0.47f), new Vector3(0.38f, 1.45f, 0.47f), 0.07f, F, "Eje_palancas");
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 piv = new Vector3(s * 0.35f, 1.45f, 0.30f), grip = new Vector3(s * 0.28f, 1.00f, -0.25f);
                    GBar(m, new Vector3(s * 0.35f, 1.45f, 0.47f), piv, 0.06f, F, "Soporte_pivote");
                    KitchenProps.C(m, piv, new Vector3(0.09f, 0.09f, 0.09f), acc, 0f, "Pivote");
                    GBar(m, piv, grip, 0.06f, F, "Palanca");
                    KitchenProps.C(m, grip, new Vector3(0.04f, 0.18f, 0.04f), TChrome, 0f, "Agarre_neutro");
                    KitchenProps.C(m, grip + new Vector3(-s * 0.08f, 0.09f, 0f), new Vector3(0.16f, 0.035f, 0.035f), TChrome, 0f, "Agarre_horizontal");
                }
                GBar(m, new Vector3(0f, 0.04f, -0.45f), new Vector3(0f, 0.09f, -0.30f), 0.05f, F, "Soporte_reposapies");
                KitchenProps.C(m, new Vector3(0f, 0.10f, -0.30f), new Vector3(0.5f, 0.05f, 0.1f), F, 0f, "Reposapies");
                seat = new Vector3(0f, 0f, 0.05f); root = 0.60f; break;

            case 1:   // PRESS DE HOMBROS: respaldo alto casi vertical (8°), palancas desde arriba y detrás
                GFrame(m, 0.9f, -0.40f, 0.78f);
                GSeat(m, 0.05f, 0.48f, 0.45f, 0.42f, pad);
                GBack(m, new Vector3(0f, 0.55f, 0.25f), 0.95f, 8f, 0.42f, pad);
                GStack(m, new Vector3(0f, 0f, 0.62f), 1.55f, 0.40f, 0.25f, 1f);
                GBar(m, new Vector3(-0.34f, 1.50f, 0.55f), new Vector3(0.34f, 1.50f, 0.55f), 0.07f, F, "Eje_palancas");
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 piv = new Vector3(s * 0.30f, 1.55f, 0.35f), grip = new Vector3(s * 0.25f, 1.18f, -0.10f);
                    GBar(m, new Vector3(s * 0.30f, 1.50f, 0.55f), piv, 0.06f, F, "Soporte_pivote");
                    KitchenProps.C(m, piv, new Vector3(0.09f, 0.09f, 0.09f), acc, 0f, "Pivote");
                    GBar(m, piv, grip, 0.06f, F, "Palanca");
                    KitchenProps.C(m, grip, new Vector3(0.04f, 0.18f, 0.04f), TChrome, 0f, "Agarre_neutro");
                }
                GBar(m, new Vector3(0f, 0.04f, -0.40f), new Vector3(0f, 0.09f, -0.28f), 0.05f, F, "Soporte_reposapies");
                KitchenProps.C(m, new Vector3(0f, 0.10f, -0.28f), new Vector3(0.5f, 0.05f, 0.1f), F, 0f, "Reposapies");
                seat = new Vector3(0f, 0f, 0.05f); root = 0.60f; break;

            case 2:   // JALÓN AL PECHO: torre DELANTE, polea alta, cable visible, barra de 1,2 m, rodillos de muslo, sin respaldo
                GFrame(m, 0.6f, -0.75f, 0.32f);
                GStack(m, new Vector3(0f, 0f, -0.55f), 2.2f, 0.45f, 0.30f, -1f);
                GBar(m, new Vector3(0f, 2.17f, -0.55f), new Vector3(0f, 2.17f, -0.09f), 0.08f, F, "Viga_superior");
                KitchenProps.C(m, new Vector3(0f, 2.10f, -0.12f), new Vector3(0.05f, 0.10f, 0.10f), TChrome, 0f, "Polea");
                GBar(m, new Vector3(0f, 2.05f, -0.10f), new Vector3(0f, 1.82f, -0.08f), 0.012f, F, "Cable");   // delante de la cara y por encima de la cabeza
                KitchenProps.C(m, new Vector3(0f, 1.80f, -0.08f), new Vector3(0.8f, 0.035f, 0.035f), TChrome, 0f, "Barra_jalon");
                for (int s = -1; s <= 1; s += 2)
                    GBar(m, new Vector3(s * 0.4f, 1.80f, -0.08f), new Vector3(s * (0.4f + 0.2f * Mathf.Cos(20f * Mathf.Deg2Rad)), 1.80f - 0.2f * Mathf.Sin(20f * Mathf.Deg2Rad), -0.08f), 0.035f, acc, "Punta_barra");
                GSeat(m, 0.05f, 0.48f, 0.40f, 0.35f, pad);
                GBar(m, new Vector3(0f, 0.04f, -0.32f), new Vector3(0f, 0.66f, -0.18f), 0.06f, F, "Soporte_rodillos");
                for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, new Vector3(s * 0.17f, 0.68f, -0.18f), new Vector3(0.26f, 0.10f, 0.10f), pad, 0f, "Rodillo_muslo");
                seat = new Vector3(0f, 0f, 0.05f); root = 0.60f; break;

            case 3:   // REMO SENTADO: torre delante, almohadilla de pecho, brazos de remo con agarres neutros, reposapiés
                GFrame(m, 0.6f, -0.80f, 0.36f);
                GStack(m, new Vector3(0f, 0f, -0.65f), 1.8f, 0.45f, 0.30f, -1f);
                GBar(m, new Vector3(0f, 1.15f, -0.50f), new Vector3(0f, 1.15f, -0.19f), 0.06f, F, "Soporte_almohadilla");
                KitchenProps.C(m, new Vector3(0f, 1.15f, -0.15f), new Vector3(0.30f, 0.40f, 0.08f), pad, 0f, "Almohadilla_pecho");
                GSeat(m, 0.10f, 0.48f, 0.40f, 0.35f, pad);
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.15f, 0.04f, -0.52f), new Vector3(s * 0.15f, 0.16f, -0.44f), 0.04f, F, "Soporte_reposapies");
                    KitchenProps.C(m, new Vector3(s * 0.15f, 0.22f, -0.42f), new Vector3(0.16f, 0.20f, 0.03f), F, new Vector3(-25f, 0f, 0f), "Reposapies");
                    GBar(m, new Vector3(s * 0.20f, 1.60f, -0.48f), new Vector3(s * 0.20f, 1.05f, -0.55f), 0.05f, F, "Brazo_remo");
                    KitchenProps.C(m, new Vector3(s * 0.20f, 1.05f, -0.55f), new Vector3(0.04f, 0.16f, 0.04f), TChrome, 0f, "Agarre_neutro");
                }
                KitchenProps.C(m, new Vector3(0f, 1.60f, -0.48f), new Vector3(0.48f, 0.07f, 0.07f), acc, 0f, "Pivote");
                seat = new Vector3(0f, 0f, 0.10f); root = 0.60f; break;

            case 4:   // PRENSA 45°: de discos (sin torre), raíles a 45°, carro con plataforma perpendicular, respaldo a 50°
            {
                GFrame(m, 1.1f, -1.22f, 0.90f, -1.15f, -0.15f);
                Vector3 d = new Vector3(0f, 1f, -1f).normalized, n = new Vector3(0f, 1f, 1f).normalized;
                Quaternion rot = Quaternion.LookRotation(d, n);
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.40f, 0.35f, -0.15f), new Vector3(s * 0.40f, 1.35f, -1.15f), 0.08f, TChrome, "Rail_45");
                    GBar(m, new Vector3(s * 0.40f, 0.04f, -1.15f), new Vector3(s * 0.40f, 1.35f, -1.15f), 0.08f, F, "Poste_rail");
                    GBar(m, new Vector3(s * 0.40f, 0.04f, -0.15f), new Vector3(s * 0.40f, 0.35f, -0.15f), 0.08f, F, "Poste_rail");
                }
                Vector3 c = new Vector3(0f, 0.75f, -0.55f);   // centro de la plataforma, abajo (rodilla a 90°)
                Vector3 sled = c + d * 0.25f + n * 0.06f;
                KitchenProps.C(m, sled, new Vector3(0.9f, 0.06f, 0.5f), F, rot, "Carro");
                KitchenProps.C(m, c, new Vector3(0.75f, 0.55f, 0.05f), acc, rot, "Plataforma_pies");
                for (int s = -1; s <= 1; s += 2)
                {
                    KitchenProps.C(m, sled + new Vector3(s * 0.60f, 0f, 0f), new Vector3(0.30f, 0.05f, 0.05f), TChrome, rot, "Cuerno");
                    for (int i = 0; i < 2; i++) KitchenProps.C(m, sled + new Vector3(s * (0.55f + i * 0.08f), 0f, 0f), new Vector3(0.05f, 0.45f, 0.45f), disc, rot, "Disco");
                    GBar(m, new Vector3(s * 0.32f, 0.04f, 0.25f), new Vector3(s * 0.32f, 0.43f, 0.25f), 0.04f, F, "Poste_asa");
                    KitchenProps.C(m, new Vector3(s * 0.32f, 0.45f, 0.25f), new Vector3(0.04f, 0.04f, 0.18f), TChrome, 0f, "Asa");
                    KitchenProps.C(m, new Vector3(s * 0.34f, 0.55f, 0.12f), new Vector3(0.03f, 0.20f, 0.03f), acc, new Vector3(-30f, 0f, 0f), "Tope_seguridad");
                }
                GBar(m, new Vector3(0f, 0.04f, 0.35f), new Vector3(0f, 0.32f, 0.35f), 0.08f, F, "Columna_asiento");
                KitchenProps.C(m, new Vector3(0f, 0.36f, 0.35f), new Vector3(0.45f, 0.08f, 0.45f), pad, new Vector3(15f, 0f, 0f), "Asiento");
                GBack(m, new Vector3(0f, 0.45f, 0.30f), 0.85f, 50f, 0.45f, pad);
                seat = new Vector3(0f, 0.42f, 0.35f); root = 0.52f; break;
            }

            case 5:   // EXTENSIÓN DE CUÁDRICEPS: pivote alineado con la rodilla, rodillo sobre el tobillo, pila a un lado
                GFrame(m, 0.6f, -0.40f, 0.55f);
                GSeat(m, 0.10f, 0.50f, 0.45f, 0.42f, pad);
                GBack(m, new Vector3(0f, 0.55f, 0.33f), 0.75f, 12f, 0.42f, pad);
                GBar(m, new Vector3(0.32f, 0.04f, -0.15f), new Vector3(0.32f, 0.53f, -0.24f), 0.05f, F, "Soporte_pivote");
                KitchenProps.C(m, new Vector3(0.32f, 0.56f, -0.25f), new Vector3(0.08f, 0.08f, 0.08f), acc, 0f, "Pivote_rodilla");
                GBar(m, new Vector3(0.32f, 0.56f, -0.25f), new Vector3(0.32f, 0.20f, -0.32f), 0.05f, F, "Palanca");
                KitchenProps.C(m, new Vector3(0.26f, 0.20f, -0.32f), new Vector3(0.12f, 0.03f, 0.03f), TChrome, 0f, "Eje_rodillo");
                KitchenProps.C(m, new Vector3(0f, 0.20f, -0.32f), new Vector3(0.40f, 0.12f, 0.12f), pad, 0f, "Rodillo_espinilla");
                for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, new Vector3(s * 0.28f, 0.50f, 0.05f), new Vector3(0.04f, 0.04f, 0.20f), TChrome, 0f, "Asa");
                GStack(m, new Vector3(0.55f, 0f, 0.15f), 1.4f, 0.30f, 0.25f, 1f);
                seat = new Vector3(0f, 0f, 0.15f); root = 0.62f; break;

            case 6:   // CURL FEMORAL TUMBADO: banco en tres tramos con joroba en la cadera, rodillo tras los tobillos
                GFrame(m, 0.6f, -0.85f, 0.60f);
                KitchenProps.C(m, new Vector3(0f, 0.58f, -0.475f), new Vector3(0.32f, 0.08f, 0.75f), pad, 0f, "Banco_pecho");
                KitchenProps.C(m, new Vector3(0f, 0.63f, 0.075f), new Vector3(0.32f, 0.10f, 0.35f), pad, 0f, "Banco_joroba");
                KitchenProps.C(m, new Vector3(0f, 0.62f, 0.375f), new Vector3(0.32f, 0.08f, 0.25f), pad, 0f, "Banco_muslos");
                GBar(m, new Vector3(0f, 0.04f, -0.60f), new Vector3(0f, 0.54f, -0.60f), 0.07f, F, "Pata");
                GBar(m, new Vector3(0f, 0.04f, 0.30f), new Vector3(0f, 0.58f, 0.30f), 0.07f, F, "Pata");
                GBar(m, new Vector3(0.30f, 0.04f, 0.58f), new Vector3(0.30f, 0.67f, 0.58f), 0.05f, F, "Soporte_pivote");
                KitchenProps.C(m, new Vector3(0.30f, 0.70f, 0.58f), new Vector3(0.08f, 0.08f, 0.08f), acc, 0f, "Pivote_rodilla");
                GBar(m, new Vector3(0.30f, 0.70f, 0.58f), new Vector3(0.30f, 0.74f, 0.98f), 0.05f, F, "Palanca");
                KitchenProps.C(m, new Vector3(0.26f, 0.74f, 0.98f), new Vector3(0.12f, 0.03f, 0.03f), TChrome, 0f, "Eje_rodillo");
                KitchenProps.C(m, new Vector3(0f, 0.74f, 0.98f), new Vector3(0.40f, 0.12f, 0.12f), pad, 0f, "Rodillo_tobillo");
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.22f, 0.04f, -0.80f), new Vector3(s * 0.22f, 0.43f, -0.80f), 0.04f, F, "Poste_asidero");
                    KitchenProps.C(m, new Vector3(s * 0.22f, 0.45f, -0.80f), new Vector3(0.04f, 0.04f, 0.16f), TChrome, 0f, "Asidero");
                }
                GStack(m, new Vector3(-0.55f, 0f, 0f), 1.5f, 0.30f, 0.25f, 1f);
                seat = new Vector3(0f, 0.62f, 0.10f); root = 0.81f; break;

            case 7:   // HIP THRUST: almohadilla de espalda baja, asiento bajo, plataforma de pies, palanca con almohadilla de cadera y discos
                GFrame(m, 1.0f, -0.80f, 0.50f);
                for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * 0.40f, 0.04f, 0.40f), new Vector3(s * 0.40f, 0.23f, 0.40f), 0.06f, F, "Pata_almohadilla");
                KitchenProps.C(m, new Vector3(0f, 0.33f, 0.40f), new Vector3(1.0f, 0.20f, 0.20f), pad, 0f, "Almohadilla_espalda");
                GBar(m, new Vector3(0f, 0.04f, 0f), new Vector3(0f, 0.10f, 0f), 0.08f, F, "Soporte_asiento");
                KitchenProps.C(m, new Vector3(0f, 0.14f, 0f), new Vector3(0.45f, 0.08f, 0.35f), pad, 0f, "Asiento_bajo");
                KitchenProps.C(m, new Vector3(0f, 0.07f, -0.55f), new Vector3(0.9f, 0.05f, 0.45f), F, new Vector3(10f, 0f, 0f), "Plataforma_pies");
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.55f, 0.04f, 0.40f), new Vector3(s * 0.55f, 0.40f, 0.40f), 0.08f, F, "Poste_pivote");
                    GBar(m, new Vector3(s * 0.55f, 0.40f, 0.40f), new Vector3(s * 0.30f, 0.40f, -0.10f), 0.06f, F, "Brazo_palanca");
                    KitchenProps.C(m, new Vector3(s * 0.62f, 0.40f, 0.40f), new Vector3(0.14f, 0.05f, 0.05f), TChrome, 0f, "Cuerno");
                    for (int i = 0; i < 2; i++) KitchenProps.C(m, new Vector3(s * (0.61f + i * 0.05f), 0.40f, 0.40f), new Vector3(0.04f, 0.45f, 0.45f), disc, 0f, "Disco");
                }
                KitchenProps.C(m, new Vector3(0f, 0.40f, -0.10f), new Vector3(0.6f, 0.15f, 0.15f), pad, 0f, "Almohadilla_cadera");
                seat = new Vector3(0f, 0.05f, -0.10f); root = 0.25f; shiftB = new Vector3(0f, 0f, 0.10f); break;   // en B la pelvis avanza 0,10 m (hacia −Z de la máquina)

            case 8:   // GEMELOS SENTADO: asiento corto sin respaldo, almohadilla sobre las rodillas, calzo para las puntas
                GFrame(m, 0.5f, -0.70f, 0.28f);
                GSeat(m, 0.05f, 0.55f, 0.35f, 0.35f, pad);
                KitchenProps.C(m, new Vector3(0f, 0.06f, -0.47f), new Vector3(0.50f, 0.12f, 0.15f), F, 0f, "Calzo_puntas");
                GBar(m, new Vector3(0f, 0.04f, -0.65f), new Vector3(0f, 0.22f, -0.65f), 0.08f, F, "Soporte_pivote");
                KitchenProps.C(m, new Vector3(0f, 0.25f, -0.65f), new Vector3(0.55f, 0.07f, 0.07f), acc, 0f, "Pivote");
                for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * 0.25f, 0.25f, -0.65f), new Vector3(s * 0.25f, 0.72f, -0.32f), 0.05f, F, "Palanca");
                KitchenProps.C(m, new Vector3(0f, 0.72f, -0.32f), new Vector3(0.42f, 0.08f, 0.15f), pad, 0f, "Almohadilla_rodillas");
                KitchenProps.C(m, new Vector3(0.33f, 0.72f, -0.32f), new Vector3(0.16f, 0.05f, 0.05f), TChrome, 0f, "Cuerno");
                KitchenProps.C(m, new Vector3(0.37f, 0.72f, -0.32f), new Vector3(0.04f, 0.45f, 0.45f), disc, 0f, "Disco");
                GBar(m, new Vector3(-0.25f, 0.72f, -0.32f), new Vector3(-0.30f, 0.86f, -0.28f), 0.03f, acc, "Maneta_seguridad");
                seat = new Vector3(0f, 0f, 0.05f); root = 0.67f; break;

            default:   // BÍCEPS Y TRÍCEPS: apoyo de brazos inclinado 40°, levas alineadas con el codo, pila detrás a la derecha
            {
                GFrame(m, 0.6f, -0.65f, 0.55f);
                GSeat(m, 0.10f, 0.52f, 0.42f, 0.40f, pad);
                Vector3 lo = new Vector3(0f, 0.85f, -0.35f), hi = new Vector3(0f, 1.05f, -0.10f);
                GBar(m, new Vector3(0f, 0.04f, -0.32f), new Vector3(0f, 0.90f, -0.25f), 0.07f, F, "Columna_apoyo");
                KitchenProps.C(m, (lo + hi) * 0.5f, new Vector3(0.50f, 0.06f, 0.35f), pad, Quaternion.LookRotation((hi - lo).normalized, Vector3.up), "Apoyo_brazos");
                for (int s = -1; s <= 1; s += 2)
                {
                    GBar(m, new Vector3(s * 0.30f, 0.04f, -0.45f), new Vector3(s * 0.30f, 0.82f, -0.39f), 0.05f, F, "Soporte_leva");
                    KitchenProps.C(m, new Vector3(s * 0.30f, 0.88f, -0.38f), new Vector3(0.04f, 0.16f, 0.16f), acc, new Vector3(45f, 0f, 0f), "Leva");
                    GBar(m, new Vector3(s * 0.30f, 0.88f, -0.38f), new Vector3(s * 0.27f, 0.62f, -0.55f), 0.045f, F, "Palanca");
                }
                KitchenProps.C(m, new Vector3(0f, 0.62f, -0.55f), new Vector3(0.56f, 0.04f, 0.04f), TChrome, 0f, "Barra_agarre");
                GStack(m, new Vector3(0.45f, 0f, 0.40f), 1.4f, 0.30f, 0.25f, 1f);
                seat = new Vector3(0f, 0f, 0.10f); root = 0.64f; break;
            }
        }
        KitchenProps.Fit(m);
        Transform pu = new GameObject("Puesto").transform; pu.SetParent(m, false);
        pu.localPosition = seat; pu.localRotation = Quaternion.Euler(0f, 180f, 0f);   // mira hacia −Z (el frente de la máquina)
        GymStation st = m.gameObject.AddComponent<GymStation>();
        st.type = (GymMachineType)type; st.copy = copy; st.puesto = pu; st.rootHeight = root; st.shiftB = shiftB; st.approachSide = approachSide;
        WorldLabel wl = m.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = 2.4f;
    }

    /// <summary>Barra (cubo alargado) de <paramref name="a"/> a <paramref name="b"/>, de sección <paramref name="t"/>.</summary>
    static Transform GBar(Transform m, Vector3 a, Vector3 b, float t, Color c, string name)
    {
        Vector3 d = b - a; float len = d.magnitude;
        Vector3 dir = len > 1e-4f ? d / len : Vector3.forward;
        Quaternion q = Quaternion.LookRotation(dir, Mathf.Abs(dir.y) > 0.99f ? Vector3.forward : Vector3.up);
        return KitchenProps.C(m, (a + b) * 0.5f, new Vector3(t, t, Mathf.Max(len, t)), c, q, name);
    }

    /// <summary>Bastidor en el suelo: dos largueros de ancho <paramref name="w"/> entre z0 y z1, con travesaños en los extremos
    /// (y en <paramref name="extra"/>).</summary>
    static void GFrame(Transform m, float w, float z0, float z1, params float[] extra)
    {
        for (int s = -1; s <= 1; s += 2) GBar(m, new Vector3(s * w * 0.5f, 0.04f, z0), new Vector3(s * w * 0.5f, 0.04f, z1), 0.08f, TIron, "Larguero");
        GBar(m, new Vector3(-w * 0.5f, 0.04f, z0), new Vector3(w * 0.5f, 0.04f, z0), 0.08f, TIron, "Travesano");
        GBar(m, new Vector3(-w * 0.5f, 0.04f, z1), new Vector3(w * 0.5f, 0.04f, z1), 0.08f, TIron, "Travesano");
        foreach (float z in extra) GBar(m, new Vector3(-w * 0.5f, 0.04f, z), new Vector3(w * 0.5f, 0.04f, z), 0.08f, TIron, "Travesano");
    }

    /// <summary>Asiento con la parte de arriba a <paramref name="top"/> m, sobre su columna.</summary>
    static void GSeat(Transform m, float z, float top, float w, float d, Color pad)
    {
        GBar(m, new Vector3(0f, 0.04f, z), new Vector3(0f, top - 0.08f, z), 0.07f, TIron, "Columna_asiento");
        KitchenProps.C(m, new Vector3(0f, top - 0.04f, z), new Vector3(w, 0.08f, d), pad, 0f, "Asiento");
    }

    /// <summary>Respaldo de <paramref name="len"/> m desde su borde de abajo, reclinado <paramref name="recline"/>° (la parte de
    /// arriba hacia +Z, lejos del frente), con su soporte desde el bastidor.</summary>
    static void GBack(Transform m, Vector3 bottom, float len, float recline, float w, Color pad)
    {
        float r = recline * Mathf.Deg2Rad;
        Vector3 up = new Vector3(0f, Mathf.Cos(r), Mathf.Sin(r));
        Vector3 c = bottom + up * (len * 0.5f);
        KitchenProps.C(m, c, new Vector3(w, len, 0.08f), pad, new Vector3(recline, 0f, 0f), "Respaldo");
        Vector3 back = new Vector3(0f, Mathf.Sin(r), -Mathf.Cos(r)) * -0.06f;   // detrás del cojín
        GBar(m, new Vector3(0f, 0.04f, bottom.z + 0.05f), c + back, 0.06f, TIron, "Soporte_respaldo");
    }

    /// <summary>Torre de placas con CARCASA (guías, tapa y panel) de <paramref name="h"/> m: el cable va dentro. El panel queda
    /// del lado <paramref name="panelSide"/> en Z (+1 detrás, −1 delante), lejos de quien entrena.</summary>
    static void GStack(Transform m, Vector3 at, float h, float w, float d, float panelSide)
    {
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(m, at + new Vector3(s * (w * 0.5f - 0.02f), h * 0.5f, 0f), new Vector3(0.04f, h, d), TIron, 0f, "Carcasa");
        KitchenProps.C(m, at + new Vector3(0f, h - 0.03f, 0f), new Vector3(w, 0.06f, d), TIron, 0f, "Tapa");
        KitchenProps.C(m, at + new Vector3(0f, h * 0.5f, panelSide * (d * 0.5f - 0.01f)), new Vector3(w, h, 0.02f), TIron, 0f, "Panel");
        int plates = Mathf.Clamp(Mathf.FloorToInt((h - 0.6f) / 0.06f), 6, 13);
        for (int i = 0; i < plates; i++) KitchenProps.C(m, at + new Vector3(0f, 0.08f + i * 0.06f, 0f), new Vector3(w - 0.1f, 0.05f, d * 0.6f), Color.black, 0f, "Placa");
        float topPlate = 0.08f + plates * 0.06f;
        KitchenProps.C(m, at + new Vector3(0f, (topPlate + h - 0.06f) * 0.5f, 0f), new Vector3(0.012f, h - 0.06f - topPlate, 0.012f), new Color(0.2f, 0.2f, 0.22f), 0f, "Cable");
    }
    /// <summary>OBRA: paredes a medio levantar, andamio y el material de UN día de trabajo a pie de obra.</summary>
    static void MakeConstructionSite(Transform area, Vector2 size, string title, int days)
    {
        float hx = size.x * 0.5f, hz = size.y * 0.5f;
        Transform scaf = KitchenProps.Root(area, "Andamio_obra", new Vector3(-hx + 1.4f, 0.1f, -hz + 1f));
        for (int i = 0; i < 4; i++) KitchenProps.C(scaf, new Vector3(i % 2 == 0 ? -0.9f : 0.9f, 1.5f, i < 2 ? -0.4f : 0.4f), new Vector3(0.05f, 3f, 0.05f), TChrome, 0f, "Tubo");
        for (int y = 0; y < 2; y++) KitchenProps.C(scaf, new Vector3(0f, 1.4f + y * 1.4f, 0f), new Vector3(1.9f, 0.05f, 0.85f), TWood, 0f, "Plataforma");
        Transform mat = KitchenProps.Root(area, "Material_del_dia", new Vector3(hx - 2f, 0.1f, 1f));
        KitchenProps.C(mat, new Vector3(-0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), TWood, 0f, "Pale");
        for (int y = 0; y < 4; y++) for (int i = 0; i < 4; i++)
            KitchenProps.C(mat, new Vector3(-1.08f + i * 0.25f, 0.18f + y * 0.1f, 0f), new Vector3(0.22f, 0.09f, 1f), new Color(0.62f, 0.5f, 0.38f), 0f, "Barro_con_paja");
        KitchenProps.C(mat, new Vector3(0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), TWood, 0f, "Pale");
        for (int y = 0; y < 3; y++) KitchenProps.C(mat, new Vector3(0.7f, 0.2f + y * 0.15f, 0f), new Vector3(0.95f, 0.14f, 0.45f), new Color(0.93f, 0.9f, 0.82f), 0f, "Panel_micelio");
        for (int i = 0; i < 5; i++) KitchenProps.C(mat, new Vector3(0f, 0.08f + i * 0.1f, 1.1f), new Vector3(2.8f, 0.09f, 0.09f), new Color(0.78f, 0.74f, 0.42f), 0f, "Guadua");
        WorldLabel ml = mat.gameObject.AddComponent<WorldLabel>(); ml.text = "Material de un día de obra: 4 guaduas, barro con paja, 2 paneles de micelio, cuerda"; ml.height = 1f;
        for (int i = 0; i < 3; i++) KitchenProps.C(area, new Vector3(-1f + i * 0.8f, 0.3f, hz - 0.6f), new Vector3(0.25f, 0.4f, 0.25f), new Color(0.95f, 0.45f, 0.15f), 45f, "Cono");
        ConstructionProject cp = area.gameObject.AddComponent<ConstructionProject>();
        cp.title = title; cp.daysRequired = days;
        WorldLabel wl = area.gameObject.AddComponent<WorldLabel>(); wl.text = $"{title} — EN OBRAS (0/{days} días)"; wl.height = 4.5f;
    }
}
