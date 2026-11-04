using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL LEVIATÁN: HOSPITAL SUBMARINO COLOSAL PARA ANIMALES GIGANTES (Berón 2026-11-03: «quizás que sea un submarino gigante que se
/// encuentra parqueado a ras del suelo y directamente va hacia la ballena herida y la tratarían directamente en dicho submarino
/// colosal»; docs/submarinos-medicos.md, con la investigación y sus fuentes). Una ballena o una orca no se puede sacar del agua:
/// fuera de ella su propio peso le aplasta los pulmones y los órganos y se recalienta (Geraci y Lounsbury 2005; NOAA). Por eso
/// el Leviatán lleva el agua con ella: un POZO MÉDICO inundado de 20 × 8 m con SUELO ELEVABLE (como la piscina médica de suelo
/// móvil de los acuarios), abierto a popa por una compuerta.
/// - Atracado en superficie junto a la orilla, «a ras del suelo» (su cubierta al nivel del hielo, con su pasarela).
/// - LA MISIÓN (<see cref="Dispatch"/>): el submarino-ambulancia trae al equipo veterinario desde el hospital (<see cref="HospitalSubmarine.Ferry"/>),
///   se sumerge bajo las placas de hielo, va a la ballena, emerge a su lado, abre la compuerta y la guía al pozo a 1 m/s, cierra y
///   sube el suelo hasta que su lomo y su espiráculo quedan FUERA del agua y su cuerpo apoyado (respira por su cuenta: nada de
///   anestesia general), la trata (ecografía, muestra de sangre de la aleta, antibiótico con jeringa de pértiga, limpieza de la
///   herida, la piel mojada con los rociadores), baja el suelo, abre y la suelta en el mar, y vuelve a su atraque.
/// - Cabe una orca (≤ 9,8 m, ≤ 10 t), una beluga o una ballena gris o jorobada joven (≤ 16 m). Las mayores (rorcual, azul) se
///   tratan junto al casco con las eslingas de los pontones (como en los reflotamientos de varamientos).
/// Todo con tope de tiempo: si algo no llega, lo dice y sigue.
/// </summary>
public class WhaleHospitalSubmarine : MonoBehaviour
{
    /// <summary>(2026-11-04) 56 × 16 × 12 m con un pozo de 20 × 8 m: a la medida de su paciente de diseño (una ballena de 16 m; una orca de
    /// ~7 m, como las del santuario). Con el de 72 m y su pozo de 32 m, la orca se perdía dentro.</summary>
    public const float Length = 56f, Beam = 16f, Height = 12f;
    public static readonly Vector2 Well = new Vector2(8f, 20f);   // ancho × largo del pozo médico
    public const float WaterY = -0.25f;
    public float swimSpeed = 4f, turnSpeed = 12f, intakeSpeed = 1f, floorSpeed = 0.6f;
    public float treatGameMinutes = 90f;
    public Vector3 berthPos; public Quaternion berthRot = Quaternion.identity;

    public enum Phase { Atracado, Lanzadera, Inmersion, AlPaciente, Entrada, Elevar, Tratamiento, Bajar, Liberacion, Vuelta }
    public Phase phase = Phase.Atracado;
    public Anima patient;
    public readonly List<GameObject> crew = new List<GameObject>();
    public float sicknessBefore, sicknessAfter, blowholeAboveWater = float.NaN, supportGap = float.NaN, missionSeconds;
    public bool inWell, released;
    public readonly List<string> log = new List<string>();

    public static readonly List<WhaleHospitalSubmarine> All = new List<WhaleHospitalSubmarine>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); if (berthPos == Vector3.zero) { berthPos = transform.position; berthRot = transform.rotation; } }
    void OnDisable() { All.Remove(this); }

    Transform WellFloor => transform.Find("Pozo/Suelo_elevable");
    Transform Gate => transform.Find("Compuerta_popa");
    public Transform Gangway => transform.Find("Pasarela/Fin");
    Transform[] Seats() { var l = new List<Transform>(); for (int i = 1; i <= 4; i++) { Transform s = transform.Find($"Puente/Asiento_{i}"); if (s != null) l.Add(s); } return l.ToArray(); }
    float SurfacedY => berthPos.y;
    float DepthY => WaterY - 1.6f - Height * 0.5f;   // el lomo bajo las placas de hielo (1,5 m)

    void Say(string s) { log.Add(s); Debug.Log($"[Leviatán] {s}"); }

    public bool Dispatch(Anima whale)
    {
        if (phase != Phase.Atracado || whale == null) return false;
        StartCoroutine(Mission(whale));
        return true;
    }

    IEnumerator MoveTo(Vector3 to, float speed, bool turn, float cap)
    {
        float t0 = Time.time;
        while ((transform.position - to).sqrMagnitude > 0.04f)
        {
            Vector3 flat = to - transform.position; flat.y = 0f;
            if (turn && flat.sqrMagnitude > 4f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat.normalized), turnSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, to, speed * Time.deltaTime);
            if (Time.time - t0 > cap) { Say($"no llega a ({to.x:0}, {to.y:0}, {to.z:0}) en {cap:0} s: sigue desde aquí"); yield break; }
            yield return null;
        }
    }

    IEnumerator TurnTo(Vector3 dir)
    {
        dir.y = 0f; if (dir.sqrMagnitude < 1e-4f) yield break;
        Quaternion q = Quaternion.LookRotation(dir.normalized); float t0 = Time.time;
        while (Quaternion.Angle(transform.rotation, q) > 0.5f && Time.time - t0 < 40f) { transform.rotation = Quaternion.RotateTowards(transform.rotation, q, turnSpeed * Time.deltaTime); yield return null; }
        transform.rotation = q;
    }

    IEnumerator Mission(Anima w)
    {
        patient = w; inWell = false; released = false; log.Clear(); crew.Clear(); blowholeAboveWater = float.NaN; supportGap = float.NaN;
        float t0 = Time.time;
        sicknessBefore = w.sickness;
        Say($"aviso: «{w.name}» ({w.SpeciesName}) herida a {Vector3.Distance(transform.position, w.transform.position):0} m; enfermedad {w.sickness:0.00}");

        // 1. LA LANZADERA: el submarino-ambulancia trae al equipo veterinario del hospital.
        phase = Phase.Lanzadera;
        HospitalSubmarine amb = HospitalSubmarine.All.Find(s => s != null && s.phase == HospitalSubmarine.Phase.Aparcado);
        if (amb != null && Gangway != null)
        {
            var handed = new List<GameObject>();
            amb.StartCoroutine(amb.Ferry(Gangway.position, Seats(), handed));
            float f0 = Time.time;
            while (handed.Count == 0 && Time.time - f0 < 600f && (amb.phase != HospitalSubmarine.Phase.Aparcado || Time.time - f0 < 2f)) yield return null;
            float f1 = Time.time; while (amb.phase == HospitalSubmarine.Phase.AlPaciente && Time.time - f1 < 60f) yield return null;   // (todos a bordo)
            crew.AddRange(handed);
            Say($"equipo a bordo por la lanzadera: {string.Join(", ", crew.ConvertAll(g => g.name))} ({Time.time - t0:0} s)");
        }
        else Say("sin submarino-ambulancia libre: sale sin equipo del hospital");

        // 2. INMERSIÓN Y VIAJE bajo el hielo.
        phase = Phase.Inmersion;
        yield return MoveTo(new Vector3(transform.position.x, DepthY, transform.position.z), 1.5f, false, 30f);
        phase = Phase.AlPaciente;
        Vector3 wp = w.transform.position;
        Vector3 from = transform.position - wp; from.y = 0f; from = from.sqrMagnitude > 1f ? from.normalized : Vector3.forward;
        Vector3 stop = wp + from * (Length * 0.5f + 8f); stop.y = DepthY;
        yield return MoveTo(stop, swimSpeed, true, Vector3.Distance(transform.position, stop) / swimSpeed * 3f + 60f);
        yield return TurnTo(from);   // la popa (la compuerta) hacia la ballena
        bool open = !IceOver(transform.position, Beam * 0.5f) && !IceOver(wp, 6f);
        if (open) yield return MoveTo(new Vector3(transform.position.x, SurfacedY, transform.position.z), 1.5f, false, 30f);
        Say(open ? "emerge junto a la ballena" : "bajo el hielo: la recibe sumergido (el pozo va inundado)");

        // 3. ENTRADA: compuerta abajo; la guían al pozo (el suelo, bajado).
        phase = Phase.Entrada;
        Transform floor = WellFloor;
        Vector3 floorLow = floor != null ? floor.localPosition : Vector3.zero;
        if (Gate != null) Gate.localRotation = Quaternion.Euler(-80f, 0f, 0f);
        Carried c = Carried.Take(w.gameObject, floor != null ? floor : transform);
        Bounds wb = BodyBounds(w.transform);
        float half = wb.extents.y;
        Vector3 sternMouth = transform.TransformPoint(new Vector3(0f, 0f, -Length * 0.5f - 2f)); sternMouth.y = Mathf.Min(w.transform.position.y, WaterY - half - 0.5f);
        Vector3 wellCenter = (floor != null ? floor.position : transform.position) + Vector3.up * (half + 0.3f);
        float e0 = Time.time;
        while (!Carried.MoveTowards(w.transform, sternMouth, intakeSpeed * 2f) && Time.time - e0 < 90f) yield return null;
        while (!Carried.MoveTowards(w.transform, wellCenter, intakeSpeed) && Time.time - e0 < 180f) yield return null;
        w.transform.rotation = Quaternion.LookRotation(transform.forward);
        // EL VIENTRE SOBRE LOS COLCHONES (2026-11-04: quedaba a 1,26 m: el pivote de la orca no está en su centro): se la baja hasta que lo
        // más bajo de su cuerpo quede a 0,15 m del suelo elevable.
        if (floor != null) { Bounds bb = BodyBounds(w.transform); w.transform.position += Vector3.up * (floor.position.y + 0.15f - bb.min.y); }
        inWell = true;
        if (Gate != null) Gate.localRotation = Quaternion.identity;
        Say($"«{w.name}» en el pozo médico ({Time.time - e0:0} s a {intakeSpeed:0.#} m/s), compuerta cerrada");

        // 4. ELEVAR el suelo hasta que el lomo y el espiráculo salgan del agua (apoyada, respira sola).
        phase = Phase.Elevar;
        if (floor != null)
        {
            float u0 = Time.time;
            while (BodyBounds(w.transform).max.y < WaterY + 0.35f && Time.time - u0 < 60f)
            {
                floor.localPosition += Vector3.up * floorSpeed * Time.deltaTime;
                yield return null;
            }
        }
        Bounds lifted = BodyBounds(w.transform);
        blowholeAboveWater = lifted.max.y - WaterY;
        supportGap = floor != null ? lifted.min.y - (floor.position.y) : float.NaN;
        Say($"suelo arriba: el lomo a {blowholeAboveWater:0.00} m sobre el agua, el vientre apoyado ({supportGap:0.00} m sobre el suelo)");

        // 5. TRATAMIENTO a bordo.
        phase = Phase.Tratamiento;
        Transform spray = transform.Find("Pozo/Rociadores");
        if (spray != null) spray.gameObject.SetActive(true);
        float secs = treatGameMinutes / 60f * (TimeController.timeController != null ? TimeController.timeController.SecondsPerGameDay / 24f : 150f);
        string[] steps = { "ecografía (pulmones y abdomen) desde la pasarela", "muestra de sangre de las venas de la aleta caudal", "antibiótico intramuscular con jeringa de pértiga",
                           "limpieza y sutura de la herida", "piel mojada y fresca con los rociadores; temperatura y respiración vigiladas (sin anestesia general)" };
        float tr0 = Time.time; int si = 0;
        BloodLoss bl = w.GetComponent<BloodLoss>();
        while (Time.time - tr0 < secs)
        {
            float k = Mathf.Clamp01((Time.time - tr0) / secs);
            if (si < steps.Length && k >= si / (float)steps.Length) Say($"{(crew.Count > 0 ? crew[si % crew.Count].name : "el equipo")}: {steps[si++]}");
            w.sickness = Mathf.Max(0.05f, w.sickness - 0.6f * Time.deltaTime / secs);
            w.stress = Mathf.Max(0f, w.stress - 0.5f * Time.deltaTime / secs);
            if (bl != null) bl.lost = Mathf.Max(0f, bl.lost - 0.3f * Time.deltaTime / secs);
            yield return null;
        }
        if (spray != null) spray.gameObject.SetActive(false);
        sicknessAfter = w.sickness;
        Say($"tratada: enfermedad {sicknessBefore:0.00} → {sicknessAfter:0.00}");

        // 6. BAJAR el suelo y LIBERARLA.
        phase = Phase.Bajar;
        if (floor != null)
        {
            float d0 = Time.time;
            while (floor.localPosition.y > floorLow.y + 0.01f && Time.time - d0 < 60f) { floor.localPosition = Vector3.MoveTowards(floor.localPosition, floorLow, floorSpeed * Time.deltaTime); yield return null; }
        }
        phase = Phase.Liberacion;
        if (Gate != null) Gate.localRotation = Quaternion.Euler(-80f, 0f, 0f);
        Vector3 outSea = transform.TransformPoint(new Vector3(0f, 0f, -Length * 0.5f - 18f)); outSea.y = -1f;
        float r0 = Time.time;
        Vector3 mouth = transform.TransformPoint(new Vector3(0f, 0f, -Length * 0.5f - 2f)); mouth.y = w.transform.position.y;
        while (!Carried.MoveTowards(w.transform, mouth, intakeSpeed * 1.5f) && Time.time - r0 < 120f) yield return null;
        while (!Carried.MoveTowards(w.transform, outSea, intakeSpeed * 2f) && Time.time - r0 < 180f) yield return null;
        c.Release(w.transform.position, Quaternion.LookRotation(-transform.forward));
        inWell = false; released = true;
        if (Gate != null) Gate.localRotation = Quaternion.identity;
        Say($"«{w.name}» liberada en el mar, sana");

        // 7. VUELTA al atraque; el equipo baja por la pasarela y vuelve a su jornada.
        phase = Phase.Vuelta;
        yield return MoveTo(new Vector3(transform.position.x, DepthY, transform.position.z), 1.5f, false, 30f);
        Vector3 bd = new Vector3(berthPos.x, DepthY, berthPos.z);
        yield return MoveTo(bd, swimSpeed, true, Vector3.Distance(transform.position, bd) / swimSpeed * 3f + 60f);
        yield return TurnTo(berthRot * Vector3.forward);
        yield return MoveTo(berthPos, 1.5f, false, 30f);
        transform.rotation = berthRot;
        foreach (GameObject g in crew)
        {
            if (g == null) continue;
            Carried cc = g.GetComponent<Carried>();
            Vector3 land = Gangway != null ? Gangway.position : transform.position;
            float g0 = Time.time;
            while (cc != null && !Carried.MoveTowards(g.transform, land, 2f) && Time.time - g0 < 40f) yield return null;
            if (cc != null) cc.Release(land, Quaternion.LookRotation(Flat(land - transform.position)));
            HospitalSubmarine.Free(g);
        }
        missionSeconds = Time.time - t0;
        phase = Phase.Atracado;
        Say($"de vuelta en su atraque ({missionSeconds:0} s de juego); el equipo baja por la pasarela");
    }

    static Vector3 Flat(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 1e-4f ? v.normalized : Vector3.forward; }

    public static Bounds BodyBounds(Transform t)
    {
        Bounds b = new Bounds(t.position, Vector3.zero); bool any = false;
        foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer || r.GetComponent<TMPro.TMP_Text>() != null) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
        }
        return b;
    }

    static readonly RaycastHit[] _hits = new RaycastHit[12];
    /// <summary>¿Hay hielo encima del mar en un radio de <paramref name="r"/> m alrededor de <paramref name="p"/>?</summary>
    public bool IceOver(Vector3 p, float r)
    {
        for (int i = 0; i < 9; i++)
        {
            Vector3 q = p + (i == 0 ? Vector3.zero : Quaternion.Euler(0f, i * 45f, 0f) * Vector3.forward * r);
            int n = Physics.RaycastNonAlloc(new Vector3(q.x, 8f, q.z), Vector3.down, _hits, 14f, ~0, QueryTriggerInteraction.Ignore);
            for (int k = 0; k < n; k++)
                if (_hits[k].point.y > -0.3f && !_hits[k].collider.transform.IsChildOf(transform) && _hits[k].collider.GetComponentInParent<Anima>() == null) return true;
        }
        return false;
    }
}
