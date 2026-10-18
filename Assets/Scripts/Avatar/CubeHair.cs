using UnityEngine;

/// <summary>
/// ASPECTO de cada persona (cabello, piel, ojos, barba) para el cuerpo de cubos: el CABELLO se arma con cubos sobre la
/// cabeza (corto, largo, moño, coleta, rizado, rapado) y hay gorro de cocinero para los cocineros. La CARA (ojos,
/// párpados, cejas, nariz, mandíbula) la pone <see cref="CubeFace"/>; aquí se eligen sus colores por personaje.
/// </summary>
public static class CubeHair
{
    public enum Style { Short, Long, Bun, Ponytail, Curly, Shaved, ChefHat }

    public struct Look
    {
        public Style style; public Color hair, skin, eyes; public bool beard;
        public float height;                 // estatura en metros (0 = la que ya tenía)
        public Color clothes; public bool hasClothes;
        public Look(Style s, Color h, Color sk, Color e, bool b = false) { style = s; hair = h; skin = sk; eyes = e; beard = b; height = 0f; clothes = default; hasClothes = false; }
        public Look Wear(Color c, float m = 0f) { clothes = c; hasClothes = true; height = m; return this; }
    }

    static Color C(float r, float g, float b) => new Color(r, g, b);
    static readonly Color SkinLight = C(0.93f, 0.8f, 0.7f), SkinMid = C(0.84f, 0.66f, 0.52f), SkinTan = C(0.72f, 0.54f, 0.4f), SkinDark = C(0.5f, 0.36f, 0.26f);

    /// <summary>El aspecto de una persona por su nombre (los del santuario, ver docs/diseno-personajes.md); el resto,
    /// uno variado por su nombre.</summary>
    public static Look For(string name)
    {
        string n = (name ?? "").ToLowerInvariant();
        // Kushal: nombre de origen nepalí/indio (inferido; la ficha no da nacionalidad). 1,75 m (DEVLOG).
        if (n.Contains("player") || n.Contains("kushal")) return new Look(Style.Short, C(0.08f, 0.07f, 0.07f), C(0.66f, 0.48f, 0.34f), C(0.2f, 0.13f, 0.08f)).Wear(C(0.22f, 0.45f, 0.5f));
        // La Magnate: sin ficha de aspecto; maga de células infinitas → canas plateadas, túnica violeta de maestra.
        if (n.Contains("magnate")) return new Look(Style.Long, C(0.9f, 0.9f, 0.94f), SkinLight, C(0.45f, 0.6f, 0.85f)).Wear(C(0.42f, 0.3f, 0.62f), 1.7f);
        // Linduarda: ecuatoriana, médica, "a la moda, deportista, gimnasio a diario" → coleta alta, bata blanca, atlética.
        if (n.Contains("linduarda")) return new Look(Style.Ponytail, C(0.12f, 0.08f, 0.06f), C(0.76f, 0.57f, 0.42f), C(0.22f, 0.14f, 0.09f)).Wear(C(0.95f, 0.96f, 0.97f), 1.6f);
        if (n.Contains("maestra")) return new Look(Style.Bun, C(0.7f, 0.7f, 0.72f), SkinLight, C(0.3f, 0.25f, 0.2f));
        // Goluis: 25 años, el de la fuerza (corpulento), jefe de cocina → cabeza rapada, barba, chaqueta de cocinero.
        if (n.Contains("goluis")) return new Look(Style.Shaved, C(0.1f, 0.08f, 0.07f), C(0.62f, 0.45f, 0.32f), C(0.18f, 0.12f, 0.08f), true).Wear(C(0.92f, 0.92f, 0.9f), 1.82f);
        // Panterilia: 42 años, la de la percepción (delgada), alma con un 5 % de león → melena leonada, ojos ámbar.
        if (n.Contains("panterilia")) return new Look(Style.Long, C(0.66f, 0.46f, 0.22f), SkinMid, C(0.78f, 0.58f, 0.18f)).Wear(C(0.45f, 0.5f, 0.32f), 1.68f);
        // Gohageneis: 28 años, "el Papi", nómada desde Venezuela (Colombia, Perú, Ecuador, España, EE. UU.), ágil.
        if (n.Contains("gohageneis")) return new Look(Style.Curly, C(0.07f, 0.06f, 0.06f), C(0.7f, 0.5f, 0.36f), C(0.2f, 0.13f, 0.08f), true).Wear(C(0.55f, 0.16f, 0.18f), 1.76f);
        // Irosene: 70+ años, ecuatoriana, madre de 9; hoy corre, escala y bucea; destaca en el textil → canas cortas, tejido andino.
        if (n.Contains("irosene")) return new Look(Style.Short, C(0.82f, 0.82f, 0.84f), C(0.74f, 0.55f, 0.4f), C(0.22f, 0.15f, 0.1f)).Wear(C(0.85f, 0.32f, 0.3f), 1.52f);
        if (n.Contains("cocinero")) return new Look(Style.ChefHat, C(0.25f, 0.18f, 0.12f), Pick(n, SkinLight, SkinMid, SkinTan, SkinDark), C(0.25f, 0.18f, 0.12f)).Wear(C(0.95f, 0.95f, 0.93f));
        Style[] st = { Style.Short, Style.Long, Style.Bun, Style.Ponytail, Style.Curly };
        Color[] hc = { C(0.15f, 0.1f, 0.08f), C(0.4f, 0.26f, 0.15f), C(0.85f, 0.7f, 0.4f), C(0.6f, 0.2f, 0.1f), C(0.55f, 0.55f, 0.58f) };
        int hsh = Mathf.Abs(n.GetHashCode());
        return new Look(st[hsh % st.Length], hc[(hsh / 7) % hc.Length], Pick(n, SkinLight, SkinMid, SkinTan, SkinDark), C(0.25f, 0.18f, 0.12f));
    }

