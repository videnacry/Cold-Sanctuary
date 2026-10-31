using UnityEngine;

/// <summary>
/// ÁREAS POR PISOS (docs/construcciones-elevadas.md §Pisos): en vez de una plataforma por área unida por puentes, una
/// misma base puede tener
/// - PLATAFORMAS HIJAS (entreplantas) de 4 m con ESCALERA INCLINADA, dentro de la plataforma: p. ej. las camas de la
///   enfermería en una galería en U, que deja el suelo libre para pasillos anchos;
/// - un PISO SUPERIOR a 11 m (no tapa la vista del de abajo), con columnas solo en el perímetro y escalera vertical
///   desde el piso de abajo: yoga + ballet, gimnasio + dojo, dos huertos en la misma base.
/// Así hay menos plataformas separadas y menos puentes.
/// </summary>
public static partial class SampleSceneBuilder
{
    public const float UpperFloorGap = 11f, MezzanineHeight = 4f;   // +1 m (Berón 2026-10-23): debajo se pasa holgado, sin chocar con la losa
    const float StairAngle = 38f;   // pendiente de las escaleras inclinadas (una escalera de casa ronda los 35–40°)
    const float SetBack = 0f;   // todos los pisos de un edificio, del mismo tamaño (Berón 2026-10-27: sin escalera, se sube en alfombra)

    /// <summary>Pone <paramref name="upper"/> como PISO SUPERIOR de <paramref name="lower"/> (ya elevada), 11 m más arriba.
    /// Devuelve el tamaño del piso (el mismo que el de abajo).</summary>
    static Vector2 StackOn(Transform upper, Transform lower, Vector2 lowerSize)
    {
        Vector2 size = new Vector2(lowerSize.x, lowerSize.y - SetBack);
        upper.SetPositionAndRotation(lower.TransformPoint(new Vector3(0f, 0f, -SetBack * 0.5f)), lower.rotation);
        _upperOf[upper] = lower;   // de qué edificio es este piso (sus alfombras de edificio lo unen con los demás)
        Elevate(upper, size, UpperFloorGap, groundBelow: false);
        return size;
    }

