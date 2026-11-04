using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DE QUÉ HABLAN (docs/conversaciones.md §Temas). Los temas salen del CONTEXTO de los dos, no de un árbol de diálogo: su trabajo
/// (<see cref="MicroJornada"/>, la jornada del santuario), la actividad que comparten (el mismo puesto, la comida, la misma franja
/// libre), su familia (<see cref="PersonLife"/>: pareja, hijos, madre), su linaje de insecto (<see cref="FolkLineage"/>), la era y el
/// lugar (<see cref="MicroEra"/>, <see cref="ConversationContext"/>), la historia del sitio, sus necesidades (hambre, cansancio) y lo
/// que piensa su <see cref="Mind"/>. Cada tema pesa según el contexto; lo ya hablado con esa persona pesa menos (memoria).
/// El guion son 4–6 frases cortas que se turnan (A, B, A, B…), compuestas de plantillas con los datos de los dos: pocas plantillas ×
/// muchos datos = variedad barata (docs/mind-model.md §6).
/// </summary>
public static class ConversationTopics
{
    public struct Script { public string topic, title; public bool positive; public List<string> lines; }

    /// <summary>La actividad que comparten AHORA: «trabajo» (mismo puesto o los dos en turno), «comida» (el mismo comedor a la hora de
    /// comer) u «ocio» (la misma actividad de su franja libre); null si nada.</summary>
    public static string SharedActivity(Anima a, Anima b)
    {
        if (a == null || b == null) return null;
        if (a.TryGetComponent(out MicroJornada ja) && b.TryGetComponent(out MicroJornada jb))
        {
            string block = MicroJornada.BlockAt(MicroJornada.Hour);
            if (block == "comida" && (ja.dine ?? ja.work) != null && (ja.dine ?? ja.work) == (jb.dine ?? jb.work)) return "comida";
            if (block == "trabajo" && ja.work != null && ja.work == jb.work) return "trabajo";
        }
        string la = Leisure(a), lb = Leisure(b);
        if (la != null && la == lb) return "ocio";
        WorkdayDirector wd = WorkdayDirector.Instance;
        if (wd != null && wd.OnShift(a.gameObject) && wd.OnShift(b.gameObject)) return "trabajo";
        return null;
    }

    /// <summary>El tema y el guion de una charla que abre <paramref name="a"/> con <paramref name="b"/>.</summary>
    public static Script Compose(Anima a, Anima b, string shared, HashSet<string> recent)
    {
        var options = new List<(string key, float w)>();
        void Opt(string k, float w) { if (w > 0f) options.Add((k, recent != null && recent.Contains(k) ? w * 0.25f : w)); }

        bool strangers = ConversationDirector.Bond(a, b) < 3f && ConversationDirector.Bond(b, a) < 3f;
        bool ia = ConversationDirector.IsInsect(a), ib = ConversationDirector.IsInsect(b);
        Opt("presentacion", strangers ? 3f : 0f);
        Opt("trabajo", shared == "trabajo" ? 4f : (WorkOf(a) != null || WorkOf(b) != null ? 1.5f : 0f));
        Opt("comida", shared == "comida" ? 4f : 0f);
        Opt("ocio", shared == "ocio" ? 4f : 0f);
        Opt("familia", Related(a, b) != null ? 3f : (HasFamily(a) ? 1.5f : 0f));
        Opt("especie", ia != ib ? 2.5f : (ia && ib ? (Species(a) != Species(b) ? 2f : 1f) : 0f));
        bool era = MicroEra.Current != null || ConversationContext.Current != null;
        Opt("era", era ? 1.5f : 0f);
        Opt("santuario", era ? 0f : 1f);
        Opt("historia", ConversationContext.Current != null && ConversationContext.Current.story != null && ConversationContext.Current.story.Length > 0 ? 2f : 0f);
        BodyMetabolism m = a.GetComponent<BodyMetabolism>();
        Opt("hambre", m != null && m.hunger >= 0.5f ? 2f : 0f);
        Opt("cansancio", a.sleepiness >= 0.55f || a.stress >= 0.6f ? 2f : 0f);
        Opt("pensamiento", a.GetComponent<Mind>() != null ? 1f : 0f);

        float total = 0f; foreach (var o in options) total += o.w;
        string key = "santuario";
        float r = Random.value * total;
        foreach (var o in options) { if ((r -= o.w) < 0f) { key = o.key; break; } }

        Script s = Build(key, a, b, shared);
        if (s.lines == null || s.lines.Count < 4) s = Build("santuario", a, b, shared);
        // Una charla buena entre quienes ya se aprecian (o muy sociables) se alarga con su despedida.
        if (s.lines.Count == 4 && (ConversationDirector.Bond(a, b) > 20f || 0.5f * (a.sociability + b.sociability) > 1.2f || Random.value < 0.35f))
        {
            s.lines.Add(Pick("Bueno, te dejo, que hay cosas que hacer.", "Me ha gustado hablar contigo.", "Seguimos luego, ¿vale?"));
            s.lines.Add(Pick($"Hasta luego, {ConversationDirector.Short(a)}.", "Cuando quieras, aquí estoy.", "Vale, luego me cuentas."));
        }
        return s;
    }

