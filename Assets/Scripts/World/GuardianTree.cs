using UnityEngine;

/// <summary>
/// EL ÁRBOL GUARDIÁN, EL MÁS GRANDE DE LA HISTORIA (Berón 2026-11-04; docs/arbol-guardian.md): un INJERTO del árbol más ANCHO y del
/// más ALTO que han existido, con los matices de los árboles de hielo del santuario (su color y sus panales).
/// - BASE: la del árbol más ancho registrado, el CASTAÑO DE LOS CIEN CABALLOS (Sicilia): 57,9 m de perímetro en 1780 →
///   <see cref="BaseDiameter"/> 18,4 m, con el tronco ACANALADO y los CONTRAFUERTES del Árbol del Tule (ciprés de Moctezuma, 42 m de
///   perímetro). Se estrecha en <see cref="BaseHeight"/> m hasta el fuste.
/// - PASADIZOS: dos túneles en cruz que atraviesan la base (como los secuoyas de los túneles —el Wawona, el Chandelier— y los baobabs
///   huecos), de <see cref="TunnelWidth"/> × <see cref="TunnelHeight"/> m (cuadrados, como las puertas de los edificios-árbol: caben 8
///   personas a la vez y una alfombra voladora), con una sala en el cruce.
/// - FUSTE Y COPA: los de la secuoya roja más alta (Hyperion, 116 m; el récord histórico, el fresno de montaña de Ferguson, 132,6 m):
///   columna recta que se estrecha y copa estrecha y cónica en el tercio de arriba. La altura la manda el santuario: lo más alto de
///   [132,6 m, el edificio más alto] (<see cref="GuardTower.TreeHeight"/>).
/// - MATICES DE HIELO: la corteza, las ramas y la escarcha de los árboles de hielo, posaderos en las ramas y PANALES de las abejas de
///   las nieves en las ramas bajas de la copa (con su colmena: <see cref="Hive"/>).
/// </summary>
public static class GuardianTree
{
    /// <summary>El árbol más alto medido de la historia: el fresno de montaña (<i>Eucalyptus regnans</i>) de Ferguson, Victoria, 1872.</summary>
    public const float RecordHeight = 132.6f;
    /// <summary>El más alto vivo: Hyperion (<i>Sequoia sempervirens</i>), 116,07 m (2019).</summary>
    public const float LivingRecordHeight = 116.07f;
    /// <summary>Perímetro del castaño de los cien caballos (Etna, Sicilia) en 1780: 57,9 m.</summary>
    public const float RecordGirth = 57.9f;
    public static float BaseDiameter => RecordGirth / Mathf.PI;   // 18,4 m
    public const float BaseHeight = 26f;          // hasta aquí, la base ancha (acanalada, con contrafuertes)
    public const float TunnelWidth = 7f, TunnelHeight = 7f;
    public const float ColumnBottom = 5.2f, ColumnTop = 1.8f;   // ancho del fuste (la secuoya: ~4,8 m de diámetro a la altura del pecho)

    static Material _bark, _branch, _frost, _perch, _nest, _floor, _lamp;

    /// <summary>Ancho (diámetro) del tronco a la altura <paramref name="y"/>.</summary>
    public static float WidthAt(float y, float height)
    {
        if (y <= BaseHeight)
        {
            float t = Mathf.SmoothStep(0f, 1f, y / BaseHeight);
            return Mathf.Lerp(BaseDiameter, ColumnBottom, t);
        }
        return Mathf.Lerp(ColumnBottom, ColumnTop, (y - BaseHeight) / Mathf.Max(1f, height - BaseHeight));
    }

    /// <summary>¿La duela (caja de <paramref name="tw"/> × <paramref name="thick"/> girada <paramref name="yaw"/>° con centro en
    /// <paramref name="c"/>) pisa alguno de los dos pasillos (|x| &lt; <paramref name="w"/> o |z| &lt; <paramref name="w"/>)?</summary>
    static bool InCorridor(Vector3 c, float yaw, float tw, float thick, float w)
    {
        Quaternion q = Quaternion.Euler(0f, yaw, 0f);
        float x0 = float.MaxValue, x1 = float.MinValue, z0 = float.MaxValue, z1 = float.MinValue;
        for (int i = 0; i < 4; i++)
        {
            Vector3 p = c + q * new Vector3((i & 1) == 0 ? -tw * 0.5f : tw * 0.5f, 0f, (i & 2) == 0 ? -thick * 0.5f : thick * 0.5f);
            x0 = Mathf.Min(x0, p.x); x1 = Mathf.Max(x1, p.x); z0 = Mathf.Min(z0, p.z); z1 = Mathf.Max(z1, p.z);
        }
        return (x0 < w && x1 > -w) || (z0 < w && z1 > -w);
    }

