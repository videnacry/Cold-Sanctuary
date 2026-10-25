using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PROGRESO DEL JUGADOR en la historia y las misiones (Berón 2026-10-24: «un hechizo con el que conocer el progreso, como
/// un checklist»). Guarda, entre partidas (PlayerPrefs), qué historias de los microcosmos ha terminado, qué misiones de
/// trabajo ha hecho y qué prácticas y lecciones ha completado. Mientras corren las pruebas no se guarda nada. Lo lee el hechizo «progreso» (<see cref="ProgressPanel"/>).
/// </summary>
public static class Progress
{
    const string Key = "cs_progreso";
    static readonly HashSet<string> _stories = new HashSet<string>(), _missions = new HashSet<string>(), _practices = new HashSet<string>();
    static bool _loaded;
    static bool Off => ColdSanctuary.TestRunner.Running;   // las pruebas completan misiones: no cuentan en la partida del jugador

    static void Load()
    {
        if (_loaded) return;
        _loaded = true;
        foreach (string e in PlayerPrefs.GetString(Key, "").Split('\n'))
        {
            if (e.Length < 3) continue;
            string v = e.Substring(2);
            if (e[0] == 'H') _stories.Add(v); else if (e[0] == 'M') _missions.Add(v); else if (e[0] == 'P') _practices.Add(v);
        }
    }

    static void Save()
    {
        var l = new List<string>();
        foreach (string s in _stories) l.Add("H:" + s);
        foreach (string s in _missions) l.Add("M:" + s);
        foreach (string s in _practices) l.Add("P:" + s);
        PlayerPrefs.SetString(Key, string.Join("\n", l));
        PlayerPrefs.Save();
    }

    /// <summary>La historia del microcosmos <paramref name="scene"/> (p. ej. «Microcosmos_Patanjali») está terminada.</summary>
    public static void MarkStory(string scene) { if (Off) return; Load(); if (_stories.Add(scene)) { Save(); Debug.Log($"[Progreso] Historia terminada: {scene}."); } }
    public static void MarkMission(string id) { if (Off) return; Load(); if (_missions.Add(id)) { Save(); Debug.Log($"[Progreso] Misión hecha por primera vez: {id}."); } }
    public static void MarkPractice(string name) { if (Off) return; Load(); if (!string.IsNullOrEmpty(name) && _practices.Add(name)) { Save(); Debug.Log($"[Progreso] Práctica completada: {name}."); } }

    public static bool StoryDone(string scene) { Load(); return _stories.Contains(scene); }
    public static bool MissionDone(string id) { Load(); return _missions.Contains(id); }
    public static IEnumerable<string> Practices() { Load(); return _practices; }

    /// <summary>Borra todo el progreso (para empezar de cero o en las pruebas).</summary>
    public static void Reset() { Load(); _stories.Clear(); _missions.Clear(); _practices.Clear(); Save(); }

    /// <summary>Las escenas de microcosmos (las historias) que hay en el juego, por su nombre de escena.</summary>
    public static List<string> StoryScenes()
    {
        var l = new List<string>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string n = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            if (n.StartsWith("Microcosmos_")) l.Add(n);
        }
        l.Sort(System.StringComparer.Ordinal);
        return l;
    }
}
