using System;
using UnityEngine;

/// <summary>
/// Puebla la escena con familias de animales al arrancar Play. Cada entrada en
/// "families" es una familia independiente (una especie, una posición, un radio de
/// dispersión) — usar varias entradas de la misma especie para simular varias
/// familias separadas en vez de una sola manada gigante en un punto.
/// </summary>
public class FamilyGenerator : MonoBehaviour
{
    [Serializable]
    public class Family
    {
        [Tooltip("Prefab de la especie (cualquier instancia con componente Animal — solo se usa como template, el sexo de cada individuo se sortea al generar).")]
        public GameObject animalPrefab;
        [Tooltip("Si es 0, usa el parentsRate del grupo de la especie (Group.parentsRate).")]
        public int minParentsCount = 0;
        [Tooltip("Si es 0, usa el familySize del grupo de la especie (Group.familySize).")]
        public int quantity = 0;
        [Tooltip("Si es 0, usa un radio por defecto proporcional a la cantidad (quantity * 2).")]
        public float radius = 0;
        public Vector3 position;
        public float renderHeight;
    }
    public Family[] families;

    void Start()
    {
        int totalSpawned = 0;
        foreach (Family family in families)
        {
            if (family.animalPrefab == null)
            {
                Debug.LogWarning("[FamilyGenerator] Entrada sin animalPrefab asignado, se omite.");
                continue;
            }

            Animal template = family.animalPrefab.GetComponent<Animal>();
            if (template == null)
            {
                Debug.LogWarning($"[FamilyGenerator] {family.animalPrefab.name}: no tiene componente Animal, se omite.");
                continue;
            }

            Animal[] members = template.RenderFamily(family.position, family.renderHeight, family.minParentsCount, family.quantity, family.radius);
            totalSpawned += members.Length;
        }
        Debug.Log($"[FamilyGenerator] {families.Length} familia(s) generadas — {totalSpawned} animales en total.");
    }
}
