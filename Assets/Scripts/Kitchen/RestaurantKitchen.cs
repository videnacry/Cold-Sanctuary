using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GESTOR del RESTAURANTE (cocina jugable, docs/kitchen-simulation.md + spell-genesis §cocina). La misión = **preparar
/// platillos para TODOS los personajes del santuario** (no se come: se cocina para VENDER). Cada platillo tiene su
/// **stock** (<see cref="FoodContainer"/>) y un objetivo de raciones; el restaurante **mantiene el equilibrio**:
/// cuando una anima entra a cocinar (<see cref="CookingStation"/>) se le asigna el platillo **más por debajo** de su
/// objetivo (contando lo ya en marcha). **Varias animas** pueden cocinar a la vez (cada una recibe un platillo
/// distinto). Al llenar el stock de todos → **misión cumplida**: XP a cada participante + **una porción de cada
/// platillo** a su inventario (y al del jugador vía <see cref="Inventory"/>).
/// </summary>
public class RestaurantKitchen : MonoBehaviour
{
    [System.Serializable]
    public class DishStock
    {
        [Tooltip("Contenedor de stock de este platillo. Su `dishName` elige el preset de receta si no se indica otra.")]
        public FoodContainer container;
        [Tooltip("Raciones objetivo = 'para todos los personajes' (el restaurante cocina hasta llegar aquí).")]
        [Min(1)] public int target = 6;
        [Tooltip("Item opcional a dar al inventario al cumplir (una porción). Vacío = solo stock, sin item.")]
        public ItemData item;

        [System.NonSerialized] public CookingRecipe recipe;
        [System.NonSerialized] public int inProgress;   // raciones que alguien está cocinando ahora (para repartir bien)

        public int Have => (container != null ? container.rations : 0);
    }

    public DishStock[] dishes = System.Array.Empty<DishStock>();
    [Tooltip("XP de Stats que recibe cada participante al cumplir la misión.")]
    [Min(0f)] public float xpOnComplete = 20f;

    bool _complete;
    readonly List<CookingStation> _participants = new List<CookingStation>();

    void Awake()
    {
        foreach (DishStock d in dishes)
            if (d.recipe == null)
                d.recipe = CookingRecipe.Preset(d.container != null ? d.container.dishName : "")
                           ?? new CookingRecipe(d.container != null ? d.container.dishName : "platillo", "prepara", "emplata");
    }

    /// <summary>Asigna a `who` el platillo MÁS por debajo de su objetivo (stock + lo ya en marcha). null = todo cubierto.</summary>
    public CookingRecipe AssignNext(CookingStation who)
    {
        if (!_participants.Contains(who)) _participants.Add(who);
        DishStock best = null; int bestDeficit = 0;
        foreach (DishStock d in dishes)
        {
            int deficit = d.target - (d.Have + d.inProgress);
            if (deficit > bestDeficit) { bestDeficit = deficit; best = d; }
        }
        if (best == null) { CheckComplete(); return null; }
        best.inProgress++;
        return best.recipe;
    }

    /// <summary>Un cocinero terminó un platillo: +1 al stock, libera el 'en marcha', y comprueba si la misión está lista.</summary>
    public void Deliver(string dishName, CookingStation who)
    {
        foreach (DishStock d in dishes)
            if (d.recipe != null && d.recipe.dishName == dishName)
            {
                d.inProgress = Mathf.Max(0, d.inProgress - 1);
                if (d.container != null) d.container.Deposit(1);
                break;
            }
        CheckComplete();
    }

    void CheckComplete()
    {
        if (_complete) return;
        foreach (DishStock d in dishes) if (d.Have < d.target) return;
        _complete = true;
        Reward();
    }

    void Reward()
    {
        Debug.Log("[Restaurante] MISIÓN CUMPLIDA: platillos preparados para todos los personajes.");
        foreach (CookingStation p in _participants)
        {
            if (p == null) continue;
            CharacterLevel cl = p.GetComponent<CharacterLevel>();
            if (cl != null) cl.GainStatsXp(xpOnComplete);          // cocinar = marga de Stats (nivel integral)
            GivePortions(p);                                       // una porción de cada platillo
        }
    }

    // A cada participante (al jugador vía Inventory.Instance): una porción de CADA platillo en su inventario.
    void GivePortions(CookingStation p)
    {
        Inventory inv = p.GetComponent<Inventory>();
        if (inv == null) inv = Inventory.Instance;
        if (inv == null) return;
        foreach (DishStock d in dishes)
            if (d.item != null) inv.AddItem(d.item, 1);
    }

    public bool Complete => _complete;
}
