using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LAS NORMAS DE CONTENCIÓN DEL ÁRBOL CAZADOR (Berón 2026-11-05: «el árbol cazador queda solo en un piso específico de investigación de modo que conserva todas sus características y
/// se implementa normas de seguridad»; docs/contencion-arbol-cazador.md). El árbol no se debilita: se le contiene. Vienen de la contención de plantas (BL3-P: acceso restringido,
/// descontaminar lo que sale, manual de prácticas) y del manejo de animales peligrosos (contacto protegido: siempre una barrera; doble puerta; simulacros; código de escape).
/// </summary>
public static class HunterRule
{
    /// <summary>Rondas diarias perdidas que aguantan los filtros.</summary>
    public const int MissedRounds = 3;
    /// <summary>Un fallo de sellos cada tantos días de media, desde <see cref="FirstFailureDay"/>.</summary>
    public const int FailureEvery = 60, FirstFailureDay = 30;
    /// <summary>Quienes tienen que firmar la recaptura: nadie trabaja solo junto a él.</summary>
    public const int RecaptureCrew = 2;
    /// <summary>Días hasta que las compuertas cierran solas, sin nadie (a prueba de fallos: la emergencia no dura para siempre).</summary>
    public const int AutoSealDays = 1;
    public const int DrillEvery = 7;
    /// <summary>Su alcance entero, el del árbol mayor de la antigua arboleda: señuelo y sopor letal (m). Dentro de la celda no sale de ella.</summary>
    public const float LureRadius = 70f, KillRadius = 12f;

    public static bool FailureOn(int day)
    {
        if (day < FirstFailureDay) return false;
        unchecked { uint h = (uint)day * 2654435761u; return ((h ^ (h >> 13)) >> 8) % FailureEvery == 0; }
    }
}

/// <summary>
/// LA CELDA DEL ÁRBOL CAZADOR: mientras está sellada, su señuelo no sale y su sopor solo alcanza lo que hay dentro. Si se rompe, el árbol actúa con todo su alcance, sobre cualquiera,
/// hasta que dos personas la recontengan o cierren las compuertas. Nadie entra: raciones por el portillo y muestras por los guantes.
/// </summary>
public class HunterCell : MonoBehaviour
{
    public enum State { Contenido, Brecha }
    public const string RoundId = "cuar_ronda", RationId = "cuar_racion", SampleId = "cuar_muestra", DrillId = "cuar_simulacro", RecaptureId = "cuar_recaptura";

    public static HunterCell Instance { get; private set; }

    [Tooltip("La caja de la celda (mundo), en coordenadas de este objeto.")] public Bounds cell;
    public IceCarnivorousTree tree;
    [Tooltip("Donde trabaja el equipo: fuera de la celda.")] public Transform post;

    public State Now { get; private set; } = State.Contenido;
    public int Rounds { get; private set; }
    public int Rations { get; private set; }
    public int Samples { get; private set; }
    public int Drills { get; private set; }
    public int Breaches { get; private set; }
    public int CrewSigned { get; private set; }

    int _missed, _lastDay = int.MinValue, _breachDay;

    static int Today => TimeController.timeController != null ? TimeController.timeController.Day : 0;

    void OnEnable()
    {
        Instance = this;
        WorkdayDirector.RegisterPlace(AreaWork.Containment, post != null ? post : transform);
    }
    void OnDisable() { if (Instance == this) Instance = null; }

    /// <summary>¿Está sujeto este árbol por la celda?</summary>
    public bool Holds(IceCarnivorousTree t) => t == tree && Now == State.Contenido;
    public bool Contains(Vector3 world) => cell.Contains(transform.InverseTransformPoint(world));

    void Update()
    {
        int d = Today;
        if (d == _lastDay) return;
        bool first = _lastDay == int.MinValue;
        _lastDay = d;
        if (first || ColdSanctuary.TestRunner.Running) return;
        if (Now == State.Brecha)
        {
            if (d - _breachDay >= HunterRule.AutoSealDays) Seal("las compuertas, solas");
            return;
        }
        _missed++;
        if (_missed > HunterRule.MissedRounds) Breach("sin ronda: filtros saturados");
        else if (HunterRule.FailureOn(d)) Breach("fallo de un sello");
    }

    public void Breach(string why)
    {
        if (Now == State.Brecha) return;
        Now = State.Brecha; Breaches++; CrewSigned = 0; _breachDay = Today;
        Debug.LogWarning($"[Contención] CÓDIGO VERDE: brecha en la celda del árbol cazador ({why}). Nadie se acerca solo.");
    }

    void Seal(string who)
    {
        Now = State.Contenido; _missed = 0; CrewSigned = 0;
        Debug.Log($"[Contención] Celda sellada de nuevo: {who}.");
    }

    public void Round(string who) { Rounds++; _missed = 0; Debug.Log($"[Contención] {who}: ronda de sellos, presión y filtros."); }
    public void Ration(string who) { Rations++; if (tree != null) tree.Absorb(); Debug.Log($"[Contención] {who}: ración por el portillo."); }
    public void Sample(string who) { Samples++; if (tree != null) tree.Prune(); Debug.Log($"[Contención] {who}: muestra por los guantes y poda."); }
    public void Drill(string who) { Drills++; Debug.Log($"[Contención] {who}: simulacro de escape."); }
    public void Recapture(string who)
    {
        if (Now != State.Brecha) return;
        CrewSigned++;
        if (CrewSigned >= HunterRule.RecaptureCrew) Seal($"equipo de recaptura, {who} el último");
    }

    /// <summary>Las misiones del laboratorio. Durante una brecha, solo la recaptura.</summary>
    public static List<WorkRoster.Mission> Missions(int day)
    {
        var l = new List<WorkRoster.Mission>();
        if (Instance == null) return l;
        string a = AreaWork.Containment;
        if (Instance.Now == State.Brecha)
        {
            l.Add(new WorkRoster.Mission(a, RecaptureId, "Código verde: recontener la celda del árbol cazador, desde fuera", 1f, true));
            l.Add(new WorkRoster.Mission(a, RecaptureId + "_2", "Código verde: segunda persona de la recaptura", 1f, true));
            return l;
        }
        l.Add(new WorkRoster.Mission(a, RoundId, "Ronda de contención: sellos, presión negativa y filtros", 1f, true));
        l.Add(new WorkRoster.Mission(a, RationId, "Dar su ración al árbol cazador por el portillo", 1f));
        l.Add(new WorkRoster.Mission(a, SampleId, "Tomar muestras y podar por los guantes de la celda", 1f));
        if (day % HunterRule.DrillEvery == 0) l.Add(new WorkRoster.Mission(a, DrillId, "Simulacro de escape del árbol cazador", 1f));
        return l;
    }
}
