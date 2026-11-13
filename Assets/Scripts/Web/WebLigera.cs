using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// LA WEB LIGERA (2026-11-13; Berón: dos versiones WebGL ligeras para abrir en el móvil). Se activa con el símbolo de compilación
/// <c>WEB_LIGERA</c>, que pone solo el menú «Tools/Cold Sanctuary/Web/Build WebGL: …» (WebLigeraBuild, en el build y nada más: el editor y
/// el Play siguen igual). Dos sabores:
///  · «Santuario ligero» (<c>WEB_LIGERA</c>): SampleScene, solo el santuario 1, con Kushal ya miembro (sin el examen ni las iniciaciones,
///    como la casilla «Kushal iniciado» de Inicio); ni el santuario marino ni el aéreo se cargan (sus escenas no van en el build).
///  · «Microcosmos ligero» (<c>WEB_LIGERA</c> + <c>WEB_LIGERA_MICRO</c>): lo mismo y, al arrancar, Kushal entra directo en el microcosmos
///    <see cref="MicroScene"/> (Siddhārtha en Kapilavastu); la fauna del santuario, de fondo, se apaga mientras.
/// En los dos: como mucho <see cref="AnimalCap"/> animales en el santuario (los que sobran de K × humanos se quitan al cargar, y el plan no
/// deja criar más), los insectos fuera (ya lo están: <see cref="InsectPurge"/>), sin sombras, sesgo de LOD bajo, media resolución opcional,
/// 30 fps, sin el orquestador de pruebas, y controles táctiles en pantalla si el dispositivo es táctil (<see cref="TouchControls"/>).
/// </summary>
public static class WebLigera
{
#if WEB_LIGERA
    public const bool Active = true;
#else
    public const bool Active = false;
#endif
#if WEB_LIGERA_MICRO
    public const bool Micro = true;
#else
    public const bool Micro = false;
#endif
    /// <summary>El microcosmos de la web «Microcosmos ligero»: Siddhārtha en Kapilavastu (el más completo: historia, paseo por su NavMesh,
    /// mapa a mano, jornada con franjas libres y charlas).</summary>
    public const string MicroScene = "Microcosmos_Buda";
    /// <summary>Tope duro de animales en el santuario (en lugar de K × humanos, ~90).</summary>
    public const int AnimalCap = 30;
    public const float LodBias = 0.4f;
    public const int TargetFps = 30;

    /// <summary>¿Hay que poner los controles táctiles? (móvil o pantalla táctil)</summary>
    public static bool Touch => Application.isMobilePlatform || Input.touchSupported;

    /// <summary>Media resolución (render scale 0,5 del URP): por defecto en el móvil; se cambia con el botón «½» de la esquina.</summary>
    public static bool HalfRes
    {
        get => PlayerPrefs.GetInt("WebLigera.MediaRes", Application.isMobilePlatform ? 1 : 0) == 1;
        set { PlayerPrefs.SetInt("WebLigera.MediaRes", value ? 1 : 0); ApplyPipeline(); }
    }

#if WEB_LIGERA
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Early()
    {
        PopulationPlan.AnimalCap = AnimalCap;
        PopulationPlan.InsectsInSanctuary1 = false;
        Application.targetFrameRate = TargetFps;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        ApplyQuality();
        SceneManager.sceneLoaded -= Loaded;
        SceneManager.sceneLoaded += Loaded;
        for (int i = 0; i < SceneManager.sceneCount; i++) Lighten(SceneManager.GetSceneAt(i));
        // Sin el orquestador de pruebas (en un build ya está apagado: solo corre en el editor si se piden; aquí, fuera del todo).
        foreach (ColdSanctuary.TestRunner tr in Object.FindObjectsByType<ColdSanctuary.TestRunner>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.Destroy(tr.gameObject);
        // Kushal, ya MIEMBRO antes del primer Update (ningún iniciador lo coge); el resto de «miembro» (cuerpo y cámara), unos fotogramas después.
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && p.TryGetComponent(out SanctuaryOnboarding ob)) { ob.introMissionsDone = ob.introMissionsRequired; ob.apt = ob.presented = ob.initiated = ob.kitchenTrained = ob.gardenTrained = true; }
        if (p != null && p.TryGetComponent(out HealthRecord hr)) { hr.analyzed = true; hr.vaccinated = true; }
        var go = new GameObject("WebLigera");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<WebLigeraRunner>();
        if (Touch) TouchControls.Ensure();
        Debug.Log($"[Web] Web ligera{(Micro ? " (microcosmos: " + MicroScene + ")" : "")}: tope de {AnimalCap} animales, sin sombras, LOD {LodBias}, {TargetFps} fps{(Touch ? ", controles táctiles" : "")}.");
    }

    static void Loaded(Scene s, LoadSceneMode m) => Lighten(s);
#endif

    /// <summary>Calidad barata para el móvil: sin sombras, LOD bajo, sin antialias ni reflejos en tiempo real, y el URP en una copia ligera.</summary>
    public static void ApplyQuality()
    {
        QualitySettings.lodBias = LodBias;
        QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
        QualitySettings.shadowCascades = 1;
        QualitySettings.antiAliasing = 0;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.softParticles = false;
        QualitySettings.pixelLightCount = 1;
        ApplyPipeline();
    }

