using UnityEngine;
using St = MicroStyles;

/// <summary>
/// LOS MUEBLES DE LOS EDIFICIOS DEL PUEBLO Y LOS FRUTALES (docs/microcosmos-reglas-edificacion.md §8–11; Berón 2026-11-03). Como
/// el resto de <see cref="MicroFurniture"/>: varias piezas de pocos cubos (low-poly), con el color de los materiales de su estilo
/// de época y lugar (<see cref="MicroStyles"/>). Claves nuevas: frutal (el de la región: <see cref="MicroStyles.Fruits"/>), estatua
/// (o estela maya, o león guardián chino), fuente, vitrina, piscina (se pisa), temazcal, pista de baile (se pisa), consola (centralita
/// o calculadora según el año), cuna, armero, andén (se pisa), carro (de bomberos o de carga), caballete de carpintero y material
/// de obra.
/// </summary>
internal static partial class MicroFurniture
{
    /// <summary>El frutal de un sitio: uno de los de su estilo, escogido por <paramref name="seed"/>.</summary>
    internal static St.Fruit FruitFor(St.Entry e, int seed) { St.Fruit[] l = St.Fruits(e); return l[Mathf.Abs(seed) % l.Length]; }

    static bool FillCivic(Transform t, string key, St.Entry e, int year, int seed, ref string need, ref bool walk)
    {
        switch (key)
        {
            case "frutal": FruitTree(t, FruitFor(e, seed)); return true;
            case "estatua": Statue(t, e); return true;
            case "fuente": Fountain(t, e); need = "lavarse"; return true;
            case "vitrina": Showcase(t, e, seed); return true;
            case "piscina": Pool(t, e); walk = true; return true;
            case "temazcal": SweatBath(t); return true;
            case "pista": DanceFloor(t, e); walk = true; return true;
            case "consola": Console(t, e, year); return true;
            case "cuna": Crib(t, e); need = "dormir"; return true;
            case "armero": Rack(t, e, year); return true;
            case "anden": Platform(t, e, year); walk = true; return true;
            case "carro": Cart(t, e, year); return true;
            case "caballete": Sawhorse(t, e); return true;
            case "material": BuildingMaterial(t, e); return true;
        }
        return false;
    }

    static void FruitTree(Transform t, St.Fruit fr)
    {
        float h = fr.height;
        C(t, new Vector3(0f, 0.06f, 0f), new Vector3(1.6f, 0.12f, 1.6f), Soil, 0f, "Alcorque");
        if (fr.bush)
        {
            for (int k = 0; k < 3; k++) C(t, new Vector3(-0.35f + k * 0.35f, h * 0.45f, (k % 2) * 0.2f - 0.1f), new Vector3(0.7f, h * 0.9f, 0.7f), fr.leaf, k * 30f, "Mata");
            for (int k = 0; k < 8; k++) C(t, new Vector3(-0.5f + (k % 4) * 0.33f, h * (0.35f + 0.12f * (k / 4)), k % 2 == 0 ? -0.38f : 0.38f), Vector3.one * 0.1f, fr.fruit, 0f, "Fruto");
            return;
        }
        Color bark = H("5A4632");
        if (fr.palm)
        {
            C(t, new Vector3(0f, h * 0.47f, 0f), new Vector3(0.3f, h * 0.94f, 0.3f), bark, 0f, "Estipe");
            for (int k = 0; k < 6; k++) C(t, Vector3.up * h + Quaternion.Euler(0f, k * 60f, 0f) * new Vector3(0f, -0.3f, 0.9f), new Vector3(0.5f, 0.06f, 2f), fr.leaf, new Vector3(25f, k * 60f, 0f), "Hoja");
            for (int k = 0; k < 4; k++) C(t, Vector3.up * (h - 0.6f) + Quaternion.Euler(0f, k * 90f, 0f) * new Vector3(0f, 0f, 0.3f), Vector3.one * 0.3f, fr.fruit, 0f, "Racimo");
            return;
        }
        float tw = Mathf.Clamp(h * 0.05f, 0.18f, 0.4f), crown = Mathf.Clamp(h * 0.55f, 1.6f, 5f);
        C(t, new Vector3(0f, h * 0.25f, 0f), new Vector3(tw, h * 0.5f, tw), bark, 0f, "Tronco");
        C(t, new Vector3(0f, h * 0.62f, 0f), new Vector3(crown, h * 0.45f, crown), fr.leaf, 0f, "Copa");
        C(t, new Vector3(0f, h * 0.7f, 0f), new Vector3(crown * 0.75f, h * 0.4f, crown * 0.75f), fr.leaf, 45f, "Copa");
        for (int k = 0; k < 8; k++)
        {
            float a = k * 45f * Mathf.Deg2Rad;
            C(t, new Vector3(Mathf.Sin(a) * crown * 0.5f, h * (0.5f + 0.06f * (k % 3)), Mathf.Cos(a) * crown * 0.5f), Vector3.one * Mathf.Clamp(h * 0.04f, 0.12f, 0.3f), fr.fruit, 0f, "Fruto");
        }
    }

