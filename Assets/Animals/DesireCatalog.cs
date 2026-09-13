using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un DESEO seleccionable (docs/volition-selection-engine.md §3): cuánto lo quiere el ser AHORA (`NeedProbe`, de sus
/// drives) y qué acción despacha (`Dispatch`, un handler que YA existe). El destino es que un `Deseo` sea una
/// `MindPhrase(PhraseCategory.Deseo)` con este binding resuelto por catálogo (D3b3); por ahora es solo el binding.
/// </summary>
public class Desire
{
    public readonly string key;
    public readonly Func<Animal, float> NeedProbe;   // 0..1+ : necesidad actual (de los drives)
    public readonly Action<Animal> Dispatch;         // corre la acción existente
    public readonly string capability;               // clave de confianza (D3a) para D3b3; null = sin ponderar

    public Desire(string key, Func<Animal, float> need, Action<Animal> dispatch, string capability = null)
    {
        this.key = key; NeedProbe = need; Dispatch = dispatch; this.capability = capability;
    }
}

/// <summary>
/// Catálogo de DESEOS (docs/volition-selection-engine.md §5): `key → { NeedProbe, Dispatch }`. Cada `Dispatch` llama
/// a un handler que YA existe — el motor de volición reubica la DECISIÓN, no reescribe la conducta.
///
/// **D3b1 (paridad):** solo `eat` (= lo único que decidía `ActiveBehaveTick` además del reflejo de amenaza). En D3b2
/// se añaden `rest`/`wander`/`defend` (este último como deseo con piso de seguridad); en D3b3 los deseos pasan a ser
/// frases y la selección pondera por `EffectiveWeight` (confianza D3a) + gate por receptor (E2).
/// </summary>
public static class DesireCatalog
{
    static List<Desire> _all;
    public static List<Desire> All { get { if (_all == null) _all = Build(); return _all; } }

    static List<Desire> Build() => new List<Desire>
    {
        // COMER: necesidad = hambre (>0 cuando hungry>=0, como el guard actual). Despacha el Feed existente
        // (Forager.Hunt/Graze) — idéntico a lo que hacía RespondToHunger.
        new Desire("eat",
            self => self.hungry >= 0f ? Mathf.Max(0.01f, self.hungry) : 0f,
            self => self.StartCoroutine(self.Feed()),
            "forage"),

        // BUSCAR PAREJA (reproducción paso 1): en celo, dirigirse al gradiente de Estrus (otras animas en celo). Dormido
        // hasta que Volition esté activo (D3b2); el cortejo/concepción llega en el paso 2. docs/environmental-navigation.md.
        new Desire("mate",
            self => { EstrusState e = self.GetComponent<EstrusState>(); return e != null && e.InEstrus ? W_MATE : 0f; },
            self =>
            {
                if (self.nav == null || !self.nav.isOnNavMesh) return;
                Vector3 g = TraceField.Trail(self.transform.position, TraceChannel.Estrus);
                if (g.sqrMagnitude > 0.0001f) self.nav.SetDestination(self.transform.position + g.normalized * 6f);
            },
            "mate"),

        // ── APREMIOS SOCIALES como DESEOS (docs/consciousness-mechanics.md §4: la sim social es capacidad de Anima por
        // config; compite en la MISMA arena que comer/celo → de la pugna, resuelta por los stats propios, emerge la
        // personalidad). Reusan el registro de bonds por-miembro (Anima.bonds). Solo mueven vía NavMesh (Animal).

        // CUIDAR: acercarse al ser QUERIDO (bond+) que está en apuros (estrés/enfermo). Necesidad = vínculo × su apuro,
        // amplificada por la afabilidad del que cuida. Es el "Tend" de Sakshi/la tribu, ahora compitiendo con el hambre.
        new Desire("tend",
            self => SocialNeed(self, care: true) * Mathf.Max(0.3f, self.afabilidad) * W_TEND,
            self => ApproachBondTarget(self, care: true),
            "tend"),

        // SEGUIR/COHESIÓN: acercarse al ser querido más cercano (mantener la manada unida). Necesidad = vínculo × lejanía,
        // amplificada por la sociabilidad. Débil por diseño (fondo) → el hambre/amenaza lo superan; los muy sociales lo sienten más.
        new Desire("follow",
            self => SocialNeed(self, care: false) * Mathf.Max(0.2f, self.sociability) * W_FOLLOW,
            self => ApproachBondTarget(self, care: false),
            "follow"),

        // DESCANSAR (drive biológico real que faltaba): necesidad = sueño + fatiga. Despacha idle (se detiene; el
        // Restore pasivo recupera). Compite con todo → un ser agotado prioriza parar sobre deambular/comer un poco.
        new Desire("rest",
            self => Mathf.Clamp01((self.sleepiness + self.mentalFatigue) * 0.5f),
            self => { if (self.nav != null && self.nav.isOnNavMesh) self.nav.ResetPath(); self.Loco?.Idle(1f); },
            "rest"),

        // EXPLORAR (impulso de base del ser SACIADO — curiosidad/neophilia, docs/environmental-navigation.md §N5/N8):
        // necesidad BAJA (solo gana cuando no hay hambre/amenaza/social) y ESCALADA por la mente curiosa (razón+creatividad)
        // → los curiosos exploran más; los apáticos se quedan. Despacha un vagabundeo corto.
        new Desire("explore",
            self => W_EXPLORE * Mathf.Clamp01((self.reasoning + self.creativity) * 0.5f),
            self => { if (self.nav != null && self.nav.isOnNavMesh) { Vector2 r = Random.insideUnitCircle * 8f; self.nav.SetDestination(self.transform.position + new Vector3(r.x, 0f, r.y)); } },
            "explore"),
    };

    // ── PESOS DE LA ARENA (docs/consciousness-mechanics.md §4) — afinables. Filosofía: la SUPERVIVENCIA gana cuando
    // aprieta (eat = hambre, que puede ser >1; la amenaza es reflejo aparte), pero lo SOCIAL gana cuando el ser está
    // saciado. El celo NO debe atropellar a comer. La resolución final depende de los STATS propios → personalidad.
    const float W_MATE    = 0.6f;    // celo moderado (antes 1.0 → atropellaba el hambre media)
    const float W_TEND    = 0.9f;    // cuidar al vínculo en apuro: fuerte (× afabilidad)
    const float W_FOLLOW  = 0.35f;   // cohesión: de fondo (× sociabilidad); el hambre/amenaza lo superan
    const float W_EXPLORE = 0.08f;   // curiosidad de base: muy bajo → solo gana cuando el ser está saciado y en calma

    // Necesidad social: recorre los bonds POSITIVOS y devuelve la mayor "urgencia" — cuidar = vínculo×apuro del otro;
    // seguir = vínculo×lejanía. 0..1+. No mueve; solo puntúa (lo usa NeedProbe).
    static float SocialNeed(Animal self, bool care)
    {
        float best = 0f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || b.target == null) continue;
            Transform t = b.target.transform; if (t == null) continue;
            Anima o = t.GetComponentInParent<Anima>(); if (o == null || o == self) continue;
            float tie = Mathf.Clamp01(b.value / 100f);
            float urg = care ? Mathf.Clamp01(o.stress)                                   // cuidar: cuanto peor está el otro
                             : Mathf.Clamp01(Vector3.Distance(self.transform.position, t.position) / 20f);  // seguir: cuán lejos
            best = Mathf.Max(best, tie * urg);
        }
        return best;
    }

    // Despacho social: navega hacia el ser querido elegido (el más urgente por cuidado, o el más cercano por cohesión).
    static void ApproachBondTarget(Animal self, bool care)
    {
        if (self.nav == null || !self.nav.isOnNavMesh) return;
        Transform best = null; float bestScore = 0f;
        foreach (Bond b in self.bonds)
        {
            if (b == null || b.value <= 0f || b.target == null) continue;
            Transform t = b.target.transform; if (t == null) continue;
            Anima o = t.GetComponentInParent<Anima>(); if (o == null || o == self) continue;
            float tie = Mathf.Clamp01(b.value / 100f);
            float urg = care ? Mathf.Clamp01(o.stress)
                             : Mathf.Clamp01(Vector3.Distance(self.transform.position, t.position) / 20f);
            float score = tie * urg;
            if (score > bestScore) { bestScore = score; best = t; }
        }
        if (best != null) self.nav.SetDestination(best.position);
    }
}
