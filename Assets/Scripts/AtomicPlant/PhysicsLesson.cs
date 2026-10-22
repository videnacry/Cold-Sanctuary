using UnityEngine;

/// <summary>
/// UNA LECCIÓN DE FÍSICA del microcosmos de la planta atómica (docs/atomic-plant.md §Microcosmos): un experimento que
/// se JUEGA junto a su científico. <see cref="PhysicsHistoryMission"/> la abre con E cerca del maestro; mientras está
/// abierta se queda con el teclado (<see cref="GameKeys.MenuOpen"/>) y Esc la cierra. <see cref="Done"/> = el jugador
/// ya entendió lo que enseña (lo comprobó con el experimento).
/// </summary>
public abstract class PhysicsLesson : MonoBehaviour
{
    public string teacher = "";
    public string topic = "";
    public Transform teacherBody;

    public bool Open { get; private set; }
    public bool Done { get; protected set; }

    protected GUIStyle Wrap => new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 15, richText = true };

    int _openedFrame = -1;

    public void Begin() { Open = true; GameKeys.MenuOpen = true; _openedFrame = Time.frameCount; OnBegin(); }

    public void Close() { Open = false; GameKeys.MenuOpen = false; OnClose(); }

    protected virtual void OnBegin() { }
    protected virtual void OnClose() { }
    protected abstract void Tick();
    protected abstract void Draw(Rect area);

    protected void Complete(string line)
    {
        if (!Done) PlaneMessenger.Send(teacher, line, 10f);
        Done = true;
    }

    void OnDisable() { if (Open) Close(); }

    void Update()
    {
        if (!Open || Time.frameCount == _openedFrame) return;   // la E que la abrió no cuenta como paso
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
        Tick();
    }

    void OnGUI()
    {
        if (!Open) return;
        Rect box = new Rect(Screen.width * 0.5f - 320f, Screen.height - 300f, 640f, 290f);
        GUI.Box(box, $"{teacher} · {topic}");
        Draw(new Rect(box.x + 14f, box.y + 24f, box.width - 28f, box.height - 30f));
    }
}
