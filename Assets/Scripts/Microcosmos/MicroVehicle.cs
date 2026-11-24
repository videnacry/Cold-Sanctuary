using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UN TRANSPORTE DE ÉPOCA QUE SE USA (docs/microcosmos/plan-microcosmos-realismo.md §3.3, P6; Berón 2026-11-18): se sube, se mueve y, si tiene
/// horquillas o caja, lleva carga. A tamaño REAL (el mundo es 1:1): para el insecto-humano encogido, un coche es un vehículo grande y
/// creíble. Va en la raíz de un mueble <see cref="MicroPiece"/> «vehiculo». Sin física de ruedas: un movimiento cinemático sobre el
/// suelo (avanza, gira y frena), con tope de velocidad real, que basta para recorrer un aparcamiento o una nave.
/// - El jugador se acerca (≤ <see cref="boardRadius"/>) y pulsa <see cref="key"/> (F): sube, se sienta en <see cref="seat"/> y lo
///   conduce con W/S (o flechas) y A/D; F otra vez, baja a su lado. Mientras conduce, su PlayerController y su CharacterController se
///   apagan (y se devuelven al bajar).
/// - <see cref="Load"/> pone una carga sobre las horquillas o en la caja (<see cref="cargoPoint"/>); viaja con él.
/// La lógica (<see cref="Board"/>, <see cref="Drive"/>, <see cref="Leave"/>) es pública para probarla sin teclado (MicroVehicleTest).
/// </summary>
public class MicroVehicle : MonoBehaviour
{
    [Tooltip("Nombre que se ve (Tesla Roadster de 2008, carretilla elevadora…).")]
    public string title = "Vehículo";
    [Tooltip("Velocidad máxima (m/s) hacia delante.")]
    public float maxSpeed = 4f;
    [Tooltip("Aceleración (m/s²).")]
    public float accel = 3f;
    [Tooltip("Giro (°/s) a toda velocidad.")]
    public float turn = 70f;
    [Tooltip("Distancia (m) desde la que se puede subir.")]
    public float boardRadius = 2.5f;
    [Tooltip("Tecla para subir y bajar.")]
    public KeyCode key = KeyCode.F;
    [Tooltip("Dónde se sienta quien conduce (local).")]
    public Transform seat;
    [Tooltip("Dónde va la carga (las horquillas, la caja); vacío si no lleva.")]
    public Transform cargoPoint;
    [Tooltip("Fuente de las medidas reales (regla M39).")]
    public string source = "";

    static readonly List<MicroVehicle> _all = new List<MicroVehicle>();
    public static IReadOnlyList<MicroVehicle> All => _all;

    /// <summary>Quien lo conduce ahora (null si nadie).</summary>
    public GameObject Driver { get; private set; }
    /// <summary>Velocidad actual (m/s; negativa marcha atrás).</summary>
    public float Speed { get; private set; }
    /// <summary>Lo que lleva encima.</summary>
    public Transform Cargo { get; private set; }
    /// <summary>Metros recorridos con conductor (pruebas).</summary>
    public float Odometer { get; private set; }

    Transform _driverParent; bool _pcWas, _ccWas;

    void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
    // La carga que ya viene puesta en la escena (el palé en las horquillas).
    void Start() { if (Cargo == null && cargoPoint != null && cargoPoint.childCount > 0) Cargo = cargoPoint.GetChild(0); }
    void OnDisable() { _all.Remove(this); if (Driver != null) Leave(); }

    /// <summary>Sube <paramref name="who"/> (si no hay nadie): a su asiento, con su control apagado. Devuelve si subió.</summary>
    public bool Board(GameObject who)
    {
        if (who == null || Driver != null) return false;
        Driver = who; Speed = 0f;
        if (who.TryGetComponent(out PlayerController pc)) { _pcWas = pc.enabled; pc.enabled = false; }
        if (who.TryGetComponent(out CharacterController cc)) { _ccWas = cc.enabled; cc.enabled = false; }
        _driverParent = who.transform.parent;
        Transform s = seat != null ? seat : transform;
        who.transform.SetParent(s, true);
        who.transform.position = s.position; who.transform.rotation = s.rotation;
        Debug.Log($"[Transporte] «{who.name}» sube a «{title}».");
        return true;
    }

