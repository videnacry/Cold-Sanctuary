using UnityEngine;

/// <summary>
/// LA MEZCLA DEL MUNDO EN EL HUERTO Y EN CONSTRUCCIÓN (Berón 2026-11-05; reglas S14 y F14; docs/huertos-y-cultivos.md §Métodos del mundo,
/// docs/construccion-sostenible.md §Técnicas del mundo). Cada pieza es una técnica real, a escala, con su origen en el cartel; no sustituyen a la maquinaria ni a los cultivos.
/// </summary>
public static partial class SampleSceneBuilder
{
    static readonly Color WSoil = new Color(0.32f, 0.23f, 0.16f), WDry = new Color(0.72f, 0.6f, 0.42f), WWater = new Color(0.35f, 0.58f, 0.68f),
        WGreen = new Color(0.36f, 0.6f, 0.3f), WStone = new Color(0.56f, 0.54f, 0.5f), WBasalt = new Color(0.2f, 0.2f, 0.22f), WEarth = new Color(0.66f, 0.46f, 0.3f);

    static Transform Bed(Transform parent, string name, Vector3 pos, string label)
    {
        Transform t = KitchenProps.Root(parent, name, pos);
        WorldLabel wl = t.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = 2.3f;
        return t;
    }

    /// <summary>Seis parcelas de 4 × 4 m en la mitad libre del piso del Huerto: la misma franja donde los demás pisos del árbol tienen su cultivo de 22 × 18 m.</summary>
    static void FurnishWorldGarden(Transform floor)
    {
        const float y = 0.06f;
        // CHINAMPA: tierra sobre esteras, en el agua, con sauces en las esquinas.
        Transform c = Bed(floor, "Metodo_Chinampa", new Vector3(-6.5f, y, 3.5f), "Chinampa (Xochimilco, México): huerto entre canales");
        KitchenProps.C(c, new Vector3(0f, 0.05f, 0f), new Vector3(4f, 0.1f, 4f), WWater, 0f, "Canal");
        KitchenProps.C(c, new Vector3(0f, 0.2f, 0f), new Vector3(2.4f, 0.3f, 3.4f), WSoil, 0f, "Tierra_sobre_esteras");
        for (int s = -1; s <= 1; s += 2) for (int u = -1; u <= 1; u += 2)
            KitchenProps.C(c, new Vector3(s * 1.1f, 0.9f, u * 1.6f), new Vector3(0.14f, 1.2f, 0.14f), new Color(0.45f, 0.36f, 0.22f), 0f, "Sauce");
        for (int i = -2; i <= 2; i++) KitchenProps.C(c, new Vector3(0f, 0.42f, i * 0.6f), new Vector3(1.8f, 0.14f, 0.2f), WGreen, 0f, "Hilera");
        KitchenProps.Fit(c);
        // ZAÏ: suelo seco con hoyos de compost, a unos 80 cm unos de otros.
        Transform z = Bed(floor, "Metodo_Zai", new Vector3(1f, y, 3.5f), "Zaï (Burkina Faso): hoyos con compost que recuperan el suelo");
        KitchenProps.C(z, new Vector3(0f, 0.03f, 0f), new Vector3(4f, 0.06f, 4f), WDry, 0f, "Suelo_seco");
        for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++)
        {
            Vector3 p = new Vector3(-1.2f + i * 0.8f, 0f, -1.2f + j * 0.8f);
            KitchenProps.C(z, p + new Vector3(0f, 0.07f, 0f), new Vector3(0.4f, 0.02f, 0.4f), WSoil, 0f, "Hoyo_con_compost");
            KitchenProps.C(z, p + new Vector3(0f, 0.2f, 0f), new Vector3(0.08f, 0.26f, 0.08f), WGreen, 0f, "Brote");
        }
        KitchenProps.Fit(z);
        // ANDÉN: escalones de piedra con tierra encima.
        Transform a = Bed(floor, "Metodo_Anden", new Vector3(8.5f, y, 3.5f), "Andén (Andes): terrazas de piedra en la ladera");
        for (int k = 0; k < 3; k++)
        {
            float h = 0.3f * (k + 1), zc = -1.3f + k * 1.3f;
            KitchenProps.C(a, new Vector3(0f, h * 0.5f, zc), new Vector3(4f, h, 1.3f), WStone, 0f, "Muro_de_piedra");
            KitchenProps.C(a, new Vector3(0f, h + 0.04f, zc + 0.05f), new Vector3(3.8f, 0.08f, 1.1f), WSoil, 0f, "Tierra");
            KitchenProps.C(a, new Vector3(0f, h + 0.16f, zc + 0.05f), new Vector3(3.4f, 0.16f, 0.25f), WGreen, 0f, "Hilera");
        }
        KitchenProps.Fit(a);
        // HÜGELKULTUR: troncos bajo un montículo de tierra.
        Transform h2 = Bed(floor, "Metodo_Hugelkultur", new Vector3(-6.5f, y, 10f), "Hügelkultur (Europa central): montículo sobre troncos");
        for (int i = -2; i <= 2; i++) KitchenProps.C(h2, new Vector3(i * 0.55f, 0.15f, 0f), new Vector3(0.3f, 0.3f, 3.8f), CWood, 0f, "Tronco");
        for (int k = 0; k < 3; k++) KitchenProps.C(h2, new Vector3(0f, 0.4f + k * 0.25f, 0f), new Vector3(3f - k * 0.9f, 0.25f, 3.3f), WSoil, 0f, "Monticulo");
        for (int i = -1; i <= 1; i++) KitchenProps.C(h2, new Vector3(0f, 1.1f, i * 1f), new Vector3(0.7f, 0.2f, 0.5f), WGreen, 0f, "Calabaza_y_hojas");
        KitchenProps.Fit(h2);
        // BATDAM: muro bajo de basalto contra el viento, con su paso.
        Transform b = Bed(floor, "Metodo_Batdam", new Vector3(1f, y, 10f), "Batdam (Jeju, Corea): muretes de basalto contra el viento");
        KitchenProps.C(b, new Vector3(0f, 0.04f, 0f), new Vector3(3.6f, 0.08f, 3.6f), WSoil, 0f, "Tierra");
        for (int k = 0; k < 4; k++)
        {
            Transform side = KitchenProps.Root(b, "Murete", Vector3.zero, k * 90f);
            for (int i = -3; i <= 3; i++)
            {
                if (k == 0 && Mathf.Abs(i) <= 1) continue;   // el paso
                float high = 0.45f + (i & 1) * 0.12f;
                KitchenProps.C(side, new Vector3(i * 0.55f, high * 0.5f, 1.85f), new Vector3(0.52f, high, 0.3f), WBasalt, 0f, "Piedra");
            }
        }
        for (int i = -1; i <= 1; i++) KitchenProps.C(b, new Vector3(i * 0.9f, 0.2f, 0f), new Vector3(0.3f, 0.24f, 2.8f), WGreen, 0f, "Hilera");
        KitchenProps.Fit(b);
        // SUBAK: la acequia que se parte en dos a partes iguales, entre vecinos.
        Transform s2 = Bed(floor, "Metodo_Subak", new Vector3(8.5f, y, 10f), "Subak (Bali): el agua se reparte entre vecinos");
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(s2, new Vector3(s * 1.05f, 0.08f, -0.2f), new Vector3(1.8f, 0.16f, 3.4f), new Color(0.46f, 0.62f, 0.62f), 0f, "Arrozal_inundado");
            for (int i = -2; i <= 2; i++) KitchenProps.C(s2, new Vector3(s * 1.05f, 0.26f, -0.2f + i * 0.6f), new Vector3(1.4f, 0.2f, 0.08f), WGreen, 0f, "Arroz");
        }
        KitchenProps.C(s2, new Vector3(0f, 0.09f, 0f), new Vector3(0.14f, 0.18f, 3.8f), WStone, 0f, "Lomo_entre_vecinos");
        KitchenProps.C(s2, new Vector3(0f, 0.1f, 1.75f), new Vector3(4f, 0.2f, 0.4f), WWater, 0f, "Acequia");
        KitchenProps.C(s2, new Vector3(0f, 0.28f, 1.75f), new Vector3(0.12f, 0.2f, 0.42f), WStone, 0f, "Partidor_del_agua");
        KitchenProps.Fit(s2);
    }

    /// <summary>El muestrario de técnicas de obra: un tramo en la franja libre del borde +Z, entre los aseos y los conos, a 2 m del material.</summary>
    static void FurnishWorldConstruction(Transform r)
    {
        Transform line = new GameObject("Muestrario_tecnicas").transform; line.SetParent(r, false); line.gameObject.AddComponent<FurnitureLine>();
        line.localPosition = new Vector3(0f, 0f, 7.6f);
        Color cane = new Color(0.76f, 0.68f, 0.38f);
        // GUADUA y bahareque.
        Transform g = CPiece(line, "Tecnica_Guadua", new Vector3(-8f, 0f, 0f), 0f);
        KitchenProps.C(g, new Vector3(0f, 0.15f, 0f), new Vector3(1.8f, 0.3f, 1.2f), WStone, 0f, "Zocalo");   // la guadua no toca el suelo
        for (int s = -1; s <= 1; s += 2) for (int u = -1; u <= 1; u += 2)
            KitchenProps.C(g, new Vector3(s * 0.7f, 1.1f, u * 0.4f), new Vector3(0.12f, 1.6f, 0.12f), cane, 0f, "Cana_de_guadua");
        for (int u = -1; u <= 1; u += 2)
        {
            KitchenProps.C(g, new Vector3(0f, 1.85f, u * 0.4f), new Vector3(1.6f, 0.1f, 0.1f), cane, 0f, "Viga_boca_de_pez");
            KitchenProps.C(g, new Vector3(0.7f, 1.8f, u * 0.4f), new Vector3(0.16f, 0.1f, 0.16f), new Color(0.6f, 0.5f, 0.32f), 0f, "Atadura_de_canamo");
        }
        KitchenProps.C(g, new Vector3(0f, 0.9f, 0.4f), new Vector3(1.28f, 1.2f, 0.06f), new Color(0.6f, 0.52f, 0.36f), 0f, "Esterilla");
        KitchenProps.C(g, new Vector3(-0.3f, 0.9f, 0.34f), new Vector3(0.66f, 1.2f, 0.06f), WEarth, 0f, "Revoque_de_barro_y_paja");
        CLabel(g, "Guadua y bahareque (Colombia)", 2.4f);
        // TAPIA: la tierra apisonada entre tablas.
        Transform t = CPiece(line, "Tecnica_Tapia", new Vector3(-4.7f, 0f, 0f), 0f);
        for (int k = 0; k < 4; k++) KitchenProps.C(t, new Vector3(0f, 0.11f + k * 0.22f, 0f), new Vector3(1.6f, 0.22f, 0.5f), Color.Lerp(WEarth, WDry, (k % 2) * 0.45f), 0f, "Tongada_de_tierra");
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(t, new Vector3(0f, 1.15f, s * 0.29f), new Vector3(1.7f, 0.5f, 0.05f), CWood, 0f, "Tabla_del_tapial");
        KitchenProps.C(t, new Vector3(0.5f, 1.7f, 0f), new Vector3(0.07f, 1.2f, 0.07f), CWood, 0f, "Mango_del_pison");
        KitchenProps.C(t, new Vector3(0.5f, 1.05f, 0f), new Vector3(0.2f, 0.12f, 0.2f), CDark, 0f, "Pison");
        CLabel(t, "Tapia: tierra apisonada (Península Ibérica, Magreb y Andes)", 2.5f);
        // KIGUMI: madera ensamblada, sin clavos.
        Transform j = CPiece(line, "Tecnica_Kigumi", new Vector3(-1.4f, 0f, 0f), 0f);
        Color cedar = new Color(0.8f, 0.64f, 0.44f);
        KitchenProps.C(j, new Vector3(0f, 0.8f, 0f), new Vector3(0.24f, 1.6f, 0.24f), cedar, 0f, "Pilar");
        KitchenProps.C(j, new Vector3(-0.45f, 1.45f, 0f), new Vector3(0.7f, 0.2f, 0.16f), cedar, 0f, "Viga_izquierda");
        KitchenProps.C(j, new Vector3(0.45f, 1.45f, 0f), new Vector3(0.7f, 0.2f, 0.16f), Color.Lerp(cedar, CWood, 0.5f), 0f, "Viga_derecha");
        KitchenProps.C(j, new Vector3(0.14f, 1.45f, 0f), new Vector3(0.3f, 0.1f, 0.08f), new Color(0.6f, 0.44f, 0.28f), 0f, "Espiga_a_la_vista");
        KitchenProps.C(j, new Vector3(0f, 1.45f, 0.13f), new Vector3(0.06f, 0.06f, 0.1f), new Color(0.45f, 0.3f, 0.2f), 0f, "Clavija_de_madera");
        KitchenProps.C(j, new Vector3(-0.6f, 0.4f, 0f), new Vector3(0.6f, 0.8f, 0.5f), CWood, 0f, "Caballete");
        KitchenProps.C(j, new Vector3(-0.6f, 0.86f, 0f), new Vector3(0.28f, 0.1f, 0.08f), CDark, 0f, "Kanna_cepillo");
        CLabel(j, "Kigumi: ensamble sin clavos (Japón)", 2.2f);
        // BÓVEDA NUBIA: hiladas inclinadas contra el testero, sin cimbra.
        Transform v = CPiece(line, "Tecnica_Boveda_nubia", new Vector3(1.9f, 0f, 0f), 0f);
        KitchenProps.C(v, new Vector3(0f, 0.6f, -0.5f), new Vector3(1.7f, 1.2f, 0.16f), WEarth, 0f, "Testero");
        for (int k = 0; k < 4; k++)
            for (int i = 0; i <= 8; i++)
            {
                float ang = i * 22.5f, rad = ang * Mathf.Deg2Rad, zc = -0.34f + k * 0.2f;
                Vector3 p = new Vector3(0.68f * Mathf.Cos(rad), 0.08f + 0.9f * Mathf.Sin(rad) - k * 0.06f, zc);
                KitchenProps.C(v, p, new Vector3(0.22f, 0.3f, 0.09f), Color.Lerp(WEarth, WDry, (i + k) % 3 * 0.2f), new Vector3(-16f, 0f, ang - 90f), "Adobe");
            }
        CLabel(v, "Bóveda nubia: adobe sin cimbra (Nubia, Egipto)", 1.9f);
        // SILLERÍA INCA: piedra labrada y ajustada, sin mortero, con el muro un poco inclinado.
        Transform s2 = CPiece(line, "Tecnica_Silleria_inca", new Vector3(5.2f, 0f, 0f), 0f);
        Transform wall = KitchenProps.Root(s2, "Muestra_de_muro", Vector3.zero); wall.localRotation = Quaternion.Euler(-3f, 0f, 0f);
        float[][] rows = { new[] { 0.7f, 0.5f, 0.6f }, new[] { 0.45f, 0.8f, 0.55f }, new[] { 0.6f, 0.4f, 0.8f } };
        float yy = 0f;
        for (int k = 0; k < rows.Length; k++)
        {
            float x = -0.9f, high = 0.42f - k * 0.05f;
            for (int i = 0; i < rows[k].Length; i++)
            {
                KitchenProps.C(wall, new Vector3(x + rows[k][i] * 0.5f, yy + high * 0.5f, 0f), new Vector3(rows[k][i] - 0.015f, high - 0.015f, 0.6f), Color.Lerp(WStone, Color.white, (i + k) % 3 * 0.1f), 0f, "Sillar");
                x += rows[k][i];
            }
            yy += high;
        }
        CLabel(s2, "Sillería inca: piedra sin mortero (Perú)", 1.8f);
    }
}
