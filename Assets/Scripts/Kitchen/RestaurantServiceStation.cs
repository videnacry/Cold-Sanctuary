using UnityEngine;

/// <summary>Puesto del servicio del restaurante virtual: el FOGÓN (cocinar el pedido) o la CAMPANA del pase (llamar por número).</summary>
[RequireComponent(typeof(Collider))]
public class RestaurantServiceStation : MonoBehaviour, IInteractable
{
    public RestaurantServiceGame game;
    public bool bell;   // false = fogón · true = campana

    public string InteractLabel => game == null ? "" : bell ? game.CallLabel : game.CookLabel;
    public bool CanInteract => game != null && (bell ? game.CanCall : game.CanCook);
    public void Interact() { if (bell) game.Call(); else game.Cook(); }
}
