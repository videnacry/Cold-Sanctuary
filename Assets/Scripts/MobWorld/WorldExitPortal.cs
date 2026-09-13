using UnityEngine;

/// <summary>
/// PORTAL DE SALIDA de un mundo interior — EL ÚNICO, REUTILIZABLE (no crees otro: añade ESTE componente al gameobject
/// que haga de salida). Devuelve al jugador al mundo normal desde cualquier mundo virtualizado: el **mob-world**
/// (sala de yoga), el **Microcosmos** (p.ej. la orilla del río en el origen de Sakshi) o el modo misión in-place.
/// La ENTRADA la hace la VirtualizationMachine / loto; esto es la vía de VUELTA. Antes se llamaba `YogaPortal`
/// (renombrado 2026-09 para que no se confunda con la sala de yoga y nadie duplique el mecanismo).
///
/// Sale de un mundo-mob-escena (MobWorldLoader) si lo hay; si no, del modo misión in-place (MeditationSession).
/// La etiqueta de interacción es configurable (<see cref="label"/>) para encajar en cada contexto (yoga/orilla/…).
/// </summary>
[RequireComponent(typeof(Collider))]
public class WorldExitPortal : MonoBehaviour, IInteractable
{
    [Tooltip("Texto de la interacción; cámbialo por contexto (sala de yoga / orilla del río / …).")]
    public string label = "Salir al mundo exterior";

    public string InteractLabel => label;

    // Sale de un mundo-mob-escena (MobWorldLoader) si lo hay; si no, del modo misión in-place.
    public bool CanInteract =>
        (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld && !MobWorldLoader.Instance.IsBusy) ||
        (MeditationSession.Instance.IsInMission && !MeditationSession.Instance.IsBusy);

    public void Interact()
    {
        if (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsInMobWorld)
        {
            MobWorldLoader.Instance.ExitMobWorld();
            return;
        }
        if (MeditationSession.Instance.IsInMission && !MeditationSession.Instance.IsBusy)
            MeditationSession.Instance.EndMission();
    }

    void Start()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger) col.isTrigger = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.75f, 0.65f, 0.85f, 0.35f);
        var col = GetComponent<Collider>();
        if (col != null) Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}
