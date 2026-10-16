using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatiza por `TestProbe` ítems de PROGRESIÓN/MENTE de `docs/testing-checklist.md` que quedaron sin verificar
/// en las pasadas manuales: la rama "pierde el control" de `PlayableCreature` (§3), la marga de Yoga desbloquea el
/// maná y cada marga sube los puntos del alma (§4), la longitud de frase por poder mental, el `ThoughtField` (tono y
/// humor) y el reparto Estricta/Libre (§8-§9), el `physicalResistance` por defecto (§10) y el log `[ECO]` (§39).
///
/// Determinista y NO destructivo: seres/mentes de prueba propios (destruidos al final). Lo único que toca del
/// jugador real es su VIDA (`CharacterLevel.currentHealth`) en la prueba de pérdida de control, y la restaura.
/// </summary>
public class ChecklistProgressTest : MonoBehaviour, ITestUnit
{
    public int Group => 24;
    public bool ParallelSafe => false;   // daña (y cura) al jugador real

    readonly List<GameObject> _trash = new List<GameObject>();

    public IEnumerator Run()
    {
        TestProbe.Begin("ChecklistProgress (§3-§10 margas, mente, reparto; §39 [ECO])");
        yield return null;

        yield return PierdeElControl();   // §3
        Margas();                         // §4
        LongitudPorPoderMental();         // §8
        yield return CampoDePensamiento();// §9
        Reparto();                        // §9
        ResistenciaFisica();              // §10
        yield return LogEco();            // §39

        foreach (GameObject g in _trash) if (g != null) Destroy(g);
        _trash.Clear();
        TestProbe.End();
    }

    GameObject New(string name, Vector3 pos)
    {
        GameObject g = new GameObject(name);
        g.transform.position = pos;
        _trash.Add(g);
        return g;
    }

    // ── §3 PlayableCreature: rama "pierde el control" ────────────────────────────
    IEnumerator PierdeElControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        CharacterLevel cl = player != null ? player.GetComponent<CharacterLevel>() : null;
        if (cl == null) { TestProbe.Check("[733] hay jugador con CharacterLevel (para el daño de Dura)", false, "no encontrado"); yield break; }
        float savedHealth = cl.currentHealth;

        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Dura_TEST";
        g.GetComponent<Collider>().isTrigger = true;    // que no empuje al jugador
        g.transform.position = player.transform.position;   // pegada al jugador (dentro de catchRange)
        g.transform.localScale = Vector3.one * 0.3f;
        _trash.Add(g);
        PlayableCreature c = g.AddComponent<PlayableCreature>();
        c.handRaised = true;
        c.canLoseControl = true;
        c.loseControlAbove = 0.5f;
        c.catchGrace = 0.1f;
        c.excitementDecayPerSec = 0f;           // que la excitación no se enfríe mientras espera
        c.excitementPerTap = 0.3f;
        c.dischargePerTouch = 0.01f;            // que no llegue a serenarse (sin botín)
        c.looseControlDamage = (cl.PassiveDefense + 10f) / 0.5f;   // supera la defensa pasiva

        c.Play(); c.Play();
        TestProbe.Check("[733] provocarla sube la excitación por encima del umbral de riesgo", c.Excitement >= c.loseControlAbove,
                        $"excitación {c.Excitement:0.##} ≥ {c.loseControlAbove:0.##}");
        float h0 = cl.currentHealth;
        float until = Time.time + 2f;
        while (Time.time < until && cl.currentHealth >= h0) yield return null;
        TestProbe.Check("[733] quedarse pegado con la excitación alta → PIERDE EL CONTROL y daña al jugador", cl.currentHealth < h0,
                        $"vida {h0:0}→{cl.currentHealth:0}");
        TestProbe.Near("[733] tras la atrapada, la excitación se resetea", c.Excitement, 0f, 0.001f);

