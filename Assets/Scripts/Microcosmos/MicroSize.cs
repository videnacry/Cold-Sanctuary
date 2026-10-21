using UnityEngine;

/// <summary>
/// TAMAÑO EN EL MICROCOSMOS: un insecto de la fauna (prefab real, que al nacer fija su propia escala por edad/especie)
/// cuelga de este padre, que ajusta SU escala para que el bicho mida <see cref="length"/> m, relativo a la hormiga, que
/// mide como una persona. Se corrige cada poco (si crece o cambia de edad, sigue a escala).
/// </summary>
public class MicroSize : MonoBehaviour
{
    public float length = 2f;
    float _next;

    void LateUpdate()
    {
        if (Time.time < _next) return;
        _next = Time.time + 0.5f;
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return;
        Bounds b = rs[0].bounds; foreach (Renderer r in rs) b.Encapsulate(r.bounds);
        float now = Mathf.Max(b.size.x, b.size.z);
        if (now < 1e-4f) return;
        float k = length / now;
        if (Mathf.Abs(k - 1f) > 0.05f) transform.localScale *= Mathf.Clamp(k, 0.01f, 1000f);
    }
}
