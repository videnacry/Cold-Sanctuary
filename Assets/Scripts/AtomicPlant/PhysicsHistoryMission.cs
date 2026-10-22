using UnityEngine;

/// <summary>
/// EL MICROCOSMOS DE LA PLANTA ATÓMICA (docs/atomic-plant.md §Microcosmos): la planta ×10, y en su suelo tres grandes
/// de la física, cada uno junto a su experimento: NEWTON (la gravedad: el cañón y la órbita), MARIE CURIE (la
/// radiactividad: el electrómetro y el cuarzo) y EINSTEIN (E = mc²: la balanza de la fisión). E junto a uno abre su
/// lección (<see cref="PhysicsLesson"/>); al hacer las tres, la <see cref="HistoryCard"/> cuenta la historia real, con
/// fuentes, y se gana experiencia (marga de los Stats). Es un tramo de la escalera de la física (hilo A: Eratóstenes
/// abre otro microcosmos).
/// </summary>
[RequireComponent(typeof(HistoryCard))]
public class PhysicsHistoryMission : MonoBehaviour
{
    public string title = "La planta atómica: Newton, Curie y Einstein";
    public PhysicsLesson[] lessons = new PhysicsLesson[0];
    public float reach = 3f;

    HistoryCard _card;
    Transform _player;
    bool _done;

    void Start()
    {
        _card = GetComponent<HistoryCard>();
        if (lessons.Length == 0) lessons = FindObjectsByType<PhysicsLesson>(FindObjectsSortMode.None);
    }

    bool AnyOpen { get { foreach (PhysicsLesson l in lessons) if (l != null && l.Open) return true; return false; } }

    int DoneCount { get { int n = 0; foreach (PhysicsLesson l in lessons) if (l != null && l.Done) n++; return n; } }

    PhysicsLesson Near()
    {
        if (_player == null) { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) _player = p.transform; }
        if (_player == null) return null;
        PhysicsLesson best = null; float bd = reach;
        foreach (PhysicsLesson l in lessons)
        {
            if (l == null) continue;
            Vector3 at = l.teacherBody != null ? l.teacherBody.position : l.transform.position;
            float d = Vector3.Distance(at, _player.position);
            if (d < bd) { bd = d; best = l; }
        }
        return best;
    }

    void Update()
    {
        if ((_card != null && _card.IsOpen) || AnyOpen) return;
        if (!_done && lessons.Length > 0 && DoneCount == lessons.Length) Finish();
        if (GameKeys.DialogOpen || !GameKeys.GetKeyDown(KeyCode.E)) return;
        PhysicsLesson l = Near();
        if (l != null) l.Begin();
    }

    void Finish()
    {
        _done = true;
        CharacterLevel lv = FindFirstObjectByType<CharacterLevel>();
        if (lv != null) lv.GainStatsXp(60f);
        if (_card != null) _card.ShowFinal();
    }

    void OnGUI()
    {
        if ((_card != null && _card.IsOpen) || AnyOpen) return;
        GUIStyle wrap = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 15 };
        GUI.Box(new Rect(10f, 10f, 440f, 74f), "");
        GUI.Label(new Rect(20f, 14f, 420f, 22f), title, wrap);
        string list = "";
        foreach (PhysicsLesson l in lessons) if (l != null) list += $"{(l.Done ? "✓" : "○")} {l.teacher}   ";
        GUI.Label(new Rect(20f, 36f, 420f, 22f), list, wrap);
        PhysicsLesson n = Near();
        if (n != null) GUI.Label(new Rect(20f, 58f, 420f, 22f), $"E · {n.teacher}: {n.topic}", wrap);
    }
}
