using UnityEngine;

/// <summary>
/// ESTATUAS DE LA SALA DE YOGA (de cubos), al fondo, tras la tarima del maestro:
///  · PATANJALI (autor de los Yoga Sutras): torso humano con las manos juntas (añjali mudra) sobre una cola de serpiente
///    enroscada, y detrás de la cabeza el capuchón de cinco cabezas de cobra (Adishesha, de quien se le considera
///    encarnación). Mármol blanco.
///  · BUDA: sentado en loto sobre un pedestal de pétalos, manos en el regazo (dhyana mudra), ushnisha (el moño-protuberancia
///    de la coronilla), rizos, lóbulos largos, urna en la frente y el manto sobre el hombro izquierdo. Bronce dorado.
///  · AUM (ॐ): la sílaba sagrada en oro sobre una estela de piedra oscura, en el centro.
///  · SHIVA en meditación: piel azulada, moño de rastas (jata) con la luna creciente, tercer ojo, cobra al cuello, collar
///    de rudraksha, sentado sobre piel de tigre, y a su lado el tridente (trishula) con el tambor damaru.
/// Y los de los PISOS DE YOGA (2026-11-04; docs/yoga-pisos.md; el anillo de yoga, con sus estatuas en los puntos cardinales, se
/// quitó): el LINGA como el Dhyanalinga, el busto de ADIYOGI, GANESHA y las LÁMPARAS DE ACEITE.
/// </summary>
public static class YogaStatues
{
    public static void Build(Transform room, float backZ)
    {
        Transform root = new GameObject("Estatuas").transform; root.SetParent(room, false);
        Patanjali(root, new Vector3(-6.2f, 0f, backZ + 0.7f));
        Buddha(root, new Vector3(-3.4f, 0f, backZ + 0.7f));
        Aum(root, new Vector3(0f, 0f, backZ + 0.25f));
        Shiva(root, new Vector3(3.4f, 0f, backZ + 0.7f));
    }

    /// <summary>Las cuatro estatuas en las ESQUINAS de la sala, mirando a la plataforma central.</summary>
    public static void BuildCorners(Transform room, float hx, float hz)
    {
        Transform root = new GameObject("Estatuas").transform; root.SetParent(room, false);
        Patanjali(root, new Vector3(-hx + 1.3f, 0f, -hz + 1.3f));
        Buddha(root, new Vector3(hx - 1.3f, 0f, -hz + 1.3f));
        Aum(root, new Vector3(hx - 1.3f, 0f, hz - 1.6f));
        Shiva(root, new Vector3(-hx + 1.3f, 0f, hz - 1.6f));
        foreach (Transform s in root) { Vector3 d = -s.localPosition; d.y = 0f; if (d.sqrMagnitude > 0.01f) s.localRotation = Quaternion.LookRotation(d); }
    }


    static Transform Root(Transform parent, string name, Vector3 pos, string label, float labelH)
    {
        Transform t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = pos;
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = labelH;
        return t;
    }

    static Transform C(Transform p, Vector3 pos, Vector3 size, Color c, float yaw = 0f, float roll = 0f, float pitch = 0f)
    {
        Transform t = KitchenProps.C(p, pos, size, c, yaw, "Piedra");
        t.localRotation = Quaternion.Euler(pitch, yaw, roll);
        return t;
    }

