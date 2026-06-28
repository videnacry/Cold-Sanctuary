using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Una presa potencial dentro de la dieta de un animal.
/// </summary>
[System.Serializable]
public class PreyEntry
{
    /// <summary> Poblacion viva de esta presa (p.ej. BunnyBehavior.population). Se asigna en codigo. </summary>
    public HashSet<GameObject> population;
    /// <summary> Cuanto prefiere esta presa el cazador. Mayor = mas preferida. </summary>
    public float preference;
    /// <summary> Hambre minima para molestarse en cazarla (presa dificil/no-favorita => alta). </summary>
    public float difficulty;
    /// <summary> Distancia a la que el cazador detecta/considera esta presa. </summary>
    public float range;

    public PreyEntry(HashSet<GameObject> pPopulation, float pPreference, float pDifficulty, float pRange)
    {
        this.population = pPopulation;
        this.preference = pPreference;
        this.difficulty = pDifficulty;
        this.range = pRange;
    }
}

/// <summary>
/// Tabla de presas priorizada de un animal. Ver docs/behavior-system.md (Componente A).
/// </summary>
[System.Serializable]
public class Diet
{
    public PreyEntry[] entries;

    public Diet(PreyEntry[] pEntries)
    {
        this.entries = pEntries;
    }

    /// <summary>
    /// Elige la presa a cazar: entre las entradas elegibles (con un miembro dentro de
    /// <see cref="PreyEntry.range"/> y para las que el hambre del cazador alcanza su
    /// <see cref="PreyEntry.difficulty"/>), toma la de mayor <see cref="PreyEntry.preference"/>;
    /// dentro de ella, el miembro mas cercano. Devuelve null si no hay nada que valga la pena.
    /// </summary>
    public GameObject SelectPrey(Animal hunter)
    {
        PreyEntry chosenEntry = null;
        GameObject chosenTarget = null;
        Vector3 hunterPosition = hunter.transform.position;

        foreach (PreyEntry entry in entries)
        {
            if (entry == null || entry.population == null) continue;
            if (hunter.hungry < entry.difficulty) continue;

            GameObject nearest = null;
            float nearestDistance = entry.range;
            foreach (GameObject candidate in entry.population)
            {
                if (candidate == null || candidate == hunter.gameObject) continue;
                ITarget candidateTarget = candidate.GetComponent<ITarget>();
                if (candidateTarget != null && !hunter.CanHarm(candidateTarget)) continue;
                float distance = Vector3.Distance(hunterPosition, candidate.transform.position);
                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }
            if (nearest == null) continue;

            if (chosenEntry == null || entry.preference > chosenEntry.preference)
            {
                chosenEntry = entry;
                chosenTarget = nearest;
            }
        }
        return chosenTarget;
    }
}
