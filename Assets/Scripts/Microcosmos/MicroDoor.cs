using UnityEngine;

/// <summary>
/// UNA PUERTA DE EDIFICIO DE MICROCOSMOS (docs/microcosmos/plan-microcosmos-realismo.md §3.3, P3–P4; Berón 2026-11-18): el hueco libre de una
/// fachada o de un tabique, en el marco local de este objeto (centro del umbral en el suelo, el paso hacia ±z). Una puerta solo
/// da paso (nunca abre menús). Las reales se ponen a su medida; las de menos de <see cref="MicroBuildRules.DoorWidth"/> m de ancho
/// se ENSANCHAN a esa medida y se anota «supuesto» (<see cref="widened"/>, <see cref="realWidth"/>), porque el cuerpo recogido no
/// pasaría. La comprueba <see cref="MicroBuildRules.Validate"/> (P4). Lo usa también la vista de corte (<see cref="CutawayView"/>):
/// los muros con puerta se desvanecen igual que los demás.
/// </summary>
public class MicroDoor : MonoBehaviour
{
    [Tooltip("Ancho libre del hueco (m), ya ensanchado si hizo falta.")]
    public float width = MicroBuildRules.DoorWidth;
    [Tooltip("Alto libre del hueco (m).")]
    public float height = 2.1f;
    [Tooltip("Ancho de la puerta en el lugar real (m); 0 si no se sabe.")]
    public float realWidth;
    [Tooltip("¿Se ensanchó a MicroBuildRules.DoorWidth (la real era más estrecha)? Es un «supuesto» en la ficha del lugar.")]
    public bool widened;
    [Tooltip("Fuente del dato (plano, foto, supuesto).")]
    public string source = "";

    /// <summary>El ancho que se construye para una puerta real de <paramref name="real"/> m: la real, o
    /// <see cref="MicroBuildRules.DoorWidth"/> si es más estrecha (P4).</summary>
    public static float BuildWidth(float real) => Mathf.Max(real, MicroBuildRules.DoorWidth);

    /// <summary>Pone una puerta en <paramref name="parent"/> (umbral en <paramref name="local"/>, girada <paramref name="yaw"/>) para
    /// un hueco real de <paramref name="realW"/> × <paramref name="realH"/> m, ensanchándola si hace falta (P4).</summary>
    public static MicroDoor Place(Transform parent, string name, Vector3 local, float yaw, float realW, float realH, string source = "")
    {
        var g = new GameObject(name);
        g.transform.SetParent(parent, false); g.transform.localPosition = local; g.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        var d = g.AddComponent<MicroDoor>();
        d.realWidth = realW; d.width = BuildWidth(realW); d.widened = d.width > realW + 1e-3f;
        d.height = Mathf.Max(realH, MicroBuildRules.DoorHeight);
        d.source = d.widened ? (string.IsNullOrEmpty(source) ? "supuesto: ensanchada" : source + " (supuesto: ensanchada)") : source;
        return d;
    }

    /// <summary>¿Pasa por ella un cuerpo de <paramref name="walkWidth"/> m de ancho andando y <paramref name="tall"/> de alto?</summary>
    public bool Fits(float walkWidth, float tall) => width >= walkWidth && height >= tall;

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = widened ? new Color(1f, 0.6f, 0.1f) : new Color(0.2f, 0.9f, 0.3f);
        Gizmos.DrawWireCube(new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, 0.1f));
    }
}
