using System;
using System.Collections;
using System.Collections.Generic;
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

    struct PendingHomeOrigin
    {
        public Animal member;
        public Vector3 origin;
    }

    void Start()
    {
        int totalSpawned = 0;
        List<PendingHomeOrigin> pending = new List<PendingHomeOrigin>();
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

            foreach (Animal member in members)
                if (member != null) pending.Add(new PendingHomeOrigin { member = member, origin = family.position });

            totalSpawned += members.Length;
        }
        StartCoroutine(ApplyHomeOriginsNextFrame(pending));
        Debug.Log($"[FamilyGenerator] {families.Length} familia(s) generadas — {totalSpawned} animales en total.");
    }

    // Cada especie llama Animal.Init() (HomeOrigin = transform.position) desde su propio
    // Start() — y Unity difiere el Start() de objetos recién instanciados al final del
    // frame actual, DESPUÉS de que este Start() ya terminó. Si asignáramos HomeOrigin acá
    // mismo, Init() lo pisaría después con el punto de aparición individual. Por eso
    // esperamos un frame (todos los Start() del frame ya corrieron) antes de fijar el nido
    // compartido, así el nuestro es el que gana. Ver
    // docs/refuge-and-adult-behavior.md "Montaje de escena: nidos antes que familias".
    IEnumerator ApplyHomeOriginsNextFrame(List<PendingHomeOrigin> pending)
    {
        yield return null;
        foreach (PendingHomeOrigin p in pending)
            if (p.member != null) p.member.HomeOrigin = p.origin;
    }
}
