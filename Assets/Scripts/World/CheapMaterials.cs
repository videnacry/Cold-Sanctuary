using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MATERIALES BARATOS (Berón 2026-11-09, rendimiento): ni iridiscencia ni brillo en ningún animal, insecto o estructura. Todo opaco, con
/// URP Simple Lit, suavidad baja (<see cref="Smoothness"/>), sin brillos especulares ni reflejos del entorno, y COMPARTIDO por color (no
/// uno por objeto). La única excepción es el CRISTAL (y el agua de una pecera): las paredes de cristal son la única pared permitida
/// porque no tapan la vista (MicroBuildRules, GlassWall), así que siguen transparentes, pero mates y con un solo material por color.
/// </summary>
public static class CheapMaterials
{
    public const float Smoothness = 0.1f;
    const string LitName = "Universal Render Pipeline/Lit", SimpleName = "Universal Render Pipeline/Simple Lit";

    static Shader _simple;
    public static Shader Simple => _simple != null ? _simple : (_simple = Shader.Find(SimpleName) ?? WebShader() ?? Shader.Find(LitName) ?? Shader.Find("Standard"));
    /// <summary>(2026-11-13, web ligera) Si Shader.Find no lo encuentra en el build, el del material de Resources/WebLigeraShaders (siempre va).</summary>
    static Shader WebShader() { Material m = Resources.Load<Material>("WebLigeraShaders/URP_SimpleLit_opaco"); return m != null && m.shader != null && m.shader.isSupported ? m.shader : null; }

    static readonly Dictionary<Color, Material> _byColor = new Dictionary<Color, Material>();

    /// <summary>El material opaco y mate de ese color (uno por color para todo el juego).</summary>
    public static Material Opaque(Color c, string prefix = "Mate")
    {
        if (_byColor.TryGetValue(c, out Material m) && m != null) return m;
        m = new Material(Simple) { color = c, name = $"{prefix}_{ColorUtility.ToHtmlStringRGBA(c)}" };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (c.a < 0.99f) MakeTransparent(m);
        return _byColor[c] = MakeCheap(m);
    }

    /// <summary>Pasa un material a barato SIN cambiarle el color: de Lit o Standard a Simple Lit, suavidad baja, sin brillo ni reflejos. Si era
    /// transparente (cristal, agua), sigue transparente, pero mate.</summary>
    public static Material MakeCheap(Material m)
    {
        if (m == null) return null;
        string sn = m.shader != null ? m.shader.name : "";
        bool transparent = m.HasProperty("_Surface") && m.GetFloat("_Surface") > 0.5f;
        if (sn == LitName || sn == "Standard")
        {
            Color c = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
            Shader s = Simple;
            if (s != null && s != m.shader) { m.shader = s; m.color = c; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c); if (transparent) MakeTransparent(m); }
        }
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", Smoothness);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", Smoothness);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
        if (m.HasProperty("_SpecColor")) m.SetColor("_SpecColor", Color.black);
        if (m.HasProperty("_SpecularHighlights")) m.SetFloat("_SpecularHighlights", 0f);
        if (m.HasProperty("_EnvironmentReflections")) m.SetFloat("_EnvironmentReflections", 0f);
        if (m.HasProperty("_ClearCoat")) m.SetFloat("_ClearCoat", 0f);
        m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF"); m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        m.DisableKeyword("_SPECULAR_COLOR"); m.DisableKeyword("_SPECGLOSSMAP"); m.DisableKeyword("_CLEARCOAT");
        return m;
    }

    static void MakeTransparent(Material m)
    {
        if (!m.HasProperty("_Surface")) return;
        m.SetFloat("_Surface", 1f); m.SetFloat("_Blend", 0f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    /// <summary>(2026-11-11) El agua (el mar del horizonte, el agua de los troncos): puede ser transparente aunque su color sea opaco (lo pone la textura).</summary>
    public static bool IsWater(Material m) => m != null && (m.name.StartsWith("Mar_") || m.name.StartsWith("Agua") || m.name.Contains("_agua") || m.name.Contains("Water")
        || m.name.Contains("cristal") || m.name.Contains("Cristal") || m.name.Contains("Glass") || m.name.Contains("Vidrio") || m.name.Contains("vidrio"));   // (ronda 73: y el CRISTAL, «Arbol_cristal», que la pasada de assets dejaba opaco)

    /// <summary>¿Es un material «caro» de los que pide quitar Berón? (URP Lit o Standard, suavidad &gt; 0,3, metálico o con capa clara; o
    /// transparente sin ser cristal o agua: alfa del color ≥ 0,99).</summary>
    public static string Expensive(Material m)
    {
        if (m == null || m.shader == null) return null;
        string sn = m.shader.name;
        if (sn != LitName && sn != SimpleName && sn != "Standard") return null;   // (partículas, cielo, texto, sin luz: fuera de esta regla)
        if (sn != SimpleName) return "sombreador " + sn;
        if (m.HasProperty("_Smoothness") && m.GetFloat("_Smoothness") > 0.2f) return $"suavidad {m.GetFloat("_Smoothness"):0.00}";   // (2026-11-11: 0,2, la de la petición #152; antes 0,3)
        if (m.HasProperty("_Metallic") && m.GetFloat("_Metallic") > 0.05f) return "metálico";
        bool transparent = m.HasProperty("_Surface") && m.GetFloat("_Surface") > 0.5f;
        Color c = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
        if (transparent && c.a >= 0.99f && !IsWater(m)) return "transparente sin ser cristal";
        return null;
    }

    /// <summary>LA RED: al cargar cada escena, todo material de sus renderers que no sea un ASSET del proyecto pasa a barato (los hechos al
    /// construir la escena y los creados en juego). Los assets no se tocan desde el juego (en el editor se guardarían solos): esos los cambian
    /// los constructores al reconstruir.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnLoad()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= Loaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += Loaded;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++) CheapenScene(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
    }
    static void Loaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m) => CheapenScene(s, true);   // (con los dueños en el registro: ronda 72)

    public static int Cheapened { get; private set; }
    public static int CheapenScene(UnityEngine.SceneManagement.Scene s) => CheapenScene(s, false);
    /// <summary><paramref name="tellOwners"/>: dice de quién eran los que convierte (el barrido de después de cargar: ronda 72, «Lit» y «Lit (Instance)» sin dueño conocido).</summary>
    public static int CheapenScene(UnityEngine.SceneManagement.Scene s, bool tellOwners)
    {
        if (!s.IsValid() || !s.isLoaded) return 0;
        var done = new HashSet<Material>(); int n = 0; var owners = tellOwners ? new List<string>() : null;
        foreach (GameObject root in s.GetRootGameObjects())
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null || !done.Add(m) || Expensive(m) == null) continue;
                    if (owners != null && owners.Count < 12) owners.Add($"{m.name} en {Path(r.transform)}");
