public enum PaletteElementType
{
    Action,         // un toque = acción inmediata
    Ingredient,     // se acumula en la fórmula actual (asana, hechizo, enriquecimiento)
    DialogueText,   // bloque de texto del NPC — solo display, no interactuable
    DialogueChoice, // respuesta del jugador — como Action pero en contexto diálogo
}
