using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EL ACCESO Y LA VIGILANCIA DE LAS CAVERNAS (docs/sitio-nuclear.md §7–§8): los instrumentos de la roca, la LÍNEA (tubo de súper velocidad del borde del
/// casquete a la galería) y la SALIDA DE ESCAPE, en su propio pozo. Sin colisiones, como las cavernas: quien va dentro lo lleva <see cref="SiteLine"/>.
/// </summary>
public static partial class SampleSceneBuilder
{
    const float LineBore = 5.2f;   // cabe el módulo (3 × 3,2 m de sección, 4,4 de diagonal): lo que se sacaría por aquí
    static SiteLine _nuclearLine;

    /// <summary>La vigilancia: instrumentos en las dos cavernas y en el cajón, y la holgura MEDIDA (si no llega a la regla, error).</summary>
    static CavernRock BuildRockMonitoring(Transform root, Transform reactor, Transform waste, Transform caisson)
    {
        CavernRock rock = root.gameObject.AddComponent<CavernRock>();
        Color yel = new Color(0.95f, 0.8f, 0.2f), steel = TreeSteel, dark = new Color(0.2f, 0.22f, 0.25f), blue = new Color(0.25f, 0.5f, 0.75f);
        void Instruments(Transform c, Vector3 size)
        {
            float hx = size.x * 0.5f - 1f, hy = size.y * 0.5f, hz = size.z * 0.5f;
            // Extensómetros: varillas a varias profundidades, 3 en el techo y 1 en la pared (el SFR usa de 2 y 6 m; aquí llegan a 8).
            for (int i = 0; i < 3; i++)
            {
                Transform e = KitchenProps.C(c, new Vector3(-hx * 0.6f + i * hx * 0.6f, hy + 4f, 0f), new Vector3(0.08f, 8f, 0.08f), yel, 0f, "Extensometro_varilla");
                KitchenProps.C(c, new Vector3(-hx * 0.6f + i * hx * 0.6f, hy - 0.35f, 0f), new Vector3(0.3f, 0.2f, 0.3f), dark, 0f, "Cabeza_extensometro");
                rock.extensometers.Add(e);
            }
            rock.extensometers.Add(KitchenProps.C(c, new Vector3(0f, 0f, hz + 4f), new Vector3(0.08f, 0.08f, 8f), yel, 0f, "Extensometro_varilla"));
            // Puntos de convergencia: dianas enfrentadas; se mide la distancia entre las de cada par.
            for (int i = -1; i <= 1; i += 2)
                for (int s = -1; s <= 1; s += 2)
                    rock.convergence.Add(KitchenProps.C(c, new Vector3(i * hx * 0.5f, 1f, s * (hz - 0.75f)), new Vector3(0.25f, 0.25f, 0.05f), new Color(0.9f, 0.25f, 0.2f), 0f, "Punto_convergencia"));
            // Células de carga en los bulones del techo, geófonos para los microsismos y piezómetros en la roca.
            for (int i = -1; i <= 1; i++) rock.loadCells.Add(KitchenProps.C(c, new Vector3(i * hx * 0.8f, hy + 1f, 2f), new Vector3(0.1f, 2.6f, 0.1f), steel, 0f, "Celula_carga_bulon"));
            for (int s = -1; s <= 1; s += 2) rock.geophones.Add(KitchenProps.C(c, new Vector3(s * (hx + 3f), 2f, 0f), new Vector3(0.4f, 0.4f, 0.4f), dark, 0f, "Geofono_microsismico"));
            for (int s = -1; s <= 1; s += 2) rock.piezometers.Add(KitchenProps.C(c, new Vector3(s * hx * 0.5f, hy + 6f, -3f), new Vector3(0.12f, 4f, 0.12f), blue, 0f, "Piezometro"));
            rock.flowMeters.Add(KitchenProps.C(c, new Vector3(-hx * 0.8f, -hy + 0.9f, hz - 1.4f), new Vector3(0.8f, 0.6f, 0.6f), blue, 0f, "Aforo_sumidero"));
        }
        Instruments(reactor, new Vector3(CavernRule.ReactorLength, NuclearSiteRule.CavernHeight, CavernRule.ReactorSpan));
        Instruments(waste, new Vector3(CavernRule.WasteLength, NuclearSiteRule.CavernHeight, CavernRule.WasteSpan));
        // LAS SALAS, grandes porque tienen en qué apoyarse: columnas de roca sin excavar, bulones y cables en el techo.
        Vector3 hallR = new Vector3(CavernRule.ReactorLength, NuclearSiteRule.CavernHeight, CavernRule.ReactorSpan), hallW = new Vector3(CavernRule.WasteLength, NuclearSiteRule.CavernHeight, CavernRule.WasteSpan);
        BuildHallSupport(rock, reactor, hallR);
        BuildHallSupport(rock, waste, hallW);
        // EL CAJÓN ya no apoya en nada: con el lecho a 500 m cuelga del casquete y lo atan tendones, como una plataforma de patas tensadas. Así tampoco le afecta que el sedimento se asiente.
        if (caisson != null)
        {
            float half = 8f, under = -NuclearSiteRule.CaissonHeight * 0.5f, len = caisson.position.y + under - NuclearSiteRule.SeabedY;
            for (int a = -1; a <= 1; a += 2)
                for (int b = -1; b <= 1; b += 2)
                {
                    KitchenProps.C(caisson, new Vector3(a * half * 0.8f, under - len * 0.5f, b * half * 0.8f), new Vector3(0.6f, len, 0.6f), steel, 0f, "Tendon_cajon");
                    KitchenProps.C(caisson, new Vector3(a * half * 0.8f, under - len + 1f, b * half * 0.8f), new Vector3(3f, 2f, 3f), dark, 0f, "Ancla_lecho");
                    rock.caissonGauges.Add(KitchenProps.C(caisson, new Vector3(a * half * 0.8f, under - 0.5f, b * half * 0.8f), new Vector3(0.9f, 1f, 0.9f), yel, 0f, "Celula_tendon"));
                }
            for (int s = -1; s <= 1; s += 2) rock.caissonGauges.Add(KitchenProps.C(caisson, new Vector3(s * 4f, NuclearSiteRule.CaissonHeight * 0.5f + 0.25f, 0f), new Vector3(0.4f, 0.5f, 0.4f), dark, 0f, "Inclinometro_cajon"));
        }
        float cr = Clearance(reactor, hallR, "Modulo_reactor", "Tapa_trebol", "Tubos_calor");
        float cw = Clearance(waste, hallW, "Contenedor_residuos");
        rock.clearanceM = Mathf.Min(cr, cw);
        if (rock.clearanceM < CavernRule.ClearanceM) Debug.LogError($"[Sitio nuclear] Holgura de {rock.clearanceM:0.0} m en las cavernas (pide ≥ {CavernRule.ClearanceM:0} m): reactor {cr:0.0} m, residuos {cw:0.0} m.");
        return rock;
    }

    // Una sala con apoyos: 4 columnas de roca del suelo al techo, cada una con su célula; bulones cada 6 m y cables más largos sobre la luz central.
    static void BuildHallSupport(CavernRock rock, Transform c, Vector3 size)
    {
        Color stone = new Color(0.45f, 0.44f, 0.42f), steel = TreeSteel, dark = new Color(0.2f, 0.22f, 0.25f);
        float hy = size.y * 0.5f, inner = size.y - 1.2f, cs = CavernRule.ColumnSize;
        foreach (Vector2 p in CavernRule.Columns)
        {
            KitchenProps.C(c, new Vector3(p.x, 0f, p.y), new Vector3(cs, inner, cs), stone, 0f, "Columna_roca");
            rock.loadCells.Add(KitchenProps.C(c, new Vector3(p.x, 1f, p.y - cs * 0.5f - 0.1f), new Vector3(0.5f, 0.5f, 0.2f), dark, 0f, "Celula_columna"));
        }
        for (float x = -size.x * 0.5f + 3f; x < size.x * 0.5f; x += 6f)
            for (float z = -size.z * 0.5f + 2f; z < size.z * 0.5f; z += 6f)
                KitchenProps.C(c, new Vector3(x, hy + 1.5f, z), new Vector3(0.06f, 3.6f, 0.06f), steel, 0f, "Bulon_techo");
        for (int a = -1; a <= 1; a += 2)
            for (int b = -1; b <= 1; b += 2)
                KitchenProps.C(c, new Vector3(a * 3.5f, hy + 5f, b * 3.5f), new Vector3(0.1f, 10f, 0.1f), steel, 0f, "Cable_anclaje");
    }

    // La holgura de verdad: del equipo al forro, por los lados y por arriba (el suelo no cuenta: el equipo apoya en él), y a las columnas.
    static float Clearance(Transform cav, Vector3 size, params string[] pieces)
    {
        Vector3 inner = (size - Vector3.one * 1.2f) * 0.5f;
        float best = float.MaxValue, half = CavernRule.ColumnSize * 0.5f;
        foreach (Transform t in cav.GetComponentsInChildren<Transform>(true).Where(x => pieces.Contains(x.name)))
        {
            Vector3 c = cav.InverseTransformPoint(t.position), h = t.localScale * 0.5f;
            best = Mathf.Min(best, inner.x - Mathf.Abs(c.x) - h.x, inner.z - Mathf.Abs(c.z) - h.z, inner.y - c.y - h.y);
            foreach (Vector2 k in CavernRule.Columns)
                best = Mathf.Min(best, Mathf.Max(Mathf.Abs(c.x - k.x) - h.x - half, Mathf.Abs(c.z - k.y) - h.z - half));
        }
        return best;
    }

    /// <summary>La línea y la salida de escape. <paramref name="gallery"/>: el centro de la galería, en el pilar de roca entre las cavernas.</summary>
    static SiteLine BuildCavernLine(Transform root, Helix hx, Transform poolCaisson, Vector3 well, Vector3 gallery)
    {
        Vector3 site = hx.site;
        Color conc = new Color(0.6f, 0.6f, 0.58f), steel = TreeSteel, dark = new Color(0.25f, 0.27f, 0.3f), yel = new Color(0.95f, 0.8f, 0.2f), green = new Color(0.25f, 0.7f, 0.4f);
        // ── ARRIBA: en el borde del casquete, a lo largo de X y a la altura de la galería. Las cavernas están a ±Z de ella: el tubo tiene que salir por el PILAR
        //    de roca (en Z se metería por el techo de una sala). Con 500 m de mar no hay sitio en el casquete para un funicular: sale casi vertical, unos 10° ──
        float edge = NuclearCapHalf - 9f, gz = Mathf.Clamp(gallery.z - site.z, -edge + 12f, edge - 12f);
        Vector3 top = new Vector3(site.x + (gallery.x >= site.x ? -edge : edge), site.y, site.z + gz);
        Vector3 foot = gallery + Vector3.down * 1.2f;   // (el piso del carro, en la galería)
        Vector3 axis = (foot - top).normalized, flat = new Vector3(axis.x, 0f, axis.z).normalized, side = Vector3.Cross(Vector3.up, flat);
        float len = Vector3.Distance(top, foot);

        Transform st = new GameObject("Linea_cavernas").transform; st.SetParent(root, false); st.SetPositionAndRotation(top, Quaternion.LookRotation(-flat));
        SiteLine line = st.gameObject.AddComponent<SiteLine>();
        BoxCollider trig = st.gameObject.AddComponent<BoxCollider>(); trig.isTrigger = true; trig.center = new Vector3(0f, 1f, 0f); trig.size = new Vector3(3f, 2f, 3f);
        // La caseta y su esclusa (las estaciones cerradas evitan lo que empeoró Kaprun: una puerta abierta tiró del fuego), y el motor, FUERA del tubo.
        KitchenProps.C(st, new Vector3(0f, 0.05f, 1.2f), new Vector3(4.4f, 0.1f, 4f), dark, 0f, "Anden_linea");
        for (int s = -1; s <= 1; s += 2)
            for (int k = 0; k < 2; k++) KitchenProps.C(st, new Vector3(s * 2.2f, 1.6f, -1f + k * 4f), new Vector3(0.2f, 3.2f, 0.2f), steel, 0f, "Poste_caseta");
        KitchenProps.C(st, new Vector3(0f, 3.3f, 1f), new Vector3(5f, 0.2f, 5.4f), conc, 0f, "Techo_caseta");
        KitchenProps.C(st, new Vector3(0f, 1.3f, 3f), new Vector3(3f, 2.6f, 0.2f), yel, 0f, "Esclusa_estacion");
        KitchenProps.C(st, new Vector3(3.8f, 1f, 3.6f), new Vector3(2.2f, 2f, 2.2f), new Color(0.35f, 0.45f, 0.55f), 0f, "Cabrestante_electrico");
        Label(st, $"Línea de las cavernas: {SiteLineRule.OneWay(len):0} s hasta la galería, esclusas incluidas", 4f);
        // La alfombra llega aquí: una pista más de la red del edificio, en su propio soporte (bajo el casquete entero, OnDeck la tomaría por la de todo el suelo).
        Transform padHolder = new GameObject("Linea_alfombras").transform; padHolder.SetParent(root, false);
        Vector3 padAt = top + side * 7.5f - flat * 1f + Vector3.up * 0.01f;
        CarpetPad.BuildZone(padHolder, padAt, Quaternion.LookRotation(flat), CarpetPad.Kind.Edificio, hx.building, 0, $"{hx.building}#linea", SiteLine.AreaName, 2);

        // ── EL TUBO: de cristal en los primeros metros, de acero hasta el lecho y revestido en la roca. Marcos cada 12 m y dos raíles ──
        Transform tube = new GameObject("Tubo_linea").transform; tube.SetParent(root, false); tube.SetPositionAndRotation(top, Quaternion.LookRotation(axis));
        tube.position -= tube.up * (LineBore * 0.5f - 0.1f);   // el piso del tubo, bajo el carro
        float sea = (top.y - NuclearSiteRule.SeabedY) / Mathf.Max(0.01f, -axis.y);   // lo que recorre hasta el lecho
        void Shell(float z0, float z1, Color c, string name)
        {
            float mid = (z0 + z1) * 0.5f, l = z1 - z0, h = LineBore * 0.5f;
            for (int s = -1; s <= 1; s += 2)
            {
                KitchenProps.C(tube, new Vector3(s * h, 0f, mid), new Vector3(0.1f, LineBore, l), c, 0f, name);
                KitchenProps.C(tube, new Vector3(0f, s * h, mid), new Vector3(LineBore, 0.1f, l), c, 0f, name);
            }
        }
        float view = 16f / Mathf.Max(0.01f, -axis.y);   // el cristal, solo arriba, donde se ve el mar: a 50 bar, abajo, va acero
        Shell(0f, view, VirtualizationTank.GlassColor, "Tubo_cristal");
        Shell(view, sea, steel, "Tubo_acero");
        Shell(sea, len, conc, "Tubo_revestido");
        // EL ESCUDO: hormigón con dientes por donde pueden llegar las quillas. Para trozos, no para un iceberg entero (§14). Compuerta en el lecho: si se pierde lo de arriba, lo de abajo sigue seco.
        float shieldEnd = Mathf.Min(IcebergRule.KeelBand / Mathf.Max(0.01f, -axis.y), sea), wallT = 2f, ring = LineBore * 0.5f + wallT * 0.5f + 0.3f, shieldMid = (view + shieldEnd) * 0.5f, shieldLen = shieldEnd - view;
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(tube, new Vector3(s * ring, 0f, shieldMid), new Vector3(wallT, 2f * ring + wallT, shieldLen), conc, 0f, "Escudo_tubo");
            KitchenProps.C(tube, new Vector3(0f, s * ring, shieldMid), new Vector3(2f * ring + wallT, wallT, shieldLen), conc, 0f, "Escudo_tubo");
        }
        for (float z = view + 20f; z < shieldEnd; z += 40f)
            for (int s = -1; s <= 1; s += 2) KitchenProps.C(tube, new Vector3(s * (ring + 2f), 0f, z), new Vector3(2f, 2f, 3f), conc, 0f, "Diente_escudo");
        Transform gate = KitchenProps.C(tube, new Vector3(0f, 0f, sea), new Vector3(LineBore + 0.4f, LineBore + 0.4f, 0.8f), new Color(0.8f, 0.2f, 0.15f), 0f, "Compuerta_lecho");
        line.seabedGate = gate;
        KitchenProps.C(tube, new Vector3(0f, 0f, sea + 6f), new Vector3(LineBore + 8f, LineBore + 8f, 12f), conc, 0f, "Collar_lecho");
        for (float z = 3f; z < len; z += 12f)
        {
            float h = LineBore * 0.5f + 0.1f;
            for (int s = -1; s <= 1; s += 2)
            {
                KitchenProps.C(tube, new Vector3(s * h, 0f, z), new Vector3(0.2f, LineBore + 0.4f, 0.3f), steel, 0f, "Marco_tubo");
                KitchenProps.C(tube, new Vector3(0f, s * h, z), new Vector3(LineBore + 0.4f, 0.2f, 0.3f), steel, 0f, "Marco_tubo");
            }
        }
        for (int s = -1; s <= 1; s += 2) KitchenProps.C(tube, new Vector3(s * 0.8f, -LineBore * 0.5f + 0.15f, len * 0.5f), new Vector3(0.12f, 0.2f, len), dark, 0f, "Rail_linea");

        // ── EL CARRO: cabina NIVELADA, con el piso siempre plano aunque el tubo vaya inclinado; abierta hasta el pasamanos, para ver a quien va ──
        Transform car = new GameObject("Carro_linea").transform; car.SetParent(st, true); car.SetPositionAndRotation(top, Quaternion.LookRotation(flat));
        KitchenProps.C(car, new Vector3(0f, 0.04f, 0f), new Vector3(2.6f, 0.08f, 3f), dark, 0f, "Piso_carro");
        for (int s = -1; s <= 1; s += 2)
        {
            KitchenProps.C(car, new Vector3(s * 1.3f, 0.55f, 0f), new Vector3(0.08f, 1.1f, 3f), yel, 0f, "Costado_carro");
            KitchenProps.C(car, new Vector3(0f, 0.55f, s * 1.5f), new Vector3(2.6f, 1.1f, 0.08f), yel, 0f, "Frontal_carro");
        }

        // ── ABAJO: la estación de la galería, con su registrador ──
        Transform bot = new GameObject("Estacion_inferior").transform; bot.SetParent(root, false); bot.SetPositionAndRotation(foot, Quaternion.LookRotation(flat));
        Hull(KitchenProps.Root(bot, "Caja_estacion", new Vector3(0f, 2.2f, 0f)), new Vector3(9f, 4.6f, 7f), 0.3f, conc, "Revestimiento_estacion");
        KitchenProps.C(bot, new Vector3(0f, -0.05f, 0f), new Vector3(8.6f, 0.1f, 6.6f), dark, 0f, "Piso_estacion");
        KitchenProps.C(bot, new Vector3(3f, 1f, 2.8f), new Vector3(1.2f, 2f, 0.5f), green, 0f, "Registrador_roca");
        Label(bot, "Galería: registrador de la roca (fibra hasta el panel) y refugio", 3f);
        line.bottom = bot; line.car = car;

        // ── LA SEGUNDA SALIDA: su pozo y su jaula a baterías, separados del tubo y del pozo del reactor ──
        // Sigue el pilar, al otro lado de la galería que el tubo (±Z son las cavernas; hacia el tubo, el tubo), hasta fuera del tronco y del cajón.
        Vector3[] tries = { flat * 13f, flat * 18f, flat * 24f, flat * 30f, flat * 36f };
        float poolD = poolCaisson != null ? 7f + 4f : 0f;
        bool Free(Vector3 off)
        {
            Vector3 p = gallery + off, d = new Vector3(p.x - site.x, 0f, p.z - site.z);
            bool trunk = Mathf.Abs(d.x) < hx.h.OuterHalf + 3f && Mathf.Abs(d.z) < hx.h.OuterHalf + 3f;
            bool onCap = Mathf.Abs(d.x) < NuclearCapHalf - 5f && Mathf.Abs(d.z) < NuclearCapHalf - 5f;
            bool pool = poolCaisson != null && Flat2(p, poolCaisson.position) < poolD;
            return !trunk && onCap && !pool && Flat2(p, well) > 6f;
        }
        Vector3 off0 = tries.FirstOrDefault(Free);
        if (off0 == Vector3.zero) { Debug.LogError("[Sitio nuclear] No hay sitio para el pozo de escape."); off0 = tries[0]; }
        Vector3 eFoot = foot + off0, eHead = new Vector3(eFoot.x, 0.05f, eFoot.z);
        KitchenProps.C(root, root.InverseTransformPoint((eFoot + gallery + Vector3.down * 1.2f) * 0.5f + Vector3.up * 1.2f), new Vector3(2.4f, 2.4f, off0.magnitude), conc, Quaternion.LookRotation(off0), "Galeria_escape");
        Transform eB = new GameObject("Pie_escape").transform; eB.SetParent(root, false); eB.position = eFoot;
        Transform eT = new GameObject("Salida_escape").transform; eT.SetParent(root, false); eT.position = eHead;
        KitchenProps.C(root, root.InverseTransformPoint((eFoot + eHead) * 0.5f), new Vector3(2.8f, eHead.y - eFoot.y, 2.8f), conc, 0f, "Pozo_escape");
        KitchenProps.C(eT, new Vector3(0f, 0.4f, 0f), new Vector3(3.2f, 0.8f, 3.2f), green, 0f, "Tapa_escape");
        float shieldTop = -16f, shieldLow = -Mathf.Min(IcebergRule.KeelBand, -NuclearSiteRule.SeabedY);
        KitchenProps.C(root, root.InverseTransformPoint(new Vector3(eHead.x, (shieldTop + shieldLow) * 0.5f, eHead.z)), new Vector3(7f, shieldTop - shieldLow, 7f), conc, 0f, "Escudo_escape");
        // LA DETECCIÓN: radar en un mástil del casquete y sonar de quillas colgado del borde. Lo que no se ve no se remolca.
        KitchenProps.C(st, new Vector3(-5f, 5f, 3f), new Vector3(0.3f, 10f, 0.3f), steel, 0f, "Mastil_radar");
        KitchenProps.C(st, new Vector3(-5f, 10.2f, 3f), new Vector3(2.4f, 0.4f, 0.4f), dark, 0f, "Radar_hielo");
        KitchenProps.C(st, new Vector3(-5f, -6f, 3f), new Vector3(1f, 2f, 1f), yel, 0f, "Sonar_quillas");
        Label(eT, "Salida de escape: pozo propio con jaula a baterías (ni por el tubo ni por el pozo del reactor)", 2.5f);
        Transform cage = new GameObject("Jaula_escape").transform; cage.SetParent(root, false); cage.position = eFoot;
        KitchenProps.C(cage, new Vector3(0f, 1.1f, 0f), new Vector3(2f, 2.2f, 2f), yel, 0f, "Caja_jaula");
        line.escapeBottom = eB; line.escapeTop = eT; line.cage = cage;
        return line;
    }

    static float Flat2(Vector3 a, Vector3 b) { a.y = 0f; b.y = 0f; return Vector3.Distance(a, b); }

    /// <summary>La regla de la línea: menos de un minuto, esclusas incluidas, y segunda salida.</summary>
    static void ValidateCavernLine(SiteLine l)
    {
        if (l == null) return;
        var bad = new List<string>();
        if (SiteLineRule.OneWay(l.Length) >= SiteLineRule.MaxOneWay) bad.Add($"{SiteLineRule.OneWay(l.Length):0.0} s por viaje (pide < {SiteLineRule.MaxOneWay:0} s)");
        if (l.escapeBottom == null || l.escapeTop == null) bad.Add("sin salida de escape");
        if (bad.Count > 0) Debug.LogError($"[Sitio nuclear] Línea: {string.Join("; ", bad)}.");
        else Debug.Log($"[Sitio nuclear] Línea: {l.Length:0} m a {l.AngleDeg:0}°, {SiteLineRule.OneWay(l.Length):0.0} s por viaje; un ascensor tardaría {SiteLineRule.HoistOneWay(l.Drop):0} s; pozo de escape en {SiteLineRule.EscapeSeconds(l.EscapeDrop, l.EscapeWalk):0} s.");
    }
}
