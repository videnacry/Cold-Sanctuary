using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static void BuildSantuario() => Single("SantuarioLigero", new[] { Sanctuary }, new[] { "WEB_LIGERA" }, new string[0]);

    [MenuItem("Tools/Cold Sanctuary/Web/Build WebGL: Microcosmos ligero")]
    public static void BuildMicrocosmos() => Single("MicrocosmosLigero", new[] { Sanctuary, "Assets/Scenes/" + WebLigera.DefaultMicroScene + ".unity" }, new[] { "WEB_LIGERA", "WEB_LIGERA_MICRO" }, new[] { WebLigera.DefaultMicroScene });

    /// <summary>Un build suelto (los dos ligeros): prepara, construye y deja el editor en Windows.</summary>
    static void Single(string name, string[] scenes, string[] defines, string[] micros)
    {
        try
        {
            if (!Prepare(scenes)) return;
            BuildOne(name, scenes, defines, Path.Combine(Root, name), micros, out _);
        }
        finally { BackToWindows(); }
    }

    /// <summary>Lo de antes de cada tanda de builds: que estén las escenas y el módulo, los ajustes, link.xml, sombreadores y plantilla.</summary>
    static bool Prepare(string[] scenes)
    {
        foreach (string s in scenes)
            if (!File.Exists(s)) { Debug.LogError($"[Web] Falta {s}: genérala antes (Build Sample Scene / su menú de microcosmos)."); return false; }
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL)) { Debug.LogError("[Web] El módulo WebGL no está instalado (Unity Hub → Installs → Add modules → WebGL Build Support)."); return false; }
        ApplyPlayerSettings();
        EnsureLinkXml();
        EnsureRuntimeShaders();
        EnsureTemplate();
        return true;
    }

    /// <summary>Un build WebGL de <paramref name="scenes"/> en <paramref name="outDir"/>, con su manifiesto de microcosmos
    /// (Resources/WebEscenas.txt: lo lee <see cref="WebLigera.Escenas"/>). Devuelve si salió y el tamaño de su .data (MB).</summary>
    static bool BuildOne(string name, string[] scenes, string[] defines, string outDir, string[] micros, out float dataMB) => BuildOne(name, scenes, defines, outDir, micros, out dataMB, out _);

    /// <summary>Como el otro, y además el tamaño del .data SIN comprimir (<paramref name="rawMB"/>: lo que ocupa en la memoria del navegador).</summary>
    static bool BuildOne(string name, string[] scenes, string[] defines, string outDir, string[] micros, out float dataMB, out float rawMB)
    {
        dataMB = 0f; rawMB = 0f;
        WriteManifest(micros);
        if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        var opts = new BuildPlayerOptions
        {
            scenes = scenes, locationPathName = outDir, target = BuildTarget.WebGL, targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None, extraScriptingDefines = defines,
        };
        Debug.Log($"[Web] Build «{name}»: {scenes.Length} escenas ({string.Join(", ", scenes.Select(Path.GetFileNameWithoutExtension))}) con {string.Join(";", defines)} → {outDir}");
        BuildReport r = BuildPipeline.BuildPlayer(opts);
        EditorUtility.UnloadUnusedAssetsImmediate(); System.GC.Collect();   // (el PC de 16 GB: suelta lo de este build antes del siguiente)
        // Lo de depuración de Burst que Unity deja junto al build (no se sube: «_DoNotShip»).
        foreach (string dir in new[] { outDir, Path.GetDirectoryName(outDir) })
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) foreach (string d in Directory.GetDirectories(dir, "*_BurstDebugInformation_DoNotShip")) { try { Directory.Delete(d, true); } catch (System.Exception) { } }
        long bytes = DirSize(outDir);
        // El .data: «X.data» sin comprimir; «X.data.unityweb» (gzip con descompresión de reserva) o «X.data.gz» comprimido.
        foreach (string f in Directory.Exists(outDir + "/Build") ? Directory.GetFiles(outDir + "/Build", "*.data*") : new string[0])
        {
            dataMB = new FileInfo(f).Length / (1024f * 1024f);
            rawMB = f.EndsWith(".data") ? dataMB : GzipRawMB(f, dataMB);
        }
        if (r.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Web] «{name}» listo en {Path.GetFullPath(outDir)}: {bytes / (1024f * 1024f):0.0} MB en disco ({CompresionLabel}; .data {dataMB:0.0} MB, {rawMB:0.0} MB sin comprimir), {r.summary.totalTime.TotalMinutes:0.0} min.");
            return true;
        }
        Debug.LogError($"[Web] «{name}» falló: {r.summary.result}, {r.summary.totalErrors} errores (mira la consola).");
        return false;
    }

    // ── LA COMPRESIÓN (2026-11-16): gzip con descompresión de reserva (por defecto) o sin comprimir ─────────────────────────────────────────
    // Con gzip y «decompression fallback», el cargador de Unity descomprime él mismo si el servidor no manda Content-Encoding: vale en
    // cualquier servidor estático (GitHub Pages incluido) y los archivos ocupan ~3–4 veces menos (el sitio entero, ~200 MB en vez de ~700),
    // a cambio de descomprimir en el móvil al cargar. Se elige en el menú (se guarda en EditorPrefs) o con «-webCompresion gzip|ninguna».
    public enum Compresion { Gzip, Ninguna }
    const string CompresionPref = "ColdSanctuary.WebCompresion";
    const string MenuGzip = "Tools/Cold Sanctuary/Web/Compresión: gzip (con descompresión de reserva)", MenuNone = "Tools/Cold Sanctuary/Web/Compresión: sin comprimir";

    public static Compresion Modo
    {
        get
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-webCompresion");
            if (i >= 0 && i + 1 < args.Length) return args[i + 1].ToLowerInvariant().StartsWith("n") ? Compresion.Ninguna : Compresion.Gzip;
            return (Compresion)EditorPrefs.GetInt(CompresionPref, (int)Compresion.Gzip);
        }
        set => EditorPrefs.SetInt(CompresionPref, (int)value);
    }
    static string CompresionLabel => Modo == Compresion.Gzip ? "gzip con descompresión de reserva" : "sin comprimir";

    [MenuItem(MenuGzip, false, 100)] static void SetGzip() { Modo = Compresion.Gzip; Debug.Log("[Web] Compresión: gzip con descompresión de reserva."); }
    [MenuItem(MenuGzip, true)] static bool SetGzipCheck() { Menu.SetChecked(MenuGzip, Modo == Compresion.Gzip); return true; }
    [MenuItem(MenuNone, false, 101)] static void SetNone() { Modo = Compresion.Ninguna; Debug.Log("[Web] Compresión: ninguna."); }
    [MenuItem(MenuNone, true)] static bool SetNoneCheck() { Menu.SetChecked(MenuNone, Modo == Compresion.Ninguna); return true; }

    /// <summary>Lo que ocupa un archivo gzip sin comprimir (MB), leyéndolo entero (acotado: si falla, el comprimido × 3,5).</summary>
    static float GzipRawMB(string path, float compMB)
    {
        try
        {
            long n = 0; var buf = new byte[1 << 20];
            using (var fs = File.OpenRead(path))
            using (var gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress))
                for (int k; (k = gz.Read(buf, 0, buf.Length)) > 0;) n += k;
            return n / (1024f * 1024f);
        }
        catch (System.Exception e) { Debug.LogWarning($"[Web] No pude medir {path} sin comprimir ({e.Message}): se estima."); return compMB * 3.5f; }
    }

    /// <summary>El editor, de vuelta a Windows (un build WebGL lo deja en WebGL, y el resto del equipo trabaja en Windows).</summary>
    static void BackToWindows()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64) return;
        bool ok = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        Debug.Log($"[Web] Editor de vuelta a StandaloneWindows64: {(ok ? "hecho" : "NO se pudo (cámbialo a mano en Build Profiles)")}.");
    }

    /// <summary>El manifiesto de microcosmos del build (Resources/WebEscenas.txt, «escena|nombre visible» por línea; vacío en el santuario).</summary>
    static void WriteManifest(string[] micros)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        var reg = Microcosmos().ToDictionary(m => m.scene, m => m.display);
        File.WriteAllText("Assets/Resources/WebEscenas.txt", string.Join("\n", micros.Select(s => s + "|" + (reg.TryGetValue(s, out string d) ? d : Slug(s)))) + "\n");
        AssetDatabase.ImportAsset("Assets/Resources/WebEscenas.txt");
    }

    // ══ LA WEB CONTINUA (2026-11-16; Berón: «una página por microcosmo existente y futuro» y subidas continuas) ═══════════════════════════════
    // UN SITIO entero en Builds/Web/sitio/, listo para copiar a cs-web-pages (tools/web/subir-web.ps1):
    //   santuario/            el Santuario ligero (SampleScene sola)
    //   grupos/gNN/           builds de «santuario + unos cuantos microcosmos» (?escena=<nombre> elige cuál abre)
    //   microcosmos/<nombre>/ una página por microcosmos: redirige a su grupo con ?escena=<nombre> (y pasa ?prueba=1)
    //   index.html            el portal: todos, con «abrir» y «probar»;  escenas.json: la lista (la lee tools/web/probar-web.ps1);  version.txt
    // Los microcosmos salen SOLOS de la carpeta Assets/Scenes (Microcosmos_*, MobWorld_*): uno nuevo aparece sin tocar código; su nombre y su
    // historia, del registro de la Sala de Eras (EraHubSceneBuilder.All) si está.
    // POR QUÉ EN GRUPOS: GitHub Pages no sirve archivos de más de 100 MB (el límite cuenta el archivo tal como se sube: con gzip, el .data
    // COMPRIMIDO), y el .data de un build lleva TODAS sus escenas (63 microcosmos ≈ 1,3 GB de escena en texto ≈ 220 MB de .data sin comprimir).
    // Cada grupo lleva el santuario (el anfitrión: el jugador, la cámara, el reloj) y los microcosmos que quepan con DOS topes:
    //  · el .data tal como se sube, ≤ ~80 MB (si uno sale de más de 90, se parte en dos y se rehace);
    //  · con gzip, además, el .data SIN comprimir ≤ ~200 MB: es lo que el navegador del móvil tiene en memoria (montón de 512→1024 MB).
    // Con gzip caben 2 grupos (3 builds con el santuario); sin comprimir, 5–6 (6–7 builds). Las medidas reales afinan el plan de la vez siguiente.
    const string Site = Root + "/sitio";
    const string PlanFile = Root + "/plan-web.txt";
    const float TargetMB = 80f, LimitMB = 90f, RawTargetMB = 200f, RawLimitMB = 260f;
    const int MaxBuilds = 40;   // (tope de seguridad: partir grupos no puede seguir sin fin)

    /// <summary>Los microcosmos que hay (escena, nombre visible, historia): la carpeta de escenas, con el registro de la Sala de Eras.</summary>
    public static List<(string scene, string display, string story)> Microcosmos()
    {
        var reg = new Dictionary<string, (string display, string story)>();
        foreach (var e in EraHubSceneBuilder.All) reg[e.scene] = (e.display, e.story);
        var l = new List<(string, string, string)>();
        if (!Directory.Exists("Assets/Scenes")) return l;
        foreach (string f in Directory.GetFiles("Assets/Scenes", "*.unity").OrderBy(x => x, System.StringComparer.Ordinal))
        {
            string s = Path.GetFileNameWithoutExtension(f);
            if (!s.StartsWith("Microcosmos_") && !s.StartsWith("MobWorld_")) continue;
            if (System.Array.IndexOf(InicioSceneBuilder.Retired, $"Assets/Scenes/{s}.unity") >= 0) continue;   // (2026-11-17: Enfermería y Veterinaria, retirados: sus .unity viejos siguen en la carpeta)
            l.Add(reg.TryGetValue(s, out var r) ? (s, r.display, r.story) : (s, Slug(s), ""));
        }
        return l;
    }

    /// <summary>El nombre corto de la URL: la escena sin su prefijo («Microcosmos_Buda» → «Buda»).</summary>
    public static string Slug(string scene) => scene.StartsWith("Microcosmos_") ? scene.Substring("Microcosmos_".Length) : scene.StartsWith("MobWorld_") ? scene.Substring("MobWorld_".Length) : scene;

    static float SceneMB(string scene) { var fi = new FileInfo($"Assets/Scenes/{scene}.unity"); return fi.Exists ? fi.Length / (1024f * 1024f) : 0f; }

    /// <summary>El modelo de tamaño, aprendido de los builds anteriores:
    ///  · <c>b</c>: el .data del santuario solo, SIN comprimir (MB);
    ///  · <c>r</c>: MB de .data sin comprimir por MB de escena en texto;
    ///  · <c>z</c>: lo que queda del .data con gzip (comprimido ÷ sin comprimir).
    /// Arranca con lo medido el 2026-11-13 (Santuario ligero 32,2 MB; + Kapilavastu, 58,7 MB de texto → 42,2 MB: r ≈ 0,17) y, para gzip, un
    /// 0,30 prudente (las escenas de cubos se comprimen bien; el primer build con gzip lo mide).</summary>
    struct Modelo { public float b, r, z; }

    static Modelo LoadPlan()
    {
        var m = new Modelo { b = 33f, r = 0.17f, z = 0.30f };
        var ci = System.Globalization.CultureInfo.InvariantCulture; var st = System.Globalization.NumberStyles.Float;
        if (File.Exists(PlanFile))
            foreach (string line in File.ReadAllLines(PlanFile))
            {
                string[] kv = line.Split('=');
                if (kv.Length != 2) continue;
                if (kv[0] == "base") float.TryParse(kv[1], st, ci, out m.b);
                if (kv[0] == "ratio") float.TryParse(kv[1], st, ci, out m.r);
                if (kv[0] == "gzip") float.TryParse(kv[1], st, ci, out m.z);
            }
        return m;
    }

    static void SavePlan(Modelo m, IEnumerable<string> lines)
    {
        Directory.CreateDirectory(Root);
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        File.WriteAllLines(PlanFile, new[] { "base=" + m.b.ToString("0.0", ci), "ratio=" + m.r.ToString("0.000", ci), "gzip=" + m.z.ToString("0.000", ci) }.Concat(lines));
    }

    /// <summary>Lo que se sube (comprimido o no, según <see cref="Modo"/>) y lo que ocupa en memoria, para el santuario más esas escenas.</summary>
    static (float up, float raw) Estimate(Modelo m, IEnumerable<string> scenes)
    {
        float raw = m.b + scenes.Sum(s => SceneMB(s) * m.r);
        return (Modo == Compresion.Gzip ? raw * m.z : raw, raw);
    }

    /// <summary>Los grupos: en orden de nombre (uno nuevo suele caer en el último), llenando cada uno hasta los dos topes.</summary>
    static List<List<string>> Pack(List<string> scenes, Modelo m)
    {
        var groups = new List<List<string>>(); var cur = new List<string>();
        foreach (string s in scenes)
        {
            var e = Estimate(m, cur.Concat(new[] { s }));
            bool full = e.up > TargetMB || (Modo == Compresion.Gzip && e.raw > RawTargetMB);
            if (cur.Count > 0 && full) { groups.Add(cur); cur = new List<string>(); }
            cur.Add(s);
        }
        if (cur.Count > 0) groups.Add(cur);
        return groups;
    }

    /// <summary>~MB del .wasm de un build (medido sin comprimir: 34,5; con gzip, ~8).</summary>
    static float WasmMB => Modo == Compresion.Gzip ? 8f : 35f;

    [MenuItem("Tools/Cold Sanctuary/Web/Web continua: ver el plan (sin construir)")]
    public static void ShowPlan()
    {
        Modelo m = LoadPlan();
        var micro = Microcosmos();
        var groups = Pack(micro.Select(x => x.scene).ToList(), m);
        float sanct = Estimate(m, new string[0]).up, total = sanct + groups.Sum(g => Estimate(m, g).up) + (groups.Count + 1) * WasmMB;
        Debug.Log($"[Web] Plan de la web continua ({CompresionLabel}): {micro.Count} microcosmos en {groups.Count} grupos + el santuario = {groups.Count + 1} builds. " +
                  $"Modelo: santuario {m.b:0.0} MB, {m.r:0.000} MB de .data por MB de escena, gzip deja el {m.z * 100f:0} %. Estimado: ~{total:0} MB a subir.\n" +
                  string.Join("\n", groups.Select((g, i) => { var e = Estimate(m, g); return $"  g{i + 1:00}: ~{e.up:0} MB a subir, ~{e.raw:0} MB en memoria · {string.Join(", ", g.Select(Slug))}"; })));
    }

    [MenuItem("Tools/Cold Sanctuary/Web/Web continua: construir todo (santuario + todos los microcosmos)")]
    public static void BuildContinuaMenu() => BuildContinua();

    /// <summary>Para la línea de comandos: <c>Unity.exe -batchmode -quit -projectPath … -executeMethod WebLigeraBuild.BuildContinuaCI
    /// [-webCompresion gzip|ninguna]</c> (con el editor CERRADO: el proyecto no se puede abrir dos veces). Sale con 0 si todo salió.</summary>
    public static void BuildContinuaCI() { bool ok = false; try { ok = BuildContinua(); } finally { EditorApplication.Exit(ok ? 0 : 1); } }

    /// <summary>EL SITIO ENTERO: el santuario, los grupos (partiendo los que salen grandes) y las páginas. Deja el editor en Windows.</summary>
    public static bool BuildContinua()
    {
        var t0 = System.DateTime.Now;
        var micro = Microcosmos();
        try
        {
            if (!Prepare(new[] { Sanctuary }.Concat(micro.Select(x => $"Assets/Scenes/{x.scene}.unity")).ToArray())) return false;
            Modelo m = LoadPlan();
            Debug.Log($"[Web] Web continua, {CompresionLabel}.");
            if (Directory.Exists(Site)) Directory.Delete(Site, true);
            Directory.CreateDirectory(Site + "/grupos");
            // 1) El santuario solo: también mide la BASE (lo que pesa el anfitrión en cada grupo) y lo que deja gzip.
            if (!BuildOne("santuario", new[] { Sanctuary }, new[] { "WEB_LIGERA" }, Site + "/santuario", new string[0], out float sUp, out float sRaw)) return false;
            if (sRaw > 0f) m.b = sRaw;
            if (Modo == Compresion.Gzip && sRaw > 0f) m.z = Mathf.Clamp(sUp / sRaw * 1.05f, 0.1f, 1f);
            // 2) Los grupos, con una cola: el que sale de más de LimitMB (o, con gzip, de más de RawLimitMB en memoria) se parte en dos.
            var queue = new Queue<List<string>>(Pack(micro.Select(x => x.scene).ToList(), m));
            var done = new List<(string name, List<string> scenes, float up, float raw)>();
            int builds = 1;
            float worstR = 0f, worstZ = 0f;
            while (queue.Count > 0 && builds < MaxBuilds)
            {
                List<string> g = queue.Dequeue();
                string name = $"g{done.Count + 1:00}", tmp = Site + "/grupos/_construyendo";
                builds++;
                bool ok = BuildOne(name, new[] { Sanctuary }.Concat(g.Select(s => $"Assets/Scenes/{s}.unity")).ToArray(), new[] { "WEB_LIGERA", "WEB_LIGERA_MICRO" }, tmp, g.ToArray(), out float up, out float raw);
                float yaml = g.Sum(SceneMB);
                if (ok && yaml > 0f) worstR = Mathf.Max(worstR, (raw - m.b) / yaml);
                if (ok && raw > 0f && Modo == Compresion.Gzip) worstZ = Mathf.Max(worstZ, up / raw);
                bool tooBig = up > LimitMB || (Modo == Compresion.Gzip && raw > RawLimitMB);
                if (ok && tooBig && g.Count > 1)
                {
                    Debug.LogWarning($"[Web] {name}: .data de {up:0.0} MB ({raw:0.0} MB en memoria): se parte en dos y se rehace.");
                    Directory.Delete(tmp, true);
                    var rest = queue.ToList(); queue.Clear();
                    queue.Enqueue(g.Take(g.Count / 2).ToList()); queue.Enqueue(g.Skip(g.Count / 2).ToList());
                    foreach (var x in rest) queue.Enqueue(x);
                    continue;
                }
                if (!ok) { Debug.LogError($"[Web] {name} falló: el sitio queda sin {string.Join(", ", g.Select(Slug))}."); continue; }
                if (up > 100f) Debug.LogError($"[Web] {name}: un solo microcosmos ({g[0]}) da {up:0.0} MB de .data: Pages no lo servirá (> 100 MB).");
                Directory.Move(tmp, $"{Site}/grupos/{name}");
                done.Add((name, g, up, raw));
            }
            if (worstR > 0f) m.r = Mathf.Max(0.05f, worstR * 1.05f);   // la próxima vez, con lo medido (y un 5 % de margen)
            if (worstZ > 0f) m.z = Mathf.Clamp(Mathf.Max(m.z, worstZ * 1.05f), 0.1f, 1f);
            SavePlan(m, done.Select(d => $"{d.name} {d.up:0.0} MB ({d.raw:0.0} en memoria): {string.Join(" ", d.scenes)}"));
            WriteSite(micro, done.Select(d => (d.name, d.scenes)).ToList());
            long bytes = DirSize(Site);
            Debug.Log($"[Web] Web continua lista en {Path.GetFullPath(Site)}: santuario + {done.Count} grupos con {done.Sum(d => d.scenes.Count)} de {micro.Count} microcosmos, " +
                      $"{bytes / (1024f * 1024f):0} MB, {(System.DateTime.Now - t0).TotalMinutes:0} min. Súbela con tools/web/subir-web.ps1.");
            return queue.Count == 0 && done.Sum(d => d.scenes.Count) == micro.Count;
        }
        finally { BackToWindows(); }
    }

    // ── LAS PÁGINAS DEL SITIO ─────────────────────────────────────────────────────────────────────────────────────────────────────────────
    static string Esc(string s) => (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    static string Json(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";

    static void WriteSite(List<(string scene, string display, string story)> micro, List<(string name, List<string> scenes)> groups)
    {
        var groupOf = new Dictionary<string, string>();
        foreach (var g in groups) foreach (string s in g.scenes) groupOf[s] = g.name;
        string stamp = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + " UTC";
        // Una página por microcosmos: redirige a su grupo con ?escena=<nombre> (y lo demás de la URL, p. ej. ?prueba=1).
        foreach (var m in micro)
        {
            if (!groupOf.TryGetValue(m.scene, out string g)) continue;
            string slug = Slug(m.scene), dir = $"{Site}/microcosmos/{slug}";
            Directory.CreateDirectory(dir);
            File.WriteAllText(dir + "/index.html",
                $"<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>{Esc(m.display)}</title>" +
                $"<script>var q = location.search ? '&' + location.search.substring(1) : ''; location.replace('../../grupos/{g}/?escena={System.Uri.EscapeDataString(slug)}' + q + location.hash);</script>" +
                $"</head><body><a href=\"../../grupos/{g}/?escena={System.Uri.EscapeDataString(slug)}\">{Esc(m.display)}</a></body></html>");
        }
        // La lista (la lee el probador) y la versión (la espera el subidor tras el despliegue de Pages).
        var items = new List<string> { $"{{\"nombre\":\"santuario\",\"titulo\":\"Santuario\",\"escena\":\"SampleScene\",\"grupo\":\"santuario\",\"url\":\"santuario/\"}}" };
        foreach (var m in micro)
            if (groupOf.TryGetValue(m.scene, out string g))
                items.Add($"{{\"nombre\":{Json(Slug(m.scene))},\"titulo\":{Json(m.display)},\"historia\":{Json(m.story)},\"escena\":{Json(m.scene)},\"grupo\":{Json(g)},\"url\":{Json("microcosmos/" + Slug(m.scene) + "/")}}}");
        File.WriteAllText(Site + "/escenas.json", "{\"version\":" + Json(stamp) + ",\"escenas\":[\n" + string.Join(",\n", items) + "\n]}\n");
        File.WriteAllText(Site + "/version.txt", stamp + "\n");
        File.WriteAllText(Site + "/.nojekyll", "");
        // El portal.
        var rows = new System.Text.StringBuilder();
        rows.Append("<tr><td><b>Santuario</b></td><td>el santuario 1, con Kushal ya miembro</td><td><a href=\"santuario/\">abrir</a></td><td><a href=\"santuario/?prueba=1\">probar</a></td></tr>\n");
        foreach (var m in micro)
        {
            string slug = Slug(m.scene);
            if (!groupOf.ContainsKey(m.scene)) { rows.Append($"<tr class=\"no\"><td>{Esc(m.display)}</td><td>{Esc(m.story)}</td><td colspan=\"2\">no cupo en el build</td></tr>\n"); continue; }
            rows.Append($"<tr><td><b>{Esc(m.display)}</b><br><small>{Esc(slug)} · {groupOf[m.scene]}</small></td><td>{Esc(m.story)}</td><td><a href=\"microcosmos/{slug}/\">abrir</a></td><td><a href=\"microcosmos/{slug}/?prueba=1\">probar</a></td></tr>\n");
        }
        File.WriteAllText(Site + "/index.html",
            "<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>Cold Sanctuary · web</title>" +
            "<style>body{font:15px system-ui,sans-serif;margin:16px;background:#101418;color:#eceff1}a{color:#80cbc4}table{border-collapse:collapse;width:100%}td{padding:6px;border-bottom:1px solid #263238;vertical-align:top}small{color:#90a4ae}.no{opacity:.5}</style>" +
            $"</head><body><h1>Cold Sanctuary</h1><p>El santuario y cada microcosmos, en su página. «probar» corre la prueba en la página (<code>?prueba=1</code>). Versión: {Esc(stamp)} · {micro.Count} microcosmos.</p>" +
            $"<table>{rows}</table></body></html>");
        Debug.Log($"[Web] Sitio: portal, {groupOf.Count} páginas de microcosmos, escenas.json y version.txt en {Site}.");
    }

    /// <summary>Los ajustes de jugador WebGL para el móvil (solo tocan la plataforma WebGL).</summary>
    public static void ApplyPlayerSettings()
    {
        NamedBuildTarget nt = NamedBuildTarget.WebGL;
        // Gzip con descompresión de reserva (vale en cualquier servidor estático) o sin comprimir: WebLigeraBuild.Modo (menú o -webCompresion).
        PlayerSettings.WebGL.compressionFormat = Modo == Compresion.Gzip ? WebGLCompressionFormat.Gzip : WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = Modo == Compresion.Gzip;
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
        Debug.Log($"[Web] Ajustes WebGL: {CompresionLabel}, 512→1024 MB, stripping alto, IL2CPP para tamaño (Master), WebGL 2.");
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
        // (2026-11-13, el bloque magenta junto al jugador: el RefugeMarker de la misión WASD, un CreatePrimitive) El material de las primitivas
        // hechas en juego es el material por defecto del URP; la pasada de CheapMaterials lo pasa a Simple Lit mate. Esa combinación exacta, también.
        Material def = GraphicsSettings.defaultRenderPipeline != null ? GraphicsSettings.defaultRenderPipeline.defaultMaterial : null;
        if (def != null)
        {
            var prim = new Material(def) { name = "Primitiva_por_defecto_barata" };
            CheapMaterials.MakeCheap(prim);
            SaveMat(prim, made);
            SaveMat(new Material(def) { name = "Primitiva_por_defecto" }, made);
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
  <pre id=""resultado"" hidden style=""position:fixed;left:8px;right:8px;bottom:8px;max-height:45vh;overflow:auto;margin:0;padding:8px;background:rgba(0,0,0,0.8);color:#c8e6c9;font:11px monospace;white-space:pre-wrap;z-index:20""></pre>
  <script>
    // LA PRUEBA EN LA PÁGINA (?prueba=1, WebPrueba): el juego escribe «[PRUEBA-JSON] {…}» y «[PRUEBA-TITULO] OK|FALLOS n» en la consola;
    // aquí pasan a #resultado y al título (los lee tools/web/probar-web.ps1). Se engancha ANTES de cargar Unity.
    (function () {
      var J = '[PRUEBA-JSON]', T = '[PRUEBA-TITULO]';
      ['log', 'info', 'warn', 'error'].forEach(function (k) {
        var orig = console[k];
        console[k] = function () {
          try {
            var s = Array.prototype.join.call(arguments, ' '), i = s.indexOf(J);
            if (i >= 0) { var el = document.getElementById('resultado'); var txt = s.slice(i + J.length).trim(); try { txt = JSON.stringify(JSON.parse(txt), null, 1); } catch (e) {} el.textContent = txt; el.hidden = false; }
            i = s.indexOf(T); if (i >= 0) document.title = s.slice(i + T.length).trim();
          } catch (e) {}
          return orig.apply(console, arguments);
        };
      });
    })();
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
      }).catch(function (m) { document.querySelector('#msg').textContent = m; document.title = 'FALLOS carga'; var el = document.getElementById('resultado'); el.textContent = JSON.stringify({ ok: false, fallos: 1, error: String(m) }); el.hidden = false; });
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
