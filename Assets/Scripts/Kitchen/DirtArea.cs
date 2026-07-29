using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera y contabiliza suciedad en una zona (docs/kitchen-simulation.md §5, paso A). La cocina se ensucia
/// sola con el tiempo; al pasar de un **umbral** se dispara la **misión de limpieza**; cuando queda limpia,
/// se completa. Las manchas se borran **una a una** (`CleanNearest`). MVP del Mesocosmos (aquí se "limpia";
/// el puente al MicroKitchen —extraer la mancha en el mundo-insecto— es el paso D).
/// </summary>
public class DirtArea : MonoBehaviour
{
    [Tooltip("Tamaño XZ de la zona donde aparecen manchas (centrada en este objeto).")]
    public Vector2 areaSize = new Vector2(8f, 8f);
    [Min(0.1f)] public float spawnInterval = 1.5f;
    [Min(1)] public int maxSpots = 8;
    [Tooltip("Nº de manchas a partir del cual se activa la misión de limpieza.")]
    public int missionThreshold = 5;
    public float spotScale = 0.4f;

    readonly List<DirtSpot> _spots = new List<DirtSpot>();
    float _nextSpawn;
    bool _missionActive;

    public int Count => _spots.Count;
    public bool MissionActive => _missionActive;

    void Update()
    {
        if (Time.time >= _nextSpawn && _spots.Count < maxSpots)
        {
            _nextSpawn = Time.time + spawnInterval;
            SpawnSpot();
            if (!_missionActive && _spots.Count >= missionThreshold)
            {
                _missionActive = true;
                Debug.Log($"[Cocina] ¡Suciedad sobre el umbral ({_spots.Count}/{missionThreshold})! " +
                          $"Misión de limpieza ACTIVA en «{name}».");
            }
        }
    }

    void SpawnSpot()
    {
        Vector3 p = transform.position + new Vector3(
            Random.Range(-areaSize.x, areaSize.x) * 0.5f, 0.02f,
            Random.Range(-areaSize.y, areaSize.y) * 0.5f);
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Dirt_{Time.frameCount}";
        go.transform.SetParent(transform);
        go.transform.position = p;
        go.transform.localScale = new Vector3(spotScale, 0.02f, spotScale);
        DirtSpot spot = go.AddComponent<DirtSpot>();
        spot.area = this;
        _spots.Add(spot);
    }

    /// <summary>Un DirtSpot avisa de que fue limpiado. Si la zona queda limpia, completa la misión.</summary>
    public void NotifyCleaned(DirtSpot spot)
    {
        _spots.Remove(spot);
        if (_missionActive && _spots.Count == 0)
        {
            _missionActive = false;
            Debug.Log($"[Cocina] «{name}» limpia — misión de limpieza COMPLETA.");
        }
    }

    /// <summary>Limpia la mancha más cercana a <paramref name="pos"/> dentro de <paramref name="radius"/>. True si limpió.</summary>
    public bool CleanNearest(Vector3 pos, float radius)
    {
        DirtSpot best = null;
        float bestSqr = radius * radius;
        foreach (DirtSpot s in _spots)
        {
            if (s == null) continue;
            float d = (s.transform.position - pos).sqrMagnitude;
            if (d <= bestSqr) { bestSqr = d; best = s; }
        }
        if (best != null) { best.Clean(); return true; }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.5f, 0.3f, 0.25f);
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0.1f, areaSize.y));
    }
}
