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

    static long DirSize(string dir)
    {
        if (!Directory.Exists(dir)) return 0;
        long n = 0;
        foreach (string f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories)) n += new FileInfo(f).Length;
        return n;
    }
}
