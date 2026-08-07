using UnityEngine;

/// <summary>
/// SUSTRATO DE QUARKS del S4 (docs/magic-metabolism-progression.md §16). En el nivel más profundo el mago ya no
/// guarda "tantos gramos de cada elemento" sino **quarks crudos** (materia de nucleones) y los **transforma en el
/// elemento que use** — los quarks sirven para **TODAS** las pools (biológica, de elementos y, desintegrados, de
/// energía). Así usar cualquier hechizo gasta, en el fondo, quarks. Física real: cada nucleón (protón/neutrón) =
/// **3 quarks**; los electrones son **leptones** (no quarks). Como 1 mol de nucleones ≈ 1 g, sale una identidad
/// **independiente del elemento**:
///   <c>1 g de materia ≈ 3 · N_A = 1,807×10²⁴ quarks</c>.
/// Por eso el coste en quarks de "crear" un elemento depende solo de los **gramos**, no de cuál sea.
///
/// EL MIX (recomendado): el jugador puede **pre-crear** elementos (rápidos de usar; ventaja de velocidad) y dejar
/// **quarks sin transformar** para conversión **dinámica al lanzar** (más lento: crear el elemento en el momento).
/// `MakeElement` es la primitiva de ambos. Opt-in; se cablea junto a `MagicReserves`.
/// </summary>
public class QuarkReserve : MonoBehaviour
{
    [Tooltip("Quarks crudos disponibles (materia de nucleones). Astronómico: 1 g ≈ 1,8e24 quarks.")]
    public double quarks = 0;

    /// <summary>3 quarks por nucleón × Avogadro (nucleones por gramo ≈ N_A).</summary>
    public const double QuarksPerGram = 3.0 * 6.02214076e23;   // 1,8067×10²⁴
    /// <summary>c² en J/g (E=mc²): 1 g = 8,99×10¹³ J.</summary>
    public const double JoulesPerGram = 8.98755e13;

    public double GramsAvailable => quarks / QuarksPerGram;

    /// <summary>Añade quarks crudos (p.ej. al consumir/desintegrar materia en S4).</summary>
    public void AddQuarks(double n) { if (n > 0) quarks += n; }
    /// <summary>Añade los quarks equivalentes a una masa (g) de materia.</summary>
    public void AddGrams(double grams) { if (grams > 0) quarks += grams * QuarksPerGram; }

    /// <summary>Nº aproximado de ÁTOMOS de un elemento que se podrían formar — para la UI ("el número sobre el
    /// elemento"). Aproxima el número másico A ≈ 2,1·Z (el catálogo solo tiene Z); el coste en gramos NO depende
    /// de esto, solo este display de átomos.</summary>
    public double AtomsAvailable(string symbol)
    {
        double a = Mathf.Max(1, AtomicNumber(symbol)) * 2.1;   // número másico aproximado
        return GramsAvailable * (6.02214076e23 / a);           // gramos × (N_A / masa molar)
    }

    /// <summary>Transforma quarks → GRAMOS de un elemento y los deposita en la pool de elementos (`Store`).
    /// Devuelve los gramos realmente creados (limitado por los quarks y por el tope de la pool). Base del MIX:
    /// pre-crear (por adelantado) o crear al vuelo al lanzar.</summary>
    public float MakeElement(MagicReserves into, string symbol, float grams)
    {
        if (into == null || grams <= 0f) return 0f;
        double affordable = System.Math.Min(grams, GramsAvailable);
        if (affordable <= 0) return 0f;
        float overflow = into.Store(symbol, (float)affordable);   // lo que no cupo NO se transforma
        float made = (float)affordable - overflow;
        if (made > 0f) quarks -= made * QuarksPerGram;            // solo se gastan quarks por lo creado
        return made;
    }

    /// <summary>RESTITUCIÓN: desintegra quarks (materia) → ENERGÍA (E=mc²) y rellena el pool de energía. Devuelve
    /// los julios añadidos. (En la cocina la energía viene de SEPARAR materia; esto es el hechizo que la repone a
    /// demanda a partir de los propios quarks.)</summary>
    public double Restitute(MagicReserves into, double grams)
    {
        if (into == null || grams <= 0) return 0;
        double affordable = System.Math.Min(grams, GramsAvailable);
        double joules = affordable * JoulesPerGram;
        float overflowJ = into.StoreEnergy((float)joules);
        double added = joules - overflowJ;
        if (added > 0) quarks -= (added / JoulesPerGram) * QuarksPerGram;   // gasta solo la materia que sí cupo
        return added;
    }

    static int AtomicNumber(string symbol)
    {
        PeriodicTableManager mgr = PeriodicTableManager.Instance;
        if (mgr != null) { ElementData d = mgr.GetData(symbol); if (d != null) return d.atomicNumber; }
        return 6;   // por defecto ~carbono
    }
}
