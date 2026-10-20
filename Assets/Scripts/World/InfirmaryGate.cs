using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA INICIACIÓN ES BLOQUEANTE (docs/jornada-y-rotacion.md). Quien acaba de llegar (Kushal, un turista) no está registrado
/// en el santuario: no puede salir de la ENFERMERÍA hasta hacerse los análisis, vacunarse y completar la iniciación con
/// quien tenga hoy la misión de iniciación (NewcomerInitiator). Dentro se mueve libre (y el iniciador lo acompaña).
/// GUARDIAS DE INICIACIÓN: los humanos que tienen ahora en su jornada «Vigilar a los recién llegados», uno por recién
/// llegado (si faltan, los cubre un clon de la Magnate). Esperan en la salida y la bloquean; si aun así alguien
/// sale (saltando, por ejemplo), SU guardia corre hacia él, le lanza «jalar» y lo trae de vuelta a la enfermería.
/// </summary>
public class InfirmaryGate : MonoBehaviour
{
    [Tooltip("Salida de la enfermería en coordenadas locales del área (lo alto de la escalera).")]
    public Vector3 exitLocal = new Vector3(0f, 1f, 9.4f);
    public float radius = 2.2f;
    public float chaseSpeed = 9f, dragSpeed = 5f;
    public int blockedCount { get; private set; }
    public int recovered { get; private set; }
    float _msgUntil; string _msg;

    class Warden { public GameObject guard; public Anima newcomer; public int state; public bool clone; }   // 0 vigila · 1 persigue · 2 lo trae
    readonly Dictionary<Anima, Warden> _wardens = new Dictionary<Anima, Warden>();
    Bounds _inside; bool _hasBounds;

    /// <summary>¿Puede salir? Cuando ya está analizado, vacunado y certificado (o no es un recién llegado).</summary>
    public static bool Cleared(GameObject who)
    {
        if (who == null) return true;
        HealthRecord hr = who.GetComponent<HealthRecord>();
        SanctuaryOnboarding ob = who.GetComponent<SanctuaryOnboarding>();
        bool health = hr == null || !hr.NeedsCare;
        bool initiated = ob == null || ob.apt;
        return health && initiated;
    }

    /// <summary>Guardias asignados ahora mismo (uno por recién llegado).</summary>
    public int Wardens => _wardens.Count;

    void Bounds()
    {
        if (_hasBounds) return;
        Transform mat = transform.Find("Mat");
        if (mat != null && mat.GetComponent<Renderer>() != null) { _inside = mat.GetComponent<Renderer>().bounds; _inside.Expand(new Vector3(1f, 8f, 1f)); _hasBounds = true; }
    }

    bool Inside(Vector3 p) { Bounds(); return !_hasBounds || _inside.Contains(p); }

    void Update()
    {
        if (ColdSanctuary.TestRunner.Running) { Clear(); return; }
        // Recién llegados: cualquier Anima con iniciación pendiente.
        var current = new HashSet<Anima>();
        foreach (SanctuaryOnboarding o in SceneCache.All<SanctuaryOnboarding>(0.5f))
        {
            Anima a = o != null ? o.GetComponent<Anima>() : null;
            if (a == null || Cleared(a.gameObject)) continue;
            current.Add(a);
            if (!_wardens.ContainsKey(a)) Assign(a);
        }
        foreach (var kv in new List<KeyValuePair<Anima, Warden>>(_wardens))
            if (kv.Key == null || !current.Contains(kv.Key)) { Release(kv.Value); _wardens.Remove(kv.Key); }

        // GUARDIAS DE INICIACIÓN = quienes tienen AHORA en su jornada la misión «Vigilar a los recién llegados»; si son
        // menos que los recién llegados, los que faltan los cubre un clon de la Magnate. Al cambiar el turno, se relevan.
        List<GameObject> guards = WorkdayDirector.Instance != null ? WorkdayDirector.Instance.InitiationGuardsNow() : new List<GameObject>();
        int gi = 0;
        foreach (Warden w in _wardens.Values)
        {
            GameObject human = gi < guards.Count ? guards[gi++] : null;
            if (human != null && w.guard != human) { Release(w); w.guard = human; w.clone = false; Debug.Log($"[Iniciación] «{human.name}» (guardia de iniciación de su jornada) vigila a «{w.newcomer.name}»."); }
            else if (human == null && (w.guard == null || !w.clone)) { Release(w); w.guard = SpawnClone(w.newcomer); w.clone = true; }
        }
        _busy.Clear(); foreach (Warden w in _wardens.Values) if (w.guard != null && !w.clone) _busy.Add(w.guard);

        Vector3 exit = transform.TransformPoint(exitLocal);
        int i = 0;
        foreach (Warden w in _wardens.Values)
        {
            if (w.guard == null) continue;
            Vector3 post = exit + transform.right * ((i++ - (_wardens.Count - 1) * 0.5f) * 1.2f);
            Tick(w, exit, post);
        }
    }

