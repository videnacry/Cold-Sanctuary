using System.Text;
using UnityEngine;

/// <summary>
/// VENTANA DE RECURSOS del santuario activo (docs/world-topology-and-planes.md §7, docs/interfaz-botones.md): los totales y las margas
/// de Kushal. Era un panel fijo arriba a la izquierda; desde el 2026-11-05 (Berón) se abre con el botón «Recursos» de la pila de la
/// derecha o con F3 (<see cref="HudModals"/>) y se cierra con Esc. Prototipo OnGUI, sin dependencias de escena; se sustituirá por la UI
/// declarativa (FollowingArrays/Palette) más adelante.
///
/// Regla de guerra (§7): en guerra el jugador solo ve los recursos del santuario del que forma parte.
/// Como de momento el ledger conoce un único santuario, aquí solo se rotula "[EN GUERRA]"; cuando haya
/// varios santuarios, la ventana filtrará por el del jugador.
/// </summary>
public class SanctuaryResourceHUD : MonoBehaviour
{
    /// <summary>El de la escena (es el que dibuja la ventana); sin él, el botón no sale.</summary>
    public static SanctuaryResourceHUD Instance { get; private set; }

    GUIStyle       _style;
    Vector2        _scroll;
    CharacterLevel _level;   // nivel del jugador (cacheado)

    void OnEnable() { Instance = this; }
    void OnDisable() { if (Instance == this) Instance = null; }

    /// <summary>El texto de la ventana: lee el libro de recursos y el nivel en cada repintado (no guarda copia).</summary>
    public string Text()
    {
        if (!SanctuaryResources.HasInstance) return "";
        SanctuaryResources res = SanctuaryResources.Instance;
        var sb = new StringBuilder();
        sb.AppendLine($"<b>{res.sanctuaryName}</b>{(res.atWar ? "  <color=#ff6666>[EN GUERRA]</color>" : "")}");
        foreach (var kv in res.All())
            sb.AppendLine($"{kv.Key}: {Mathf.FloorToInt(kv.Value)}");

        // Nivel del personaje del jugador (XP/vida/maná) — docs §4.1.
        if (_level == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) p.TryGetComponent(out _level);
        }
        if (_level != null)
        {
            sb.AppendLine("");
            sb.AppendLine($"<b>Kushal</b> — margas del alma");
            sb.AppendLine($"Stats {_level.stats.level} (xp {_level.stats.xp:0}/{_level.stats.XpToNext:0})");
            sb.AppendLine($"Yoga {_level.yoga.level}   ·   Vínculos {_level.bonds.level}");
            sb.AppendLine($"Vida: {_level.currentHealth:0}/{_level.MaxHealth:0}");
            string mana = _level.ManaUnlocked
                ? $"   Maná: {_level.currentMana:0}/{_level.MaxMana:0}"
                : "   Maná: (bloqueado — practica yoga)";
            sb.AppendLine($"Energía: {_level.currentEnergy:0}/{_level.MaxEnergy:0}{mana}");
            sb.AppendLine($"Def: {_level.PassiveDefense:0}   Poder: {_level.SpellPower:0.0}");
        }
        return sb.ToString();
    }

    void OnGUI()
    {
        if (HudModals.Current != HudModals.Modal.Recursos || !SanctuaryResources.HasInstance) return;
        if (_style == null)
            _style = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true };

        string text = Text();
        float w = Mathf.Min(420f, Screen.width - 40f), textH = _style.CalcHeight(new GUIContent(text), w - 40f);
        float h = Mathf.Min(Screen.height - 120f, textH + 44f);
        var r = new Rect((Screen.width - w) * 0.5f, 60f, w, h);
        GUI.Box(r, ""); GUI.Box(r, "");
        GUI.Label(new Rect(r.x + 12f, r.y + 6f, w - 100f, 22f), "<b>✦ Recursos</b>  <size=11>(Esc o F3 para cerrar)</size>", _style);
        if (GUI.Button(new Rect(r.xMax - 80f, r.y + 6f, 68f, 22f), "cerrar")) HudModals.Close();
        var view = new Rect(0f, 0f, w - 40f, textH);
        _scroll = GUI.BeginScrollView(new Rect(r.x + 8f, r.y + 34f, w - 16f, h - 42f), _scroll, view);
        GUI.Label(new Rect(4f, 0f, view.width, textH), text, _style);
        GUI.EndScrollView();
    }
}
