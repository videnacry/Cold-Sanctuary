using UnityEngine;

/// <summary>
/// DEMOSTRADOR de una sesión de yoga: viste al ser con un cuerpo de cubos articulado (<see cref="CubeHumanoid"/>) y
/// ejecuta en bucle la secuencia de la sesión (<see cref="YogaLibrary"/>) por la <see cref="ActionQueue"/>. Pensado para
/// tener **una escena por sesión** (cada práctica se revisa por separado). Autónomo: no necesita a la Magnate.
/// </summary>
[RequireComponent(typeof(Anima))]
public class YogaSessionRunner : MonoBehaviour
{
    [Tooltip("Nombre exacto de la sesión en YogaLibrary (p. ej. 'Saludo al Sol').")]
    public string sessionName = "Saludo al Sol";
    public bool loop = true;
    public Color clothes = new Color(0.45f, 0.5f, 0.7f);
    public Color skin = new Color(0.85f, 0.7f, 0.55f);

    ActionQueue _q;

    void Start()
    {
        if (GetComponent<CreatureRig>() == null || transform.Find(CubeHumanoid.BodyName) == null)
            CubeHumanoid.Dress(gameObject, clothes, skin);
        _q = this.GetOrAdd<ActionQueue>();
        Enqueue();
    }

    void Update()
    {
        if (_q != null && loop && _q.Own.Count == 0) Enqueue();
    }

    void Enqueue()
    {
        YogaSession s = YogaLibrary.Find(sessionName);
        if (s == null) { Debug.LogWarning($"[YogaSessionRunner] sesión desconocida: «{sessionName}»."); enabled = false; return; }
        foreach (QueuedSpell sp in s.Build()) _q.EnqueueOwn(sp);
    }
}
