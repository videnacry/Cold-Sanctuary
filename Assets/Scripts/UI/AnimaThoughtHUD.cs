using UnityEngine;

/// <summary>
/// HUD del PENSAMIENTO del ser que MANEJA el jugador (docs/microcosmos-dungbeetle-level.md §5). El jugador puede
/// controlar CUALQUIER `Anima` (posesión: cambia el puntero de control + la cámara); esta UI lee la `Mind` del ser
/// cuyo `AnimaController.Active` es un `PlayerBrain` y muestra su **pensamiento actual + intensidad** (verde=positivo /
/// rojo=negativo). Es el canal por el que el jugador "lee" a Kushal/al poseído (miedo a acercarse, deseo…), pilar del
/// modelo **jugador = conector, no titiritero**. El pensamiento **persiste ~10 min** (o hasta que la Mente formule otro).
///
/// Prototipo OnGUI (como <see cref="AnimaStatusHUD"/>); la versión declarativa (un bloque FollowingArrays visible con
/// el menú cerrado) es el paso siguiente. Si no hay ser poseído, cae al tag Player, y si no, no muestra nada.
/// </summary>
public class AnimaThoughtHUD : MonoBehaviour
{
    [Tooltip("Segundos que un pensamiento sigue visible tras formularse (por defecto ~10 min).")]
    public float linger = 600f;
    public Vector2 origin = new Vector2(12f, 12f);
    public float width = 360f;

    GUIStyle _style;
    Mind _cached;
    float _recheck;

    // El ser que maneja el jugador = aquel cuyo cerebro ACTIVO es un PlayerBrain. Se re-busca cada poco (la posesión
    // mueve al jugador de cuerpo). Cae al tag Player si no hay posesión activa.
    Mind ControlledMind()
    {
        if (Time.time < _recheck && _cached != null) return _cached;
        _recheck = Time.time + 0.5f;

        foreach (AnimaController ac in FindObjectsOfType<AnimaController>())
            if (ac != null && ac.Active is PlayerBrain && ac.TryGetComponent(out Mind m)) return _cached = m;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && p.TryGetComponent(out Mind pm)) return _cached = pm;
        return _cached = null;
    }

    void OnGUI()
    {
        Mind mind = ControlledMind();
        if (mind == null || string.IsNullOrEmpty(mind.lastThought)) return;
        if (Time.time - mind.lastThoughtTime > linger) return;   // caducó

        if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true, wordWrap = true };

        Color c = mind.lastPositive ? ElementsStatus.ColorOf(ElementLevel.Ideal)
                                    : ElementsStatus.ColorOf(ElementLevel.Deficiente);
        int pct = Mathf.RoundToInt(mind.lastIntensity * 100f);
        // Intensidad como puntos ● (0..3) + %; el nombre del ser + su pensamiento en su color de valencia.
        int dots = Mathf.RoundToInt(mind.lastIntensity * 3f);
        string bar = new string('●', dots) + new string('·', 3 - dots);
        string who = string.IsNullOrEmpty(mind.identity) ? mind.name : mind.identity;

        string txt = $"<b>{who}</b>  <color=#{Hex(c)}>{bar}</color> <size=11>{pct}%</size>\n" +
                     $"<color=#{Hex(c)}><i>\"{mind.lastThought}\"</i></color>";
        GUI.Label(new Rect(origin.x, origin.y, width, 90f), txt, _style);
    }

    static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
}
