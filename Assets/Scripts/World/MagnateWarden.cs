using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA MAGNATE GUARDA LOS PUESTOS (Berón 2026-10-22). Nadie queda encerrado en su área: se puede salir. Pero si alguien
/// abandona su puesto (su trabajo de la jornada) o un recién llegado sale de la enfermería sin terminar la iniciación,
/// la Magnate, que conoce todos los hechizos, se TELETRANSPORTA delante de él, le dice por qué no puede irse y lo
/// TELETRANSPORTA de vuelta dentro del área, junto a ella. Luego ella vuelve a donde estaba.
///
/// Sustituye a los guardias que perseguían al fugitivo y lo arrastraban («jalar»), que se movían por el aire y
/// atravesaban cosas. Aquí no hay persecución: dos saltos instantáneos y una pausa para que se lea el aviso.
/// </summary>
public class MagnateWarden : MonoBehaviour
{
    static MagnateWarden _inst;
    public static MagnateWarden Instance
    {
        get
        {
            if (_inst == null) _inst = new GameObject("MagnateWarden_AUTO").AddComponent<MagnateWarden>();
            return _inst;
        }
    }

    [Tooltip("Segundos que la Magnate se queda delante del que se iba, hablándole, antes de devolverlo.")]
    public float talk = 2.2f;
    [Tooltip("Segundos que se queda junto a él dentro del área antes de volver a lo suyo.")]
    public float stay = 1.5f;
    [Tooltip("Tras devolver a alguien, segundos de gracia antes de poder volver a traerlo.")]
    public float grace = 4f;

    public static int Returned { get; private set; }

    readonly HashSet<GameObject> _busy = new HashSet<GameObject>();
    readonly Dictionary<GameObject, float> _graceUntil = new Dictionary<GameObject, float>();
    string _msg; float _msgUntil;

    /// <summary>¿La Magnate está ahora devolviendo a <paramref name="who"/> (o acaba de hacerlo), o es la propia Magnate
    /// en plena aparición? (La jornada no los mueve mientras tanto.)</summary>
    public bool Handling(GameObject who) =>
        who != null && (_busy.Contains(who) || _magnates.Contains(who) || (_graceUntil.TryGetValue(who, out float t) && Time.time < t));

    readonly HashSet<GameObject> _magnates = new HashSet<GameObject>();

    /// <summary>La Magnate aparece delante de <paramref name="who"/>, le dice <paramref name="why"/> y lo lleva a
    /// <paramref name="back"/> (dentro de su área). No hace nada si ya está en ello.</summary>
    public static void Bring(GameObject who, Vector3 back, string why)
    {
        if (who == null || Instance.Handling(who)) return;
        Instance.StartCoroutine(Instance.Run(who, back, why));
    }

    IEnumerator Run(GameObject who, Vector3 back, string why)
    {
        _busy.Add(who);
        Magnate real = FindAnyObjectByType<Magnate>();
        bool clone = real == null || _magnates.Contains(real.gameObject);   // si ya está devolviendo a otro, va un clon suyo
        GameObject m = clone ? WorkdayDirector.SpawnMagnateBody("Magnate (aparición)") : real.gameObject;
        Vector3 home = m.transform.position; Quaternion homeRot = m.transform.rotation;
        Behaviour[] paused = Pause(m);
        _magnates.Add(m);

        // 1) APARECE delante de él, mirándolo.
        Vector3 fwd = Flat(who.transform.forward);
        Vector3 front = who.transform.position + fwd * 1.6f;
        WorkdayDirector.Teleport(m.transform, front);
        m.transform.rotation = Quaternion.LookRotation(-fwd);
        Debug.Log($"[Magnate] Se teletransporta delante de «{who.name}»: {why}");
        Say(why);
        Behaviour[] frozen = Pause(who);   // se detiene a escucharla
        yield return new WaitForSeconds(talk);

        // 2) LOS DOS, de vuelta dentro del área.
        if (who != null)
        {
            WorkdayDirector.Teleport(who.transform, back);
            if (m != null) { WorkdayDirector.Teleport(m.transform, back + Flat(who.transform.right) * 1.2f); m.transform.rotation = Quaternion.LookRotation(Flat(who.transform.position - m.transform.position)); }
            Returned++;
            Debug.Log($"[Magnate] Devuelve a «{who.name}» a su sitio.");
        }
        Resume(frozen);
        yield return new WaitForSeconds(stay);

        // 3) Ella vuelve a lo suyo (o la aparición se desvanece).
        if (m != null)
        {
            if (clone) Destroy(m);
            else { WorkdayDirector.Teleport(m.transform, home); m.transform.rotation = homeRot; Resume(paused); }
            _magnates.Remove(m);
        }
        if (who != null) { _busy.Remove(who); _graceUntil[who] = Time.time + grace; }
    }

    // Mientras dura, ni ella ni él se mueven por su cuenta (su conducta y el control del jugador quedan en pausa).
    static Behaviour[] Pause(GameObject g)
    {
        var list = new List<Behaviour>();
        if (g == null) return list.ToArray();
        foreach (Behaviour b in new Behaviour[] { g.GetComponent<PlayerController>(), g.GetComponent<IdleBehavior>(), g.GetComponent<Magnate>() })
            if (b != null && b.enabled) { b.enabled = false; list.Add(b); }
        return list.ToArray();
    }

    static void Resume(Behaviour[] list) { foreach (Behaviour b in list) if (b != null) b.enabled = true; }

    void Say(string text) { _msg = "La Magnate: «" + text + "»"; _msgUntil = Time.time + talk + stay + 1f; }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v.normalized : Vector3.forward; }

    void OnGUI()
    {
        if (Time.time > _msgUntil || string.IsNullOrEmpty(_msg)) return;
        GUI.Box(new Rect(Screen.width * 0.5f - 300, Screen.height - 120, 600, 60), "");
        GUI.Label(new Rect(Screen.width * 0.5f - 290, Screen.height - 114, 580, 50), _msg);
    }
}
