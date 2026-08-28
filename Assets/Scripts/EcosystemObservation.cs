using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Misión-OBSERVACIÓN (WASD) del mundo vivo (docs/testing-checklist.md §39). No es un check mecánico de "llega al punto":
/// el jugador **recorre el santuario** y un **HUD de status global** le muestra la salud del ecosistema (por especie:
/// etapas child/teen/adult, hambrientos, en celo, enfermos; totales herbívoros vs depredadores; tendencia nacimientos/
/// muertes). Además **loguea** el status cada `logInterval` y **alerta** de desbalances (extinción de una especie,
/// colapso de herbívoros o depredadores) → el compañero **lo juega, lo vive y reporta** (UX/QA), y el log delata bugs de
/// balance que un test automático no capta igual.
///
/// Solo LECTURA (no muta la fauna). Barato: agrega `Animal.wholePopulation` (~decenas) cada `sampleInterval`.
/// </summary>
public class EcosystemObservation : MonoBehaviour
{
    [Tooltip("Cada cuánto se re-agrega el status (s).")]
    [Min(0.1f)] public float sampleInterval = 0.5f;
    [Tooltip("Cada cuánto se loguea el resumen a consola (s).")]
    [Min(1f)] public float logInterval = 10f;

    struct Stat { public int total, child, teen, adult, hungry, estrus, sick, herbivore, predator; }

    readonly Dictionary<string, Stat> _current = new Dictionary<string, Stat>();
    readonly Dictionary<string, int> _prevTotals = new Dictionary<string, int>();
    int _herbTotal, _predTotal, _liveTotal, _prevLiveTotal;
    float _nextSample, _nextLog;

    void Update()
    {
        if (Time.time >= _nextSample) { _nextSample = Time.time + sampleInterval; Sample(); }
        if (Time.time >= _nextLog)    { _nextLog = Time.time + logInterval; LogSummary(); }
    }

    void Sample()
    {
        _current.Clear();
        _herbTotal = _predTotal = _liveTotal = 0;

        foreach (GameObject go in Animal.wholePopulation)
        {
            if (go == null) continue;
            Animal a = go.GetComponent<Animal>();
            if (a == null || a.death) continue;

            string sp = a.SpeciesName ?? "?";
            _current.TryGetValue(sp, out Stat s);
            s.total++;
            if (a.lifeStage == LifeStage.child) s.child++;
            else if (a.lifeStage == LifeStage.teen) s.teen++;
            else if (a.lifeStage == LifeStage.adult) s.adult++;
            if (a.hungry >= 0f) s.hungry++;
            EstrusState e = a.GetComponent<EstrusState>();
            if (e != null && e.InEstrus) s.estrus++;
            if (a.sickness > 0.01f) s.sick++;
            bool herb = a.Forage != null && a.Forage.eatsGrass;
            bool pred = a.Forage != null && a.Forage.eatsPrey;
            if (herb) { s.herbivore++; _herbTotal++; }
            if (pred) { s.predator++; _predTotal++; }
            _current[sp] = s;
            _liveTotal++;
        }
    }

    void LogSummary()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"[ECO] vivos={_liveTotal} (Δ{_liveTotal - _prevLiveTotal:+0;-0;0}) · herbívoros={_herbTotal} · depredadores={_predTotal}");
        foreach (KeyValuePair<string, Stat> kv in _current)
        {
            Stat s = kv.Value;
            sb.Append($" | {kv.Key}:{s.total}(c{s.child}/t{s.teen}/a{s.adult} h{s.hungry} celo{s.estrus} enf{s.sick})");
        }
        Debug.Log(sb.ToString());

        // Alertas de desbalance.
        foreach (KeyValuePair<string, int> prev in _prevTotals)
            if (prev.Value > 0 && (!_current.ContainsKey(prev.Key) || _current[prev.Key].total == 0))
                Debug.LogWarning($"[ECO] ALERTA: la especie «{prev.Key}» se EXTINGUIÓ.");
        if (_prevLiveTotal > 0 && _herbTotal == 0) Debug.LogWarning("[ECO] ALERTA: NO quedan herbívoros (colapso de la base).");
        if (_prevLiveTotal > 0 && _predTotal == 0) Debug.LogWarning("[ECO] ALERTA: NO quedan depredadores.");

        _prevTotals.Clear();
        foreach (KeyValuePair<string, Stat> kv in _current) _prevTotals[kv.Key] = kv.Value.total;
        _prevLiveTotal = _liveTotal;
    }

    void OnGUI()
    {
        float w = 360f, h = 40f + _current.Count * 20f;
        GUI.Box(new Rect(10f, 60f, w, h), "Santuario (WASD) — recorre y evalúa el equilibrio");
        float y = 82f;
        GUI.Label(new Rect(20f, y, w - 20f, 20f), $"Vivos: {_liveTotal}   Herbívoros: {_herbTotal}   Depredadores: {_predTotal}");
        y += 20f;
        foreach (KeyValuePair<string, Stat> kv in _current)
        {
            Stat s = kv.Value;
            GUI.Label(new Rect(20f, y, w - 20f, 20f),
                $"{kv.Key}: {s.total}  (cría {s.child} / juv {s.teen} / adulto {s.adult})  hambre {s.hungry} · celo {s.estrus} · enf {s.sick}");
            y += 20f;
        }
    }
}
