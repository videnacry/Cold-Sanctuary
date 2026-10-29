using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EGINA, EL REINO DE LOS MIRMIDONES (docs/microcosmos-myrmidons.md): el mito de Ovidio contado de forma humana. Éaco
/// sueña con un reino de virtud y grandeza; Zeus, su padre, con muchos contactos e influencia, va a ver a todos los
/// insecto-humanos que conoce y los anima a unirse y a trabajar por el bien común. El jugador ayuda (E: hablar con
/// alguien que aún no se ha unido). Unidos, cargan piedra de la cantera y alzan el templo (<see cref="GreekBuildSite"/>);
/// al terminarlo, Éaco es rey y la <see cref="HistoryCard"/> cuenta el mito y el templo reales, con fuentes.
/// </summary>
[RequireComponent(typeof(HistoryCard))]
public class MythMission : MonoBehaviour
{
    public string title = "Egina, el reino de los mirmidones";
    public GreekBuildSite site;
    public WorldLabel kingLabel;
    public float reach = 2.5f;

    public static MythMission Instance { get; private set; }
    public readonly List<MythRecruit> Recruits = new List<MythRecruit>();

    HistoryCard _card;
    Transform _player;
    bool _done;
    string _note; float _noteUntil;

    public bool CardOpen => _card != null && _card.IsOpen;
    public int Stage => site != null ? site.Stage : 0;
    public int Joined { get { int n = 0; foreach (MythRecruit r in Recruits) if (r != null && r.Joined) n++; return n; } }
    public float JoinedFraction => Recruits.Count == 0 ? 1f : Joined / (float)Recruits.Count;

    void OnEnable() => Instance = this;
    void OnDisable() { if (Instance == this) Instance = null; }

    void Start()
    {
        _card = GetComponent<HistoryCard>();
        Recruits.Clear();
        Recruits.AddRange(FindObjectsByType<MythRecruit>(FindObjectsSortMode.None));
    }

    public void Say(string who, string line)
    {
        _note = string.IsNullOrEmpty(who) ? line : $"{who}: {line}";
        _noteUntil = Time.time + 5f;
        Debug.Log($"[Egina] {_note}");
    }

    public void Recruit(MythRecruit r, string by)
    {
        if (r == null || r.Joined) return;
        r.Join();
        Say(r.displayName, r.acceptance);
    }

    MythRecruit Near()
    {
        if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) _player = p.transform; }
        if (_player == null) return null;
        MythRecruit best = null; float bd = reach;
        foreach (MythRecruit r in Recruits)
        {
            if (r == null || r.Joined) continue;
            float d = Vector3.Distance(r.transform.position, _player.position);
            if (d < bd) { bd = d; best = r; }
        }
        return best;
    }

    void Update()
    {
        if (CardOpen) return;
        if (!_done && site != null && site.Done) Finish();
        if (GameKeys.DialogOpen || !GameKeys.GetKeyDown(KeyCode.E)) return;
        MythRecruit r = Near();
        if (r == null) return;
        if (r.CanJoin(JoinedFraction, Stage)) { Recruit(r, "Kushal"); KushalVoice.Reply(KushalVoice.Ctx.Thanks); }
        else { Say(r.displayName, r.refusal); KushalVoice.Reply(KushalVoice.Ctx.Patience); }   // humilde ante el «aún no»
    }

    void Finish()
    {
        _done = true;
        if (kingLabel != null) kingLabel.text = "Éaco, rey de Egina";
        CharacterLevel lv = FindFirstObjectByType<CharacterLevel>();
        if (lv != null) lv.GainBondXp(50f);   // marga de los Vínculos: trabajar juntos por el bien común
        Say("Éaco", "«No es mío: es de todos los que lo levantaron.»");
        _card.ShowFinal();
    }

    void OnGUI()
    {
        if (CardOpen) return;
        GUIStyle wrap = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 15 };
        MythRecruit r = Near();
        string hint = r == null ? null : r.CanJoin(JoinedFraction, Stage) ? $"E · hablar con {r.displayName} (unirse)" : $"E · hablar con {r.displayName}";
        GUI.Box(new Rect(10f, 10f, 420f, 74f), "");
        GUI.Label(new Rect(20f, 14f, 400f, 22f), title, wrap);
        string obra = site == null ? "" : site.Done ? "templo terminado" : $"templo: {site.StageName} ({site.Blocks}/{site.NextBlocks} bloques)";
        GUI.Label(new Rect(20f, 36f, 400f, 22f), $"Se han unido: {Joined}/{Recruits.Count}   ·   {obra}", wrap);
        if (!string.IsNullOrEmpty(hint)) GUI.Label(new Rect(20f, 58f, 400f, 22f), hint, wrap);
        if (Time.time < _noteUntil) GUI.Label(new Rect(Screen.width * 0.5f - 280f, Screen.height - 90f, 560f, 60f), _note, new GUIStyle(wrap) { alignment = TextAnchor.MiddleCenter });
    }
}