    static UniversalRenderPipelineAsset _light;
    /// <summary>El URP de la web: una COPIA (como ShadowQuality) sin sombras, 1 cascada, sin MSAA ni HDR, y a media resolución si se pide.</summary>
    static void ApplyPipeline()
    {
        if (_light == null)
        {
            if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset src)) return;
            _light = Object.Instantiate(src);
            _light.name = src.name + " (web ligera)";
            _light.shadowDistance = 0f;
            _light.shadowCascadeCount = 1;
            _light.msaaSampleCount = 1;
            _light.supportsHDR = false;
            QualitySettings.renderPipeline = _light;
        }
        _light.renderScale = HalfRes ? 0.5f : 1f;
    }

    /// <summary>Una escena cargada, a la medida de la web: ninguna luz con sombra; en el santuario 1, como mucho <see cref="AnimalCap"/> animales.</summary>
    public static void Lighten(Scene s)
    {
        if (!s.IsValid() || !s.isLoaded) return;
        foreach (GameObject root in s.GetRootGameObjects())
            foreach (Light l in root.GetComponentsInChildren<Light>(true)) l.shadows = LightShadows.None;
        if (InsectPurge.IsSanctuary1(s)) TrimFauna(s, AnimalCap);
    }

    /// <summary>Quita los animales del plan que sobran de <paramref name="cap"/> en esa escena, por igual entre especies (cada una guarda su
    /// parte, y al menos una pareja si la tenía). Devuelve cuántos quitó.</summary>
    public static int TrimFauna(Scene s, int cap)
    {
        var bySpecies = new Dictionary<string, List<Animal>>();
        int total = 0;
        foreach (GameObject root in s.GetRootGameObjects())
            foreach (Animal a in root.GetComponentsInChildren<Animal>(true))
            {
                if (a == null || a.death || a.SpeciesName == null || !PopulationPlan.Groups.ContainsKey(a.SpeciesName)) continue;
                if (!bySpecies.TryGetValue(a.SpeciesName, out var l)) bySpecies[a.SpeciesName] = l = new List<Animal>();
                l.Add(a); total++;
            }
        if (total <= cap) return 0;
        // La parte de cada especie (proporcional, al menos 2 si tenía 2); si aún sobra, se quita de las más numerosas.
        var keep = new Dictionary<string, int>();
        int kept = 0;
        foreach (var kv in bySpecies) { int k = Mathf.Min(kv.Value.Count, Mathf.Max(Mathf.Min(2, kv.Value.Count), Mathf.FloorToInt(kv.Value.Count * (float)cap / total))); keep[kv.Key] = k; kept += k; }
        var order = new List<string>(bySpecies.Keys);
        int guard = 0;
        while (kept > cap && guard++ < 1000)
        {
            order.Sort((x, y) => keep[y].CompareTo(keep[x]));
            if (keep[order[0]] <= 1) break;
            keep[order[0]]--; kept--;
        }
        int removed = 0;
        foreach (var kv in bySpecies)
            for (int i = keep[kv.Key]; i < kv.Value.Count; i++) { Object.Destroy(kv.Value[i].gameObject); removed++; }
        Debug.Log($"[Web] {s.name}: {removed} animales quitados ({total} → {total - removed}; tope de la web, {cap}).");
        return removed;
    }

    // ── LOS SOMBREADORES QUE FALTAN (2026-11-13: un bloque magenta junto al jugador en el build web) ─────────────────────────────────────
    static readonly HashSet<Material> _checked = new HashSet<Material>();
    static readonly HashSet<string> _reported = new HashSet<string>();
    static Shader _fallback;
    static readonly string[] FallbackNames = { "Universal Render Pipeline/Simple Lit", "Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit", "Unlit/Color" };

    /// <summary>¿Se pinta en magenta? (sin sombreador, sombreador que esta GPU no soporta, o el de error).</summary>
    public static bool Broken(Shader sh) => sh == null || !sh.isSupported || sh.name == "Hidden/InternalErrorShader";

    /// <summary>Busca los renderers activos con un material que sale MAGENTA. REGISTRA cada uno (material, sombreador y dónde, una vez; así se
    /// sabe qué sombreador añadir a WebLigeraShaders) y le pone el primer sombreador bueno de <see cref="FallbackNames"/>, con su color.
    /// Devuelve cuántos materiales arregló.</summary>
    public static int FixBrokenShaders(string when)
    {
        if (_fallback == null) foreach (string n in FallbackNames) { Shader s = Shader.Find(n); if (!Broken(s)) { _fallback = s; break; } }
        int fixedN = 0;
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material m = mats[i];
                if (m == null || !_checked.Add(m)) continue;
                if (!Broken(m.shader)) continue;
                string sn = m.shader != null ? m.shader.name : "(ninguno)";
                if (_reported.Add(sn + "|" + m.name)) Debug.LogWarning($"[Web] MAGENTA ({when}): material «{m.name}» con el sombreador «{sn}» (soportado: {(m.shader != null && m.shader.isSupported)}) en «{Path(r.transform)}». Añádelo a WebLigeraShaders.");
                if (_fallback == null) continue;
                Color c = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.HasProperty("_Color") ? m.GetColor("_Color") : Color.gray;
                m.shader = _fallback;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                fixedN++;
            }
        }
        if (fixedN > 0) Debug.Log($"[Web] {fixedN} materiales magenta ({when}) pasados a «{(_fallback != null ? _fallback.name : "—")}».");
        return fixedN;
    }

    static string Path(Transform t) { string p = t.name; for (Transform q = t.parent; q != null; q = q.parent) p = q.name + "/" + p; return p; }
}

