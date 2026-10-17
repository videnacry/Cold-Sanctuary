using System.Collections;
using UnityEngine;

/// <summary>
/// TABLERO DE MISIONES de una escena de microcosmos por área: al entrar (cuando MobWorldLoader termina de traer al
/// jugador) muestra el menú de prácticas del área; elegir una la arranca (MobMission.RaiseBegin) y, al terminarla,
/// el menú vuelve a salir. Escape en el menú = salir del microcosmos (MobWorldLoader.ExitMobWorld), igual que el portal.
/// </summary>
public class MobWorldMissionBoard : MonoBehaviour
{
    public string areaName = "área";
    public MobMission[] missions;

    MobMission _current;

    void OnEnable() { StartCoroutine(ShowWhenEntered()); }

    // Al descargarse la escena (salir del microcosmos por el portal o el hechizo `exit`), su menú no puede quedarse abierto:
    // bloqueaba el control del jugador ya de vuelta en el mesocosmos.
    void OnDisable() { if (MissionSelectMenu.Instance != null) MissionSelectMenu.Instance.CloseSilently(); }

    IEnumerator ShowWhenEntered()
    {
        while (MobWorldLoader.HasInstance && MobWorldLoader.Instance.IsBusy) yield return null;
        while (GameObject.FindGameObjectWithTag("Player") == null) yield return null;
        yield return new WaitForSeconds(0.5f);
        Show();
    }

    void Show()
    {
        MissionSelectMenu menu = MissionSelectMenu.Get();
        if (menu == null) { Debug.LogWarning($"[Microcosmos] {areaName}: sin menú de misiones; arranco la primera."); Begin(First()); return; }
        menu.Show(missions, Begin, () =>
        {
            Debug.Log($"[Microcosmos] Sales del microcosmos de {areaName}.");
            if (MobWorldLoader.HasInstance) MobWorldLoader.Instance.ExitMobWorld();
        });
    }

    void Begin(MobMission m)
    {
        if (m == null) return;
        _current = m;
        m.OnEnded += Ended;
        Debug.Log($"[Microcosmos] {areaName}: «{m.missionName}».");
        m.RaiseBegin();
    }

    void Ended()
    {
        if (_current != null) _current.OnEnded -= Ended;
        _current = null;
        StartCoroutine(ShowAgain());
    }

    IEnumerator ShowAgain() { yield return new WaitForSeconds(1.5f); Show(); }

    MobMission First() { foreach (MobMission m in missions) if (m != null && m.isAvailable) return m; return null; }
}
