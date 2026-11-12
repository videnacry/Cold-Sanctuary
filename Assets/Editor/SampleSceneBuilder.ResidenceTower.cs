using UnityEngine;

/// <summary>
/// LA RESIDENCIA, UN EDIFICIO-ÁRBOL NUEVO EN LA ISLA (Berón 2026-11-05: «la residencia se puede pasar a un arbol-edificio y las casas arbol quedan para otro santuary»; docs/residencia.md)
///.
/// </summary>
public static partial class SampleSceneBuilder
{
    /// <summary>Lóbulo Isla:Cabo: lejos de la cueva de los lobos y de la de la osa, y a ≥ 16 m de Producción y del Árbol de los Campos.</summary>
    public static readonly Vector3 ResidenciaCenter = new Vector3(-66f, 0f, -215f);
    public static readonly Vector2 ResidenciaSize = new Vector2(40f, 38f);
    public const float ResidenciaYaw = 0f;

    static void BuildResidenceTower(Transform areasGroup)
    {
        int roomFloors = ResidentialBuilder.RoomFloors;
        int levels = Mathf.CeilToInt((roomFloors + 1) / 2f);
        Helix hx = BeginTreeTower(areasGroup, ResidentialBuilder.Name, ResidenciaCenter, ResidenciaSize, ResidenciaYaw, levels, 0f, 4f);
        hx.santuarioLabel = $"{ResidentialBuilder.Name}: habitaciones, plaza y ring de almohadas";
        hx.floorTitle = k => "Residencia";
        Transform plaza = new GameObject(ResidentialBuilder.PlazaName).transform; plaza.SetParent(areasGroup, false);
        Vector2 s = TowerFloor(plaza);
        ResidentialBuilder.FurnishPlaza(plaza, s);
        Aseos(plaza, new Vector3(s.x * 0.5f - 2.1f, 0.1f, -s.y * 0.5f + 5.5f), 0f, 1);
        for (int f = 0; f < roomFloors; f++)
        {
            Transform floor = new GameObject($"Planta_habitaciones_{f + 1}").transform; floor.SetParent(areasGroup, false);
            s = TowerFloor(floor);
            ResidentialBuilder.FurnishRoomFloor(floor, s, f);
            Aseos(floor, new Vector3(-s.x * 0.5f + 2.1f, 0.1f, s.y * 0.5f - 0.9f), 180f, 1);
        }
        Debug.Log($"[Residencia] Edificio-árbol: {ResidentialBuilder.RoomCount} habitaciones en {roomFloors} pisos y la plaza; {levels} niveles.");
    }
}