    static void Statue(Transform t, St.Entry e)
    {
        Color stone = e.id == "maya_clasico" ? H("C8C0A8") : e.id == "egipto" ? H("C9A86A") : e.id == "sovietico" ? H("5A5E60") : H("BDB6A8");
        C(t, new Vector3(0f, 0.5f, 0f), new Vector3(1.4f, 1f, 1.4f), Stone, 0f, "Peana");
        if (e.id == "maya_clasico" || e.id == "mesopotamia")
        {   // la ESTELA: losa tallada con su glifo (Palenque prefería los tableros, pero las estelas son el monumento maya)
            C(t, new Vector3(0f, 2.4f, 0f), new Vector3(1f, 2.8f, 0.35f), stone, 0f, "Estela");
            C(t, new Vector3(0f, 2.6f, -0.19f), new Vector3(0.7f, 1.6f, 0.04f), Color.Lerp(stone, Color.black, 0.25f), 0f, "Relieve");
            for (int k = 0; k < 4; k++) C(t, new Vector3(-0.25f + (k % 2) * 0.5f, 3.55f - (k / 2) * 0.3f, -0.2f), new Vector3(0.18f, 0.18f, 0.03f), Color.Lerp(stone, Color.black, 0.35f), 0f, "Glifo");
            return;
        }
        if (e.id == "china_tradicional" || e.id == "hongkong_tonglau")
        {   // león guardián (shishi): cuerpo sentado, cabeza grande, la bola bajo la pata
            C(t, new Vector3(0f, 1.5f, 0.1f), new Vector3(0.7f, 1f, 0.9f), stone, 0f, "Cuerpo");
            C(t, new Vector3(0f, 2.2f, -0.2f), new Vector3(0.75f, 0.65f, 0.6f), stone, 0f, "Cabeza");
            C(t, new Vector3(0.25f, 1.15f, -0.45f), Vector3.one * 0.3f, stone, 0f, "Bola");
            return;
        }
        C(t, new Vector3(0f, 1.85f, 0f), new Vector3(0.6f, 1.4f, 0.4f), stone, 0f, "Figura");
        C(t, new Vector3(0f, 2.75f, 0f), new Vector3(0.36f, 0.4f, 0.36f), stone, 0f, "Cabeza");
        C(t, new Vector3(-0.42f, 2.1f, 0f), new Vector3(0.18f, 0.9f, 0.2f), stone, new Vector3(0f, 0f, -12f), "Brazo");
        C(t, new Vector3(0.42f, 2.3f, -0.1f), new Vector3(0.18f, 0.9f, 0.2f), stone, new Vector3(-40f, 0f, 10f), "Brazo");
    }

    static void Fountain(Transform t, St.Entry e)
    {
        Color stone = e.id == "otomano" || e.id == "islam_abasi" ? H("D9D0BC") : Stone;
        for (int s = -1; s <= 1; s += 2)
        {
            C(t, new Vector3(0f, 0.3f, s * 1.1f), new Vector3(2.4f, 0.6f, 0.2f), stone, 0f, "Pretil");
            C(t, new Vector3(s * 1.1f, 0.3f, 0f), new Vector3(0.2f, 0.6f, 2.4f), stone, 0f, "Pretil");
        }
        C(t, new Vector3(0f, 0.45f, 0f), new Vector3(2f, 0.04f, 2f), Water, 0f, "Agua");
        C(t, new Vector3(0f, 0.9f, 0f), new Vector3(0.35f, 1.8f, 0.35f), stone, 0f, "Columna");
        C(t, new Vector3(0f, 1.6f, -0.3f), new Vector3(0.1f, 0.1f, 0.35f), Brass, 0f, "Caño");
        C(t, new Vector3(0f, 1.1f, -0.45f), new Vector3(0.06f, 0.9f, 0.06f), Water, 0f, "Chorro");
    }

