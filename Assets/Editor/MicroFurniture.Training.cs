using UnityEngine;
using St = MicroStyles;

/// <summary>
/// LOS APARATOS DE ENTRENAR DE CADA ÉPOCA (2026-11-04; Berón: «¿en ninguna época de microcosmos hay gimnasio? … se investigaría la
/// época/lugar para recrear edificaciones, muebles y maquinaria nativos»; docs/microcosmos-entrenamiento-eras.md, con fuentes).
/// Como el resto de <see cref="MicroFurniture"/>: pocas piezas low-poly con los materiales de su estilo. Claves nuevas:
/// - foso_lucha (se pisa): el skamma griego de arena, la akhāṛā de tierra con aceite, el ruedo de serrín del Schwingen, el círculo
///   de lucha del prado inglés;
/// - pista_carreras (se pisa): el dromos o el xisto, la manga de coleo, el parcours de Hébert, la pista de atletismo;
/// - campo (se pisa): un campo de pelota con sus porterías (el fútbol de la tregua, el terreno de deporte de Uagadugú);
/// - halteres (piedra o plomo, para el salto griego), piedras (de levantar: el nal indio, la piedra de Unspunnen, el candado de
///   piedra chino, el saco de arena de Beni Hasan), mazas (gada y mugdar, meels, mazas indias victorianas), pesas (barra y discos);
/// - saco (el kórykos griego, el sha bao chino, el saco de boxeo), makiwara (Okinawa/Japón);
/// - barra_fija, paralelas, potro, anillas (la Turnhalle de Jahn, la YMCA, el método natural);
/// - pista_esgrima (se pisa), diana (el blanco del arco: qabaq, mato, Okmeydanı), balon (pelota de hule, camp-ball, balón de cuero).
/// </summary>
internal static partial class MicroFurniture
{
    static readonly Color Sand = new Color(0.86f, 0.78f, 0.58f), Earth = new Color(0.55f, 0.4f, 0.28f), Sawdust = new Color(0.82f, 0.7f, 0.48f),
        Leather = new Color(0.45f, 0.28f, 0.16f), Rubber = new Color(0.18f, 0.16f, 0.14f), Lead = new Color(0.45f, 0.47f, 0.5f);

    static bool FillTraining(Transform t, string key, St.Entry e, int year, int seed, ref bool walk)
    {
        switch (key)
        {
            case "foso_lucha": WrestlingPit(t, e); walk = true; return true;
            case "pista_carreras": RunningTrack(t, e, year); walk = true; return true;
            case "campo": BallField(t, e); walk = true; return true;
            case "pista_esgrima": FencingPiste(t, e); walk = true; return true;
            case "halteres": Halteres(t, e); return true;
            case "piedras": LiftingStones(t, e); return true;
            case "mazas": Clubs(t, e, year); return true;
            case "pesas": Barbell(t); return true;
            case "saco": Bag(t, e); return true;
            case "makiwara": Makiwara(t); return true;
            case "barra_fija": HighBar(t, e); return true;
            case "paralelas": ParallelBars(t, e); return true;
            case "potro": VaultingHorse(t, e); return true;
            case "anillas": Rings(t, e); return true;
            case "diana": Target(t, e); return true;
            case "balon": Ball(t, e, year); return true;
        }
        return false;
    }

    // El suelo del foso: arena (Grecia, Egipto, Roma, Mesopotamia), tierra con aceite (akhāṛā, güreş), serrín (Schwingen), hierba.
    static Color PitFloor(St.Entry e) => e.id == "sur_asia" || e.id == "india_moderna" ? Earth : e.id == "alpino" ? Sawdust :
        e.id == "estuardo" || e.id == "georgiano" ? new Color(0.36f, 0.55f, 0.28f) : e.id == "otomano" || e.id == "islam_abasi" ? new Color(0.6f, 0.5f, 0.36f) : Sand;

