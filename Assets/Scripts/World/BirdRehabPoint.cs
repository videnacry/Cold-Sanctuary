using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL REFUGIO DE AVES (Berón 2026-11-05; docs/energia.md §7): cajas de recuperación en la planta veterinaria del Hospital. Las aves heridas que encuentra la ronda del parque
/// se curan aquí y se sueltan aquí, a ≥ <see cref="EnergyRule.RehabMinDistance"/> m de las turbinas, porque el alimento y los animales concentrados atraen a otras aves: no se
/// cura ni se cría nada junto al rotor. Solo rehabilita: no cría aves.
/// </summary>
public class BirdRehabPoint : MonoBehaviour
{
    public static BirdRehabPoint Instance { get; private set; }

    [Tooltip("Las aves dentro de sus cajas: una encendida por paciente.")]
    public List<GameObject> birds = new List<GameObject>();

    public sealed class Patient { public string species; public int days; }
    public readonly List<Patient> patients = new List<Patient>();
    public int Admitted { get; private set; }
    public int Released { get; private set; }
    public int Capacity => birds.Count;

    void OnEnable() { Instance = this; Show(); }
    void OnDisable() { if (Instance == this) Instance = null; }

    /// <summary>Entra un ave herida. Sin caja libre no entra (la ronda la deja en la caja de transporte hasta otro día).</summary>
    public bool Admit(string species)
    {
        if (patients.Count >= Capacity) return false;
        patients.Add(new Patient { species = species });
        Admitted++;
        Show();
        Debug.Log($"[Refugio de aves] Entra un {species} herido ({patients.Count} de {Capacity}).");
        return true;
    }

    /// <summary>«hos_aves»: un día más de cura para cada una; las que cumplen <see cref="EnergyRule.RehabDays"/> se sueltan. Devuelve cuántas.</summary>
    public int Treat(string who)
    {
        int gone = 0;
        for (int i = patients.Count - 1; i >= 0; i--)
        {
            if (++patients[i].days < EnergyRule.RehabDays) continue;
            Debug.Log($"[Refugio de aves] {who} suelta un {patients[i].species}, lejos de las turbinas.");
            patients.RemoveAt(i); Released++; gone++;
        }
        Show();
        return gone;
    }

    void Show() { for (int i = 0; i < birds.Count; i++) if (birds[i] != null) birds[i].SetActive(i < patients.Count); }
}
