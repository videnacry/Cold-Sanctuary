// Describe el contenido y comportamiento completo de un menú Palette.
// Se construye desde código (el sistema que abre el menú pasa su propia config).
// Ejemplos de uso:
//   AsanaSystem.OpenMenu()       → new PaletteConfig { mode = Formula, elements = bodyParts, evaluator = asanaEval }
//   EnchantmentSystem.OpenMenu() → new PaletteConfig { mode = Formula, elements = elements, evaluator = enchEval  }
//   CareSystem.OpenMenu(cub)     → new PaletteConfig { mode = Direct,  elements = activities                      }
//   DialogueSystem.Say(lines)    → new PaletteConfig { mode = Dialogue, elements = blocks + choices               }
[System.Serializable]
public class PaletteConfig
{
    public enum Mode
    {
        Direct,   // todos los elementos disparan acción inmediata
        Formula,  // todos los elementos se acumulan; al completar → evaluador
        Hybrid,   // por elemento: isDirectUnlocked → directo; si no → ingrediente de fórmula
        Dialogue, // DialogueText solo display; DialogueChoice dispara acción
    }

    public Mode                 mode;
    public PaletteElementData[] elements;
    public IPaletteEvaluator    evaluator;            // solo en Formula / Hybrid
    public int                  maxSelection     = 1;    // ingredientes máximos antes de evaluar
    // Propuesta pendiente: el multiplicador debería ser lo suficientemente alto como para que
    // la diferencia entre directo y fórmula sea perceptible y motive el uso de la fórmula,
    // pero sin hacer que el modo directo se sienta inútil. Valor tentativo: 2.5–4×.
    // Calibrar según feedback de jugabilidad — ver DEVLOG.md §Propuestas de balance.
    public float                formulaMultiplier = 1.5f;

    // Agrupación — opcional. Si se define, permite navegación por categoría.
    // Se activa automáticamente cuando elements.Length > groupThreshold.
    public PaletteGroup[]      groups;
    public int                 groupThreshold   = 12;    // umbral a partir del cual se sugiere ByGroup
    public PaletteDisplayMode  defaultDisplay   = PaletteDisplayMode.All;
}
