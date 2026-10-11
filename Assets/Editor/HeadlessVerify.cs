using System;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Verificación por LÍNEA DE COMANDOS: compila, monta el blockout y guarda las escenas sin
/// tocar el Editor a mano. Existe porque el bridge MCP de Unity no conecta de forma fiable y
/// la verificación en vivo se quedaba bloqueada ciclo tras ciclo (ver docs/testing-checklist.md).
///
/// Uso (la licencia Personal NO tiene el entitlement 'com.unity.editor.headless', así que
/// -batchmode falla con return code 198; hay que lanzarlo en modo GUI normal):
/// <code>
/// Unity.exe -projectPath &lt;proyecto&gt; -logFile &lt;log&gt; -executeMethod HeadlessVerify.Run -quit
/// </code>
///
/// Todo lo relevante se escribe al log con el prefijo <c>[VERIFY]</c>, así que el resultado se
/// lee con un `grep "\[VERIFY\]"` sobre el log. El método NUNCA lanza: captura y reporta, para
/// que un fallo del builder no se confunda con un fallo de compilación (si los scripts no
/// compilan, Unity ni llega a invocar este método y el log solo tendrá los `error CS`).
/// </summary>
public static class HeadlessVerify
{
    const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Cold Sanctuary/Verify (compilar + blockout + guardar)")]
    public static void Run()
    {
        var report = new StringBuilder();
        int failures = 0;

        Debug.Log("[VERIFY] inicio — los scripts COMPILAN (si no, este método no se invocaría).");

        // 1) Abrir la escena del mesocosmos. El builder monta sobre la escena ACTIVA, así que
        //    sin esto construiría sobre la escena vacía que abre Unity al arrancar.
        try
        {
            EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
            Debug.Log($"[VERIFY] escena abierta: {SampleScenePath}");
        }
        catch (Exception e)
        {
            failures++;
            Debug.LogError($"[VERIFY] FALLO al abrir {SampleScenePath}: {e}");
            Finish(failures);
            return;
        }

        // 2) Montar el blockout (también genera las escenas hermanas del Microcosmos y hornea NavMesh).
        try
        {
            SampleSceneBuilder.Build();
            Debug.Log("[VERIFY] SampleSceneBuilder.Build() completado.");
        }
        catch (Exception e)
        {
            failures++;
            Debug.LogError($"[VERIFY] FALLO en SampleSceneBuilder.Build(): {e}");
        }

        // 3) Inventario de lo construido: confirma que el blockout quedó realmente en la escena
        //    (no basta con que Build() no lanzara).
        try
        {
            GameObject root = GameObject.Find("LevelDressing_AUTO");
            if (root == null)
            {
                failures++;
                Debug.LogError("[VERIFY] FALLO: no existe 'LevelDressing_AUTO' tras Build().");
            }
            else
            {
                int descendants = root.GetComponentsInChildren<Transform>(true).Length - 1;
                report.AppendLine($"[VERIFY] LevelDressing_AUTO con {descendants} descendientes.");
                foreach (string n in new[] { "TraceField_AUTO", "Clock_AUTO", "TestRunner_AUTO" })
                {
                    bool ok = root.transform.Find(n) != null || GameObject.Find(n) != null;
                    if (!ok) failures++;
                    report.AppendLine($"[VERIFY]   {(ok ? "OK  " : "FALTA")} {n}");
                }
                bool player = GameObject.FindGameObjectWithTag("Player") != null;
                report.AppendLine($"[VERIFY]   {(player ? "OK  " : "FALTA")} GameObject con tag 'Player'");
                if (!player) failures++;
            }
        }
        catch (Exception e)
        {
            failures++;
            Debug.LogError($"[VERIFY] FALLO en el inventario: {e}");
        }

        // 4) Guardar. El builder solo marca la escena como dirty; en CLI nadie pulsa Ctrl+S.
        try
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[VERIFY] escena guardada.");
        }
        catch (Exception e)
        {
            failures++;
            Debug.LogError($"[VERIFY] FALLO al guardar la escena: {e}");
        }

        Debug.Log(report.ToString());
        Finish(failures);
    }

    static void Finish(int failures)
    {
        if (failures == 0) Debug.Log("[VERIFY] RESULTADO: PASS (0 fallos).");
        else Debug.LogError($"[VERIFY] RESULTADO: FAIL ({failures} fallos).");
    }
}
