using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UNA SALA DE PRÁCTICA de los PISOS DE YOGA (Berón 2026-11-04; docs/yoga-pisos.md). Se quitó el anillo de yoga (desde él no se
/// veía al maestro) y la sala pasó a ser DOS PISOS de la torre del Entrenamiento: el de HATHA (el piso de la Sala de Yoga) y el de
/// ASHTANGA (su planta propia, «Planta_ashtanga»), cada uno con dos ZONAS, la OCCIDENTAL (suelo de madera, esterillas, accesorios,
/// sin espejos) y la TRADICIONAL (suelo de tierra, alfombras de algodón, un kund para sumergirse antes de practicar, lámparas de
/// aceite y su elemento distintivo: un linga como el Dhyanalinga en Hatha, el busto de Adiyogi y Patanjali en Ashtanga).
/// Cada zona es una YogaHall: su tarima del maestro en el centro, sus esterillas («Esterilla_N», alfombras-esterilla) en UN solo
/// anillo alrededor (nadie se pone entre otro alumno y el maestro) y, en la tradicional, su kund. La clase en grupo
/// (<see cref="YogaClass"/>) se da en una sala; puede haber una clase en cada sala a la vez.
/// </summary>
public class YogaHall : MonoBehaviour
{
    public enum Style { Hatha, Ashtanga }
    public enum Version { Occidental, Tradicional }

    public Style style;
    public Version version;
    [Tooltip("La tarima del maestro (en el centro de la sala).")]
    public Transform tarima;
    [Tooltip("El kund (agua del tronco, sujeta por el campo magnético): solo en la versión tradicional.")]
    public Transform kund;
    [Tooltip("Temperatura de la sala (°C): un valor de diseño (Ashtanga, más cálida).")]
    public float roomTempC = 22f;

    /// <summary>Altura de los ojos de un alumno SENTADO en su esterilla (m sobre ella) y del maestro DE PIE en la tarima.</summary>
    public const float SeatedEye = 0.8f, StandingEye = 1.55f;
    /// <summary>Lo más lejos que puede quedar una esterilla del maestro para verle bien los gestos (m).</summary>
    public const float MaxSight = 9f;
    /// <summary>Cuánto dura la inmersión en el kund (s de juego).</summary>
    public const float ImmersionSeconds = 8f;

    public string Label => $"{style} {(version == Version.Occidental ? "occidental" : "tradicional")}";
    public bool Traditional => version == Version.Tradicional;
    /// <summary>¿Se entra al kund antes de practicar? (solo en la tradicional, y si tiene kund)</summary>
    public bool Immersion => Traditional && kund != null;

    // ── El registro ──────────────────────────────────────────────────────────────────────────────

    static readonly List<YogaHall> _all = new List<YogaHall>();
    void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
    void OnDisable() { _all.Remove(this); }

    public static IReadOnlyList<YogaHall> All
    {
        get
        {
            _all.RemoveAll(h => h == null);
            if (_all.Count == 0) foreach (YogaHall h in FindObjectsByType<YogaHall>(FindObjectsSortMode.None)) _all.Add(h);
            return _all;
        }
    }

    public static YogaHall Find(Style s, Version v)
    {
        foreach (YogaHall h in All) if (h != null && h.style == s && h.version == v) return h;
        return null;
    }

    /// <summary>El estilo de una sesión de <see cref="YogaLibrary"/>: la Ashtanga Primaria, en el piso de Ashtanga; el resto (Saludo al
    /// Sol, Yoga Namaskar, upa-yoga…), en el de Hatha.</summary>
    public static Style StyleOf(string session) => session != null && session.StartsWith("Ashtanga") ? Style.Ashtanga : Style.Hatha;

    /// <summary>La versión del día: la tradicional los días pares y la occidental los impares (las dos zonas se usan cada semana).</summary>
    public static Version VersionOn(int day) => (day % 2 + 2) % 2 == 0 ? Version.Tradicional : Version.Occidental;

    public static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    /// <summary>La sala donde se da hoy la sesión <paramref name="session"/> (su estilo y la versión del día; si falta, la otra versión).</summary>
    public static YogaHall For(string session, int day)
    {
        Style s = StyleOf(session); Version v = VersionOn(day);
        YogaHall h = Find(s, v) ?? Find(s, v == Version.Occidental ? Version.Tradicional : Version.Occidental);
        if (h != null) return h;
        foreach (YogaHall x in All) if (x != null) return x;
        return null;
    }

    public static YogaHall Of(Transform t) => t != null ? t.GetComponentInParent<YogaHall>() : null;

    // ── Lo que tiene ─────────────────────────────────────────────────────────────────────────────

    /// <summary>Sus esterillas (alfombras-esterilla «Esterilla_N»), en orden.</summary>
    public List<CarpetPad> Mats()
    {
        var l = new List<CarpetPad>();
        foreach (CarpetPad p in GetComponentsInChildren<CarpetPad>(true))
            if (p != null && p.name.StartsWith("Esterilla_")) l.Add(p);
        l.Sort((x, y) => Index(x).CompareTo(Index(y)));
        return l;
    }
    static int Index(CarpetPad p) => int.TryParse(p.name.Substring("Esterilla_".Length), out int n) ? n : 999;

    /// <summary>Lo alto de la tarima (mundo).</summary>
    public float TarimaTop => tarima != null ? tarima.position.y + tarima.lossyScale.y * 0.5f : transform.position.y;
    public Vector3 TeacherEye => (tarima != null ? new Vector3(tarima.position.x, TarimaTop, tarima.position.z) : transform.position) + Vector3.up * StandingEye;
    public static Vector3 MatEye(CarpetPad m) => m.transform.position + Vector3.up * SeatedEye;

    /// <summary>Un sitio DENTRO del agua del kund para el alumno <paramref name="i"/> (rejilla de 4 × 2 a 1,2 m).</summary>
    public Vector3 KundSpot(int i)
    {
        if (kund == null) return transform.position;
        i = Mathf.Abs(i) % 8;
        return kund.TransformPoint(new Vector3(-1.8f + (i % 4) * 1.2f, 0f, -0.6f + (i / 4) * 1.2f));
    }

    /// <summary>¿Está <paramref name="p"/> (mundo) dentro del agua del kund (en planta y a su altura)?</summary>
    public bool InKund(Vector3 p)
    {
        if (kund == null) return false;
        Vector3 l = kund.InverseTransformPoint(p);
        return Mathf.Abs(l.x) <= 3.1f && Mathf.Abs(l.z) <= 1.6f && l.y > -0.6f && l.y < 2.5f;
    }

    // ── Líneas de visión ─────────────────────────────────────────────────────────────────────────

    /// <summary>LÍNEAS DE VISIÓN AL MAESTRO (Berón 2026-11-04: «desde el anillo no pueden ver ni interactuar con el profesor»): de
    /// los ojos de cada alumno sentado en su esterilla a los del maestro de pie en la tarima, ningún mueble, estatua ni pieza del
    /// piso en medio, y a ≤ <see cref="MaxSight"/> m. Devuelve cuántas esterillas no lo ven (0 = todas) y el detalle.</summary>
    public int BlockedSightLines(out string why)
    {
        why = "";
        List<CarpetPad> mats = Mats();
        if (tarima == null || mats.Count == 0) { why = $"{Label}: sin tarima o sin esterillas"; return Mathf.Max(1, mats.Count); }
        Transform deck = DeckOf(transform);
        var blockers = new List<(Renderer r, Bounds b)>();
        foreach (Renderer r in (deck != null ? deck : transform).GetComponentsInChildren<Renderer>())
            if (Blocks(r, deck)) blockers.Add((r, r.bounds));
        int bad = 0; Vector3 eye = TeacherEye;
        foreach (CarpetPad m in mats)
        {
            Vector3 from = MatEye(m), d = eye - from; float len = d.magnitude;
            string hit = null;
            if (len > MaxSight) hit = $"a {len:0.0} m (> {MaxSight:0} m)";
            else
            {
                var ray = new Ray(from, d / len);
                foreach (var (r, b) in blockers)
                    if (b.IntersectRay(ray, out float t) && t < len - 0.05f) { hit = $"tapa «{r.name}» ({r.transform.parent?.name})"; break; }
            }
            if (hit == null) continue;
            bad++;
            if (why.Length < 300) why += $"{m.name}: {hit}; ";
        }
        if (bad == 0) why = $"{Label}: {mats.Count} esterillas ven al maestro (≤ {MaxSight:0} m, nada en medio)";
        return bad;
    }

    // Qué puede tapar: lo dibujado del piso que se alza más de 0,35 m sobre él; no las esterillas (alfombras), la tarima, el
    // maestro ni los alumnos (seres vivos), los rótulos, los suelos ni lo de OTRO piso (las plantas propias cuelgan del área).
    bool Blocks(Renderer r, Transform deck)
    {
        if (!r.enabled || r.GetComponent<TMPro.TMP_Text>() != null) return false;
        Transform t = r.transform;
        if (r.GetComponentInParent<CarpetPad>() != null || r.GetComponentInParent<FlyingCarpet>() != null || r.GetComponentInParent<Anima>() != null) return false;
        if (tarima != null && (t == tarima || t.IsChildOf(tarima))) return false;
        if (t.name.StartsWith("Suelo") || t.name.StartsWith("Agua") || t.name == "Mat" || t.name.StartsWith("Losa")) return false;
        if (deck != null && DeckOf(t) != deck) return false;
        Bounds b = r.bounds;
        return b.max.y > transform.position.y + 0.35f;
    }

    /// <summary>El piso (TreeFloor, o el área) en que está <paramref name="t"/>.</summary>
    static Transform DeckOf(Transform t)
    {
        for (Transform x = t; x != null; x = x.parent) if (x.GetComponent<TreeFloor>() != null || x.GetComponent<SanctuaryArea>() != null) return x;
        return null;
    }
}
