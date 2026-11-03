using System.Collections.Generic;
using UnityEngine;
using Fit = MicroBody.Fit;
using Plan = MicroBody.Plan;
using St = MicroStyles;

/// <summary>
/// LOS MUEBLES DETALLADOS DE LOS MICROCOSMOS (docs/microcosmos-reglas-edificacion.md, reglas 3 y 7; Berón 2026-11-01: «la cocina y
/// los muebles parecían cubos»; 2026-11-02: «las camas y demás servicios deben estar adaptados para los usuarios… con el estilo de
/// época/lugar»). Cada mueble es una unión de piezas que se reconoce por su forma y lleva sus DEPENDENCIAS (la cocina, su sartén,
/// su olla, su tabla con cuchillo, sus cucharones, cuencos y platos; la mesa, sus asientos; la cama, su colchón y su manta).
/// DOS COSAS deciden cómo es:
/// - el ESTILO DE ÉPOCA Y LUGAR (<see cref="MicroStyles"/>: el año del pueblo + su región): la cama es una estera, un lecho
///   de juncos, una cama egipcia con reposacabezas, una kliné, un charpai, un kang, un futón, un minder, una cama de dosel, de
///   hierro, una litera, una hamaca o un sofá-cama; y lo mismo el asiento, la mesa, el hogar, el retrete, el lavamanos, el
///   almacén y la luz, con los colores de sus materiales;
/// - el CUERPO de quien lo usa (<see cref="MicroBody.Fit"/>): una persona duerme de espaldas en una cama de 2 m y se sienta en
///   una silla; un insecto-humano (centauro: torso erguido, abdomen horizontal detrás) duerme DE VIENTRE en un lecho de su largo
///   y su ancho, con un apoyo para el pecho; se sienta A HORCAJADAS en un asiento que sostiene el abdomen; come a una mesa a la
///   altura de sus manos; su retrete tiene el agujero bajo la punta del abdomen. La mariquita y el pelotero (cúpula) duermen en
///   un nido con borde; la abeja, la luciérnaga y la cigarra, agarradas a una percha; la mantis, colgada de una barra; la araña,
///   en una hamaca de seda; el ciempiés, en un lecho larguísimo.
/// La raíz de cada mueble lleva <see cref="MicroPiece"/> (qué es, qué necesidad cubre, su estilo, para quién está hecho y cómo se
/// tumba) y un BoxCollider que envuelve sus piezas (<see cref="KitchenProps.Fit"/>): sobre ella va el puesto (AreaFunctionStation).
/// </summary>
internal static partial class MicroFurniture
{
    static Color H(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color c); return c; }
    static readonly Color Wood = KitchenProps.Wood, WoodDark = KitchenProps.WoodDark, Iron = H("3B3B3E"), Stone = H("8C857A"), Clay = H("B0714A"),
        Linen = H("EDE9DE"), Wool = H("8A5A44"), Pillow = H("F6F3EA"), Brass = H("C9A24A"), Water = H("6E9CC0"), Ash = H("4A4440"), Ember = H("E0702A"),
        Straw = H("D2B56E"), Paper = H("EFE8D6"), Green = H("4E7A3E"), Soil = H("5A4430"), Chalk = H("F0F0E8"), Slate = H("2F3A36"), Sack = H("C9B48C"),
        Brick = H("A0523A"), Reed = H("C9B07A"), Flame = H("FFD27A"), Silk = H("F2F0EA"), Enamel = H("E8EEF0"), Chrome = H("C0C4C8"), Copper = H("B87333");

    /// <summary>Las claves que <see cref="Build"/> sabe construir (cualquier otra sale como mesa con dos asientos).</summary>
    internal static readonly string[] Keys =
    {
        "fogon", "cocina", "horno", "barra", "mesa", "mesa_laboratorio", "cama", "camilla", "inodoro", "lavabo", "lavadero", "silla", "banco",
        "estante", "armario", "pizarra", "banco_taller", "torno", "tunel", "muneco", "altar", "puesto", "granero", "torno_alfarero", "parapeto",
        "pozo", "telar", "tatami", "esterilla", "bancal",
        // (2026-11-03) los edificios del pueblo y los frutales (MicroFurniture.Civic.cs)
        "frutal", "estatua", "fuente", "vitrina", "piscina", "pista", "consola", "cuna", "armero", "anden", "carro", "caballete", "material", "temazcal",
    };

    /// <summary>El cuerpo para el que se hace un mueble cuando no se dice (lo pone CivicBuilder: en un microcosmos, un
    /// insecto-humano centauro; en las escenas de ciclo de vida humano, una persona). null = una persona de 1,75 m.</summary>
    internal static Fit? DefaultBody;

    static Transform C(Transform t, Vector3 p, Vector3 s, Color c, float yaw = 0f, string n = "pieza") => KitchenProps.C(t, p, s, c, yaw, n);
    static Transform C(Transform t, Vector3 p, Vector3 s, Color c, Vector3 euler, string n) => KitchenProps.C(t, p, s, c, euler, n);

    /// <summary>
    /// Construye el mueble <paramref name="key"/> (las claves de <see cref="AreaFunctionStation.station"/> y las de las necesidades
    /// básicas: «inodoro», «lavabo», «silla», «banco») en <paramref name="pos"/> (local de <paramref name="parent"/>, la base en el
    /// suelo), girado <paramref name="yaw"/> (el frente de uso mira a −z local), en el ESTILO de <paramref name="year"/> y
    /// <paramref name="region"/> (<see cref="MicroStyles.For"/>) y para el CUERPO <paramref name="body"/> (o
    /// <see cref="DefaultBody"/>). Una mesa sienta a <paramref name="users"/> (un asiento a la medida de cada uno).
    /// <paramref name="size"/> solo lo usan los que se pisan (tatami, esterilla, bancal). Devuelve la raíz (con MicroPiece y su caja).
    /// </summary>
    internal static Transform Build(Transform parent, string key, Vector3 pos, float yaw = 0f, string label = null, int year = 1950, Vector2? size = null,
                                    string region = null, Fit? body = null, IList<Fit> users = null)
    {
        string name = string.IsNullOrEmpty(label) ? key : label;
        Transform t = KitchenProps.Root(parent, name, pos, yaw);
        Fill(t, key, year, size, region, body, users);
        return t;
    }

    /// <summary>REHACE el mueble <paramref name="t"/> (mismo sitio, nombre y puesto) para otro cuerpo: la cama de cada miembro de
    /// un hogar, la mesa con un asiento para cada uno, el retrete y el lavamanos de la familia (CivicBuilder.MakeHousehold).</summary>
    internal static void Refit(Transform t, string key, int year, string region, Fit body, IList<Fit> users = null)
    {
        for (int i = t.childCount - 1; i >= 0; i--) Object.DestroyImmediate(t.GetChild(i).gameObject);
        Fill(t, key, year, null, region, body, users);
    }

    static void Fill(Transform t, string key, int year, Vector2? size, string region, Fit? body, IList<Fit> users)
    {
        St.Entry e = St.For(year, string.IsNullOrEmpty(region) ? St.Parse(null, year).ToString() : region);
        Fit f = body ?? DefaultBody ?? MicroBody.Human();
        string need = "", kind = key, rest = ""; bool walk = false, fitted = false;
        int seed = t.name.GetHashCode();
        switch (key)
        {
            case "fogon": case "cocina": Kitchen(t, e, year); need = "cocinar"; kind = "cocina"; break;
            case "horno": Oven(t); need = "cocinar"; break;
            case "barra": Bar(t); need = "comer"; break;
            case "mesa": TableWithSeats(t, e, f, users, 4); need = "comer"; fitted = true; break;
            case "mesa_laboratorio": LabBench(t); break;
            case "cama": Bed(t, e, f); need = "dormir"; rest = f.Rest; fitted = true; break;
            case "camilla": Stretcher(t, e); need = "dormir"; rest = MicroBody.Espalda; break;
            case "inodoro": Toilet(t, e, f); need = "aseo"; fitted = true; break;
            case "lavabo": Washbasin(t, e, f); need = "lavarse"; fitted = true; break;
            case "lavadero": WashTub(t); need = "lavarse"; break;
            case "silla": SeatOn(t, e, f, false); need = "sentarse"; fitted = true; break;
            case "banco": SeatOn(t, e, f, true); need = "sentarse"; fitted = true; break;
            case "armario": Storage(t, e, seed); break;
            case "estante": Shelf(t, seed); break;
            case "pizarra": Blackboard(t); break;
            case "banco_taller": Workbench(t, year); break;
            case "torno": Lathe(t); break;
            case "tunel": WindTunnel(t); break;
            case "muneco": WoodenDummy(t); break;
            case "altar": Altar(t); break;
            case "puesto": Stall(t, seed); break;
            case "granero": Granary(t); need = "cocinar"; break;
            case "torno_alfarero": PotterWheel(t); break;
            case "parapeto": Sandbags(t); break;
            case "pozo": Well(t); need = "lavarse"; break;
            case "telar": Loom(t); break;
            case "tatami": case "esterilla": case "bancal": Mat(t, key, size ?? new Vector2(key == "tatami" ? 6f : key == "bancal" ? 2.5f : 1f, key == "tatami" ? 5f : 2f)); walk = true; break;
            default:
                if (FillCivic(t, key, e, year, seed, ref need, ref walk)) break;
                TableWithSeats(t, e, f, users, 2); need = "comer"; kind = "mesa"; fitted = true; break;
        }
        if (!t.TryGetComponent(out MicroPiece mp)) mp = t.gameObject.AddComponent<MicroPiece>();
        mp.kind = kind; mp.need = need; mp.walkable = walk; mp.style = e.id; mp.rest = rest;
        mp.fitFor = fitted ? (f.plan == Plan.Humano ? "" : $"{f.plan} ({f.species})") : "";
        mp.fitted = fitted;
        KitchenProps.Fit(t, !walk);
    }

    /// <summary>El nombre de un mueble de necesidad básica en su estilo y para su cuerpo («Charpai», «Kliné», «Kang», «Nido de
    /// cúpula», «Percha de dormir», «Letrina a la turca»…).</summary>
    internal static string NameOf(string key, St.Entry e, Fit? body = null)
    {
        Fit f = body ?? DefaultBody ?? MicroBody.Human();
        switch (key)
        {
            case "cama":
                if (f.plan == Plan.Arana) return "Hamaca de seda";
                if (f.plan == Plan.Alado) return "Percha de dormir";
                if (f.plan == Plan.Mantis) return "Barra de colgarse";
                string b = BedName(e.bed);
                return f.plan == Plan.Humano ? b : f.plan == Plan.Cupula ? $"{b} (nido de cúpula)" : $"{b} (de vientre)";
            case "inodoro": return f.Insect ? $"{ToiletName(e.toilet)} (de abdomen)" : ToiletName(e.toilet);
            case "lavabo": return BasinName(e.basin);
            case "mesa": return e.FloorSeating ? "Mesa baja con cojines" : "Mesa con asientos";
            case "fogon": return HearthName(e.hearth);
            default: return key;
        }
    }

    static string BedName(St.Bed b) => b switch
    {
        St.Bed.Estera => "Estera de dormir", St.Bed.LechoJuncos => "Lecho de juncos", St.Bed.Reposacabezas => "Cama con reposacabezas", St.Bed.Kline => "Kliné",
        St.Bed.Lectus => "Lectus", St.Bed.Charpai => "Charpai", St.Bed.Kang => "Kang", St.Bed.Futon => "Futón", St.Bed.Minder => "Minder", St.Bed.Jergon => "Jergón",
        St.Bed.Dosel => "Cama de dosel", St.Bed.Madera => "Cama de madera", St.Bed.Hierro => "Cama de hierro", St.Bed.Litera => "Litera", St.Bed.Hamaca => "Chinchorro",
        St.Bed.Moderna => "Cama", _ => "Sofá-cama",
    };
    static string ToiletName(St.Toilet t) => t switch
    {
        St.Toilet.Pozo => "Pozo negro", St.Toilet.Vasija => "Letrina sobre vasija", St.Toilet.Comunal => "Letrina comunal", St.Toilet.Bacinilla => "Silla-orinal",
        St.Toilet.Turca => "Letrina a la turca", St.Toilet.Caseta => "Letrina del patio", St.Toilet.CisternaAlta => "Retrete de cisterna alta",
        St.Toilet.CisternaBaja => "Inodoro", _ => "Cubo de letrina",
    };
    static string BasinName(St.Basin b) => b switch
    {
        St.Basin.Pila => "Pila de piedra", St.Basin.Cantaro => "Cántaro y jofaina", St.Basin.Louterion => "Louterion", St.Basin.Aguamanil => "Aguamanil",
        St.Basin.Ibrik => "Ibrik y jofaina", St.Basin.Soporte => "Palangana en su soporte", St.Basin.Tarai => "Tarai", St.Basin.Tinajero => "Tinajero", _ => "Lavabo",
    };
    static string HearthName(St.Hearth h) => h switch
    {
        St.Hearth.Suelo => "Hogar", St.Hearth.Tannur => "Tannur", St.Hearth.Chulha => "Chulha", St.Hearth.Brasero => "Brasero", St.Hearth.Caldero => "Hogar con caldero",
        St.Hearth.Ocak => "Ocak", St.Hearth.KangWok => "Cocina del kang", St.Hearth.Irori => "Irori", St.Hearth.Kachelofen => "Estufa de azulejos",
        St.Hearth.Topias => "Fogón de topias", St.Hearth.Hierro => "Cocina de hierro", St.Hearth.GasWok => "Fogón con wok", St.Hearth.Gas => "Cocina de gas", _ => "Cocina eléctrica",
    };

    /// <summary>La huella del mueble (en sus locales), medida por sus piezas.</summary>
    internal static Bounds LocalBounds(Transform piece)
    {
        Bounds b = new Bounds(Vector3.zero, Vector3.zero); bool any = false;
        foreach (Renderer r in piece.GetComponentsInChildren<Renderer>(true))
        {
            Bounds rb = r.bounds;
            Vector3 mn = piece.InverseTransformPoint(rb.min), mx = piece.InverseTransformPoint(rb.max);
            Bounds lb = new Bounds((mn + mx) * 0.5f, new Vector3(Mathf.Abs(mx.x - mn.x), Mathf.Abs(mx.y - mn.y), Mathf.Abs(mx.z - mn.z)));
            if (!any) { b = lb; any = true; } else b.Encapsulate(lb);
        }
        return b;
    }

    static float Hands(Fit f) => f.Insect ? Mathf.Clamp(f.hands, 0.75f, 1.1f) : 0.8f;

    // ══ LA CAMA ═════════════════════════════════════════════════════════════════════════════════════════════════════
    // Para una persona: la del estilo, de 2 × 1 m, de espaldas con su almohada. Para un insecto-humano: la del estilo a su
    // largo y su ancho (tumbado de vientre, las patas recogidas), sin almohada: un APOYO PARA EL PECHO en la cabecera (+z) y,
    // en la cúpula, un BORDE de nido; la percha, la barra de colgarse y la hamaca de seda para los que no duermen tumbados.
    static void Bed(Transform t, St.Entry e, Fit f)
    {
        switch (f.plan)
        {
            case Plan.Arana: SilkHammock(t, e, f); return;
            case Plan.Alado: case Plan.Mantis: Perch(t, e, f); return;
        }
        bool insect = f.Insect;
        float L = insect ? Mathf.Max(1.4f, f.length + 0.15f) : 2.0f, W = insect ? Mathf.Max(0.9f, f.span + 0.15f) : 1.0f;
        if (f.plan == Plan.Cupula) W = Mathf.Max(W, f.width + 0.5f);
        float top = BedFrame(t, e, L, W);
        if (insect)
        {
            // El apoyo del pecho (el torso descansa sobre él) y la manta sobre el abdomen; en la cúpula, el borde del nido.
            C(t, new Vector3(0f, top + 0.07f, L * 0.5f - 0.25f), new Vector3(W * 0.55f, 0.14f, 0.32f), e.textile, 0f, "Apoyo_del_pecho");
            C(t, new Vector3(0f, top + 0.02f, -L * 0.15f), new Vector3(W * 0.92f, 0.03f, L * 0.55f), Color.Lerp(e.textile, Wool, 0.4f), 0f, "Manta");
            if (f.plan == Plan.Cupula)
                for (int s = -1; s <= 1; s += 2)
                {
                    C(t, new Vector3(s * (W * 0.5f - 0.08f), top + 0.1f, 0f), new Vector3(0.16f, 0.2f, L * 0.85f), e.textile, 0f, "Borde_del_nido");
                    C(t, new Vector3(0f, top + 0.08f, s * (L * 0.5f - 0.08f) - (s > 0 ? 0.3f : 0f)), new Vector3(W * 0.7f, 0.16f, 0.14f), e.textile, 0f, "Borde_del_nido");
                }
        }
        else if (e.bed != St.Bed.Kang && e.bed != St.Bed.Hamaca)
        {
            C(t, new Vector3(0f, top + 0.05f, L * 0.5f - 0.25f), new Vector3(W * 0.6f, 0.1f, 0.32f), e.bed == St.Bed.Reposacabezas ? e.wood : Pillow, 0f, e.bed == St.Bed.Reposacabezas ? "Reposacabezas" : "Almohada");
            C(t, new Vector3(0f, top + 0.02f, -0.15f), new Vector3(W * 0.95f, 0.03f, L * 0.6f), Color.Lerp(e.textile, Wool, 0.3f), 0f, "Manta");
        }
        Lamp(t, e, new Vector3(W * 0.5f + 0.3f, 0f, L * 0.5f - 0.3f), true);
    }

    /// <summary>El armazón de la cama del estilo, de <paramref name="L"/> × <paramref name="W"/> (la cabecera en +z). Devuelve la
    /// altura de lo que se pisa tumbado (el colchón).</summary>
    static float BedFrame(Transform t, St.Entry e, float L, float W)
    {
        Color w = e.wood, tx = e.textile;
        void Legs(float h, float thick, Color c, string n = "Pata") { foreach (var (x, z) in Corners) C(t, new Vector3(x * (W * 0.5f - thick), h * 0.5f, z * (L * 0.5f - thick)), new Vector3(thick * 1.6f, h, thick * 1.6f), c, 0f, n); }
        void Mattress(float y, float h, Color c) => C(t, new Vector3(0f, y + h * 0.5f, 0f), new Vector3(W - 0.06f, h, L - 0.06f), c, 0f, "Colchon");
        switch (e.bed)
        {
            case St.Bed.Estera:
                C(t, new Vector3(0f, 0.04f, 0f), new Vector3(W, 0.08f, L), Straw, 0f, "Estera");
                C(t, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(W * 0.8f, 0.04f, L * 0.7f), H("7A5A3A"), 8f, "Piel");
                return 0.12f;
            case St.Bed.LechoJuncos:
                Legs(0.32f, 0.04f, w);
                for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * (W * 0.5f - 0.04f), 0.3f, 0f), new Vector3(0.06f, 0.06f, L), w, 0f, "Larguero");
                for (int k = 0; k < Mathf.RoundToInt(L / 0.12f); k++) C(t, new Vector3(0f, 0.32f, -L * 0.5f + 0.06f + k * 0.12f), new Vector3(W - 0.08f, 0.02f, 0.05f), Reed, 0f, "Cuerda_de_junco");
                return 0.34f;
            case St.Bed.Reposacabezas:   // egipcia: inclinada (la cabecera más alta), patas de león y TABLA DE LOS PIES (no cabecero)
                foreach (var (x, z) in Corners)
                {
                    float h = z > 0 ? 0.42f : 0.32f;
                    C(t, new Vector3(x * (W * 0.5f - 0.05f), h * 0.5f, z * (L * 0.5f - 0.05f)), new Vector3(0.07f, h, 0.07f), w, 0f, "Pata_de_leon");
                    C(t, new Vector3(x * (W * 0.5f - 0.05f), 0.03f, z * (L * 0.5f - 0.05f) + 0.04f), new Vector3(0.09f, 0.06f, 0.12f), w, 0f, "Garra");
                }
                C(t, new Vector3(0f, 0.38f, 0f), new Vector3(W, 0.05f, L), w, new Vector3(-2.9f, 0f, 0f), "Bastidor_inclinado");
                C(t, new Vector3(0f, 0.43f, 0f), new Vector3(W - 0.08f, 0.05f, L - 0.08f), Linen, new Vector3(-2.9f, 0f, 0f), "Colchon_de_lino");
                C(t, new Vector3(0f, 0.55f, -L * 0.5f + 0.02f), new Vector3(W, 0.4f, 0.04f), w, 0f, "Tabla_de_los_pies");
                C(t, new Vector3(0f, 0.56f, -L * 0.5f + 0.0f), new Vector3(W * 0.6f, 0.2f, 0.05f), Brass, 0f, "Bes_tallado");
                return 0.47f;
            case St.Bed.Kline: case St.Bed.Lectus:
                Legs(0.55f, 0.04f, w, "Pata_torneada");
                C(t, new Vector3(0f, 0.55f, 0f), new Vector3(W, 0.06f, L), w, 0f, "Bastidor");
                Mattress(0.58f, 0.12f, tx);
                C(t, new Vector3(0f, 0.85f, L * 0.5f - 0.06f), new Vector3(W, 0.2f, 0.12f), w, new Vector3(-30f, 0f, 0f), "Fulcro");
                if (e.bed == St.Bed.Lectus) C(t, new Vector3(-W * 0.5f + 0.03f, 0.85f, 0f), new Vector3(0.05f, 0.4f, L), w, 0f, "Respaldo_lateral");
                C(t, new Vector3(W * 0.5f + 0.3f, 0.1f, -L * 0.25f), new Vector3(0.45f, 0.2f, 0.35f), w, 0f, "Escabel");
                return 0.7f;
            case St.Bed.Charpai:
                foreach (var (x, z) in Corners) C(t, new Vector3(x * (W * 0.5f - 0.05f), 0.22f, z * (L * 0.5f - 0.05f)), new Vector3(0.1f, 0.44f, 0.1f), w, 45f, "Pata_torneada");
                for (int s = -1; s <= 1; s += 2) { C(t, new Vector3(s * (W * 0.5f - 0.05f), 0.4f, 0f), new Vector3(0.07f, 0.07f, L), w, 0f, "Larguero"); C(t, new Vector3(0f, 0.4f, s * (L * 0.5f - 0.05f)), new Vector3(W, 0.07f, 0.07f), w, 0f, "Travesano"); }
                for (int k = 0; k < Mathf.RoundToInt(W / 0.1f); k++) C(t, new Vector3(-W * 0.5f + 0.08f + k * 0.1f, 0.42f, 0f), new Vector3(0.015f, 0.015f, L - 0.1f), tx, 0f, "Cuerda");
                for (int k = 0; k < Mathf.RoundToInt(L / 0.1f); k++) C(t, new Vector3(0f, 0.425f, -L * 0.5f + 0.08f + k * 0.1f), new Vector3(W - 0.1f, 0.015f, 0.015f), tx, 0f, "Cuerda");
                C(t, new Vector3(0f, 0.47f, -L * 0.35f), new Vector3(W * 0.7f, 0.08f, 0.4f), H("C8763E"), 0f, "Rajai_doblada");
                return 0.44f;
            case St.Bed.Kang:   // plataforma de ladrillo calentada por la boca de la cocina, con su estera, su colcha y la mesita
                C(t, new Vector3(0f, 0.3f, 0f), new Vector3(W + 0.6f, 0.6f, L + 0.2f), Brick, 0f, "Kang");
                C(t, new Vector3(0f, 0.25f, -L * 0.5f - 0.11f), new Vector3(0.4f, 0.3f, 0.02f), Ash, 0f, "Boca_del_kang");
                C(t, new Vector3(0f, 0.62f, 0f), new Vector3(W + 0.5f, 0.03f, L + 0.1f), Reed, 0f, "Estera");
                C(t, new Vector3(0f, 0.68f, -0.1f), new Vector3(W * 0.9f, 0.08f, L * 0.6f), tx, 0f, "Colcha");
                C(t, new Vector3(0f, 0.68f, L * 0.5f - 0.2f), new Vector3(W * 0.5f, 0.1f, 0.2f), H("C9A050"), 0f, "Almohada_dura");
                return 0.64f;
            case St.Bed.Futon:
                C(t, new Vector3(0f, 0.025f, 0f), new Vector3(W + 0.4f, 0.05f, L + 0.2f), H("C9BC86"), 0f, "Tatami");
                C(t, new Vector3(0f, 0.1f, 0f), new Vector3(W, 0.1f, L), Linen, 0f, "Shikibuton");
                C(t, new Vector3(0f, 0.17f, -0.15f), new Vector3(W * 0.98f, 0.05f, L * 0.65f), tx, 0f, "Kakebuton");
                return 0.15f;
            case St.Bed.Minder:   // sedir: estrado bajo con su colchón (minder), cojines contra el respaldo y la colcha (yorgan)
                C(t, new Vector3(0f, 0.15f, 0f), new Vector3(W + 0.1f, 0.3f, L + 0.1f), w, 0f, "Sedir");
                Mattress(0.3f, 0.12f, tx);
                C(t, new Vector3(-W * 0.5f + 0.1f, 0.55f, 0f), new Vector3(0.18f, 0.32f, L * 0.9f), Color.Lerp(tx, Brass, 0.3f), 0f, "Cojin_del_respaldo");
                C(t, new Vector3(0f, 0.45f, -0.1f), new Vector3(W * 0.85f, 0.04f, L * 0.55f), Color.Lerp(tx, Linen, 0.5f), 0f, "Yorgan");
                return 0.42f;
            case St.Bed.Jergon:
                Legs(0.15f, 0.05f, w);
                C(t, new Vector3(0f, 0.15f, 0f), new Vector3(W, 0.05f, L), w, 0f, "Tablas");
                Mattress(0.17f, 0.14f, Straw);
                return 0.31f;
            case St.Bed.Dosel:   // cuatro postes con su larguero y las cortinas recogidas, colchón de pluma y colcha
                foreach (var (x, z) in Corners) C(t, new Vector3(x * (W * 0.5f - 0.04f), 1.05f, z * (L * 0.5f - 0.04f)), new Vector3(0.08f, 2.1f, 0.08f), w, 0f, "Poste_del_dosel");
                for (int s = -1; s <= 1; s += 2) { C(t, new Vector3(s * (W * 0.5f - 0.04f), 2.08f, 0f), new Vector3(0.06f, 0.08f, L), w, 0f, "Larguero_del_dosel"); C(t, new Vector3(0f, 2.08f, s * (L * 0.5f - 0.04f)), new Vector3(W, 0.08f, 0.06f), w, 0f, "Larguero_del_dosel"); }
                foreach (var (x, z) in Corners) C(t, new Vector3(x * (W * 0.5f - 0.1f), 1.3f, z * (L * 0.5f - 0.1f)), new Vector3(0.14f, 1.5f, 0.14f), tx, 0f, "Cortina_recogida");
                C(t, new Vector3(0f, 0.35f, 0f), new Vector3(W, 0.08f, L), w, 0f, "Bastidor");
                Mattress(0.39f, 0.2f, Linen);
                C(t, new Vector3(0f, 0.75f, L * 0.5f - 0.04f), new Vector3(W, 0.7f, 0.05f), w, 0f, "Cabecero");
                return 0.59f;
            case St.Bed.Madera:
                Legs(0.3f, 0.04f, w);
                C(t, new Vector3(0f, 0.3f, 0f), new Vector3(W, 0.08f, L), w, 0f, "Bastidor");
                Mattress(0.34f, 0.22f, Linen);
                C(t, new Vector3(0f, 0.7f, L * 0.5f - 0.03f), new Vector3(W + 0.06f, 1.0f, 0.06f), w, 0f, "Cabecero");
                C(t, new Vector3(0f, 0.45f, -L * 0.5f + 0.03f), new Vector3(W + 0.06f, 0.55f, 0.06f), w, 0f, "Piecero");
                C(t, new Vector3(0f, 0.6f, 0.2f), new Vector3(W * 0.9f, 0.14f, L * 0.45f), tx, 0f, "Edredon");
                return 0.56f;
            case St.Bed.Hierro:
                foreach (var (x, z) in Corners) C(t, new Vector3(x * (W * 0.5f - 0.03f), z > 0 ? 0.6f : 0.45f, z * (L * 0.5f - 0.03f)), new Vector3(0.04f, z > 0 ? 1.2f : 0.9f, 0.04f), Iron, 0f, "Poste");
                foreach (var (x, z) in Corners) C(t, new Vector3(x * (W * 0.5f - 0.03f), z > 0 ? 1.22f : 0.92f, z * (L * 0.5f - 0.03f)), new Vector3(0.07f, 0.07f, 0.07f), Brass, 0f, "Pomo");
                for (int s = -1; s <= 1; s += 2)
                {
                    float zz = s * (L * 0.5f - 0.03f), hh = s > 0 ? 1.1f : 0.8f;
                    C(t, new Vector3(0f, hh, zz), new Vector3(W, 0.03f, 0.03f), Iron, 0f, s > 0 ? "Cabecero" : "Piecero");
                    for (int k = 1; k < 6; k++) C(t, new Vector3(-W * 0.5f + k * W / 6f, (hh + 0.35f) * 0.5f, zz), new Vector3(0.02f, hh - 0.35f, 0.02f), Iron, 0f, "Barrote");
                }
                C(t, new Vector3(0f, 0.35f, 0f), new Vector3(W, 0.05f, L), Iron, 0f, "Somier");
                Mattress(0.38f, 0.16f, Linen);
                C(t, new Vector3(0f, 0.56f, -0.15f), new Vector3(W * 0.98f, 0.04f, L * 0.6f), tx, 0f, "Colcha");
                return 0.54f;
            case St.Bed.Litera:
                foreach (var (x, z) in Corners) C(t, new Vector3(x * (W * 0.5f - 0.03f), 0.85f, z * (L * 0.5f - 0.03f)), new Vector3(0.06f, 1.7f, 0.06f), w, 0f, "Poste");
                foreach (float y in new[] { 0.3f, 1.3f })
                {
                    C(t, new Vector3(0f, y, 0f), new Vector3(W, 0.05f, L), e.metal, 0f, "Somier");
                    C(t, new Vector3(0f, y + 0.08f, 0f), new Vector3(W - 0.08f, 0.1f, L - 0.08f), Linen, 0f, "Colchoneta");
                    C(t, new Vector3(0f, y + 0.15f, -0.2f), new Vector3(W - 0.1f, 0.03f, L * 0.55f), tx, 0f, "Manta");
                }
                for (int k = 0; k < 4; k++) C(t, new Vector3(W * 0.5f + 0.02f, 0.4f + k * 0.3f, -L * 0.5f + 0.2f), new Vector3(0.04f, 0.03f, 0.3f), w, 0f, "Escalon");
                return 0.43f;
            case St.Bed.Hamaca:   // chinchorro colgado de dos horcones, con sus cabuyeras
                for (int s = -1; s <= 1; s += 2)
                {
                    C(t, new Vector3(0f, 0.9f, s * (L * 0.5f + 0.35f)), new Vector3(0.14f, 1.8f, 0.14f), w, 0f, "Horcon");
                    C(t, new Vector3(0f, 1.25f, s * (L * 0.5f + 0.15f)), new Vector3(0.02f, 0.02f, 0.45f), Straw, new Vector3(s * 35f, 0f, 0f), "Cabuyera");
                }
                for (int k = 0; k < 5; k++)
                {
                    float u = (k - 2) / 2f, z = u * L * 0.42f, y = 0.55f + 0.35f * u * u;
                    C(t, new Vector3(0f, y, z), new Vector3(W, 0.03f, L * 0.22f), k % 2 == 0 ? tx : Color.Lerp(tx, H("8E2A26"), 0.5f), new Vector3(-Mathf.Atan(0.7f * u / (L * 0.42f)) * Mathf.Rad2Deg, 0f, 0f), "Tejido_del_chinchorro");
                }
                return 0.58f;
            case St.Bed.Moderna:
                C(t, new Vector3(0f, 0.18f, 0f), new Vector3(W, 0.36f, L), w, 0f, "Canape");
                Mattress(0.36f, 0.2f, Linen);
                C(t, new Vector3(0f, 0.75f, L * 0.5f - 0.04f), new Vector3(W + 0.1f, 0.8f, 0.08f), tx, 0f, "Cabecero_tapizado");
                C(t, new Vector3(0f, 0.58f, -0.2f), new Vector3(W * 0.98f, 0.04f, L * 0.6f), Color.Lerp(tx, Linen, 0.4f), 0f, "Colcha");
                C(t, new Vector3(W * 0.5f + 0.3f, 0.25f, L * 0.5f - 0.25f), new Vector3(0.45f, 0.5f, 0.4f), w, 0f, "Mesilla");
                return 0.56f;
            default:   // sofá-cama soviético abierto: base, asiento, respaldo y brazos
                C(t, new Vector3(0f, 0.2f, 0f), new Vector3(W, 0.4f, L), w, 0f, "Base_del_divan");
                Mattress(0.4f, 0.12f, tx);
                C(t, new Vector3(-W * 0.5f - 0.1f, 0.55f, 0f), new Vector3(0.2f, 0.7f, L), tx, 0f, "Respaldo");
                for (int s = -1; s <= 1; s += 2) C(t, new Vector3(-0.05f, 0.45f, s * (L * 0.5f + 0.08f)), new Vector3(W + 0.2f, 0.5f, 0.16f), w, 0f, "Brazo");
                return 0.52f;
        }
    }

    static readonly (int x, int z)[] Corners = { (-1, -1), (1, -1), (-1, 1), (1, 1) };

    // PERCHA DE DORMIR (abeja, luciérnaga, cigarra: duermen agarradas, las abejas colgadas de las mandíbulas) y BARRA DE
    // COLGARSE (mantis): dos postes del estilo con la barra acolchada a la altura de agarre, sobre una estera blanda.
    static void Perch(Transform t, St.Entry e, Fit f)
    {
        bool mantis = f.plan == Plan.Mantis;
        float W = Mathf.Max(1.2f, f.span + 0.4f), D = Mathf.Max(0.9f, f.length * 0.6f), bar = mantis ? f.height + 0.15f : Mathf.Clamp(f.hands + 0.35f, 1.1f, 1.7f);
        C(t, new Vector3(0f, 0.04f, 0f), new Vector3(W, 0.08f, D), e.bed == St.Bed.Futon || e.bed == St.Bed.Estera ? Straw : Color.Lerp(e.textile, Linen, 0.4f), 0f, "Estera_blanda");
        for (int s = -1; s <= 1; s += 2)
        {
            C(t, new Vector3(s * (W * 0.5f - 0.05f), bar * 0.5f, 0f), new Vector3(0.1f, bar, 0.1f), e.wood, 0f, "Poste_de_la_percha");
            C(t, new Vector3(s * (W * 0.5f - 0.05f), 0.04f, 0f), new Vector3(0.3f, 0.08f, 0.4f), e.wood, 0f, "Pie");
        }
        C(t, new Vector3(0f, bar, 0f), new Vector3(W, 0.08f, 0.08f), e.wood, 0f, mantis ? "Barra_de_colgarse" : "Barra_de_la_percha");
        C(t, new Vector3(0f, bar, 0f), new Vector3(W * 0.7f, 0.12f, 0.12f), e.textile, 0f, "Funda_de_agarre");
        if (!mantis) C(t, new Vector3(0f, bar - 0.25f, 0.02f), new Vector3(W * 0.6f, 0.04f, 0.04f), e.wood, 0f, "Barra_de_las_patas");
        Lamp(t, e, new Vector3(W * 0.5f + 0.3f, 0f, 0f), true);
    }

    // HAMACA DE SEDA de la araña (descansa en su red, de vientre, con las ocho patas abiertas): cuatro postes y la red tensa
    // con sus hilos radiales y en espiral, en el color de los tejidos del estilo.
    static void SilkHammock(Transform t, St.Entry e, Fit f)
    {
        float W = Mathf.Max(1.4f, f.span + 0.3f), L = Mathf.Max(1.6f, f.length + 0.3f), y = 0.55f;
        foreach (var (x, z) in Corners) C(t, new Vector3(x * W * 0.5f, 0.45f, z * L * 0.5f), new Vector3(0.1f, 0.9f, 0.1f), e.wood, 0f, "Poste");
        Color silk = Color.Lerp(Silk, e.textile, 0.25f);
        C(t, new Vector3(0f, y - 0.06f, 0f), new Vector3(W * 0.9f, 0.02f, L * 0.9f), silk, 0f, "Red_de_seda");
        for (int k = 0; k < 4; k++) C(t, new Vector3(0f, y, 0f), new Vector3(0.015f, 0.015f, Mathf.Sqrt(W * W + L * L) * 0.98f), Silk, k * 45f + Mathf.Atan2(W, L) * Mathf.Rad2Deg * (k % 2 == 0 ? 1 : 0), "Hilo_radial");
        for (int k = 1; k <= 3; k++)
            for (int s = 0; s < 4; s++) C(t, Quaternion.Euler(0f, s * 90f, 0f) * new Vector3(0f, y - 0.02f, k * 0.15f * Mathf.Min(W, L)), new Vector3(k * 0.32f * Mathf.Min(W, L), 0.012f, 0.012f), Silk, s * 90f, "Hilo_espiral");
        foreach (var (x, z) in Corners) C(t, new Vector3(x * W * 0.45f, y + 0.12f, z * L * 0.45f), new Vector3(0.012f, 0.3f, 0.012f), Silk, new Vector3(z * 25f, 0f, -x * 25f), "Amarre");
        Lamp(t, e, new Vector3(W * 0.5f + 0.35f, 0f, L * 0.5f - 0.2f), true);
    }

    // La camilla de curas (hospital): la del estilo de hierro o de madera, a la medida de una persona tumbada.
    static void Stretcher(Transform t, St.Entry e)
    {
        Color frame = e.from >= 1850 ? Iron : e.wood;
        foreach (var (x, z) in Corners) C(t, new Vector3(x * 0.33f, 0.35f, z * 0.96f), new Vector3(0.05f, 0.7f, 0.05f), frame, 0f, "Pata");
        C(t, new Vector3(0f, 0.7f, 0f), new Vector3(0.75f, 0.06f, 2f), frame, 0f, "Bastidor");
        C(t, new Vector3(0f, 0.78f, 0f), new Vector3(0.7f, 0.1f, 1.95f), Linen, 0f, "Colchon");
        C(t, new Vector3(0f, 0.85f, 0.75f), new Vector3(0.45f, 0.07f, 0.3f), Pillow, 0f, "Almohada");
        C(t, new Vector3(0f, 0.84f, -0.3f), new Vector3(0.72f, 0.02f, 1.1f), Linen, 0f, "Sabana");
    }

    // ══ LOS ASIENTOS ════════════════════════════════════════════════════════════════════════════════════════════════
    /// <summary>Un asiento (su raíz «Silla» o «Banco» con MicroPiece) en <paramref name="at"/>, mirando a +z local (a la mesa).
    /// Para una persona, el del estilo; para un insecto-humano, uno que sostiene su ABDOMEN (que queda detrás, hacia −z) a la
    /// altura de su vientre, sin respaldo: a horcajadas (centauro), bajo y ancho con barandillas (cúpula), un anillo (araña), una
    /// barra de agarre (alados y mantis) o un banco corrido (ciempiés).</summary>
    static Transform Seat(Transform parent, Vector3 at, float yaw, St.Entry e, Fit f, bool bench)
    {
        Transform s = KitchenProps.Root(parent, bench ? "Banco" : "Silla", at, yaw);
        SeatOn(s, e, f, bench);
        var mp = s.gameObject.AddComponent<MicroPiece>();
        mp.kind = bench ? "banco" : "silla"; mp.need = "sentarse"; mp.style = e.id; mp.fitFor = f.Insect ? $"{f.plan} ({f.species})" : "";
        return s;
    }

    /// <summary>Las piezas del asiento dentro de <paramref name="s"/> (su raíz).</summary>
    static void SeatOn(Transform s, St.Entry e, Fit f, bool bench)
    {
        Color w = e.wood, tx = e.textile;
        if (f.Insect)
        {
            float y = Mathf.Clamp(f.belly + 0.08f, 0.15f, 0.8f), len = Mathf.Clamp(f.abdomen + 0.1f, 0.4f, 1.6f), wid = Mathf.Clamp(f.width * 0.75f, 0.3f, 1.2f);
            float zc = -len * 0.5f + 0.15f;   // el abdomen va detrás del torso: el asiento se alarga hacia −z
            switch (f.plan)
            {
                case Plan.Alado: case Plan.Mantis:
                {
                    float bar = Mathf.Clamp(f.belly + 0.3f, 0.45f, 0.9f), half = Mathf.Max(0.3f, wid * 0.6f);
                    for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * half, bar * 0.5f, -0.1f), new Vector3(0.06f, bar, 0.06f), w, 0f, "Pata");
                    C(s, new Vector3(0f, bar, -0.1f), new Vector3(half * 2f + 0.1f, 0.07f, 0.07f), w, 0f, "Asiento");   // la barra de agarre
                    C(s, new Vector3(0f, bar, -0.1f), new Vector3(half * 1.2f, 0.1f, 0.1f), tx, 0f, "Funda");
                    C(s, new Vector3(0f, 0.12f, -0.1f), new Vector3(half * 2f, 0.04f, 0.04f), w, 0f, "Reposapies");
                    break;
                }
                case Plan.Arana:
                    for (int k = 0; k < 4; k++) C(s, Quaternion.Euler(0f, k * 90f, 0f) * new Vector3(0f, y * 0.5f, len * 0.35f) + new Vector3(0f, 0f, zc), new Vector3(len * 0.7f, y, 0.16f), k % 2 == 0 ? tx : w, k * 90f, k == 0 ? "Asiento" : "Anillo");
                    break;
                case Plan.Cupula:
                    y = Mathf.Clamp(f.belly + 0.04f, 0.12f, 0.5f); wid = Mathf.Max(wid, f.width * 0.9f);
                    foreach (var (x, z) in Corners) C(s, new Vector3(x * wid * 0.45f, y * 0.5f, zc + z * len * 0.4f), new Vector3(0.06f, y, 0.06f), w, 0f, "Pata");
                    C(s, new Vector3(0f, y, zc), new Vector3(wid, 0.08f, len), tx, 0f, "Asiento");
                    for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * wid * 0.5f, y + 0.12f, zc), new Vector3(0.06f, 0.2f, len), w, 0f, "Barandilla_de_cuna");
                    break;
                default:   // centauro y ciempiés: la silla de montar del estilo
                    if (f.plan == Plan.Ciempies) len = Mathf.Max(len, f.length * 0.6f);
                    SeatLegs(s, e, y, wid, len, zc);
                    C(s, new Vector3(0f, y + 0.04f, zc), new Vector3(wid, 0.08f, len), e.FloorSeating ? tx : w, 0f, "Asiento");
                    C(s, new Vector3(0f, y + 0.1f, zc), new Vector3(wid * 0.7f, 0.06f, len * 0.85f), tx, 0f, "Cojin_del_abdomen");
                    C(s, new Vector3(0f, y + 0.14f, zc + len * 0.5f - 0.05f), new Vector3(wid * 0.9f, 0.12f, 0.08f), w, 0f, "Pomo_de_la_silla");
                    break;
            }
        }
        else HumanSeat(s, e, bench);
    }

    // Las patas del asiento de abdomen en el estilo: cojín en el suelo, taburete, tijera, patas curvas del klismós, caja de munición…
    static void SeatLegs(Transform s, St.Entry e, float y, float wid, float len, float zc)
    {
        Color w = e.wood;
        switch (e.seat)
        {
            case St.Seat.Suelo: case St.Seat.Cojines:
                C(s, new Vector3(0f, y * 0.5f, zc), new Vector3(wid * 0.9f, y, len * 0.9f), Color.Lerp(e.textile, Linen, 0.2f), 0f, "Cojin_alto");
                break;
            case St.Seat.Diphros:
                for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * wid * 0.4f, y * 0.5f, zc), new Vector3(0.05f, y * 1.3f, 0.05f), w, new Vector3(0f, 0f, k * 0f), "Tijera").localRotation = Quaternion.Euler(35f, 0f, 0f);
                for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * wid * 0.4f, y * 0.5f, zc), new Vector3(0.05f, y * 1.3f, 0.05f), w, 0f, "Tijera").localRotation = Quaternion.Euler(-35f, 0f, 0f);
                break;
            case St.Seat.Klismos:
                foreach (var (x, z) in Corners) C(s, new Vector3(x * wid * 0.42f, y * 0.5f, zc + z * len * 0.45f), new Vector3(0.05f, y, 0.05f), w, new Vector3(z * 12f, 0f, 0f), "Pata_curva");
                break;
            case St.Seat.Banco: case St.Seat.Banqueta: case St.Seat.Taburete:
                for (int z = -1; z <= 1; z += 2) C(s, new Vector3(0f, y * 0.5f, zc + z * len * 0.4f), new Vector3(wid * 0.9f, y, 0.06f), w, 0f, "Pie");
                break;
            case St.Seat.Caja:
                C(s, new Vector3(0f, y * 0.5f, zc), new Vector3(wid, y, len), H("6B5A40"), 0f, "Caja_de_municion");
                C(s, new Vector3(0f, y * 0.5f, zc - len * 0.5f), new Vector3(wid * 0.6f, 0.08f, 0.02f), Paper, 0f, "Rotulo");
                break;
            case St.Seat.Thonet: case St.Seat.Windsor: case St.Seat.Butaca:
                foreach (var (x, z) in Corners) C(s, new Vector3(x * wid * 0.4f, y * 0.5f, zc + z * len * 0.42f), new Vector3(0.04f, y, 0.04f), w, 0f, e.seat == St.Seat.Thonet ? "Pata_curvada" : "Pata");
                C(s, new Vector3(0f, y * 0.4f, zc), new Vector3(wid * 0.85f, 0.03f, len * 0.85f), w, 0f, "Aro");
                break;
            default:   // cromado, soviética: patas de tubo
                foreach (var (x, z) in Corners) C(s, new Vector3(x * wid * 0.4f, y * 0.5f, zc + z * len * 0.42f), new Vector3(0.035f, y, 0.035f), Chrome, 0f, "Pata_de_tubo");
                break;
        }
    }

    // El asiento de una persona en el estilo (frente a la mesa: el respaldo, si lo hay, detrás, en −z).
    static void HumanSeat(Transform s, St.Entry e, bool bench)
    {
        Color w = e.wood, tx = e.textile;
        float wid = bench ? 1.4f : 0.44f;
        switch (bench ? St.Seat.Banco : e.seat)
        {
            case St.Seat.Suelo:
                C(s, new Vector3(0f, 0.04f, 0f), new Vector3(0.5f, 0.08f, 0.5f), tx, 0f, "Asiento");   // zabutón, estera o piel
                break;
            case St.Seat.Cojines:
                C(s, new Vector3(0f, 0.08f, 0f), new Vector3(0.6f, 0.16f, 0.6f), tx, 0f, "Asiento");
                C(s, new Vector3(0f, 0.32f, -0.3f), new Vector3(0.6f, 0.36f, 0.16f), Color.Lerp(tx, Brass, 0.3f), 0f, "Cojin_del_respaldo");
                break;
            case St.Seat.Diphros:
                C(s, new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.05f, 0.42f), w, 0f, "Asiento");
                for (int k = -1; k <= 1; k += 2) { C(s, new Vector3(k * 0.2f, 0.22f, 0f), new Vector3(0.04f, 0.55f, 0.04f), w, 0f, "Tijera").localRotation = Quaternion.Euler(38f, 0f, 0f); C(s, new Vector3(k * 0.2f, 0.22f, 0f), new Vector3(0.04f, 0.55f, 0.04f), w, 0f, "Tijera").localRotation = Quaternion.Euler(-38f, 0f, 0f); }
                C(s, new Vector3(0f, 0.475f, 0f), new Vector3(0.42f, 0.02f, 0.36f), H("C8A070"), 0f, "Cuero");
                break;
            case St.Seat.Klismos:
                C(s, new Vector3(0f, 0.45f, 0f), new Vector3(0.46f, 0.05f, 0.44f), w, 0f, "Asiento");
                foreach (var (x, z) in Corners) C(s, new Vector3(x * 0.2f, 0.22f, z * 0.24f), new Vector3(0.04f, 0.46f, 0.04f), w, new Vector3(-z * 14f, 0f, 0f), "Pata_curva");
                C(s, new Vector3(0f, 0.85f, -0.27f), new Vector3(0.46f, 0.16f, 0.04f), w, new Vector3(-20f, 0f, 0f), "Respaldo_curvo");
                for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * 0.2f, 0.65f, -0.24f), new Vector3(0.04f, 0.4f, 0.04f), w, new Vector3(-12f, 0f, 0f), "Montante");
                break;
            case St.Seat.Taburete: case St.Seat.Banqueta:
                C(s, new Vector3(0f, 0.45f, 0f), new Vector3(0.38f, 0.05f, 0.38f), w, 0f, "Asiento");
                foreach (var (x, z) in Corners) C(s, new Vector3(x * 0.15f, 0.22f, z * 0.15f), new Vector3(0.04f, 0.44f, 0.04f), w, 0f, "Pata");
                C(s, new Vector3(0f, 0.15f, 0f), new Vector3(0.32f, 0.02f, 0.02f), w, 0f, "Travesano");
                break;
            case St.Seat.Banco:
                C(s, new Vector3(0f, 0.44f, 0f), new Vector3(wid, 0.05f, 0.32f), w, 0f, "Asiento");
                for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * (wid * 0.5f - 0.1f), 0.21f, 0f), new Vector3(0.06f, 0.42f, 0.28f), WoodDark, 0f, "Pata");
                break;
            case St.Seat.Caja:
                C(s, new Vector3(0f, 0.2f, 0f), new Vector3(0.5f, 0.4f, 0.32f), H("6B5A40"), 0f, "Asiento");
                C(s, new Vector3(0f, 0.25f, -0.17f), new Vector3(0.3f, 0.08f, 0.01f), Paper, 0f, "Rotulo");
                break;
            case St.Seat.Butaca:   // butaca venezolana: asiento de cuero inclinado sobre patas en tijera
                C(s, new Vector3(0f, 0.3f, 0f), new Vector3(0.55f, 0.04f, 0.5f), H("8A5A34"), new Vector3(-12f, 0f, 0f), "Asiento");
                C(s, new Vector3(0f, 0.6f, -0.32f), new Vector3(0.55f, 0.5f, 0.04f), H("8A5A34"), new Vector3(-25f, 0f, 0f), "Respaldo");
                for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * 0.27f, 0.3f, -0.05f), new Vector3(0.05f, 0.05f, 0.8f), w, new Vector3(30f, 0f, 0f), "Pata_en_tijera");
                break;
            default:   // Windsor, Thonet, cromada, soviética: cuatro patas, asiento y respaldo de su material
            {
                bool tube = e.seat == St.Seat.Cromado || e.seat == St.Seat.Sovietica;
                Color legs = tube ? Chrome : w, seat = tube ? tx : w;
                C(s, new Vector3(0f, 0.45f, 0f), new Vector3(0.42f, 0.05f, 0.42f), seat, 0f, "Asiento");
                foreach (var (x, z) in Corners) C(s, new Vector3(x * 0.18f, 0.22f, z * 0.18f), new Vector3(tube ? 0.03f : 0.04f, 0.44f, tube ? 0.03f : 0.04f), legs, 0f, "Pata");
                if (e.seat == St.Seat.Windsor) for (int k = 0; k < 5; k++) C(s, new Vector3(-0.16f + k * 0.08f, 0.72f, -0.19f), new Vector3(0.02f, 0.5f, 0.02f), w, 0f, "Husillo");
                if (e.seat == St.Seat.Windsor) C(s, new Vector3(0f, 0.98f, -0.19f), new Vector3(0.44f, 0.05f, 0.05f), w, 0f, "Copete");
                if (e.seat == St.Seat.Thonet) { C(s, new Vector3(0f, 0.75f, -0.2f), new Vector3(0.38f, 0.4f, 0.03f), w, 0f, "Aro_del_respaldo"); C(s, new Vector3(0f, 0.25f, 0f), new Vector3(0.36f, 0.02f, 0.36f), w, 45f, "Aro_de_las_patas"); }
                if (tube) C(s, new Vector3(0f, 0.72f, -0.2f), new Vector3(0.4f, e.seat == St.Seat.Sovietica ? 0.02f : 0.32f, 0.04f), tx, 0f, "Respaldo");
                break;
            }
        }
    }

    // ══ LA MESA ═════════════════════════════════════════════════════════════════════════════════════════════════════
    // Del estilo (estera de comer, mesa baja o bandeja, caballetes, trapeza de tres patas, mesa larga, redonda, fórmica) a la
    // ALTURA DE LAS MANOS de quien come (una persona sentada: 0,75 m; en el suelo: 0,35 m; un insecto-humano, erguido sobre
    // su asiento de abdomen, a la de sus manos), con un asiento a la medida de cada comensal (<paramref name="users"/>; los que
    // falten, para el cuerpo por defecto), sus platos, cuencos, cubiertos o palillos, la jarra y la luz de la época.
    static void TableWithSeats(Transform t, St.Entry e, Fit f, IList<Fit> users, int minSeats)
    {
        int seats = Mathf.Max(minSeats, users != null ? users.Count : 0);
        var bodies = new List<Fit>();
        for (int i = 0; i < seats; i++) bodies.Add(users != null && users.Count > 0 ? (i < users.Count ? users[i] : users[i % users.Count]) : f);
        Fit all = MicroBody.Union(bodies);
        bool insect = all.Insect, floor = e.FloorSeating;
        float h = insect ? Mathf.Clamp(Hands(all) - 0.12f, 0.6f, 1.0f) * (floor ? 0.8f : 1f) : floor ? 0.35f : 0.75f;
        if (e.table == St.Table.EsteraDeComer && !insect) h = 0.06f;
        float seatW = 0.55f; foreach (Fit b in bodies) seatW = Mathf.Max(seatW, b.Insect ? Mathf.Clamp(b.width * 0.75f, 0.3f, 1.2f) + 0.2f : 0.55f);
        int perSide = (seats + 1) / 2;
        float TL = Mathf.Max(1.2f, perSide * (seatW + 0.12f)), TD = insect ? 0.9f : 0.8f;
        Transform tb = new GameObject("Mesa").transform; tb.SetParent(t, false);
        TableTop(tb, e, new Vector2(TL, TD), h);
        for (int i = 0; i < seats; i++)
        {
            bool front = i % 2 == 0; int k = i / 2;
            float x = perSide <= 1 ? 0f : -TL * 0.5f + (k + 0.5f) * TL / perSide;
            Seat(t, new Vector3(x, 0f, front ? -(TD * 0.5f + 0.25f) : TD * 0.5f + 0.25f), front ? 0f : 180f, e, bodies[i], false);
            float zp = front ? -TD * 0.25f : TD * 0.25f;
            KitchenProps.Plate(tb, "Plato", new Vector3(x, h, zp));
            KitchenProps.Bowl(tb, "Cuenco", new Vector3(x + 0.13f, h, zp));
            bool chop = e.regions[0] == St.Region.China || e.regions[0] == St.Region.HongKong || e.regions[0] == St.Region.Japon;
            C(tb, new Vector3(x - 0.16f, h + 0.01f, zp), new Vector3(0.01f, 0.01f, 0.2f), chop ? WoodDark : KitchenProps.Steel, 0f, chop ? "Palillos" : "Cuchara");
        }
        C(tb, new Vector3(-TL * 0.25f, h + 0.05f, 0f), new Vector3(0.14f, 0.1f, 0.14f), e.from < 1700 ? Clay : KitchenProps.Ceramic, 0f, "Jarra");
        Lamp(tb, e, new Vector3(TL * 0.25f, h, 0f), false);
    }

    static void TableTop(Transform t, St.Entry e, Vector2 size, float h)
    {
        Color w = e.wood;
        switch (e.table)
        {
            case St.Table.EsteraDeComer:
                if (h < 0.1f) { C(t, new Vector3(0f, 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), Straw, 0f, "Estera_de_comer"); C(t, new Vector3(0f, 0.04f, 0f), new Vector3(size.x * 0.8f, 0.02f, size.y * 0.6f), Reed, 0f, "Tablero"); return; }
                C(t, new Vector3(0f, h - 0.05f, 0f), new Vector3(size.x, 0.1f, size.y), Stone, 0f, "Tablero");   // losa sobre piedras
                for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * size.x * 0.35f, (h - 0.1f) * 0.5f, 0f), new Vector3(0.3f, h - 0.1f, size.y * 0.7f), Stone, 0f, "Piedra");
                return;
            case St.Table.Baja:   // mesa baja o la bandeja redonda (sini) sobre su pie
                C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), e.regions[0] == St.Region.Otomano || e.regions[0] == St.Region.Islam ? Copper : w, 0f, "Tablero");
                C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(size.x * 0.85f, 0.045f, size.y * 1.12f), e.regions[0] == St.Region.Otomano || e.regions[0] == St.Region.Islam ? Copper : w, 45f, "Tablero");
                C(t, new Vector3(0f, (h - 0.04f) * 0.5f, 0f), new Vector3(size.x * 0.6f, h - 0.04f, 0.05f), w, 45f, "Pie_en_aspa");
                C(t, new Vector3(0f, (h - 0.04f) * 0.5f, 0f), new Vector3(size.x * 0.6f, h - 0.04f, 0.05f), w, -45f, "Pie_en_aspa");
                return;
            case St.Table.Caballete:
                C(t, new Vector3(0f, h - 0.025f, 0f), new Vector3(size.x, 0.05f, size.y), w, 0f, "Tablero");
                for (int k = -1; k <= 1; k += 2)
                {
                    C(t, new Vector3(k * size.x * 0.35f, (h - 0.05f) * 0.5f, -size.y * 0.25f), new Vector3(0.06f, h, 0.06f), WoodDark, new Vector3(15f, 0f, 0f), "Caballete");
                    C(t, new Vector3(k * size.x * 0.35f, (h - 0.05f) * 0.5f, size.y * 0.25f), new Vector3(0.06f, h, 0.06f), WoodDark, new Vector3(-15f, 0f, 0f), "Caballete");
                    C(t, new Vector3(k * size.x * 0.35f, h - 0.08f, 0f), new Vector3(0.08f, 0.06f, size.y * 0.8f), WoodDark, 0f, "Travesano");
                }
                return;
            case St.Table.Trapeza:   // griega: tablero rectangular, dos patas delante y una detrás
                C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), w, 0f, "Tablero");
                for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * (size.x * 0.5f - 0.08f), (h - 0.04f) * 0.5f, -size.y * 0.3f), new Vector3(0.05f, h - 0.04f, 0.05f), w, 0f, "Pata");
                C(t, new Vector3(0f, (h - 0.04f) * 0.5f, size.y * 0.3f), new Vector3(0.05f, h - 0.04f, 0.05f), w, 0f, "Pata");
                return;
            case St.Table.Redonda:   // redonda (octógono de dos tableros) sobre su pie
                C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), w, 0f, "Tablero");
                C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(size.x * 0.85f, 0.042f, size.y * 1.15f), w, 45f, "Tablero");
                C(t, new Vector3(0f, (h - 0.04f) * 0.5f, 0f), new Vector3(0.12f, h - 0.04f, 0.12f), WoodDark, 0f, "Pie");
                C(t, new Vector3(0f, 0.03f, 0f), new Vector3(0.6f, 0.06f, 0.6f), WoodDark, 45f, "Base");
                return;
            case St.Table.Formica:
                C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), H("D8DCD0"), 0f, "Tablero_de_formica");
                C(t, new Vector3(0f, h - 0.04f, 0f), new Vector3(size.x + 0.02f, 0.02f, size.y + 0.02f), Chrome, 0f, "Canto_cromado");
                foreach (var (x, z) in Corners) C(t, new Vector3(x * (size.x * 0.5f - 0.06f), (h - 0.04f) * 0.5f, z * (size.y * 0.5f - 0.06f)), new Vector3(0.035f, h - 0.04f, 0.035f), Chrome, 0f, "Pata_de_tubo");
                return;
            default:   // refectorio: mesa larga con travesaño bajo
                C(t, new Vector3(0f, h - 0.03f, 0f), new Vector3(size.x, 0.06f, size.y), w, 0f, "Tablero");
                foreach (var (x, z) in Corners) C(t, new Vector3(x * (size.x * 0.5f - 0.08f), (h - 0.06f) * 0.5f, z * (size.y * 0.5f - 0.08f)), new Vector3(0.07f, h - 0.06f, 0.07f), WoodDark, 0f, "Pata");
                C(t, new Vector3(0f, 0.15f, 0f), new Vector3(size.x - 0.16f, 0.05f, 0.05f), WoodDark, 0f, "Travesano");
                return;
        }
    }

    // TableTop genérico (lo usan los muebles de trabajo: el horno, el laboratorio, el puesto…).
    static void TableTop(Transform t, Vector2 size, float h, Color c)
    {
        C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(size.x, 0.04f, size.y), c, 0f, "Tablero");
        foreach (var (x, z) in Corners)
            C(t, new Vector3(x * (size.x * 0.5f - 0.05f), (h - 0.04f) * 0.5f, z * (size.y * 0.5f - 0.05f)), new Vector3(0.05f, h - 0.04f, 0.05f), WoodDark, 0f, "Pata");
    }

    // ══ LA LUZ de la época (en la mesa o junto a la cama): la concha de aceite, la lucerna, la diya, la vela, el candil de bronce,
    //    el andon de papel, el quinqué, el farol de gas o la lámpara eléctrica. El fuego de la prehistoria es el hogar: nada.
    static void Lamp(Transform t, St.Entry e, Vector3 at, bool floor)
    {
        if (e.light == St.Light.Fuego) return;
        Transform l = new GameObject("Luz").transform; l.SetParent(t, false); l.localPosition = at;
        float stand = floor && e.light != St.Light.Andon && e.light != St.Light.Bombilla ? 0.6f : 0f;
        if (stand > 0f) { C(l, new Vector3(0f, stand * 0.5f, 0f), new Vector3(0.05f, stand, 0.05f), e.light == St.Light.Candil ? Brass : e.wood, 0f, "Pie_de_la_luz"); C(l, new Vector3(0f, 0.02f, 0f), new Vector3(0.22f, 0.04f, 0.22f), e.wood, 0f, "Base"); }
        Vector3 o = Vector3.up * stand;
        switch (e.light)
        {
            case St.Light.Concha: C(l, o + new Vector3(0f, 0.03f, 0f), new Vector3(0.14f, 0.05f, 0.1f), H("E8DCC8"), 0f, "Concha_de_aceite"); C(l, o + new Vector3(0.06f, 0.07f, 0f), new Vector3(0.02f, 0.04f, 0.02f), Flame, 0f, "Llama"); break;
            case St.Light.Lucerna: case St.Light.Diya:
                C(l, o + new Vector3(0f, 0.025f, 0f), new Vector3(0.1f, 0.05f, e.light == St.Light.Diya ? 0.1f : 0.16f), Clay, 0f, e.light == St.Light.Diya ? "Diya" : "Lucerna");
                C(l, o + new Vector3(0f, 0.07f, e.light == St.Light.Diya ? 0.03f : 0.07f), new Vector3(0.02f, 0.04f, 0.02f), Flame, 0f, "Llama"); break;
            case St.Light.Vela: C(l, o + new Vector3(0f, 0.02f, 0f), new Vector3(0.1f, 0.04f, 0.1f), Brass, 0f, "Candelero"); C(l, o + new Vector3(0f, 0.11f, 0f), new Vector3(0.03f, 0.16f, 0.03f), Pillow, 0f, "Vela"); C(l, o + new Vector3(0f, 0.21f, 0f), new Vector3(0.015f, 0.04f, 0.015f), Flame, 0f, "Llama"); break;
            case St.Light.Candil: C(l, o + new Vector3(0f, 0.03f, 0f), new Vector3(0.18f, 0.05f, 0.1f), Brass, 0f, "Candil"); C(l, o + new Vector3(0.08f, 0.07f, 0f), new Vector3(0.02f, 0.04f, 0.02f), Flame, 0f, "Llama"); break;
            case St.Light.Andon: C(l, o + new Vector3(0f, 0.3f, 0f), new Vector3(0.28f, 0.6f, 0.28f), Paper, 0f, "Andon"); C(l, o + new Vector3(0f, 0.3f, 0f), new Vector3(0.3f, 0.62f, 0.04f), e.wood, 0f, "Marco"); C(l, o + new Vector3(0f, 0.3f, 0f), new Vector3(0.04f, 0.62f, 0.3f), e.wood, 0f, "Marco"); break;
            case St.Light.Quinque: case St.Light.Gas:
                C(l, o + new Vector3(0f, 0.06f, 0f), new Vector3(0.12f, 0.12f, 0.12f), e.light == St.Light.Gas ? Brass : H("6E9C7A"), 0f, e.light == St.Light.Gas ? "Lampara_de_gas" : "Deposito_de_petroleo");
                C(l, o + new Vector3(0f, 0.2f, 0f), new Vector3(0.07f, 0.16f, 0.07f), KitchenProps.Glass, 0f, "Tubo_de_cristal"); C(l, o + new Vector3(0f, 0.18f, 0f), new Vector3(0.025f, 0.05f, 0.025f), Flame, 0f, "Llama"); break;
            default:   // lámpara eléctrica: pie y pantalla
                float y0 = floor ? 1.3f : 0.35f;
                C(l, new Vector3(0f, y0 * 0.5f, 0f), new Vector3(0.04f, y0, 0.04f), Chrome, 0f, "Pie_de_la_lampara"); C(l, new Vector3(0f, 0.02f, 0f), new Vector3(0.2f, 0.04f, 0.2f), Chrome, 0f, "Base");
                C(l, new Vector3(0f, y0 + 0.08f, 0f), new Vector3(0.3f, 0.2f, 0.3f), H("E8D9A6"), 0f, "Pantalla"); C(l, new Vector3(0f, y0, 0f), new Vector3(0.06f, 0.06f, 0.06f), Flame, 0f, "Bombilla"); break;
        }
    }

    // ══ EL RETRETE ══════════════════════════════════════════════════════════════════════════════════════════════════
    // Del estilo: pozo negro, asiento sobre vasija de arena (Amarna) o pozo de anillos, letrina comunal (Ostia), silla-orinal,
    // letrina a la turca con su jarra, letrina del patio, cisterna alta o baja, cubo de trinchera. Para un insecto-humano, una
    // PLATAFORMA DE ABDOMEN: se pasa por encima y la punta del abdomen queda sobre el agujero (detrás, en −z), con dos
    // estribos para las patas y un asidero delante para los brazos; la cisterna, la jarra o la cal, delante, al alcance.
    static void Toilet(Transform t, St.Entry e, Fit f)
    {
        if (f.Insect) { AbdomenToilet(t, e, f); return; }
        Color w = e.wood;
        switch (e.toilet)
        {
            case St.Toilet.Pozo:
                for (int k = 0; k < 6; k++) C(t, Quaternion.Euler(0f, k * 60f, 0f) * new Vector3(0f, 0.08f, 0.38f), new Vector3(0.3f, 0.16f, 0.16f), Stone, k * 60f, "Piedra");
                C(t, new Vector3(0f, 0.02f, 0f), new Vector3(0.4f, 0.03f, 0.4f), H("1E1A16"), 0f, "Hoyo");
                for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.22f, 0.05f, -0.05f), new Vector3(0.18f, 0.08f, 0.4f), Stone, 0f, "Piedra_de_los_pies");
                C(t, new Vector3(0.65f, 0.12f, 0f), new Vector3(0.4f, 0.24f, 0.4f), Soil, 20f, "Tierra_para_tapar");
                C(t, new Vector3(0.65f, 0.26f, 0f), new Vector3(0.3f, 0.04f, 0.3f), Green, 50f, "Hojas");
                return;
            case St.Toilet.Vasija:   // asiento con su agujero sobre una vasija de arena (Amarna) o sobre el pozo de anillos (India)
                C(t, new Vector3(0f, 0.2f, 0f), new Vector3(0.36f, 0.4f, 0.36f), Clay, 45f, "Vasija_de_arena");
                for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.28f, 0.2f, 0f), new Vector3(0.14f, 0.4f, 0.5f), Brick, 0f, "Apoyo");
                C(t, new Vector3(0f, 0.42f, 0f), new Vector3(0.7f, 0.05f, 0.5f), Stone, 0f, "Asiento_de_piedra");
                C(t, new Vector3(0f, 0.445f, -0.02f), new Vector3(0.14f, 0.01f, 0.22f), H("1E1A16"), 0f, "Agujero");
                C(t, new Vector3(0.55f, 0.15f, 0.1f), new Vector3(0.22f, 0.3f, 0.22f), Clay, 0f, "Jarra_de_agua");
                return;
            case St.Toilet.Comunal:
                C(t, new Vector3(0f, 0.22f, 0.1f), new Vector3(1.6f, 0.44f, 0.5f), Stone, 0f, "Banco_de_la_letrina");
                for (int k = -1; k <= 1; k++) C(t, new Vector3(k * 0.5f, 0.445f, 0.05f), new Vector3(0.14f, 0.01f, 0.26f), H("1E1A16"), 0f, "Agujero");
                C(t, new Vector3(0f, 0.04f, -0.3f), new Vector3(1.6f, 0.06f, 0.12f), Water, 0f, "Canal_de_agua");
                C(t, new Vector3(0.75f, 0.35f, -0.3f), new Vector3(0.03f, 0.6f, 0.03f), w, 20f, "Xylospongium");
                return;
            case St.Toilet.Bacinilla:   // silla-orinal (close stool) con su tapa y el orinal dentro
                foreach (var (x, z) in Corners) C(t, new Vector3(x * 0.22f, 0.22f, z * 0.2f), new Vector3(0.05f, 0.44f, 0.05f), w, 0f, "Pata");
                C(t, new Vector3(0f, 0.3f, 0f), new Vector3(0.48f, 0.26f, 0.44f), w, 0f, "Caja");
                C(t, new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.04f, 0.46f), e.textile, 0f, "Asiento");
                C(t, new Vector3(0f, 0.47f, -0.02f), new Vector3(0.18f, 0.01f, 0.2f), H("1E1A16"), 0f, "Agujero");
                C(t, new Vector3(0f, 0.75f, 0.22f), new Vector3(0.5f, 0.55f, 0.04f), w, 0f, "Respaldo");
                C(t, new Vector3(0.45f, 0.08f, 0f), new Vector3(0.22f, 0.16f, 0.22f), KitchenProps.Ceramic, 0f, "Orinal");
                return;
            case St.Toilet.Turca:
                C(t, new Vector3(0f, 0.03f, 0f), new Vector3(0.6f, 0.06f, 0.8f), e.from >= 1900 ? KitchenProps.White : Stone, 0f, "Placa_de_la_turca");
                C(t, new Vector3(0f, 0.065f, 0.05f), new Vector3(0.14f, 0.01f, 0.3f), H("1E1A16"), 0f, "Agujero");
                for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.17f, 0.075f, -0.15f), new Vector3(0.12f, 0.03f, 0.26f), e.from >= 1900 ? KitchenProps.White : Stone, 0f, "Pisadera");
                C(t, new Vector3(0.45f, 0.14f, 0.2f), new Vector3(0.16f, 0.28f, 0.16f), e.regions[0] == St.Region.HongKong ? Enamel : Copper, 0f, "Jarra_de_agua");
                return;
            case St.Toilet.Caseta:   // la letrina del patio: banco con su agujero y su tapa, los cuatro postes (sin muros) y el papel o las mazorcas
                C(t, new Vector3(0f, 0.22f, 0.05f), new Vector3(0.8f, 0.44f, 0.55f), w, 0f, "Banco_de_la_letrina");
                C(t, new Vector3(0f, 0.445f, 0.03f), new Vector3(0.24f, 0.01f, 0.22f), H("1E1A16"), 0f, "Agujero");
                C(t, new Vector3(0f, 0.47f, 0.25f), new Vector3(0.36f, 0.04f, 0.1f), w, 0f, "Tapa");
                foreach (var (x, z) in Corners) C(t, new Vector3(x * 0.55f, 1.0f, z * 0.45f), new Vector3(0.08f, 2.0f, 0.08f), w, 0f, "Poste_de_la_caseta");
                C(t, new Vector3(0.6f, 0.15f, 0f), new Vector3(0.25f, 0.3f, 0.25f), Wood, 0f, "Cubo_de_cal");
                return;
            case St.Toilet.Cubo:
                C(t, new Vector3(0f, 0.22f, 0f), new Vector3(0.36f, 0.44f, 0.36f), H("4A4A4A"), 0f, "Cubo_de_letrina");
                C(t, new Vector3(0f, 0.46f, 0f), new Vector3(0.7f, 0.04f, 0.3f), w, 0f, "Tabla_asiento");
                C(t, new Vector3(0.5f, 0.12f, 0.1f), new Vector3(0.3f, 0.24f, 0.25f), Sack, 0f, "Saco_de_cal");
                return;
            default:   // inodoro de porcelana: cisterna alta (con su tubo y su cadena) o baja
            {
                bool high = e.toilet == St.Toilet.CisternaAlta;
                C(t, new Vector3(0f, 0.2f, 0.05f), new Vector3(0.36f, 0.4f, 0.5f), KitchenProps.White, 0f, "Taza");
                C(t, new Vector3(0f, 0.41f, 0f), new Vector3(0.4f, 0.03f, 0.48f), high ? e.wood : KitchenProps.White, 0f, "Asiento");
                C(t, new Vector3(0f, 0.4f, -0.05f), new Vector3(0.24f, 0.01f, 0.28f), Water, 0f, "Agua");
                if (high)
                {
                    C(t, new Vector3(0f, 1.0f, 0.3f), new Vector3(0.04f, 1.2f, 0.04f), KitchenProps.Steel, 0f, "Tubo");
                    C(t, new Vector3(0f, 1.7f, 0.3f), new Vector3(0.45f, 0.25f, 0.2f), e.from >= 1919 ? KitchenProps.White : Iron, 0f, "Cisterna");
                    C(t, new Vector3(0.2f, 1.45f, 0.22f), new Vector3(0.01f, 0.4f, 0.01f), Brass, 0f, "Cadena");
                }
                else C(t, new Vector3(0f, 0.6f, 0.27f), new Vector3(0.42f, 0.36f, 0.18f), KitchenProps.White, 0f, "Cisterna");
                C(t, new Vector3(0.35f, 0.6f, 0.2f), new Vector3(0.12f, 0.12f, 0.1f), Paper, 0f, "Papel");
                return;
            }
        }
    }

    static void AbdomenToilet(Transform t, St.Entry e, Fit f)
    {
        float W = Mathf.Clamp(f.span * 0.8f, 0.8f, 2.2f), L = Mathf.Clamp(f.abdomen + 0.7f, 1.0f, 2.6f), y = Mathf.Clamp(f.belly * 0.6f, 0.06f, 0.4f);
        bool porcelain = e.toilet == St.Toilet.CisternaAlta || e.toilet == St.Toilet.CisternaBaja || (e.toilet == St.Toilet.Turca && e.from >= 1900);
        Color plate = porcelain ? KitchenProps.White : e.toilet == St.Toilet.Pozo || e.toilet == St.Toilet.Comunal || e.toilet == St.Toilet.Turca || e.toilet == St.Toilet.Vasija ? Stone : e.wood;
        // Los estribos (para las patas de detrás) a los lados y, entre ellos, el canal con el agujero bajo la punta del abdomen.
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * (W * 0.5f - 0.12f), y * 0.5f, 0f), new Vector3(0.24f, y, L), plate, 0f, "Estribo");
        C(t, new Vector3(0f, 0.02f, 0f), new Vector3(W - 0.48f, 0.04f, L), plate, 0f, "Canal");
        C(t, new Vector3(0f, 0.045f, -L * 0.5f + 0.25f), new Vector3(Mathf.Min(0.4f, W - 0.6f), 0.01f, 0.35f), H("1E1A16"), 0f, "Agujero");
        if (e.toilet == St.Toilet.Vasija || e.toilet == St.Toilet.Pozo) C(t, new Vector3(0f, -0.0f, -L * 0.5f - 0.15f), new Vector3(0.36f, 0.3f, 0.3f), Clay, 0f, "Vasija_de_arena");
        // Delante: el asidero para los brazos y lo de la época (cisterna, jarra, cal).
        for (int s = -1; s <= 1; s += 2) C(t, new Vector3(s * 0.3f, 0.45f, L * 0.5f + 0.05f), new Vector3(0.05f, 0.9f, 0.05f), porcelain ? Chrome : e.wood, 0f, "Poste_del_asidero");
        C(t, new Vector3(0f, 0.88f, L * 0.5f + 0.05f), new Vector3(0.66f, 0.05f, 0.05f), porcelain ? Chrome : e.wood, 0f, "Asidero");
        switch (e.toilet)
        {
            case St.Toilet.CisternaAlta: C(t, new Vector3(0f, 1.6f, L * 0.5f + 0.15f), new Vector3(0.45f, 0.25f, 0.2f), e.from >= 1919 ? KitchenProps.White : Iron, 0f, "Cisterna"); C(t, new Vector3(0f, 1.0f, L * 0.5f + 0.15f), new Vector3(0.04f, 1.2f, 0.04f), KitchenProps.Steel, 0f, "Tubo"); break;
            case St.Toilet.CisternaBaja: C(t, new Vector3(0f, 0.55f, L * 0.5f + 0.2f), new Vector3(0.42f, 0.36f, 0.18f), KitchenProps.White, 0f, "Cisterna"); break;
            case St.Toilet.Turca: case St.Toilet.Vasija: case St.Toilet.Comunal: C(t, new Vector3(0.5f, 0.15f, L * 0.5f), new Vector3(0.18f, 0.3f, 0.18f), e.regions[0] == St.Region.HongKong ? Enamel : e.regions[0] == St.Region.Otomano || e.regions[0] == St.Region.Islam ? Copper : Clay, 0f, "Jarra_de_agua"); break;
            case St.Toilet.Caseta: case St.Toilet.Cubo: C(t, new Vector3(0.5f, 0.15f, L * 0.5f), new Vector3(0.25f, 0.3f, 0.25f), Sack, 0f, "Saco_de_cal"); break;
            case St.Toilet.Bacinilla: C(t, new Vector3(0f, 0.1f, -L * 0.5f + 0.25f), new Vector3(0.3f, 0.16f, 0.3f), KitchenProps.Ceramic, 0f, "Orinal"); break;
            default: C(t, new Vector3(0.5f, 0.12f, L * 0.5f), new Vector3(0.4f, 0.24f, 0.4f), Soil, 20f, "Tierra_para_tapar"); break;
        }
    }

    // ══ EL LAVAMANOS ════════════════════════════════════════════════════════════════════════════════════════════════
    // Del estilo, a la ALTURA DE LAS MANOS de quien lo usa; para un insecto-humano, la jofaina más ancha (cuatro brazos en la
    // mantis) y el PEINE DE ANTENAS (los insectos se limpian las antenas pasándolas por el peine de la pata delantera).
    static void Washbasin(Transform t, St.Entry e, Fit f)
    {
        float h = f.Insect ? Mathf.Clamp(Hands(f) - 0.05f, 0.7f, 1.05f) : 0.8f, bw = f.plan == Plan.Mantis ? 0.75f : f.Insect ? 0.55f : 0.42f;
        Color w = e.wood;
        switch (e.basin)
        {
            case St.Basin.Pila:
                C(t, new Vector3(0f, h * 0.31f, 0f), new Vector3(bw + 0.5f, h * 0.62f, 0.6f), Stone, 0f, "Pila_de_piedra");
                C(t, new Vector3(0f, h * 0.62f - 0.02f, 0f), new Vector3(bw + 0.35f, 0.04f, 0.45f), Water, 0f, "Agua");
                C(t, new Vector3(0.6f, 0.08f, 0f), new Vector3(0.2f, 0.16f, 0.2f), H("C9A050"), 0f, "Calabaza");
                break;
            case St.Basin.Cantaro:   // el cántaro grande, la jofaina en su soporte y la lota (o el cuenco) para echarse el agua
                C(t, new Vector3(-0.45f, 0.35f, 0.1f), new Vector3(0.4f, 0.7f, 0.4f), Clay, 0f, "Cantaro");
                C(t, new Vector3(-0.45f, 0.72f, 0.1f), new Vector3(0.22f, 0.06f, 0.22f), Clay, 45f, "Boca");
                C(t, new Vector3(0.2f, (h - 0.1f) * 0.5f, 0f), new Vector3(0.12f, h - 0.1f, 0.12f), Brick, 0f, "Soporte");
                C(t, new Vector3(0.2f, h - 0.05f, 0f), new Vector3(bw, 0.1f, bw), Clay, 45f, "Jofaina");
                C(t, new Vector3(0.2f, h - 0.02f, 0f), new Vector3(bw * 0.8f, 0.02f, bw * 0.8f), Water, 45f, "Agua");
                C(t, new Vector3(0.55f, 0.08f, -0.1f), new Vector3(0.14f, 0.16f, 0.14f), Brass, 0f, "Lota");
                break;
            case St.Basin.Louterion:   // pila sobre una columna
                C(t, new Vector3(0f, (h - 0.1f) * 0.5f, 0f), new Vector3(0.18f, h - 0.1f, 0.18f), H("E0DCCC"), 0f, "Columna");
                C(t, new Vector3(0f, 0.04f, 0f), new Vector3(0.4f, 0.08f, 0.4f), H("E0DCCC"), 0f, "Basa");
                C(t, new Vector3(0f, h - 0.04f, 0f), new Vector3(bw + 0.25f, 0.1f, bw + 0.25f), H("E0DCCC"), 45f, "Pila");
                C(t, new Vector3(0f, h, 0f), new Vector3(bw + 0.1f, 0.02f, bw + 0.1f), Water, 45f, "Agua");
                break;
            case St.Basin.Ibrik:   // el ibrik (jarra de pico largo) y la leğen (jofaina con su rejilla) sobre un taburete bajo
                C(t, new Vector3(0f, (h - 0.12f) * 0.5f, 0f), new Vector3(0.45f, h - 0.12f, 0.4f), w, 0f, "Taburete");
                C(t, new Vector3(0f, h - 0.06f, 0f), new Vector3(bw, 0.1f, bw), Copper, 45f, "Legen");
                C(t, new Vector3(0f, h, 0f), new Vector3(bw * 0.6f, 0.02f, bw * 0.6f), Copper, 0f, "Rejilla");
                C(t, new Vector3(0.12f, h + 0.13f, 0.1f), new Vector3(0.12f, 0.26f, 0.12f), Copper, 0f, "Ibrik");
                C(t, new Vector3(0.02f, h + 0.18f, 0.1f), new Vector3(0.18f, 0.02f, 0.02f), Copper, new Vector3(0f, 0f, 25f), "Pico");
                C(t, new Vector3(-0.32f, h * 0.7f, 0f), new Vector3(0.03f, 0.3f, 0.3f), e.textile, 0f, "Toalla_bordada");
                break;
            case St.Basin.Soporte:   // palangana esmaltada en su soporte de madera (o de hierro), con el jabón y la toalla
                foreach (var (x, z) in Corners) C(t, new Vector3(x * 0.2f, h * 0.5f, z * 0.2f), new Vector3(0.035f, h, 0.035f), e.from >= 1900 ? e.metal : w, 0f, "Pata");
                C(t, new Vector3(0f, h - 0.05f, 0f), new Vector3(bw, 0.1f, bw), Enamel, 45f, "Palangana_esmaltada");
                C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(bw * 0.8f, 0.02f, bw * 0.8f), Water, 45f, "Agua");
                C(t, new Vector3(0f, h * 0.3f, 0f), new Vector3(0.42f, 0.02f, 0.42f), w, 0f, "Balda");
                C(t, new Vector3(0.28f, h * 0.75f, 0f), new Vector3(0.03f, 0.3f, 0.3f), Linen, 0f, "Toalla");
                break;
            case St.Basin.Tarai:
                C(t, new Vector3(0f, (h - 0.15f) * 0.5f, 0f), new Vector3(0.5f, h - 0.15f, 0.5f), w, 0f, "Soporte");
                C(t, new Vector3(0f, h - 0.07f, 0f), new Vector3(bw + 0.1f, 0.14f, bw + 0.1f), H("A07A50"), 0f, "Tarai");
                C(t, new Vector3(0f, h - 0.02f, 0f), new Vector3(bw, 0.02f, bw), Water, 0f, "Agua");
                C(t, new Vector3(0.5f, 0.15f, 0f), new Vector3(0.25f, 0.3f, 0.25f), H("A07A50"), 0f, "Cubo_de_madera");
                C(t, new Vector3(0.5f, 0.32f, 0f), new Vector3(0.05f, 0.05f, 0.3f), e.wood, 0f, "Hishaku");
                break;
            case St.Basin.Tinajero:   // el tinajero: piedra de destilar arriba, que gotea en la tinaja de abajo, con su tacita
                foreach (var (x, z) in Corners) C(t, new Vector3(x * 0.28f, 0.75f, z * 0.28f), new Vector3(0.06f, 1.5f, 0.06f), w, 0f, "Pata_del_tinajero");
                C(t, new Vector3(0f, 1.3f, 0f), new Vector3(0.45f, 0.25f, 0.45f), Stone, 45f, "Piedra_de_destilar");
                C(t, new Vector3(0f, 0.35f, 0f), new Vector3(0.4f, 0.6f, 0.4f), Clay, 0f, "Tinaja");
                C(t, new Vector3(0f, h, -0.45f), new Vector3(bw, 0.08f, bw * 0.8f), Clay, 0f, "Ponchera");
                C(t, new Vector3(0f, (h - 0.04f) * 0.5f, -0.45f), new Vector3(0.12f, h - 0.04f, 0.12f), w, 0f, "Pie_de_la_ponchera");
                break;
            case St.Basin.Pedestal:
                C(t, new Vector3(0f, (h - 0.05f) * 0.5f, 0.05f), new Vector3(0.16f, h - 0.05f, 0.16f), KitchenProps.White, 0f, "Pie");
                C(t, new Vector3(0f, h + 0.04f, 0f), new Vector3(bw + 0.13f, 0.12f, 0.42f), KitchenProps.White, 0f, "Lavabo");
                C(t, new Vector3(0f, h + 0.08f, -0.02f), new Vector3(bw, 0.04f, 0.28f), Water, 0f, "Agua");
                C(t, new Vector3(0f, h + 0.2f, 0.18f), new Vector3(0.03f, 0.2f, 0.03f), Chrome, 0f, "Grifo");
                C(t, new Vector3(0f, h + 0.65f, 0.22f), new Vector3(0.4f, 0.5f, 0.02f), KitchenProps.Glass, 0f, "Espejo");
                C(t, new Vector3(0.12f, h + 0.12f, 0.12f), new Vector3(0.08f, 0.04f, 0.05f), H("E8D9A6"), 0f, "Jabon");
                break;
            default:   // aguamanil: mesita con la jofaina, la jarra y la toalla
                TableTop(t, new Vector2(0.6f, 0.45f), h, w);
                C(t, new Vector3(0f, h + 0.05f, 0f), new Vector3(bw, 0.1f, bw), e.from >= 1800 ? KitchenProps.Ceramic : Brass, 45f, "Jofaina");
                C(t, new Vector3(0f, h + 0.08f, 0f), new Vector3(bw * 0.8f, 0.02f, bw * 0.8f), Water, 45f, "Agua");
                C(t, new Vector3(0.22f, h + 0.15f, 0.12f), new Vector3(0.12f, 0.26f, 0.12f), e.from >= 1800 ? KitchenProps.Ceramic : Brass, 0f, "Jarra");
                C(t, new Vector3(-0.32f, h - 0.1f, 0f), new Vector3(0.03f, 0.3f, 0.3f), Linen, 0f, "Toalla");
                break;
        }
        if (f.Insect)
        {   // el peine de antenas: un listón con sus púas, a la altura de la cabeza, junto a la jofaina
            float hy = Mathf.Clamp(f.height - 0.25f, 1.2f, 1.8f);
            C(t, new Vector3(-bw * 0.5f - 0.35f, hy * 0.5f, 0.15f), new Vector3(0.05f, hy, 0.05f), w, 0f, "Poste_del_peine");
            C(t, new Vector3(-bw * 0.5f - 0.35f, hy, 0.15f), new Vector3(0.3f, 0.04f, 0.05f), w, 0f, "Peine_de_antenas");
            for (int k = 0; k < 6; k++) C(t, new Vector3(-bw * 0.5f - 0.47f + k * 0.05f, hy - 0.05f, 0.15f), new Vector3(0.012f, 0.08f, 0.012f), Brass, 0f, "Pua");
        }
    }

    // ══ EL ALMACÉN de la casa (armario): cestos, tinajas, arcón, alacena de nicho, armario de rollos, alacena, cómoda,
    //    estantería metálica o el mueble de pared soviético.
    static void Storage(Transform t, St.Entry e, int seed)
    {
        Color w = e.wood;
        switch (e.storage)
        {
            case St.Storage.Cesto: for (int k = 0; k < 3; k++) C(t, new Vector3(-0.45f + k * 0.45f, 0.18f + (k % 2) * 0.04f, 0f), new Vector3(0.38f, 0.36f + (k % 2) * 0.08f, 0.38f), Reed, k * 20f, "Cesto"); break;
            case St.Storage.Tinaja: for (int k = 0; k < 3; k++) { C(t, new Vector3(-0.5f + k * 0.5f, 0.35f, 0f), new Vector3(0.4f, 0.7f, 0.4f), Clay, k * 15f, "Tinaja"); C(t, new Vector3(-0.5f + k * 0.5f, 0.72f, 0f), new Vector3(0.24f, 0.05f, 0.24f), Clay, 45f, "Boca"); } break;
            case St.Storage.Arcon:
                C(t, new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.5f, 0.55f), w, 0f, "Arcon");
                C(t, new Vector3(0f, 0.57f, 0f), new Vector3(1.24f, 0.06f, 0.59f), Color.Lerp(w, Color.black, 0.2f), 0f, "Tapa_del_arcon");
                for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.5f, 0.03f, 0f), new Vector3(0.12f, 0.06f, 0.5f), w, 0f, "Pie");
                C(t, new Vector3(0f, 0.45f, -0.29f), new Vector3(0.12f, 0.1f, 0.02f), Brass, 0f, "Cerradura");
                break;
            case St.Storage.Nicho:   // dolap: alacena baja con sus puertas y los cojines doblados encima
                C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.9f, 0.45f), w, 0f, "Dolap");
                for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.29f, 0.45f, -0.23f), new Vector3(0.55f, 0.8f, 0.02f), Color.Lerp(w, Brass, 0.15f), 0f, "Puerta");
                C(t, new Vector3(0f, 1.0f, 0f), new Vector3(0.9f, 0.2f, 0.4f), e.textile, 0f, "Colchas_dobladas");
                break;
            case St.Storage.Rollos: Shelf(t, seed); break;
            case St.Storage.Comoda:
                C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.0f, 0.85f, 0.5f), w, 0f, "Comoda");
                for (int k = 0; k < 3; k++) { C(t, new Vector3(0f, 0.2f + k * 0.26f, -0.26f), new Vector3(0.92f, 0.22f, 0.02f), Color.Lerp(w, Color.black, 0.15f), 0f, "Cajon"); C(t, new Vector3(0f, 0.2f + k * 0.26f, -0.28f), new Vector3(0.1f, 0.03f, 0.02f), Brass, 0f, "Tirador"); }
                Lamp(t, e, new Vector3(0.3f, 0.88f, 0f), false);
                break;
            case St.Storage.Estanteria: Shelf(t, seed); break;
            case St.Storage.Stenka:   // mueble de pared: armarios abajo, vitrina con la vajilla y los libros arriba
                C(t, new Vector3(0f, 0.4f, 0f), new Vector3(2.0f, 0.8f, 0.45f), w, 0f, "Armarios_bajos");
                C(t, new Vector3(0f, 1.25f, 0.05f), new Vector3(2.0f, 0.9f, 0.35f), w, 0f, "Vitrina");
                C(t, new Vector3(0f, 1.25f, -0.13f), new Vector3(1.0f, 0.8f, 0.02f), KitchenProps.Glass, 0f, "Cristal");
                for (int k = 0; k < 4; k++) C(t, new Vector3(-0.35f + k * 0.2f, 1.3f, 0f), new Vector3(0.1f, 0.2f, 0.1f), k % 2 == 0 ? KitchenProps.Ceramic : H("8A3A30"), 0f, "Vajilla");
                break;
            default:   // alacena con puertas
                C(t, new Vector3(0f, 0.8f, 0f), new Vector3(1.1f, 1.6f, 0.5f), w, 0f, "Alacena");
                for (int k = -1; k <= 1; k += 2) { C(t, new Vector3(k * 0.27f, 0.8f, -0.26f), new Vector3(0.5f, 1.5f, 0.02f), Color.Lerp(w, Color.black, 0.12f), 0f, "Puerta"); C(t, new Vector3(k * 0.06f, 0.85f, -0.28f), new Vector3(0.02f, 0.12f, 0.02f), Brass, 0f, "Tirador"); }
                break;
        }
    }

    // ══ LA COCINA: el hogar del estilo y los utensilios (lo que el trabajador necesita) ══════════════════════════════
    static void Kitchen(Transform t, St.Entry e, int year)
    {
        switch (e.hearth)
        {
            case St.Hearth.Suelo: GroundHearth(t, year); return;
            case St.Hearth.Tannur: case St.Hearth.Chulha: case St.Hearth.Irori: case St.Hearth.Topias: case St.Hearth.Ocak: FloorKitchen(t, e); return;
        }
        // Con encimera: el hogar del estilo en medio, los utensilios a los lados, el agua al otro extremo.
        Transform run = KitchenProps.CounterRun(t, "Encimera", Vector3.zero, 2.4f);
        Object.DestroyImmediate(run.GetComponent<BoxCollider>());
        float top = KitchenProps.CounterTop;
        Transform stove = new GameObject("Fogon").transform; stove.SetParent(t, false); stove.localPosition = new Vector3(-0.55f, 0f, 0f);
        float potY = top + 0.13f;
        switch (e.hearth)
        {
            case St.Hearth.Brasero:   // brasero (eschara) de bronce sobre su trébede, con la parrilla
                C(stove, new Vector3(0f, top + 0.06f, 0f), new Vector3(0.5f, 0.12f, 0.4f), Brass, 0f, "Brasero");
                for (int k = 0; k < 3; k++) C(stove, new Vector3(-0.12f + k * 0.12f, top + 0.13f, 0f), new Vector3(0.06f, 0.04f, 0.2f), Ember, 20f * k, "Brasa");
                C(stove, new Vector3(0f, top + 0.16f, 0f), new Vector3(0.46f, 0.01f, 0.36f), Iron, 0f, "Parrilla");
                potY = top + 0.17f; break;
            case St.Hearth.Caldero:   // hogar de piedra con morillos, el caldero colgado de su pescante y el espetón
                for (int k = 0; k < 4; k++) C(stove, Quaternion.Euler(0f, k * 90f, 0f) * new Vector3(0f, top + 0.08f, 0.22f), new Vector3(0.5f, 0.16f, 0.08f), Stone, k * 90f, "Piedra_del_hogar");
                C(stove, new Vector3(0f, top + 0.03f, 0f), new Vector3(0.36f, 0.04f, 0.36f), Ash, 0f, "Ceniza");
                for (int k = 0; k < 3; k++) C(stove, new Vector3(-0.08f + k * 0.08f, top + 0.06f, 0.02f * k), new Vector3(0.05f, 0.05f, 0.2f), Ember, 20f * k, "Brasa");
                C(stove, new Vector3(0.3f, top + 0.5f, 0.25f), new Vector3(0.05f, 1.0f, 0.05f), Iron, 0f, "Pescante");
                C(stove, new Vector3(0.12f, top + 0.95f, 0.15f), new Vector3(0.4f, 0.04f, 0.04f), Iron, 30f, "Brazo_del_pescante");
                C(stove, new Vector3(0f, top + 0.45f, 0.05f), new Vector3(0.015f, 0.5f, 0.015f), Iron, 0f, "Cadena");
                C(stove, new Vector3(0f, top + 0.22f, 0.05f), new Vector3(0.3f, 0.24f, 0.3f), Iron, 0f, "Caldero");
                C(stove, new Vector3(0f, top + 0.2f, -0.18f), new Vector3(0.7f, 0.02f, 0.02f), Iron, 0f, "Espeton");
                potY = top + 0.24f; break;
            case St.Hearth.KangWok:   // cocina de ladrillo (la que calienta el kang) con su gran wok empotrado
                C(stove, new Vector3(0f, top + 0.1f, 0f), new Vector3(0.7f, 0.2f, 0.6f), Brick, 0f, "Cocina_de_ladrillo");
                C(stove, new Vector3(0f, top + 0.21f, 0f), new Vector3(0.5f, 0.04f, 0.5f), Iron, 45f, "Wok_empotrado");
                C(stove, new Vector3(0f, top - 0.2f, -0.31f), new Vector3(0.3f, 0.2f, 0.02f), Ash, 0f, "Boca_del_fuego");
                potY = top + 0.23f; break;
            case St.Hearth.Kachelofen:   // estufa de azulejos (calienta la Stube) con su placa de cocinar
                C(stove, new Vector3(-0.1f, top + 0.45f, 0.1f), new Vector3(0.7f, 0.9f, 0.5f), H("2E6A5A"), 0f, "Estufa_de_azulejos");
                for (int k = 0; k < 3; k++) C(stove, new Vector3(-0.1f, top + 0.15f + k * 0.3f, -0.16f), new Vector3(0.72f, 0.02f, 0.02f), H("E8E0C8"), 0f, "Junta");
                C(stove, new Vector3(0.1f, top + 0.02f, -0.1f), new Vector3(0.6f, 0.04f, 0.4f), Iron, 0f, "Placa");
                potY = top + 0.06f; break;
            case St.Hearth.Hierro:   // cocina económica de hierro (leña o carbón)
                C(stove, new Vector3(0f, top + 0.02f, 0f), new Vector3(0.7f, 0.04f, 0.6f), Iron, 0f, "Chapa");
                for (int k = -1; k <= 1; k += 2) C(stove, new Vector3(k * 0.16f, top + 0.045f, 0f), new Vector3(0.2f, 0.01f, 0.2f), H("555559"), 45f, "Aro");
                C(stove, new Vector3(0f, 0.55f, -0.33f), new Vector3(0.36f, 0.24f, 0.02f), H("2A2A2C"), 0f, "Puerta_del_fuego");
                C(stove, new Vector3(0.12f, 0.55f, -0.35f), new Vector3(0.05f, 0.03f, 0.03f), Brass, 0f, "Tirador");
                C(stove, new Vector3(0f, top + 0.6f, 0.28f), new Vector3(0.12f, 1.2f, 0.12f), Iron, 0f, "Tubo");
                break;
            case St.Hearth.GasWok:   // fogón de dos fuegos con el wok en el de la izquierda (Hong Kong)
                C(stove, new Vector3(0f, top + 0.05f, 0f), new Vector3(0.7f, 0.1f, 0.55f), H("6B6E72"), 0f, "Fogon_de_gas");
                for (int k = -1; k <= 1; k += 2) { C(stove, new Vector3(k * 0.17f, top + 0.11f, 0f), new Vector3(0.14f, 0.02f, 0.14f), Iron, 45f, "Quemador"); C(stove, new Vector3(k * 0.17f, top + 0.05f, -0.28f), new Vector3(0.04f, 0.04f, 0.03f), Ash, 0f, "Mando"); }
                Transform wok = new GameObject("Wok").transform; wok.SetParent(stove, false); wok.localPosition = new Vector3(-0.17f, top + 0.14f, 0f);
                C(wok, new Vector3(0f, 0.02f, 0f), new Vector3(0.18f, 0.03f, 0.18f), Iron, 0f, "Fondo");
                C(wok, new Vector3(0f, 0.06f, 0f), new Vector3(0.34f, 0.05f, 0.34f), Iron, 45f, "Cuenco");
                C(wok, new Vector3(0f, 0.08f, -0.26f), new Vector3(0.03f, 0.03f, 0.2f), WoodDark, 0f, "Mango");
                break;
            default:   // cocina de gas o eléctrica de cuatro fuegos con horno
            {
                bool gas = e.hearth == St.Hearth.Gas;
                C(stove, new Vector3(0f, top + 0.02f, 0f), new Vector3(0.62f, 0.04f, 0.58f), gas ? KitchenProps.White : H("E8E8E4"), 0f, gas ? "Encimera_de_gas" : "Placa_electrica");
                foreach (var (x, z) in Corners) C(stove, new Vector3(x * 0.15f, top + 0.045f, z * 0.14f), new Vector3(0.18f, 0.01f, 0.18f), gas ? Iron : H("2B2B2E"), gas ? 45f : 0f, gas ? "Quemador" : "Fuego_electrico");
                C(stove, new Vector3(0f, 0.5f, -0.33f), new Vector3(0.55f, 0.4f, 0.02f), KitchenProps.Black, 0f, "Puerta_del_horno");
                for (int k = 0; k < 4; k++) C(stove, new Vector3(-0.2f + k * 0.13f, 0.8f, -0.34f), new Vector3(0.04f, 0.04f, 0.03f), Chrome, 0f, "Mando");
                break;
            }
        }
        KitchenProps.Pan(t, "Sarten", new Vector3(-0.38f, potY, 0.05f), 0f);
        KitchenProps.Pot(t, "Olla", new Vector3(-0.72f, potY, 0.05f));
        Utensils(t, top, 0f);
        if (e.from >= 1880 && e.hearth != St.Hearth.Kachelofen)
        {
            C(t, new Vector3(0.62f, top - 0.04f, 0.12f), new Vector3(0.4f, 0.1f, 0.3f), KitchenProps.Steel, 0f, "Pila");
            C(t, new Vector3(0.62f, top + 0.15f, 0.27f), new Vector3(0.03f, 0.3f, 0.03f), KitchenProps.Steel, 0f, "Grifo");
        }
        else
        {
            C(t, new Vector3(0.62f, top + 0.08f, 0.12f), new Vector3(0.36f, 0.16f, 0.3f), WoodDark, 0f, "Barreno");
            C(t, new Vector3(0.62f, top + 0.12f, 0.12f), new Vector3(0.3f, 0.02f, 0.24f), Water, 0f, "Agua");
        }
    }

    // Tabla y cuchillo, cucharones, cuencos y platos (sobre la encimera, o en la estera junto al hogar del suelo).
    static void Utensils(Transform t, float y, float dz)
    {
        KitchenProps.BoardAndKnife(t, "Tabla_y_cuchillo", new Vector3(0.3f, y, -0.05f + dz), 0f);
        KitchenProps.UtensilCrock(t, "Cucharones", new Vector3(0.95f, y, 0.18f + dz));
        for (int k = 0; k < 2; k++) KitchenProps.Bowl(t, "Cuenco", new Vector3(0.75f + k * 0.18f, y, -0.15f + dz));
        for (int k = 0; k < 3; k++) KitchenProps.Plate(t, "Plato", new Vector3(1.0f, y + k * 0.018f, -0.15f + dz));
    }

    // LOS HOGARES DEL SUELO (se cocina agachado o sentado): el tannur de barro (pan pegado a su pared), el chulha en U con su
    // tawa, el irori hundido en su marco con la tetera colgada de su trébede, las topias (tres piedras) con el budare, el ocak de
    // obra con el mangal. Al lado, una estera o una tabla baja con los utensilios y el agua.
    static void FloorKitchen(Transform t, St.Entry e)
    {
        Transform h = new GameObject("Fogon").transform; h.SetParent(t, false); h.localPosition = new Vector3(-0.6f, 0f, 0f);
        float potY = 0.3f;
        switch (e.hearth)
        {
            case St.Hearth.Tannur:
                for (int k = 0; k < 4; k++) C(h, new Vector3(0f, 0.15f + k * 0.2f, 0f), new Vector3(0.8f - k * 0.08f, 0.2f, 0.8f - k * 0.08f), Clay, k * 22f, "Tannur");
                C(h, new Vector3(0f, 0.92f, 0f), new Vector3(0.36f, 0.04f, 0.36f), Ash, 0f, "Boca_del_tannur");
                C(h, new Vector3(0f, 0.15f, -0.41f), new Vector3(0.2f, 0.16f, 0.02f), Ember, 0f, "Tiro");
                C(h, new Vector3(0.3f, 0.6f, -0.33f), new Vector3(0.18f, 0.02f, 0.14f), H("D8B070"), new Vector3(70f, 0f, 0f), "Pan_pegado");
                potY = 0.1f; h.localPosition += new Vector3(0f, 0f, 0.2f);
                break;
            case St.Hearth.Chulha:
                C(h, new Vector3(0f, 0.12f, 0.15f), new Vector3(0.6f, 0.24f, 0.15f), Clay, 0f, "Chulha");
                for (int s = -1; s <= 1; s += 2) C(h, new Vector3(s * 0.23f, 0.12f, 0f), new Vector3(0.14f, 0.24f, 0.45f), Clay, 0f, "Chulha");
                C(h, new Vector3(0f, 0.05f, -0.05f), new Vector3(0.3f, 0.06f, 0.3f), Ember, 0f, "Brasa");
                C(h, new Vector3(0f, 0.26f, 0f), new Vector3(0.36f, 0.02f, 0.36f), Iron, 45f, "Tawa");
                potY = 0.27f; break;
            case St.Hearth.Irori:
                C(h, new Vector3(0f, 0.06f, 0f), new Vector3(1.0f, 0.12f, 1.0f), e.wood, 0f, "Marco_del_irori");
                C(h, new Vector3(0f, 0.07f, 0f), new Vector3(0.8f, 0.12f, 0.8f), Ash, 0f, "Ceniza");
                C(h, new Vector3(0f, 0.15f, 0f), new Vector3(0.12f, 0.06f, 0.2f), Ember, 30f, "Brasa");
                for (int k = 0; k < 3; k++) C(h, Quaternion.Euler(0f, k * 120f, 0f) * new Vector3(0f, 0.6f, 0.2f), new Vector3(0.04f, 1.2f, 0.04f), e.wood, Quaternion.Euler(0f, k * 120f, 0f).eulerAngles + new Vector3(-10f, 0f, 0f), "Pata_del_jizaikagi");
                C(h, new Vector3(0f, 0.75f, 0f), new Vector3(0.02f, 0.5f, 0.02f), Iron, 0f, "Jizaikagi");
                C(h, new Vector3(0f, 0.42f, 0f), new Vector3(0.24f, 0.2f, 0.24f), Iron, 0f, "Tetsubin");
                potY = 0.13f; break;
            case St.Hearth.Topias:
                for (int k = 0; k < 3; k++) C(h, Quaternion.Euler(0f, k * 120f, 0f) * new Vector3(0f, 0.1f, 0.18f), new Vector3(0.18f, 0.2f, 0.18f), Stone, k * 40f, "Topia");
                C(h, new Vector3(0f, 0.05f, 0f), new Vector3(0.25f, 0.06f, 0.25f), Ember, 0f, "Brasa");
                C(h, new Vector3(0f, 0.22f, 0f), new Vector3(0.45f, 0.02f, 0.45f), Clay, 45f, "Budare");
                potY = 0.23f; break;
            default:   // ocak de obra y el mangal
                C(h, new Vector3(0f, 0.3f, 0.1f), new Vector3(0.9f, 0.6f, 0.6f), H("D8CFBE"), 0f, "Ocak");
                C(h, new Vector3(0f, 0.25f, -0.21f), new Vector3(0.4f, 0.3f, 0.02f), Ash, 0f, "Boca_del_ocak");
                C(h, new Vector3(0f, 0.18f, -0.22f), new Vector3(0.3f, 0.05f, 0.02f), Ember, 0f, "Brasas");
                C(h, new Vector3(0.75f, 0.15f, -0.2f), new Vector3(0.4f, 0.3f, 0.4f), Copper, 45f, "Mangal");
                C(h, new Vector3(0.75f, 0.31f, -0.2f), new Vector3(0.3f, 0.02f, 0.3f), Ember, 0f, "Brasa");
                potY = 0.61f; break;
        }
        KitchenProps.Pot(t, "Olla", new Vector3(-0.6f, potY, 0.05f));
        KitchenProps.Pan(t, "Sarten", new Vector3(-0.2f, 0.02f, -0.5f), 30f);
        // La tabla baja de trabajo con los utensilios, y el agua.
        Transform board = new GameObject("Tabla_de_trabajo").transform; board.SetParent(t, false); board.localPosition = new Vector3(0.1f, 0f, 0f);
        C(board, new Vector3(0.55f, 0.12f, 0f), new Vector3(1.0f, 0.04f, 0.6f), e.wood, 0f, "Tabla_baja");
        for (int k = -1; k <= 1; k += 2) C(board, new Vector3(0.55f + k * 0.4f, 0.05f, 0f), new Vector3(0.06f, 0.1f, 0.5f), e.wood, 0f, "Pie");
        Utensils(board, 0.14f, 0f);
        C(t, new Vector3(1.4f, 0.3f, 0.2f), new Vector3(0.4f, 0.6f, 0.4f), Clay, 0f, "Tinaja_de_agua");
    }

    // ══ LOS MUEBLES DE TRABAJO (sin cambios de estilo: el horno, la barra, el laboratorio, el taller, el puesto…) ═════════
    static void Oven(Transform t)
    {
        C(t, new Vector3(0f, 0.45f, 0.2f), new Vector3(1.4f, 0.9f, 1.1f), Clay, 0f, "Base");
        for (int k = 0; k < 3; k++) C(t, new Vector3(0f, 1.0f + k * 0.18f, 0.2f), new Vector3(1.2f - k * 0.3f, 0.2f, 0.95f - k * 0.25f), Clay, k * 15f, "Boveda");
        C(t, new Vector3(0f, 1.0f, -0.36f), new Vector3(0.45f, 0.3f, 0.02f), Ash, 0f, "Boca");
        C(t, new Vector3(0f, 0.92f, -0.37f), new Vector3(0.35f, 0.04f, 0.02f), Ember, 0f, "Brasas");
        // Mesa de amasar al lado, con la pala del horno y dos hogazas.
        Transform tb = new GameObject("Mesa_de_amasar").transform; tb.SetParent(t, false); tb.localPosition = new Vector3(1.3f, 0f, 0f);
        TableTop(tb, new Vector2(0.9f, 0.7f), 0.85f, Wood);
        C(tb, new Vector3(0f, 0.9f, 0f), new Vector3(0.12f, 0.04f, 1.4f), Wood, 80f, "Pala_del_horno");
        for (int k = 0; k < 2; k++) C(tb, new Vector3(-0.2f + k * 0.35f, 0.92f, 0.15f), new Vector3(0.22f, 0.1f, 0.16f), H("C88B5A"), 15f, "Hogaza");
    }

    static void Bar(Transform t)
    {
        C(t, new Vector3(0f, 0.5f, 0f), new Vector3(3f, 1.0f, 0.55f), WoodDark, 0f, "Barra");
        C(t, new Vector3(0f, 1.02f, -0.04f), new Vector3(3.06f, 0.05f, 0.65f), H("D9D2C2"), 0f, "Tablero");
        C(t, new Vector3(0f, 0.1f, -0.3f), new Vector3(3f, 0.04f, 0.04f), Brass, 0f, "Reposapies");
        for (int k = 0; k < 4; k++)
        {
            float x = -1.1f + k * 0.73f;
            Transform s = new GameObject("Taburete").transform; s.SetParent(t, false); s.localPosition = new Vector3(x, 0f, -0.7f);
            C(s, new Vector3(0f, 0.36f, 0f), new Vector3(0.06f, 0.72f, 0.06f), Iron, 0f, "Pie");
            C(s, new Vector3(0f, 0.74f, 0f), new Vector3(0.36f, 0.06f, 0.36f), H("8E2A26"), 0f, "Asiento");
            C(s, new Vector3(0f, 0.03f, 0f), new Vector3(0.36f, 0.04f, 0.36f), Iron, 45f, "Base");
            C(t, new Vector3(x, 1.08f, 0f), new Vector3(0.08f, 0.08f, 0.08f), KitchenProps.Ceramic, 0f, "Taza");
        }
        C(t, new Vector3(1.25f, 1.12f, 0.1f), new Vector3(0.18f, 0.16f, 0.18f), H("6E4B2E"), 0f, "Tetera");
        C(t, new Vector3(1.36f, 1.15f, 0.1f), new Vector3(0.08f, 0.03f, 0.03f), H("6E4B2E"), 0f, "Pitorro");
    }

    static void PlainChair(Transform parent, Vector3 at, float yaw)
    {
        Transform s = KitchenProps.Root(parent, "Silla", at, yaw);
        C(s, new Vector3(0f, 0.45f, 0f), new Vector3(0.42f, 0.04f, 0.42f), Wood, 0f, "Asiento");
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) C(s, new Vector3(x * 0.18f, 0.22f, z * 0.18f), new Vector3(0.04f, 0.44f, 0.04f), WoodDark, 0f, "Pata");
        C(s, new Vector3(0f, 0.72f, 0.19f), new Vector3(0.42f, 0.5f, 0.04f), Wood, 0f, "Respaldo");
        var mp = s.gameObject.AddComponent<MicroPiece>(); mp.kind = "silla"; mp.need = "sentarse";
    }

    static void Bench(Transform parent, Vector3 at, float len)
    {
        Transform s = new GameObject("Banco").transform; s.SetParent(parent, false); s.localPosition = at;
        C(s, new Vector3(0f, 0.44f, 0f), new Vector3(len, 0.05f, 0.32f), Wood, 0f, "Asiento");
        for (int k = -1; k <= 1; k += 2) C(s, new Vector3(k * (len * 0.5f - 0.1f), 0.21f, 0f), new Vector3(0.06f, 0.42f, 0.28f), WoodDark, 0f, "Pata");
        var mp = s.gameObject.AddComponent<MicroPiece>(); mp.kind = "banco"; mp.need = "sentarse";
    }

    static void LabBench(Transform t)
    {
        TableTop(t, new Vector2(2.2f, 0.8f), 0.9f, H("3A3633"));
        C(t, new Vector3(0f, 0.4f, 0.1f), new Vector3(2.0f, 0.6f, 0.5f), WoodDark, 0f, "Cajones");
        for (int k = 0; k < 4; k++) C(t, new Vector3(-0.8f + k * 0.18f, 1.02f, 0.15f), new Vector3(0.08f, 0.22f, 0.08f), KitchenProps.Glass, 0f, "Matraz");
        C(t, new Vector3(0.1f, 0.96f, 0f), new Vector3(0.08f, 0.1f, 0.08f), Brass, 0f, "Mechero");
        C(t, new Vector3(0.1f, 1.1f, 0f), new Vector3(0.18f, 0.02f, 0.18f), Iron, 0f, "Rejilla");
        C(t, new Vector3(0.7f, 0.91f, -0.15f), new Vector3(0.3f, 0.02f, 0.22f), Paper, 10f, "Cuaderno");
        C(t, new Vector3(0.75f, 1.0f, 0.2f), new Vector3(0.25f, 0.18f, 0.2f), H("6A6A6A"), 0f, "Electrometro");
        PlainChair(t, new Vector3(0f, 0f, -0.7f), 0f);
    }

    static void WashTub(Transform t)
    {
        C(t, new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 0.7f), WoodDark, 0f, "Pila");
        C(t, new Vector3(0f, 0.57f, 0f), new Vector3(1.05f, 0.04f, 0.55f), Water, 0f, "Agua");
        C(t, new Vector3(0.3f, 0.75f, 0.2f), new Vector3(0.4f, 0.5f, 0.04f), Wood, 0f, "Tabla_de_lavar");
        for (int k = 0; k < 5; k++) C(t, new Vector3(0.3f, 0.58f + k * 0.08f, 0.18f), new Vector3(0.38f, 0.015f, 0.02f), WoodDark, 0f, "Estria");
        C(t, new Vector3(-0.85f, 0.2f, 0f), new Vector3(0.3f, 0.4f, 0.3f), Wood, 0f, "Cubo");
        C(t, new Vector3(-0.85f, 0.42f, 0f), new Vector3(0.28f, 0.04f, 0.28f), Linen, 0f, "Ropa");
    }

    static void Shelf(Transform t, int seed) { Transform s = KitchenProps.Shelving(t, "Estanteria", Vector3.zero, 0f, seed); Object.DestroyImmediate(s.GetComponent<BoxCollider>()); }

    static void Blackboard(Transform t)
    {
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.7f, 0.9f, 0.1f), new Vector3(0.05f, 1.8f, 0.05f), Wood, 0f, "Pata_del_caballete");
        C(t, new Vector3(0f, 1.0f, 0f), new Vector3(1.4f, 1.0f, 0.04f), Slate, 0f, "Pizarra");
        C(t, new Vector3(0f, 0.48f, -0.04f), new Vector3(1.3f, 0.03f, 0.08f), Wood, 0f, "Repisa");
        C(t, new Vector3(0.2f, 0.51f, -0.04f), new Vector3(0.08f, 0.015f, 0.015f), Chalk, 0f, "Tiza");
        for (int k = 0; k < 3; k++) C(t, new Vector3(-0.3f + k * 0.25f, 1.15f - k * 0.1f, -0.025f), new Vector3(0.4f - k * 0.1f, 0.015f, 0.005f), Chalk, 0f, "Escrito");
    }

    static void Workbench(Transform t, int year = 1950)
    {
        TableTop(t, new Vector2(2.0f, 0.75f), 0.9f, Wood);
        C(t, new Vector3(0f, 0.2f, 0f), new Vector3(1.9f, 0.04f, 0.6f), WoodDark, 0f, "Balda");
        C(t, new Vector3(-0.8f, 0.98f, -0.3f), new Vector3(0.2f, 0.12f, 0.15f), Iron, 0f, "Tornillo_de_banco");
        if (year >= 1840) C(t, new Vector3(-0.3f, 0.92f, 0f), new Vector3(0.3f, 0.03f, 0.04f), Iron, 20f, "Llave_inglesa");   // la llave ajustable, s. XIX
        C(t, new Vector3(0f, 0.92f, 0.1f), new Vector3(0.25f, 0.03f, 0.05f), WoodDark, -10f, "Martillo");
        if (year >= 1870) C(t, new Vector3(0.5f, 1.0f, 0.1f), new Vector3(0.55f, 0.2f, 0.04f), Iron, 0f, "Llanta_de_bicicleta");
        // El panel de la pared, con sus herramientas colgadas (vacío parecía una pantalla, en París 1898): sierra, martillo,
        // tenazas, escuadra, cinceles y un rollo de cuerda; cada una cuelga de su clavija.
        C(t, new Vector3(0f, 1.4f, 0.36f), new Vector3(1.8f, 0.8f, 0.03f), Wood, 0f, "Panel_de_herramientas");
        C(t, new Vector3(-0.6f, 1.45f, 0.33f), new Vector3(0.5f, 0.16f, 0.01f), Iron, 0f, "Sierra_colgada");
        C(t, new Vector3(-0.6f, 1.45f, 0.335f), new Vector3(0.12f, 0.14f, 0.03f), WoodDark, 0f, "Mango_de_la_sierra");
        C(t, new Vector3(-0.15f, 1.5f, 0.335f), new Vector3(0.04f, 0.32f, 0.03f), WoodDark, 0f, "Martillo_colgado");
        C(t, new Vector3(-0.15f, 1.67f, 0.33f), new Vector3(0.14f, 0.05f, 0.04f), Iron, 0f, "Cabeza_del_martillo");
        C(t, new Vector3(0.15f, 1.45f, 0.335f), new Vector3(0.03f, 0.28f, 0.02f), Iron, 8f, "Tenazas");
        C(t, new Vector3(0.45f, 1.55f, 0.335f), new Vector3(0.3f, 0.03f, 0.01f), Iron, 0f, "Escuadra");
        C(t, new Vector3(0.45f, 1.42f, 0.335f), new Vector3(0.03f, 0.26f, 0.01f), Iron, 0f, "Escuadra_brazo");
        for (int k = 0; k < 3; k++) C(t, new Vector3(0.7f + k * 0.05f, 1.25f, 0.335f), new Vector3(0.02f, 0.18f, 0.02f), k == 0 ? WoodDark : Iron, 0f, "Cincel");
        C(t, new Vector3(-0.35f, 1.2f, 0.33f), new Vector3(0.18f, 0.18f, 0.05f), Sack, 0f, "Rollo_de_cuerda");
    }

    static void Lathe(Transform t)
    {
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.6f, 0.4f, 0f), new Vector3(0.2f, 0.8f, 0.5f), Iron, 0f, "Pie");
        C(t, new Vector3(0f, 0.85f, 0f), new Vector3(1.5f, 0.1f, 0.3f), Iron, 0f, "Bancada");
        C(t, new Vector3(-0.55f, 1.02f, 0f), new Vector3(0.25f, 0.25f, 0.25f), H("4A5560"), 0f, "Cabezal");
        C(t, new Vector3(0.5f, 1.0f, 0f), new Vector3(0.15f, 0.18f, 0.18f), H("4A5560"), 0f, "Contrapunto");
        C(t, new Vector3(0f, 1.02f, 0f), new Vector3(0.8f, 0.04f, 0.04f), KitchenProps.Steel, 0f, "Pieza");
        C(t, new Vector3(-0.75f, 1.0f, 0f), new Vector3(0.05f, 0.45f, 0.45f), Iron, 0f, "Volante");
    }

    static void WindTunnel(Transform t)
    {
        C(t, new Vector3(0f, 0.95f, 0f), new Vector3(0.5f, 0.05f, 1.8f), Wood, 0f, "Tapa");
        C(t, new Vector3(0f, 0.75f, 0f), new Vector3(0.5f, 0.05f, 1.8f), Wood, 0f, "Fondo");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.25f, 0.85f, 0f), new Vector3(0.03f, 0.25f, 1.8f), Wood, 0f, "Lado");
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) C(t, new Vector3(x * 0.22f, 0.36f, z * 0.8f), new Vector3(0.05f, 0.72f, 0.05f), WoodDark, 0f, "Pata");
        C(t, new Vector3(0f, 0.85f, -0.95f), new Vector3(0.45f, 0.45f, 0.06f), Iron, 45f, "Ventilador");
        C(t, new Vector3(0f, 0.85f, 0.2f), new Vector3(0.2f, 0.01f, 0.06f), Brass, 0f, "Balanza_de_sustentacion");
    }

    static void WoodenDummy(Transform t)
    {
        C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.3f, 1.7f, 0.3f), Wood, 0f, "Tronco");
        C(t, new Vector3(0f, 0.85f, 0f), new Vector3(0.3f, 1.7f, 0.3f), Wood, 45f, "Tronco");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.12f, 1.25f, -0.35f), new Vector3(0.06f, 0.06f, 0.4f), WoodDark, k * 10f, "Brazo_alto");
        C(t, new Vector3(0f, 1.0f, -0.35f), new Vector3(0.06f, 0.06f, 0.4f), WoodDark, 0f, "Brazo_medio");
        C(t, new Vector3(0f, 0.35f, -0.3f), new Vector3(0.07f, 0.55f, 0.07f), WoodDark, 0f, "Pierna");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.45f, 1.0f, 0.15f), new Vector3(0.06f, 2.0f, 0.06f), WoodDark, 0f, "Poste_del_bastidor");
        for (int k = 0; k < 2; k++) C(t, new Vector3(0f, 1.5f - k * 0.9f, 0.15f), new Vector3(0.96f, 0.06f, 0.06f), WoodDark, 0f, "Travesano");
    }

    static void Altar(Transform t)
    {
        C(t, new Vector3(0f, 0.45f, 0f), new Vector3(1.4f, 0.9f, 0.7f), Stone, 0f, "Mesa_de_ofrendas");
        C(t, new Vector3(0f, 0.92f, 0f), new Vector3(1.5f, 0.05f, 0.8f), H("D8CFBE"), 0f, "Losa");
        for (int k = 0; k < 3; k++) C(t, new Vector3(-0.45f + k * 0.45f, 1.05f, 0f), new Vector3(0.18f, 0.22f, 0.18f), Clay, 0f, "Vasija");
        C(t, new Vector3(0.6f, 0.98f, 0.2f), new Vector3(0.06f, 0.08f, 0.06f), Ember, 0f, "Lamparilla");
    }

    static void Stall(Transform t, int seed)
    {
        TableTop(t, new Vector2(1.8f, 0.9f), 0.85f, Wood);
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.88f, 1.0f, 0.42f), new Vector3(0.05f, 2.0f, 0.05f), WoodDark, 0f, "Poste");
        C(t, new Vector3(0f, 2.0f, 0.2f), new Vector3(1.9f, 0.03f, 0.6f), H("B5462F"), 0f, "Toldo");
        var rnd = new System.Random(seed);
        for (int k = 0; k < 5; k++)
        {
            Color c = new[] { H("E8C35A"), H("C8763E"), H("8CB070"), H("D9CBA0"), Clay }[rnd.Next(5)];
            C(t, new Vector3(-0.7f + k * 0.35f, 0.92f, 0f), new Vector3(0.28f, 0.14f, 0.28f), Sack, 0f, "Cesto");
            C(t, new Vector3(-0.7f + k * 0.35f, 1.0f, 0f), new Vector3(0.22f, 0.04f, 0.22f), c, 0f, "Mercancia");
        }
        Bench(t, new Vector3(0f, 0f, 0.75f), 1.2f);
    }

    static void Granary(Transform t)
    {
        for (int k = 0; k < 4; k++) C(t, new Vector3(-0.5f, 0.3f + k * 0.4f, 0f), new Vector3(1.2f - k * 0.15f, 0.4f, 1.2f - k * 0.15f), Clay, k * 22f, "Silo");
        C(t, new Vector3(-0.5f, 1.9f, 0f), new Vector3(0.5f, 0.12f, 0.5f), WoodDark, 0f, "Tapa");
        C(t, new Vector3(0.75f, 0.12f, 0f), new Vector3(0.6f, 0.24f, 0.4f), Stone, 0f, "Molino_de_mano");
        C(t, new Vector3(0.75f, 0.28f, 0f), new Vector3(0.3f, 0.08f, 0.2f), Stone, 0f, "Mano_del_molino");
        C(t, new Vector3(0.75f, 0.25f, -0.35f), new Vector3(0.3f, 0.2f, 0.3f), Sack, 0f, "Saco_de_cebada");
    }

    static void PotterWheel(Transform t)
    {
        C(t, new Vector3(0f, 0.2f, 0f), new Vector3(0.5f, 0.4f, 0.5f), WoodDark, 0f, "Pie");
        C(t, new Vector3(0f, 0.42f, 0f), new Vector3(0.6f, 0.04f, 0.6f), Wood, 45f, "Plato_del_torno");
        C(t, new Vector3(0f, 0.52f, 0f), new Vector3(0.24f, 0.16f, 0.24f), Clay, 0f, "Vasija");
        Bench(t, new Vector3(0f, 0f, -0.6f), 0.6f);
        for (int k = 0; k < 4; k++) C(t, new Vector3(0.55f, 0.06f + k * 0.07f, 0.2f), new Vector3(0.2f, 0.07f, 0.2f), Clay, 0f, "Cuencos_de_borde_biselado");
    }

    static void Sandbags(Transform t)
    {
        for (int row = 0; row < 4; row++)
            for (int k = 0; k < 6; k++)
                C(t, new Vector3(-2.5f + k * 1.0f + (row % 2) * 0.5f, 0.12f + row * 0.24f, 0f), new Vector3(0.95f, 0.24f, 0.5f), Sack, (k + row) % 2 == 0 ? 2f : -2f, "Saco_terrero");
    }

    // POZO: brocal de piedra, agua al fondo, dos postes con travesaño, torno con soga y cubo, y una pila al lado.
    static void Well(Transform t)
    {
        for (int k = 0; k < 8; k++) C(t, Quaternion.Euler(0f, k * 45f, 0f) * new Vector3(0f, 0.4f, 0.55f), new Vector3(0.45f, 0.8f, 0.22f), Stone, k * 45f, "Brocal");
        C(t, new Vector3(0f, 0.15f, 0f), new Vector3(0.8f, 0.04f, 0.8f), Water, 22f, "Agua");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.6f, 1.1f, 0f), new Vector3(0.1f, 2.2f, 0.1f), WoodDark, 0f, "Poste");
        C(t, new Vector3(0f, 1.9f, 0f), new Vector3(1.3f, 0.12f, 0.12f), Wood, 0f, "Torno");
        C(t, new Vector3(0.68f, 1.9f, -0.12f), new Vector3(0.04f, 0.3f, 0.04f), WoodDark, 0f, "Manivela");
        C(t, new Vector3(0f, 1.45f, 0f), new Vector3(0.02f, 0.9f, 0.02f), Straw, 0f, "Soga");
        C(t, new Vector3(0f, 0.92f, 0f), new Vector3(0.24f, 0.26f, 0.24f), Wood, 0f, "Cubo");
        C(t, new Vector3(1.0f, 0.2f, 0f), new Vector3(0.5f, 0.4f, 0.8f), Stone, 0f, "Pila");
        C(t, new Vector3(1.0f, 0.38f, 0f), new Vector3(0.4f, 0.03f, 0.7f), Water, 0f, "Agua_de_la_pila");
    }

    // TELAR de bajo lizo: bastidor, enjulio de urdimbre y de tela, los hilos, el lizo, el peine, los pedales y el banco del tejedor.
    static void Loom(Transform t)
    {
        foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) C(t, new Vector3(x * 0.7f, 0.75f, z * 0.6f), new Vector3(0.08f, 1.5f, 0.08f), WoodDark, 0f, "Pie");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(0f, 0.85f, k * 0.6f), new Vector3(1.5f, 0.1f, 0.1f), Wood, 0f, k < 0 ? "Enjulio_de_tela" : "Enjulio_de_urdimbre");
        for (int k = 0; k < 12; k++) C(t, new Vector3(-0.55f + k * 0.1f, 0.88f, 0f), new Vector3(0.01f, 0.01f, 1.2f), Linen, 0f, "Hilo_de_urdimbre");
        C(t, new Vector3(0f, 0.86f, -0.35f), new Vector3(1.2f, 0.02f, 0.5f), H("A0523A"), 0f, "Tela_tejida");
        C(t, new Vector3(0f, 1.3f, 0.1f), new Vector3(1.3f, 0.5f, 0.04f), Wood, 0f, "Lizos");
        C(t, new Vector3(0f, 1.0f, -0.15f), new Vector3(1.3f, 0.25f, 0.05f), WoodDark, 0f, "Peine");
        for (int k = -1; k <= 1; k += 2) C(t, new Vector3(k * 0.2f, 0.06f, 0.2f), new Vector3(0.12f, 0.04f, 0.6f), Wood, 0f, "Pedal");
        C(t, new Vector3(0.3f, 0.9f, -0.3f), new Vector3(0.25f, 0.05f, 0.06f), Wood, 0f, "Lanzadera");
        Bench(t, new Vector3(0f, 0f, -0.95f), 1.0f);
    }

    static void Mat(Transform t, string key, Vector2 size)
    {
        if (key == "bancal")
        {
            C(t, new Vector3(0f, 0.08f, 0f), new Vector3(size.x, 0.16f, size.y), Soil, 0f, "Tierra");
            for (float z = -size.y * 0.5f + 0.3f; z < size.y * 0.5f; z += 0.5f)
                for (float x = -size.x * 0.5f + 0.25f; x < size.x * 0.5f; x += 0.5f) C(t, new Vector3(x, 0.24f, z), new Vector3(0.18f, 0.16f, 0.18f), Green, (x + z) * 40f, "Mata");
            return;
        }
        Color c = key == "tatami" ? H("C9BC86") : H("9A7346");
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(size.x, 0.06f, size.y), c, 0f, key == "tatami" ? "Tatami" : "Esterilla");
        if (key == "tatami")
        {
            for (int k = 0; k < 4; k++) C(t, Quaternion.Euler(0f, k * 90f, 0f) * new Vector3(0f, 0.065f, (k % 2 == 0 ? size.y : size.x) * 0.5f - 0.04f), new Vector3(k % 2 == 0 ? size.x : size.y, 0.01f, 0.08f), H("3B4A2E"), k * 90f, "Borde");
            for (float x = -size.x * 0.5f + 1f; x < size.x * 0.5f - 0.1f; x += 1f) C(t, new Vector3(x, 0.062f, 0f), new Vector3(0.02f, 0.005f, size.y - 0.1f), H("B3A673"), 0f, "Junta");
        }
    }

    // El HOGAR EN EL SUELO de las aldeas y las primeras ciudades: anillo de piedras, brasas, la losa de asar, la piedra de moler
    // con su mano, el cuchillo de sílex sobre una piedra plana y, desde la cerámica (~7000 a. C.), la olla de barro en las brasas.
    static void GroundHearth(Transform t, int year)
    {
        for (int k = 0; k < 8; k++) C(t, Quaternion.Euler(0f, k * 45f, 0f) * new Vector3(0f, 0.08f, 0.42f), new Vector3(0.26f, 0.16f, 0.18f), Stone, k * 45f, "Piedra_del_hogar");
        C(t, new Vector3(0f, 0.03f, 0f), new Vector3(0.6f, 0.04f, 0.6f), Ash, 0f, "Ceniza");
        for (int k = 0; k < 4; k++) C(t, new Vector3(-0.15f + k * 0.1f, 0.07f, 0.05f * (k % 2)), new Vector3(0.06f, 0.06f, 0.26f), Ember, 25f * k, "Brasa");
        C(t, new Vector3(0.15f, 0.12f, -0.1f), new Vector3(0.3f, 0.04f, 0.24f), Stone, 10f, "Losa_de_asar");
        if (year >= -7000)
        {
            C(t, new Vector3(-0.15f, 0.18f, 0.1f), new Vector3(0.26f, 0.22f, 0.26f), Clay, 0f, "Olla_de_barro");
            C(t, new Vector3(-0.15f, 0.3f, 0.1f), new Vector3(0.2f, 0.03f, 0.2f), Clay, 45f, "Boca");
        }
        C(t, new Vector3(0f, 0.06f, -0.75f), new Vector3(0.5f, 0.12f, 0.3f), Stone, 0f, "Piedra_de_moler");
        C(t, new Vector3(0.05f, 0.15f, -0.75f), new Vector3(0.2f, 0.07f, 0.1f), Stone, 15f, "Mano_de_moler");
        C(t, new Vector3(0.55f, 0.05f, -0.4f), new Vector3(0.3f, 0.1f, 0.25f), Stone, -10f, "Piedra_de_cortar");
        C(t, new Vector3(0.55f, 0.11f, -0.4f), new Vector3(0.14f, 0.015f, 0.04f), H("4A4A52"), 20f, "Cuchillo_de_silex");
        C(t, new Vector3(-0.55f, 0.06f, -0.4f), new Vector3(0.22f, 0.1f, 0.22f), WoodDark, 0f, "Cuenco_de_madera");
    }
}
