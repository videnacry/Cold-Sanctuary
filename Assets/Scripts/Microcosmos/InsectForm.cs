using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Recuerda qué se ocultó del cuerpo humano para devolvérselo (<see cref="InsectOnly.Release"/>), y (2026-11-18) lo que se
/// encogió al entrar con el cuerpo de insecto a la mitad (<see cref="Shrink"/>: cápsula, controlador, agente, velocidad y la cámara).</summary>
public class InsectForm : MonoBehaviour
{
    public string species;
    [SerializeField] List<Renderer> _hidden = new List<Renderer>();

    // Lo que se encogió (para devolverlo al salir).
    [SerializeField] float _shrunk = 1f;
    [SerializeField] float _ccHeight, _ccRadius, _ccStep; [SerializeField] Vector3 _ccCenter;
    [SerializeField] float _capHeight, _capRadius; [SerializeField] Vector3 _capCenter;
    [SerializeField] float _agRadius, _agHeight, _agSpeed;
    [SerializeField] float _walk, _sprint, _jump;
    [SerializeField] List<Transform> _anchors = new List<Transform>();
    [SerializeField] List<Vector3> _anchorPos = new List<Vector3>();

    /// <summary>El factor con que se encogió (1 = sin encoger).</summary>
    public float Shrunk => _shrunk;

    /// <summary>Oculta todo lo visible (menos los rótulos y lo que ya sea insecto) y devuelve su estatura.</summary>
    public float HideHuman()
    {
        bool any = false; Bounds b = default;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled || r is ParticleSystemRenderer || r.GetComponent<TextMesh>() != null) continue;
            if (r.transform.IsChildOf(transform) && r.GetComponentInParent<CubeInsect>() != null && r.transform.parent != null && IsUnderInsect(r.transform)) continue;
            if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            r.enabled = false; _hidden.Add(r);
        }
        float h = any ? b.size.y : 1.75f;
        return Mathf.Clamp(h, 1.4f, 2.1f);
    }

    /// <summary>
    /// ENCOGE el cuerpo físico a <paramref name="s"/> (MicroBuildRules.FolkScale, docs/microcosmos/plan-microcosmos-realismo.md §3.1 E1: «el avatar
    /// del jugador insecto, igual»): la cápsula o el CharacterController, el NavMeshAgent, la velocidad del PlayerController y los
    /// anclajes de la cámara (las distancias de seguimiento). Una sola vez; <see cref="Restore"/> lo devuelve todo.
    /// </summary>
    public void Shrink(float s)
    {
        if (_shrunk != 1f || s <= 0f || Mathf.Approximately(s, 1f)) return;
        _shrunk = s;
        if (TryGetComponent(out CharacterController cc))
        {
            _ccHeight = cc.height; _ccRadius = cc.radius; _ccStep = cc.stepOffset; _ccCenter = cc.center;
            cc.height = _ccHeight * s; cc.radius = _ccRadius * s; cc.center = Feet(_ccCenter, _ccHeight, s); cc.stepOffset = Mathf.Min(_ccStep * s, cc.height);
        }
        if (TryGetComponent(out CapsuleCollider cap))
        {
            _capHeight = cap.height; _capRadius = cap.radius; _capCenter = cap.center;
            cap.height = _capHeight * s; cap.radius = _capRadius * s; cap.center = Feet(_capCenter, _capHeight, s);
        }
        if (TryGetComponent(out UnityEngine.AI.NavMeshAgent ag))
        {
            _agRadius = ag.radius; _agHeight = ag.height; _agSpeed = ag.speed;
            ag.radius = _agRadius * s; ag.height = _agHeight * s; ag.speed = _agSpeed * s;
        }
        if (TryGetComponent(out PlayerController pc))
        {
            _walk = pc.walkSpeed; _sprint = pc.sprintSpeed; _jump = pc.jumpHeight;
            pc.walkSpeed = _walk * s; pc.sprintSpeed = _sprint * s; pc.jumpHeight = _jump * s;
        }
        _anchors.Clear(); _anchorPos.Clear();
        CameraManager cm = CameraManager.Instance;
        if (cm != null)
            foreach (Transform a in new[] { cm.thirdPersonAnchor, cm.firstPersonAnchor })
                if (a != null && a != transform && a.IsChildOf(transform)) { _anchors.Add(a); _anchorPos.Add(a.localPosition); a.localPosition *= s; }
    }

    // El centro de una cápsula encogida con los PIES donde estaban (la cara de abajo no se mueve: no cae ni se hunde).
    static Vector3 Feet(Vector3 center, float height, float s) => new Vector3(center.x, center.y - height * 0.5f + height * s * 0.5f, center.z);

    bool IsUnderInsect(Transform t) { for (Transform p = t; p != null && p != transform; p = p.parent) if (p.name == "Insecto") return true; return false; }

    public void Restore()
    {
        Transform ins = transform.Find("Insecto");
        if (ins != null) { if (Application.isPlaying) Destroy(ins.gameObject); else DestroyImmediate(ins.gameObject); }
        CubeInsect ci = GetComponent<CubeInsect>();
        if (ci != null) { if (Application.isPlaying) Destroy(ci); else DestroyImmediate(ci); }
        foreach (Renderer r in _hidden) if (r != null) r.enabled = true;
        _hidden.Clear();
        if (_shrunk != 1f)
        {
            if (TryGetComponent(out CharacterController cc)) { cc.stepOffset = _ccStep; cc.height = _ccHeight; cc.radius = _ccRadius; cc.center = _ccCenter; }
            if (TryGetComponent(out CapsuleCollider cap)) { cap.height = _capHeight; cap.radius = _capRadius; cap.center = _capCenter; }
            if (TryGetComponent(out UnityEngine.AI.NavMeshAgent ag)) { ag.radius = _agRadius; ag.height = _agHeight; ag.speed = _agSpeed; }
            if (TryGetComponent(out PlayerController pc)) { pc.walkSpeed = _walk; pc.sprintSpeed = _sprint; pc.jumpHeight = _jump; }
            for (int i = 0; i < _anchors.Count; i++) if (_anchors[i] != null) _anchors[i].localPosition = _anchorPos[i];
            _shrunk = 1f;
        }
        if (Application.isPlaying) Destroy(this); else DestroyImmediate(this);
    }
}
