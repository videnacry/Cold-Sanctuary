using UnityEngine;
using St = MicroStyles;

/// <summary>
/// LOS INSTRUMENTOS Y EL EQUIPO DE LA MÚSICA (docs/microcosmos-musica.md, 2026-11-05): piano, batería, amplificador con su guitarra,
/// micrófono de pie, mesa de mezclas (magnetófono de cuatro pistas antes de 1990; portátil e interfaz después) y tarima de escenario
/// (se pisa). Acompañan a los puestos de un edificio (<c>CivicBuilder.NotStation</c>): no son puestos. Muebles detallados (regla M25).
/// </summary>
internal static partial class MicroFurniture
{
    internal static bool IsMusic(string key) => key == "piano" || key == "bateria" || key == "amplificador" || key == "microfono" || key == "mesa_mezclas" || key == "tarima";

    static bool FillMusic(Transform t, string key, St.Entry e, int year, ref bool walk)
    {
        switch (key)
        {
            case "piano": Piano(t, e); return true;
            case "bateria": Drums(t); return true;
            case "amplificador": Amplifier(t, e); return true;
            case "microfono": MicStand(t); return true;
            case "mesa_mezclas": MixingDesk(t, e, year); return true;
            case "tarima": Stage(t, e); walk = true; return true;
            default: return false;
        }
    }

