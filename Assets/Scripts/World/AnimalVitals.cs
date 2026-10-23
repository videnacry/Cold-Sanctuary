using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// REGISTRO VITAL de un animal para el CENSO (`censo` en el lanzador, y `[ECO]` en el log): cuándo comió por última vez,
/// cuánto ha dormido, cuántas veces ha ido al baño, cuánto ha recorrido (merodeo) y si lleva rato sin moverse (atascado).
/// Solo observa: no cambia su conducta.
/// </summary>
public class AnimalVitals : MonoBehaviour
{
    public float lastAte = -1f, sleptSeconds, walked;
    public int defecations, meals;
    public float stillSince;
    Animal _a;
    Vector3 _last;
    float _lastHungry;

    void Start() { _a = GetComponent<Animal>(); _last = transform.position; _lastHungry = _a != null ? _a.hungry : 0f; stillSince = Time.time; }

    void Update()
    {
        if (_a == null || _a.death) return;
        Vector3 p = transform.position;
        float d = Vector3.Distance(p, _last);
        if (d > 0.002f) { walked += d; if (d / Mathf.Max(1e-4f, Time.deltaTime) > 0.15f) stillSince = Time.time; }
        _last = p;
        if (_a.hungry < _lastHungry - 0.02f) { lastAte = Time.time; meals++; }   // le bajó el hambre: ha comido
        _lastHungry = _a.hungry;
        if (_a.asleep) sleptSeconds += Time.deltaTime;
    }

    public void Defecated() { defecations++; }

    /// <summary>Resumen por especie de los animales vivos.</summary>
    public static List<string> Census()
    {
        var by = new SortedDictionary<string, List<Animal>>();
        foreach (Animal a in FindObjectsByType<Animal>(FindObjectsSortMode.None))
        {
            if (a == null || a.death) continue;
            if (!by.TryGetValue(a.SpeciesName, out var l)) by[a.SpeciesName] = l = new List<Animal>();
            l.Add(a);
        }
        var lines = new List<string>();
        foreach (var kv in by)
        {
            int child = 0, teen = 0, adult = 0, hungry = 0, sleeping = 0, ate = 0, pooped = 0, wander = 0, stuck = 0, inWater = 0, female = 0, habit = 0, habitNear = 0, busy = 0;
            var won = new SortedDictionary<string, int>();
            foreach (Animal a in kv.Value)
            {
                if (a.lifeStage == LifeStage.child) child++; else if (a.lifeStage == LifeStage.teen) teen++; else adult++;
                if (a.hungry >= 0f) hungry++;
                if (a.asleep) sleeping++;
                if (a.sex == Sex.female) female++;
                if (a.transform.position.y < -0.3f) inWater++;
                bool hasHabit = a.TryGetComponent(out Habit h);
                bool natural = (hasHabit && h.Active) || (a.TryGetComponent(out MarineLife m) && m.InHabitat);   // acechando, posado, en su respiradero: quieto a propósito
                if (natural) habit++;
                if (natural || (hasHabit && h.HasOptions())) habitNear++;   // diagnóstico: ¿encuentra dónde hacer sus cosas?
                if (a.busy) busy++;   // ocupado en una corrutina (comer/cazar): ni volición ni hábito
                if (a.TryGetComponent(out Volition vo) && vo.LastDesire != "") won[vo.LastDesire] = won.TryGetValue(vo.LastDesire, out int c) ? c + 1 : 1;
                if (a.TryGetComponent(out AnimalVitals v))
                {
                    if (v.meals > 0) ate++;
                    if (v.defecations > 0) pooped++;
                    if (v.walked > 10f) wander++;
                    if (!a.asleep && !natural && Time.time - v.stillSince > 60f) stuck++;
                }
            }
            lines.Add($"{kv.Key}: {kv.Value.Count} (crías {child}, jóvenes {teen}, adultos {adult}; hembras {female}) · con hambre {hungry} · durmiendo {sleeping} · han comido {ate} · han ido al baño {pooped} · han merodeado {wander} · en un hábito natural {habit} (con sitio cerca {habitNear}; ocupados {busy}; último deseo: {string.Join(", ", System.Linq.Enumerable.Select(won, w => w.Key + " " + w.Value))}) · quietos >1 min {stuck} · en el agua {inWater}");
        }
        return lines;
    }
}
