using UnityEngine;

/// <summary>
/// Portal de la SALA DE ERAS (escena de validación): al cruzarlo, el jugador entra en el microcosmos <see cref="scene"/>
/// (MobWorldLoader, como desde el tanque); su portal de salida lo devuelve aquí. Con su cartel de año, lugar e historia.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EraPortal : MonoBehaviour
{
    public string scene;
    public string label;

    void Start() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || string.IsNullOrEmpty(scene)) return;
        MobWorldLoader l = MobWorldLoader.Instance;
        if (l.IsBusy || l.IsInMobWorld) return;
        l.SetReturnPoint(transform.position + transform.forward * -3f + Vector3.up * 0.2f);   // al volver, delante del portal
        Debug.Log($"[Eras] Entra en {label} ({scene}).");
        l.EnterMobWorld(scene);
    }
}