    /// <summary>ENTREPLANTA: una plataforma hija de <paramref name="size"/> con su cara de arriba a <paramref name="h"/>,
    /// sobre postes finos en el perímetro (cada ~4 m). <paramref name="noPosts"/> = lado sin postes (x/y = −1/+1: el borde
    /// −X/+X o −Z/+Z), p. ej. el que da al borde de la plataforma, por donde puede llegar un puente.
    /// Devuelve el Transform a la altura de la cara de arriba.</summary>
    static Transform Deck(Transform parent, string name, Vector3 center, Vector2 size, float h = MezzanineHeight, Vector2Int noPosts = default)
    {
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f);
        Transform d = new GameObject(name).transform; d.SetParent(parent, false); d.localPosition = center;
        Solid(d, new Vector3(0f, h - 0.075f, 0f), new Vector3(size.x, 0.15f, size.y), woodDark, "Losa_entreplanta");
        float hx = size.x * 0.5f - 0.1f, hz = size.y * 0.5f - 0.1f;
        int nx = Mathf.Max(1, Mathf.CeilToInt(size.x / 4f)), nz = Mathf.Max(1, Mathf.CeilToInt(size.y / 4f));
        for (int i = 0; i <= nx; i++)
            for (int k = 0; k <= nz; k++)
            {
                if (i != 0 && i != nx && k != 0 && k != nz) continue;   // solo el perímetro: debajo queda libre
                if ((noPosts.x < 0 && i == 0) || (noPosts.x > 0 && i == nx) || (noPosts.y < 0 && k == 0) || (noPosts.y > 0 && k == nz)) continue;
                Solid(d, new Vector3(-hx + 2f * hx * i / nx, (h - 0.15f) * 0.5f, -hz + 2f * hz * k / nz), new Vector3(0.12f, h - 0.15f, 0.12f), wood, "Poste");
            }
        Transform top = new GameObject("Encima").transform; top.SetParent(d, false); top.localPosition = Vector3.up * h;
        return top;
    }

    /// <summary>ESCALERA INCLINADA (<see cref="StairAngle"/>) de <paramref name="h"/> m: el pie en <paramref name="foot"/> (en el suelo del padre)
    /// y sube hacia <paramref name="yaw"/>. Rampa invisible (para andar y para el NavMesh) + peldaños, zancas y pasamanos.</summary>
    static void InclinedStair(Transform parent, string name, Vector3 foot, float yaw, float h = MezzanineHeight, float width = 1.4f)
    {
        Color wood = new Color(0.62f, 0.52f, 0.42f), woodDark = new Color(0.48f, 0.4f, 0.33f);
        float run = h / Mathf.Tan(StairAngle * Mathf.Deg2Rad), len = Mathf.Sqrt(run * run + h * h);
        Transform s = new GameObject(name).transform; s.SetParent(parent, false);
        s.localPosition = foot; s.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Transform ramp = Solid(s, new Vector3(0f, h * 0.5f, run * 0.5f), new Vector3(width, 0.05f, len), wood, "Rampa");
        ramp.localRotation = Quaternion.Euler(-StairAngle, 0f, 0f);
        ramp.GetComponent<Renderer>().enabled = false;
        int steps = Mathf.CeilToInt(h / 0.18f);
        for (int i = 0; i < steps; i++)
        {
            float y = (i + 1) * h / steps, z = (i + 0.5f) * run / steps;
            KitchenProps.C(s, new Vector3(0f, y - 0.04f, z), new Vector3(width, 0.08f, run / steps + 0.02f), wood, 0f, "Peldano");
        }
        for (int k = -1; k <= 1; k += 2)
        {
            Transform z1 = KitchenProps.C(s, new Vector3(k * width * 0.5f, h * 0.5f, run * 0.5f), new Vector3(0.08f, 0.25f, len), woodDark, 0f, "Zanca");
            z1.localRotation = Quaternion.Euler(-StairAngle, 0f, 0f);
            Transform rail = KitchenProps.C(s, new Vector3(k * width * 0.5f, h * 0.5f + 0.9f, run * 0.5f), new Vector3(0.05f, 0.05f, len), wood, 0f, "Pasamanos");
            rail.localRotation = Quaternion.Euler(-StairAngle, 0f, 0f);
        }
        WorldLabel wl = s.gameObject.AddComponent<WorldLabel>(); wl.text = "Escalera"; wl.height = h + 0.6f;
    }

    /// <summary>Largo (en planta) de una escalera inclinada de <paramref name="h"/> m.</summary>
    static float StairRun(float h = MezzanineHeight) => h / Mathf.Tan(StairAngle * Mathf.Deg2Rad);

    /// <summary>
    /// ANILLO (Berón, 2026-10-23): plataforma hija CUADRADA Y COMPLETA alrededor de una planta de <paramref name="floor"/>, a
    /// <paramref name="h"/> m sobre ella, de <paramref name="width"/> m de ancho, con solo el <paramref name="inside"/> (20 %)
    /// sobre la planta y el resto VOLANDO fuera del borde, para no quitarle sitio. Sin postes (voladizo) y sin barreras en los
    /// bordes (Berón 2026-10-27: se puede saltar); sin escaleras (Berón 2026-10-28): se sube en alfombra, con <see cref="RingCarpets"/>.
    /// Devuelve la raíz (a la altura de la cara de arriba). <see cref="RingPoint"/> recorre su línea central.
    /// </summary>
    static Transform RingDeck(Transform parent, string name, Vector2 floor, float h = MezzanineHeight, float width = 10f, float inside = 0.2f)
    {
        Color woodDark = new Color(0.48f, 0.4f, 0.33f), wood = new Color(0.62f, 0.52f, 0.42f);
        float inn = width * inside, hx = floor.x * 0.5f - inn, hz = floor.y * 0.5f - inn, ox = hx + width, oz = hz + width;
        Transform ring = new GameObject(name).transform; ring.SetParent(parent, false); ring.localPosition = Vector3.up * h;
        _ringOpenBack = false;
        // EN UN PISO PEGADO AL TRONCO de un edificio-árbol (docs/edificio-arbol.md §Migración): el anillo va en U, abierto hacia el
        // tronco (su lado −Z), con las alas hasta la cara del tronco y un BALCÓN de 3 m sobre el borde del piso, contra el tronco:
        // ahí está su puerta (se llega nadando, como a los anillos de las puntas).
        if (HelixDeckOf(parent, out Helix hxr, out _, out Transform deckr))
        {
            float S = hxr.h.floorSize;
            float zBack = ring.InverseTransformPoint(deckr.TransformPoint(new Vector3(0f, 0f, -S * 0.5f))).z, len = oz - zBack, zc = (oz + zBack) * 0.5f;
            Solid(ring, new Vector3(0f, -0.075f, hz + width * 0.5f), new Vector3(2f * ox, 0.15f, width), woodDark, "Losa_anillo");
            KitchenProps.C(ring, new Vector3(0f, -0.3f, oz - 0.15f), new Vector3(2f * ox, 0.3f, 0.3f), wood, 0f, "Viga_borde");
            for (int s = -1; s <= 1; s += 2)
            {
                Solid(ring, new Vector3(s * (hx + width * 0.5f), -0.075f, (zBack + hz) * 0.5f), new Vector3(width, 0.15f, hz - zBack), woodDark, "Losa_anillo");
                KitchenProps.C(ring, new Vector3(s * (ox - 0.15f), -0.3f, zc), new Vector3(0.3f, 0.3f, len), wood, 0f, "Viga_borde");
            }
            Solid(ring, new Vector3(0f, -0.075f, zBack + 1.5f), new Vector3(2f * hx, 0.15f, 3f), woodDark, "Losa_anillo");   // el balcón
            new GameObject("Balcon_tronco").transform.SetParent(ring, false);
            _ringOpenBack = true; _ringBackZ = zBack;
            return ring;
        }
        for (int s = -1; s <= 1; s += 2)
        {
            Solid(ring, new Vector3(0f, -0.075f, s * (hz + width * 0.5f)), new Vector3(2f * ox, 0.15f, width), woodDark, "Losa_anillo");
            Solid(ring, new Vector3(s * (hx + width * 0.5f), -0.075f, 0f), new Vector3(width, 0.15f, 2f * hz), woodDark, "Losa_anillo");
            KitchenProps.C(ring, new Vector3(0f, -0.3f, s * (oz - 0.15f)), new Vector3(2f * ox, 0.3f, 0.3f), wood, 0f, "Viga_borde");
            KitchenProps.C(ring, new Vector3(s * (ox - 0.15f), -0.3f, 0f), new Vector3(0.3f, 0.3f, 2f * oz), wood, 0f, "Viga_borde");
        }
        return ring;
    }

    /// <summary>Punto y orientación a la distancia <paramref name="s"/> por la línea central del anillo (empieza en el centro
    /// del lado +Z y gira en sentido horario). yaw = la cama atravesada al anillo (su largo a lo ancho).</summary>
    // El último anillo hecho va en U (un piso de un edificio-árbol): sus puntos del lado del tronco no valen (RingPoint los da como esquina).
    static bool _ringOpenBack; static float _ringBackZ;

    static (Vector3 p, float yaw, bool corner) RingPoint(Vector2 floor, float s, float width = 10f, float inside = 0.2f, float cornerClear = -1f)
    {
        var r = RingPointFull(floor, s, width, inside, cornerClear);
        if (_ringOpenBack && r.p.z < _ringBackZ + 1.5f) r.corner = true;   // el lado del tronco (anillo en U): no vale
        return r;
    }

    static (Vector3 p, float yaw, bool corner) RingPointFull(Vector2 floor, float s, float width, float inside, float cornerClear)
    {
        float inn = width * inside, cx = floor.x * 0.5f - inn + width * 0.5f, cz = floor.y * 0.5f - inn + width * 0.5f;
        float per = 4f * (cx + cz); s = Mathf.Repeat(s, per);
        float a = cx, b = 2f * cz, c = 2f * cx, d = 2f * cz;   // tramos: +Z (mitad derecha), +X, −Z, −X, +Z (mitad izquierda)
        float cc = cornerClear >= 0f ? cornerClear : width * 0.5f;   // cuánto se aparta de la esquina (las camas caben a 2,6 m: dos por esquina)
        bool Corner(float along, float len) => along < cc || along > len - cc;
        if (s < a) return (new Vector3(s, 0f, cz), 0f, s > a - cc);
        s -= a; if (s < b) return (new Vector3(cx, 0f, cz - s), 90f, Corner(s, b));
        s -= b; if (s < c) return (new Vector3(cx - s, 0f, -cz), 0f, Corner(s, c));
        s -= c; if (s < d) return (new Vector3(-cx, 0f, -cz + s), 90f, Corner(s, d));
        s -= d; return (new Vector3(-cx + s, 0f, cz), 0f, s < cc);
    }

    static float RingLength(Vector2 floor, float width = 10f, float inside = 0.2f) => 4f * (floor.x * 0.5f + floor.y * 0.5f - 2f * width * inside + width);

    /// <summary>ÁREA DE ALFOMBRAS DEL ANILLO (Berón 2026-10-28: sin escaleras, a los anillos se sube en alfombra): en el centro del
    /// lado +Z de su línea central, atravesada (7,6 m a lo ancho del anillo), de la red del EDIFICIO: sus alfombras llevan a los
    /// pisos del edificio y las de cada piso, a este anillo. Superficie «edificio#anillo» (en el piso N &gt; 1, «edificio#anilloN»),
    /// como las esterillas del anillo de yoga. Quien amuebla el anillo deja libre ese hueco (|x| &lt; 4,6 m en el lado +Z).</summary>
    static CarpetPad RingCarpets(Transform ring, Transform floor, Vector2 floorSize, string building, string label, float width = 10f, float inside = 0.2f)
    {
        if (HelixDeckOf(floor, out _, out _, out _)) return null;   // en un edificio-árbol se llega al anillo nadando, por su puerta del tronco
        var (p, _, _) = RingPoint(floorSize, 0f, width, inside);   // s = 0: el centro del lado +Z
        building = BuildingOf(floor, building);   // en una torre, la red es la de la torre (Servicios, Ciencias…), no la del área
        int i = FloorIndex(floor);
        string surf = i == 1 ? $"{building}#anillo" : $"{building}#anillo{i}";
        return CarpetPad.BuildZone(ring, ring.TransformPoint(p + Vector3.up * 0.01f), ring.rotation * Quaternion.Euler(0f, 90f, 0f), CarpetPad.Kind.Edificio,
            building, i, surf, label, 2);
    }
}
