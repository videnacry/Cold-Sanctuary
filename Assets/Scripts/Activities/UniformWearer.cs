using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// QUIEN SE PONE EL UNIFORME (docs/actividades-y-uniformes.md §4; <see cref="Uniforms"/>). La conducta que lo lleva (la jornada, la
/// jornada del microcosmos) dice qué uniforme QUIERE (<see cref="Want"/>); este componente se lo pone y se lo quita. Si está NADANDO
/// (<see cref="SwimBody.Swimming"/>), manda el neopreno; al salir del agua vuelve al que tocaba. Humano: recolorea los cubos de la ropa
/// (torso, brazos, cadera, piernas, pies y, con guantes, las manos) y los relieves de músculo que van con ellos, oculta los detalles de su
/// ropa de diario (collares, chalecos, cadenas) y añade las piezas «Uniforme_…»; el pelo, la cara y la piel no se tocan. Al quitárselo
/// todo vuelve a su material y a su sitio. Insecto-humano: el delantal de su oficio (<see cref="Uniforms.DressInsect"/>).
/// </summary>
[DisallowMultipleComponent]
public class UniformWearer : MonoBehaviour
{
    public Uniforms.Kind Current { get; private set; } = Uniforms.Kind.None;
    public Uniforms.Kind Wanted { get; private set; } = Uniforms.Kind.None;
    /// <summary>Cambios de ropa desde el arranque (pruebas, registro).</summary>
    public static int Changes { get; private set; }

    readonly List<(Renderer r, Material m)> _saved = new List<(Renderer, Material)>();
    readonly List<GameObject> _hidden = new List<GameObject>();
    readonly List<GameObject> _added = new List<GameObject>();
    float _next;

    /// <summary>Que <paramref name="g"/> lleve <paramref name="k"/> (None = su ropa). Se aplica ya (y luego, cada ¼ s, se vigila el agua).</summary>
    public static UniformWearer Want(GameObject g, Uniforms.Kind k)
    {
        if (g == null) return null;
        if (k == Uniforms.Kind.None && !g.TryGetComponent(out UniformWearer _)) return null;   // sin uniforme y sin haberlo llevado: nada
        UniformWearer w = g.GetOrAdd<UniformWearer>();
        w.Wanted = k;
        w.Refresh();
        return w;
    }

