using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>
/// PÉRDIDA DE SANGRE de un paciente (docs/hospital.md §Banco de sangre): fracción del volumen de sangre perdida (0–1). Las
/// clases de hemorragia del ATLS (American College of Surgeons, <i>Advanced Trauma Life Support</i>, 10.ª ed., 2018):
/// I &lt; 15 %, II 15–30 %, III 30–40 % (hace falta sangre), IV &gt; 40 % (riesgo de muerte inmediato). Con más del 30 % el paciente
/// empeora solo (shock); cada unidad transfundida devuelve su volumen (<see cref="BloodBank.UnitFraction"/>).
/// </summary>
public class BloodLoss : MonoBehaviour
{
    [Range(0f, 1f)] public float lost;
    public int unitsReceived;
    public string lastProduct;

    public int AtlsClass => lost < 0.15f ? 1 : lost < 0.3f ? 2 : lost < 0.4f ? 3 : 4;
    public bool NeedsBlood => lost >= 0.3f;

    void Update()
    {
        if (lost < 0.3f) return;
        Anima a = GetComponent<Anima>();
        if (a == null || a.death) return;
        a.sickness = Mathf.Min(1f, a.sickness + (lost - 0.25f) * 0.02f * Time.deltaTime);   // shock: empeora mientras no se repone
        a.stress = Mathf.Min(1f, a.stress + 0.01f * Time.deltaTime);
    }
}
