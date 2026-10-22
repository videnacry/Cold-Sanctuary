using UnityEngine;

/// <summary>
/// E = mc² (Einstein, 1905: «¿Depende la inercia de un cuerpo de su contenido de energía?»), jugado en una BALANZA:
/// a un platillo, un núcleo de uranio-235 y el neutrón que lo parte; al otro, lo que sale de la FISIÓN (bario-141,
/// kriptón-92 y 3 neutrones). Los trozos pesan MENOS que el todo: faltan 0,186 u, que se han ido como energía,
/// E = Δm·c² ≈ 173 MeV (con las desintegraciones posteriores, ~200 MeV por fisión). Lise Meitner y Otto Frisch lo
/// calcularon así en 1938–39. Masas atómicas: AME (Atomic Mass Evaluation). E avanza paso a paso.
/// </summary>
public class EinsteinMassLesson : PhysicsLesson
{
    public Transform beam;   // el brazo de la balanza (gira según la diferencia de masa)
    public Transform leftPan, rightPan;

    const double U235 = 235.043930, Neutron = 1.008665, Ba141 = 140.914411, Kr92 = 91.926156;
    const double MeVPerU = 931.494, JPerMeV = 1.602177e-13, Avogadro = 6.02214e23, CoalJPerKg = 2.9e7;

    int _step;
    Transform[] _pieces = new Transform[0];

    static readonly string[] Steps =
    {
        "Pon el núcleo de uranio-235 en el platillo izquierdo (E).",
        "Añade el neutrón lento que lo va a partir (E).",
        "¡Fisión! (E): un núcleo igual se parte en bario-141, kriptón-92 y 3 neutrones; los trozos van al otro platillo.",
        "Pesa: ¿cuánto falta? (E)",
        "¿Adónde fue esa masa? E = mc² (E)",
        "¿Y en un gramo de uranio? (E)",
    };

    protected override void Tick()
    {
        if (!(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)) || _step >= Steps.Length) return;
        _step++;
        if (_step == 1) Piece(leftPan, "U-235", new Color(0.35f, 0.75f, 0.35f), 0.7f, Vector3.zero);
        if (_step == 2) Piece(leftPan, "n", new Color(0.75f, 0.75f, 0.8f), 0.18f, new Vector3(0.5f, 0f, 0f));
        if (_step == 3)
        {
            Piece(rightPan, "Ba-141", new Color(0.85f, 0.7f, 0.3f), 0.55f, new Vector3(-0.35f, 0f, 0f));
            Piece(rightPan, "Kr-92", new Color(0.45f, 0.6f, 0.9f), 0.45f, new Vector3(0.35f, 0f, 0f));
            for (int i = 0; i < 3; i++) Piece(rightPan, "n", new Color(0.75f, 0.75f, 0.8f), 0.18f, new Vector3(-0.3f + i * 0.3f, 0f, 0.45f));
        }
        if (_step == Steps.Length) Complete("«La masa de un cuerpo es una medida de su contenido de energía.» (1905)");
    }

    void Piece(Transform pan, string name, Color c, float size, Vector3 offset)
    {
        if (pan == null) return;
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(g.GetComponent<Collider>());
        g.name = name; g.transform.SetParent(pan, false);
        g.transform.localPosition = offset + Vector3.up * (0.1f + size * 0.5f); g.transform.localScale = Vector3.one * size;
        Renderer r = g.GetComponent<Renderer>(); r.material.color = c;
        var list = new System.Collections.Generic.List<Transform>(_pieces) { g.transform };
        _pieces = list.ToArray();
    }

    void LateUpdate()
    {
        if (beam == null) return;
        // Izquierda = antes (U-235 + n), derecha = después (los trozos). Al pesar, el de ANTES baja: los trozos pesan menos
        // (inclinación exagerada: la diferencia real es del 0,08 %).
        float tilt = _step == 0 ? 0f : _step < 3 ? 8f : _step == 3 ? 0f : 3f;
        beam.localRotation = Quaternion.Slerp(beam.localRotation, Quaternion.Euler(0f, 0f, tilt), Time.deltaTime * 2f);
    }

    protected override void Draw(Rect a)
    {
        double before = U235 + Neutron, after = Ba141 + Kr92 + 3 * Neutron, dm = before - after, mev = dm * MeVPerU;
        double perGram = Avogadro / U235 * 200.0 * JPerMeV;   // J por gramo de U-235 fisionado (~200 MeV por fisión)
        GUILayout.BeginArea(a);
        GUILayout.Label(_step < Steps.Length ? Steps[_step] : "Hecho. (Esc cierra.)", Wrap);
        if (_step >= 2) GUILayout.Label($"Antes: U-235 + n = {U235:0.000000} + {Neutron:0.000000} = <b>{before:0.000000} u</b>", Wrap);
        if (_step >= 4) GUILayout.Label($"Después: Ba-141 + Kr-92 + 3 n = <b>{after:0.000000} u</b>   →   faltan <b>{dm:0.000} u</b> ({dm / before * 100.0:0.00} % de la masa)", Wrap);
        if (_step >= 5) GUILayout.Label($"E = Δm·c² = {dm:0.000} u × 931,5 MeV/u = <b>{mev:0} MeV</b> por núcleo, como energía de movimiento de los trozos (calor). Con las desintegraciones de después, ~200 MeV.", Wrap);
        if (_step >= 6) GUILayout.Label($"1 g de U-235 fisionado: {perGram:0.0e0} J, como quemar <b>{perGram / CoalJPerKg / 1000.0:0.0} toneladas de carbón</b>. Por eso un reactor pequeño cabe en una piscina.", Wrap);
        if (Done) GUILayout.Label("<i>Einstein no trabajó en reactores: su fórmula explica de dónde sale la energía. En 1939 firmó la carta a Roosevelt que avisaba de la bomba; más tarde lo lamentó.</i>", Wrap);
        GUILayout.EndArea();
    }
}
