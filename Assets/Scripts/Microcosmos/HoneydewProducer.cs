using UnityEngine;

/// <summary>
/// El PULGÓN produce **melaza** (honeydew) cada cierto tiempo — el "líquido codiciado" por el que las
/// hormigas lo cuidan y ordeñan (**mirmecofilia**, docs/microcosmos-insects.md §2). Es el análogo de la
/// **cría/ganadería** a escala insecto.
///
/// Si <see cref="dropPickup"/> está asignado, spawna un <see cref="HoneydewPickup"/> en el suelo cerca
/// del pulgón (el jugador lo recoge activando el trigger). Sin prefab, solo incrementa el contador.
///
/// El total producido es limitado (<see cref="maxTotal"/>), lo que fuerza al jugador a planificar —
/// hay más hormigas viejas de las que la maleza puede curar, así que deberá usar "Jalar".
/// </summary>
public class HoneydewProducer : MonoBehaviour
{
    [Min(0.2f)] public float interval = 3f;

    [Tooltip("Máximo de gotas que Ambrosio produce en total (recurso limitado). 0 = ilimitado.")]
    [Min(0)] public int maxTotal = 5;

    [Tooltip("Prefab de HoneydewPickup (esfera pequeña) que se instantia en el suelo. " +
             "Si es null, solo incrementa el contador (modo contador legacy).")]
    public GameObject dropPickup;

    [Tooltip("Radio aleatorio de dispersión del drop alrededor del pulgón.")]
    [Min(0f)] public float dropRadius = 0.8f;

    public int honeydew;

    float _next;

    void Update()
    {
        if (maxTotal > 0 && honeydew >= maxTotal) return;
        if (Time.time < _next) return;
        _next = Time.time + interval;
        honeydew++;
        Debug.Log($"[Micro] «{name}» (pulgón) produce melaza ({honeydew}" +
                  (maxTotal > 0 ? $"/{maxTotal}" : "") + "). Las hormigas la ansían.");

        if (dropPickup != null)
        {
            // Posición: ligeramente por encima del suelo, dispersa alrededor del pulgón.
            Vector2 rnd = Random.insideUnitCircle * dropRadius;
            Vector3 pos = transform.position + new Vector3(rnd.x, 0.1f, rnd.y);
            Instantiate(dropPickup, pos, Quaternion.identity);
        }
    }
}
