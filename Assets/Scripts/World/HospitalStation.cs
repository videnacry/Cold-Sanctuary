using System.Collections.Generic;
using UnityEngine;

/// <summary>Servicios de un hospital aislado (docs/hospital.md): lo mínimo para tratar todos los casos sin derivar.</summary>
public enum HospitalService
{
    Urgencias, Consulta, UCI, Quirofano, Anestesia, Paritorio, Aislamiento, Imagen, Laboratorio, BancoDeSangre,
    Farmacia, Esterilizacion, Oxigeno, Telemedicina, Veterinaria, CirugiaVeterinaria
}

/// <summary>
/// PUESTO DEL HOSPITAL (docs/hospital.md): cada equipo importante es INTERACTUABLE (F) y actúa sobre el paciente más
/// cercano según su servicio: urgencias estabiliza, la UCI y la farmacia curan, el quirófano opera (solo si hay alguien en
/// la máquina de anestesia), imagen/laboratorio diagnostican, la veterinaria atiende animales. Los <see cref="doctorPost"/>
/// son los 8 PUESTOS MÉDICOS: la jornada coloca a cada médico que trabaja en el hospital en uno libre (<see cref="PostFor"/>).
/// </summary>
public class HospitalStation : MonoBehaviour, IInteractable
{
    public HospitalService service;
    [Tooltip("Uno de los 8 puestos donde trabaja un médico a la vez.")]
    public bool doctorPost;
    [Tooltip("Dónde se coloca quien trabaja aquí, en coordenadas locales del equipo.")]
    public Vector3 standLocal = new Vector3(0f, 0f, -1.1f);
    public float patientRadius = 3.5f;

    static readonly List<HospitalStation> _all = new List<HospitalStation>();
    static readonly Dictionary<string, HospitalStation> _posts = new Dictionary<string, HospitalStation>();

    void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
    void OnDisable() { _all.Remove(this); foreach (var kv in new List<KeyValuePair<string, HospitalStation>>(_posts)) if (kv.Value == this) _posts.Remove(kv.Key); }

    public static IReadOnlyList<HospitalStation> All => _all;
    public Vector3 StandPoint => transform.TransformPoint(standLocal) + Vector3.up;

    /// <summary>Puesto médico de <paramref name="who"/>: el que ya tenía, o el primero libre. null si los 8 están ocupados.</summary>
    public static HospitalStation PostFor(string who)
    {
        if (string.IsNullOrEmpty(who)) return null;
        if (_posts.TryGetValue(who, out HospitalStation mine) && mine != null) return mine;
        foreach (HospitalStation s in _all)
            if (s != null && s.doctorPost && !_posts.ContainsValue(s)) { _posts[who] = s; return s; }
        return null;
    }

    /// <summary>Deja libre el puesto de <paramref name="who"/> (ya no trabaja en el hospital).</summary>
    public static void Release(string who) { if (who != null) _posts.Remove(who); }

    public string InteractLabel => Label(service);
    public bool CanInteract => true;

