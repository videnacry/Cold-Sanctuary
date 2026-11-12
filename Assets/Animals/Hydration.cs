using UnityEngine;

/// <summary>
/// SED Y AGUA (docs/ciclo-de-vida.md; antes «beber: no modelado, no hay sed»). Cada especie del santuario saca el agua de
/// donde la saca en la naturaleza (<see cref="LifeHistory.water"/>):
///   · <b>De la comida</b> (oso polar, foca, ballena, orca, araña de las nieves): no beben; el agua les llega con la presa y,
///     sobre todo en los mamíferos marinos y el oso, como agua METABÓLICA al quemar grasa (Ortiz 2001, J Exp Biol 204:1831:
///     los pinnípedos y cetáceos casi nunca beben). Cada comida les quita la sed.
///   · <b>De la miel</b> (abeja de las nieves): la del néctar/miel que comen en el panal.
///   · <b>Agua de mar o nieve</b> (pingüino): en el mar bebe agua salada y la glándula de la sal expulsa el exceso
///     (Schmidt-Nielsen 1960); en tierra come nieve (el emperador que incuba no tiene otra).
///   · <b>Bebe</b> (malamute): va al bebedero (<see cref="WaterSource"/>) o, sin él, come nieve como los perros de trineo.
/// La sed sube con las horas de juego; con sed, el deseo «drink» (<see cref="DesireCatalog"/>) lo lleva a beber. Una sed
/// muy larga incomoda (estrés), sin matar: en el hielo siempre hay nieve.
/// </summary>
[RequireComponent(typeof(Animal))]
public class Hydration : MonoBehaviour
{
    /// <summary>Sed: 0 recién bebido, 1 = necesita beber.</summary>
    public float thirst;
    public int drinks;
    public string lastHow = "";
    public static event System.Action<Animal, string> Drank;

    Animal _a;
    LifeHistory _life;
    float _lastHungry;

    public LifeHistory Life => _life;

    void Start()
    {
        _a = GetComponent<Animal>();
        _life = LifeHistory.Of(_a.LifeSpecies);
        _lastHungry = _a.hungry;
        thirst = Random.Range(0f, 0.5f);
    }

    /// <summary>¿Va a buscar el agua (bebedero, nieve)? Los que la sacan de la comida, no.</summary>
    public bool Seeks => _life != null && (_life.water == LifeHistory.Water.Drinks || _life.water == LifeHistory.Water.SeaOrSnow);

    void Update()
    {
        if (_a == null || _a.death || _life == null) return;
        TimeController tc = TimeController.timeController;
        float hourSecs = tc != null ? tc.SecondsPerGameDay / 24f : 150f;
        float hours = Seeks ? Mathf.Max(0.5f, _life.drinkHours) : 12f;
        thirst += Time.deltaTime / (hourSecs * hours);

        bool ate = _a.hungry < _lastHungry - 0.02f;
        _lastHungry = _a.hungry;
        switch (_life.water)
        {
            case LifeHistory.Water.FromFood: if (ate) Drink("con la comida (agua de la presa y metabólica)"); break;
            case LifeHistory.Water.FromHoney: if (ate) Drink("de la miel"); break;
            case LifeHistory.Water.SeaOrSnow:
                // En el mar bebe mientras nada (la glándula de la sal se encarga del exceso).
                if (thirst > 0.3f && transform.position.y < -0.3f) Drink("agua de mar (glándula de la sal)");
                break;
        }
        if (thirst > 3f) _a.stress = Mathf.Min(1f, _a.stress + 0.01f * Time.deltaTime);   // sed larga: incomoda
    }

    public void Drink(string how)
    {
        thirst = 0f; drinks++; lastHow = how;
        if (drinks <= 3) Debug.Log($"[Agua] «{name}» bebe: {how}.");
        Drank?.Invoke(_a, how);
    }

    /// <summary>Necesidad del deseo «drink» (0 si no busca agua o no tiene sed).</summary>
    public float Need => Seeks && thirst >= 1f ? Mathf.Min(1.5f, thirst) : 0f;
}