    public static GameObject Build(Transform parent, string name, Vector3 groundPos, float height, System.Func<Vector3, bool> blocked = null)
    {
        var rnd = new System.Random(4242);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        Material bark = CubeTree.Mat(ref _bark, new Color(0.55f, 0.72f, 0.80f));       // el hielo opaco azulado de los árboles de hielo
        Material branch = CubeTree.Mat(ref _branch, new Color(0.70f, 0.86f, 0.92f));
        Material frost = CubeTree.Mat(ref _frost, new Color(0.45f, 0.75f, 0.85f));
        Material perch = CubeTree.Mat(ref _perch, new Color(0.85f, 0.94f, 0.98f));
        Material nest = CubeTree.Mat(ref _nest, new Color(0.90f, 0.78f, 0.35f));
        Material floor = CubeTree.Mat(ref _floor, new Color(0.62f, 0.58f, 0.52f));
        Material lamp = CubeTree.Mat(ref _lamp, new Color(1f, 0.85f, 0.45f));

        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = groundPos;
        Transform tr = root.transform;
        float hw = TunnelWidth * 0.5f;

        // ── BASE ACANALADA: anillos de 16 duelas alrededor del eje, cada 2 m, que se estrechan; las duelas alternas, metidas hacia dentro
        // (las acanaladuras del Tule). Por debajo de la altura de los túneles no hay duelas en sus pasillos; dentro, el núcleo macizo en
        // cuatro cuartos (entre los túneles) y, encima, el techo de la sala. ──
        const int staves = 16; const float layer = 2f;
        for (float y = 0f; y < BaseHeight; y += layer)
        {
            float W = WidthAt(y + layer * 0.5f, height), Rr = W * 0.5f, thick = Mathf.Min(3.2f, Rr * 0.45f);
            if (y < TunnelHeight + 1f)
            {
                // (2026-11-04) A LA ALTURA DE LOS PASADIZOS: cada cuarto de la base, entre los dos túneles, se llena con una rejilla de
                // bloques de 1,5 m hasta el borde (el círculo de la base), alineados con los túneles. Con duelas giradas, sus esquinas pisaban
                // el pasillo y se quitaban todas: la base salía de 13,4 m (prueba 91).
                const float cell = 1.5f;
                for (float x = hw; x < Rr; x += cell)
                    for (float z = hw; z < Rr; z += cell)
                    {
                        float x1 = Mathf.Min(x + cell, Rr), z1 = Mathf.Min(z + cell, Rr);
                        if (x * x + z * z > Rr * Rr) continue;                       // del todo fuera del círculo
                        float cx = (x + x1) * 0.5f, cz = (z + z1) * 0.5f;
                        if (cx * cx + cz * cz > (Rr + 0.4f) * (Rr + 0.4f)) continue;
                        bool flute = ((Mathf.RoundToInt(Mathf.Atan2(cz, cx) * Mathf.Rad2Deg / 11.25f)) % 2 == 1) && cx * cx + cz * cz > (Rr - cell) * (Rr - cell);   // acanaladura
                        for (int qx = -1; qx <= 1; qx += 2)
                            for (int qz = -1; qz <= 1; qz += 2)
                                CubeTree.Block(tr, "Tronco", new Vector3(qx * cx, y + layer * 0.5f, qz * cz), new Vector3((x1 - x) * (flute ? 0.8f : 1.02f), layer * 1.02f, (z1 - z) * (flute ? 0.8f : 1.02f)), Quaternion.identity, bark, true);
                    }
                continue;
            }
            for (int s = 0; s < staves; s++)
            {
                float a = (s + (Mathf.RoundToInt(y / layer) % 2) * 0.5f) * 360f / staves;
                float rr = Rr * (s % 2 == 0 ? 1f : 0.9f) - thick * 0.5f;   // acanaladura
                Vector3 c = Quaternion.Euler(0f, a, 0f) * Vector3.forward * rr;
                float tw = 2f * Mathf.PI * Rr / staves * 1.15f;
                CubeTree.Block(tr, "Tronco", c + Vector3.up * (y + layer * 0.5f), new Vector3(tw, layer * 1.02f, thick), Quaternion.Euler(0f, a, 0f), bark, true);
            }
            float core = Mathf.Max(0f, Rr - thick * 0.9f);
            if (core >= 0.5f) CubeTree.Block(tr, "Tronco", Vector3.up * (y + layer * 0.5f), new Vector3(core * 1.42f, layer * 1.02f, core * 1.42f), Quaternion.Euler(0f, 22.5f, 0f), bark, true);   // (el techo de los pasadizos, el primero)
        }
        // ── CONTRAFUERTES (raíces tabulares): 12 aletas que bajan al hielo, entre las bocas de los túneles ──
        float R0 = BaseDiameter * 0.5f;
        for (int k = 0; k < 12; k++)
        {
            float a = k * 30f + 15f;
            Vector3 d = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
            if (Mathf.Abs(d.x * R0) < hw + 1.5f || Mathf.Abs(d.z * R0) < hw + 1.5f) continue;   // (no tapan las bocas)
            CubeTree.Block(tr, "Raiz", d * (R0 + 2.2f) + Vector3.up * 1.6f, new Vector3(1.4f, 3.2f, 5f), Quaternion.LookRotation(d) * Quaternion.Euler(22f, 0f, 0f), bark, true);
        }
        // ── LOS PASADIZOS: suelo pisado, faroles y la sala del cruce ──
        Transform tun = new GameObject("Pasadizos").transform; tun.SetParent(tr, false);
        float len = R0 + 3f;
        CubeTree.Block(tun, "Pasadizo_NS", new Vector3(0f, 0.03f, 0f), new Vector3(TunnelWidth, 0.06f, 2f * len), Quaternion.identity, floor, false);
        CubeTree.Block(tun, "Pasadizo_EO", new Vector3(0f, 0.035f, 0f), new Vector3(2f * len, 0.06f, TunnelWidth), Quaternion.identity, floor, false);
        CubeTree.Block(tun, "Sala_del_cruce", new Vector3(0f, 0.04f, 0f), new Vector3(TunnelWidth, 0.06f, TunnelWidth), Quaternion.identity, lamp, false);
        for (int s = 0; s < 4; s++)
        {
            Quaternion q = Quaternion.Euler(0f, s * 90f, 0f);
            CubeTree.Block(tun, "Farol_pasadizo", q * new Vector3(hw - 0.3f, TunnelHeight - 1f, R0 * 0.5f), new Vector3(0.3f, 0.4f, 0.3f), q, lamp, false);
        }
        // ── EL FUSTE (secuoya): bloques de 3 m que se estrechan, con un leve giro ──
        for (float y = BaseHeight; y < height; y += 3f)
        {
            float W = WidthAt(y + 1.5f, height);
            CubeTree.Block(tr, "Tronco", new Vector3(0f, y + 1.5f, 0f), new Vector3(W, 3.06f, W), Quaternion.Euler(0f, y * 3.7f, 0f), bark, true);
        }
        // ── LA COPA: estrecha y cónica en el tercio de arriba (la de la secuoya), ramas cortas que bajan un poco y se levantan en la punta,
        // posaderos con escarcha, y panales en las ramas bajas de la copa ──
        int nests = 0;
        var lowPerches = new System.Collections.Generic.List<Vector3>();
        float crownBottom = height * 0.62f;
        for (float y = crownBottom; y < height - 2f; y += 4f)
        {
            float t = (y - crownBottom) / (height - crownBottom);
            float reach = Mathf.Lerp(11f, 2.5f, t);
            int count = t < 0.5f ? 5 : 4;
            float off = R(0f, 360f);
            for (int b = 0; b < count; b++)
            {
                float yaw = off + b * 360f / count + R(-10f, 10f), pitch = R(-8f, 14f);
                Vector3 p = new Vector3(0f, y, 0f) + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * WidthAt(y, height) * 0.45f;
                const int segs = 3; float segLen = reach / segs;
                if (blocked != null)
                {
                    bool ok = false;
                    for (int tryN = 0; tryN < 8 && !ok; tryN++) { float yy = yaw + tryN * 45f; ok = !CubeTree.BranchHits(tr, p, yy, pitch, segLen, segs, blocked); if (ok) yaw = yy; }
                    if (!ok) continue;
                }
                float thick = Mathf.Lerp(1.1f, 0.5f, t);
                for (int s = 0; s < segs; s++)
                {
                    Vector3 dir = Quaternion.Euler(-pitch + s * 6f, yaw, 0f) * Vector3.forward;
                    CubeTree.Block(tr, "Rama", p + dir * segLen * 0.5f, new Vector3(thick, thick, segLen * 1.05f), Quaternion.LookRotation(dir), branch, true);
                    p += dir * segLen; thick *= 0.8f;
                }
                GameObject plat = CubeTree.Block(tr, "Rama_Plataforma", p + Vector3.up * 0.1f, new Vector3(1.8f, 0.2f, 1.8f), Quaternion.Euler(0f, yaw, 0f), perch, true);
                CubeTree.Block(tr, "Escarcha", p + Vector3.up * 0.8f, Vector3.one * R(1f, 1.6f), Quaternion.Euler(R(0f, 30f), R(0f, 90f), 0f), frost, false);
                if (t < 0.45f) lowPerches.Add(p);
            }
        }
        // PANALES (2026-11-04: «0 panales», prueba 91): en las puntas de las ramas bajas de la copa, repartidos (uno de cada dos), hasta 8.
        GuardianHives gh = root.AddComponent<GuardianHives>();
        for (int i = 0; i < lowPerches.Count && nests < 8; i += 2) { Vector3 at = lowPerches[i] + new Vector3(0.5f, -0.05f, 0.2f); CubeTree.Honeycomb(tr, at, nest, 977 + i * 13).stays = true; gh.spots.Add(at); nests++; }
        // Escarcha también en los hombros de la base (donde el ancho se recoge), como la nieve en los contrafuertes.
        for (int k = 0; k < 8; k++)
        {
            Vector3 d = Quaternion.Euler(0f, k * 45f + 22.5f, 0f) * Vector3.forward;
            CubeTree.Block(tr, "Escarcha", d * WidthAt(BaseHeight - 4f, height) * 0.5f + Vector3.up * (BaseHeight - 3f), Vector3.one * R(1.2f, 2f), Quaternion.Euler(0f, R(0f, 90f), 0f), frost, false);
        }
        // La punta.
        CubeTree.Block(tr, "Copa", new Vector3(0f, height + 1.5f, 0f), new Vector3(3f, 3f, 3f), Quaternion.Euler(0f, 20f, 0f), frost, false);
        CubeTree.Block(tr, "Copa", new Vector3(0f, height + 3.6f, 0f), new Vector3(1.6f, 1.6f, 1.6f), Quaternion.Euler(0f, 55f, 0f), frost, false);
        Debug.Log($"[Guardia] Árbol guardián (injerto): base de {BaseDiameter:0.0} m (el castaño de los cien caballos, {RecordGirth:0.0} m de perímetro), fuste y copa de secuoya hasta {height:0} m " +
                  $"(récord histórico {RecordHeight:0.0} m; vivo, Hyperion, {LivingRecordHeight:0.00} m), dos pasadizos de {TunnelWidth:0} × {TunnelHeight:0} m y {nests} panales.");
        return root;
    }
}

/// <summary>Los PANALES del árbol guardián (sitios locales). Al empezar el juego, si no están (prueba 91, 2026-11-04: «0 colmenas en 0
/// nidos» aunque el constructor puso 8: algo los quita entre el Blockout y el juego, aún sin encontrar), los vuelve a poner y lo avisa.</summary>
public class GuardianHives : MonoBehaviour
{
    public System.Collections.Generic.List<Vector3> spots = new System.Collections.Generic.List<Vector3>();
    static Material _nest;

    void Awake()
    {
        if (GetComponentsInChildren<Hive>(true).Length > 0 || spots.Count == 0) return;
        Material nest = CubeTree.Mat(ref _nest, new Color(0.90f, 0.78f, 0.35f));
        for (int i = 0; i < spots.Count; i++) CubeTree.Honeycomb(transform, spots[i], nest, 977 + i * 26).stays = true;
        Debug.LogWarning($"[Guardia] Faltaban los panales del árbol guardián al empezar: se vuelven a poner {spots.Count} (algo los quita después del Blockout).");
    }
}