/// <summary>Lo de la web ligera que va por fotogramas: hacer miembro a Kushal, entrar en el microcosmos y el botón de resolución.</summary>
public class WebLigeraRunner : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;
        yield return null;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        string r = SpellConsole.MakeMember(p);
        Debug.Log("[Web] " + (r ?? "Kushal ya es del santuario."));
        StartCoroutine(MagentaWatch());
        if (!WebLigera.Micro) yield break;
        // EL MICROCOSMOS: directo dentro, como «Inicio → microcosmos» (SceneLauncher.EnterMicro). Espera acotada al jugador.
        float until = Time.realtimeSinceStartup + 10f;
        while (GameObject.FindGameObjectWithTag("Player") == null && Time.realtimeSinceStartup < until) yield return null;
        if (!Application.CanStreamedLevelBeLoaded(WebLigera.MicroScene)) { Debug.LogWarning($"[Web] {WebLigera.MicroScene} no está en el build."); yield break; }
        MobWorldLoader.Instance.EnterMobWorld(WebLigera.MicroScene);
        until = Time.realtimeSinceStartup + 120f;
        while (!MobWorldLoader.Instance.IsInMobWorld && Time.realtimeSinceStartup < until) yield return null;
        if (!MobWorldLoader.Instance.IsInMobWorld) { Debug.LogWarning($"[Web] {WebLigera.MicroScene}: no se entró en 120 s."); yield break; }
        // Mientras Kushal está dentro, la fauna del santuario (de fondo, lejos) no hace falta: se apaga, y vuelve al salir.
        StartCoroutine(SanctuaryFaunaWhileInside());
    }

    /// <summary>Lo magenta, al arrancar y unas veces más (muchos materiales se hacen en juego, más tarde: marcadores, cultivos, el microcosmos);
    /// luego cada minuto. Cada material se mira una sola vez.</summary>
    IEnumerator MagentaWatch()
    {
        foreach (float wait in new[] { 0f, 3f, 10f, 30f })
        {
            if (wait > 0f) yield return new WaitForSecondsRealtime(wait);
            WebLigera.FixBrokenShaders($"a los {Time.realtimeSinceStartup:0} s");
        }
        while (true) { yield return new WaitForSecondsRealtime(60f); WebLigera.FixBrokenShaders($"a los {Time.realtimeSinceStartup:0} s"); }
    }

    IEnumerator SanctuaryFaunaWhileInside()
    {
        var off = new List<GameObject>();
        Scene micro = SceneManager.GetSceneByName(WebLigera.MicroScene);
        foreach (GameObject g in new List<GameObject>(Animal.wholePopulation)) if (g != null && g.activeSelf && g.scene != micro) { g.SetActive(false); off.Add(g); }
        Debug.Log($"[Web] Dentro de {WebLigera.MicroScene}: {off.Count} animales del santuario en pausa.");
        while (MobWorldLoader.Instance.IsInMobWorld) yield return new WaitForSecondsRealtime(1f);
        foreach (GameObject g in off) if (g != null) g.SetActive(true);
    }

    GUIStyle _s;
    int _fontFor = -1;

    void OnGUI()
    {
        // TEXTO LEGIBLE EN EL MÓVIL: la piel IMGUI por defecto (compartida por todos los paneles del juego que no fijan su tamaño) a ~3,5 % del
        // lado corto de la pantalla (el lienzo va a ≤ 1,5 píxeles por píxel CSS: la plantilla WebLigera). Se rehace al girar el móvil.
        float shortSide = Mathf.Min(Screen.width, Screen.height);
        int fs = Mathf.Max(14, Mathf.RoundToInt(shortSide * 0.035f));
        if (fs != _fontFor)
        {
            _fontFor = fs;
            foreach (GUIStyle st in new[] { GUI.skin.label, GUI.skin.button, GUI.skin.box, GUI.skin.toggle, GUI.skin.textField, GUI.skin.textArea, GUI.skin.window })
                if (st != null) st.fontSize = fs;
            _s = new GUIStyle(GUI.skin.button) { fontSize = fs };
        }
        // El botón de resolución, arriba a la izquierda (encima de la pila de ventanas de HudModals; abajo están los joysticks).
        float h = Mathf.Max(32f, shortSide * 0.07f), w = h * 2.2f;
        if (GUI.Button(new Rect(8f, 8f, w, h), WebLigera.HalfRes ? "½ res" : "1× res", _s)) WebLigera.HalfRes = !WebLigera.HalfRes;
    }
}
