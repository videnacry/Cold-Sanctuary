using UnityEngine;

/// <summary>
/// EL ÁRBOL GIGANTE, EL MÁS GRANDE DE LA HISTORIA (Berón 2026-11-05; docs/arbol-guardian.md): un INJERTO del árbol más ANCHO y del más ALTO de los que hay registro, con los
/// matices de los árboles de hielo (su color y sus panales). No tiene túnel ni guardia: es solo el árbol, en su isla. La guardia está en la torre de Servicios (<see cref="GuardRoom"/>).
/// - BASE: el CASTAÑO DE LOS CIEN CABALLOS (Sicilia): 57,9 m de perímetro en 1780 → <see cref="BaseDiameter"/> 18,4 m, con el tronco ACANALADO y los CONTRAFUERTES del Árbol del Tule.
///   Era una cepa de varios troncos: el mayor de un solo fuste es mucho menor. Se elige por ser el mayor perímetro registrado, no el más seguro.
/// - FUSTE: el de la secuoya gigante General Sherman (<see cref="ColumnBottom"/> 7,7 m); a 40 m de altura supera al árbol de Lindsey Creek (5,8 m).
/// - ALTO: <see cref="RecordHeight"/> 142 m, el abeto de Nooksack (medido con cinta tras talarlo, discutido); el aceptado por Guinness es el de Ferguson, <see cref="AcceptedHeight"/> 132,6 m.
///   Lo manda lo mayor de ese récord y del edificio más alto. Ningún fósil se acerca: el mayor estimado ronda los 59 m.
/// - VOLUMEN: el tronco supera los 2550 m³ del árbol de Lindsey Creek, el mayor registrado (<see cref="TrunkVolume"/>).
/// - MATICES DE HIELO: la corteza, las ramas y la escarcha de los árboles de hielo, posaderos y PANALES de las abejas de las nieves (<see cref="Hive"/>).
/// </summary>
public static class GuardianTree
{
    public const string ObjectName = "ArbolGigante", GroveName = "ArbolGigante_AUTO";
    /// <summary>Lo más alto que se ha afirmado con medición: el abeto de Nooksack, Washington, 1897, a cinta tras talarlo. Discutido.</summary>
    public const float RecordHeight = 142f;
    /// <summary>El aceptado por Guinness: el fresno de montaña de Ferguson, Victoria, 1872 (le faltaba la punta).</summary>
    public const float AcceptedHeight = 132.6f;
    /// <summary>El más alto vivo: Hyperion (<i>Sequoia sempervirens</i>), 116,07 m.</summary>
    public const float LivingRecordHeight = 116.07f;
    /// <summary>El fósil más alto que encontré estimado: <i>Araucarioxylon arizonicum</i>, 59 m (por proporciones; no medido).</summary>
    public const float FossilHeight = 59f;
    /// <summary>Perímetro del castaño de los cien caballos (Etna, Sicilia) en 1780: 57,9 m.</summary>
    public const float RecordGirth = 57.9f;
    /// <summary>Volumen del árbol de Lindsey Creek (secuoya roja, cayó en 1905): 2550 m³, sin comprobar. General Sherman: 1591 m³ (Guinness, 2013).</summary>
    public const float RecordVolume = 2550f;
    public static float BaseDiameter => RecordGirth / Mathf.PI;   // 18,4 m
    public const float BaseHeight = 26f;          // hasta aquí, la base ancha (acanalada, con contrafuertes)
    public const float ColumnBottom = 7.7f, ColumnTop = 1.8f;   // ancho del fuste: el de General Sherman a la altura del pecho
    /// <summary>Su altura: la fija el constructor; lo mayor del récord y del edificio más alto.</summary>
    public static float Height = RecordHeight;

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

    /// <summary>Volumen del tronco (m³), con un círculo del ancho de cada altura.</summary>
    public static float TrunkVolume(float height)
    {
        float v = 0f;
        for (float y = 0f; y < height; y += 1f) { float w = WidthAt(y + 0.5f, height); v += Mathf.PI * 0.25f * w * w; }
        return v;
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
        root.AddComponent<NestTree>();
        root.transform.SetParent(parent, false);
        root.transform.position = groundPos;
        Transform tr = root.transform;

        // ── BASE ACANALADA: anillos de 16 duelas alrededor del eje, cada 2 m, que se estrechan; las duelas alternas, metidas hacia dentro
        // (las acanaladuras del Tule). Dentro, el núcleo macizo. ──
        const int staves = 16; const float layer = 2f;
        for (float y = 0f; y < BaseHeight; y += layer)
        {
            float W = WidthAt(y + layer * 0.5f, height), Rr = W * 0.5f, thick = Mathf.Min(3.2f, Rr * 0.45f);
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
        // ── CONTRAFUERTES (raíces tabulares): 12 aletas que bajan al hielo ──
        float R0 = BaseDiameter * 0.5f;
        for (int k = 0; k < 12; k++)
        {
            float a = k * 30f + 15f;
            Vector3 d = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
            CubeTree.Block(tr, "Raiz", d * (R0 + 2.2f) + Vector3.up * 1.6f, new Vector3(1.4f, 3.2f, 5f), Quaternion.LookRotation(d) * Quaternion.Euler(22f, 0f, 0f), bark, true);
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
        Debug.Log($"[Árbol gigante] Base de {BaseDiameter:0.0} m, {height:0.0} m de alto y {TrunkVolume(height):0} m³ (récords: {RecordHeight:0.0} m, {RecordVolume:0} m³); {nests} panales.");
        return root;
    }
}

/// <summary>Los PANALES del árbol gigante (sitios locales). Al empezar el juego, si no están (prueba 91, 2026-11-04: «0 colmenas en 0
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
        Debug.LogWarning($"[Árbol gigante] Faltaban los panales al empezar: se vuelven a poner {spots.Count} (algo los quita después del Blockout).");
    }
}
