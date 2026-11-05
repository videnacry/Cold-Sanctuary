using UnityEngine;

/// <summary>
/// EL TERRITORIO POLAR (Berón 2026-11-05; docs/territorio-polar.md): lo que el constructor deja apuntado para el juego y la prueba (grupo 99):
/// puntos de la isla de tierra, cuevas y colonias. La isla y los casquetes son las placas de «Banquisa_Casquetes» (hielo: «Casquete»;
/// tierra: «Tierra» y «Tierra_orilla»). Cada cueva lleva dos hijos: «Boca» (fuera, en el suelo) y «Fondo» (dentro).
/// </summary>
public class Territorio : MonoBehaviour
{
    public const string Land = "Tierra", Ice = "Casquete";

    /// <summary>¿Este collider es suelo de la isla de tierra?</summary>
    public static bool IsLand(Collider c) => c != null && c.name.StartsWith(Land);

    [Tooltip("Puntos repartidos por la isla de tierra (uno por lóbulo): la prueba comprueba que se anda de uno a otro.")]
    public Vector3[] landPoints = new Vector3[0];
    [Tooltip("Las cuevas (hielo y roca), con sus hijos «Boca» y «Fondo».")]
    public Transform[] caves = new Transform[0];
    public Vector3 emperorColony, walrusBeach;
    [Tooltip("Metros cuadrados de cada suelo al construir (lo que se comprueba es que quede en torno a mitad y mitad).")]
    public float landArea, iceArea;
    public int cliffs;
}
