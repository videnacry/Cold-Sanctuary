using UnityEngine;

/// <summary>
/// UNA PAREJA de insectos-humanos de una era civilizada (docs/microcosmos-civilizaciones.md §7): pueden ser de especies
/// distintas (una mariquita y una araña). Cada <see cref="daysPerChild"/> días de juego tienen un hijo, con tope por pareja
/// (<see cref="maxChildren"/>) y por escena (<see cref="MaxPerScene"/>): sin bucles sin fin. La especie del hijo se sortea
/// entre las de su árbol (<see cref="InsectGenealogy"/>): casi siempre como uno de los padres, pero un antepasado lejano
/// puede volver. El hijo nace a 0,6× de estatura, sigue a su madre (<see cref="MicroJornada.follow"/>) y crece.
/// Va en la MADRE (con su <see cref="FolkLineage"/>; la del padre es <see cref="FolkLineage.partner"/>).
/// </summary>
public class FolkCouple : MonoBehaviour
{
    public int daysPerChild = 2;
    public int maxChildren = 2;
    public const int MaxPerScene = 24;
    [Tooltip("Días de juego que tarda un hijo en crecer hasta la estatura adulta.")]
    public float growDays = 6f;

    int _lastDay = int.MinValue;
    static int _born;

    int Day => TimeController.timeController != null ? TimeController.timeController.Day : Mathf.FloorToInt(Time.time / 360f);

    void Update()
    {
        if (Time.frameCount % 30 != 0) return;
        FolkLineage me = GetComponent<FolkLineage>();
        if (me == null || me.partner == null || me.children >= maxChildren || _born >= MaxPerScene) return;
        int d = Day;
        if (_lastDay == int.MinValue) { _lastDay = d; return; }
        if (d - _lastDay < daysPerChild) return;
        _lastDay = d;
        Birth(me, me.partner, Random.value);
    }

    /// <summary>Nace un hijo de <paramref name="mother"/> y <paramref name="father"/> (<paramref name="roll"/> sortea su especie).</summary>
    public static GameObject Birth(FolkLineage mother, FolkLineage father, float roll)
    {
        var anc = InsectGenealogy.ChildAncestry(mother.name, mother.species, mother.ancestors, father.name, father.species, father.ancestors);
        string sp = InsectGenealogy.ChildSpecies(anc, roll);
        mother.children++; father.children++; _born++;
        string nm = $"Hijo de {Short(mother.name)} y {Short(father.name)} ({mother.children})";
        var g = new GameObject(nm);
        g.transform.SetParent(mother.transform.parent, true);
        g.transform.position = mother.transform.position - mother.transform.forward * 1.2f;
        bool female = Random.value < 0.5f;
        CubeInsect.DressFolk(g, sp, 1.0f, nm, female);
        var lin = g.AddComponent<FolkLineage>(); lin.species = sp; lin.female = female; lin.ancestors = anc;
        var wl = g.AddComponent<WorldLabel>(); wl.text = nm; wl.height = 1.4f;
        var j = g.AddComponent<MicroJornada>(); j.follow = mother.transform; j.role = "niño";
        g.AddComponent<FolkGrowth>().Begin(1.0f, 1.7f, 6f);
        Debug.Log($"[Microcosmos] Nace {nm}: {sp} (madre {mother.species}, padre {father.species}; posibilidades: {Odds(anc)}).");
        return g;
    }

    static string Short(string n) { int p = n.IndexOf(' '); return p > 0 ? n.Substring(0, p) : n; }
    static string Odds(System.Collections.Generic.IEnumerable<InsectGenealogy.Ancestor> anc)
    {
        var s = new System.Text.StringBuilder();
        foreach (var kv in InsectGenealogy.Odds(anc)) s.Append($"{kv.Key} {kv.Value:P0} ");
        return s.ToString().Trim();
    }
}

/// <summary>El niño crece hasta la estatura adulta en unos días de juego (y deja de seguir a su madre).</summary>
public class FolkGrowth : MonoBehaviour
{
    float _from, _to, _days, _start;
    public void Begin(float from, float to, float days) { _from = from; _to = to; _days = Mathf.Max(0.1f, days); _start = Now; }
    static float Now => TimeController.timeController != null ? TimeController.timeController.Day + TimeController.timeController.Hour / 24f : Time.time / 360f;

    void Update()
    {
        if (Time.frameCount % 60 != 0) return;
        float k = Mathf.Clamp01((Now - _start) / _days);
        CubeInsect.FitHeight(gameObject, Mathf.Lerp(_from, _to, k));
        if (k >= 1f) { MicroJornada j = GetComponent<MicroJornada>(); if (j != null) j.follow = null; Destroy(this); }
    }
}
