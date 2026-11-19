using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mini-harness de PRUEBAS por consola (PASS/FAIL) — la capa que faltaba: los sandboxes `*_AUTO` y los drivers
/// existentes (`MagicSandboxDriver`) **conducen** o **vuelcan valores** (`MigrationDiagnostics`), pero **no aseveran**.
/// Esto sí: un chequeo con condición esperada que loguea `[TEST] PASS/FAIL` + un resumen, de modo que el compañero
/// **lea el veredicto en `Editor.log`** (grep `[TEST]`) en vez de interpretar volcados.
///
/// **Reutilizable en dos frentes** (docs/testing-checklist.md): (1) sandboxes autoejecutables (`NavAuto`/`EmergenceAuto`…
/// conducen y luego `Check`ean); (2) **misiones jugables (WASD)** — una misión llama `Check` en sus criterios de éxito,
/// así el juego real reporta pruebas igual que los sandboxes. Verdad de laboratorio y verdad de juego, mismo canal.
///
/// FAIL usa `LogWarning` (NO `LogError`) a propósito: un test fallido **no** debe contaminar el "0 errores de
/// compilación" que valida el compañero. Un fallo se ve como advertencia `[TEST] FAIL`.
/// </summary>
public static class TestProbe
{
    static int _pass, _fail;
    static string _group = "";
    static int _grandPass, _grandFail;   // acumulados a través de TODOS los grupos (para el TOTAL del TestRunner)
    public static int GrandPass => _grandPass;
    public static int GrandFail => _grandFail;

    // (2026-11-11, pruebas rápidas) EL CONTEXTO: grupo y unidad que corren (lo pone el TestRunner). Cada FAIL lo lleva en su línea, y se guardan
    // los textos de los FAIL para el archivo de resultados (TestResults).
    public static int ContextGroup { get; private set; }
    public static string ContextUnit { get; private set; } = "";
    public static readonly List<string> Fails = new List<string>();
    /// <summary>(2026-11-13, sistema de QA, docs/qa-propuesta.md §3) Los checks cuyo texto empieza por un ID de caso («[CV-03] …»): el ID y si
    /// pasó. El TestRunner los lleva a la columna «casos» de Logs/test-results.tsv, así un FAIL se liga solo a su caso.</summary>
    public static readonly List<(string id, bool ok)> Cases = new List<(string id, bool ok)>();
    public static void Context(int group, string unit) { ContextGroup = group; ContextUnit = unit ?? ""; }
    /// <summary>La línea de un FAIL: «[TEST] FAIL · g50 JornadaTest · nombre — detalle» (sin contexto, como antes). Sigue empezando por «[TEST] FAIL ·».</summary>
    public static string FailLine(string name, string detail) => $"[TEST] FAIL · {(ContextUnit.Length > 0 ? $"g{ContextGroup} {ContextUnit} · " : "")}{name}{Tail(detail)}";

    /// <summary>Resetea el TOTAL global (lo llama el `TestRunner` antes de correr todos los grupos).</summary>
    public static void ResetTotals() { _grandPass = 0; _grandFail = 0; }

    /// <summary>Loguea el veredicto GLOBAL de la corrida (suma de todos los grupos).</summary>
    public static void Total()
    {
        string v = _grandFail == 0 ? "OK ✓" : "FALLOS ✗";
        Debug.Log($"[TEST] ═══ TOTAL: {_grandPass} PASS / {_grandFail} FAIL → {v} ═══");
    }

    /// <summary>Abre un grupo de checks (resetea contadores). Llamar al empezar un sandbox/misión.</summary>
    public static void Begin(string group)
    {
        _group = group; _pass = 0; _fail = 0;
        Debug.Log($"[TEST] ▼ {group} — inicio");
    }

    /// <summary>Un aserto booleano. Loguea PASS/FAIL y cuenta. Devuelve la condición (para encadenar).</summary>
    public static bool Check(string name, bool ok, string detail = "")
    {
        string caseId = QaCheck.CaseIdOf(name);
        if (caseId != null) Cases.Add((caseId, ok));
        if (ok) { _pass++; _grandPass++; Debug.Log($"[TEST] PASS · {name}{Tail(detail)}"); }
        else    { _fail++; _grandFail++; Debug.LogWarning(FailLine(name, detail)); Fails.Add(name + Tail(detail)); }
        return ok;
    }

    /// <summary>Aserto numérico con tolerancia (|actual − esperado| ≤ tol).</summary>
    public static bool Near(string name, float actual, float expected, float tol, string detail = "")
        => Check(name, Mathf.Abs(actual - expected) <= tol, $"actual={actual:0.###} esperado={expected:0.###}±{tol:0.###}{Tail(detail)}");

    /// <summary>Aserto "actual &gt; umbral" (p.ej. la confianza subió, el rastro es &gt; 0).</summary>
    public static bool Greater(string name, float actual, float threshold, string detail = "")
        => Check(name, actual > threshold, $"actual={actual:0.###} > {threshold:0.###}?{Tail(detail)}");

    /// <summary>Aserto "no es null" (componente/objeto presente).</summary>
    public static bool NotNull(string name, object obj, string detail = "")
        => Check(name, obj != null && !obj.Equals(null), detail);

    /// <summary>Cierra el grupo y loguea el veredicto (grep `[TEST] SUMMARY`).</summary>
    public static void End()
    {
        string verdict = _fail == 0 ? "OK ✓" : "FALLOS ✗";
        Debug.Log($"[TEST] ▲ {_group} — SUMMARY: {_pass} PASS / {_fail} FAIL → {verdict}");
    }

    static string Tail(string d) => string.IsNullOrEmpty(d) ? "" : $" — {d}";
}
