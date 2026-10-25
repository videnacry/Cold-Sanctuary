using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESCALA REAL de los animales del santuario respecto al humano (≈ 1,75 m). Los modelos traían tamaños dispares (el oso
/// medía 8 m, la orca 23 m, el pingüino medio metro): este componente ajusta la escala para que el adulto mida su largo
/// real, y las crías y los jóvenes, su fracción (≈ 40 % y 70 %). Se corrige cada poco, porque LifeStage reasigna la escala
/// al cambiar de etapa. Las abejas y arañas de hielo (ficticias, del tamaño de una cabeza) y los insectos del microcosmos
/// (MicroSize) no pasan por aquí. Auditoría: menú «Auditoría de escala (animales vs humano)».
/// </summary>
public class SpeciesScaleFix : MonoBehaviour
{
    // Largo real del adulto (m), cabeza a cola; en los erguidos (pingüino), la altura. [A] aproximaciones de manual.
    public static readonly Dictionary<string, float> Real = new Dictionary<string, float>
    {
        ["Bear"] = 2.2f, ["PolarBear"] = 2.2f, ["Seal"] = 1.6f, ["Penguin"] = 1.1f, ["Whale"] = 15f, ["Orca"] = 7f,
        ["Wolf"] = 1.4f, ["Malamute"] = 1.0f, ["Fox"] = 0.65f, ["Bunny"] = 0.6f, ["Deer"] = 1.9f,
    };

    public static bool Has(string species) => !string.IsNullOrEmpty(species) && Real.ContainsKey(species);

    Animal _a;
    float _next;

    void Awake() { _a = GetComponent<Animal>(); }

    void LateUpdate()
    {
        if (_a == null || Time.time < _next) return;
        _next = Time.time + 1f;
        if (_a.death || !Real.TryGetValue(_a.SpeciesName, out float adult)) return;
        if (GetComponentInParent<MicroSize>() != null) return;   // en un microcosmos manda su escala
        float stage = _a.lifeStage == LifeStage.child ? 0.4f : _a.lifeStage == LifeStage.teen ? 0.7f : 1f;
        // Medido en el espacio del PROPIO animal (no la caja del mundo, que cambia con el rumbo: una ballena en diagonal
        // medía un 18 % menos y crecía y encogía al girar). Solo su cuerpo: ni carteles ni lo que lleve encima.
        bool any = false; Bounds b = default;
        Matrix4x4 toRoot = transform.worldToLocalMatrix;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r.GetComponent<WorldLabel>() != null || r.GetComponentInParent<Animal>() != _a) continue;
            Bounds lb = r is SkinnedMeshRenderer sk ? sk.localBounds : r.TryGetComponent(out MeshFilter mf) && mf.sharedMesh != null ? mf.sharedMesh.bounds : default;
            if (lb.size == Vector3.zero) continue;
            Matrix4x4 m = toRoot * r.localToWorldMatrix;
            for (int c = 0; c < 8; c++)
            {
                Vector3 p = m.MultiplyPoint3x4(lb.center + Vector3.Scale(lb.extents, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1)));
                if (!any) { b = new Bounds(p, Vector3.zero); any = true; } else b.Encapsulate(p);
            }
        }
        if (!any) return;
        float now = Mathf.Max(b.size.x, b.size.y, b.size.z) * transform.lossyScale.x;   // a la escala actual
        if (now < 1e-3f) return;
        float k = adult * stage / now;
        if (Mathf.Abs(k - 1f) > 0.08f) transform.localScale *= Mathf.Clamp(k, 0.05f, 20f);
    }
}
