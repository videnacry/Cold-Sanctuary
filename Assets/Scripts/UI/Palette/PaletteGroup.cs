using UnityEngine;

// Agrupa elementos de la paleta bajo una categoría navegable por teclado.
// Ejemplos: familias de la tabla periódica, tipos de posturas de yoga.
// El sistema de grupos se activa cuando el número de elementos supera groupThreshold
// en PaletteConfig o cuando el usuario lo elige explícitamente.
[System.Serializable]
public class PaletteGroup
{
    public string              groupId;
    public string              label;           // "Gases nobles", "Equilibrio", etc.
    public KeyCode             shortcut = KeyCode.None;
    public PaletteElementData[] elements;
}

public enum PaletteDisplayMode
{
    All,      // todos los elementos visibles — recomendado cuando son pocos
    ByGroup,  // solo el grupo activo visible; navega con shortcuts o flechas
    None,     // paleta colapsada
}
