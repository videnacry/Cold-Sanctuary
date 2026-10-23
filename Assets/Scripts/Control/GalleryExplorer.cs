using UnityEngine;

/// <summary>
/// EXPLORADOR de las escenas de validación (galería de insectos-humanos, sala de microcosmos): un observador que anda
/// y vuela sin depender del resto del juego (reloj, jornada, UI), para recorrerlas y revisarlas de cerca.
/// WASD mueve · ratón con clic derecho mira · Q/E baja/sube · Mayús corre · F anda pegado al suelo / vuela.
/// Se le puede dar la etiqueta Player para que los portales y diálogos de la escena lo reconozcan.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class GalleryExplorer : MonoBehaviour
{
    public float speed = 6f, fastFactor = 4f, lookSpeed = 3f, eyeHeight = 1.6f;
    public bool walking = true;
    float _yaw, _pitch, _fall;
    CharacterController _cc;
    Camera _cam;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _cam = GetComponentInChildren<Camera>();
        _yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            _yaw += Input.GetAxis("Mouse X") * lookSpeed;
            _pitch = Mathf.Clamp(_pitch - Input.GetAxis("Mouse Y") * lookSpeed, -85f, 85f);
        }
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        if (_cam != null) _cam.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        if (Input.GetKeyDown(KeyCode.F)) walking = !walking;

        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        float v = speed * (Input.GetKey(KeyCode.LeftShift) ? fastFactor : 1f);
        move = move.normalized * v;
        if (walking)
        {
            _fall = _cc.isGrounded ? -1f : _fall - 20f * Time.deltaTime;
            move.y = _fall;
        }
        else
        {
            if (Input.GetKey(KeyCode.E)) move.y += v;
            if (Input.GetKey(KeyCode.Q)) move.y -= v;
        }
        _cc.Move(move * Time.deltaTime);
    }

    /// <summary>Crea el explorador (cuerpo + cámara) en <paramref name="at"/>, mirando a <paramref name="yaw"/> grados.</summary>
    public static GalleryExplorer Spawn(Vector3 at, float yaw)
    {
        var go = new GameObject("Player"); go.tag = "Player";
        go.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
        var cc = go.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = 0.35f; cc.center = Vector3.up * 0.9f;
        var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; camGo.transform.SetParent(go.transform, false);
        camGo.transform.localPosition = Vector3.up * 1.6f;
        Camera cam = camGo.AddComponent<Camera>(); cam.farClipPlane = 3000f; cam.nearClipPlane = 0.05f;
        camGo.AddComponent<AudioListener>();
        return go.AddComponent<GalleryExplorer>();
    }
}
