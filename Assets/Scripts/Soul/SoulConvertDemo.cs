using UnityEngine;

/// <summary>Demo (OnGUI) de la CONVERSIÓN de stats (docs/soul-relations-reincarnation §1) para el sandbox
/// `AlmaBlend_AUTO`. Guarda los stats originales y permite convertir a hormiga+humano en modo **A (relativa)** o
/// **B (literal)** y **resetear** para comparar desde el mismo punto. Solo para el sandbox.</summary>
public class SoulConvertDemo : MonoBehaviour
{
    public SoulComposition soul;
    public Anima anima;

    Aptitudes _orig;
    bool _saved;
    string _last = "-";

    void Awake()
    {
        if (soul == null) soul = GetComponent<SoulComposition>();
        if (anima == null) anima = GetComponent<Anima>();
    }

    void Start() { Invoke(nameof(Save), 0.2f); }   // tras el Resolve inicial
    void Save() { if (soul != null) { _orig = soul.ReadStats(); _saved = true; } }

    void OnGUI()
    {
        if (soul == null || anima == null) return;
        int x = 392, y = 152;
        GUI.Box(new Rect(x, y, 342, 150), "SoulConvert (conversión A/B) — docs soul-relations §1");
        y += 24;
        GUI.Label(new Rect(x + 8, y, 326, 20),
            $"str {anima.strength:0.00}  masa {anima.bodyMass:0.00}  agi {anima.agility:0.00}  razón {anima.reasoning:0.00}");
        y += 20;
        GUI.Label(new Rect(x + 8, y, 326, 20), $"Último: {_last}   (resetea para comparar A vs B)");
        y += 26;
        if (GUI.Button(new Rect(x + 8, y, 158, 26), "→ Hormiga+Human (A relativa)"))
        { soul.ConvertTo("Ant", "Human", ConversionMode.Relative); _last = "A/relativa → Ant+Human"; }
        if (GUI.Button(new Rect(x + 176, y, 158, 26), "→ Hormiga+Human (B literal)"))
        { soul.ConvertTo("Ant", "Human", ConversionMode.Literal); _last = "B/literal → Ant+Human"; }
        y += 30;
        if (_saved && GUI.Button(new Rect(x + 8, y, 158, 26), "Reset (stats originales)"))
        { soul.WriteStats(_orig); _last = "reset"; }
    }
}
