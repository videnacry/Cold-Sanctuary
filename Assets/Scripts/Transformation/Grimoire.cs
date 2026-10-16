using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GRIMORIO — registro de hechizos APRENDIDOS por un `Anima` (docs/magic-metabolism-progression.md).
/// El PRIMER hechizo (`awakenSpellId`) es "Despertar las Reservas": al aprenderlo se crean las pools de
/// magia (<see cref="MagicReserves.Unlock"/>) con su límite. A partir de ahí:
///   • el exceso de comer/descomponer llena las pools (ver `Metabolism.Absorb`),
///   • y se van aprendiendo los demás hechizos (fuego/agua/tierra/viento → nuclear → masa-energía).
/// Emite <see cref="OnLearned"/> para refrescar barras/UI al vuelo (ver checklist «learning-unlocks»). Opt-in;
/// se cablea en `Anima`s reales (jugador/companions). No hay sandbox (Anima es abstracta).
/// </summary>
public class Grimoire : MonoBehaviour
{
    public Anima anima;
    public MagicReserves reserves;

    [Tooltip("Id del primer hechizo: aprenderlo DESBLOQUEA las pools de magia.")]
    public string awakenSpellId = "awaken-reserves";

    [Header("Hechizos ya aprendidos (ids).")]
    public List<string> known = new List<string>();

    [Tooltip("Maestro omnisciente (p. ej. la Magnate): puede enseñar cualquier hechizo aunque no lo tenga en la lista.")]
    public bool knowsAll = false;

    /// <summary>Se dispara al aprender un hechizo (id) — para refrescar la UI/barras en vivo.</summary>
    public event Action<string> OnLearned;

    void Awake()
    {
        if (anima == null) anima = GetComponent<Anima>();
        if (reserves == null) reserves = GetComponent<MagicReserves>();
    }

    public bool Knows(string id) => !string.IsNullOrEmpty(id) && (knowsAll || known.Contains(id));

    /// <summary>¿Puede APRENDER este hechizo ahora? (no lo sabe ya y cumple todos los prerequisitos). `missing` dice qué falta.</summary>
    public bool CanLearn(string id, out string missing)
    {
        missing = null;
        if (string.IsNullOrEmpty(id)) { missing = "(vacío)"; return false; }
        if (known.Contains(id)) { missing = "(ya lo sabe)"; return false; }
        foreach (string pre in SpellCatalog.Prerequisites(id))
            if (!Knows(pre)) { missing = pre; return false; }
        return true;
    }

    /// <summary>Aprende un hechizo. El primero (`awakenSpellId`) despierta las reservas. Idempotente.</summary>
    public bool Learn(string id)
    {
        if (string.IsNullOrEmpty(id) || known.Contains(id)) return false;
        known.Add(id);
        if (id == awakenSpellId)
        {
            if (reserves == null) reserves = GetComponent<MagicReserves>();
            if (reserves != null) reserves.Unlock();
        }
        Debug.Log($"[Grimorio] «{(anima != null ? anima.name : name)}» aprende «{id}»" +
                  (id == awakenSpellId ? " → pools de magia DESBLOQUEADAS." : "."));
        OnLearned?.Invoke(id);
        return true;
    }
}