    static Script Build(string key, Anima a, Anima b, string shared)
    {
        string sa = ConversationDirector.Short(a), sb = ConversationDirector.Short(b);
        bool bad = a.stress > 0.7f;
        var s = new Script { topic = key, positive = !bad, lines = new List<string>() };
        List<string> L = s.lines;
        switch (key)
        {
            case "presentacion":
                s.title = "presentarse";
                L.Add($"Hola, soy {sa}. {Intro(a)}");
                L.Add($"Un gusto, {sa}. Yo soy {sb}. {Intro(b)}");
                L.Add(Pick("¿Llevas mucho por aquí?", "¿Y qué tal te tratan por aquí?", "¿Conoces ya a todos?"));
                L.Add(Pick("Lo justo para saber dónde está la comida.", "Bien; poco a poco me hago al sitio.", "A casi todos; ahora también a ti."));
                break;
            case "trabajo":
            {
                string w = WorkOf(a) ?? WorkOf(b) ?? "lo de siempre";
                s.title = "su trabajo";
                L.Add($"¿Qué tal va lo tuyo? Yo sigo con {Lower(w)}.");
                L.Add(bad ? "Cuesta; hoy no sale nada a la primera." : $"Bien, {sa}. {(WorkOf(b) != null ? "Ando con " + Lower(WorkOf(b)) + "." : "Hoy cunde.")}");
                L.Add(Pick("Si necesitas una mano, me dices.", "Entre los dos se hace antes.", "Así da gusto trabajar."));
                L.Add(Pick("Gracias. Luego te cuento cómo acaba.", "Lo mismo digo.", "Venga, a ello."));
                break;
            }
            case "comida":
                s.title = "la comida";
                L.Add(Pick("¡Por fin la comida! ¿Qué tal lo tuyo?", "Qué bien huele esto, ¿verdad?", "¿Te sientas conmigo?"));
                L.Add(Pick("Rico. Esto da fuerzas para la tarde.", "Muy bien; tenía hambre.", "Claro, aquí hay sitio."));
                L.Add(Pick($"En {Place()} se come bien.", "Comer acompañado sabe mejor.", "Luego toca volver al trabajo."));
                L.Add(Pick("Y mejor en compañía.", "Eso digo yo.", "Pues a disfrutarlo mientras."));
                break;
            case "ocio":
            {
                string t = Leisure(a) ?? "esto";
                s.title = t;
                L.Add($"¿Tú también a {Lower(t)}?");
                L.Add(Pick("Sí, me despeja después del trabajo.", "Sí; es lo que más espero de la semana.", "Me apetecía probarlo."));
                L.Add(Pick("Así da gusto la semana.", "Pues hacemos equipo.", "A ver qué tal se nos da."));
                L.Add(Pick("La próxima vez vamos juntos.", "Hecho.", "¡Vamos allá!"));
                break;
            }
            case "familia":
            {
                s.title = "su familia";
                string rel = Related(a, b);
                PersonLife pa = a.GetComponent<PersonLife>();
                if (rel == "pareja")
                {
                    L.Add($"{sb}, ¿te acuerdas de cuando nos conocimos?");
                    L.Add("Como si fuera ayer.");
                    L.Add(pa != null && pa.children > 0 ? $"Y ahora con {Kids(pa.children)}…" : "Quién nos lo iba a decir.");
                    L.Add("Ha merecido la pena.");
                }
                else if (rel == "padre" || rel == "madre")
                {
                    L.Add(Pick("¿Has comido bien hoy?", "¿Qué tal te ha ido el día?", "No te olvides de descansar."));
                    L.Add($"Sí, {(pa != null && pa.female ? "mamá" : "papá")}. No te preocupes.");
                    L.Add("Me preocupo porque te quiero.");
                    L.Add("Lo sé. Yo también a ti.");
                }
                else if (rel == "hijo")
                {
                    PersonLife pb = b.GetComponent<PersonLife>();
                    L.Add($"{(pb != null && pb.female ? "Mamá" : "Papá")}, ¿te ayudo con algo?");
                    L.Add("Con que estés bien me basta.");
                    L.Add("Estoy bien. Aprendo mucho.");
                    L.Add("Se nota. Estoy orgulloso de ti.".Replace("orgulloso", pb != null && pb.female ? "orgullosa" : "orgulloso"));
                }
                else
                {
                    L.Add(FamilyLine(a) ?? "Echo de menos a los míos.");
                    L.Add(Pick("¡Qué bien! ¿Cómo lo lleváis?", "Eso es lo que importa.", "La familia es lo primero."));
                    L.Add(Pick("Con paciencia y mucho cariño.", "Como podemos, pero juntos.", "Bien; cada día un poco mejor."));
                    L.Add(FamilyLine(b) ?? "Yo todavía estoy en ello.");
                }
                break;
            }
            case "especie":
            {
                bool ia = ConversationDirector.IsInsect(a), ib = ConversationDirector.IsInsect(b);
                s.title = "de dónde vienen";
                if (!ia && ib)
                {
                    L.Add("¿Puedo preguntarte de qué familia vienes?");
                    L.Add(Ancestry(b));
                    L.Add("¡Qué mezcla tan bonita!");
                    L.Add("Cada uno saca algo de los suyos.");
                }
                else if (ia && !ib)
                {
                    L.Add("¿Nunca te ha dado curiosidad tener seis patas?");
                    L.Add("Alguna vez… ¿Cómo es?");
                    L.Add(Ancestry(a));
                    L.Add("Me lo tienes que contar despacio.");
                }
                else
                {
                    L.Add($"Tú eres de linaje de {Es(Species(b))}, ¿no?");
                    L.Add(Ancestry(b));
                    L.Add(Ancestry(a));
                    L.Add(Species(a) == Species(b) ? "Entonces somos casi familia." : "Distintos, pero del mismo pueblo.");
                }
                break;
            }
            case "era":
            {
                s.title = Place();
                string e = EraText();
                L.Add(string.IsNullOrEmpty(e) ? $"Aquí en {Place()} cada día se parece al anterior." : $"Aquí en {Place()}, {e}, cada día se parece al anterior.");
                L.Add(Pick("Pero cada día aprendemos algo.", "Será la costumbre.", "A mí me gusta así."));
                L.Add(Pick("Cuando cuente esto a mis nietos…", "Algún día contarán lo que vivimos aquí.", "Me pregunto cómo será esto dentro de cien años."));
                L.Add(Pick("Que no se nos olvide lo que vivimos aquí.", "Seguro que no se lo creen.", "Mejor, si lo cuidamos."));
                break;
            }
            case "historia":
            {
                string[] st = ConversationContext.Current != null ? ConversationContext.Current.story : null;
                if (st == null || st.Length == 0) return Build("santuario", a, b, shared);
                s.title = "una historia de " + Place();
                L.Add($"¿Sabes lo que cuentan? {st[Random.Range(0, st.Length)]}");
                L.Add(Pick("No lo sabía… ¿y qué pasó después?", "¿De verdad? Cuéntame más.", "Eso me lo contaron distinto."));
                L.Add(Pick("Eso lo cuentan los mayores mejor que yo.", "Hay quien dice que aún se nota.", "Cada uno lo cuenta a su manera."));
                L.Add(Pick("Habrá que preguntarles.", "Qué historia.", "Me la guardo."));
                break;
            }
            case "hambre":
                s.title = "el hambre"; s.positive = false;
                L.Add(Pick("Tengo un hambre que no veo.", "Me ruge la tripa.", "¿Falta mucho para comer?"));
                L.Add(Pick("Yo también; aún queda para la comida.", "Aguanta, que ya falta poco.", "Y yo sin nada a mano."));
                L.Add(Pick("Pues no sé si aguanto.", "Me comería cualquier cosa.", "A la próxima traigo algo."));
                L.Add(Pick("Aguanta, que ya falta poco.", "Luego nos desquitamos.", "Ánimo."));
                break;
            case "cansancio":
                s.title = "el cansancio"; s.positive = false;
                L.Add(Pick("Hoy no puedo con mi alma.", "Estoy que me caigo.", "Qué día más largo."));
                L.Add(Pick("Se te nota. ¿Has dormido algo?", "Respira un poco.", "Yo también lo noto."));
                L.Add(Pick("Poco y mal.", "No mucho.", "Algo, pero no basta."));
                L.Add(Pick("Esta noche, a la cama temprano.", "Descansa en cuanto puedas.", "Mañana irá mejor."));
                break;
            case "pensamiento":
            {
                Mind mind = a.GetComponent<Mind>();
                string t = mind != null ? mind.SpeakNow() : "";
                if (string.IsNullOrEmpty(t)) return Build("santuario", a, b, shared);
                s.title = "lo que piensa"; s.positive = mind.lastPositive && !bad;
                L.Add($"Llevo un rato pensando: «{t}».");
                L.Add(Pick("¿Y eso?", "¿Por qué lo dices?", "Cuéntame."));
                L.Add(Pick("No sé, me vino así.", "Cosas que se le ocurren a una.", "Me lo pide el cuerpo."));
                L.Add(Pick("A veces las ideas llegan solas.", "Pues dale una vuelta.", "Me has dejado pensando a mí también."));
                break;
            }
            default:   // "santuario": el sitio de hoy
                s.topic = "santuario"; s.title = "el santuario";
                L.Add(Pick("¿Has visto cómo brilla el hielo hoy?", "Hoy el mar está tranquilo.", "Cada día hay más vida por aquí."));
                L.Add(Pick("Sí, y el agua del tronco está tranquila.", "Da gusto verlo.", "Y eso que hace frío."));
                L.Add(Pick("Este es un buen sitio para vivir.", "Me alegro de estar aquí.", "Quién lo iba a decir."));
                L.Add(Pick("Lo es, si lo cuidamos entre todos.", "Yo también.", "Entre todos lo hacemos."));
                break;
        }
        return s;
    }