    static void WrestlingPit(Transform t, St.Entry e)
    {
        Color floor = PitFloor(e), rim = Color.Lerp(floor, Color.black, 0.3f);
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(5f, 0.06f, 5f), floor, 0f, "Suelo_del_foso");
        C(t, new Vector3(0f, 0.08f, 2.45f), new Vector3(5.2f, 0.16f, 0.2f), rim, 0f, "Borde");
        C(t, new Vector3(0f, 0.08f, -2.45f), new Vector3(5.2f, 0.16f, 0.2f), rim, 0f, "Borde");
        C(t, new Vector3(2.45f, 0.08f, 0f), new Vector3(0.2f, 0.16f, 5f), rim, 0f, "Borde");
        C(t, new Vector3(-2.45f, 0.08f, 0f), new Vector3(0.2f, 0.16f, 5f), rim, 0f, "Borde");
        if (e.id == "sur_asia" || e.id == "india_moderna")   // la akhāṛā: el ídolo de Hanumān en su esquina y el cántaro de aceite
        { C(t, new Vector3(2.1f, 0.4f, 2.1f), new Vector3(0.35f, 0.8f, 0.35f), new Color(0.85f, 0.35f, 0.15f), 0f, "Hanuman"); C(t, new Vector3(-2.1f, 0.25f, 2.1f), new Vector3(0.3f, 0.5f, 0.3f), Clay, 0f, "Aceite_y_curcuma"); }
        else if (e.id == "egeo" || e.id == "roma")   // el pico para cavar el skamma y el aceite con los estrígiles
        { C(t, new Vector3(2.1f, 0.4f, 2.1f), new Vector3(0.06f, 0.8f, 0.06f), Wood, 0f, "Pico_del_skamma"); C(t, new Vector3(-2.1f, 0.2f, 2.1f), new Vector3(0.22f, 0.4f, 0.22f), Clay, 0f, "Aribalo_de_aceite"); }
        else if (e.id == "otomano") C(t, new Vector3(-2.1f, 0.2f, 2.1f), new Vector3(0.4f, 0.4f, 0.4f), Copper, 0f, "Caldero_de_aceite_de_oliva");
    }

    static void RunningTrack(Transform t, St.Entry e, int year)
    {
        bool modern = year >= 1900 && e.id != "frente_1914";
        Color lane = modern && year >= 1960 ? new Color(0.62f, 0.27f, 0.2f) : e.id == "hispanoamerica" ? Earth : Sand;
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(2.4f, 0.06f, 12f), lane, 0f, "Pista");
        C(t, new Vector3(0f, 0.065f, 5.6f), new Vector3(2.4f, 0.01f, 0.12f), Chalk, 0f, "Salida");   // la balbís griega, la línea de salida
        C(t, new Vector3(0f, 0.065f, -5.6f), new Vector3(2.4f, 0.01f, 0.12f), Chalk, 0f, "Meta");
        if (e.id == "egeo" || e.id == "roma") C(t, new Vector3(0f, 0.6f, -5.9f), new Vector3(0.15f, 1.2f, 0.15f), Stone, 0f, "Kampter (poste de la vuelta)");
        if (e.id == "hispanoamerica") for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 1.3f, 0.6f, 0f), new Vector3(0.1f, 1.2f, 11f), Wood, 0f, "Talanquera_de_la_manga");
        if (e.id == "francia_rural") for (int k = 0; k < 3; k++) C(t, new Vector3(0f, 0.25f + k * 0.15f, -2f + k * 2f), new Vector3(2f, 0.1f, 0.1f), Wood, 0f, "Obstaculo_del_parcours");
    }

    static void BallField(Transform t, St.Entry e)
    {
        Color grass = e.id == "frente_1914" ? new Color(0.45f, 0.42f, 0.3f) : e.id == "sahel_moderno" ? new Color(0.7f, 0.52f, 0.35f) : new Color(0.38f, 0.58f, 0.3f);
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(7f, 0.06f, 11f), grass, 0f, "Campo");
        C(t, new Vector3(0f, 0.065f, 0f), new Vector3(7f, 0.01f, 0.1f), Chalk, 0f, "Linea_central");
        for (int s = -1; s <= 1; s += 2)
        {
            if (e.id == "frente_1914") { C(t, new Vector3(-1f, 0.3f, s * 5.3f), new Vector3(0.35f, 0.6f, 0.35f), new Color(0.35f, 0.3f, 0.22f), 0f, "Gorra_de_poste"); C(t, new Vector3(1f, 0.3f, s * 5.3f), new Vector3(0.35f, 0.6f, 0.35f), new Color(0.35f, 0.3f, 0.22f), 0f, "Casco_de_poste"); continue; }   // porterías de gorras y cascos
            C(t, new Vector3(-1.1f, 0.6f, s * 5.4f), new Vector3(0.08f, 1.2f, 0.08f), Chalk, 0f, "Poste");
            C(t, new Vector3(1.1f, 0.6f, s * 5.4f), new Vector3(0.08f, 1.2f, 0.08f), Chalk, 0f, "Poste");
            C(t, new Vector3(0f, 1.2f, s * 5.4f), new Vector3(2.3f, 0.08f, 0.08f), Chalk, 0f, "Larguero");
        }
    }

    static void FencingPiste(Transform t, St.Entry e)
    {
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(2f, 0.06f, 10f), Color.Lerp(e.wood, Color.white, 0.15f), 0f, "Pista_de_esgrima");
        C(t, new Vector3(0f, 0.065f, 0f), new Vector3(2f, 0.01f, 0.06f), Chalk, 0f, "Linea_central");
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(0f, 0.065f, s * 2f), new Vector3(2f, 0.01f, 0.06f), Chalk, 0f, "Linea_de_guardia");
    }

    static void Halteres(Transform t, St.Entry e)
    {   // los HALTERES griegos: piedra o plomo con asidero, por pares, para el salto de longitud (y la gimnasia)
        C(t, new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 0.5f), Stone, 0f, "Poyo");
        for (int k = 0; k < 3; k++)
            for (int s = -1; s <= 1; s += 2)
                C(t, new Vector3(-0.4f + k * 0.4f + s * 0.08f, 0.66f, 0f), new Vector3(0.12f, 0.12f, 0.3f), k == 1 ? Lead : Stone, 0f, k == 1 ? "Halter_de_plomo" : "Halter_de_piedra");
    }

    static void LiftingStones(Transform t, St.Entry e)
    {
        string what = e.id == "sur_asia" || e.id == "india_moderna" ? "Nal (anilla de piedra)" : e.id == "alpino" ? "Piedra de Unspunnen" :
                      e.id == "china_tradicional" || e.id == "hongkong_tonglau" ? "Candado de piedra (shi suo)" : e.id == "egipto" ? "Saco de arena de levantar" : "Piedra de levantar";
        Color c = e.id == "egipto" ? Linen : Stone;
        for (int k = 0; k < 3; k++)
        {
            float s = 0.3f + k * 0.12f;
            C(t, new Vector3(-0.6f + k * 0.6f, s * 0.5f, 0f), new Vector3(s, s, s), c, k * 15f, what);
            if (e.id == "sur_asia" || e.id == "india_moderna" || e.id == "china_tradicional" || e.id == "hongkong_tonglau") C(t, new Vector3(-0.6f + k * 0.6f, s + 0.05f, 0f), new Vector3(0.05f, 0.12f, s * 0.6f), Stone, k * 15f, "Asa");
        }
    }

    static void Clubs(Transform t, St.Entry e, int year)
    {
        string what = e.id == "sur_asia" || e.id == "india_moderna" ? "Gada y mugdar" : e.id == "otomano" || e.id == "islam_abasi" ? "Mazas de los luchadores (mil)" : "Mazas indias";
        C(t, new Vector3(0f, 0.05f, 0f), new Vector3(1.4f, 0.1f, 0.5f), e.wood, 0f, "Base");
        for (int k = 0; k < 4; k++)
        {
            float h = k < 2 ? 0.9f : 0.6f;
            C(t, new Vector3(-0.5f + k * 0.33f, 0.1f + h * 0.35f, 0f), new Vector3(0.16f, h * 0.7f, 0.16f), Wood, 0f, what);
            C(t, new Vector3(-0.5f + k * 0.33f, 0.1f + h * 0.85f, 0f), new Vector3(0.05f, h * 0.3f, 0.05f), WoodDark, 0f, "Mango");
        }
        if (e.id == "sur_asia" || e.id == "india_moderna") C(t, new Vector3(0.6f, 0.55f, 0f), new Vector3(0.35f, 0.35f, 0.35f), Iron, 0f, "Cabeza_de_la_gada");
    }

    static void Barbell(Transform t)
    {
        C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.6f), Iron, 0f, "Soporte");
        C(t, new Vector3(0f, 1f, 0f), new Vector3(2f, 0.05f, 0.05f), Lead, 0f, "Barra");
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.85f, 1f, 0f), new Vector3(0.08f, 0.45f, 0.45f), Rubber, 0f, "Disco");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.4f, 0.1f, -0.5f), new Vector3(0.35f, 0.12f, 0.12f), Iron, 0f, "Mancuerna");
    }

    static void Bag(Transform t, St.Entry e)
    {   // el saco colgado de su bastidor: kórykos (cuero con arena, harina o higos), sha bao, saco de boxeo
        Color frame = e.id == "egeo" || e.id == "roma" ? Wood : e.wood;
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.6f, 1.2f, 0f), new Vector3(0.1f, 2.4f, 0.1f), frame, 0f, "Poste");
        C(t, new Vector3(0f, 2.4f, 0f), new Vector3(1.3f, 0.1f, 0.1f), frame, 0f, "Travesaño");
        C(t, new Vector3(0f, 2.15f, 0f), new Vector3(0.03f, 0.4f, 0.03f), Linen, 0f, "Cuerda");
        C(t, new Vector3(0f, 1.45f, 0f), new Vector3(0.45f, 1f, 0.45f), e.id == "hongkong_tonglau" || e.id == "china_tradicional" ? Linen : Leather, 0f, "Saco");
    }

    static void Makiwara(Transform t)
    {
        C(t, new Vector3(0f, 0.65f, 0f), new Vector3(0.12f, 1.3f, 0.08f), Wood, 0f, "Poste");
        C(t, new Vector3(0f, 1.1f, -0.06f), new Vector3(0.14f, 0.25f, 0.04f), Linen, 0f, "Paja_atada");
        C(t, new Vector3(0f, 0.05f, 0f), new Vector3(0.6f, 0.1f, 0.6f), Stone, 0f, "Base");
    }

    static void HighBar(Transform t, St.Entry e)
    {
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 1.1f, 1.25f, 0f), new Vector3(0.1f, 2.5f, 0.1f), e.wood, 0f, "Poste");
        C(t, new Vector3(0f, 2.4f, 0f), new Vector3(2.3f, 0.04f, 0.04f), Iron, 0f, "Barra");
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(2.2f, 0.06f, 1.6f), Sawdust, 0f, "Colchoneta_de_serrin");
    }

    static void ParallelBars(Transform t, St.Entry e)
    {
        for (int b = -1; b <= 1; b += 2)
        {
            for (int s = -1; s <= 1; s += 2) C(t, new Vector3(b * 0.25f, 0.75f, s * 1f), new Vector3(0.07f, 1.5f, 0.07f), e.wood, 0f, "Pie");
            C(t, new Vector3(b * 0.25f, 1.5f, 0f), new Vector3(0.06f, 0.06f, 2.6f), Wood, 0f, "Larguero");
        }
        C(t, new Vector3(0f, 0.04f, 0f), new Vector3(1.2f, 0.08f, 2.8f), e.wood, 0f, "Base");
    }

    static void VaultingHorse(Transform t, St.Entry e)
    {
        for (int sx = -1; sx <= 1; sx += 2) for (int sz = -1; sz <= 1; sz += 2) C(t, new Vector3(sx * 0.15f, 0.55f, sz * 0.6f), new Vector3(0.08f, 1.1f, 0.08f), e.wood, 0f, "Pata");
        C(t, new Vector3(0f, 1.15f, 0f), new Vector3(0.4f, 0.3f, 1.6f), Leather, 0f, "Cuerpo_de_cuero");
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(0f, 1.36f, s * 0.2f), new Vector3(0.06f, 0.12f, 0.25f), Wood, 0f, "Arzon");
        C(t, new Vector3(0f, 0.05f, -1.8f), new Vector3(0.9f, 0.1f, 0.5f), e.wood, 0f, "Trampolin");
    }

    static void Rings(Transform t, St.Entry e)
    {
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 1f, 1.5f, 0f), new Vector3(0.1f, 3f, 0.1f), e.wood, 0f, "Poste");
        C(t, new Vector3(0f, 3f, 0f), new Vector3(2.1f, 0.1f, 0.1f), e.wood, 0f, "Travesaño");
        for (int s = -1; s <= 1; s += 2)
        {
            C(t, new Vector3(s * 0.25f, 2.35f, 0f), new Vector3(0.03f, 1.3f, 0.03f), Linen, 0f, "Cuerda");
            C(t, new Vector3(s * 0.25f, 1.62f, 0f), new Vector3(0.2f, 0.2f, 0.04f), Wood, 0f, "Anilla");
        }
    }

    static void Target(Transform t, St.Entry e)
    {
        bool japan = e.id == "japon_edo_meiji";
        C(t, new Vector3(0f, 0.5f, 0.1f), new Vector3(0.08f, 1f, 0.08f), Wood, 0f, "Pie");
        C(t, new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0.12f), japan ? Chalk : new Color(0.8f, 0.72f, 0.5f), 0f, japan ? "Mato" : "Blanco");
        C(t, new Vector3(0f, 1f, -0.07f), new Vector3(0.45f, 0.45f, 0.02f), Rubber, 0f, "Anillo");
        C(t, new Vector3(0f, 1f, -0.08f), new Vector3(0.15f, 0.15f, 0.02f), new Color(0.75f, 0.15f, 0.12f), 0f, "Centro");
        C(t, new Vector3(0.9f, 0.6f, 0f), new Vector3(0.08f, 1.2f, 0.08f), Wood, 0f, "Arco_apoyado");
    }

    static void Ball(Transform t, St.Entry e, int year)
    {   // la pelota de su época, en su cesto: hule maya, camp-ball de cuero, balón de reglamento
        C(t, new Vector3(0f, 0.25f, 0f), new Vector3(0.8f, 0.5f, 0.8f), e.id == "maya_clasico" ? Linen : e.wood, 0f, "Cesto");
        Color ball = e.id == "maya_clasico" ? Rubber : year >= 1900 ? Leather : new Color(0.55f, 0.4f, 0.25f);
        for (int k = 0; k < 2; k++) C(t, new Vector3(-0.15f + k * 0.3f, 0.62f, 0f), new Vector3(0.24f, 0.24f, 0.24f), ball, k * 30f, "Pelota");
    }
}
