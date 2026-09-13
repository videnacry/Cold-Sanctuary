using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HECHIZO PASIVO DE LOS OJOS: OBSERVAR (docs/apremios-guardian-observacion.md §5) — mirar SOSTENIDO a un ánima (o a la
/// tarea que se tiene delante) produce **observación/ecuanimidad**: entrena la habilidad (<see cref="ObservationSkill"/>,
/// evolución por uso) y da un impulso temporal de calma. Es el pasivo que conceden los OJOS; en el modelo de anatomía se
/// gatea por `CharacterComposition.grants` ("receptor visual") — aquí se añade al ser que tiene ojos (p.ej. Sakshi, que
/// débil solo puede observar). Auto-crea el `ObservationSkill` si falta. Balance-safe: sin objetivo a la vista, no hace nada.
/// </summary>
public class ObserveSpell : MonoBehaviour
{
    [Tooltip("Segundos ATENDIENDO al mismo objetivo para que cuente como 'observar'.")]
    [Min(0f)] public float gazeSeconds = 1.5f;
    [Tooltip("SENTIDOS con los que este ser observa (docs/consciousness-mechanics §3). Por defecto los 5 básicos; un " +
             "topo podría ser {olfato,tacto}, un murciélago {eco,oido}… El alcance sale del mejor sentido (Senses).")]
    public string[] senses = { "vista", "oido", "olfato", "gusto", "tacto" };
    [Tooltip("Radio de respaldo si sus sentidos no dan alcance (0).")]
    [Min(0.5f)] public float fallbackRange = 8f;
    [Tooltip("Cuánto ENTRENA la observación por segundo mientras observa (sube despacio).")]
    [Min(0f)] public float trainPerSecond = 0.02f;
    [Tooltip("Impulso de ECUANIMIDAD por segundo mientras observa (decae luego).")]
    [Min(0f)] public float calmPerSecond = 0.4f;

    Anima _self;
    ObservationSkill _skill;
    Transform _gazed;
    float _gazeTime;

    void Awake()
    {
        _self = GetComponent<Anima>();
        _skill = GetComponent<ObservationSkill>();
        if (_skill == null) _skill = gameObject.AddComponent<ObservationSkill>();
    }

    [Tooltip("Velocidad a la que la ALERTA sube/baja con lo que se percibe (percepción → alertness → decisión).")]
    [Min(0f)] public float alertnessRate = 0.8f;

    void Update()
    {
        Transform t = NearestAnima();
        if (t != null && t == _gazed) _gazeTime += Time.deltaTime;
        else { _gazed = t; _gazeTime = 0f; }

        if (_gazed != null && _gazeTime >= gazeSeconds)
        {
            _skill.Train(trainPerSecond * Time.deltaTime);    // mirar sostenido → sube la observación (evolución por uso)
            _skill.AddBoost(calmPerSecond * Time.deltaTime);  // y da ecuanimidad AHORA (amortigua el sufrimiento)
        }

        // PERCEPCIÓN → ALERTNESS: la alerta sube hacia la mayor calidad de lo que se PERCIBE del más cercano; cae si no
        // hay nada. Así un ser solo se pone en guardia por lo que sus sentidos (o el grimoire) alcanzan (idea del usuario).
        float strongest = 0f;
        if (t != null)
        {
            Anima ta = t.GetComponentInParent<Anima>();
            foreach (KeyValuePair<PerceptChannel, float> kv in Perceive(ta)) strongest = Mathf.Max(strongest, kv.Value);
        }
        if (_self != null) _self.alertness = Mathf.MoveTowards(_self.alertness, strongest, alertnessRate * Time.deltaTime);
    }

    /// <summary>Alcance de observación: el MEJOR de sus ÓRGANOS (SenseOrgan) o, si no tiene, el mejor de su lista `senses`.</summary>
    float Reach()
    {
        float reach = 0f;
        foreach (SenseOrgan o in GetComponents<SenseOrgan>()) reach = Mathf.Max(reach, o.Reach);
        if (reach <= 0f) reach = Senses.BestReach(senses);
        return reach > 0f ? reach : fallbackRange;
    }

    Transform NearestAnima()
    {
        float reach = Reach();
        Transform best = null; float bestSq = reach * reach;
        foreach (Collider c in Physics.OverlapSphere(transform.position, reach))
        {
            Anima a = c.GetComponentInParent<Anima>();
            if (a == null || a == _self) continue;
            float d = (a.transform.position - transform.position).sqrMagnitude;
            if (d < bestSq) { bestSq = d; best = a.transform; }
        }
        return best;
    }

    /// <summary>QUÉ información extrae este ser de un objetivo: calidad [0,1] por <see cref="PerceptChannel"/>. Agrega los
    /// canales de sus ÓRGANOS (máxima calidad), atenuada por la distancia; y, si conoce el hechizo "observar" por el
    /// GRIMORIO (vía mágica), añade TODOS los canales a una calidad = su confianza en el hechizo (más habilidad → más
    /// información). Devuelve un dict canal→calidad. docs/consciousness-mechanics.md §3.</summary>
    public Dictionary<PerceptChannel, float> Perceive(Anima target)
    {
        var result = new Dictionary<PerceptChannel, float>();
        if (target == null || _self == null) return result;

        float dist = Vector3.Distance(transform.position, target.transform.position);

        // Vía ANATÓMICA: cada órgano, si su alcance cubre al objetivo, aporta sus canales atenuados por la distancia.
        foreach (SenseOrgan o in GetComponents<SenseOrgan>())
        {
            float reach = o.Reach;
            if (reach <= 0.01f || dist > reach) continue;
            float atten = Mathf.Clamp01(1f - dist / reach);
            if (o.provides != null)
                foreach (SenseOrgan.ChannelQuality cq in o.provides)
                {
                    float q = cq.quality * atten;
                    if (q > 0f && (!result.TryGetValue(cq.channel, out float cur) || q > cur)) result[cq.channel] = q;
                }
        }

        // Vía MÁGICA (grimorio): "observar" aprendido → todos los canales a calidad = confianza/100 (habilidad del hechicero).
        if (_self.KnowsSpell("observar"))
        {
            float magic = _self.Confidence("observar") / 100f;
            if (magic > 0f)
                foreach (PerceptChannel ch in System.Enum.GetValues(typeof(PerceptChannel)))
                    if (!result.TryGetValue(ch, out float cur) || magic > cur) result[ch] = magic;
        }
        return result;
    }
}
