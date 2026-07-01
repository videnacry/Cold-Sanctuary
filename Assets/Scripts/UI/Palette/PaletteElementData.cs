using UnityEngine;

[System.Serializable]
public class PaletteElementData
{
    public string             id;
    public string             label;
    public Sprite             icon;
    public Color              tint        = Color.white;
    public KeyCode            shortcut    = KeyCode.None;
    public PaletteElementType type        = PaletteElementType.Action;
    public int                bondMin     = 0;   // bond mínimo del jugador para desbloquearse

    // payload: el dato real que el evaluador o el sistema de acción consume.
    // No se serializa por Unity — se asigna desde código al construir la config.
    // Ejemplos: ChemicalElement, BodyPosition, CareActivity, DialogueLine...
    [System.NonSerialized] public object payload;
}
