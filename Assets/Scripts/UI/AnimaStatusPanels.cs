using UnityEngine;

/// <summary>
/// HUD DECLARATIVO de estado — la mitad de CÓDIGO (docs/consciousness-mechanics.md §UI): vincula PANELES-GameObject
/// (uno por elemento) a los datos VIVOS de un `Anima`, aplicando el **color directamente al panel** (su `Renderer`) y
/// el valor con unidad a un `TextMesh` opcional, vía <see cref="ElementsStatus"/>. Los paneles (prefabs) los provee el
/// sistema declarativo `FollowingArrays`/`Palette` en Unity — este componente NO los crea; los CONECTA a los datos.
///
/// El "actual" por elemento sale de <see cref="Constitution"/> (`El(símbolo)`, 1.0 = normal) × el ideal por masa; sin
/// `Constitution`, muestra el ideal (verde). Así el color del panel refleja si el ser está deficiente/ideal/en exceso.
/// Es el paso hacia la versión declarativa; el prototipo inmediato es `AnimaStatusHUD` (OnGUI).
/// </summary>
public class AnimaStatusPanels : MonoBehaviour
{
    [System.Serializable]
    public class ElementPanel
    {
        [Tooltip("Símbolo del elemento (O/C/H/N/Ca/Fe…).")] public string element = "O";
        [Tooltip("Panel-GameObject a colorear (su Renderer). Opcional: un TextMesh hijo recibe el valor.")] public GameObject panel;
    }

    [Tooltip("El Anima a mostrar. Si es null, se usa el del tag Player o el primero de la escena.")]
    public Anima target;
    public ElementPanel[] panels;
    [Min(0.05f)] public float refresh = 0.5f;

    float _next;
    Anima _cached;
    Constitution _con;

    void Update()
    {
        if (panels == null || Time.time < _next) return;
        _next = Time.time + refresh;

        Anima a = Resolve();
        if (a == null) return;
        if (_con == null || _con.gameObject != a.gameObject) _con = a.GetComponent<Constitution>();
        float massKg = Mathf.Max(0.001f, a.BodyMass);

        foreach (ElementPanel p in panels)
        {
            if (p == null || p.panel == null || string.IsNullOrEmpty(p.element)) continue;
            float ideal = ElementsStatus.IdealGrams(p.element, massKg);
            float actual = ideal * (_con != null ? _con.El(p.element) : 1f);   // El=1 normal; <1 deficiente; >1 exceso
            (ElementLevel level, Color color) = ElementsStatus.Evaluate(p.element, actual, massKg);

            Renderer r = p.panel.GetComponentInChildren<Renderer>();
            if (r != null && r.material != null) r.material.color = color;
            TextMesh tm = p.panel.GetComponentInChildren<TextMesh>();
            if (tm != null) { tm.text = $"{p.element} {ElementsStatus.Format(actual)}"; tm.color = color; }
        }
    }

    Anima Resolve()
    {
        if (target != null) return target;
        if (_cached != null) return _cached;
        GameObject pl = GameObject.FindGameObjectWithTag("Player");
        if (pl != null && pl.TryGetComponent(out Anima pa)) return _cached = pa;
        return _cached = FindObjectOfType<Anima>();
    }
}
