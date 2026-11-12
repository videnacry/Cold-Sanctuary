using UnityEngine;

/// <summary>
/// UNA PAREJA de insectos-humanos de una era civilizada (docs/microcosmos-civilizaciones.md §7): pueden ser de especies
/// distintas (una mariquita y una araña). Va en la MADRE (con su <see cref="FolkLineage"/>; la del padre es
/// <see cref="FolkLineage.partner"/>). Desde 2026-11-02 el ciclo es EL MISMO que el de los humanos (<see cref="PersonLife"/>:
/// pareja → embarazo de noche en casa → parto a los 9 meses del calendario → el hijo crece con <see cref="BodyAging"/>); lo único
/// propio del microcosmos es la ESPECIE del hijo, que se sortea entre las de su árbol (<see cref="InsectGenealogy"/>): casi
/// siempre como uno de los padres, pero un antepasado lejano puede volver. El hijo nace con la estatura de adulto encogida a la
/// del recién nacido (BodyAging), sigue a su madre (<see cref="MicroJornada.follow"/>) y crece.
/// </summary>
public class FolkCouple : MonoBehaviour
{
    [Tooltip("Hijos como mucho de esta pareja (lo copia su PersonLife).")]
    public int maxChildren = 2;
    public const int MaxPerScene = PersonLife.MaxBirthsPerScene;

    void Start()
    {
        if (TryGetComponent(out PersonLife pl)) pl.maxChildren = maxChildren;
    }

    /// <summary>Nace un hijo de <paramref name="mother"/> y <paramref name="father"/> (<paramref name="roll"/> sortea su especie).</summary>
    public static GameObject Birth(FolkLineage mother, FolkLineage father, float roll)
    {
        var anc = InsectGenealogy.ChildAncestry(mother.name, mother.species, mother.ancestors, father.name, father.species, father.ancestors);
        string sp = InsectGenealogy.ChildSpecies(anc, roll);
        mother.children++; father.children++;
        bool female = Random.value < 0.5f;
        string nm = $"{(female ? "Hija" : "Hijo")} de {Short(mother.name)} y {Short(father.name)} ({mother.children})";
        var g = new GameObject(nm);
        g.transform.SetParent(mother.transform.parent, true);
        float h = AdultHeight(mother, father);
        g.transform.position = mother.transform.position - mother.transform.forward * 0.4f * h;
        g.transform.rotation = mother.transform.rotation;
        var col = g.AddComponent<CapsuleCollider>(); col.height = h; col.radius = 0.2f * h; col.center = Vector3.up * h * 0.5f;
        var ma = mother.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ma != null) { var ag = g.AddComponent<UnityEngine.AI.NavMeshAgent>(); ag.radius = ma.radius; ag.height = h; ag.speed = ma.speed; ag.angularSpeed = ma.angularSpeed; }
        CubeInsect.DressFolk(g, sp, h, nm, female);
        var lin = g.AddComponent<FolkLineage>(); lin.species = sp; lin.female = female; lin.ancestors = anc;
        var wl = g.AddComponent<WorldLabel>(); wl.text = nm; wl.height = h + 0.4f;
        var j = g.AddComponent<MicroJornada>(); j.follow = mother.transform; j.role = "niño";
        MicroJornada mj = mother.GetComponent<MicroJornada>();
        if (mj != null) { j.home = mj.home; j.dine = mj.dine; }
        PersonLife.Born(g, mother.GetComponent<PersonLife>(), father.GetComponent<PersonLife>(), female);
        Debug.Log($"[Microcosmos] Nace {nm}: {sp} (madre {mother.species}, padre {father.species}; posibilidades: {Odds(anc)}).");
        return g;
    }

    // La estatura de adulto del hijo: la media de sus padres (sus cápsulas, a su escala).
    static float AdultHeight(FolkLineage a, FolkLineage b) => 0.5f * (PersonLife.HeightOf(a.transform) + PersonLife.HeightOf(b.transform));

    static string Short(string n) { int p = n.IndexOf(' '); return p > 0 ? n.Substring(0, p) : n; }
    static string Odds(System.Collections.Generic.IEnumerable<InsectGenealogy.Ancestor> anc)
    {
        var s = new System.Text.StringBuilder();
        foreach (var kv in InsectGenealogy.Odds(anc)) s.Append($"{kv.Key} {kv.Value:P0} ");
        return s.ToString().Trim();
    }
}