    // ── Datos del contexto ─────────────────────────────────────────────────────────────────

    /// <summary>Lo que está haciendo en su trabajo («Cocinando: sopa») o su papel; null si nada.</summary>
    public static string WorkOf(Anima a)
    {
        if (a == null) return null;
        if (a.TryGetComponent(out MicroJornada mj))
        {
            if (!string.IsNullOrEmpty(mj.Doing)) return mj.Doing;
            if (!string.IsNullOrEmpty(mj.role)) return "mi oficio de " + mj.role;
        }
        if (a.TryGetComponent(out Conversable cv) && !string.IsNullOrEmpty(cv.role)) return "mi oficio de " + cv.role;
        if (a.TryGetComponent(out WorldLabel wl) && wl.text != null)
        {
            int p = wl.text.IndexOf("\n<"), q = wl.text.LastIndexOf('>');
            if (p >= 0 && q > p + 2) return wl.text.Substring(p + 2, q - p - 2);   // el estado que pone la jornada del santuario
        }
        return null;
    }

    static string Leisure(Anima a)
    {
        string s = LeisureDirector.StatusOf(a.gameObject);
        if (string.IsNullOrEmpty(s) || !s.StartsWith("Libre: ")) return null;
        s = s.Substring(7);
        if (s.StartsWith("va a «")) { int e = s.IndexOf('»'); s = e > 6 ? s.Substring(6, e - 6) : s; }
        return s;
    }

