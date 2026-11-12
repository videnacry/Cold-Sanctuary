using UnityEngine;

/// <summary>
/// POR QUÉ UN ANIMAL VA A UN ICEBERG, CUÁNTO SE QUEDA Y CUÁNDO SE VA (Berón 2026-11-05; docs/vida-en-icebergs.md). Son animales de verdad (foca y pingüino de Adelia) que
/// viven alrededor de su iceberg con <see cref="MarineLife"/>, pero con este sitio de hogar. No crían aquí y no suman coste de cría: son pocos y fijos.
/// VA: tras comer o de noche, como en la laguna (descansar lejos del agua). SE QUEDA: lo que marca <see cref="Stay"/>. SE VA: al acabar, o antes si tiene hambre.
/// </summary>
public class IcebergVisitor : MonoBehaviour
{
    public enum Reason { Descanso, Muda, CuidadoCrias }

    /// <summary>Individuos por especie que viven en los dos icebergs: los cuenta <see cref="FoodChain"/>.</summary>
    public const int Seals = 4, Penguins = 4;

    public Iceberg berg;
    bool _stopped;

    /// <summary>Por qué se queda hoy y cuántas horas de juego. Foca: la muda en los hielos que se mueven y el cuidado de crías en los quietos (Glacier Bay, 2024). Días: 7 por estación.</summary>
    public static (Reason why, float hours) Stay(string species, bool grounded, int day)
    {
        int season = Mathf.FloorToInt(Mathf.Repeat(day, 28f) / 7f);   // 0 primavera, 1 verano, 2 otoño, 3 invierno
        if (species == "Seal")
        {
            if (season == 0 && grounded) return (Reason.CuidadoCrias, 72f);
            if (season == 1 && !grounded) return (Reason.Muda, 48f);
            return (Reason.Descanso, 6f);
        }
        return (Reason.Descanso, 8f);
    }

    void Update()
    {
        if (_stopped) return;
        if (TryGetComponent(out Reproduction r)) { r.enabled = false; _stopped = true; }   // no crían aquí (nadie deja un huevo en un iceberg que deriva)
    }
}
