using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// POSICIONES RESERVADAS DE UN PISO (docs/reglas-plataformas.md, regla 8; Berón 2026-10-28): cada piso crea de antemano sus
/// plazas para lo que pondrá ahí el MICROCOSMOS de su área (la plantilla, <c>MicrocosmTemplate</c>):
/// - <see cref="Kind.Historia"/>: la estructura de la historia (la guarida, la cueva, el bancal…): la primera es la principal,
///   el punto del suelo más alejado de los muebles; las demás, otros huecos grandes para estructuras de la historia;
/// - <see cref="Kind.Personaje"/>: los personajes (mirmidones de la tribu), a 3–8 m de la historia;
/// - <see cref="Kind.Fauna"/>: los insectos, junto a los bordes y las esquinas del piso;
/// - <see cref="Kind.Flora"/>: las plantas, cada 2 m por el suelo libre que queda.
/// Las calcula el constructor (<c>SampleSceneBuilder.PlaceReservedSlots</c>) con los muebles y las áreas de alfombras ya
/// puestos: una rejilla de 1 m sobre el piso; libre = a ≥ 1 m de todo mueble del piso, ≥ 1 m de las áreas de alfombras y
/// ≥ 0,8 m del borde. Van en coordenadas LOCALES del piso (a ras de su suelo): el microcosmos, que agranda el área ×10
/// (<c>AreaMicrocosmos.AreaScale</c>), las pasa a las suyas con <see cref="World(Kind, int, Transform, float)"/>.
/// Si una plataforma cambia y ya no caben, la prueba de la vida del santuario falla (<c>SanctuaryLifeTest</c>).
/// </summary>
public class PlatformSlots : MonoBehaviour
{
    public enum Kind { Historia, Personaje, Fauna, Flora }

    /// <summary>Mínimos que pide la prueba en cada área (con menos, la historia no cabe en el piso).</summary>
    public const int MinStory = 1, MinCharacters = 4, MinFauna = 4, MinFlora = 12;

    [Tooltip("Estructuras de la historia: [0] la principal (el hueco más libre), luego otros huecos grandes.")]
    public List<Vector3> story = new List<Vector3>();
    [Tooltip("Personajes, a 3–8 m de la historia.")]
    public List<Vector3> characters = new List<Vector3>();
    [Tooltip("Fauna, junto a los bordes y las esquinas.")]
    public List<Vector3> fauna = new List<Vector3>();
    [Tooltip("Flora, cada 2 m por el suelo libre.")]
    public List<Vector3> flora = new List<Vector3>();
    [Tooltip("Tamaño del piso (m) con el que se calcularon: si el piso cambia, hay que recalcularlas.")]
    public Vector2 floorSize;
    [Tooltip("En la copia del área gigante del microcosmos: las plazas de flora que tapa la historia (su claro).")]
    public List<int> floraUnderStory = new List<int>();
    [Tooltip("Celdas libres (de 1 m) que quedaron en el piso al calcularlas.")]
    public int freeCells;
    [Tooltip("Hueco libre (m) alrededor de la plaza principal de la historia.")]
    public float storyClearance;

    // Lo que ya se ha tomado de cada tipo (Take), en esta sesión.
    readonly int[] _taken = new int[4];

    /// <summary>Las plazas de un tipo (locales al piso).</summary>
    public List<Vector3> All(Kind k)
    {
        switch (k)
        {
            case Kind.Historia: return story;
            case Kind.Personaje: return characters;
            case Kind.Fauna: return fauna;
            default: return flora;
        }
    }

    public int Count(Kind k) => All(k).Count;

    /// <summary>La siguiente plaza libre de ese tipo (local), o null si ya se tomaron todas.</summary>
    public Vector3? Take(Kind k)
    {
        List<Vector3> l = All(k);
        int i = _taken[(int)k];
        if (i >= l.Count) return null;
        _taken[(int)k] = i + 1;
        return l[i];
    }

    /// <summary>Vuelve a dar las plazas desde la primera.</summary>
    public void ResetTaken() { for (int i = 0; i < _taken.Length; i++) _taken[i] = 0; }

    /// <summary>La plaza <paramref name="i"/> en el mundo, sobre este piso.</summary>
    public Vector3 World(Kind k, int i) => transform.TransformPoint(All(k)[i]);

    /// <summary>Una plaza local en el espacio de <paramref name="frame"/> agrandada <paramref name="scale"/> veces (el microcosmos:
    /// el área clonada ×10 en el origen de su raíz, sin girar).</summary>
    public static Vector3 World(Vector3 local, Transform frame, float scale) => frame != null ? frame.TransformPoint(local * scale) : local * scale;

    /// <summary>La plaza <paramref name="i"/> de ese tipo en el espacio de <paramref name="frame"/>, agrandada.</summary>
    public Vector3 World(Kind k, int i, Transform frame, float scale) => World(All(k)[i], frame, scale);

    /// <summary>Resumen para el registro y las pruebas.</summary>
    public string Summary() => $"historia {story.Count}, personajes {characters.Count}, fauna {fauna.Count}, flora {flora.Count} ({freeCells} m² libres)";

    void OnDrawGizmosSelected()
    {
        Color[] c = { new Color(0.9f, 0.3f, 0.9f), new Color(0.2f, 0.6f, 1f), new Color(1f, 0.6f, 0.1f), new Color(0.3f, 0.85f, 0.3f) };
        for (int k = 0; k < 4; k++)
        {
            Gizmos.color = c[k];
            foreach (Vector3 p in All((Kind)k)) Gizmos.DrawWireSphere(transform.TransformPoint(p), k == 0 ? 0.6f : 0.3f);
        }
    }
}
