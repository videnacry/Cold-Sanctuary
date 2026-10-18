using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GALERÍA DE POSTURAS: pone un cuerpo de cubos en la postura FINAL de cada paso de cada sesión de yoga
/// (<see cref="YogaLibrary"/>) y lo fotografía de frente y de lado, con el suelo marcado, en
/// `Temp/PoseGallery/&lt;sesión&gt;_NN.png`. Sirve para revisar a ojo que cada postura se vea real: miembros que se
/// atraviesan (codos dentro del torso, piernas una dentro de otra) o cuerpos flotando sobre el suelo. Además escribe en
/// el log, por paso, cuánto queda el punto más bajo del cuerpo sobre el suelo y qué segmentos se interpenetran.
/// </summary>
public static class PoseGallery
{
    const int W = 360, H = 420;

    [MenuItem("Tools/Cold Sanctuary/Pose Gallery (PNG de cada postura)")]
    public static void Run()
    {
        string dir = Path.Combine(Application.dataPath, "..", "Temp", "PoseGallery");
        Directory.CreateDirectory(dir);
        foreach (string f in Directory.GetFiles(dir, "*.png")) File.Delete(f);

        Scene scene = EditorSceneManager.NewPreviewScene();
        try
        {
            var lightGO = new GameObject("Luz");
            var light = lightGO.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.2f;
            lightGO.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            SceneManager.MoveGameObjectToScene(lightGO, scene);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.localScale = new Vector3(6f, 0.02f, 6f); floor.transform.position = new Vector3(0f, -0.01f, 0f);
            floor.GetComponent<Renderer>().sharedMaterial = KitchenProps.Mat(new Color(0.55f, 0.6f, 0.65f));
            SceneManager.MoveGameObjectToScene(floor, scene);

            var camGO = new GameObject("Cam");
            var cam = camGO.AddComponent<Camera>();
            cam.scene = scene; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.93f, 0.95f, 0.98f);
            cam.fieldOfView = 35f; cam.nearClipPlane = 0.05f;
            SceneManager.MoveGameObjectToScene(camGO, scene);
            var rt = new RenderTexture(W, H, 24);
            cam.targetTexture = rt;

            int total = 0, flagged = 0;
            foreach (YogaSession s in YogaLibrary.All)
            {
                List<QueuedSpell> seq = s.Build();
                int idx = 0;
                string lastName = null;
                foreach (QueuedSpell q in seq)
                {
                    if (!(q is AsanaPoseSpell pose)) continue;
                    string baseName = pose.asanaName.Split(new[] { " · " }, System.StringSplitOptions.None)[0];
                    if (baseName == lastName) continue;   // las fases de "mantener" (y la otra respiración) repiten la postura
                    lastName = baseName;

                    var who = new GameObject("Modelo");
                    SceneManager.MoveGameObjectToScene(who, scene);
                    Anima a = who.AddComponent<Anima>();
                    CubeHumanoid.Dress(who, new Color(0.55f, 0.45f, 0.65f), new Color(0.86f, 0.7f, 0.58f), 1.75f);
                    pose.ApplyFinal(a);

                    // Medidas: punto más bajo (¿flota?) y segmentos que se atraviesan.
                    float lowest = float.MaxValue;
                    var segs = new List<(string name, Bounds b, Transform t)>();
                    foreach (CubeJoint j in who.GetComponentsInChildren<CubeJoint>())
                    {
                        if (j.segment == null) continue;
                        Renderer r = j.segment.GetComponent<Renderer>(); if (r == null) continue;
                        lowest = Mathf.Min(lowest, r.bounds.min.y);
                        segs.Add((j.part.ToString(), r.bounds, j.transform));
                    }
                    string clash = Clashes(who);
                    string file = $"{Safe(s.name)}_{idx:00}.png";
                    Shoot(cam, rt, who.transform, Path.Combine(dir, file), pose.asanaName);
                    bool bad = lowest > 0.12f || clash.Length > 0;
                    if (bad) flagged++;
                    Debug.Log($"[Posturas] {(bad ? "REVISAR" : "ok")} · {s.name} #{idx:00} «{pose.asanaName}» · punto más bajo {lowest:0.00} m{(lowest > 0.12f ? " (FLOTA)" : "")}{(clash.Length > 0 ? " · se atraviesan: " + clash : "")} → {file}");
                    Object.DestroyImmediate(who);
                    idx++; total++;
                }
            }
            Debug.Log($"[Posturas] {total} posturas fotografiadas en Temp/PoseGallery; {flagged} para revisar.");
            rt.Release();
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    // Pares de segmentos NO contiguos cuyos cubos se meten uno dentro del otro (más de 3 cm de solape en los 3 ejes,
    // medido en el espacio del primero con su caja orientada, no la AABB del mundo).
    static string Clashes(GameObject who)
    {
        var list = new List<(CubeJoint j, Transform seg)>();
        foreach (CubeJoint j in who.GetComponentsInChildren<CubeJoint>()) if (j.segment != null) list.Add((j, j.segment));
        var outp = new List<string>();
        for (int i = 0; i < list.Count; i++)
            for (int k = i + 1; k < list.Count; k++)
            {
                CubeJoint a = list[i].j, b = list[k].j;
                if (a.transform.parent == b.transform || b.transform.parent == a.transform || a.transform.parent == b.transform.parent) continue;   // contiguos o hermanos (cadera-muslos)
                if (Overlap(list[i].seg, list[k].seg) > 0.03f) outp.Add($"{a.part}↔{b.part}");
            }
        return string.Join(", ", outp);
    }

    // Penetración aproximada: vértices de B dentro de la caja de A (y viceversa); devuelve la mayor profundidad.
    static float Overlap(Transform a, Transform b)
    {
        return Mathf.Max(Depth(a, b), Depth(b, a));
    }

    static float Depth(Transform box, Transform other)
    {
        float best = 0f;
        Vector3 half = Vector3.one * 0.5f;
        for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++) for (int z = -1; z <= 1; z++)
        {
            Vector3 w = other.TransformPoint(new Vector3(x, y, z) * 0.45f);   // 27 puntos del otro cubo (algo hacia dentro)
            Vector3 l = box.InverseTransformPoint(w);
            if (Mathf.Abs(l.x) < 0.5f && Mathf.Abs(l.y) < 0.5f && Mathf.Abs(l.z) < 0.5f)
            {
                Vector3 s = box.lossyScale;
                float d = Mathf.Min((0.5f - Mathf.Abs(l.x)) * s.x, (0.5f - Mathf.Abs(l.y)) * s.y, (0.5f - Mathf.Abs(l.z)) * s.z);
                best = Mathf.Max(best, d);
            }
        }
        return best;
    }

    static void Shoot(Camera cam, RenderTexture rt, Transform who, string path, string title)
    {
        var tex = new Texture2D(W * 2, H, TextureFormat.RGB24, false);
        Vector3 c = new Vector3(0f, 1.0f, 0f);
        Vector3[] eyes = { new Vector3(0f, 1.1f, 4.4f), new Vector3(4.4f, 1.1f, 0f) };   // de frente y de lado
        for (int v = 0; v < 2; v++)
        {
            cam.transform.position = eyes[v]; cam.transform.LookAt(c);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, W, H), v * W, 0);
        }
        tex.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    static string Safe(string s)
    {
        foreach (char ch in Path.GetInvalidFileNameChars()) s = s.Replace(ch, '_');
        return s.Replace(' ', '_');
    }
}