    /// <summary>Baja a quien conduce, a su izquierda, y le devuelve el control.</summary>
    public void Leave()
    {
        GameObject who = Driver; if (who == null) return;
        Driver = null; Speed = 0f;
        Vector3 side = transform.position - transform.right * (Size().x * 0.5f + 0.6f);
        who.transform.SetParent(_driverParent, true);
        who.transform.position = new Vector3(side.x, transform.position.y, side.z);
        who.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        if (who.TryGetComponent(out CharacterController cc)) cc.enabled = _ccWas;
        if (who.TryGetComponent(out PlayerController pc)) pc.enabled = _pcWas;
        Debug.Log($"[Transporte] «{who.name}» baja de «{title}».");
    }

    /// <summary>Un paso de conducción de <paramref name="dt"/> s con el acelerador <paramref name="throttle"/> (−1…1) y la dirección
    /// <paramref name="steer"/> (−1…1). Sin conductor, frena.</summary>
    public void Drive(float throttle, float steer, float dt)
    {
        float target = Driver != null ? Mathf.Clamp(throttle, -1f, 1f) * (throttle < 0f ? maxSpeed * 0.4f : maxSpeed) : 0f;
        Speed = Mathf.MoveTowards(Speed, target, accel * dt);
        if (Mathf.Abs(Speed) < 1e-3f) return;
        float k = Mathf.Clamp01(Mathf.Abs(Speed) / Mathf.Max(0.1f, maxSpeed * 0.3f)) * Mathf.Sign(Speed);
        transform.Rotate(0f, Mathf.Clamp(steer, -1f, 1f) * turn * k * dt, 0f, Space.World);
        Vector3 step = transform.forward * Speed * dt;
        transform.position += new Vector3(step.x, 0f, step.z);
        Odometer += Mathf.Abs(Speed) * dt;
    }

    /// <summary>Pone <paramref name="load"/> en su sitio de carga (si lo tiene). Devuelve si la cargó.</summary>
    public bool Load(Transform load)
    {
        if (cargoPoint == null || load == null || Cargo != null) return false;
        Cargo = load; load.SetParent(cargoPoint, true); load.position = cargoPoint.position; load.rotation = cargoPoint.rotation;
        return true;
    }

    /// <summary>Deja la carga en el suelo, delante.</summary>
    public Transform Unload()
    {
        Transform c = Cargo; if (c == null) return null;
        Cargo = null; c.SetParent(transform.parent, true);
        Vector3 p = transform.position + transform.forward * (Size().z * 0.5f + 0.8f); c.position = new Vector3(p.x, transform.position.y, p.z);
        return c;
    }

    Vector3 Size()
    {
        Bounds b = new Bounds(transform.position, Vector3.zero); bool any = false;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) { if (Driver != null && r.transform.IsChildOf(Driver.transform)) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
        return any ? b.size : Vector3.one * 2f;
    }

    void Update()
    {
        GameObject player = null;
        if (Driver == null)
        {
            if (GameKeys.GetKeyDown(key))
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player != null && (player.transform.position - transform.position).sqrMagnitude <= boardRadius * boardRadius) Board(player);
            }
            if (Mathf.Abs(Speed) > 1e-3f) Drive(0f, 0f, Time.deltaTime);
            return;
        }
        if (GameKeys.GetKeyDown(key)) { Leave(); return; }
        float v = (GameKeys.GetKey(KeyCode.W) || GameKeys.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (GameKeys.GetKey(KeyCode.S) || GameKeys.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        float h = (GameKeys.GetKey(KeyCode.D) || GameKeys.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (GameKeys.GetKey(KeyCode.A) || GameKeys.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
        Drive(v, h, Time.deltaTime);
    }
}
