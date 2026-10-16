using UnityEngine;

/// <summary>
/// «ALIMENTAR A LOS VISITANTES» (huerto; antes «Limpiar la plaga»): llegan animalitos hambrientos que van a por los
/// cultivos. El jugador protege el huerto con bondad: les da de comer (F o el hechizo «alimentar») y, saciados, se van.
/// Cuenta cada visitante alimentado; el que llega a una planta la muerde y sigue con hambre (no cuenta). Si no hay
/// cultivos cerca de la misión, siembra unos pocos para que haya algo que proteger.
/// </summary>
public class FeedVisitorMission : MeditationMissionBase
{
    [Tooltip("Radio en el que se buscan cultivos alrededor de la misión.")]
    public float cropRadius = 30f;
    [Tooltip("Escala de los cultivos que siembra si no hay (en el microcosmos se ven gigantes).")]
    public float sownCropScale = 1f;

    int _species;

    protected override string BeginMessage =>
        $"[Huerto] Llegan {targetCount} visitantes con hambre a por los cultivos: acércate y dales de comer (F o «alimentar») antes de que los muerdan.";

    protected override string CompleteMessage => "[Huerto] ✅ Todos los visitantes comieron y el huerto sigue en pie.";

    protected override MeditationMob CreateMob(Vector3 position)
    {
        EnsureCrops();
        // Que lleguen desde fuera del huerto, no encima de las plantas.
        GardenCrop c = GardenCrop.Nearest(transform.position, cropRadius);
        if (c != null)
        {
            Vector3 away = position - c.transform.position; away.y = 0f;
            if (away.magnitude < 11f) position = c.transform.position + (away.sqrMagnitude > 0.01f ? away.normalized : Vector3.forward) * 12f;
        }
        position.y = GroundY(position);
        return HungryVisitorMob.Create(position, _species++);
    }

    void EnsureCrops()
    {
        if (GardenCrop.Nearest(transform.position, cropRadius) != null) return;
        GardenCrop.Kind[] kinds = { GardenCrop.Kind.Lenteja, GardenCrop.Kind.Tomate, GardenCrop.Kind.Espinaca, GardenCrop.Kind.Maiz };
        for (int i = 0; i < kinds.Length; i++)
        {
            Vector3 p = transform.position + new Vector3((i - 1.5f) * 2f * sownCropScale, 0f, 0f);
            p.y = GroundY(p);
            GardenCrop.Build(transform, kinds[i], p, 77 + i, sownCropScale);
        }
        Debug.Log("[Huerto] No había cultivos cerca: se siembran unos pocos para protegerlos.");
    }

    static float GroundY(Vector3 p)
    {
        return Physics.Raycast(p + Vector3.up * 20f, Vector3.down, out RaycastHit hit, 60f, ~0, QueryTriggerInteraction.Ignore)
            ? hit.point.y : p.y;
    }
}