    static Color Pick(string n, params Color[] c) => c[Mathf.Abs(n.GetHashCode() / 3) % c.Length];

    /// <summary>Pone el pelo sobre la articulación de la cabeza (su cubo: <paramref name="size"/> centrado en <paramref name="center"/>).</summary>
    public static void Build(Transform head, Vector3 center, Vector3 size, Look look)
    {
        Transform h = new GameObject("Cabello").transform;
        h.SetParent(head, false);
        float top = center.y + size.y * 0.5f, back = center.z - size.z * 0.5f, w = size.x + 0.02f, d = size.z + 0.02f;
        Color c = look.hair;
        switch (look.style)
        {
            case Style.Shaved:
                P(h, new Vector3(0f, top + 0.004f, center.z - 0.01f), new Vector3(w - 0.02f, 0.01f, d - 0.04f), c);
                break;
            case Style.ChefHat:
                P(h, new Vector3(0f, top + 0.03f, center.z), new Vector3(w, 0.06f, d), Color.white);
                P(h, new Vector3(0f, top + 0.16f, center.z), new Vector3(w * 0.9f, 0.2f, d * 0.9f), Color.white);
                P(h, new Vector3(0f, top + 0.27f, center.z), new Vector3(w * 1.05f, 0.06f, d * 1.05f), Color.white);
                break;
            default:
                // Casquete + nuca (base de todos los peinados) y flequillo corto sobre la frente.
                P(h, new Vector3(0f, top + 0.025f, center.z - 0.005f), new Vector3(w, 0.05f, d), c);
                P(h, new Vector3(0f, center.y + size.y * 0.15f, back - 0.012f), new Vector3(w, size.y * 0.75f, 0.03f), c);
                for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * (size.x * 0.5f + 0.012f), center.y + size.y * 0.25f, center.z - size.z * 0.15f), new Vector3(0.03f, size.y * 0.5f, size.z * 0.6f), c);
                P(h, new Vector3(0f, top - 0.02f, center.z + size.z * 0.5f - 0.005f), new Vector3(w * 0.85f, 0.05f, 0.025f), c);
                if (look.style == Style.Long)
                {
                    P(h, new Vector3(0f, center.y - size.y * 0.55f, back - 0.02f), new Vector3(w * 1.05f, size.y * 1.1f, 0.04f), c);
                    for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * (size.x * 0.5f + 0.015f), center.y - size.y * 0.3f, center.z - size.z * 0.1f), new Vector3(0.035f, size.y * 0.9f, size.z * 0.5f), c);
                }
                else if (look.style == Style.Bun)
                    P(h, new Vector3(0f, top - 0.01f, back - 0.05f), new Vector3(0.12f, 0.1f, 0.1f), c);
                else if (look.style == Style.Ponytail)
                {
                    P(h, new Vector3(0f, center.y + size.y * 0.3f, back - 0.04f), new Vector3(0.05f, 0.05f, 0.06f), c * 0.8f + new Color(0, 0, 0, 1));
                    P(h, new Vector3(0f, center.y - size.y * 0.15f, back - 0.06f), new Vector3(0.06f, size.y * 0.8f, 0.05f), c);
                }
                else if (look.style == Style.Curly)
                    for (int i = 0; i < 10; i++)
                    {
                        float a = i * 36f * Mathf.Deg2Rad;
                        P(h, new Vector3(Mathf.Cos(a) * size.x * 0.42f, top + 0.03f + (i % 2) * 0.02f, center.z + Mathf.Sin(a) * size.z * 0.42f), Vector3.one * 0.075f, c);
                    }
                break;
        }
        if (look.beard)
        {
            P(h, new Vector3(0f, center.y - size.y * 0.42f, center.z + size.z * 0.42f), new Vector3(size.x * 0.75f, size.y * 0.25f, 0.06f), c);
            for (int s = -1; s <= 1; s += 2) P(h, new Vector3(s * size.x * 0.42f, center.y - size.y * 0.2f, center.z + size.z * 0.3f), new Vector3(0.03f, size.y * 0.35f, 0.08f), c);
        }
    }

    static void P(Transform parent, Vector3 pos, Vector3 size, Color c)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Pelo";
        Collider col = g.GetComponent<Collider>();
        if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = CubeCreature.Mat(c);
    }
}