        // Contraste: sin `canLoseControl`, la atrapada es solo juego (sin daño).
        cl.currentHealth = savedHealth;
        c.canLoseControl = false;
        g.transform.position = player.transform.position + new Vector3(3f, 0f, 0f);   // fuera de catchRange: no re-atrapa
        until = Time.time + 0.8f;                // dejar pasar el cooldown de la atrapada (0,6 s)
        while (Time.time < until) yield return null;
        g.transform.position = player.transform.position;
        c.Play(); c.Play();
        float h1 = cl.currentHealth;
        until = Time.time + 1f;
        while (Time.time < until && c.Excitement > 0f) yield return null;
        TestProbe.Check("[733] sin canLoseControl, la atrapada no hace daño (rama segura)", c.Excitement == 0f && cl.currentHealth == h1,
                        $"excitación {c.Excitement:0.##}, vida {h1:0}→{cl.currentHealth:0}");

        cl.currentHealth = savedHealth;          // restaurar la vida del jugador
        Destroy(g);
    }

    // ── §4 margas ────────────────────────────────────────────────────────────────
    void Margas()
    {
        CharacterLevel cl = New("Margas_TEST", new Vector3(9000f, -700f, 9000f)).AddComponent<CharacterLevel>();
        float mana0 = cl.MaxMana, hp0 = cl.MaxHealth;
        TestProbe.Check("[758] al empezar el maná está bloqueado (Yoga 1)", !cl.ManaUnlocked && cl.SoulLevels == 0);
        cl.GainYogaXp(cl.yoga.XpToNext);
        TestProbe.Check("[758] marga de Yoga nivel 2 → maná DESBLOQUEADO", cl.yoga.level == 2 && cl.ManaUnlocked, $"yoga L{cl.yoga.level}");
        TestProbe.Greater("[758] …y el MaxMana sube", cl.MaxMana, mana0, $"{mana0:0}→{cl.MaxMana:0}");
        TestProbe.Near("[758] los puntos se rellenan al subir (maná actual = máximo)", cl.currentMana, cl.MaxMana, 0.01f);

        float hp1 = cl.MaxHealth;
        TestProbe.Check("[763] subir Yoga sube los puntos del alma (SoulLevels 1, vida ↑)", cl.SoulLevels == 1 && hp1 > hp0, $"vida {hp0:0}→{hp1:0}");
        cl.GainStatsXp(cl.stats.XpToNext);
        float hp2 = cl.MaxHealth;
        TestProbe.Check("[763] subir Stats sube los puntos del alma (SoulLevels 2, vida ↑)", cl.SoulLevels == 2 && hp2 > hp1, $"vida {hp1:0}→{hp2:0}");
        cl.GainBondXp(cl.bonds.XpToNext);
        float hp3 = cl.MaxHealth;
        TestProbe.Check("[763] subir Vínculos sube los puntos del alma (SoulLevels 3, vida ↑)", cl.SoulLevels == 3 && hp3 > hp2, $"vida {hp2:0}→{hp3:0}");
    }

    // ── §8 poder mental → longitud de la frase ───────────────────────────────────
    Mind MakeMind(string name, Vector3 pos, Aptitudes apt)
    {
        Mind m = New(name, pos).AddComponent<Mind>();
        m.enabled = false;            // sin Update: solo pensamientos forzados (SpeakNow)
        m.aptitudes = apt;
        return m;
    }

    static Aptitudes Power(float mental)
    {
        Aptitudes a = Aptitudes.Default;
        a.reasoning = mental; a.memory = mental; a.discipline = mental;
        return a;
    }

    static void Fresh(Mind m) { m.humores = new Humores(); m.lastThought = ""; m.lastIntensity = 0f; }

    void LongitudPorPoderMental()
    {
        Mind low = MakeMind("MenteDebil_TEST", new Vector3(9010f, -700f, 9000f), Power(0.6f));
        Mind high = MakeMind("MentePoderosa_TEST", new Vector3(9020f, -700f, 9000f), Power(1.5f));
        Mind sage = MakeMind("MenteSabia_TEST", new Vector3(9030f, -700f, 9000f), Power(2.5f));

        float li = 0f, hi = 0f; int lw = 0, hw = 0;
        for (int i = 0; i < 6; i++)
        {
            Fresh(low); low.SpeakNow(); li += low.lastIntensity; lw += Words(low.lastThought);
            Fresh(high); high.SpeakNow(); hi += high.lastIntensity; hw += Words(high.lastThought);
        }
        TestProbe.Check("[834] más poder mental → llega MÁS LEJOS en la frase (intensidad)", hi > li, $"débil {li / 6f:0.##} · poderosa {hi / 6f:0.##}");
        TestProbe.Check("[834] …y la frase es más LARGA", hw > lw, $"palabras débil {lw} · poderosa {hw}");
        Fresh(sage); sage.SpeakNow();
        TestProbe.Check("[834] una mente muy poderosa llega al SILENCIO (no formula texto)", sage.lastThought == "",
                        $"\"{sage.lastThought}\"");
    }

    static int Words(string s) => string.IsNullOrEmpty(s) ? 0 : s.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;

    // ── §9 ThoughtField: tono + humor ────────────────────────────────────────────
    IEnumerator CampoDePensamiento()
    {
        Vector3 f = new Vector3(9500f, -700f, 9500f);
        ThoughtField field = New("CampoTierra_TEST", f).AddComponent<ThoughtField>();
        field.tone = ElementalTone.Tierra; field.radius = 6f; field.pull = 100f;
        field.nudgesHumor = true; field.humor = Humor.Serotonina; field.humorPerSecond = -1f;

        // Aptitudes que NO empujan a Tierra (fuerza/resistencia/masa 0): sin el campo, nunca piensa en Tierra.
        Aptitudes apt = new Aptitudes { memory = 2f, discipline = 2f };
        Mind inside = MakeMind("MenteEnCampo_TEST", f, apt);
        Mind outside = MakeMind("MenteFuera_TEST", f + new Vector3(50f, 0f, 0f), apt);
        TestProbe.Check("[840] el campo cubre a la mente de dentro y no a la de fuera", field.Covers(inside.transform.position) && !field.Covers(outside.transform.position));

        inside.enabled = true; outside.enabled = true;     // que refresquen sus campos y apliquen humores
        float until = Time.time + 0.4f;
        while (Time.time < until) yield return null;
        inside.enabled = false; outside.enabled = false;
        TestProbe.Check("[840] el campo mueve el HUMOR de quien está dentro (serotonina ↓)", inside.humores.serotonina < outside.humores.serotonina - 0.1f,
                        $"dentro {inside.humores.serotonina:0.##} · fuera {outside.humores.serotonina:0.##}");

        HashSet<string> tierra = new HashSet<string>();
        foreach (MindPhrase p in PhraseLibrary.ForTone(ElementalTone.Tierra))
        {
            if (p.positive != null && p.positive.Length > 0) tierra.Add(p.positive[0]);
            if (p.negative != null && p.negative.Length > 0) tierra.Add(p.negative[0]);
        }
        int inT = 0, outT = 0, inN = 0, outN = 0;
        for (int i = 0; i < 20; i++)
        {
            if (IsTone(inside, tierra, out bool a)) { inN++; if (a) inT++; }
            if (IsTone(outside, tierra, out bool b)) { outN++; if (b) outT++; }
        }
        TestProbe.Check("[840] dentro del campo, el TONO tiende al del campo (Tierra)", inN > 0 && inT >= inN * 0.75f, $"Tierra {inT}/{inN}");
        TestProbe.Check("[840] fuera del campo, sin aptitudes de Tierra, casi nunca piensa en Tierra", outN > 0 && outT <= outN * 0.1f, $"Tierra {outT}/{outN}");
    }

    static bool IsTone(Mind m, HashSet<string> firstParts, out bool match)
    {
        m.humores.glucosa = 0.6f; m.humores.adrenalina = 0.4f;   // misma energía cada vez (la expresión gasta glucosa)
        m.lastThought = "";
        m.SpeakNow();
        match = false;
        if (string.IsNullOrEmpty(m.lastThought)) return false;
        foreach (string s in firstParts) if (!string.IsNullOrEmpty(s) && m.lastThought.StartsWith(s)) { match = true; break; }
        return true;
    }

    // ── §9 reparto Estricta / Libre ──────────────────────────────────────────────
    void Reparto()
    {
        string[] ids = { "Goluis", "Irosene", "Panterilia", "Gohageneis" };
        var strict = new List<PhraseHolder>();
        foreach (string id in ids) strict.Add(new PhraseHolder(id, false));
        List<MindPhrase>[] planS = PhraseDistribution.Plan(NarrativeMode.Estricta, strict);
        bool own = true; int total = 0;
        for (int i = 0; i < ids.Length; i++)
        {
            List<MindPhrase> mine = PhraseLibrary.VivenciasOf(ids[i]);
            total += mine.Count;
            if (planS[i].Count != mine.Count) own = false;
            foreach (MindPhrase p in mine) if (!planS[i].Contains(p)) own = false;
        }
        TestProbe.Check("[846] Estricta: cada ser recibe EXACTAMENTE las vivencias de su identidad", own && total > 0, $"{total} vivencias");

        var libre = new List<PhraseHolder> { new PhraseHolder("Goluis", true) };   // bloqueado (como la Magnate/históricos)
        for (int i = 1; i < ids.Length; i++) libre.Add(new PhraseHolder(ids[i], false));
        List<MindPhrase>[] planL = PhraseDistribution.Plan(NarrativeMode.Libre, libre);
        List<MindPhrase> goluis = PhraseLibrary.VivenciasOf("Goluis");
        bool lockedKeeps = planL[0].Count == goluis.Count;
        foreach (MindPhrase p in goluis) if (!planL[0].Contains(p)) lockedKeeps = false;
        TestProbe.Check("[846] Libre: el BLOQUEADO conserva los suyos", lockedKeeps);

        HashSet<MindPhrase> pool = new HashSet<MindPhrase>();
        for (int i = 1; i < ids.Length; i++) foreach (MindPhrase p in PhraseLibrary.VivenciasOf(ids[i])) pool.Add(p);
        HashSet<MindPhrase> dealt = new HashSet<MindPhrase>();
        bool leak = false;
        for (int i = 1; i < planL.Length; i++)
            foreach (MindPhrase p in planL[i]) { dealt.Add(p); if (goluis.Contains(p) || !pool.Contains(p)) leak = true; }
        TestProbe.Check("[846] Libre: el pool PÚBLICO se reparte entero entre los libres", dealt.SetEquals(pool), $"{dealt.Count}/{pool.Count}");
        TestProbe.Check("[846] Libre: nadie hereda los pensamientos del bloqueado", !leak);
    }

    // ── §10 physicalResistance ───────────────────────────────────────────────────
    void ResistenciaFisica()
    {
        Anima a = New("Resistencia_TEST", new Vector3(9100f, -700f, 9000f)).AddComponent<Anima>();
        TestProbe.Near("[880] physicalResistance por defecto = 1 en todo Anima (NPCs incluidos)", a.physicalResistance, 1f, 0.0001f);
        int n = 0, bad = 0;
        foreach (WorldCharacter wc in FindObjectsOfType<WorldCharacter>())
        {
            Anima wa = wc.GetComponent<Anima>();
            if (wa == null) continue;
            n++;
            if (!Mathf.Approximately(wc.Strength, wa.physicalResistance) || wa.physicalResistance < 0f || wa.physicalResistance > 1f) bad++;
        }
        TestProbe.Check("[880] los NPC (WorldCharacter) leen su fuerza de Anima.physicalResistance (0–1)", bad == 0, $"{n} NPC, {bad} incoherentes");
    }

    // ── §39 log [ECO] ────────────────────────────────────────────────────────────
    string _eco;
    void CaptureEco(string msg, string stack, LogType t) { if (msg != null && msg.StartsWith("[ECO] vivos=")) _eco = msg; }

    IEnumerator LogEco()
    {
        _eco = null;
        Application.logMessageReceived += CaptureEco;
        GameObject g = New("EcoObservation_TEST", new Vector3(9200f, -700f, 9000f));
        EcosystemObservation eo = g.AddComponent<EcosystemObservation>();
        eo.logInterval = 1f;
        float until = Time.time + 3f;
        while (Time.time < until && _eco == null) yield return null;
        Application.logMessageReceived -= CaptureEco;
        Destroy(g);

        TestProbe.NotNull("[1437] EcosystemObservation loguea el resumen [ECO]", _eco);
        if (_eco != null)
            TestProbe.Check("[1437] el resumen trae vivos + tendencia (Δ) + herbívoros/depredadores",
                            _eco.Contains("(Δ") && _eco.Contains("herbívoros=") && _eco.Contains("depredadores="), _eco.Length > 160 ? _eco.Substring(0, 160) + "…" : _eco);
    }
}
