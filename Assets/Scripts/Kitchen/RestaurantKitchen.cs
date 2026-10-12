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

    bool _rewardedThisBalance;
    readonly List<CookingStation> _participants = new List<CookingStation>();
    AreaHours _hours;

    // EQUILIBRIO (docs): no hay tope; el objetivo es nivelar TODOS los platillos al que tiene MÁS stock (o al piso
    // `target` de cada uno, lo que sea mayor). El análisis = este nº; así, si uno va muy por detrás, se asignan VARIOS
    // obreros a él hasta igualar (p. ej. 4 obreros a huevos si los demás están a 24 y van de 6 en 6).
    int Equilibrium()
    {
        int eq = 0;
        foreach (DishStock d in dishes) { eq = Mathf.Max(eq, d.Have); eq = Mathf.Max(eq, d.target); }
        return eq;
    }

    /// <summary>El restaurante está abierto (de día) — de noche cierra y no se cocina. Usa `AreaHours` si está.</summary>
    public bool Open => _hours == null || _hours.IsOpen;

    void Awake()
    {
        _hours = GetComponent<AreaHours>();
        foreach (DishStock d in dishes)
            if (d.recipe == null)
                d.recipe = CookingRecipe.Preset(d.container != null ? d.container.dishName : "")
                           ?? new CookingRecipe(d.container != null ? d.container.dishName : "platillo", "prepara", "emplata");
    }

    /// <summary>Asigna a `who` el platillo MÁS por debajo del EQUILIBRIO (stock + lo ya en marcha). Varios obreros
    /// pueden recibir el MISMO platillo hasta nivelarlo. null = todo al nivel del más alto (equilibrado).</summary>
    public CookingRecipe AssignNext(CookingStation who)
    {
        if (!_participants.Contains(who)) _participants.Add(who);
        int eq = Equilibrium();
        DishStock best = null; int bestDeficit = 0;
        foreach (DishStock d in dishes)
        {
            int deficit = eq - (d.Have + d.inProgress);
            if (deficit > bestDeficit) { bestDeficit = deficit; best = d; }
        }
        if (best == null) { CheckBalanced(); return null; }   // ya está todo equilibrado
        _rewardedThisBalance = false;                          // hay trabajo → se podrá premiar el próximo equilibrio
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
        CheckBalanced();
    }

    // EQUILIBRADO: todos los platillos al nivel del más alto. Premia UNA vez por ciclo de equilibrio (se re-arma cuando
    // vuelve a haber trabajo — un comensal consume y hay que reponer → producción continua, sin "fin" fijo).
    void CheckBalanced()
    {
        if (_rewardedThisBalance) return;
        int eq = Equilibrium();
        foreach (DishStock d in dishes) if (d.Have < eq) return;
        _rewardedThisBalance = true;
        Reward();
    }

    void Reward()
    {
        Debug.Log("[Restaurante] EQUILIBRIO alcanzado: stock nivelado de todos los platillos.");
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

    public bool Balanced => _rewardedThisBalance;

    /// <summary>Vuelca el STOCK del área (lo lee `StatusSpell` al lanzarse sobre el santuario/restaurante).</summary>
    public void LogStocks()
    {
        int eq = Equilibrium();
        var sb = new System.Text.StringBuilder($"[Status] «{name}» stock (equilibrio {eq}): ");
        foreach (DishStock d in dishes)
            sb.Append($"{(d.container != null ? d.container.dishName : d.recipe?.dishName)} {d.Have}/{eq}  ");
        Debug.Log(sb.ToString());
    }
}
