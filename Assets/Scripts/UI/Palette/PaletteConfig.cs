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
    public enum Mode { Direct, Formula, Dialogue }

    public Mode                 mode;
    public PaletteElementData[] elements;
    public IPaletteEvaluator    evaluator;      // solo en Formula
    public int                  maxSelection = 1; // ingredientes máximos antes de evaluar
}