#if UNITY_EDITOR
                    if (UnityEditor.EditorUtility.IsPersistent(m)) { ReplaceWithClone(r, m); n++; continue; }   // (2026-11-11) un asset que no se puede tocar (los de dentro de un FBX: «Skin», «Teshirt» de Kushal): en este renderer, una copia mate
#endif
                    MakeCheap(m); n++;
                }
        Cheapened += n;
        if (n > 0) Debug.Log($"[Rendimiento] {s.name}: {n} materiales pasados a Simple Lit mate (de {done.Count} distintos){(owners != null && owners.Count > 0 ? " — " + string.Join("; ", owners) : "")}.");
        return n;
    }

    static string Path(Transform t) { string p = t.name; for (Transform u = t.parent; u != null && p.Length < 120; u = u.parent) p = u.name + "/" + p; return p; }

    // (2026-11-11, #152) COPIAS MATES de los assets que no se pueden cambiar (los materiales de dentro de un modelo FBX): una por material.
    static readonly Dictionary<Material, Material> _clones = new Dictionary<Material, Material>();
    static void ReplaceWithClone(Renderer r, Material m)
    {
        if (!_clones.TryGetValue(m, out Material c) || c == null) { c = new Material(m) { name = m.name }; MakeCheap(c); _clones[m] = c; }
        Material[] ms = r.sharedMaterials;
        for (int i = 0; i < ms.Length; i++) if (ms[i] == m) ms[i] = c;
        r.sharedMaterials = ms;
    }

    /// <summary>(2026-11-11, #152) LAS COPIAS DE DESPUÉS: los materiales que se crean en juego al vestir o colorear a la gente y a los animales
    /// («Lit (Instance)», «Gris», «DarkGreen» de la ronda 70) no existían al cargar la escena. Cada 15 s se pasan a mate los de los cuerpos (Anima).</summary>
    static readonly HashSet<Anima> _bodiesDone = new HashSet<Anima>();
    public static int CheapenBodies(bool onlyNew = false)
    {
        int n = 0;
        foreach (Anima a in Object.FindObjectsByType<Anima>(FindObjectsSortMode.None))
        {
            if (a == null) continue;
            if (onlyNew && !_bodiesDone.Add(a)) continue;   // (cada cuerpo, una vez: sus materiales ya quedaron mates)
            foreach (Renderer r in a.GetComponentsInChildren<Renderer>(true))
                foreach (Material m in r.sharedMaterials)
                {
                    if (m == null || Expensive(m) == null) continue;
#if UNITY_EDITOR
                    if (UnityEditor.EditorUtility.IsPersistent(m)) { ReplaceWithClone(r, m); n++; continue; }
#endif
                    MakeCheap(m); n++;
                }
        }
        Cheapened += n;
        return n;
    }

    /// <summary>Materiales distintos en los renderers activos de las escenas cargadas, y cuántos son «caros».</summary>
    public static (int unique, int expensive, string worst) Census()
    {
        var seen = new HashSet<Material>(); int bad = 0; string worst = "";
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            foreach (Material m in r.sharedMaterials)
            {
                if (m == null || !seen.Add(m)) continue;
                string why = Expensive(m);
                if (why != null) { bad++; if (worst.Length < 240) worst += $"{m.name} ({why}); "; }
            }
        return (seen.Count, bad, worst);
    }
}
