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


    /// <summary>RETRATOS de las personas del santuario (de frente y de lado, a escala real) en Temp/PoseGallery/Personajes.png.</summary>
    [MenuItem("Tools/Cold Sanctuary/Character Gallery (PNG de las personas)")]
    public static void Characters()
    {
        string[] names = { "Player", "Magnate", "Linduarda", "Goluis", "Panterilia", "Gohageneis", "Irosene" };
        string dir = Path.Combine(Application.dataPath, "..", "Temp", "PoseGallery");
        Directory.CreateDirectory(dir);
        Scene scene = EditorSceneManager.NewPreviewScene();
        try
        {
            var lightGO = new GameObject("Luz"); var light = lightGO.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.2f;
            lightGO.transform.rotation = Quaternion.Euler(30f, 160f, 0f); SceneManager.MoveGameObjectToScene(lightGO, scene);   // luz de frente
            var camGO = new GameObject("Cam"); var cam = camGO.AddComponent<Camera>();
            cam.scene = scene; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.93f, 0.95f, 0.98f); cam.fieldOfView = 30f;
            SceneManager.MoveGameObjectToScene(camGO, scene);
            int w = 220, h = 460; var rt = new RenderTexture(w, h, 24); cam.targetTexture = rt;
            var tex = new Texture2D(w * names.Length, h * 2, TextureFormat.RGB24, false);
            int fw = 260; var frt = new RenderTexture(fw, fw, 24); var faces = new Texture2D(fw * names.Length, fw, TextureFormat.RGB24, false);
            for (int i = 0; i < names.Length; i++)
            {
                var who = new GameObject(names[i]); SceneManager.MoveGameObjectToScene(who, scene);
                who.AddComponent<Anima>();
                CubeHumanoid.Dress(who, Color.gray, Color.gray);
                // Sus stats (los de la escena) para que el cuerpo salga como en el juego: estatura, músculo y grasa.
                Anima an = who.GetComponent<Anima>();
                var st = new System.Collections.Generic.Dictionary<string, Vector4>
                {
                    { "Linduarda", new Vector4(1.15f, 1.35f, 1.45f, 1.05f) }, { "Goluis", new Vector4(1.5f, 0.9f, 1.4f, 1.29f) },
                    { "Panterilia", new Vector4(0.7f, 0.95f, 0.9f, 0.79f) }, { "Gohageneis", new Vector4(1.1f, 1.2f, 1.3f, 1.09f) },
                    { "Irosene", new Vector4(1.2f, 1.2f, 1.3f, 1.29f) },
                };
                if (an != null && st.TryGetValue(names[i], out Vector4 sv)) { an.strength = sv.x; an.agility = sv.y; an.endurance = sv.z; an.bodyMass = sv.w; }
                if (who.TryGetComponent(out BodyShape bsh)) bsh.Apply(true);
                for (int v = 0; v < 2; v++)
                {
                    cam.transform.position = v == 0 ? new Vector3(0f, 1.05f, 4.6f) : new Vector3(4.6f, 1.05f, 0f);
                    cam.transform.LookAt(new Vector3(0f, 0.95f, 0f));
                    cam.Render(); RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, w, h), i * w, (1 - v) * h);
                }
                // PRIMER PLANO de la cara (a la altura de los ojos de cada uno).
                Transform headBone = null;
                foreach (CubeJoint j in who.GetComponentsInChildren<CubeJoint>()) if (j.part == BodyPart.Head) headBone = j.transform;
                if (headBone != null)
                {
                    Vector3 hc = headBone.TransformPoint(new Vector3(0f, 0.12f, 0f));
                    cam.targetTexture = frt; cam.fieldOfView = 30f;
                    cam.transform.position = hc + new Vector3(0.25f, 0.05f, 0.95f); cam.transform.LookAt(hc);
                    cam.Render(); RenderTexture.active = frt;
                    faces.ReadPixels(new Rect(0, 0, fw, fw), i * fw, 0);
                    cam.targetTexture = rt; cam.fieldOfView = 30f;
                }
                Object.DestroyImmediate(who);
            }
            tex.Apply(); RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(dir, "Personajes.png"), tex.EncodeToPNG());
            faces.Apply(); File.WriteAllBytes(Path.Combine(dir, "Caras.png"), faces.EncodeToPNG()); Object.DestroyImmediate(faces); frt.Release();
            Object.DestroyImmediate(tex); rt.Release();
            Debug.Log($"[Personajes] retratos en Temp/PoseGallery/Personajes.png: {string.Join(", ", names)}");
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }

    /// <summary>MAPA desde arriba de la escena abierta (casquetes, áreas, árboles) en Temp/PoseGallery/Mapa.png.</summary>
    [MenuItem("Tools/Cold Sanctuary/Map (PNG desde arriba)")]
    public static void Map()
    {
        var camGO = new GameObject("MapCam_TEMP");
        try
        {
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 255f;
            cam.transform.position = new Vector3(0f, 400f, 0f); cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.farClipPlane = 1000f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.2f, 0.35f, 0.5f);
            var rt = new RenderTexture(1024, 1024, 24); cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0); tex.Apply();
            RenderTexture.active = null;
            string dir = Path.Combine(Application.dataPath, "..", "Temp", "PoseGallery"); Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "Mapa.png"), tex.EncodeToPNG());
            Object.DestroyImmediate(tex); rt.Release();
            Debug.Log("[Mapa] Temp/PoseGallery/Mapa.png");
        }
        finally { Object.DestroyImmediate(camGO); }
    }

    /// <summary>FOTOS de objetos de la escena abierta (el tanque, el laboratorio…) en Temp/PoseGallery/Foto_*.png.</summary>
    [MenuItem("Tools/Cold Sanctuary/Snapshots (tanque y laboratorio)")]
    public static void Snapshots()
    {
        foreach (string n in new[] { "Tanque", "Laboratorio_CarneCultivada", "Nido_Abejas", "Estatuas", "Comedor_Gourmet", "Sala", "Estudio_Textil" }) Snapshot(n);
    }

    public static void Snapshot(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null) { Debug.LogWarning($"[Foto] no encuentro «{objectName}»"); return; }
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
        var camGO = new GameObject("FotoCam_TEMP");
        try
        {
            var cam = camGO.AddComponent<Camera>(); cam.fieldOfView = 45f; cam.nearClipPlane = 0.05f;
            float d = b.extents.magnitude * 2.1f;
            Vector3 view = objectName.StartsWith("Nido") ? new Vector3(0.5f, -0.25f, -1f) : new Vector3(0.6f, 0.55f, -1f);   // los panales cuelgan: se miran desde abajo
            cam.transform.position = b.center + (go.transform.rotation * view).normalized * d;
            cam.transform.LookAt(b.center);
            var rt = new RenderTexture(1100, 700, 24); cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1100, 700, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1100, 700), 0, 0); tex.Apply();
            RenderTexture.active = null;
            string dir = Path.Combine(Application.dataPath, "..", "Temp", "PoseGallery"); Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, $"Foto_{objectName}.png"), tex.EncodeToPNG());
            Object.DestroyImmediate(tex); rt.Release();
            Debug.Log($"[Foto] Temp/PoseGallery/Foto_{objectName}.png");
        }
        finally { Object.DestroyImmediate(camGO); }
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
