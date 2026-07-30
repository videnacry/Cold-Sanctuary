using UnityEngine;

/// <summary>
/// Cámara-CABEZA del **modo primera persona** (docs/kitchen-simulation.md §3b). Va **en la cámara de 1ª
/// persona** del jugador. La retícula (<see cref="VirtualPointer"/>) está fija en el centro de esa cámara;
/// para apuntar, **giras la cabeza** con las teclas de cámara **I/K/J/L** (mecanografía) — pero con
/// **restricciones** de giro (yaw/pitch limitados), como una cabeza real: no da la vuelta entera. Se congela
/// mientras se teclea (<see cref="TypingChallenge.Active"/>).
/// Al **entrar en modo primera persona** (p. ej. al acercarse a una estación) se activa esta cabeza y se
/// desactiva el look libre de `PlayerController`; al salir, se invierte.
/// </summary>
public class HeadLook : MonoBehaviour
{
    public KeyCode up = KeyCode.I;
    public KeyCode down = KeyCode.K;
    public KeyCode left = KeyCode.J;
    public KeyCode right = KeyCode.L;

    [Tooltip("Velocidad de giro de la cabeza (grados/s).")]
    public float speed = 90f;
    [Tooltip("Giro horizontal máximo a cada lado (grados).")]
    public float yawLimit = 70f;
    [Tooltip("Giro vertical máximo arriba/abajo (grados).")]
    public float pitchLimit = 55f;

    static int _activeCount;
    /// <summary>¿Hay una cabeza restringida activa? (para que `PlayerController` ceda el look libre).</summary>
    public static bool Active => _activeCount > 0;

    float _yaw, _pitch;
    Quaternion _home;

    void Awake() { _home = transform.localRotation; }

    // IMPORTANTE (reuso): el CAMBIO de perspectiva 1ª/3ª y el forzado por zona ya los hacen
    // CameraManager + AutoCameraZone (una estación = AutoCameraZone{desiredMode=FirstPerson}). HeadLook NO
    // reimplementa eso — solo aporta el giro de cabeza restringido, y debe estar ENABLED únicamente en el
    // modo primera persona de estación (lo activa/desactiva la entrada a la estación), no siempre.
    void OnEnable() { _activeCount++; }
    void OnDisable() { _activeCount = Mathf.Max(0, _activeCount - 1); }

    void Update()
    {
        if (TypingChallenge.Active) return;   // la cabeza no se mueve mientras se teclea

        float dy = 0f, dp = 0f;
        if (Input.GetKey(right)) dy += 1f;
        if (Input.GetKey(left)) dy -= 1f;
        if (Input.GetKey(up)) dp -= 1f;     // mirar arriba = pitch negativo
        if (Input.GetKey(down)) dp += 1f;

        _yaw = Mathf.Clamp(_yaw + dy * speed * Time.deltaTime, -yawLimit, yawLimit);
        _pitch = Mathf.Clamp(_pitch + dp * speed * Time.deltaTime, -pitchLimit, pitchLimit);
        transform.localRotation = _home * Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
