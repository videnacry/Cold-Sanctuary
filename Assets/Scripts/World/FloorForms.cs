using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS TRES FORMAS DE CADA PISO DEL SANTUARIO (la tercera, la de MICROCOSMOS, desde 2026-11-04: <see cref="MicroName"/>,
/// docs/microcosmos-suelo-y-elementos.md) (docs/construcciones-elevadas.md §Formas del piso; Berón 2026-11-03: «todos los pisos
/// deben tener preparada su forma en construcción y su forma finalizada, y los microcosmos se crean siempre sobre la forma
/// finalizada»).
/// - FORMA FINALIZADA: lo que el piso es terminado. En un piso en obras (Dojo, Sala de Ballet: <see cref="ConstructionProject"/>)
///   es su «Acabado» (oculto hasta terminar la obra); en un piso ya terminado, el área tal cual (<see cref="finished"/> = null).
/// - FORMA EN CONSTRUCCIÓN: andamio, material del día, conos y muretes. En un piso en obras son sus piezas de obra
///   («Andamio_obra», «Material_del_dia», «Cono»); en un piso terminado, una «Forma_en_obra» oculta que se ve si el piso se
///   reforma (<see cref="ShowConstruction"/>).
/// Los microcosmos (AreaMicrocosmos.Build) llaman a <see cref="ShowFinished"/> sobre su copia gigante del piso ANTES de nada más:
/// siempre se montan encima de la forma finalizada, y lo apuntan en <see cref="MicroFloor.finishedForm"/>.
/// </summary>
public class FloorForms : MonoBehaviour
{
    [Tooltip("La forma finalizada (el «Acabado» de una obra); null = el área tal cual ya es la finalizada.")]
    public Transform finished;
    [Tooltip("La forma en construcción preparada (oculta en un piso terminado: «Forma_en_obra»).")]
    public Transform construction;
    [Tooltip("¿El santuario lo enseña hoy EN OBRAS? (Dojo y Ballet, hasta que su ConstructionProject termina).")]
    public bool underConstruction;
    [Tooltip("(2026-11-04) La TERCERA forma, la de MICROCOSMOS («Forma_microcosmos», oculta): el inventario del piso con la versión de microcosmos de cada pieza (MicroFloorForm). Los microcosmos se montan desde ella.")]
    public Transform microcosm;

    /// <summary>Nombres de las piezas de OBRA (la forma en construcción) que cuelgan del área.</summary>
    public static readonly string[] ObraNames = { "Andamio_obra", "Material_del_dia", "Cono", "Forma_en_obra" };
    public const string FinishedName = "Acabado", ConstructionName = "Forma_en_obra";
    /// <summary>(2026-11-04) LA TERCERA FORMA, la de MICROCOSMOS: en el piso del santuario, oculta, con su inventario
    /// (<see cref="MicroFloorForm"/>: cada pieza con su versión de mesocosmos y de microcosmos); en el microcosmos, la versión
    /// detallada de cada pieza (<see cref="MicroElement"/>, la viste MicroFloorDress) junto a la copia gigante del piso.</summary>
    public const string MicroName = "Forma_microcosmos";

    public static bool IsObra(Transform t)
    {
        foreach (string n in ObraNames) if (t.name == n) return true;
        return false;
    }

    /// <summary>Deja <paramref name="area"/> en su FORMA FINALIZADA: enciende su «Acabado» (si lo tiene) y quita (o apaga, si
    /// <paramref name="destroy"/> es false) las piezas de obra, en todo el subárbol. Devuelve cuántas piezas de obra quitó.</summary>
    public static int ShowFinished(Transform area, bool destroy = true)
    {
        if (area == null) return 0;
        var obra = new List<Transform>();
        foreach (Transform t in area.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t == area) continue;
            if (t.name == FinishedName) t.gameObject.SetActive(true);
            else if (IsObra(t)) obra.Add(t);
        }
        int n = 0;
        foreach (Transform t in obra)
        {
            if (t == null) continue;
            if (destroy) Object.DestroyImmediate(t.gameObject); else t.gameObject.SetActive(false);
            n++;
        }
        return n;
    }

    /// <summary>¿Queda alguna pieza de obra VISIBLE bajo <paramref name="area"/> o algún «Acabado» apagado? (lo que vigilan las
    /// pruebas: un microcosmos sobre la forma en construcción).</summary>
    public static List<string> ObraLeft(Transform area)
    {
        var l = new List<string>();
        if (area == null) return l;
        foreach (Transform t in area.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t == area) continue;
            if (t.name == FinishedName && !t.gameObject.activeSelf) l.Add($"«{FinishedName}» apagado en {t.parent.name}");
            else if (IsObra(t) && t.gameObject.activeInHierarchy) l.Add($"«{t.name}» en {t.parent.name}");
        }
        return l;
    }

    /// <summary>Enseña la forma EN CONSTRUCCIÓN preparada de un piso terminado (una reforma); <see cref="ShowFinished"/> con
    /// destroy = false la vuelve a ocultar.</summary>
    public void ShowConstruction(bool on)
    {
        if (construction != null) construction.gameObject.SetActive(on);
        underConstruction = on || GetComponent<ConstructionProject>() != null && !GetComponent<ConstructionProject>().Finished;
    }
}
