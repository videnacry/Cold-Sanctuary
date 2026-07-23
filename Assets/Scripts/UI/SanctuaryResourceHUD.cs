using System.Text;
using UnityEngine;

/// <summary>
/// HUD PROTOTIPO (OnGUI) de los recursos del santuario activo (docs/world-topology-and-planes.md §7):
/// muestra los totales creciendo/decreciendo mientras el jugador está en el Mesocosmos. Es
/// deliberadamente mínimo y sin dependencias de escena; se sustituirá por la UI declarativa
/// (FollowingArrays/Palette) más adelante.
///
/// Regla de guerra (§7): en guerra el jugador solo ve los recursos del santuario del que forma parte.
/// Como de momento el ledger conoce un único santuario, aquí solo se rotula "[EN GUERRA]"; cuando haya
/// varios santuarios, el HUD filtrará por el del jugador.
/// </summary>
public class SanctuaryResourceHUD : MonoBehaviour
{
    [Tooltip("Posición de la esquina del panel, en píxeles desde arriba-izquierda.")]
    public Vector2 origin = new Vector2(12f, 12f);

    GUIStyle _style;

    void OnGUI()
    {
        if (!SanctuaryResources.HasInstance) return;
        SanctuaryResources res = SanctuaryResources.Instance;

        if (_style == null)
            _style = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true };

        var sb = new StringBuilder();
        sb.AppendLine($"<b>{res.sanctuaryName}</b>{(res.atWar ? "  <color=#ff6666>[EN GUERRA]</color>" : "")}");
        foreach (var kv in res.All())
            sb.AppendLine($"{kv.Key}: {Mathf.FloorToInt(kv.Value)}");

        GUI.Label(new Rect(origin.x, origin.y, 280f, 140f), sb.ToString(), _style);
    }
}