    static readonly HashSet<GameObject> _busy = new HashSet<GameObject>();
    /// <summary>¿Este personaje está ahora de guardia de iniciación? (la jornada no lo mueve: lo mueve su guardia).</summary>
    public static bool IsWarden(GameObject g) => g != null && _busy.Contains(g);

    void Release(Warden w) { if (w.clone && w.guard != null) Destroy(w.guard); w.guard = null; }

    void Assign(Anima newcomer) => _wardens[newcomer] = new Warden { newcomer = newcomer };

    GameObject SpawnClone(Anima newcomer)
    {
        GameObject g = WorkdayDirector.SpawnMagnateBody($"Magnate (clon guardiana de {newcomer.name})");
        g.transform.position = transform.TransformPoint(exitLocal);
        g.transform.rotation = Quaternion.LookRotation(-transform.forward);
        Debug.Log($"[Iniciación] Faltan guardias de iniciación: un clon de la Magnate queda asignado a «{newcomer.name}».");
        return g;
    }

    void Tick(Warden w, Vector3 exit, Vector3 post)
    {
        Transform g = w.guard.transform, n = w.newcomer.transform;
        switch (w.state)
        {
            case 0:   // VIGILA la salida
                Move(g, Vector3.MoveTowards(g.position, post, chaseSpeed * Time.deltaTime));
                Vector3 d = n.position - exit; d.y = 0f;
                if (d.magnitude < radius) { Move(n, transform.TransformPoint(exitLocal + new Vector3(0f, 0.3f, -3.5f))); blockedCount++; Say(w, "Aún no estás registrado en el santuario. Termina tu iniciación: análisis, vacuna y tu primera virtualización."); }
                else if (!Inside(n.position)) { w.state = 1; Debug.Log($"[Iniciación] ¡«{n.name}» ha salido de la enfermería! Su guardia de iniciación corre a por él."); }
                break;
            case 1:   // PERSIGUE
                Vector3 to = n.position + (g.position - n.position).normalized * 0.8f;
                Move(g, Vector3.MoveTowards(g.position, to, chaseSpeed * Time.deltaTime));
                g.rotation = Quaternion.LookRotation(Flat(n.position - g.position));
                if (Vector3.Distance(g.position, n.position) < 1.6f) { w.state = 2; Debug.Log($"[Iniciación] Su guardia alcanza a «{n.name}» y le lanza «jalar»: vuelve a la enfermería."); }
                if (Inside(n.position)) w.state = 0;
                break;
            case 2:   // LO TRAE (jalar): los dos vuelven a la enfermería
                Vector3 back = transform.TransformPoint(exitLocal + new Vector3(0f, 0.3f, -4f));
                Move(g, Vector3.MoveTowards(g.position, back + transform.right * 0.8f, dragSpeed * Time.deltaTime));
                Move(n, Vector3.MoveTowards(n.position, back, dragSpeed * Time.deltaTime));
                if (Vector3.Distance(n.position, back) < 0.3f) { w.state = 0; recovered++; Say(w, "Te he traído de vuelta: hasta terminar la iniciación no puedes salir de la enfermería."); }
                break;
        }
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v : Vector3.forward; }

    static void Move(Transform t, Vector3 p) => WorkdayDirector.Teleport(t, p);

    void Say(Warden w, string text)
    {
        if (w.newcomer.GetComponent<PlayerController>() == null) return;
        _msg = (w.clone ? "La Magnate" : w.guard.name) + ": «" + text + "»"; _msgUntil = Time.time + 4f;
        Debug.Log("[Iniciación] " + _msg);
    }

    void Clear() { foreach (Warden w in _wardens.Values) Release(w); _wardens.Clear(); _busy.Clear(); }

    void OnGUI()
    {
        if (Time.time > _msgUntil || string.IsNullOrEmpty(_msg)) return;
        GUI.Box(new Rect(Screen.width * 0.5f - 300, Screen.height - 120, 600, 60), "");
        GUI.Label(new Rect(Screen.width * 0.5f - 290, Screen.height - 114, 580, 50), _msg);
    }
}