    // Cuerpo sentado en loto (piernas cruzadas, torso, cabeza) a partir de y0 (altura del asiento). Devuelve la altura de la cabeza.
    static float SeatedBody(Transform t, float y0, Color skin, Color cloth, bool handsInLap)
    {
        C(t, new Vector3(0f, y0 + 0.12f, 0.05f), new Vector3(0.95f, 0.24f, 0.55f), cloth);                       // piernas cruzadas
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.36f, y0 + 0.2f, 0.22f), new Vector3(0.2f, 0.14f, 0.2f), cloth); // rodillas
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.12f, y0 + 0.24f, 0.3f), new Vector3(0.14f, 0.06f, 0.2f), skin, s * 30f); // pies sobre los muslos
        C(t, new Vector3(0f, y0 + 0.52f, 0f), new Vector3(0.42f, 0.5f, 0.27f), cloth);                            // torso
        C(t, new Vector3(0f, y0 + 0.8f, 0f), new Vector3(0.1f, 0.08f, 0.1f), skin);                               // cuello
        C(t, new Vector3(0f, y0 + 0.96f, 0.01f), new Vector3(0.24f, 0.26f, 0.24f), skin);                         // cabeza
        for (int s = -1; s <= 1; s += 2)
        {
            C(t, new Vector3(s * 0.26f, y0 + 0.58f, 0.02f), new Vector3(0.1f, 0.3f, 0.1f), skin, 0f, s * 8f);      // brazo
            if (handsInLap)
                C(t, new Vector3(s * 0.12f, y0 + 0.36f, 0.18f), new Vector3(0.26f, 0.08f, 0.1f), skin, s * 25f);   // antebrazo al regazo
        }
        if (handsInLap) C(t, new Vector3(0f, y0 + 0.36f, 0.24f), new Vector3(0.16f, 0.04f, 0.12f), skin);         // manos en dhyana mudra
        return y0 + 0.96f;
    }

    public static void Patanjali(Transform parent, Vector3 pos)
    {
        Color marble = new Color(0.93f, 0.92f, 0.88f), hood = new Color(0.55f, 0.62f, 0.58f), scale = new Color(0.62f, 0.68f, 0.62f);
        Transform t = Root(parent, "Estatua_Patanjali", pos, "Patanjali", 2.6f);
        C(t, new Vector3(0f, 0.15f, 0f), new Vector3(1.2f, 0.3f, 1f), new Color(0.6f, 0.58f, 0.55f));            // pedestal
        // Cola de serpiente: tres anillos enroscados que se estrechan, y la punta.
        for (int k = 0; k < 3; k++)
        {
            float rr = 0.48f - k * 0.1f, y = 0.38f + k * 0.16f;
            for (int a = 0; a < 8; a++)
            {
                float ang = a * 45f * Mathf.Deg2Rad;
                C(t, new Vector3(Mathf.Cos(ang) * rr, y, Mathf.Sin(ang) * rr), new Vector3(0.22f - k * 0.03f, 0.15f, 0.22f - k * 0.03f), k % 2 == 0 ? scale : marble, -a * 45f);
            }
        }
        C(t, new Vector3(0.45f, 0.36f, 0.35f), new Vector3(0.18f, 0.08f, 0.1f), scale, 40f);                     // punta de la cola
        // Torso humano con las manos juntas en el pecho (añjali mudra).
        C(t, new Vector3(0f, 1.05f, 0f), new Vector3(0.4f, 0.5f, 0.26f), marble);
        C(t, new Vector3(0f, 1.34f, 0f), new Vector3(0.1f, 0.08f, 0.1f), marble);
        C(t, new Vector3(0f, 1.5f, 0.01f), new Vector3(0.24f, 0.26f, 0.24f), marble);
        C(t, new Vector3(0f, 1.68f, -0.02f), new Vector3(0.16f, 0.1f, 0.16f), marble);                           // moño
        for (int s = -1; s <= 1; s += 2)
        {
            C(t, new Vector3(s * 0.25f, 1.08f, 0.04f), new Vector3(0.1f, 0.28f, 0.1f), marble, 0f, s * 10f);
            C(t, new Vector3(s * 0.12f, 1.0f, 0.16f), new Vector3(0.22f, 0.08f, 0.09f), marble, s * 35f);
        }
        C(t, new Vector3(0f, 1.1f, 0.23f), new Vector3(0.07f, 0.18f, 0.05f), marble);                            // palmas juntas
        // Capuchón de cobra de cinco cabezas (Adishesha) tras la cabeza.
        C(t, new Vector3(0f, 1.45f, -0.22f), new Vector3(0.7f, 0.6f, 0.06f), hood);
        for (int h = 0; h < 5; h++)
        {
            float ang = (-60f + h * 30f) * Mathf.Deg2Rad;
            Vector3 hp = new Vector3(Mathf.Sin(ang) * 0.42f, 1.55f + Mathf.Cos(ang) * 0.38f, -0.16f);
            C(t, hp, new Vector3(0.14f, 0.12f, 0.16f), hood);
            C(t, hp + new Vector3(0f, 0f, 0.09f), new Vector3(0.08f, 0.06f, 0.06f), hood);                         // hocico
            for (int s = -1; s <= 1; s += 2) C(t, hp + new Vector3(s * 0.035f, 0.03f, 0.085f), new Vector3(0.02f, 0.02f, 0.01f), new Color(0.85f, 0.7f, 0.2f));
        }
    }

    public static void Buddha(Transform parent, Vector3 pos)
    {
        Color bronze = new Color(0.78f, 0.6f, 0.28f), robe = new Color(0.7f, 0.52f, 0.22f), petal = new Color(0.82f, 0.68f, 0.4f);
        Transform t = Root(parent, "Estatua_Buda", pos, "Buda", 2.1f);
        C(t, new Vector3(0f, 0.12f, 0f), new Vector3(1.15f, 0.24f, 0.95f), new Color(0.55f, 0.5f, 0.45f));
        for (int a = 0; a < 12; a++)   // pedestal de loto: dos coronas de pétalos
        {
            float ang = a * 30f * Mathf.Deg2Rad;
            C(t, new Vector3(Mathf.Cos(ang) * 0.5f, 0.3f, Mathf.Sin(ang) * 0.42f), new Vector3(0.22f, 0.12f, 0.1f), petal, -a * 30f + 90f, 0f, 20f);
        }
        C(t, new Vector3(0f, 0.32f, 0f), new Vector3(0.9f, 0.08f, 0.7f), petal);
        float head = SeatedBody(t, 0.36f, bronze, robe, true);
        C(t, new Vector3(0.06f, 0.88f, 0.1f), new Vector3(0.36f, 0.06f, 0.29f), robe, 0f, 35f);                  // manto sobre el hombro izquierdo
        C(t, new Vector3(0f, head + 0.17f, 0f), new Vector3(0.12f, 0.1f, 0.12f), bronze);                       // ushnisha
        for (int i = 0; i < 9; i++) C(t, new Vector3(((i % 3) - 1) * 0.07f, head + 0.135f, ((i / 3) - 1) * 0.07f + 0.01f), new Vector3(0.05f, 0.03f, 0.05f), robe); // rizos
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.13f, head - 0.07f, 0f), new Vector3(0.03f, 0.14f, 0.05f), bronze); // lóbulos largos
        C(t, new Vector3(0f, head + 0.04f, 0.125f), new Vector3(0.02f, 0.02f, 0.01f), new Color(0.95f, 0.85f, 0.5f)); // urna
        C(t, new Vector3(0f, head + 0.0f, -0.25f), new Vector3(0.62f, 0.62f, 0.04f), new Color(0.85f, 0.72f, 0.38f)); // aureola
    }

    static void Aum(Transform parent, Vector3 pos)
    {
        Color stone = new Color(0.25f, 0.24f, 0.28f), gold = new Color(0.95f, 0.75f, 0.25f);
        Transform t = Root(parent, "Estela_Aum", pos, "Aum (ॐ)", 2.7f);
        C(t, new Vector3(0f, 0.15f, 0f), new Vector3(1.5f, 0.3f, 0.4f), stone * 0.8f + new Color(0, 0, 0, 1));
        C(t, new Vector3(0f, 1.25f, 0f), new Vector3(1.3f, 1.9f, 0.15f), stone);
        // ॐ en píxeles de oro (16 × 13): el "3" grande, la cola que sale del centro, la luna creciente y el punto (chandrabindu).
        string[] om =
        {
            "...........#....",
            "........#....#..",
            ".........####...",
            "................",
            ".####...........",
            "#....#..........",
            ".....#..........",
            "..###.##..###...",
            ".....#..##...#..",
            "......#.......#.",
            "......#.......#.",
            "#....#.......#..",
            ".####.....###...",
        };
        float px = 0.07f;
        for (int row = 0; row < om.Length; row++)
            for (int col = 0; col < om[row].Length; col++)
                if (om[row][col] == '#')
                    C(t, new Vector3((col - 7.5f) * px, 1.85f - row * px, 0.085f), new Vector3(px, px, 0.03f), gold);
    }

    static void Shiva(Transform parent, Vector3 pos)
    {
        Color skin = new Color(0.45f, 0.58f, 0.75f), cloth = new Color(0.55f, 0.42f, 0.3f), tiger = new Color(0.9f, 0.55f, 0.15f);
        Color hair = new Color(0.25f, 0.2f, 0.18f), snake = new Color(0.25f, 0.45f, 0.3f), metal = new Color(0.75f, 0.75f, 0.78f);
        Transform t = Root(parent, "Estatua_Shiva", pos, "Shiva", 2.5f);
        C(t, new Vector3(0f, 0.12f, 0f), new Vector3(1.15f, 0.24f, 0.95f), new Color(0.5f, 0.48f, 0.46f));
        C(t, new Vector3(0f, 0.27f, 0.05f), new Vector3(1.0f, 0.06f, 0.75f), tiger);                              // piel de tigre
        for (int i = 0; i < 4; i++) C(t, new Vector3(-0.35f + i * 0.23f, 0.305f, 0.05f), new Vector3(0.04f, 0.01f, 0.6f), new Color(0.15f, 0.1f, 0.08f), 15f);
        float head = SeatedBody(t, 0.3f, skin, cloth, false);
        for (int s = -1; s <= 1; s += 2)   // manos en chin mudra sobre las rodillas
        {
            C(t, new Vector3(s * 0.33f, 0.62f, 0.16f), new Vector3(0.09f, 0.08f, 0.26f), skin, 0f, 0f, 30f);
            C(t, new Vector3(s * 0.36f, 0.52f, 0.28f), new Vector3(0.07f, 0.05f, 0.08f), skin);
        }
        C(t, new Vector3(0f, head + 0.2f, -0.02f), new Vector3(0.16f, 0.22f, 0.16f), hair);                    // jata (moño de rastas)
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.14f, head - 0.02f, -0.04f), new Vector3(0.05f, 0.3f, 0.12f), hair); // rastas
        C(t, new Vector3(0.07f, head + 0.24f, 0.07f), new Vector3(0.1f, 0.03f, 0.02f), new Color(0.95f, 0.95f, 0.9f), 0f, -30f); // luna creciente
        C(t, new Vector3(0f, head + 0.07f, 0.125f), new Vector3(0.015f, 0.045f, 0.01f), new Color(0.75f, 0.1f, 0.1f)); // tercer ojo
        C(t, new Vector3(0f, head - 0.2f, 0.02f), new Vector3(0.3f, 0.05f, 0.3f), snake);                       // cobra al cuello
        C(t, new Vector3(0.1f, head - 0.12f, 0.15f), new Vector3(0.06f, 0.08f, 0.06f), snake);                 // su cabeza
        for (int b = 0; b < 7; b++) C(t, new Vector3(-0.12f + b * 0.04f, 0.7f - Mathf.Abs(b - 3) * 0.02f, 0.14f), new Vector3(0.03f, 0.03f, 0.03f), new Color(0.45f, 0.25f, 0.15f)); // rudraksha
        // Trishula (tridente) clavado a su lado, con el damaru.
        Transform tr = new GameObject("Trishula").transform; tr.SetParent(t, false); tr.localPosition = new Vector3(0.7f, 0f, 0f);
        C(tr, new Vector3(0f, 1.05f, 0f), new Vector3(0.04f, 2.1f, 0.04f), new Color(0.45f, 0.32f, 0.2f));
        C(tr, new Vector3(0f, 2.12f, 0f), new Vector3(0.32f, 0.04f, 0.04f), metal);
        for (int s = -1; s <= 1; s++) C(tr, new Vector3(s * 0.15f, 2.3f + (s == 0 ? 0.06f : 0f), 0f), new Vector3(0.04f, s == 0 ? 0.36f : 0.28f, 0.03f), metal);
        C(tr, new Vector3(0f, 1.8f, 0f), new Vector3(0.12f, 0.1f, 0.12f), new Color(0.6f, 0.45f, 0.3f), 45f);   // damaru
    }

    // ── LOS PISOS DE YOGA (Berón 2026-11-04; docs/yoga-pisos.md): los elementos distintivos de cada sala ──

    /// <summary>LINGA como el DHYANALINGA de Isha (Coimbatore): una piedra negra pulida, alta y redondeada (allí, granito negro de
    /// 13 pies y 9 pulgadas) que sale del AVUDAIYAR, el receptáculo en forma de serpiente de SIETE vueltas (de cobre), sobre un zócalo de
    /// piedra. Aquí a escala de sala (≈ 2 m). Sin rasgos humanos: es un espacio para meditar, no un retrato.</summary>
    public static Transform Linga(Transform parent, Vector3 pos)
    {
        Color granite = new Color(0.08f, 0.08f, 0.09f), copper = new Color(0.72f, 0.45f, 0.22f), stone = new Color(0.55f, 0.5f, 0.44f);
        Transform t = Root(parent, "Linga_Dhyanalinga", pos, "Linga (como el Dhyanalinga de Isha)", 2.6f);
        C(t, new Vector3(0f, 0.1f, 0f), new Vector3(1.6f, 0.2f, 1.6f), stone);                                   // zócalo
        for (int k = 0; k < 7; k++)   // el avudaiyar: siete vueltas de la serpiente, cada una algo más estrecha
        {
            float w = 1.3f - k * 0.08f;
            C(t, new Vector3(0f, 0.24f + k * 0.07f, 0f), new Vector3(w, 0.06f, w), k % 2 == 0 ? copper : new Color(0.61f, 0.38f, 0.19f), k * 8f);
        }
        C(t, new Vector3(0.62f, 0.5f, 0f), new Vector3(0.3f, 0.12f, 0.2f), copper, 0f);                            // la cabeza de la serpiente
        // La piedra: más ancha abajo, redondeada arriba (cubos apilados).
        float[] widths = { 0.62f, 0.66f, 0.66f, 0.63f, 0.58f, 0.5f, 0.38f, 0.22f };
        for (int i = 0; i < widths.Length; i++) C(t, new Vector3(0f, 0.8f + i * 0.15f, 0f), new Vector3(widths[i], 0.15f, widths[i]), granite, i * 11f);
        return t;
    }

    /// <summary>BUSTO DE ADIYOGI (el primer yogui, Shiva; el de Isha mide 34 m y es de acero): cabeza y hombros oscuros, el moño de
    /// rastas (jata) con la luna creciente, el tercer ojo, la cobra al cuello y el collar de rudraksha. Aquí ≈ 2,1 m.</summary>
    public static Transform AdiyogiBust(Transform parent, Vector3 pos)
    {
        Color steel = new Color(0.12f, 0.12f, 0.13f), snake = new Color(0.2f, 0.35f, 0.25f), bead = new Color(0.45f, 0.25f, 0.15f);
        Transform t = Root(parent, "Busto_Adiyogi", pos, "Adiyogi (busto, como el de Isha)", 2.6f);
        C(t, new Vector3(0f, 0.15f, 0f), new Vector3(1.5f, 0.3f, 0.9f), new Color(0.5f, 0.47f, 0.44f));         // pedestal
        C(t, new Vector3(0f, 0.65f, 0f), new Vector3(1.4f, 0.7f, 0.6f), steel);                                  // hombros y pecho
        C(t, new Vector3(0f, 1.08f, 0f), new Vector3(0.28f, 0.18f, 0.28f), steel);                               // cuello
        C(t, new Vector3(0f, 1.42f, 0.02f), new Vector3(0.5f, 0.55f, 0.48f), steel);                             // cabeza
        C(t, new Vector3(0f, 1.86f, -0.04f), new Vector3(0.36f, 0.36f, 0.34f), steel);                           // jata
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.3f, 1.2f, -0.06f), new Vector3(0.12f, 0.6f, 0.24f), steel);   // rastas sobre los hombros
        C(t, new Vector3(0.14f, 1.98f, 0.12f), new Vector3(0.18f, 0.05f, 0.03f), new Color(0.95f, 0.95f, 0.9f), 0f, -30f);  // luna creciente
        C(t, new Vector3(0f, 1.52f, 0.26f), new Vector3(0.03f, 0.09f, 0.02f), new Color(0.75f, 0.1f, 0.1f));    // tercer ojo
        C(t, new Vector3(0f, 1.0f, 0.05f), new Vector3(0.6f, 0.08f, 0.5f), snake);                               // cobra al cuello
        C(t, new Vector3(0.22f, 1.12f, 0.3f), new Vector3(0.1f, 0.12f, 0.1f), snake);                            // su cabeza
        for (int b = 0; b < 9; b++) C(t, new Vector3(-0.32f + b * 0.08f, 0.88f - Mathf.Abs(b - 4) * 0.03f, 0.31f), new Vector3(0.05f, 0.05f, 0.04f), bead);   // rudraksha
        return t;
    }

    /// <summary>GANESHA (el que quita los obstáculos; frecuente en los estudios occidentales): sentado, cabeza de elefante con la
    /// trompa curvada, orejas grandes, un colmillo roto, la barriga, y su ratón (mushika) a los pies. Latón.</summary>
    public static Transform Ganesha(Transform parent, Vector3 pos)
    {
        Color brass = new Color(0.8f, 0.62f, 0.3f), cloth = new Color(0.78f, 0.35f, 0.18f), ivory = new Color(0.95f, 0.92f, 0.85f);
        Transform t = Root(parent, "Estatua_Ganesha", pos, "Ganesha", 2.2f);
        C(t, new Vector3(0f, 0.12f, 0f), new Vector3(1.15f, 0.24f, 0.95f), new Color(0.55f, 0.5f, 0.45f));
        float head = SeatedBody(t, 0.3f, brass, cloth, true);
        C(t, new Vector3(0f, 0.62f, 0.1f), new Vector3(0.5f, 0.36f, 0.3f), brass);                                // barriga
        C(t, new Vector3(0f, head + 0.04f, 0.02f), new Vector3(0.34f, 0.32f, 0.3f), brass);                       // cabeza de elefante
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.27f, head + 0.02f, -0.02f), new Vector3(0.22f, 0.32f, 0.04f), brass, s * 20f);   // orejas
        C(t, new Vector3(0f, head - 0.14f, 0.2f), new Vector3(0.1f, 0.18f, 0.1f), brass);                         // trompa, arriba
        C(t, new Vector3(0.04f, head - 0.3f, 0.24f), new Vector3(0.08f, 0.16f, 0.08f), brass, 0f, 15f);
        C(t, new Vector3(0.1f, head - 0.4f, 0.28f), new Vector3(0.12f, 0.06f, 0.07f), brass, 0f, 50f);           // la punta, curvada hacia el dulce
        C(t, new Vector3(-0.08f, head - 0.12f, 0.17f), new Vector3(0.03f, 0.1f, 0.03f), ivory, 0f, 20f);          // el colmillo entero (el otro, roto)
        C(t, new Vector3(0.08f, head - 0.08f, 0.17f), new Vector3(0.03f, 0.04f, 0.03f), ivory);
        C(t, new Vector3(0f, head + 0.24f, 0f), new Vector3(0.22f, 0.12f, 0.22f), new Color(0.95f, 0.75f, 0.25f)); // corona
        C(t, new Vector3(0.38f, 0.3f, 0.42f), new Vector3(0.16f, 0.1f, 0.24f), new Color(0.45f, 0.42f, 0.4f));    // el ratón
        return t;
    }

    /// <summary>LÁMPARA DE ACEITE (diya): cuenco de latón en su pie, con la mecha encendida. Pequeña: no tapa la vista.</summary>
    public static Transform OilLamp(Transform parent, Vector3 pos)
    {
        Color brass = new Color(0.82f, 0.64f, 0.3f), flame = new Color(1f, 0.62f, 0.15f);
        Transform t = new GameObject("Lampara_aceite").transform; t.SetParent(parent, false); t.localPosition = pos;
        C(t, new Vector3(0f, 0.06f, 0f), new Vector3(0.08f, 0.12f, 0.08f), brass);            // pie
        C(t, new Vector3(0f, 0.14f, 0f), new Vector3(0.2f, 0.05f, 0.2f), brass, 45f);          // cuenco
        C(t, new Vector3(0f, 0.2f, 0f), new Vector3(0.035f, 0.07f, 0.035f), flame);            // llama
        return t;
    }

    /// <summary>Patanjali y Buda, devolviendo la estatua (para hacerla mueble).</summary>
    public static Transform PatanjaliStatue(Transform parent, Vector3 pos) { Patanjali(parent, pos); return parent.GetChild(parent.childCount - 1); }
    public static Transform BuddhaStatue(Transform parent, Vector3 pos) { Buddha(parent, pos); return parent.GetChild(parent.childCount - 1); }
}
