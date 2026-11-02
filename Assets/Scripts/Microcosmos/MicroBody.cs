using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL CUERPO DE QUIEN USA UN MUEBLE (docs/microcosmos-reglas-edificacion.md §Muebles adaptados al cuerpo; Berón 2026-11-02: «las
/// camas y demás servicios deben estar adaptados para los usuarios, en este caso las de humanos-insectos no me han dado la
/// impresión de que estén adaptadas»). Los insectos-humanos (<c>CubeInsect.DressFolk</c>) son CENTAUROS: el torso se yergue a la
/// estatura de una persona, pero el abdomen (gáster, élitros, opistosoma) sigue horizontal detrás, sobre 4–6 patas. No caben en
/// una cama humana tumbados de espaldas ni en una silla con respaldo. <see cref="Fit"/> mide el cuerpo de cada uno con los datos de
/// su especie (<see cref="InsectMorphology"/>) a su escala real, y su <see cref="Plan"/> decide la FORMA del mueble; el estilo de
/// época y lugar (<see cref="MicroStyles"/>) decide sus materiales y sus detalles.
/// </summary>
public static class MicroBody
{
    /// <summary>La forma de descansar y de sentarse de cada cuerpo.</summary>
    public enum Plan
    {
        Humano,     // de espaldas en la cama, sentado en silla
        Centauro,   // hormiga, termita, tijereta, grillo, saltamontes: abdomen largo detrás; duerme de vientre, se sienta a horcajadas
        Cupula,     // mariquita, pelotero: élitros en cúpula, ancho; duerme de vientre en un nido con borde, asiento bajo y ancho
        Alado,      // abeja, luciérnaga, cigarra: duerme agarrado a una percha (las abejas duermen colgadas de las mandíbulas)
        Mantis,     // cuatro brazos y dos patas: descansa colgada de una barra (como cuelgan las mantis en la muda)
        Arana,      // ocho patas abiertas y opistosoma grande: hamaca de seda; asiento de anillo
        Ciempies,   // cuerpo muy largo y bajo: estera larga y banco corrido
    }

    /// <summary>Cómo se tumba en su cama (<see cref="MicroPiece.rest"/>): de espaldas, de vientre o agarrado a la percha.</summary>
    public const string Espalda = "espalda", Vientre = "vientre", Percha = "percha";

    /// <summary>Las medidas que usan los muebles (m).</summary>
    public struct Fit
    {
        public Plan plan;
        public string species;
        public float height;     // de pie (erguido)
        public float length;     // tumbado (de la cabeza a la punta del abdomen)
        public float width;      // el ancho del cuerpo (sin las patas abiertas)
        public float span;       // el ancho con las patas (lo que ocupa tumbado)
        public float belly;      // la altura del vientre del abdomen sobre el suelo, de pie
        public float abdomen;    // el largo del abdomen (lo que apoya el asiento)
        public float hands;      // la altura de las manos con el torso erguido (la de la mesa y el lavamanos)
        public bool Insect => plan != Plan.Humano;
        public string Rest => plan == Plan.Humano ? Espalda : plan == Plan.Alado || plan == Plan.Mantis ? Percha : Vientre;
        public override string ToString() => $"{plan} {species} ({height:0.00} m de pie, {length:0.00} × {span:0.00} m tumbado, vientre {belly:0.00}, manos {hands:0.00})";
    }

    /// <summary>El plan de cada especie de insecto-humano (<c>InsectCast.Folk</c>).</summary>
    public static Plan PlanOf(string species)
    {
        switch (species)
        {
            case null: case "": case "Human": case "Humano": return Plan.Humano;
            case "Ladybug": case "DungBeetle": return Plan.Cupula;
            case "SnowBee": case "Firefly": case "Cicada": return Plan.Alado;
            case "Mantis": return Plan.Mantis;
            case "Spider": return Plan.Arana;
            case "Centipede": return Plan.Ciempies;
            default: return Plan.Centauro;
        }
    }

    /// <summary>Una persona de <paramref name="height"/> m.</summary>
    public static Fit Human(float height = 1.75f)
        => new Fit { plan = Plan.Humano, species = "Humano", height = height, length = height + 0.2f, width = height * 0.27f, span = height * 0.3f, belly = height * 0.45f, abdomen = 0.45f, hands = height * 0.42f };

    /// <summary>
    /// El cuerpo de <paramref name="g"/>: un insecto-humano (CubeInsect folk) se mide con su ficha de especie (InsectMorphology) a
    /// la escala de su cuerpo (el largo L de su «Insecto»); las manos, donde están ahora (erguido). Si no es un insecto, una persona
    /// de su estatura.
    /// </summary>
    public static Fit Of(GameObject g)
    {
        if (g == null) return Human();
        float h = 1.75f;
        var cap = g.GetComponent<CapsuleCollider>(); if (cap != null && cap.height > 0.3f) h = cap.height;
        var ci = g.GetComponent<CubeInsect>();
        Transform ins = g.transform.Find("Insecto");
        string sp = ci != null ? ci.species : null;
        if (ci == null || ins == null || string.IsNullOrEmpty(sp)) return Human(h);
        InsectMorphology.Spec s = InsectMorphology.ForFolk(InsectMorphology.ForSex(InsectMorphology.For(sp), ci.female));
        float L = Mathf.Abs(ins.localScale.z) * Mathf.Abs(g.transform.lossyScale.z);
        var f = ForSpec(sp, s, L, h);
        // Las manos, medidas (las piezas «Mano» de los brazos, con el torso erguido).
        float hy = 0f; int n = 0;
        foreach (Renderer r in ins.GetComponentsInChildren<Renderer>(true)) if (r.name == "Mano") { hy += g.transform.InverseTransformPoint(r.bounds.center).y; n++; }
        if (n > 0) f.hands = Mathf.Clamp(hy / n + 0.15f, 0.45f * h, 0.8f * h);
        return f;
    }

    /// <summary>El cuerpo de la especie <paramref name="species"/> de <paramref name="height"/> m sin tenerlo delante (pruebas,
    /// muebles por defecto): L se estima de la proporción erguida de la ficha.</summary>
    public static Fit OfSpecies(string species, float height = 1.75f)
    {
        if (PlanOf(species) == Plan.Humano) return Human(height);
        InsectMorphology.Spec s = InsectMorphology.ForFolk(InsectMorphology.For(species));
        float total = 0f; foreach (var seg in s.body) total += seg.len;
        // Erguido mide ~ el vientre + el tórax y la cabeza levantados + la columna (0,22 L): ver CubeInsect.Build.
        float up = s.bodyHeight + 0.22f; for (int i = 0; i < Mathf.Min(2, s.body.Length); i++) up += s.body[i].len;
        return ForSpec(species, s, height / Mathf.Max(0.3f, up), height);
    }

    static Fit ForSpec(string sp, InsectMorphology.Spec s, float L, float h)
    {
        float total = 0f, wid = 0f; foreach (var seg in s.body) { total += seg.len; wid = Mathf.Max(wid, seg.wid); }
        var last = s.body[s.body.Length - 1];
        float legs = 0f; foreach (float[] leg in s.legs) { float a = 0f; foreach (float x in leg) a += x; legs = Mathf.Max(legs, a); }
        var f = new Fit { plan = PlanOf(sp), species = sp, height = h };
        f.length = Mathf.Clamp(total * L + 0.25f, 1.2f, 6f);
        f.width = Mathf.Clamp(wid * L, 0.3f, 2.2f);
        // Tumbado, las patas se recogen junto al cuerpo (las de la araña quedan abiertas: la hamaca es más ancha).
        f.span = Mathf.Clamp(f.width + (s.spider ? legs * L * 0.7f : legs * L * 0.35f), f.width + 0.2f, 3.2f);
        f.belly = Mathf.Clamp(s.bodyHeight * L, 0.05f, 0.7f);
        f.abdomen = Mathf.Clamp(last.len * L, 0.25f, 3f);
        f.hands = 0.55f * h;
        return f;
    }

    /// <summary>Un cuerpo que valga para todos (un retrete o una mesa compartidos): lo más largo, lo más ancho, las manos a su
    /// media; el plan, el del primero (o humano si hay alguno).</summary>
    public static Fit Union(IList<Fit> all)
    {
        if (all == null || all.Count == 0) return Human();
        Fit u = all[0]; float hands = 0f;
        foreach (Fit f in all)
        {
            if (f.plan == Plan.Humano) u.plan = Plan.Humano;
            u.height = Mathf.Max(u.height, f.height); u.length = Mathf.Max(u.length, f.length); u.width = Mathf.Max(u.width, f.width);
            u.span = Mathf.Max(u.span, f.span); u.belly = Mathf.Max(u.belly, f.belly); u.abdomen = Mathf.Max(u.abdomen, f.abdomen);
            hands += f.hands;
        }
        u.hands = hands / all.Count;
        return u;
    }
}
