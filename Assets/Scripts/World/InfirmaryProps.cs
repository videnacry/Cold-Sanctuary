using UnityEngine;

/// <summary>
/// MOBILIARIO Y MAQUINARIA DE LA ENFERMERÍA (docs/sanctuary1-level-and-live-tests §4: Linduarda analiza y vacuna), de
/// UNIONES DE CUBOS y a tamaño real: camas de hospital con suero y monitor, camilla de exploración, mesa del médico con
/// ordenador, botiquín de cristal, aparato de rayos X, banco de laboratorio (microscopio, centrífuga, analizador de
/// sangre, gradilla de tubos), y la parte VETERINARIA: mesa de exploración hidráulica con lámpara, cuadra para animales
/// grandes, jaulas apiladas, báscula de suelo y ecógrafo. Cada objeto con su nombre y collider (KitchenProps.Fit).
/// </summary>
public static class InfirmaryProps
{
    static Color H(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color c); return c; }
    static readonly Color Steel = KitchenProps.Steel, Dark = KitchenProps.DarkSteel, White = KitchenProps.White, Black = KitchenProps.Black,
        Mint = H("BFE5DA"), Blue = H("9CC3E6"), Screen = H("1F3A44"), Glow = H("7CF5B0"), Wood = KitchenProps.Wood, Glass = KitchenProps.Glass, Red = H("E07070");

    static Transform C(Transform t, Vector3 p, Vector3 s, Color c, float yaw = 0f, string n = "cubo") => KitchenProps.C(t, p, s, c, yaw, n);

    public static Transform HospitalBed(Transform parent, string name, Vector3 pos, float yaw, Color blanket)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        for (int i = 0; i < 4; i++)   // patas con ruedas
        {
            Vector3 p = new Vector3(i % 2 == 0 ? -0.45f : 0.45f, 0f, i < 2 ? -0.95f : 0.95f);
            C(t, p + Vector3.up * 0.25f, new Vector3(0.05f, 0.45f, 0.05f), Steel, 0f, "Pata");
            C(t, p + Vector3.up * 0.04f, new Vector3(0.06f, 0.08f, 0.1f), Black, 0f, "Rueda");
        }
        C(t, new Vector3(0f, 0.5f, 0f), new Vector3(1f, 0.08f, 2.05f), Steel, 0f, "Somier");
        C(t, new Vector3(0f, 0.62f, 0f), new Vector3(0.95f, 0.16f, 2f), White, 0f, "Colchon");
        C(t, new Vector3(0f, 0.74f, 0.25f), new Vector3(0.96f, 0.06f, 1.4f), blanket, 0f, "Manta");
        C(t, new Vector3(0f, 0.75f, -0.78f), new Vector3(0.6f, 0.1f, 0.32f), White, 0f, "Almohada");
        C(t, new Vector3(0f, 0.85f, -1.02f), new Vector3(1f, 0.75f, 0.06f), Mint, 0f, "Cabecero");
        C(t, new Vector3(0f, 0.7f, 1.02f), new Vector3(1f, 0.45f, 0.06f), Mint, 0f, "Piecero");
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(0.52f * s, 0.82f, -0.3f), new Vector3(0.03f, 0.18f, 0.8f), Steel, 0f, "Baranda");
        // Gotero (suero) y monitor de constantes junto a la cabecera.
        Transform iv = KitchenProps.Root(t, "Gotero", new Vector3(-0.75f, 0f, -0.85f));
        C(iv, new Vector3(0f, 0.9f, 0f), new Vector3(0.03f, 1.8f, 0.03f), Steel, 0f, "Pie");
        C(iv, new Vector3(0f, 0.02f, 0f), new Vector3(0.4f, 0.04f, 0.4f), Steel, 45f, "Base");
        C(iv, new Vector3(0.1f, 1.75f, 0f), new Vector3(0.2f, 0.02f, 0.02f), Steel, 0f, "Gancho");
        C(iv, new Vector3(0.17f, 1.6f, 0f), new Vector3(0.1f, 0.18f, 0.04f), H("DDEFF6"), 0f, "Bolsa");
        Transform mon = KitchenProps.Root(t, "Monitor", new Vector3(0.78f, 0f, -0.9f));
        C(mon, new Vector3(0f, 0.6f, 0f), new Vector3(0.05f, 1.2f, 0.05f), Steel, 0f, "Pie");
        C(mon, new Vector3(0f, 1.3f, 0f), new Vector3(0.4f, 0.3f, 0.08f), Dark, 0f, "Carcasa");
        C(mon, new Vector3(0f, 1.3f, 0.045f), new Vector3(0.34f, 0.22f, 0.005f), Screen, 0f, "Pantalla");
        for (int k = 0; k < 5; k++) C(mon, new Vector3(-0.12f + k * 0.06f, 1.3f + (k % 2 == 0 ? 0.03f : -0.02f), 0.05f), new Vector3(0.06f, 0.008f, 0.004f), Glow, k % 2 == 0 ? 0f : 0f, "Pulso");
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform ExamTable(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.35f, 0f), new Vector3(0.6f, 0.7f, 1.6f), Wood, 0f, "Base");
        C(t, new Vector3(0f, 0.76f, 0.1f), new Vector3(0.65f, 0.12f, 1.5f), Blue, 0f, "Acolchado");
        C(t, new Vector3(0f, 0.92f, -0.72f), new Vector3(0.65f, 0.12f, 0.45f), Blue, 0f, "Respaldo").localRotation = Quaternion.Euler(-30f, 0f, 0f);
        C(t, new Vector3(0f, 0.83f, 0.1f), new Vector3(0.5f, 0.005f, 1.5f), White, 0f, "Papel");
        C(t, new Vector3(0.6f, 0.25f, 0.6f), new Vector3(0.35f, 0.5f, 0.35f), Dark, 0f, "Taburete");
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform DoctorDesk(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.74f, 0f), new Vector3(1.4f, 0.05f, 0.7f), Wood, 0f, "Tablero");
        for (int i = 0; i < 4; i++) C(t, new Vector3(i % 2 == 0 ? -0.65f : 0.65f, 0.37f, i < 2 ? -0.3f : 0.3f), new Vector3(0.05f, 0.74f, 0.05f), Dark, 0f, "Pata");
        C(t, new Vector3(0.1f, 1.0f, 0.15f), new Vector3(0.55f, 0.35f, 0.04f), Dark, 0f, "Monitor");
        C(t, new Vector3(0.1f, 1.0f, 0.127f), new Vector3(0.5f, 0.3f, 0.005f), H("3C6E8F"), 0f, "Pantalla");
        C(t, new Vector3(0.1f, 0.8f, 0.17f), new Vector3(0.05f, 0.1f, 0.05f), Dark, 0f, "Pie");
        C(t, new Vector3(0.1f, 0.775f, -0.1f), new Vector3(0.45f, 0.02f, 0.15f), Black, 0f, "Teclado");
        C(t, new Vector3(-0.45f, 0.78f, -0.05f), new Vector3(0.22f, 0.03f, 0.3f), White, 10f, "Historias");
        C(t, new Vector3(0f, 0.45f, -0.6f), new Vector3(0.45f, 0.08f, 0.45f), Black, 0f, "Silla");
        C(t, new Vector3(0f, 0.75f, -0.82f), new Vector3(0.45f, 0.55f, 0.06f), Black, 0f, "Respaldo");
        C(t, new Vector3(0f, 0.22f, -0.6f), new Vector3(0.05f, 0.4f, 0.05f), Steel, 0f, "Pie_Silla");
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform MedicineCabinet(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.95f, 0f), new Vector3(1.1f, 1.9f, 0.4f), White, 0f, "Mueble");
        C(t, new Vector3(0f, 1.25f, -0.205f), new Vector3(1f, 1.2f, 0.01f), Glass, 0f, "Vitrina");
        C(t, new Vector3(0.48f, 1.25f, -0.22f), new Vector3(0.02f, 0.2f, 0.02f), Steel, 0f, "Tirador");
        C(t, new Vector3(0.3f, 1.75f, -0.22f), new Vector3(0.18f, 0.18f, 0.01f), Red, 0f, "Cruz_H");
        C(t, new Vector3(0.3f, 1.75f, -0.221f), new Vector3(0.06f, 0.18f, 0.01f), White, 0f, "Cruz_V");
        Color[] meds = { H("F2C46D"), H("8FC1E3"), H("E58E8E"), H("A8D8A0"), White };
        for (int lv = 0; lv < 3; lv++)
            for (int k = 0; k < 6; k++)
                C(t, new Vector3(-0.4f + k * 0.16f, 0.8f + lv * 0.36f, -0.05f), new Vector3(0.07f, 0.14f, 0.07f), meds[(k + lv) % meds.Length], 0f, "Frasco");
        C(t, new Vector3(0f, 0.3f, -0.205f), new Vector3(1f, 0.5f, 0.01f), Mint, 0f, "Cajones");
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform XRay(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.05f, 0f), new Vector3(0.8f, 0.1f, 0.8f), Dark, 0f, "Base");
        C(t, new Vector3(0f, 1.2f, 0f), new Vector3(0.2f, 2.3f, 0.2f), White, 0f, "Columna");
        C(t, new Vector3(0f, 1.8f, 0.45f), new Vector3(0.15f, 0.15f, 0.9f), White, 0f, "Brazo");
        C(t, new Vector3(0f, 1.6f, 0.85f), new Vector3(0.35f, 0.35f, 0.35f), Mint, 0f, "Tubo");
        C(t, new Vector3(0f, 1.2f, 2.1f), new Vector3(0.7f, 0.9f, 0.08f), White, 0f, "Detector");
        C(t, new Vector3(0f, 0.55f, 2.1f), new Vector3(0.1f, 0.8f, 0.1f), Steel, 0f, "Soporte");
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform LabBench(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.CounterRun(parent, name, pos, 3.2f, yaw);
        float top = KitchenProps.CounterTop;
        // Microscopio
        C(t, new Vector3(-1.2f, top + 0.02f, 0f), new Vector3(0.18f, 0.04f, 0.24f), Dark, 0f, "Microscopio_Base");
        C(t, new Vector3(-1.2f, top + 0.18f, 0.08f), new Vector3(0.05f, 0.3f, 0.05f), Dark, 0f, "Microscopio_Brazo");
        C(t, new Vector3(-1.2f, top + 0.12f, -0.02f), new Vector3(0.14f, 0.015f, 0.12f), Black, 0f, "Platina");
        C(t, new Vector3(-1.2f, top + 0.3f, 0f), new Vector3(0.05f, 0.14f, 0.05f), White, 0f, "Tubo_Optico").localRotation = Quaternion.Euler(-25f, 0f, 0f);
        C(t, new Vector3(-1.2f, top + 0.38f, -0.05f), new Vector3(0.035f, 0.05f, 0.035f), Black, 0f, "Ocular");
        // Centrífuga
        C(t, new Vector3(-0.5f, top + 0.12f, 0f), new Vector3(0.35f, 0.24f, 0.35f), White, 0f, "Centrifuga");
        C(t, new Vector3(-0.5f, top + 0.25f, 0f), new Vector3(0.3f, 0.03f, 0.3f), Blue, 45f, "Tapa");
        C(t, new Vector3(-0.5f, top + 0.1f, -0.176f), new Vector3(0.12f, 0.05f, 0.005f), Glow, 0f, "Display");
        // Gradilla con tubos de ensayo
        C(t, new Vector3(0.1f, top + 0.04f, 0f), new Vector3(0.3f, 0.08f, 0.1f), White, 0f, "Gradilla");
        for (int k = 0; k < 6; k++) C(t, new Vector3(-0.03f + k * 0.05f - 0.1f, top + 0.12f, 0f), new Vector3(0.025f, 0.14f, 0.025f), k % 2 == 0 ? Red : H("F2D46D"), 0f, "Tubo");
        KitchenProps.Fit(t);
        return t;
    }

    /// <summary>Analizador de sangre: caja con pantalla y bandeja de muestras (la estación de análisis clínico).</summary>
    public static Transform BloodAnalyzer(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.5f, 0f), new Vector3(0.9f, 1f, 0.7f), White, 0f, "Mueble");
        C(t, new Vector3(0f, 1.25f, 0f), new Vector3(0.7f, 0.5f, 0.55f), H("E8EEF2"), 0f, "Analizador");
        C(t, new Vector3(-0.12f, 1.3f, -0.28f), new Vector3(0.35f, 0.25f, 0.01f), Screen, 0f, "Pantalla");
        C(t, new Vector3(-0.12f, 1.3f, -0.286f), new Vector3(0.28f, 0.03f, 0.004f), Glow, 0f, "Grafica");
        C(t, new Vector3(0.22f, 1.08f, -0.25f), new Vector3(0.18f, 0.06f, 0.1f), Dark, 0f, "Bandeja");
        for (int k = 0; k < 3; k++) C(t, new Vector3(0.17f + k * 0.05f, 1.15f, -0.25f), new Vector3(0.02f, 0.08f, 0.02f), Red, 0f, "Muestra");
        KitchenProps.Fit(t);
        return t;
    }

    // ── Veterinaria ─────────────────────────────────────────────────────────────────────────────

    public static Transform VetTable(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.05f, 0f), new Vector3(0.7f, 0.1f, 0.9f), Dark, 0f, "Base");
        C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.22f, 0.7f, 0.22f), Steel, 0f, "Hidraulico");
        C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.8f, 0.06f, 1.5f), Steel, 0f, "Mesa");
        C(t, new Vector3(0f, 0.89f, 0f), new Vector3(0.75f, 0.01f, 1.45f), H("D9DEE3"), 0f, "Superficie");
        Transform lampT = KitchenProps.Root(t, "Lampara", new Vector3(0.7f, 0f, 0.6f));
        C(lampT, new Vector3(0f, 1f, 0f), new Vector3(0.05f, 2f, 0.05f), Steel, 0f, "Pie");
        C(lampT, new Vector3(-0.35f, 2f, -0.35f), new Vector3(0.05f, 0.05f, 1f), Steel, 45f, "Brazo");
        C(lampT, new Vector3(-0.65f, 1.95f, -0.65f), new Vector3(0.35f, 0.08f, 0.35f), H("FFF4C8"), 0f, "Foco");
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform LargeAnimalStall(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        float w = 3.4f, d = 3f;
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(w, 0.06f, d), H("E9D79A"), 0f, "Paja");
        for (int i = 0; i < 6; i++) C(t, new Vector3(-w * 0.4f + i * w * 0.16f, 0.08f, -d * 0.3f + (i % 3) * d * 0.3f), new Vector3(0.4f, 0.05f, 0.25f), H("DCC57E"), i * 30f, "Brizna");
        for (int y = 0; y < 3; y++)
        {
            float hy = 0.35f + y * 0.4f;
            C(t, new Vector3(0f, hy, d * 0.5f), new Vector3(w, 0.08f, 0.08f), Wood, 0f, "Valla");
            C(t, new Vector3(-w * 0.5f, hy, 0f), new Vector3(0.08f, 0.08f, d), Wood, 0f, "Valla");
            C(t, new Vector3(w * 0.5f, hy, 0f), new Vector3(0.08f, 0.08f, d), Wood, 0f, "Valla");
            C(t, new Vector3(-w * 0.3f, hy, -d * 0.5f), new Vector3(w * 0.4f, 0.08f, 0.08f), Wood, 0f, "Valla");
        }
        for (int s = -1; s <= 1; s += 2) for (int f = -1; f <= 1; f += 2) C(t, new Vector3(s * w * 0.5f, 0.7f, f * d * 0.5f), new Vector3(0.12f, 1.4f, 0.12f), Wood, 0f, "Poste");
        C(t, new Vector3(w * 0.35f, 0.25f, d * 0.35f), new Vector3(0.6f, 0.3f, 0.35f), Steel, 0f, "Bebedero");
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform Cages(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < 2; c++)
            {
                Vector3 o = new Vector3(-0.45f + c * 0.9f, 0.35f + r * 0.7f, 0f);
                C(t, o + Vector3.down * 0.3f, new Vector3(0.85f, 0.05f, 0.6f), Steel, 0f, "Suelo_Jaula");
                C(t, o + Vector3.up * 0.3f, new Vector3(0.85f, 0.03f, 0.6f), Steel, 0f, "Techo_Jaula");
                C(t, o + new Vector3(0f, 0f, 0.29f), new Vector3(0.85f, 0.6f, 0.02f), Steel, 0f, "Fondo");
                for (int b = 0; b < 6; b++) C(t, o + new Vector3(-0.36f + b * 0.145f, 0f, -0.3f), new Vector3(0.015f, 0.6f, 0.015f), Steel, 0f, "Barrote");
                C(t, o + new Vector3(0f, -0.25f, 0.05f), new Vector3(0.6f, 0.04f, 0.4f), Blue, 0f, "Manta");
            }
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform FloorScale(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.04f, 0f), new Vector3(1.6f, 0.08f, 1f), Dark, 0f, "Plataforma");
        C(t, new Vector3(0f, 0.085f, 0f), new Vector3(1.5f, 0.01f, 0.9f), H("9AA3AB"), 0f, "Antideslizante");
        C(t, new Vector3(0.95f, 0.6f, 0f), new Vector3(0.05f, 1.1f, 0.05f), Steel, 0f, "Columna");
        C(t, new Vector3(0.95f, 1.15f, 0f), new Vector3(0.1f, 0.2f, 0.3f), White, 0f, "Visor");
        C(t, new Vector3(0.9f, 1.15f, 0f), new Vector3(0.005f, 0.1f, 0.2f), Glow, 0f, "Peso");
        KitchenProps.Fit(t);
        return t;
    }

    public static Transform Ultrasound(Transform parent, string name, Vector3 pos, float yaw)
    {
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.7f, 0.45f), White, 0f, "Carro");
        for (int i = 0; i < 4; i++) C(t, new Vector3(i % 2 == 0 ? -0.2f : 0.2f, 0.04f, i < 2 ? -0.18f : 0.18f), new Vector3(0.06f, 0.08f, 0.08f), Black, 0f, "Rueda");
        C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.55f, 0.05f, 0.4f), H("DCE3E8"), 0f, "Teclado");
        C(t, new Vector3(0f, 1.15f, 0.12f), new Vector3(0.45f, 0.35f, 0.04f), Dark, 0f, "Monitor");
        C(t, new Vector3(0f, 1.15f, 0.097f), new Vector3(0.4f, 0.3f, 0.005f), Screen, 0f, "Pantalla");
        C(t, new Vector3(-0.05f, 1.15f, 0.093f), new Vector3(0.15f, 0.12f, 0.003f), H("9AA5A8"), 30f, "Eco");
        C(t, new Vector3(0.3f, 0.85f, -0.1f), new Vector3(0.05f, 0.12f, 0.04f), Mint, 0f, "Sonda");
        KitchenProps.Fit(t);
        return t;
    }
}

