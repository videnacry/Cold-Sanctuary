using System.Collections;

/// <summary>
/// Una unidad de test que el <see cref="ColdSanctuary.TestRunner"/> ejecuta de forma coordinada (docs/testing-checklist.md §32).
/// Los sandboxes `*_AUTO` la implementan en vez de auto-correr en su `Start` — así el runner controla el ORDEN y evita
/// que se pisen (p.ej. la rejilla es un singleton; dos tests mutando la misma fauna).
/// </summary>
public interface ITestUnit
{
    /// <summary>Índice de GRUPO. Los grupos se corren en SERIE, de menor a mayor. (El array-de-arrays del usuario.)</summary>
    int Group { get; }

    /// <summary>¿Se puede correr EN PARALELO con otras unidades de su mismo grupo? Default recomendado: false —
    /// solo true para tests independientes/solo-lectura (la mayoría muta estado compartido: fauna, el singleton).</summary>
    bool ParallelSafe { get; }

    /// <summary>El cuerpo del test (antes era `Start`). Asevera por `TestProbe`.</summary>
    IEnumerator Run();
}
