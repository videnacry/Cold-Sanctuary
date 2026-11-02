using UnityEngine;

/// <summary>
/// EL PISO DEL MICROCOSMOS (docs/reglas-plataformas.md, regla 11; Berón 2026-10-29): el mapa de un microcosmos es SOLO el piso
/// elegido, ×100 (AreaMicrocosmos.AreaScale), cerrado por MUROS en su borde para que nada salga. Fuera del piso no se puebla nada (ni flora ni fauna); todo
/// —flora, fauna, personajes y estructuras de la historia— va dentro. Lo pone <c>AreaMicrocosmos</c> en la raíz del
/// microcosmos y lo leen las pruebas (<c>MicrocosmosFloorTest</c>): el rectángulo de los muros y lo que se contó al poblarlo.
/// </summary>
public class MicroFloor : MonoBehaviour
{
    [Tooltip("Centro del piso (cara de arriba del suelo), en locales de la raíz del microcosmos.")]
    public Vector3 center;
    [Tooltip("Tamaño del piso ×100 (x, z), en metros: el rectángulo interior de los muros.")]
    public Vector2 size;
    [Tooltip("Altura de los muros sobre el suelo (con tapa invisible arriba: nada vuela fuera).")]
    public float wallHeight;
    [Tooltip("Al construirlo: plantas, bichos y personajes dentro, y piezas que quedaron fuera (debe ser 0).")]
    public int floraInside, faunaInside, charactersInside, outside;
    [Tooltip("Al construirlo: caras de arriba de muebles gigantes pobladas (camas, encimeras, mesas, máquinas…), y las plantas y bichos que se pusieron encima.")]
    public int furnitureTops, floraOnTops, faunaOnTops;
    [Tooltip("Era avanzada con su pueblo en el piso (regla 5 de docs/microcosmos-reglas-edificacion.md): la flora es a escala de pueblo y no pisa calles ni solares.")]
    public bool town;

    [Tooltip("El BARRIO de la historia (×100, 2026-11-02): el piso real × 10 alrededor de la plaza de la historia, en locales de la raíz; ahí van la historia, su pueblo y la flora densa.")]
    public Vector3 districtCenter;
    public Vector2 districtSize;

    /// <summary>Nombres de los cuatro muros (bajo «Muros_del_piso»).</summary>
    public static readonly string[] WallNames = { "Muro_N", "Muro_S", "Muro_E", "Muro_O" };

    /// <summary>El rectángulo interior de los muros, en el mundo (la raíz no gira: solo se traslada).</summary>
    public Bounds WorldRect()
    {
        Vector3 c = transform.TransformPoint(center);
        return new Bounds(c, new Vector3(size.x, 0f, size.y));
    }

    /// <summary>¿Las cajas <paramref name="b"/> caben en el rectángulo de los muros (en XZ, con <paramref name="tol"/> m de holgura)?</summary>
    public bool Inside(Bounds b, float tol = 0.5f)
    {
        Bounds r = WorldRect();
        return b.min.x >= r.min.x - tol && b.max.x <= r.max.x + tol && b.min.z >= r.min.z - tol && b.max.z <= r.max.z + tol;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.5f, 0.2f);
        Bounds r = WorldRect();
        Gizmos.DrawWireCube(r.center + Vector3.up * wallHeight * 0.5f, new Vector3(size.x, wallHeight, size.y));
    }
}
