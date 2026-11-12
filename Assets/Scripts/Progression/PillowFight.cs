using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LA GUERRA DE ALMOHADAS en el ring de la arboleda (docs/guerra-de-almohadas.md; Berón 2026-11-04). La lleva el horario libre
/// (<see cref="LeisureDirector"/>, pose «almohadas»): cuando cada uno llega a su celda del colchón,
///  1. COGE UNA ALMOHADA del ring (la suelta más cercana se esconde y aparece una en su mano derecha);
///  2. cada ~1,1–1,7 s busca al luchador más cercano a su alcance (≤ <see cref="Reach"/> m), se gira hacia él y le da un ALMOHADAZO
///     (la almohada barre en arco);
///  3. los dos RÍEN (cara alegre), el vínculo de amistad sube, el estrés baja y sueltan serotonina.
/// ACOTADO: como mucho <see cref="MaxHitsPerSlot"/> golpes por persona y franja; nadie se mueve de su celda (no hay saltos ni
/// empujones). Al acabar la franja (o si le llama una necesidad) deja la almohada en el ring (<see cref="Stop"/>).
/// </summary>
public static class PillowFight
{
    public const float Reach = 2.9f;            // un poco más que la celda (2,5 m): llega a los vecinos de al lado
    public const int MaxHitsPerSlot = 40;
    public const float BondPerHit = 0.4f, StressPerHit = 0.01f;

    class State { public Transform pillow, taken; public float next, swingAt = -10f; public int hits; }
    static readonly Dictionary<GameObject, State> _s = new Dictionary<GameObject, State>();

    /// <summary>Golpes y risas desde que se empezó a contar (las pruebas lo ponen a cero).</summary>
    public static int Hits, Laughs, PickedUp;
    public static void ResetCounters() { Hits = Laughs = PickedUp = 0; }
    /// <summary>Quién lleva ahora almohada en la mano.</summary>
    public static IEnumerable<GameObject> Fighters => _s.Keys;
    public static bool Holding(GameObject g) => g != null && _s.TryGetValue(g, out State st) && st.pillow != null;

    /// <summary>Un fotograma de guerra de <paramref name="g"/> en el ring <paramref name="ring"/> (ya está en su celda).</summary>
    public static void Tick(GameObject g, Anima a, PillowRing ring)
    {
        if (g == null || a == null) return;
        int h = Mathf.Abs(g.name.GetHashCode());
        if (!_s.TryGetValue(g, out State st))
        {
            st = new State { next = Time.time + 0.4f + (h % 10) * 0.08f };
            PickUp(g, ring, st);
            _s[g] = st;
            DailyPoses.Stand().ApplyFinal(a);
        }
        // El ARCO del almohadazo: 0,35 s de ida y vuelta.
        if (st.pillow != null)
        {
            float u = (Time.time - st.swingAt) / 0.35f;
            float ang = u >= 0f && u <= 1f ? Mathf.Sin(u * Mathf.PI) * 110f : 0f;
            st.pillow.localRotation = Quaternion.Euler(-ang, 0f, 0f);
        }
        if (Time.time < st.next || st.hits >= MaxHitsPerSlot) return;
        st.next = Time.time + 1.1f + (h % 7) * 0.09f;
        Anima target = Nearest(g);
        if (target == null) return;
        Vector3 look = target.transform.position - g.transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) g.transform.rotation = Quaternion.LookRotation(look);
        st.swingAt = Time.time; st.hits++; Hits++;
        // RÍEN los dos (cara alegre un momento), el vínculo sube y el estrés baja.
        g.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Happy, 1.4f);
        target.GetComponentInChildren<CubeFace>()?.Show(CubeFace.Expression.Happy, 1.4f);
        Laughs += 2;
        a.GrowBond(ConversationDirector.Target(target), BondType.Friend, BondPerHit);
        target.GrowBond(ConversationDirector.Target(a), BondType.Friend, BondPerHit);
        a.stress = Mathf.Clamp01(a.stress - StressPerHit); target.stress = Mathf.Clamp01(target.stress - StressPerHit);
        if (a.TryGetComponent(out Mind ma)) ma.humores.Produce(Humor.Serotonina, 0.01f);
        if (target.TryGetComponent(out Mind mt)) mt.humores.Produce(Humor.Serotonina, 0.01f);
    }

    // El luchador más cercano (que también lleva almohada) a su alcance.
    static Anima Nearest(GameObject g)
    {
        Anima best = null; float bd = Reach * Reach;
        foreach (var kv in _s)
        {
            if (kv.Key == null || kv.Key == g || kv.Value.pillow == null) continue;
            Vector3 d = kv.Key.transform.position - g.transform.position; d.y = 0f;
            if (d.sqrMagnitude > bd || !kv.Key.TryGetComponent(out Anima o) || o.asleep || o.death) continue;
            bd = d.sqrMagnitude; best = o;
        }
        return best;
    }

    // COGER UNA ALMOHADA: la suelta más cercana del ring se esconde (se la ha llevado) y aparece en su mano derecha.
    static void PickUp(GameObject g, PillowRing ring, State st)
    {
        Transform near = null; float bd = 9f;
        if (ring != null && ring.pillows != null)
            foreach (Transform p in ring.pillows)
            {
                if (!p.gameObject.activeSelf) continue;
                float d = (p.position - g.transform.position).sqrMagnitude;
                if (d < bd) { bd = d; near = p; }
            }
        Color c = new Color(0.97f, 0.93f, 0.96f);
        if (near != null) { st.taken = near; near.gameObject.SetActive(false); Renderer nr = near.GetComponent<Renderer>(); if (nr != null) c = nr.sharedMaterial.color; }
        Transform hand = null;
        foreach (CubeJoint j in g.GetComponentsInChildren<CubeJoint>()) if (j.part == BodyPart.HandRight) { hand = j.transform; break; }
        Transform pivot = new GameObject("Almohada_en_mano").transform;
        pivot.SetParent(hand != null ? hand : g.transform, false);
        pivot.localPosition = hand != null ? new Vector3(0f, -0.08f, 0f) : new Vector3(0.35f, 0.1f, 0.3f);
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(cube.GetComponent<Collider>());
        cube.name = "Almohada";
        cube.transform.SetParent(pivot, false);
        cube.transform.localPosition = new Vector3(0f, -0.25f, 0.05f);
        Vector3 ls = pivot.lossyScale;   // (la mano está escalada con el cuerpo: la almohada mide 0,55 × 0,14 × 0,38 m en el mundo)
        cube.transform.localScale = new Vector3(0.38f / Mathf.Max(0.01f, ls.x), 0.55f / Mathf.Max(0.01f, ls.y), 0.14f / Mathf.Max(0.01f, ls.z));
        cube.GetComponent<Renderer>().sharedMaterial = CubeCreature.Mat(c);
        st.pillow = pivot; PickedUp++;
    }

    /// <summary>Deja la almohada en el ring (vuelve a aparecer la que cogió) y olvida su estado.</summary>
    public static void Stop(GameObject g)
    {
        if (g == null || !_s.TryGetValue(g, out State st)) return;
        if (st.pillow != null) Object.Destroy(st.pillow.gameObject);
        if (st.taken != null) st.taken.gameObject.SetActive(true);
        _s.Remove(g);
    }

    /// <summary>Golpes que lleva <paramref name="g"/> en esta franja.</summary>
    public static int HitsOf(GameObject g) => g != null && _s.TryGetValue(g, out State st) ? st.hits : 0;
}
