#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility that creates pre-populated DialogueSequence assets.
///
/// Usage: Cold Sanctuary → Create Dialogue Assets
///
/// Creates the following assets in Assets/Dialogue/:
///   - DLG_Magnate_PrimeraEvaluacion  — the Magnate evaluates Kushal on arrival
///   - DLG_Magnate_BienvenidaSantuario — after placement, the Magnate reveals the sanctuary
///   - DLG_Goluis_Saludo               — Goluis greets Kushal for the first time
///   - DLG_Goluis_Presion              — Goluis pressures Kushal during a shift
///
/// Run this once. After that, assign the assets in the Inspector as needed.
/// </summary>
public static class DialogueAssetCreator
{
    [MenuItem("Cold Sanctuary/Create Dialogue Assets")]
    static void CreateAllDialogueAssets()
    {
        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Dialogue"))
            AssetDatabase.CreateFolder("Assets", "Dialogue");

        CreateMagnatePrimeraEvaluacion();
        CreateMagnateBienvenida();
        CreateGoluisSaludo();
        CreateGoloisPresion();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[DialogueAssetCreator] Todos los assets de diálogo creados en Assets/Dialogue/.");
        EditorUtility.DisplayDialog(
            "Diálogos creados",
            "Assets de diálogo creados en Assets/Dialogue/.\n\n" +
            "Asigna los portraits en el Inspector de cada asset.",
            "OK");
    }

    // ── La Magnate — Primera Evaluación ──────────────────────────────────────
    // Kushal acaba de llegar. La Magnate aparece. No pregunta — evalúa.
    // Tono: glacial, precisa, imposible de intimidar.

