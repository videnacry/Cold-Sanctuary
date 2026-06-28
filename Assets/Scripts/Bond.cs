using UnityEngine;

public enum BondType
{
    Imprint, // criado desde muy pequeño por el objetivo (jugador como madre)
    Friend,  // convivencia prolongada desde joven
}

/// <summary>
/// Vínculo afectivo de un animal hacia un objetivo (0–100).
/// 100 = incondicional; nunca causa daño.
/// Ver docs/behavior-system.md (Componente D).
/// </summary>
[System.Serializable]
public class Bond
{
    public ITarget target;
    public float value;
    public BondType type;

    public Bond(ITarget pTarget, BondType pType, float initialValue = 0f)
    {
        target = pTarget;
        type = pType;
        value = Mathf.Clamp(initialValue, 0f, 100f);
    }
}
