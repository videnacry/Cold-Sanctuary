using UnityEngine;

/// <summary>
/// Contenedor de servicio (docs/kitchen-simulation.md §3/§6): donde queda la comida lista. Se **rellena**
/// mientras alguien cocina (<see cref="BreakfastCook"/>) y los personajes se acercan a **comer**
/// (<see cref="Eater"/>). En el paso C, comer aplicará los **compuestos** del platillo a los humores.
/// </summary>
public class FoodContainer : MonoBehaviour
{
    public string dishName = "desayuno";
    [Min(0)] public int rations = 0;
    [Min(1)] public int capacity = 20;

    public bool HasFood => rations > 0;

    /// <summary>Añade raciones (lo hace el cocinero al emplatar). Devuelve false si ya está lleno.</summary>
    public bool Deposit(int n = 1)
    {
        // SIN TOPE: se puede generar comida/recursos de forma infinita; lo que importa es el EQUILIBRIO entre
        // productos (lo gestiona RestaurantKitchen). `capacity` queda como referencia visual, no limita.
        rations += n;
        Debug.Log($"[Cocina] Contenedor «{name}»: +{n} {dishName} ({rations}).");
        return true;
    }

    /// <summary>Toma una ración (lo hace un comensal). Devuelve false si está vacío.</summary>
    public bool Take()
    {
        if (rations <= 0) return false;
        rations--;
        return true;
    }
}