    public void Interact()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        Treat(p != null ? p.GetComponent<Anima>() : null);
    }

    static bool ForAnimals(HospitalService s) => s == HospitalService.Veterinaria || s == HospitalService.CirugiaVeterinaria;

    /// <summary>Atiende al paciente más cercano (animal en veterinaria, persona en el resto). Devuelve a quién.</summary>
    public Anima Treat(Anima by)
    {
        Anima patient = Nearest(by);
        string what = Science(service);
        if (patient == null) { Debug.Log($"[Hospital] {Label(service)}: no hay ningún paciente aquí. ({what})"); return null; }
        SicknessState ss = patient.GetComponent<SicknessState>();
        void Heal(float a) { if (ss != null) ss.Heal(a); else patient.sickness = Mathf.Max(0f, patient.sickness - a); }
        switch (service)
        {
            case HospitalService.Urgencias: Heal(0.25f); patient.stress = Mathf.Max(0f, patient.stress - 0.2f); break;
            case HospitalService.UCI: Heal(0.5f); break;
            case HospitalService.Farmacia: Heal(0.1f); break;
            case HospitalService.Consulta: case HospitalService.Veterinaria: patient.stress = Mathf.Max(0f, patient.stress - 0.1f); break;
            case HospitalService.Quirofano: case HospitalService.CirugiaVeterinaria:
                if (!AnesthetistPresent(patient)) { Debug.Log($"[Hospital] {Label(service)}: no se opera sin alguien en la anestesia (Lista de verificación de cirugía segura de la OMS)."); return null; }
                Heal(1f); patient.stress = Mathf.Min(1f, patient.stress + 0.1f); break;
        }
        BodyInjuries inj = patient.GetComponent<BodyInjuries>();
        string dx = $"enfermedad {patient.sickness:0.00}" + (inj != null ? $", {inj.Describe()}" : "");
        Debug.Log($"[Hospital] {Label(service)} atiende a «{patient.name}» ({dx}). {what}");
        return patient;
    }

    Anima Nearest(Anima by)
    {
        bool animals = ForAnimals(service);
        Anima best = null; float bd = patientRadius;
        foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
        {
            if (a == by || a.death || (a is Animal) != animals) continue;
            float d = Vector3.Distance(a.transform.position, transform.position);
            if (d < bd) { bd = d; best = a; }
        }
        return best;
    }

    // En el quirófano tiene que haber alguien (que no sea el paciente) en la máquina de anestesia de la misma sala.
    bool AnesthetistPresent(Anima patient)
    {
        foreach (HospitalStation s in _all)
        {
            if (s == null || s.service != HospitalService.Anestesia || Vector3.Distance(s.transform.position, transform.position) > 5f) continue;
            foreach (Anima a in FindObjectsByType<Anima>(FindObjectsSortMode.None))
                if (a != patient && !(a is Animal) && Vector3.Distance(a.transform.position, s.StandPoint) < 1.6f) return true;
        }
        return false;
    }

    public static string Label(HospitalService s)
    {
        switch (s)
        {
            case HospitalService.Urgencias: return "Urgencias: estabilizar (ABC)";
            case HospitalService.Consulta: return "Consulta médica";
            case HospitalService.UCI: return "UCI: cuidados intensivos";
            case HospitalService.Quirofano: return "Quirófano: operar";
            case HospitalService.Anestesia: return "Anestesia: dormir y vigilar al paciente";
            case HospitalService.Paritorio: return "Paritorio: atender un parto";
            case HospitalService.Aislamiento: return "Aislamiento: paciente infeccioso";
            case HospitalService.Imagen: return "Imagen: radiografía / ecografía";
            case HospitalService.Laboratorio: return "Laboratorio: análisis";
            case HospitalService.BancoDeSangre: return "Banco de sangre: cruzar y transfundir";
            case HospitalService.Farmacia: return "Farmacia: dispensar tratamiento";
            case HospitalService.Esterilizacion: return "Esterilizar el instrumental";
            case HospitalService.Oxigeno: return "Planta de oxígeno";
            case HospitalService.Telemedicina: return "Telemedicina: consultar a un especialista";
            case HospitalService.Veterinaria: return "Consulta veterinaria";
            default: return "Quirófano veterinario: operar";
        }
    }

    /// <summary>Qué enseña cada puesto (docs/hospital.md, con sus fuentes).</summary>
    public static string Science(HospitalService s)
    {
        switch (s)
        {
            case HospitalService.Urgencias: return "Triaje y ABC: vía aérea, respiración, circulación; el desfibrilador revierte la fibrilación ventricular.";
            case HospitalService.UCI: return "UCI: ventilación mecánica y monitorización continua (SpO2, ECG, tensión).";
            case HospitalService.Quirofano: return "Cesárea, laparotomía y fractura abierta: si un hospital puede hacer estas tres, puede con casi toda la cirugía urgente (Lancet Commission on Global Surgery, 2015).";
            case HospitalService.Anestesia: return "Anestesia general: oxígeno + vapor anestésico; sin pulsioxímetro no se empieza (OMS, cirugía segura).";
            case HospitalService.Paritorio: return "Parto: la cuna térmica evita la hipotermia del recién nacido.";
            case HospitalService.Aislamiento: return "Aislamiento: barrera de contacto y gotas; equipo de protección en la antesala.";
            case HospitalService.Imagen: return "Rayos X para huesos y tórax; ecografía para abdomen, embarazo y líquido libre.";
            case HospitalService.Laboratorio: return "Hemograma, glucosa y grupo sanguíneo: lo básico para decidir.";
            case HospitalService.BancoDeSangre: return "Sangre a 2-6 °C; lejos de todo, los donantes son el propio santuario (banco de sangre andante).";
            case HospitalService.Farmacia: return "Lista de medicamentos esenciales de la OMS: antibióticos, analgésicos, anestésicos, oxitocina…";
            case HospitalService.Esterilizacion: return "Autoclave: vapor a 121 °C durante 15 min (o 134 °C, 3 min) mata bacterias y esporas.";
            case HospitalService.Oxigeno: return "Concentrador PSA: la zeolita retiene el nitrógeno del aire y deja ~93 % de oxígeno.";
            case HospitalService.Telemedicina: return "Telemedicina: como en las bases antárticas, un especialista lejano guía al médico in situ.";
            case HospitalService.Veterinaria: return "Una sola salud: el mismo hospital cuida a personas y animales.";
            case HospitalService.CirugiaVeterinaria: return "Cirugía veterinaria: la dosis de anestesia va por kilo y por especie (se pesa en la báscula).";
            default: return "";
        }
    }
}
