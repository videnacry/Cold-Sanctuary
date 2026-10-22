using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL MÉTODO DE MARIE CURIE (1898, «Rayos emitidos por los compuestos del uranio y del torio», Comptes Rendus 126): la
/// muestra, en polvo sobre una placa de un condensador (8 cm de diámetro, 3 cm de separación, 100 V), ioniza el aire y
/// deja pasar una corriente diminuta (~1e-11 A) que mueve la aguja del electrómetro (un punto de luz sobre una regla).
/// Con una mano va soltando la carga del CUARZO PIEZOELÉCTRICO de Pierre y Jacques Curie (tirando de una pesa) para
/// mantener el punto de luz en CERO; con la otra, el cronómetro. Corriente = carga del cuarzo / tiempo.
/// Aquí: 1-7 elige muestra, E empieza; mantén ESPACIO para soltar carga y que el punto no se salga de la regla.
/// Las corrientes son las de su tabla (en 1e-12 A). El hallazgo: la PECBLENDA y la CALCOLITA NATURAL activan más que el
/// propio uranio, y la calcolita ARTIFICIAL (que ella misma fabricó) no: dentro hay un elemento nuevo (polonio, radio).
/// </summary>
public class CurieElectrometerLesson : PhysicsLesson
{
    static readonly (string name, float pA)[] Samples =
    {
        ("Uranio metálico", 24f),
        ("Óxido negro de uranio", 27f),
        ("Óxido de torio (capa de 6 mm)", 53f),
        ("Calcolita ARTIFICIAL (la hizo ella)", 9f),
        ("Calcolita natural", 52f),
        ("Pecblenda de Joachimsthal", 67f),
        ("Pecblenda de Johanngeorgenstadt", 83f),
    };

    const float QuartzCharge = 10f;   // carga total que suelta el cuarzo con la pesa entera (unidades de la regla)
    const float ReleaseRate = 12f;    // al tirar de la pesa
    const float Ruler = 3f;           // el punto de luz se sale de la regla a ±3

    int _sel;
    bool _measuring;
    float _spot, _released, _t;
    readonly Dictionary<int, float> _measured = new Dictionary<int, float>();
    string _msg = "Elige una muestra (1-7) y pulsa E para medir.";

    protected override void Tick()
    {
        if (!_measuring)
        {
            for (int i = 0; i < Samples.Length; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) _sel = i;
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)) { _measuring = true; _spot = 0f; _released = 0f; _t = 0f; _msg = "Cronómetro en marcha: suelta la carga del cuarzo (ESPACIO) para que el punto de luz siga en cero."; }
            return;
        }
        float dt = Time.deltaTime;
        float drift = Samples[_sel].pA / 10f;   // la corriente de la muestra empuja el punto de luz
        float release = Input.GetKey(KeyCode.Space) ? ReleaseRate : 0f;
        release = Mathf.Min(release, (QuartzCharge - _released) / Mathf.Max(dt, 1e-4f));
        _spot += (drift - release) * dt;
        _released += release * dt;
        _t += dt;
        if (Mathf.Abs(_spot) > Ruler) { _measuring = false; _msg = _spot > 0f ? "El punto se salió de la regla: suelta carga más deprisa. Otra vez (E)." : "Soltaste demasiada: el punto se fue al otro lado. Otra vez (E)."; return; }
        if (_released >= QuartzCharge - 1e-3f)
        {
            _measuring = false;
            float current = QuartzCharge / _t * 10f;   // corriente medida (1e-12 A)
            _measured[_sel] = current;
            _msg = $"{Samples[_sel].name}: {_t:0.0} s → {current:0} ×10⁻¹² A (en su tabla: {Samples[_sel].pA:0}). Cuanto más corto el tiempo, más activa la muestra.";
            Check();
        }
    }

    void Check()
    {
        bool u = _measured.ContainsKey(0), art = _measured.ContainsKey(3), nat = _measured.ContainsKey(4), pech = _measured.ContainsKey(5) || _measured.ContainsKey(6);
        if (u && art && nat && pech)
            Complete("«La pecblenda y la calcolita natural son mucho más activas que el uranio. Este hecho sugiere que contienen un elemento mucho más activo que el uranio.» (1898)");
    }

    protected override void Draw(Rect a)
    {
        GUILayout.BeginArea(a);
        GUILayout.Label("El cobertizo de la calle Lhomond. Marie mide; Pierre construyó el cuarzo piezoeléctrico con su hermano Jacques.", Wrap);
        for (int i = 0; i < Samples.Length; i++)
        {
            string m = _measured.TryGetValue(i, out float c) ? $"  →  medido {c:0}" : "";
            GUILayout.Label($"{(i == _sel ? "▶" : "  ")} {i + 1}. {Samples[i].name}{m}", Wrap);
        }
        if (_measuring)
        {
            int pos = Mathf.RoundToInt((_spot + Ruler) / (2f * Ruler) * 30f);
            GUILayout.Label($"Regla: [{new string('-', Mathf.Clamp(pos, 0, 30))}●{new string('-', Mathf.Clamp(30 - pos, 0, 30))}]   cuarzo {_released / QuartzCharge * 100f:0}%   ⏱ {_t:0.0} s", Wrap);
        }
        GUILayout.Label(_msg, Wrap);
        if (!Done) GUILayout.Label("<i>Pista: mide el uranio, las dos calcolitas y una pecblenda. ¿Qué no cuadra?</i>", Wrap);
        GUILayout.EndArea();
    }
}