    static void Piano(Transform t, St.Entry e)
    {
        Color black = H("1E1C1C");
        C(t, new Vector3(0f, 0.85f, 0.25f), new Vector3(1.5f, 1.2f, 0.45f), black, 0f, "Caja_del_piano");
        C(t, new Vector3(0f, 0.75f, -0.1f), new Vector3(1.5f, 0.1f, 0.35f), black, 0f, "Teclado");
        for (int k = 0; k < 14; k++) C(t, new Vector3(-0.65f + k * 0.1f, 0.81f, -0.18f), new Vector3(0.08f, 0.02f, 0.16f), Chalk, 0f, "Tecla");
        for (int k = 0; k < 9; k++) C(t, new Vector3(-0.6f + k * 0.15f, 0.84f, -0.13f), new Vector3(0.05f, 0.03f, 0.1f), black, 0f, "Tecla_negra");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.68f, 0.35f, -0.12f), new Vector3(0.08f, 0.7f, 0.08f), black, 0f, "Pata");
        C(t, new Vector3(0f, 1.05f, 0f), new Vector3(0.5f, 0.3f, 0.02f), Paper, 0f, "Partitura");
        C(t, new Vector3(0f, 0.25f, -0.65f), new Vector3(0.8f, 0.5f, 0.35f), e.wood, 0f, "Banqueta_del_piano");
    }

    static void Drums(Transform t)
    {
        Color shell = H("7A2E2A"), skin = H("E8E2D2"), metal = H("C9A24A");
        C(t, new Vector3(0f, 0.4f, 0.3f), new Vector3(0.6f, 0.6f, 0.45f), shell, 0f, "Bombo");
        for (int k = -1; k <= 1; k += 2)
        {
            C(t, new Vector3(k * 0.28f, 0.85f, 0.15f), new Vector3(0.3f, 0.2f, 0.3f), shell, 0f, "Timbal");
            C(t, new Vector3(k * 0.28f, 0.96f, 0.15f), new Vector3(0.28f, 0.02f, 0.28f), skin, 0f, "Parche");
            C(t, new Vector3(k * 0.75f, 0.6f, 0f), new Vector3(0.04f, 1.2f, 0.04f), Iron, 0f, "Soporte");
            C(t, new Vector3(k * 0.75f, 1.2f, 0f), new Vector3(0.45f, 0.02f, 0.45f), metal, 0f, "Platillo");
        }
        C(t, new Vector3(0.45f, 0.55f, -0.3f), new Vector3(0.36f, 0.18f, 0.36f), H("B0B4B8"), 0f, "Caja");
        C(t, new Vector3(0f, 0.25f, -0.6f), new Vector3(0.36f, 0.5f, 0.36f), H("2F2F33"), 0f, "Taburete");
    }

    static void Amplifier(Transform t, St.Entry e)
    {
        C(t, new Vector3(0f, 0.4f, 0f), new Vector3(0.8f, 0.8f, 0.4f), H("232326"), 0f, "Caja_del_amplificador");
        C(t, new Vector3(0f, 0.36f, -0.21f), new Vector3(0.62f, 0.6f, 0.02f), H("5E5A52"), 0f, "Rejilla");
        for (int k = 0; k < 4; k++) C(t, new Vector3(-0.24f + k * 0.16f, 0.74f, -0.21f), new Vector3(0.05f, 0.05f, 0.03f), Chrome, 0f, "Mando");
        // La guitarra en su soporte, junto al amplificador.
        C(t, new Vector3(0.75f, 0.3f, 0f), new Vector3(0.36f, 0.42f, 0.09f), e.wood, 0f, "Caja_de_guitarra");
        C(t, new Vector3(0.75f, 0.8f, 0f), new Vector3(0.06f, 0.6f, 0.05f), WoodDark, 0f, "Mastil");
        C(t, new Vector3(0.75f, 1.13f, 0f), new Vector3(0.09f, 0.1f, 0.05f), WoodDark, 0f, "Clavijero");
        C(t, new Vector3(0.75f, 0.08f, 0f), new Vector3(0.3f, 0.04f, 0.24f), Iron, 0f, "Soporte_de_guitarra");
    }

    static void MicStand(Transform t)
    {
        C(t, new Vector3(0f, 0.02f, 0f), new Vector3(0.3f, 0.04f, 0.3f), Iron, 0f, "Base");
        C(t, new Vector3(0f, 0.8f, 0f), new Vector3(0.03f, 1.56f, 0.03f), Chrome, 0f, "Pie");
        C(t, new Vector3(0f, 1.62f, -0.06f), new Vector3(0.07f, 0.16f, 0.07f), H("2F2F33"), 0f, "Microfono");
        C(t, new Vector3(0f, 1.71f, -0.06f), new Vector3(0.08f, 0.05f, 0.08f), Chrome, 0f, "Rejilla_del_microfono");
    }

    static void MixingDesk(Transform t, St.Entry e, int year)
    {
        C(t, new Vector3(0f, 0.38f, 0f), new Vector3(1.6f, 0.76f, 0.7f), e.wood, 0f, "Mesa");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.9f, 0.25f, 0.1f), new Vector3(0.3f, 0.5f, 0.3f), H("232326"), 0f, "Altavoz");
        if (year >= 1990)
        {   // el portátil, la interfaz y los auriculares de quien graba en casa
            C(t, new Vector3(0f, 0.78f, -0.05f), new Vector3(0.36f, 0.02f, 0.25f), H("B0B4B8"), 0f, "Portatil");
            C(t, new Vector3(0f, 0.9f, 0.08f), new Vector3(0.36f, 0.24f, 0.02f), H("2F3A40"), 0f, "Pantalla");
            C(t, new Vector3(0.45f, 0.8f, 0f), new Vector3(0.22f, 0.05f, 0.14f), H("7A2E2A"), 0f, "Interfaz_de_audio");
            C(t, new Vector3(-0.45f, 0.8f, 0f), new Vector3(0.18f, 0.08f, 0.18f), H("232326"), 0f, "Auriculares");
        }
        else
        {   // el magnetófono de cuatro pistas: dos bobinas y sus vúmetros
            C(t, new Vector3(0f, 0.9f, 0.1f), new Vector3(0.9f, 0.3f, 0.3f), H("8A9096"), 0f, "Magnetofono");
            for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.25f, 1.07f, 0.1f), new Vector3(0.26f, 0.03f, 0.26f), H("2F2F33"), 0f, "Bobina");
            for (int k = 0; k < 8; k++) C(t, new Vector3(-0.56f + k * 0.16f, 0.79f, -0.2f), new Vector3(0.1f, 0.03f, 0.14f), k % 3 == 0 ? Flame : H("5E666E"), 0f, "Fader");
        }
    }

    static void Stage(Transform t, St.Entry e)
    {
        C(t, new Vector3(0f, 0.15f, 0f), new Vector3(6f, 0.3f, 4f), H("2F2B28"), 0f, "Tarima");
        C(t, new Vector3(0f, 0.31f, 0f), new Vector3(5.8f, 0.02f, 3.8f), e.wood, 0f, "Tablas_de_la_tarima");
        for (int k = -1; k <= 1; k += 2)
        {
            C(t, new Vector3(k * 3.2f, 0.6f, 1.6f), new Vector3(0.7f, 1.2f, 0.6f), H("232326"), 0f, "Altavoz_de_escenario");
            C(t, new Vector3(k * 3.2f, 1.4f, 1.6f), new Vector3(0.5f, 0.4f, 0.5f), H("3A3A3E"), 0f, "Altavoz_de_agudos");
        }
    }
}
