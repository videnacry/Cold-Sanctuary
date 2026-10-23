using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// FOTOS DE LOS MICROCOSMOS (para revisarlos sin entrar en Play): abre cada escena Microcosmos_* en aditivo y guarda en
/// Temp/PoseGallery/Micro_&lt;área&gt;_*.png una vista general desde arriba y otra a la altura de un insecto junto a la historia.
/// </summary>
public static class MicroSnapshot
{
    [MenuItem("Tools/Cold Sanctuary/Fotos de los microcosmos (PNG)")]
    public static void All()
    {
        foreach (string key in new[] { "Cocina", "Hospital", "Huerto", "SalaYoga", "Textil", "Mecanica", "LeChambon", "IrenaSendler",
                                       "Buda", "Patanjali", "Nightingale", "Tregua1914", "Wright", "Eratostenes", "Egina", "PlantaAtomica" }) Shoot(key);
    }

    static void Shoot(string key)
    {
        string path = $"Assets/Scenes/Microcosmos_{key}.unity";
        if (!File.Exists(path)) { Debug.LogWarning($"[FotoMicro] no existe {path}"); return; }
        Scene s = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try
        {
            GameObject root = null;
            foreach (GameObject g in s.GetRootGameObjects()) if (g.name.StartsWith("Microcosmos_")) root = g;
            if (root == null) return;
            Bounds b = new Bounds(root.transform.position, Vector3.zero);
            Transform giant = null; foreach (Transform t in root.transform) if (t.name.EndsWith("_gigante")) giant = t;
            foreach (Renderer r in (giant != null ? giant : root.transform).GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
            Transform story = null; foreach (Transform t in root.GetComponentsInChildren<Transform>()) if (t.name == "Historia") story = t;
            Vector3 c = b.center;
            Render(key + "_general", c + new Vector3(0f, b.extents.magnitude * 1.1f, -b.extents.magnitude * 0.9f), c, 55f);
            if (story != null) Render(key + "_suelo", story.position + new Vector3(-6f, 2.2f, -16f), story.position + new Vector3(0f, 3f, 10f), 70f);
        }
        finally { EditorSceneManager.CloseScene(s, true); }
    }

    static void Render(string name, Vector3 from, Vector3 at, float fov)
    {
        var camGO = new GameObject("FotoMicro_TEMP");
        try
        {
            var cam = camGO.AddComponent<Camera>(); cam.fieldOfView = fov; cam.nearClipPlane = 0.1f; cam.farClipPlane = 3000f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.55f, 0.7f, 0.85f);
            cam.transform.position = from; cam.transform.LookAt(at);
            var rt = new RenderTexture(1100, 700, 24); cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1100, 700, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1100, 700), 0, 0); tex.Apply();
            RenderTexture.active = null;
            string dir = Path.Combine(Application.dataPath, "..", "Temp", "PoseGallery"); Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, $"Micro_{name}.png"), tex.EncodeToPNG());
            Object.DestroyImmediate(tex); rt.Release();
            Debug.Log($"[FotoMicro] Temp/PoseGallery/Micro_{name}.png");
        }
        finally { Object.DestroyImmediate(camGO); }
    }
}
