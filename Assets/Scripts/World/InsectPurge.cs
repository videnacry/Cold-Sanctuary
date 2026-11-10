using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// SIN INSECTOS EN EL SANTUARIO 1 (Berón 2026-11-09, rendimiento): con <see cref="PopulationPlan.InsectsInSanctuary1"/> en false, al cargar
/// el santuario 1 (la escena con <see cref="Territorio"/>) se quitan las abejas y arañas de las nieves y demás insectos (Animal de plan
/// insecto, <see cref="IceInsectLife"/>), sus colmenas (<see cref="Hive"/>) y los enjambres de abejas (<see cref="Swarm"/> de abejas). Las
/// familias de insectos ni se generan (<see cref="FamilyGenerator"/>). Los peces, el kril y las medusas se quedan.
/// </summary>
public static class InsectPurge
{
    public static int Removed { get; private set; }

    public static bool IsSanctuary1(Scene s)
    {
        if (!s.IsValid() || !s.isLoaded) return false;
        foreach (GameObject r in s.GetRootGameObjects()) if (r.GetComponentInChildren<Territorio>(true) != null) return true;
        return false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnLoad()
    {
        SceneManager.sceneLoaded -= Loaded;
        SceneManager.sceneLoaded += Loaded;
        for (int i = 0; i < SceneManager.sceneCount; i++) Purge(SceneManager.GetSceneAt(i));
    }

    static void Loaded(Scene s, LoadSceneMode m) => Purge(s);

    /// <summary>Quita los insectos de esa escena si es el santuario 1 y el interruptor está apagado. Devuelve cuántos objetos quitó.</summary>
    public static int Purge(Scene s)
    {
        if (PopulationPlan.InsectsInSanctuary1 || !IsSanctuary1(s)) return 0;
        int n = 0;
        foreach (GameObject r in s.GetRootGameObjects())
        {
            foreach (IceInsectLife l in r.GetComponentsInChildren<IceInsectLife>(true)) if (l != null) { Object.Destroy(l.gameObject); n++; }
            foreach (Hive h in r.GetComponentsInChildren<Hive>(true)) if (h != null) { Object.Destroy(h.gameObject); n++; }
            foreach (Swarm w in r.GetComponentsInChildren<Swarm>(true)) if (w != null && w.kind == SwarmKind.Bee) { Object.Destroy(w.gameObject); n++; }
            foreach (Animal a in r.GetComponentsInChildren<Animal>(true))
                if (a != null && CubeCreature.IsInsect(a.SpeciesName ?? a.name.Replace("(Clone)", ""))) { Object.Destroy(a.gameObject); n++; }
        }
        Removed += n;
        if (n > 0) Debug.Log($"[Fauna] Santuario 1 sin insectos por ahora (PopulationPlan.InsectsInSanctuary1): {n} quitados (abejas, arañas, colmenas y enjambres de abejas).");
        return n;
    }
}
