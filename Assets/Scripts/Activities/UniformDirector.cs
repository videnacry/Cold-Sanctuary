using UnityEngine;

/// <summary>
/// El VIGILANTE de la ropa y la vajilla (docs/actividades-y-uniformes.md): una vez por segundo, a quien nada sin tener aún quien le
/// cambie la ropa le pone un <see cref="UniformWearer"/> (así se pone el neopreno al entrar en el agua y se lo quita al salir), y
/// recoge la vajilla de las comidas que quedaron a medias (<see cref="MealRitual.Sweep"/>: alguien dejó de comer porque se le cortó la
/// necesidad o la historia lo llamó). Se crea solo al cargar la escena.
/// </summary>
public class UniformDirector : MonoBehaviour
{
    public static UniformDirector Instance { get; private set; }
    /// <summary>¿También el jugador se pone el neopreno al nadar? (es solo ropa: no toca cómo juega)</summary>
    public static bool PlayerToo = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Instance != null) return;
        GameObject g = new GameObject("UniformDirector_AUTO");
        DontDestroyOnLoad(g);
        Instance = g.AddComponent<UniformDirector>();
    }

    float _next;

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + 1f;
        foreach (SwimBody s in FindObjectsByType<SwimBody>(FindObjectsSortMode.None))
        {
            if (s == null || !s.Active || s.GetComponent<UniformWearer>() != null) continue;
            if (s.GetComponent<CreatureRig>() == null && s.GetComponent<CubeInsect>() == null) continue;
            if (!PlayerToo && s.CompareTag("Player")) continue;
            s.gameObject.AddComponent<UniformWearer>();   // (con Wanted = su ropa: al salir del agua, vuelve a ella)
        }
        MealRitual.Sweep();
    }
}