    /// <summary>Qué es <paramref name="a"/> de <paramref name="b"/>: «pareja», «madre»/«padre» (a lo es de b), «hijo» (a es hijo de b) o null.</summary>
    static string Related(Anima a, Anima b)
    {
        PersonLife pa = a.GetComponent<PersonLife>(), pb = b.GetComponent<PersonLife>();
        if (pa == null || pb == null) return null;
        if (pa.partner == pb || pb.partner == pa) return "pareja";
        if (pb.mother == pa) return "madre";
        if (pb.father == pa) return "padre";
        if (pa.mother == pb || pa.father == pb) return "hijo";
        return null;
    }

    static bool HasFamily(Anima a) => a.TryGetComponent(out PersonLife p) && (p.partner != null || p.children > 0 || p.pregnant || p.mother != null);

    static string FamilyLine(Anima a)
    {
        if (!a.TryGetComponent(out PersonLife p)) return null;
        if (p.pregnant) return "Estoy esperando un bebé.";
        if (p.children > 0) return $"Tengo {Kids(p.children)}; crecen muy deprisa.";
        if (p.partner != null) return $"Mi pareja, {ConversationDirector.Short(p.partner.GetComponent<Anima>())}, anda muy liada estos días.";
        if (p.mother != null) return $"Mi madre, {ConversationDirector.Short(p.mother.GetComponent<Anima>())}, siempre me cuida.";
        return null;
    }