    static void Showcase(Transform t, St.Entry e, int seed)
    {
        C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.6f), e.wood, 0f, "Mueble");
        C(t, new Vector3(0f, 1.2f, 0f), new Vector3(1.5f, 0.6f, 0.5f), H("CFE3EA"), 0f, "Vidrio");
        Color[] things = { Clay, Brass, Paper, Copper, Stone };
        for (int k = 0; k < 3; k++) C(t, new Vector3(-0.5f + k * 0.5f, 1.05f, 0f), new Vector3(0.22f, 0.25f + 0.1f * k, 0.22f), things[(seed + k) & 3], 0f, "Pieza_expuesta");
        C(t, new Vector3(0f, 0.95f, -0.32f), new Vector3(0.4f, 0.12f, 0.02f), Paper, 0f, "Cartela");
    }

    static void Pool(Transform t, St.Entry e)
    {
        Color tile = e.id == "sovietico" || e.id == "occidental_moderna" || e.id == "norteamerica_moderna" ? H("DDE6EA") : e.id == "roma" || e.id == "egeo" ? H("D8CBB0") : Stone;
        C(t, new Vector3(0f, 0.15f, 2.1f), new Vector3(6.4f, 0.3f, 0.2f), tile, 0f, "Borde");
        C(t, new Vector3(0f, 0.15f, -2.1f), new Vector3(6.4f, 0.3f, 0.2f), tile, 0f, "Borde");
        C(t, new Vector3(3.1f, 0.15f, 0f), new Vector3(0.2f, 0.3f, 4.2f), tile, 0f, "Borde");
        C(t, new Vector3(-3.1f, 0.15f, 0f), new Vector3(0.2f, 0.3f, 4.2f), tile, 0f, "Borde");
        C(t, new Vector3(0f, 0.1f, 0f), new Vector3(6f, 0.04f, 4f), Water, 0f, "Agua");
        C(t, new Vector3(-2.6f, 0.3f, -2.25f), new Vector3(0.6f, 0.08f, 0.3f), tile, 0f, "Escalon");
    }

    static void SweatBath(Transform t)
    {   // el TEMAZCAL (pib naah): cámara baja de mampostería con su hogar de piedras calientes y la puerta pequeña
        Color stone = H("B8AE98");
        C(t, new Vector3(0f, 0.8f, 0.3f), new Vector3(2.2f, 1.6f, 2f), stone, 0f, "Camara");
        C(t, new Vector3(0f, 1.7f, 0.3f), new Vector3(1.6f, 0.25f, 1.4f), stone, 0f, "Boveda");
        C(t, new Vector3(0f, 0.45f, -0.72f), new Vector3(0.7f, 0.9f, 0.06f), Ash, 0f, "Puerta");
        C(t, new Vector3(0.8f, 0.25f, -0.95f), new Vector3(0.5f, 0.3f, 0.5f), Ember, 0f, "Piedras_calientes");
        C(t, new Vector3(-0.8f, 0.2f, -0.95f), new Vector3(0.35f, 0.4f, 0.35f), Clay, 0f, "Cantaro_de_agua");
    }

    static void DanceFloor(Transform t, St.Entry e)
    {
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(6f, 0.06f, 6f), e.id == "maya_clasico" ? H("D8D0BC") : e.wood, 0f, "Pista");
        C(t, new Vector3(0f, 0.065f, 0f), new Vector3(5.6f, 0.01f, 5.6f), Color.Lerp(e.wood, Color.white, 0.25f), 0f, "Marquetería");
    }

    static void Console(Transform t, St.Entry e, int year)
    {
        C(t, new Vector3(0f, 0.38f, 0f), new Vector3(1.6f, 0.76f, 0.7f), year >= 1950 ? H("8A9096") : e.wood, 0f, "Pupitre");
        if (year >= 1950)
        {   // armarios de la calculadora con sus cintas y su consola de luces
            for (int k = -1; k <= 1; k++) C(t, new Vector3(k * 0.55f, 1.2f, 0.2f), new Vector3(0.5f, 1.6f, 0.3f), H("5E666E"), 0f, "Armario_de_calculo");
            for (int k = -1; k <= 1; k++) C(t, new Vector3(k * 0.55f, 1.45f, 0.04f), new Vector3(0.3f, 0.3f, 0.02f), H("2F3A40"), 0f, "Cinta");
            for (int k = 0; k < 6; k++) C(t, new Vector3(-0.5f + k * 0.2f, 0.78f, -0.15f), new Vector3(0.05f, 0.03f, 0.05f), k % 2 == 0 ? Flame : Ember, 0f, "Luz");
        }
        else if (year >= 1880)
        {   // centralita telefónica: el panel de clavijas y los cordones
            C(t, new Vector3(0f, 1.25f, 0.25f), new Vector3(1.5f, 1f, 0.15f), e.wood, 0f, "Panel_de_clavijas");
            for (int k = 0; k < 12; k++) C(t, new Vector3(-0.6f + (k % 6) * 0.24f, 1.05f + (k / 6) * 0.3f, 0.16f), new Vector3(0.05f, 0.05f, 0.03f), Brass, 0f, "Clavija");
            C(t, new Vector3(0.3f, 0.8f, -0.1f), new Vector3(0.2f, 0.05f, 0.15f), Iron, 0f, "Auricular");
        }
        else
        {   // taller de instrumentos: astrolabio o engranajes sobre la mesa
            C(t, new Vector3(-0.3f, 0.85f, 0f), new Vector3(0.4f, 0.04f, 0.4f), Brass, new Vector3(70f, 0f, 0f), "Astrolabio");
            C(t, new Vector3(0.35f, 0.8f, 0f), new Vector3(0.25f, 0.06f, 0.25f), Copper, 0f, "Engranaje");
            C(t, new Vector3(0.35f, 0.86f, 0.15f), new Vector3(0.15f, 0.06f, 0.15f), Copper, 20f, "Engranaje");
        }
    }

    static void Crib(Transform t, St.Entry e)
    {
        C(t, new Vector3(0f, 0.3f, 0f), new Vector3(0.75f, 0.06f, 1.3f), e.wood, 0f, "Somier");
        C(t, new Vector3(0f, 0.38f, 0f), new Vector3(0.68f, 0.1f, 1.2f), Linen, 0f, "Colchon");
        for (int s = -1; s <= 1; s += 2)
        {
            C(t, new Vector3(s * 0.36f, 0.5f, 0f), new Vector3(0.04f, 0.5f, 1.3f), e.wood, 0f, "Baranda");
            C(t, new Vector3(0f, 0.5f, s * 0.64f), new Vector3(0.75f, 0.5f, 0.04f), e.wood, 0f, "Cabecero");
        }
        C(t, new Vector3(0f, 0.15f, 0f), new Vector3(0.7f, 0.3f, 1.2f), Color.Lerp(e.wood, Color.black, 0.2f), 0f, "Patas");
        C(t, new Vector3(0.1f, 0.46f, 0.3f), new Vector3(0.25f, 0.08f, 0.2f), e.textile, 0f, "Mantita");
    }

    static void Rack(Transform t, St.Entry e, int year)
    {
        C(t, new Vector3(0f, 0.05f, 0f), new Vector3(1.6f, 0.1f, 0.4f), e.wood, 0f, "Base");
        C(t, new Vector3(0f, 1.3f, 0.15f), new Vector3(1.6f, 0.08f, 0.1f), e.wood, 0f, "Travesaño");
        bool guns = year >= 1700;
        for (int k = 0; k < 5; k++) C(t, new Vector3(-0.6f + k * 0.3f, guns ? 0.75f : 1f, 0.05f), new Vector3(0.05f, guns ? 1.4f : 2f, 0.05f), guns ? Iron : e.wood, 0f, guns ? "Fusil" : "Lanza");
        if (!guns) for (int k = 0; k < 5; k++) C(t, new Vector3(-0.6f + k * 0.3f, 2.05f, 0.05f), new Vector3(0.08f, 0.15f, 0.03f), e.id == "maya_clasico" ? H("2A2A2E") : Iron, 0f, "Punta");   // obsidiana
        C(t, new Vector3(0.95f, 0.4f, 0f), new Vector3(0.5f, 0.8f, 0.12f), e.textile, 0f, e.id == "maya_clasico" ? "Escudo_de_algodon" : "Escudo");
    }

    static void Platform(Transform t, St.Entry e, int year)
    {
        C(t, new Vector3(0f, 0.2f, 0f), new Vector3(8f, 0.4f, 3f), year >= 1830 ? H("9A9690") : e.wood, 0f, "Anden");
        C(t, new Vector3(0f, 0.41f, -1.35f), new Vector3(8f, 0.02f, 0.15f), H("E8D25A"), 0f, "Linea_de_borde");
        C(t, new Vector3(3f, 1.5f, 1.2f), new Vector3(0.1f, 2.2f, 0.1f), Iron, 0f, "Poste_del_cartel");
        C(t, new Vector3(3f, 2.5f, 1.2f), new Vector3(1.4f, 0.4f, 0.06f), Paper, 0f, "Cartel");
        C(t, new Vector3(-2f, 0.65f, 1f), new Vector3(1.8f, 0.08f, 0.45f), e.wood, 0f, "Banco_del_anden");
    }

    static void Cart(Transform t, St.Entry e, int year)
    {
        bool engine = year >= 1900;
        Color body = engine ? H("B02A20") : e.wood;
        C(t, new Vector3(0f, 0.8f, 0f), new Vector3(1.6f, 0.8f, 3f), body, 0f, "Caja");
        for (int sx = -1; sx <= 1; sx += 2) for (int sz = -1; sz <= 1; sz += 2)
            C(t, new Vector3(sx * 0.85f, 0.4f, sz * 1f), new Vector3(0.12f, 0.8f, 0.8f), Iron, 0f, "Rueda");
        C(t, new Vector3(0f, 1.3f, 0.6f), new Vector3(0.5f, 0.3f, 1.4f), Brass, new Vector3(0f, 0f, 90f), "Bomba_o_tonel");
        C(t, new Vector3(0f, 1.25f, -0.9f), new Vector3(1.2f, 0.12f, 0.12f), H("C9B48C"), 0f, "Manguera_o_carga");
        if (!engine) C(t, new Vector3(0f, 0.5f, -2.1f), new Vector3(0.08f, 0.08f, 1.4f), e.wood, 0f, "Lanza_de_tiro");
    }

    static void Sawhorse(Transform t, St.Entry e)
    {
        for (int s = -1; s <= 1; s += 2)
        {
            C(t, new Vector3(s * 0.7f, 0.6f, 0f), new Vector3(0.08f, 0.08f, 0.6f), e.wood, 0f, "Caballete");
            C(t, new Vector3(s * 0.7f, 0.3f, -0.2f), new Vector3(0.06f, 0.6f, 0.06f), e.wood, new Vector3(-15f, 0f, 0f), "Pata");
            C(t, new Vector3(s * 0.7f, 0.3f, 0.2f), new Vector3(0.06f, 0.6f, 0.06f), e.wood, new Vector3(15f, 0f, 0f), "Pata");
        }
        C(t, new Vector3(0f, 0.68f, 0f), new Vector3(2f, 0.06f, 0.3f), Color.Lerp(e.wood, Color.white, 0.2f), 0f, "Tablon");
        C(t, new Vector3(0.3f, 0.75f, -0.05f), new Vector3(0.6f, 0.08f, 0.02f), Iron, 0f, "Sierra");
        C(t, new Vector3(-0.5f, 0.74f, 0.05f), new Vector3(0.25f, 0.06f, 0.06f), e.wood, 0f, "Cepillo");
    }

    static void BuildingMaterial(Transform t, St.Entry e)
    {
        Color block = e.id == "maya_clasico" ? H("D8D0BC") : e.id == "egipto" || e.id == "mesopotamia" ? H("B89A70") : e.id == "sovietico" ? H("A8A8A4") : Brick;
        C(t, new Vector3(-0.7f, 0.06f, 0f), new Vector3(1.1f, 0.12f, 1.1f), e.wood, 0f, "Pale");
        for (int y = 0; y < 3; y++) C(t, new Vector3(-0.7f, 0.25f + y * 0.22f, 0f), new Vector3(1f, 0.2f, 1f), block, y * 8f, "Bloques");
        for (int k = 0; k < 4; k++) C(t, new Vector3(0.7f, 0.08f + k * 0.13f, 0f), new Vector3(0.15f, 0.12f, 2.4f), e.wood, 0f, "Viga");
        C(t, new Vector3(0.7f, 0.3f, -1.4f), new Vector3(0.5f, 0.6f, 0.5f), H("D8D4C8"), 0f, e.id == "maya_clasico" ? "Saco_de_cal" : "Saco_de_cemento");
    }
}
