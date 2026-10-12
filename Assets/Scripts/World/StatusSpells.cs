using UnityEngine;

/// <summary>
/// HECHIZO DE STATUS (docs/typed-spells-and-queues): las UIs no están siempre visibles — se **invocan con un hechizo**.
/// Lanzado sobre uno mismo/otra anima → muestra su **HUD de estado** (`AnimaStatusHUD.Show`). Lanzado sobre el
/// **santuario/restaurante** (un área con `RestaurantKitchen`) → vuelca sus **stocks**. `targetObject` lo fija la
/// consola (`status({destiny:"self"})` / `status({destiny:"Restaurante_AUTO"})`); por eso puede apuntar a cosas que no
/// son `ITarget` (un área). Linduarda lanza un status-global para saber a quién falta vacunar/examinar.
/// </summary>
public class StatusSpell : SpellBase
{
    public GameObject targetObject;   // anima o área; si null, el caster

    public override bool CanCast(Anima caster, ITarget target) => true;

    public override void Cast(Anima caster, ITarget target)
    {
        GameObject go = targetObject != null ? targetObject : (caster != null ? caster.gameObject : null);
        if (go == null) return;
        Anima a = go.GetComponentInParent<Anima>();
        if (a != null) { AnimaStatusHUD.Show(a); Debug.Log($"[Status] estado de «{a.name}»."); return; }
        RestaurantKitchen k = go.GetComponentInChildren<RestaurantKitchen>() ?? Object.FindObjectOfType<RestaurantKitchen>();
        if (k != null) k.LogStocks();
    }
}

/// <summary>
/// HECHIZO DE ABRIR/CERRAR un área (docs): el mismo hechizo abre o cierra según la property `close` que se le pasa.
/// Fija el override manual de `AreaHours`. `open({destiny:"Restaurante_AUTO"})` abre; `open({destiny:"...", close:true})`
/// cierra. (Volver a `auto`/reloj = `reset:true`.)
/// </summary>
public class OpenSpell : SpellBase
{
    public GameObject targetObject;
    public bool close = false;
    public bool reset = false;

    public override bool CanCast(Anima caster, ITarget target) => true;

    public override void Cast(Anima caster, ITarget target)
    {
        GameObject go = targetObject != null ? targetObject : (caster != null ? caster.gameObject : null);
        AreaHours h = go != null ? go.GetComponentInChildren<AreaHours>() : null;
        if (h == null) { Debug.Log("[Área] sin AreaHours en el objetivo."); return; }
        h.manualOverride = reset ? 0 : (close ? -1 : 1);
        Debug.Log($"[Área] «{go.name}»: {(reset ? "auto (reloj)" : close ? "cerrada" : "abierta")}.");
    }
}