    /// <summary>¿Qué lleva <paramref name="g"/> ahora?</summary>
    public static Uniforms.Kind Of(GameObject g) => g != null && g.TryGetComponent(out UniformWearer w) ? w.Current : Uniforms.Kind.None;

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + 0.25f;
        Refresh();
    }

    void Refresh()
    {
        Uniforms.Kind target = SwimBody.Swimming(gameObject) ? Uniforms.Kind.Neopreno : Wanted;
        if (target != Current) Wear(target);
    }

    /// <summary>Se cambia YA a <paramref name="k"/> (quitándose lo que llevara).</summary>
    public void Wear(Uniforms.Kind k)
    {
        if (k == Current) return;
        Uniforms.Kind was = Current;
        TakeOff();
        if (k != Uniforms.Kind.None)
        {
            if (GetComponent<CubeInsect>() != null) Uniforms.DressInsect(gameObject, k, Eras.Now);
            else PutOn(Uniforms.SpecOf(k));
        }
        Current = k; Changes++;
        Debug.Log(k == Uniforms.Kind.None ? $"[Uniforme] «{name}» se quita {Uniforms.Title(was)} y vuelve a su ropa."
                                          : $"[Uniforme] «{name}» se pone {Uniforms.Title(k)}.");
    }

    void OnDestroy() { if (Current != Uniforms.Kind.None) TakeOff(); }

    // ── Quitar ──────────────────────────────────────────────────────────────────────────────────────────────────
    void TakeOff()
    {
        if (GetComponent<CubeInsect>() != null) CubeInsect.RemoveWorkUniform(gameObject);
        foreach (var s in _saved) if (s.r != null) s.r.sharedMaterial = s.m;
        _saved.Clear();
        foreach (GameObject h in _hidden) if (h != null) h.SetActive(true);
        _hidden.Clear();
        foreach (GameObject a in _added) if (a != null) { a.SetActive(false); Destroy(a); }
        _added.Clear();
    }

    // ── Poner (cuerpo de cubos humano) ──────────────────────────────────────────────────────────────────────────
    static readonly HashSet<BodyPart> Keep = new HashSet<BodyPart> { BodyPart.Head, BodyPart.Neck };

    void PutOn(Uniforms.Spec s)
    {
        Transform body = transform.Find(CubeHumanoid.BodyName);
        if (body == null) return;
        Color skin = CubeHair.For(name.Replace("_Post", "")).skin;
        var joints = new Dictionary<BodyPart, CubeJoint>();
        foreach (CubeJoint j in body.GetComponentsInChildren<CubeJoint>(true)) joints[j.part] = j;
        foreach (var kv in joints)
        {
            BodyPart p = kv.Key; CubeJoint j = kv.Value;
            if (Keep.Contains(p) || j.segment == null) continue;
            Color? c = ColorFor(p, s, skin);
            Renderer seg = j.segment.GetComponent<Renderer>();
            if (c.HasValue && seg != null)
            {
                Material old = seg.sharedMaterial, mat = KitchenProps.Mat(c.Value);
                // El cubo y los relieves de músculo/grasa que llevan su mismo material (BodyShape los pega al hueso).
                foreach (Transform ch in j.transform)
                {
                    if (ch.GetComponent<CubeJoint>() != null) continue;
                    Renderer r = ch.GetComponent<Renderer>();
                    if (r != null && (ch == j.segment || r.sharedMaterial == old)) { _saved.Add((r, r.sharedMaterial)); r.sharedMaterial = mat; }
                }
            }
            // La ropa de diario que asoma (collares, chalecos, cadenas, tatuajes bajo la manga): fuera mientras lleva el uniforme.
            if (p == BodyPart.HandLeft || p == BodyPart.HandRight) { if (!s.gloves.HasValue) continue; }
            foreach (Transform ch in j.transform)
                if ((ch.name == "Detalle" || ch.name == "Tatuaje") && ch.gameObject.activeSelf) { ch.gameObject.SetActive(false); _hidden.Add(ch.gameObject); }
        }
        Accessories(joints, s);
    }

    static Color? ColorFor(BodyPart p, Uniforms.Spec s, Color skin)
    {
        switch (p)
        {
            case BodyPart.Chest: case BodyPart.Back: case BodyPart.ScapulaLeft: case BodyPart.ScapulaRight: return s.top;
            case BodyPart.ShoulderLeft: case BodyPart.ShoulderRight: return s.sleeves;
            case BodyPart.ForearmLeft: case BodyPart.ForearmRight: return s.forearms ?? (s.shortSleeves ? skin : s.sleeves);
            case BodyPart.HandLeft: case BodyPart.HandRight: return s.gloves;
            case BodyPart.Hips: case BodyPart.ThighLeft: case BodyPart.ThighRight: return s.pants;
            case BodyPart.ShinLeft: case BodyPart.ShinRight: return s.shorts ? skin : s.pants;
            case BodyPart.FootLeft: case BodyPart.FootRight: return s.shoes;
        }
        return null;
    }

    Transform Piece(Transform parent, string what, Vector3 pos, Vector3 size, Color c)
    {
        Transform t = KitchenProps.C(parent, pos, size, c, 0f, CubeInsect.UniformPrefix + what);
        _added.Add(t.gameObject);
        return t;
    }

    void Accessories(Dictionary<BodyPart, CubeJoint> j, Uniforms.Spec s)
    {
        Transform Bone(BodyPart p) => j.TryGetValue(p, out CubeJoint cj) ? cj.transform : null;
        Transform chest = Bone(BodyPart.Chest), back = Bone(BodyPart.Back), hips = Bone(BodyPart.Hips), head = Bone(BodyPart.Head);
        Color silver = new Color(0.85f, 0.87f, 0.88f), brown = new Color(0.45f, 0.3f, 0.18f);
        if (chest != null)
        {
            if (s.apron) Piece(chest, "peto", new Vector3(0f, 0.12f, 0.124f), new Vector3(0.22f, 0.22f, 0.008f), s.apronColor);
            if (s.hiVis) { Piece(chest, "banda", new Vector3(0f, 0.08f, 0f), new Vector3(0.412f, 0.025f, 0.248f), silver); Piece(chest, "banda", new Vector3(0f, 0.2f, 0f), new Vector3(0.412f, 0.025f, 0.248f), silver); }
            if (s.stethoscope)
            {
                Piece(chest, "fonendo", new Vector3(-0.05f, 0.22f, 0.126f), new Vector3(0.012f, 0.12f, 0.01f), new Color(0.15f, 0.15f, 0.17f));
                Piece(chest, "fonendo", new Vector3(0.05f, 0.22f, 0.126f), new Vector3(0.012f, 0.12f, 0.01f), new Color(0.15f, 0.15f, 0.17f));
                Piece(chest, "fonendo", new Vector3(0.05f, 0.15f, 0.13f), new Vector3(0.035f, 0.035f, 0.012f), silver);
            }
            if (s.badge) Piece(chest, "acreditacion", new Vector3(0.1f, 0.22f, 0.123f), new Vector3(0.06f, 0.03f, 0.005f), new Color(0.95f, 0.95f, 0.95f));
            if (s.collar) { Piece(chest, "cuello", new Vector3(0f, 0.29f, 0.09f), new Vector3(0.2f, 0.04f, 0.06f), s.top); Piece(chest, "logo", new Vector3(-0.09f, 0.22f, 0.123f), new Vector3(0.04f, 0.04f, 0.005f), new Color(0.95f, 0.75f, 0.2f)); }
            if (s.tape) { Piece(chest, "cinta_metrica", new Vector3(-0.06f, 0.18f, 0.126f), new Vector3(0.02f, 0.2f, 0.006f), new Color(0.95f, 0.85f, 0.25f)); Piece(chest, "cinta_metrica", new Vector3(0.06f, 0.18f, 0.126f), new Vector3(0.02f, 0.2f, 0.006f), new Color(0.95f, 0.85f, 0.25f)); }
            if (s.dosimeter) Piece(chest, "dosimetro", new Vector3(-0.1f, 0.22f, 0.124f), new Vector3(0.04f, 0.05f, 0.008f), new Color(0.95f, 0.8f, 0.2f));
            if (s.wetStripes) { Piece(chest, "franja", new Vector3(0.2f, 0.15f, 0f), new Vector3(0.012f, 0.3f, 0.245f), new Color(0.2f, 0.55f, 0.95f)); Piece(chest, "franja", new Vector3(-0.2f, 0.15f, 0f), new Vector3(0.012f, 0.3f, 0.245f), new Color(0.2f, 0.55f, 0.95f)); }
        }
        if (back != null && s.hiVis) Piece(back, "banda", new Vector3(0f, 0.08f, 0f), new Vector3(0.372f, 0.025f, 0.228f), silver);
        if (hips != null)
        {
            if (s.apron || s.shortApron) Piece(hips, "delantal", new Vector3(0f, s.shortApron ? -0.04f : -0.08f, 0.116f), new Vector3(0.36f, s.shortApron ? 0.2f : 0.34f, 0.01f), s.apronColor);
            if (s.coatTails)
            {
                Piece(hips, "faldon", new Vector3(-0.09f, -0.12f, 0.116f), new Vector3(0.16f, 0.36f, 0.01f), s.top);
                Piece(hips, "faldon", new Vector3(0.09f, -0.12f, 0.116f), new Vector3(0.16f, 0.36f, 0.01f), s.top);
                Piece(hips, "faldon", new Vector3(0f, -0.12f, -0.116f), new Vector3(0.36f, 0.36f, 0.01f), s.top);
            }
            if (s.toolBelt)
            {
                Piece(hips, "cinturon", new Vector3(0f, 0.06f, 0f), new Vector3(0.37f, 0.05f, 0.25f), brown);
                Piece(hips, "bolsillo", new Vector3(0.15f, 0.0f, 0.12f), new Vector3(0.08f, 0.09f, 0.04f), brown);
                Piece(hips, "martillo", new Vector3(-0.17f, -0.02f, 0.06f), new Vector3(0.025f, 0.16f, 0.025f), new Color(0.7f, 0.55f, 0.4f));
            }
        }
        if (head != null)
        {
            float top = HeadTop(head);
            switch (s.hat)
            {
                case Uniforms.Hat.Toque: Piece(head, "gorro", new Vector3(0f, top + 0.02f, 0f), new Vector3(0.27f, 0.06f, 0.27f), s.hatColor); Piece(head, "gorro", new Vector3(0f, top + 0.12f, 0f), new Vector3(0.3f, 0.16f, 0.3f), s.hatColor); break;
                case Uniforms.Hat.ScrubCap: Piece(head, "gorro", new Vector3(0f, top - 0.005f, -0.005f), new Vector3(0.27f, 0.07f, 0.27f), s.hatColor); break;
                case Uniforms.Hat.Helmet: Piece(head, "casco", new Vector3(0f, top + 0.03f, 0f), new Vector3(0.3f, 0.1f, 0.3f), s.hatColor); Piece(head, "visera", new Vector3(0f, top - 0.01f, 0.16f), new Vector3(0.3f, 0.02f, 0.08f), s.hatColor); break;
                case Uniforms.Hat.Straw: Piece(head, "ala", new Vector3(0f, top, 0f), new Vector3(0.46f, 0.02f, 0.46f), s.hatColor); Piece(head, "copa", new Vector3(0f, top + 0.05f, 0f), new Vector3(0.26f, 0.09f, 0.26f), s.hatColor); break;
                case Uniforms.Hat.Cap: Piece(head, "gorra", new Vector3(0f, top + 0.01f, 0f), new Vector3(0.27f, 0.07f, 0.27f), s.hatColor); Piece(head, "visera", new Vector3(0f, top - 0.02f, 0.16f), new Vector3(0.22f, 0.015f, 0.1f), s.hatColor); break;
            }
            if (s.goggles) Piece(head, "gafas", new Vector3(0f, 0.17f, 0.125f), new Vector3(0.23f, 0.05f, 0.02f), new Color(0.75f, 0.9f, 0.95f, 0.65f));
        }
    }

    // Lo más alto de la cabeza (el pelo incluido), en el marco de la cabeza: ahí va el gorro, encima del peinado y no dentro de él.
    static float HeadTop(Transform head)
    {
        float top = 0.26f;
        foreach (Renderer r in head.GetComponentsInChildren<Renderer>())
        {
            if (r.name.StartsWith(CubeInsect.UniformPrefix)) continue;
            Bounds b = r.bounds;
            float y = head.InverseTransformPoint(new Vector3(b.center.x, b.max.y, b.center.z)).y;
            if (y > top && y < 0.6f) top = y;
        }
        return top;
    }
}