    static void CreateMagnatePrimeraEvaluacion()
    {
        const string path = "Assets/Dialogue/DLG_Magnate_PrimeraEvaluacion.asset";
        if (AssetDatabase.LoadAssetAtPath<DialogueSequence>(path) != null)
        {
            Debug.Log($"[DialogueAssetCreator] {path} ya existe — omitido.");
            return;
        }

        var seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.sequenceId = "magnate_primera_evaluacion";
        seq.playOnce   = true;
        seq.lines      = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Silencio.",
                typeSpeed    = 18f,
                pauseAfter   = 1.2f,
                screenEffect = DialogueScreenEffect.Darken,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Llegas tarde. Tres días, dos horas y —",
                typeSpeed    = 22f,
                pauseAfter   = 0.4f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "No importa.",
                typeSpeed    = 18f,
                pauseAfter   = 1.0f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Te has caído tres veces en el trayecto. " +
                               "La segunda vez dudaste. " +
                               "Eso me dice más de ti que cualquier currículum.",
                typeSpeed    = 24f,
                pauseAfter   = 0.8f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Este lugar no es para todos. Ni siquiera es para muchos. " +
                               "Pero tú ya estás aquí.",
                typeSpeed    = 22f,
                pauseAfter   = 0.6f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Kushal.",
                typeSpeed    = 14f,
                pauseAfter   = 1.5f,
                screenEffect = DialogueScreenEffect.Flash,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Vamos a ver qué sabes hacer.",
                typeSpeed    = 20f,
                pauseAfter   = 0.8f,
                screenEffect = DialogueScreenEffect.None,
            },
        };

        AssetDatabase.CreateAsset(seq, path);
        Debug.Log($"[DialogueAssetCreator] Creado: {path}");
    }

    // ── La Magnate — Bienvenida al Santuario ─────────────────────────────────
    // Después de la evaluación y el primer placement.
    // Tono: reveladora, casi cálida — pero solo por un momento.

    static void CreateMagnateBienvenida()
    {
        const string path = "Assets/Dialogue/DLG_Magnate_BienvenidaSantuario.asset";
        if (AssetDatabase.LoadAssetAtPath<DialogueSequence>(path) != null)
        {
            Debug.Log($"[DialogueAssetCreator] {path} ya existe — omitido.");
            return;
        }

        var seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.sequenceId = "magnate_bienvenida_santuario";
        seq.playOnce   = true;
        seq.lines      = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Este santuario existe porque alguien decidió que debía existir. " +
                               "Eso soy yo. No preguntes más sobre eso todavía.",
                typeSpeed    = 22f,
                pauseAfter   = 0.7f,
                screenEffect = DialogueScreenEffect.Darken,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Aquí trabajarás. Aprenderás. " +
                               "Si tienes suerte, entenderás por qué importa lo que haces.",
                typeSpeed    = 24f,
                pauseAfter   = 0.5f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "No te pido que confíes en mí. " +
                               "Te pido que no desperdicies lo que tienes.",
                typeSpeed    = 20f,
                pauseAfter   = 0.9f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "El mundo fuera de aquí seguirá sin ti. " +
                               "Aquí, tú marcas la diferencia.",
                typeSpeed    = 22f,
                pauseAfter   = 0.6f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "La Magnate",
                text         = "Empieza.",
                typeSpeed    = 12f,
                pauseAfter   = 1.5f,
                screenEffect = DialogueScreenEffect.Flash,
            },
        };

        AssetDatabase.CreateAsset(seq, path);
        Debug.Log($"[DialogueAssetCreator] Creado: {path}");
    }

    // ── Goluis — Saludo inicial ───────────────────────────────────────────────
    // Primera vez que Kushal entra en rango de Goluis en la cocina.
    // Tono: seco, directo, sin tiempo para protocolos.

    static void CreateGoluisSaludo()
    {
        const string path = "Assets/Dialogue/DLG_Goluis_Saludo.asset";
        if (AssetDatabase.LoadAssetAtPath<DialogueSequence>(path) != null)
        {
            Debug.Log($"[DialogueAssetCreator] {path} ya existe — omitido.");
            return;
        }

        var seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.sequenceId = "goluis_saludo";
        seq.playOnce   = true;
        seq.lines      = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName  = "Goluis",
                text         = "No toques nada todavía.",
                typeSpeed    = 35f,
                pauseAfter   = 0.4f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "Goluis",
                text         = "Esta cocina tiene un orden. " +
                               "Si lo rompes, lo limpias.",
                typeSpeed    = 35f,
                pauseAfter   = 0.3f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "Goluis",
                text         = "Aprende observando primero. " +
                               "Luego te digo qué puedes tocar.",
                typeSpeed    = 35f,
                pauseAfter   = 0.5f,
                screenEffect = DialogueScreenEffect.None,
            },
        };

        AssetDatabase.CreateAsset(seq, path);
        Debug.Log($"[DialogueAssetCreator] Creado: {path}");
    }

    // ── Goluis — Presión ─────────────────────────────────────────────────────
    // Cuando pressureActive = true y Kushal está en rango.
    // Tono: exigente, sin margen, pero sin crueldad gratuita.

    static void CreateGoloisPresion()
    {
        const string path = "Assets/Dialogue/DLG_Goluis_Presion.asset";
        if (AssetDatabase.LoadAssetAtPath<DialogueSequence>(path) != null)
        {
            Debug.Log($"[DialogueAssetCreator] {path} ya existe — omitido.");
            return;
        }

        var seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.sequenceId = "goluis_presion";
        seq.playOnce   = false;   // puede repetirse
        seq.lines      = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName  = "Goluis",
                text         = "Más rápido.",
                typeSpeed    = 40f,
                pauseAfter   = 0.2f,
                screenEffect = DialogueScreenEffect.None,
            },
            new DialogueLine
            {
                speakerName  = "Goluis",
                text         = "Si te duele, bien. " +
                               "Significa que todavía puedes mejorar.",
                typeSpeed    = 35f,
                pauseAfter   = 0.4f,
                screenEffect = DialogueScreenEffect.None,
            },
        };

        AssetDatabase.CreateAsset(seq, path);
        Debug.Log($"[DialogueAssetCreator] Creado: {path}");
    }
}
#endif
