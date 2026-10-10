using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FÁBRICA DE ANIMAS (docs/anyma-factory-and-control.md §2) — crea variantes a partir de un <see cref="AnymaSpec"/>.
/// Alberga las plantillas por especie (prefabs) en un array (los prefabs no se versionan → se asignan en Unity). Un
/// spec **vacío** → todo random dentro de márgenes; con campos, esos se fijan y el resto es random. Instancia el prefab
/// de la especie y aplica el spec DESPUÉS de `Animal.Init` (vía <see cref="AnymaSpecApplier"/>, un frame después).
/// Reutiliza el sistema actual (Animal+catálogos como base/medias); no lo reemplaza.
/// </summary>
public class AnymaFactory : MonoBehaviour
{
    [System.Serializable]
    public class Template
    {
        public string species;      // clave (Bear/Wolf/Ant/…), debe casar con SpeciesArchetype del prefab
        public GameObject prefab;   // prefab con un Animal (AnimalPrefabGenerator lo produce)
    }

    public Template[] templates = System.Array.Empty<Template>();

    /// <summary>Crea UNA anima según el spec (vacío = todo random) en `pos`. Devuelve el GameObject o null.</summary>
    public GameObject Create(AnymaSpec spec, Vector3 pos)
    {
        spec = spec ?? new AnymaSpec();
        Template t = Pick(spec.species);
        if (t == null || t.prefab == null) { Debug.LogWarning($"[AnymaFactory] Sin plantilla para «{spec.species}»."); return null; }
        GameObject go = Instantiate(t.prefab, pos, Quaternion.identity);
        go.AddComponent<AnymaSpecApplier>().spec = spec;
        return go;
    }

    /// <summary>Crea un GRUPO (p. ej. "8 osos: pareja + 2 crías + 2 amigos + 2 solitarios") alrededor de `center`.</summary>
    public List<GameObject> CreateMany(IEnumerable<AnymaSpec> specs, Vector3 center, float radius = 5f)
    {
        var made = new List<GameObject>();
        foreach (AnymaSpec s in specs)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            GameObject g = Create(s, center + new Vector3(r.x, 0f, r.y));
            if (g != null) made.Add(g);
        }
        return made;
    }

    Template Pick(string species)
    {
        if (templates == null || templates.Length == 0) return null;
        if (string.IsNullOrEmpty(species)) return templates[Random.Range(0, templates.Length)];
        foreach (Template t in templates)
            if (t != null && t.species == species) return t;
        return null;
    }
}

/// <summary>Aplica un <see cref="AnymaSpec"/> a un anima recién instanciada, UN FRAME después (tras `Animal.Init`),
/// y se autodestruye. Multiplica los stats sobre la media de la especie y fija hambre/sueño/hogar si el spec los da.</summary>
public class AnymaSpecApplier : MonoBehaviour
{
    public AnymaSpec spec;

    void Start() => StartCoroutine(Apply());

    IEnumerator Apply()
    {
        yield return null;   // deja que Animal.Init fije los stats base de la especie
        Anima a = GetComponent<Anima>();
        if (a == null || spec == null) { Destroy(this); yield break; }

        if (spec.randomizeStats)
        {
            float bm = spec.randomStatsLimitations.Roll();   // multiplicador de CUERPO
            float mm = spec.randomStatsLimitations.Roll();   // multiplicador de MENTE
            a.strength *= bm; a.bodyMass *= bm; a.endurance *= bm; a.agility *= bm; a.perception *= bm;
            a.composure *= mm; a.reasoning *= mm; a.memory *= mm; a.creativity *= mm; a.sociability *= mm;
            a.discipline *= mm; a.adaptability *= mm;
        }

        a.sleepiness = spec.sleepiness >= 0f ? spec.sleepiness : Random.value * 0.5f;
        if (a is Animal an)
        {
            an.hungry = spec.hunger >= 0f ? spec.hunger : Random.value * 0.6f;
            if (spec.hasHome) an.HomeOrigin = spec.home;
        }
        // familyRole / sexualidad / pensamientos random: extensión futura (campos preparados en AnymaSpec).
        Destroy(this);
    }
}
