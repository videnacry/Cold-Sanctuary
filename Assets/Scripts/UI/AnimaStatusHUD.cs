using System.Text;
using UnityEngine;

/// <summary>
/// HUD de ESTADO de un Ánima (docs/consciousness-mechanics.md §2) — hace VISIBLE lo que hay que autorregular: los drives
/// (estrés/sueño/fatiga/satisfacción), la ZONA de actividad (ACWR) y la composición de elementos IDEAL por masa. Cada
/// valor sale con **número + unidad + COLOR** (verde=estable / naranja=fuera de rango / rojo=crítico) — para que el
/// jugador sepa si Kushal debe comer, descansar o entrenar. Prototipo OnGUI (como `SanctuaryResourceHUD`); la versión
/// declarativa (FollowingArrays: un panel-elemento con su color) es el paso siguiente. Colores por `ElementsStatus`.
///
/// Objetivo: el `Anima` de <see cref="target"/>; si es null, el del tag Player, si no, el `Anima` más cercano.
/// </summary>
public class AnimaStatusHUD : MonoBehaviour
{
    public Anima target;
    public Vector2 origin = new Vector2(12f, 180f);
    public bool showElements = true;

    GUIStyle _style;
    Anima _cached;

    Anima Target()
    {
        if (target != null) return target;
        if (_cached != null) return _cached;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && p.TryGetComponent(out Anima pa)) return _cached = pa;
        return _cached = FindObjectOfType<Anima>();
    }

    void OnGUI()
    {
        Anima a = Target();
        if (a == null) return;
        if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<b>{a.name} — estado</b>");

        // Drives (0..1). En estos, MENOS es mejor (badHigh) salvo la satisfacción.
        sb.AppendLine(Bar("Estrés",       a.stress,        badHigh: true));
        sb.AppendLine(Bar("Sueño",        a.sleepiness,    badHigh: true));
        sb.AppendLine(Bar("Fatiga",       a.mentalFatigue, badHigh: true));
        sb.AppendLine(Bar("Enfermedad",   a.sickness,      badHigh: true));
        sb.AppendLine(Bar("Satisfacción", a.satisfaction,  badHigh: false));
        sb.AppendLine(Bar("Alerta",       a.alertness,     badHigh: false));   // percepción → alertness
        if (a is Animal an) sb.AppendLine(Bar("Hambre", Mathf.Clamp01(an.hungry), badHigh: true));

        // Actividad (ACWR): la zona Bendecida es la ideal.
        ActivityLevel act = a.GetComponent<ActivityLevel>();
        if (act != null)
        {
            Color zc = act.Zone == ActivityZone.Bendecido ? ElementsStatus.ColorOf(ElementLevel.Ideal)
                                                          : ElementsStatus.ColorOf(ElementLevel.Bajo);
            sb.AppendLine($"Actividad: <color=#{Hex(zc)}>{act.Zone}</color> (ACWR {act.Acwr:0.00})");
        }

        // Elementos: composición IDEAL por masa (educativo + demuestra número/unidad/color).
        if (showElements)
        {
            float massKg = Mathf.Max(0.001f, a.BodyMass);
            sb.AppendLine($"<b>Elementos (ideal para {massKg:0.##} kg)</b>");
            foreach (string el in new[] { "O", "C", "H", "N", "Ca", "P" })
            {
                float ideal = ElementsStatus.IdealGrams(el, massKg);
                Color c = ElementsStatus.ColorOf(ElementLevel.Ideal);   // sin fuente de "actual" aún → se muestra el ideal
                sb.AppendLine($"  {el}: <color=#{Hex(c)}>{ElementsStatus.Format(ideal)}</color>");
            }
        }

        GUI.Label(new Rect(origin.x, origin.y, 320f, 460f), sb.ToString(), _style);
    }

    // Un valor 0..1 → "etiqueta: NN% " coloreado (verde bien / rojo mal). badHigh: alto = malo (estrés); si no, alto = bueno.
    static string Bar(string label, float v01, bool badHigh)
    {
        v01 = Mathf.Clamp01(v01);
        float bad = badHigh ? v01 : 1f - v01;               // 0 = perfecto, 1 = crítico
        Color c = bad < 0.34f ? ElementsStatus.ColorOf(ElementLevel.Ideal)
                : bad < 0.67f ? ElementsStatus.ColorOf(ElementLevel.Bajo)
                :               ElementsStatus.ColorOf(ElementLevel.Deficiente);
        return $"{label}: <color=#{Hex(c)}>{Mathf.RoundToInt(v01 * 100f)}%</color>";
    }

    static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
}
