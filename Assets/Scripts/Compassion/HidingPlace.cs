using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ESCONDITE de las simulaciones de compasión: una casa, una granja, la escuela o una pensión de Le Chambon, el sótano
/// de una familia de Varsovia. Tiene sitio para unos pocos y una despensa: cada día de juego cada escondido gasta una
/// ración, y el jugador la rellena trayendo pan (E junto a un <see cref="BreadSource"/> y luego aquí). Sin comida, los
/// escondidos se inquietan (estrés) y alguno sale a buscarla, y queda a la vista de las patrullas.
/// Mientras está dentro, una <see cref="Patrol"/> no lo ve.
/// </summary>
public class HidingPlace : MonoBehaviour
{
    public string label = "Casa";
    [Min(1)] public int capacity = 3;
    public float radius = 3f;
    [Tooltip("Raciones en la despensa.")]
    public float food = 3f;
    [Tooltip("Cuenta como SALVADO quien se esconde aquí (Le Chambon: las familias los acogían hasta el final de la guerra).")]
    public bool countsAsSaved;

    readonly List<Refugee> _inside = new List<Refugee>();
    public int Count => _inside.Count;
    public bool Full => _inside.Count >= capacity;
    public static readonly List<HidingPlace> All = new List<HidingPlace>();
    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    float _lastDay = -1f;

    /// <summary>Saca a todos los escondidos (no los ya salvados) para que sigan al jugador a otro sitio.</summary>
    public int CallOut(Transform leader)
    {
        int n = 0;
        for (int i = _inside.Count - 1; i >= 0; i--)
        {
            Refugee r = _inside[i];
            if (r == null || r.Saved) continue;
            r.Follow(leader);
            r.transform.position = transform.position + transform.forward * (radius + 1f);
            n++;
        }
        return n;
    }

    public bool TryHide(Refugee r)
    {
        if (r == null || Full || _inside.Contains(r)) return false;
        _inside.Add(r);
        r.Hide(this);
        Vector2 off = Random.insideUnitCircle * radius * 0.25f;   // dentro de la casa: no se le ve
        r.transform.position = transform.position + new Vector3(off.x, 0f, off.y);
        if (countsAsSaved) r.MarkSaved();
        Debug.Log($"[Compasión] «{r.displayName}» queda escondido en {label} ({Count}/{capacity}).");
        return true;
    }

    public void Release(Refugee r) => _inside.Remove(r);

    public void AddFood(float rations) => food += rations;

    void Update()
    {
        TimeController tc = TimeController.timeController;
        if (tc == null || _inside.Count == 0) return;
        float day = Time.time / Mathf.Max(1f, tc.SecondsPerGameDay);
        if (_lastDay < 0f) { _lastDay = day; return; }
        if (day - _lastDay < 1f) return;
        _lastDay = day;
        food -= _inside.Count;
        if (food >= 0f) return;
        food = 0f;
        // Sin comida: el más inquieto sale a buscarla (si ya estaba a salvo, aguanta).
        for (int i = _inside.Count - 1; i >= 0; i--)
        {
            Refugee r = _inside[i];
            if (r == null || r.Saved) continue;
            Anima a = r.GetComponent<Anima>();
            if (a != null) a.stress = Mathf.Clamp01(a.stress + 0.3f);
            Debug.Log($"[Compasión] En {label} no queda comida: «{r.displayName}» sale a buscarla.");
            r.Follow(null);
            r.transform.position += transform.forward * (radius + 1.5f);
            break;
        }
    }
}
