using UnityEngine;

/// <summary>
/// LA INICIACIÓN ES BLOQUEANTE (docs/jornada-y-rotacion.md): quien acaba de llegar (Kushal, o un turista) no está
/// registrado en el santuario, así que no puede salir de la ENFERMERÍA hasta hacerse los análisis, vacunarse y completar
/// la iniciación con Linduarda. Mientras tanto la MAGNATE se planta en la salida (lo alto de la escalera) y lo devuelve
/// dentro con un aviso; dentro de la enfermería puede moverse libremente, y Linduarda lo busca para seguir.
/// </summary>
public class InfirmaryGate : MonoBehaviour
{
    [Tooltip("Salida de la enfermería en coordenadas locales del área (lo alto de la escalera).")]
    public Vector3 exitLocal = new Vector3(0f, 1f, 9.4f);
    public float radius = 2.2f;
    public int blockedCount { get; private set; }
    float _msgUntil; string _msg;

    /// <summary>¿Puede salir? Cuando ya está analizado, vacunado y certificado (o no es un recién llegado).</summary>
    public static bool Cleared(GameObject who)
    {
        if (who == null) return true;
        HealthRecord hr = who.GetComponent<HealthRecord>();
        SanctuaryOnboarding ob = who.GetComponent<SanctuaryOnboarding>();
        bool health = hr == null || !hr.NeedsCare;
        bool initiated = ob == null || ob.apt;
        return health && initiated;
    }

    void Update()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (ColdSanctuary.TestRunner.Running || p == null || Cleared(p)) return;   // (las pruebas mueven al jugador libremente)
        Vector3 exit = transform.TransformPoint(exitLocal);
        // La Magnate vigila la salida (si no está ocupada presentando a alguien).
        GameObject mag = GameObject.Find("Magnate");
        if (mag != null && (mag.transform.position - exit).sqrMagnitude > 1f) mag.transform.position = exit + transform.forward * 0.3f;
        Vector3 d = p.transform.position - exit; d.y = 0f;
        if (d.magnitude < radius)
        {
            Vector3 back = transform.TransformPoint(exitLocal + new Vector3(0f, 0.3f, -3.5f));
            CharacterController cc = p.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            p.transform.position = back;
            if (cc != null) cc.enabled = true;
            blockedCount++;
            _msg = "La Magnate: «Aún no estás registrado en el santuario. Termina tu iniciación con Linduarda: análisis, vacuna y tu primera virtualización.»";
            _msgUntil = Time.time + 4f;
            Debug.Log("[Iniciación] La Magnate bloquea la salida de la enfermería: el recién llegado aún no ha completado la iniciación.");
        }
    }

    void OnGUI()
    {
        if (Time.time > _msgUntil || string.IsNullOrEmpty(_msg)) return;
        GUI.Box(new Rect(Screen.width * 0.5f - 300, Screen.height - 120, 600, 60), "");
        GUI.Label(new Rect(Screen.width * 0.5f - 290, Screen.height - 114, 580, 50), _msg);
    }
}
