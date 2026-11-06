using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// EL SOL POLAR DEL SANTUARIO: mueve la luz direccional de la escena con la hora y la estación del reloj de juego
/// (<see cref="SolarPosition.Sanctuary"/>): sol de medianoche rasante y dorado, noches polares con luna, y días y noches
/// en las transiciones. <see cref="TimeController.IsDay"/> (6–18 h) sigue siendo el horario humano.
/// </summary>
public class SunCycle : MonoBehaviour
{
    Light _light;
    public SunPos Now { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Attach()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SampleScene") return;
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Directional && l.GetComponent<SunCycle>() == null) { l.gameObject.AddComponent<SunCycle>(); return; }
    }

    void Awake() { _light = GetComponent<Light>(); }

    void Update()
    {
        TimeController t = TimeController.timeController; if (t == null || _light == null) return;
        Now = SolarPosition.Sanctuary(t);
        float L = SolarPosition.Apply(_light, Now);
        // El ambiente solo si su escena manda (en un microcosmos cargado encima manda la luz de allí); la nieve refleja.
        if (gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene()) RenderSettings.ambientIntensity = 0.3f + 0.7f * L;
    }
}
