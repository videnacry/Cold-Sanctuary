using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// LOS BUILDS WEBGL LIGEROS (2026-11-13; Berón: abrirlos en el móvil). Dos menús, cada uno con SOLO sus escenas y el símbolo
/// <c>WEB_LIGERA</c> (y <c>WEB_LIGERA_MICRO</c>) puesto SOLO en ese build (<see cref="BuildPlayerOptions.extraScriptingDefines"/>: el editor
/// y el Play no cambian). Lo que hace el juego con ellos: <see cref="WebLigera"/> (Kushal miembro, ≤ 30 animales, sin sombras, controles
/// táctiles…). Ajustes de jugador WebGL: sin compresión (vale cualquier servidor estático), sin «decompression fallback», 512 MB de memoria
/// al empezar (hasta 1 GB), stripping alto, IL2CPP para tamaño (Master), sin el código del motor que no se usa, WebGL 2.
/// Sale a <c>Builds/Web/&lt;nombre&gt;/</c> (junto a Assets) y registra su tamaño. Necesita el módulo WebGL instalado en el Hub.
/// </summary>
public static class WebLigeraBuild
{
    const string Root = "Builds/Web";
    const string Sanctuary = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Cold Sanctuary/Web/Build WebGL: Santuario ligero")]
    public static void BuildSantuario() => Build("SantuarioLigero", new[] { Sanctuary }, new[] { "WEB_LIGERA" });

    [MenuItem("Tools/Cold Sanctuary/Web/Build WebGL: Microcosmos ligero")]
    public static void BuildMicrocosmos() => Build("MicrocosmosLigero", new[] { Sanctuary, "Assets/Scenes/" + WebLigera.MicroScene + ".unity" }, new[] { "WEB_LIGERA", "WEB_LIGERA_MICRO" });