    static string Kids(int n) => n == 1 ? "un hijo" : $"{n} hijos";

    static string Intro(Anima a)
    {
        string w = WorkOf(a);
        if (w != null) return $"Ahora mismo, {Lower(w)}.";
        if (ConversationDirector.IsInsect(a)) return $"Vengo de un linaje de {Es(Species(a))}.";
        return Pick("Vivo aquí cerca.", "Soy nuevo por aquí.", "Me gusta conocer gente.");
    }

    static string Species(Anima a) => a.TryGetComponent(out FolkLineage fl) ? fl.species : null;

    static string Ancestry(Anima a)
    {
        if (!a.TryGetComponent(out FolkLineage fl)) return "Mi familia es de aquí de siempre.";
        string mother = null, grand = null;
        foreach (InsectGenealogy.Ancestor x in fl.ancestors)
        {
            if (x.depth == 1 && mother == null && (x.name == "madre" || x.name == "padre")) mother = $"mi {x.name} era {Es(x.species)}";
            if (x.depth == 2 && grand == null) grand = $"mi {x.name} {Es(x.species)}";
        }
        string me = $"Soy {Es(fl.species)}";
        if (mother != null && grand != null) return $"{me}; {mother} y {grand}.";
        if (mother != null) return $"{me}; {mother}.";
        return $"{me}, como los míos.";
    }

    static string Place()
    {
        if (ConversationContext.Current != null && !string.IsNullOrEmpty(ConversationContext.Current.place)) return ConversationContext.Current.place;
        if (MicroEra.Current != null && !string.IsNullOrEmpty(MicroEra.Current.place)) return MicroEra.Current.place;
        return "el santuario";
    }

    static string EraText()
    {
        if (ConversationContext.Current != null && !string.IsNullOrEmpty(ConversationContext.Current.era)) return ConversationContext.Current.era;
        if (MicroEra.Current != null) return string.IsNullOrEmpty(MicroEra.Current.date) ? "en " + Eras.Format(MicroEra.Current.year) : MicroEra.Current.date;
        return "";
    }

    static readonly Dictionary<string, string> SpeciesEs = new Dictionary<string, string>
    {
        { "Ant", "hormiga" }, { "Ladybug", "mariquita" }, { "Grasshopper", "saltamontes" }, { "Cricket", "grillo" }, { "Termite", "termita" },
        { "SnowBee", "abeja de las nieves" }, { "Bee", "abeja" }, { "Firefly", "luciérnaga" }, { "Cicada", "cigarra" }, { "Earwig", "tijereta" },
        { "Spider", "araña" }, { "DungBeetle", "escarabajo pelotero" }, { "Mantis", "mantis" }, { "Centipede", "ciempiés" }, { "Aphid", "pulgón" },
        { "Butterfly", "mariposa" }, { "Moth", "polilla" }, { "Dragonfly", "libélula" }, { "Wasp", "avispa" }, { "Antlion", "hormiga león" },
    };

    /// <summary>El nombre en castellano de una especie de insecto (o el suyo si no está en la tabla).</summary>
    public static string Es(string species) => string.IsNullOrEmpty(species) ? "insecto" : SpeciesEs.TryGetValue(species, out string es) ? es : species.ToLowerInvariant();

    static string Lower(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);
    static string Pick(params string[] o) => o[Random.Range(0, o.Length)];
}
