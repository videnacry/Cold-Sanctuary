using UnityEngine;

/// <summary>
/// Bootstrapper de PRUEBA (runtime, solo para el sandbox de SampleSceneBuilder): crea `ItemData`
/// placeholder (un consumible y un artefacto) y los asigna como drops a las `PlayableCreature` de la
/// escena. Existe porque el repo es solo-código (no hay assets de ItemData versionados), así que los
/// items se generan en memoria al arrancar para poder probar el botín del farming.
///
/// Se ejecuta en `Awake` (Play mode). `PlayableCreature` solo lee sus drops al serenarse, mucho
/// después, así que el orden de Awake no importa.
/// </summary>
public class FarmingSandboxItems : MonoBehaviour
{
    void Awake()
    {
        ItemData treat = ScriptableObject.CreateInstance<ItemData>();
        treat.itemName      = "Golosina";
        treat.description   = "Un premio: sube la satisfacción al usarlo.";
        treat.itemType      = ItemType.Consumable;
        treat.restoreChannel = MindChannel.Satisfaction;
        treat.restoreAmount = 10f;
        treat.sellPrice     = 3;

        ItemData artifact = ScriptableObject.CreateInstance<ItemData>();
        artifact.itemName         = "Colmillo de guerra";
        artifact.description      = "Artefacto de guerra: aumenta el daño.";
        artifact.itemType         = ItemType.Tool;
        artifact.damageMultiplier = 1.2f;
        artifact.sellPrice        = 20;

        foreach (PlayableCreature pc in FindObjectsOfType<PlayableCreature>())
        {
            pc.itemDrops = new[]
            {
                new PlayableCreature.ItemDrop { item = treat,    quantity = 1, chance = 1f    },
                new PlayableCreature.ItemDrop { item = artifact, quantity = 1, chance = 0.35f },
            };
        }
    }
}