    static void Build(string name, string[] scenes, string[] defines)
    {
        foreach (string s in scenes)
            if (!File.Exists(s)) { Debug.LogError($"[Web] Falta {s}: genérala antes (Build Sample Scene / su menú de microcosmos)."); return; }
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL)) { Debug.LogError("[Web] El módulo WebGL no está instalado (Unity Hub → Installs → Add modules → WebGL Build Support)."); return; }
        ApplyPlayerSettings();
        EnsureLinkXml();
        EnsureRuntimeShaders();
        EnsureTemplate();
        string outDir = Path.Combine(Root, name);
        var opts = new BuildPlayerOptions
        {
            scenes = scenes, locationPathName = outDir, target = BuildTarget.WebGL, targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None, extraScriptingDefines = defines,
        };
        Debug.Log($"[Web] Build «{name}»: {string.Join(", ", scenes)} con {string.Join(";", defines)} → {outDir}");
        BuildReport r = BuildPipeline.BuildPlayer(opts);
        long bytes = DirSize(outDir);
        if (r.summary.result == BuildResult.Succeeded)
            Debug.Log($"[Web] «{name}» listo en {Path.GetFullPath(outDir)}: {bytes / (1024f * 1024f):0.0} MB en disco (sin comprimir), {r.summary.totalTime.TotalMinutes:0.0} min. Súbelo tal cual a un servidor estático y abre index.html.");
        else Debug.LogError($"[Web] «{name}» falló: {r.summary.result}, {r.summary.totalErrors} errores (mira la consola).");
    }

    /// <summary>Los ajustes de jugador WebGL para el móvil (solo tocan la plataforma WebGL).</summary>
    public static void ApplyPlayerSettings()
    {
        NamedBuildTarget nt = NamedBuildTarget.WebGL;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;   // sin Brotli ni gzip: cualquier servidor estático
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.initialMemorySize = 512;
        PlayerSettings.WebGL.maximumMemorySize = 1024;
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.nameFilesAsHashes = false;
        PlayerSettings.SetScriptingBackend(nt, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(nt, ManagedStrippingLevel.High);
        PlayerSettings.SetIl2CppCodeGeneration(nt, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetIl2CppCompilerConfiguration(nt, Il2CppCompilerConfiguration.Master);
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES3 });   // WebGL 2
        Debug.Log("[Web] Ajustes WebGL: sin compresión, sin fallback, 512→1024 MB, stripping alto, IL2CPP para tamaño (Master), WebGL 2.");
    }

    /// <summary>Con stripping alto, el código del juego se guarda entero (usa reflexión: arranques, AddComponent por tipo…); se recortan el
    /// motor y las bibliotecas. El repo solo versiona .cs: el link.xml se escribe en el proyecto al construir.</summary>
    static void EnsureLinkXml()
    {
        const string path = "Assets/link.xml";
        if (File.Exists(path)) return;
        File.WriteAllText(path, "<linker>\n  <!-- (web ligera) el juego usa reflexión: su ensamblado entero; el resto, con stripping alto -->\n  <assembly fullname=\"Assembly-CSharp\" preserve=\"all\"/>\n</linker>\n");
        AssetDatabase.ImportAsset(path);
    }

    // ── LOS SOMBREADORES DE LOS MATERIALES HECHOS EN JUEGO (2026-11-13; el cubo magenta del primer build web) ─────────────────────────────
    // Muchos materiales se crean en juego con Shader.Find (CheapMaterials: URP Simple Lit; GardenCrop, CubeTree…: URP Lit; GalaxyPortal,
    // MarHorizonte: URP Unlit; las estelas: Sprites/Default…). Ningún asset del build los referencia, así que el build quita esos sombreadores y
    // salen en magenta. Dos remedios, según el peso:
    //  · los del URP (con miles de variantes): un MATERIAL de cada uno en Resources, con las mismas palabras clave que les pone el juego (mate,
    //    opaco y transparente): el build guarda el sombreador y solo esas variantes;
    //  · los pequeños de siempre (Sprites/Default, Unlit/Color, Unlit/Transparent): a «Always Included Shaders».
    const string ShaderDir = "Assets/Resources/WebLigeraShaders";
    static readonly string[] UrpShaders = { "Universal Render Pipeline/Simple Lit", "Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit", "Universal Render Pipeline/Particles/Unlit" };
    static readonly string[] AlwaysShaders = { "Sprites/Default", "Unlit/Color", "Unlit/Transparent" };

    static void EnsureRuntimeShaders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(ShaderDir)) AssetDatabase.CreateFolder("Assets/Resources", "WebLigeraShaders");
        var made = new System.Collections.Generic.List<string>();
        foreach (string sn in UrpShaders)
        {
            Shader sh = Shader.Find(sn);
            if (sh == null) { Debug.LogWarning($"[Web] No encuentro el sombreador «{sn}»."); continue; }
            string baseName = sn.Replace("Universal Render Pipeline/", "URP_").Replace("/", "_").Replace(" ", "");
            // Opaco y mate (CheapMaterials.MakeCheap pasa Lit a Simple Lit: aquí se le devuelve su sombreador después).
            var opaque = new Material(sh) { name = baseName + "_opaco" };
            CheapMaterials.MakeCheap(opaque); opaque.shader = sh;
            SaveMat(opaque, made);
            // Transparente (el cristal y el agua: CheapMaterials.Opaque con alfa < 1 hace lo mismo).
            var glass = new Material(CheapMaterials.Opaque(new Color(0.6f, 0.8f, 1f, 0.5f), "WebLigera")) { name = baseName + "_transparente" };
            if (glass.shader != sh) { glass.shader = sh; if (glass.HasProperty("_Surface")) { glass.SetFloat("_Surface", 1f); glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); glass.renderQueue = 3000; } }
            SaveMat(glass, made);
            // Tal cual (sin palabras clave del juego): los que crean el material sin pasarlo a barato.
            SaveMat(new Material(sh) { name = baseName + "_base" }, made);
        }
        // «Always Included Shaders» (ProjectSettings/GraphicsSettings.asset, m_AlwaysIncludedShaders).
        var added = new System.Collections.Generic.List<string>();
        Object gs = GraphicsSettings.GetGraphicsSettings();
        if (gs != null)
        {
            var so = new SerializedObject(gs);
            SerializedProperty arr = so.FindProperty("m_AlwaysIncludedShaders");
            foreach (string sn in AlwaysShaders)
            {
                Shader sh = Shader.Find(sn);
                if (sh == null) continue;
                bool has = false;
                for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) { has = true; break; }
                if (has) continue;
                arr.InsertArrayElementAtIndex(arr.arraySize);
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
                added.Add(sn);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[Web] Sombreadores de los materiales hechos en juego: {made.Count} materiales en {ShaderDir} ({string.Join(", ", made)}); " +
                  $"«Always Included Shaders» +{added.Count}{(added.Count > 0 ? " (" + string.Join(", ", added) + ")" : " (ya estaban)")}.");
    }

    static void SaveMat(Material m, System.Collections.Generic.List<string> made)
    {
        string path = $"{ShaderDir}/{m.name}.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);
        made.Add(m.name);
    }

    // ── LA PLANTILLA WEBGL PARA EL MÓVIL (Assets/WebGLTemplates/WebLigera; el repo solo versiona .cs: se escribe al construir) ──────────
    // El lienzo al 100 % de ancho y alto (horizontal y vertical), el meta viewport sin zoom, la densidad de píxeles con tope (1,5: el móvil
    // no pinta a 3×), una barra de carga y un botón de pantalla completa (si el navegador la permite; en el iPhone, Safari no).
    const string TemplateName = "WebLigera";

    static void EnsureTemplate()
    {
        string dir = "Assets/WebGLTemplates/" + TemplateName;
        Directory.CreateDirectory(dir);
        File.WriteAllText(dir + "/index.html", TemplateHtml);
        AssetDatabase.ImportAsset(dir + "/index.html");
        PlayerSettings.WebGL.template = "PROJECT:" + TemplateName;
        Debug.Log($"[Web] Plantilla WebGL: {dir} (lienzo a pantalla completa, viewport del móvil, densidad ≤ 1,5, botón de pantalla completa).");
    }

    const string TemplateHtml = @"<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, height=device-height, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no, viewport-fit=cover"">
  <meta name=""mobile-web-app-capable"" content=""yes"">
  <meta name=""apple-mobile-web-app-capable"" content=""yes"">
  <meta name=""theme-color"" content=""#101418"">
  <title>{{{ PRODUCT_NAME }}}</title>
  <style>
    html, body { margin: 0; padding: 0; width: 100%; height: 100%; overflow: hidden; background: #101418; touch-action: none; overscroll-behavior: none; -webkit-user-select: none; user-select: none; }
    #unity-canvas { position: fixed; left: 0; top: 0; width: 100vw; height: 100vh; height: 100dvh; display: block; background: #101418; outline: none; }
    #loading { position: fixed; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #cfd8dc; font: 16px system-ui, sans-serif; }
    #bar { width: 60vw; max-width: 320px; height: 10px; border-radius: 5px; background: #263238; overflow: hidden; }
    #fill { width: 0; height: 100%; background: #80cbc4; }
    #fs { position: fixed; right: max(8px, env(safe-area-inset-right)); top: 50%; transform: translateY(-50%); width: 44px; height: 44px; border: 0; border-radius: 10px;
          background: rgba(255,255,255,0.18); color: #fff; font-size: 22px; display: none; z-index: 10; }
  </style>
</head>
<body>
  <canvas id=""unity-canvas"" tabindex=""-1""></canvas>
  <div id=""loading""><div id=""bar""><div id=""fill""></div></div><p id=""msg"">Cargando {{{ PRODUCT_NAME }}}…</p></div>
  <button id=""fs"" title=""Pantalla completa"">⛶</button>
  <script>
    var canvas = document.querySelector('#unity-canvas');
    var config = {
      arguments: [],
      dataUrl: 'Build/{{{ DATA_FILENAME }}}',
      frameworkUrl: 'Build/{{{ FRAMEWORK_FILENAME }}}',
#if USE_THREADS
      workerUrl: 'Build/{{{ WORKER_FILENAME }}}',
#endif
#if USE_WASM
      codeUrl: 'Build/{{{ CODE_FILENAME }}}',
#endif
#if MEMORY_FILENAME
      memoryUrl: 'Build/{{{ MEMORY_FILENAME }}}',
#endif
#if SYMBOLS_FILENAME
      symbolsUrl: 'Build/{{{ SYMBOLS_FILENAME }}}',
#endif
      streamingAssetsUrl: 'StreamingAssets',
      companyName: {{{ JSON.stringify(COMPANY_NAME) }}},
      productName: {{{ JSON.stringify(PRODUCT_NAME) }}},
      productVersion: {{{ JSON.stringify(PRODUCT_VERSION) }}},
      devicePixelRatio: Math.min(window.devicePixelRatio || 1, 1.5),
      matchWebGLToCanvasSize: true,
      showBanner: function (msg, type) { console.log(msg); if (type === 'error') document.querySelector('#msg').textContent = msg; }
    };
    var fill = document.querySelector('#fill'), fs = document.querySelector('#fs');
    var root = document.documentElement;
    var canFs = !!(root.requestFullscreen || root.webkitRequestFullscreen);
    var script = document.createElement('script');
    script.src = 'Build/{{{ LOADER_FILENAME }}}';
    script.onload = function () {
      createUnityInstance(canvas, config, function (p) { fill.style.width = (100 * p) + '%'; }).then(function (inst) {
        window.unityInstance = inst;
        document.querySelector('#loading').style.display = 'none';
        if (canFs) {
          fs.style.display = 'block';
          fs.onclick = function () {
            if (document.fullscreenElement || document.webkitFullscreenElement) { (document.exitFullscreen || document.webkitExitFullscreen).call(document); return; }
            var r = (root.requestFullscreen || root.webkitRequestFullscreen).call(root);
            if (r && r.then && screen.orientation && screen.orientation.unlock) r.then(function () { try { screen.orientation.unlock(); } catch (e) {} });
          };
        }
        canvas.focus();
      }).catch(function (m) { document.querySelector('#msg').textContent = m; });
    };
    document.body.appendChild(script);
  </script>
</body>
</html>
";

    static long DirSize(string dir)
    {
        if (!Directory.Exists(dir)) return 0;
        long n = 0;
        foreach (string f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories)) n += new FileInfo(f).Length;
        return n;
    }
}
