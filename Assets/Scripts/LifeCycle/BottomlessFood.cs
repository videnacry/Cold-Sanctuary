using UnityEngine;

/// <summary>
/// CUENCO QUE NUNCA SE ACABA (escenas de ciclo de vida, Berón 2026-11-01: «un cuenco con toneladas de su alimento favorito
/// para que nunca se acabe»). Rellena, una vez por segundo, la fuente de comida que lleva al lado:
///   · <see cref="FoodItem"/> (la ración de carne del oso o del malamute): vuelve a sus gramos iniciales;
///   · <see cref="Swarm"/> (el banco de peces/krill de la foca, la ballena, la orca, el pingüino): vuelve a su tope de peces;
///   · <see cref="Hive"/> (la colonia de las abejas de las nieves): miel en reserva de sobra.
/// Así lo que se observa es el ciclo de vida, no si la comida alcanza. Cuenta lo que se ha comido (para el panel).
/// </summary>
public class BottomlessFood : MonoBehaviour
{
    [Tooltip("Gramos de la ración que se mantienen (toneladas: nunca se acaba).")]
    public float grams = 2000000f;
    [Tooltip("Banco de peces: no deriva más allá de este radio de su sitio (es «el cuenco» del que comen los marinos).")]
    public float leash = 6f;
    [Tooltip("Colmena: miel en reserva que se mantiene (gramos).")]
    public float honey = 500f;

    /// <summary>Lo comido desde el Play (gramos de la ración, peces del banco o miel).</summary>
    public float eaten { get; private set; }

    FoodItem _food; Swarm _swarm; Hive _hive;
    Vector3 _home;
    float _next;

    void Start()
    {
        _food = GetComponent<FoodItem>(); _swarm = GetComponent<Swarm>(); _hive = GetComponentInChildren<Hive>();
        if (_food != null) _food.grams = grams;
        _home = transform.position;
    }

    void Update()
    {
        if (Time.time < _next) return;
        _next = Time.time + 1f;
        if (_food != null) { if (_food.grams < grams) { eaten += grams - _food.grams; _food.grams = grams; } }
        if (_swarm != null)
        {
            if (_swarm.count < _swarm.maxCount) { eaten += _swarm.maxCount - _swarm.count; _swarm.count = _swarm.maxCount; }
            Vector3 d = _swarm.transform.position - _home; d.y = 0f;
            if (d.magnitude > leash) _swarm.transform.position = new Vector3(_home.x, _swarm.transform.position.y, _home.z) + d.normalized * leash;
        }
        if (_hive == null) _hive = GetComponentInChildren<Hive>();
        if (_hive != null && _hive.honeyStock < honey) { _hive.AddHoney(honey - _hive.honeyStock); }
    }
}
