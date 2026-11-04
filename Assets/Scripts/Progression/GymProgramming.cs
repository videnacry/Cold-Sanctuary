using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Objetivo del plan (docs/gimnasio-maquinas.md §15): cada uno fija series, repeticiones, descanso y RPE.</summary>
public enum TrainingGoal { Fuerza, Hipertrofia, Resistencia, Salud }

/// <summary>Reparto de la semana (§15.1): cuerpo entero, torso/pierna, empuje/tirón/pierna y la rutina Weider («bro split»).</summary>
public enum TrainingSplit { CuerpoEntero, TorsoPierna, EmpujeTironPierna, Weider }

/// <summary>
/// PLANES DE ENTRENAMIENTO REALES del gimnasio (Berón 2026-11-04: «hay que actualizar a los demás humanos para que realicen
/// entrenamientos reales (series, repeticiones… rutinas donde se varían músculos por días)»; docs/gimnasio-maquinas.md §15).
/// Datos puros, sin escena (los usan la jornada, el entrenador, el panel y las pruebas):
///  · cada humano del personal tiene un <see cref="TrainingPlan"/>: reparto, días de la semana, objetivo y por qué;
///  · <see cref="Prescribe"/> da la rutina de un día: calentamiento, ejercicios (máquina, series × repeticiones, carga, descanso,
///    RPE) y vuelta a la calma; los días sin gimnasio en su plan, una sesión suave de recuperación;
///  · <see cref="AdvanceWeek"/> es la PROGRESIÓN DOBLE (más repeticiones dentro del rango; arriba del rango, más carga y vuelta
///    abajo) con una semana de DESCARGA cada cuarta (mitad de series, −10 % de carga, RPE −2);
///  · el ENTRENADOR (<see cref="Trainer"/>) revisa el registro y ajusta los planes en su misión de la jornada (<see cref="Review"/>).
/// Los humanos-insecto de los microcosmos no tienen plan: en sus eras no hay gimnasio (<see cref="Eligible"/>).
/// </summary>
public static class GymProgramming
{
    // ── El papel de entrenador ──────────────────────────────────────────────────────────────────
    /// <summary>ENTRENADOR DEL GIMNASIO: GOLUIS, «el personaje de la fuerza», corpulento y el más disciplinado del personal
    /// (Archetypes: disciplina 1,3; fuerza 1,5). Gohageneis ya lleva las artes marciales y la Guardia (y es «poco disciplinado»,
    /// 0,6); Linduarda entrena a diario pero su conducta de médica (recién llegados, hospital) manda sobre la jornada.</summary>
    public const string Trainer = "Goluis";
    public const string GymArea = "Gimnasio";
    /// <summary>Misión de la jornada del entrenador: revisar el registro de series y ajustar los planes (1 h, solo él).</summary>
    public const string PlanMissionId = "gym_planes";

    /// <summary>Las misiones de papel fijo del gimnasio (la de los planes, con dueño: <see cref="WorkRoster.Mission.owner"/>).</summary>
    public static List<WorkRoster.Mission> Missions() => new List<WorkRoster.Mission>
    {
        new WorkRoster.Mission(GymArea, PlanMissionId, "Entrenador: revisar el registro de series y ajustar los planes", 1f) { owner = Trainer, lead = true },
    };

    // ── Parámetros por objetivo (NSCA, ACSM 2009; §15.2) ────────────────────────────────────────
    public struct GoalSpec
    {
        public int sets, setsAccessory, repLo, repHi;
        public float restSec, rpe;
        public string intensity;
    }

    /// <summary>Series, rango de repeticiones, descanso (s) y RPE del objetivo; <paramref name="compound"/> = ejercicio
    /// multiarticular. En Fuerza los accesorios van como en hipertrofia (así se programa en la práctica).</summary>
    public static GoalSpec Spec(TrainingGoal g, bool compound = true)
    {
        switch (g)
        {
            case TrainingGoal.Fuerza:
                return compound ? new GoalSpec { sets = 4, setsAccessory = 3, repLo = 4, repHi = 6, restSec = 180f, rpe = 8f, intensity = "80–88 % 1RM" }
                                : new GoalSpec { sets = 3, setsAccessory = 3, repLo = 8, repHi = 12, restSec = 90f, rpe = 8f, intensity = "67–80 % 1RM" };
            case TrainingGoal.Hipertrofia: return new GoalSpec { sets = 3, setsAccessory = 3, repLo = 8, repHi = 12, restSec = 90f, rpe = 8f, intensity = "67–80 % 1RM" };
            case TrainingGoal.Resistencia: return new GoalSpec { sets = 3, setsAccessory = 2, repLo = 15, repHi = 20, restSec = 45f, rpe = 7f, intensity = "≤ 67 % 1RM" };
            default: return new GoalSpec { sets = 2, setsAccessory = 2, repLo = 10, repHi = 15, restSec = 90f, rpe = 6f, intensity = "60–70 % 1RM" };
        }
    }

    /// <summary>Multiplicador de la carga de partida por objetivo (más pesado con menos repeticiones).</summary>
    static float GoalLoad(TrainingGoal g) => g == TrainingGoal.Fuerza ? 1.25f : g == TrainingGoal.Hipertrofia ? 1f : g == TrainingGoal.Resistencia ? 0.7f : 0.6f;

    // ── Biblioteca de ejercicios: máquina → músculo, si es multiarticular, carga base y alternativas ──
    public class Exercise
    {
        public GymMachineType station; public string muscle; public bool compound, lower; public float baseKg;
        public GymMachineType[] alt;
        public bool Bodyweight => baseKg <= 0f;
    }

    static readonly Dictionary<GymMachineType, Exercise> _lib = new Dictionary<GymMachineType, Exercise>();
    static void X(GymMachineType t, string muscle, bool compound, bool lower, float kg, params GymMachineType[] alt) =>
        _lib[t] = new Exercise { station = t, muscle = muscle, compound = compound, lower = lower, baseKg = kg, alt = alt };

    static GymProgramming()
    {
        // Carga base: la de trabajo en hipertrofia de un adulto medio que ya entrena (kg en la pila o en la barra; 0 = peso corporal).
        X(GymMachineType.PressBancaPlano, "pecho", true, false, 40f, GymMachineType.PressPecho, GymMachineType.PressBancaInclinado);
        X(GymMachineType.PressBancaInclinado, "pecho", true, false, 32.5f, GymMachineType.PressPecho, GymMachineType.PressBancaPlano);
        X(GymMachineType.PressPecho, "pecho", true, false, 35f, GymMachineType.PressBancaPlano, GymMachineType.PressBancaInclinado);
        X(GymMachineType.Fondos, "tríceps y pecho", true, false, 0f, GymMachineType.FondosAnillas, GymMachineType.BicepsTriceps);
        X(GymMachineType.FondosAnillas, "tríceps y pecho", true, false, 0f, GymMachineType.Fondos);
        X(GymMachineType.PressHombros, "hombros", true, false, 22.5f, GymMachineType.PressBancaInclinado);
        X(GymMachineType.Jalon, "dorsal", true, false, 40f, GymMachineType.Dominadas);
        X(GymMachineType.Dominadas, "dorsal", true, false, 0f, GymMachineType.Jalon);
        X(GymMachineType.Remo, "espalda media", true, false, 35f, GymMachineType.RemoMancuerna, GymMachineType.RemoAustraliano);
        X(GymMachineType.RemoMancuerna, "espalda media", true, false, 14f, GymMachineType.Remo, GymMachineType.RemoAustraliano);
        X(GymMachineType.RemoAustraliano, "espalda media y bíceps", true, false, 0f, GymMachineType.Remo, GymMachineType.RemoMancuerna);
        X(GymMachineType.BicepsTriceps, "bíceps y tríceps", false, false, 15f, GymMachineType.RemoAustraliano);
        X(GymMachineType.SentadillaJaula, "cuádriceps y glúteo", true, true, 45f, GymMachineType.SentadillaSmith, GymMachineType.Prensa);
        X(GymMachineType.SentadillaSmith, "cuádriceps y glúteo", true, true, 40f, GymMachineType.SentadillaJaula, GymMachineType.Prensa);
        X(GymMachineType.Prensa, "cuádriceps y glúteo", true, true, 90f, GymMachineType.SentadillaSmith, GymMachineType.ExtensionCuadriceps);
        X(GymMachineType.ExtensionCuadriceps, "cuádriceps", false, true, 30f, GymMachineType.Prensa);
        X(GymMachineType.PesoMuerto, "cadena posterior", true, true, 60f, GymMachineType.Hiperextension, GymMachineType.HipThrust);
        X(GymMachineType.CurlFemoral, "isquiotibiales", false, true, 25f, GymMachineType.Hiperextension);
        X(GymMachineType.Hiperextension, "lumbares e isquios", false, true, 0f, GymMachineType.CurlFemoral);
        X(GymMachineType.HipThrust, "glúteo", true, true, 50f, GymMachineType.Hiperextension);
        X(GymMachineType.Gemelos, "gemelos", false, true, 35f);
        X(GymMachineType.Crunch, "abdomen", false, false, 0f, GymMachineType.AbdominalDeclinado, GymMachineType.ElevacionPiernas);
        X(GymMachineType.AbdominalDeclinado, "abdomen", false, false, 0f, GymMachineType.Crunch, GymMachineType.ElevacionPiernas);
        X(GymMachineType.ElevacionPiernas, "abdomen", false, false, 0f, GymMachineType.Crunch, GymMachineType.AbdominalDeclinado);
        X(GymMachineType.SillaRomana, "abdomen", false, false, 0f, GymMachineType.ElevacionPiernas, GymMachineType.Crunch);
        // Calentamiento y vuelta a la calma.
        X(GymMachineType.BiciVertical, "cardio", false, true, 0f, GymMachineType.BiciReclinada, GymMachineType.Eliptica, GymMachineType.Cinta, GymMachineType.RemoErgometro);
        X(GymMachineType.Cinta, "cardio", false, true, 0f, GymMachineType.BiciVertical, GymMachineType.Eliptica, GymMachineType.BiciReclinada, GymMachineType.RemoErgometro);
        X(GymMachineType.EstiramientoIsquios, "estiramientos", false, true, 0f);
        _profiles = BuildProfiles();
    }

    public static Exercise Ex(GymMachineType t) => _lib.TryGetValue(t, out Exercise e) ? e : new Exercise { station = t, muscle = "general", baseKg = 0f, alt = new GymMachineType[0] };

    // ── Rutinas de cada reparto (§15.1) ──────────────────────────────────────────────────────────
    public class DayTemplate { public string name; public GymMachineType[] items; public DayTemplate(string n, params GymMachineType[] i) { name = n; items = i; } }

    static List<DayTemplate> Templates(TrainingSplit s, TrainingGoal g)
    {
        var T = new List<DayTemplate>();
        switch (s)
        {
            case TrainingSplit.CuerpoEntero:
                if (g == TrainingGoal.Salud)   // mayores y principiantes: máquinas guiadas (ACSM, adultos mayores)
                {
                    T.Add(new DayTemplate("Cuerpo entero A (máquinas)", GymMachineType.Prensa, GymMachineType.PressPecho, GymMachineType.Remo, GymMachineType.CurlFemoral, GymMachineType.Crunch));
                    T.Add(new DayTemplate("Cuerpo entero B (máquinas)", GymMachineType.ExtensionCuadriceps, GymMachineType.PressHombros, GymMachineType.Jalon, GymMachineType.HipThrust, GymMachineType.Gemelos));
                }
                else
                {
                    T.Add(new DayTemplate("Cuerpo entero A", GymMachineType.SentadillaJaula, GymMachineType.PressBancaPlano, GymMachineType.Remo, GymMachineType.HipThrust, GymMachineType.Crunch));
                    T.Add(new DayTemplate("Cuerpo entero B", GymMachineType.PesoMuerto, GymMachineType.PressHombros, GymMachineType.Jalon, GymMachineType.CurlFemoral, GymMachineType.Gemelos));
                }
                break;
            case TrainingSplit.TorsoPierna:
                T.Add(new DayTemplate("Torso A", GymMachineType.PressBancaPlano, GymMachineType.Remo, GymMachineType.PressHombros, GymMachineType.Jalon, GymMachineType.BicepsTriceps));
                T.Add(new DayTemplate("Pierna A", GymMachineType.SentadillaJaula, GymMachineType.CurlFemoral, GymMachineType.Prensa, GymMachineType.Gemelos, GymMachineType.Crunch));
                T.Add(new DayTemplate("Torso B", GymMachineType.PressBancaInclinado, GymMachineType.RemoMancuerna, GymMachineType.Dominadas, GymMachineType.PressPecho, GymMachineType.BicepsTriceps));
                T.Add(new DayTemplate("Pierna B", GymMachineType.PesoMuerto, GymMachineType.SentadillaSmith, GymMachineType.ExtensionCuadriceps, GymMachineType.HipThrust, GymMachineType.ElevacionPiernas));
                break;
            case TrainingSplit.EmpujeTironPierna:
                T.Add(new DayTemplate("Empuje", GymMachineType.PressBancaPlano, GymMachineType.PressHombros, GymMachineType.PressBancaInclinado, GymMachineType.Fondos));
                T.Add(new DayTemplate("Tirón", GymMachineType.Jalon, GymMachineType.Remo, GymMachineType.RemoMancuerna, GymMachineType.BicepsTriceps, GymMachineType.Hiperextension));
                T.Add(new DayTemplate("Pierna", GymMachineType.SentadillaJaula, GymMachineType.CurlFemoral, GymMachineType.Prensa, GymMachineType.Gemelos, GymMachineType.Crunch));
                break;
            default:   // Weider: un grupo muscular por día
                T.Add(new DayTemplate("Pecho", GymMachineType.PressBancaPlano, GymMachineType.PressBancaInclinado, GymMachineType.PressPecho, GymMachineType.Fondos));
                T.Add(new DayTemplate("Espalda", GymMachineType.Dominadas, GymMachineType.Remo, GymMachineType.RemoMancuerna, GymMachineType.Hiperextension));
                T.Add(new DayTemplate("Hombros y abdomen", GymMachineType.PressHombros, GymMachineType.ElevacionPiernas, GymMachineType.AbdominalDeclinado, GymMachineType.SillaRomana));
                T.Add(new DayTemplate("Pierna", GymMachineType.SentadillaJaula, GymMachineType.Prensa, GymMachineType.ExtensionCuadriceps, GymMachineType.CurlFemoral, GymMachineType.Gemelos));
                T.Add(new DayTemplate("Brazos", GymMachineType.BicepsTriceps, GymMachineType.FondosAnillas, GymMachineType.RemoAustraliano, GymMachineType.Crunch));
                break;
        }
        return T;
    }

    public static string SplitName(TrainingSplit s) => s == TrainingSplit.CuerpoEntero ? "Cuerpo entero" : s == TrainingSplit.TorsoPierna ? "Torso / pierna" : s == TrainingSplit.EmpujeTironPierna ? "Empuje / tirón / pierna" : "Weider (un grupo por día)";
    static readonly string[] DayNames = { "lun", "mar", "mié", "jue", "vie", "sáb", "dom" };
    public static string Days(int[] d) => string.Join("-", d.Select(x => DayNames[((x % 7) + 7) % 7]));

    // ── El plan de cada persona ─────────────────────────────────────────────────────────────────
    public class Progress { public float load; public int reps, sets; }

    public class TrainingPlan
    {
        public string who, why;
        public TrainingSplit split; public TrainingGoal goal;
        public int[] days;            // días de la semana con gimnasio (0 = lunes)
        public float strength = 1f;   // multiplicador de la carga de partida (complexión, edad)
        public List<DayTemplate> sessions;
        public int week;              // semana del plan (día / 7)
        public readonly Dictionary<GymMachineType, Progress> progress = new Dictionary<GymMachineType, Progress>();
        /// <summary>Ejercicios con series sin acabar esta semana: el entrenador no les sube la carga.</summary>
        public readonly HashSet<GymMachineType> missed = new HashSet<GymMachineType>();
        public string adjustedBy; public int reviews;
        public int SessionsDone;
        public bool Trains(int day) => days.Contains(((day % 7) + 7) % 7);
        /// <summary>Frecuencia semanal de cada grupo muscular (veces que se trabaja a la semana).</summary>
        public int PerMuscleFrequency => Mathf.Max(1, split == TrainingSplit.CuerpoEntero ? days.Length : split == TrainingSplit.TorsoPierna ? days.Length / 2 : split == TrainingSplit.EmpujeTironPierna ? days.Length / 3 : days.Length / 5);
        /// <summary>La rutina del día <paramref name="day"/> (absoluto): rota por las sesiones del reparto, de semana en semana.</summary>
        public DayTemplate SessionOn(int day)
        {
            int wd = ((day % 7) + 7) % 7, idx = System.Array.IndexOf(days, wd);
            if (idx < 0) return null;
            int n = (day / 7) * days.Length + idx;
            return sessions[((n % sessions.Count) + sessions.Count) % sessions.Count];
        }
        public override string ToString() => $"{who}: {SplitName(split)} · {goal} · {Days(days)} ({days.Length} d/sem, cada músculo {PerMuscleFrequency}×/sem)";
    }

    /// <summary>Perfil de partida de cada persona del personal: reparto, días, objetivo, fuerza y por qué (docs/diseno-personajes.md).</summary>
    class Profile { public TrainingSplit s; public TrainingGoal g; public int[] d; public float k; public string why; }
    static Dictionary<string, Profile> _profiles;
    static Profile P(TrainingSplit s, TrainingGoal g, float k, string why, params int[] d) => new Profile { s = s, g = g, d = d, k = k, why = why };

    static Dictionary<string, Profile> BuildProfiles() => new Dictionary<string, Profile>
    {
        { "Player", P(TrainingSplit.CuerpoEntero, TrainingGoal.Hipertrofia, 0.9f, "Kushal, recién llegado: principiante, cuerpo entero 3 d (ACSM: 2–3 d/sem al empezar).", 0, 2, 4) },
        { "Linduarda", P(TrainingSplit.EmpujeTironPierna, TrainingGoal.Hipertrofia, 0.8f, "«Deportista, gimnasio a diario»: empuje/tirón/pierna 6 d (cada músculo 2×/sem).", 0, 1, 2, 3, 4, 5) },
        { "Lizmibet", P(TrainingSplit.CuerpoEntero, TrainingGoal.Salud, 0.45f, "Más de 70 años: fuerza 2 d/sem en máquinas guiadas (OMS y ACSM para mayores).", 1, 4) },
        { "Goluis", P(TrainingSplit.TorsoPierna, TrainingGoal.Fuerza, 1.4f, "El personaje de la fuerza y entrenador: torso/pierna 4 d, fuerza (4–6 reps).", 0, 1, 3, 4) },
        { "Panterilia", P(TrainingSplit.CuerpoEntero, TrainingGoal.Hipertrofia, 0.75f, "Delgada, 42 años: cuerpo entero 3 d para ganar músculo.", 1, 3, 5) },
        { "Gohageneis", P(TrainingSplit.EmpujeTironPierna, TrainingGoal.Fuerza, 1.1f, "Artes marciales y Guardia: fuerza en empuje/tirón/pierna 3 d (el resto, su dojo).", 0, 2, 4) },
        { "Irosene", P(TrainingSplit.CuerpoEntero, TrainingGoal.Resistencia, 0.5f, "Más de 70, corre, escala y bucea: resistencia muscular 2 d (15–20 reps).", 2, 5) },
        { "JefeConstruccion", P(TrainingSplit.TorsoPierna, TrainingGoal.Resistencia, 1.15f, "Tote, robusto, jornadas de obra: torso/pierna 4 d de resistencia.", 1, 2, 4, 5) },
        { "Andrea", P(TrainingSplit.CuerpoEntero, TrainingGoal.Fuerza, 0.9f, "Corre 25 km y hace yoga: fuerza pesada 2 d (mejora la economía de carrera).", 0, 3) },
        { "Nesvitor", P(TrainingSplit.CuerpoEntero, TrainingGoal.Salud, 0.6f, "El mayor de las cuentas: salud, 2 d en máquinas guiadas.", 0, 3) },
        { "Amahuz", P(TrainingSplit.TorsoPierna, TrainingGoal.Hipertrofia, 0.85f, "Delgado: torso/pierna 4 d para ganar masa.", 0, 1, 3, 4) },
        { "Karen", P(TrainingSplit.CuerpoEntero, TrainingGoal.Resistencia, 0.6f, "Entrena con Amahuz los lunes: cuerpo entero 3 d de resistencia.", 0, 2, 4) },
        { "Alicita", P(TrainingSplit.EmpujeTironPierna, TrainingGoal.Hipertrofia, 0.65f, "Huerto (cargar cajas): empuje/tirón/pierna 3 d.", 1, 3, 5) },
        { "Ducristin", P(TrainingSplit.TorsoPierna, TrainingGoal.Fuerza, 1f, "Mecánico (levantar piezas): torso/pierna 4 d de fuerza.", 0, 2, 3, 5) },
        { "Josedrilo", P(TrainingSplit.EmpujeTironPierna, TrainingGoal.Hipertrofia, 1f, "Guardia: empuje/tirón/pierna 6 d.", 1, 2, 3, 4, 5, 6) },
        { "Maite", P(TrainingSplit.CuerpoEntero, TrainingGoal.Resistencia, 0.65f, "Registros y vacunación (mucho de pie): cuerpo entero 3 d de resistencia.", 1, 3, 5) },
        { "Shalva", P(TrainingSplit.TorsoPierna, TrainingGoal.Hipertrofia, 0.9f, "Personal general: torso/pierna 4 d.", 1, 2, 4, 5) },
        { "Adaguchi", P(TrainingSplit.Weider, TrainingGoal.Hipertrofia, 0.9f, "Sudadera sin mangas, estilo culturista: Weider 5 d (un grupo por día; cada músculo 1×/sem).", 0, 1, 2, 3, 4) },
    };

    /// <summary>¿Puede tener plan de gimnasio? Los humanos del santuario sí; los humanos-insecto de los microcosmos no: en sus
    /// eras no hay gimnasio (docs/gimnasio-maquinas.md §15.6).</summary>
    public static bool Eligible(string who) => !string.IsNullOrEmpty(who) && System.Array.IndexOf(WorkdayDirector.Staff, who) >= 0;

    static readonly Dictionary<string, TrainingPlan> _plans = new Dictionary<string, TrainingPlan>();
    public static IEnumerable<TrainingPlan> AllPlans => _plans.Values;
    /// <summary>Olvida todos los planes (las pruebas, para empezar desde la semana 0).</summary>
    public static void ResetAll() => _plans.Clear();

    /// <summary>El plan de <paramref name="who"/> (lo escribe la primera vez; null si no puede tener plan).</summary>
    public static TrainingPlan For(string who, int day = 0)
    {
        if (!Eligible(who)) return null;
        if (_plans.TryGetValue(who, out TrainingPlan p)) return p;
        Profile pr = _profiles.TryGetValue(who, out Profile f) ? f : P(TrainingSplit.CuerpoEntero, TrainingGoal.Salud, 0.7f, "Sin perfil: cuerpo entero 3 d de salud.", 0, 2, 4);
        p = new TrainingPlan { who = who, why = pr.why, split = pr.s, goal = pr.g, days = pr.d, strength = pr.k, sessions = Templates(pr.s, pr.g), week = Mathf.Max(0, day / 7) };
        foreach (DayTemplate t in p.sessions) foreach (GymMachineType m in t.items) Init(p, m);
        _plans[who] = p;
        return p;
    }

    static Progress Init(TrainingPlan p, GymMachineType m)
    {
        if (p.progress.TryGetValue(m, out Progress pr)) return pr;
        Exercise e = Ex(m); GoalSpec s = Spec(p.goal, e.compound);
        pr = new Progress { reps = s.repLo, sets = e.compound ? s.sets : s.setsAccessory, load = e.Bodyweight ? 0f : Round(e.baseKg * GoalLoad(p.goal) * p.strength) };
        p.progress[m] = pr;
        return pr;
    }

    static float Round(float kg) => Mathf.Max(1.25f, Mathf.Round(kg / 1.25f) * 1.25f);

    /// <summary>Lo que sube la carga al llegar arriba del rango: 2,5 kg tren superior y 5 kg inferior (≈ 2–10 %, ACSM 2009);
    /// la mitad para el objetivo Salud.</summary>
    public static float Increment(GymMachineType m, TrainingGoal g) { float i = Ex(m).lower ? 5f : 2.5f; return g == TrainingGoal.Salud ? i * 0.5f : i; }

    /// <summary>¿La semana <paramref name="week"/> es de DESCARGA? Una de cada cuatro (la 4.ª, 8.ª…; Bell et al. 2023: cada 4–8).</summary>
    public static bool IsDeload(int week) => week % 4 == 3;

    // ── La rutina de un día ─────────────────────────────────────────────────────────────────────
    public class Prescription
    {
        public GymMachineType station;
        public GymMachineType origin;      // la máquina del plan (si estaba ocupada y se cambió por otra, la progresión sigue en esta)
        public int sets, reps, rampSets;   // rampSets: series de aproximación (≈50 % de la carga) antes de las de trabajo
        public float load, restSec, rpe;
        public bool timed; public float minutes;   // calentamiento y vuelta a la calma: por tiempo
        public string part = "principal";          // «calentamiento», «principal», «vuelta a la calma», «recuperación»
        public string Name => GymStation.Label(station);
        public string LoadText => timed ? $"{minutes:0} min" : load <= 0f ? "peso corporal" : $"{load:0.##} kg";
        public override string ToString() => timed ? $"{part}: {Name} {minutes:0} min" : $"{Name} {sets}×{reps} · {LoadText} · RPE {rpe:0} · descanso {restSec:0} s{(rampSets > 0 ? $" (+{rampSets} de aproximación)" : "")}";
    }

    public class DayRoutine
    {
        public string who, name; public int day, week; public bool deload, recovery;
        public List<Prescription> items = new List<Prescription>();
        public IEnumerable<Prescription> Main => items.Where(i => !i.timed);
    }

    /// <summary>La rutina de <paramref name="who"/> el día <paramref name="day"/>: calentamiento (5 min de cardio suave y una serie
    /// de aproximación en el primer multiarticular con carga), los ejercicios del día y 5 min de estiramientos. Si su plan no
    /// tiene gimnasio ese día: 10 min de cardio suave y estiramientos (recuperación activa).</summary>
    public static DayRoutine Prescribe(string who, int day)
    {
        TrainingPlan p = For(who, day);
        if (p == null) return null;
        SyncWeek(p, day);
        var r = new DayRoutine { who = who, day = day, week = p.week, deload = IsDeload(p.week) };
        DayTemplate t = p.SessionOn(day);
        if (t == null)
        {
            r.recovery = true; r.name = "Recuperación activa";
            r.items.Add(new Prescription { station = GymMachineType.BiciVertical, timed = true, minutes = 10f, part = "recuperación", rpe = 4f });
            r.items.Add(new Prescription { station = GymMachineType.EstiramientoIsquios, timed = true, minutes = 5f, part = "vuelta a la calma", rpe = 2f });
            return r;
        }
        r.name = t.name + (r.deload ? " (descarga)" : "");
        r.items.Add(new Prescription { station = p.goal == TrainingGoal.Resistencia ? GymMachineType.Cinta : GymMachineType.BiciVertical, timed = true, minutes = 5f, part = "calentamiento", rpe = 4f });
        bool ramp = false;
        foreach (GymMachineType m in t.items)
        {
            Exercise e = Ex(m); Progress pr = Init(p, m); GoalSpec s = Spec(p.goal, e.compound);
            var x = new Prescription { station = m, origin = m, sets = pr.sets, reps = pr.reps, load = pr.load, restSec = s.restSec, rpe = s.rpe };
            if (r.deload) { x.sets = Mathf.Max(1, Mathf.CeilToInt(x.sets * 0.5f)); if (x.load > 0f) x.load = Round(x.load * 0.9f); x.rpe -= 2f; }
            if (!ramp && e.compound && x.load > 0f && p.goal != TrainingGoal.Resistencia) { x.rampSets = 1; ramp = true; }
            r.items.Add(x);
        }
        r.items.Add(new Prescription { station = GymMachineType.EstiramientoIsquios, timed = true, minutes = 5f, part = "vuelta a la calma", rpe = 2f });
        return r;
    }

    // ── Progresión semanal ──────────────────────────────────────────────────────────────────────
    /// <summary>Pone el plan en la semana del día <paramref name="day"/> (aplica la progresión de cada semana pasada).</summary>
    public static void SyncWeek(TrainingPlan p, int day)
    {
        int target = Mathf.Max(0, day / 7), guard = 0;
        while (p.week < target && guard++ < 520) AdvanceWeek(p);   // (tope: 10 años de semanas)
    }

    /// <summary>PROGRESIÓN DOBLE de una semana: si la semana que acaba no fue de descarga, cada ejercicio sin series falladas
    /// suma repeticiones (+1; +2 en resistencia) hasta arriba del rango y, ya arriba, sube la carga (<see cref="Increment"/>) y
    /// vuelve abajo del rango. Con peso corporal: +1 repetición (tope 30). Devuelve qué cambió.</summary>
    public static List<string> AdvanceWeek(TrainingPlan p)
    {
        var l = new List<string>();
        bool wasDeload = IsDeload(p.week);
        p.week++;
        if (wasDeload) { l.Add($"{p.who}: vuelve de la descarga (semana {p.week})"); p.missed.Clear(); return l; }
        foreach (var kv in p.progress)
        {
            Progress pr = kv.Value; Exercise e = Ex(kv.Key); GoalSpec s = Spec(p.goal, e.compound);
            if (p.missed.Contains(kv.Key)) { l.Add($"{p.who}: {GymStation.Label(kv.Key)} repite {pr.reps} × {(pr.load > 0 ? pr.load + " kg" : "peso corporal")} (series sin acabar)"); continue; }
            if (e.Bodyweight || pr.load <= 0f) { pr.reps = Mathf.Min(30, pr.reps + 1); l.Add($"{p.who}: {GymStation.Label(kv.Key)} → {pr.reps} reps"); continue; }
            if (pr.reps < s.repHi) { pr.reps = Mathf.Min(s.repHi, pr.reps + (p.goal == TrainingGoal.Resistencia ? 2 : 1)); l.Add($"{p.who}: {GymStation.Label(kv.Key)} → {pr.reps} reps a {pr.load:0.##} kg"); }
            else { pr.load = Round(pr.load + Increment(kv.Key, p.goal)); pr.reps = s.repLo; l.Add($"{p.who}: {GymStation.Label(kv.Key)} → {pr.load:0.##} kg × {pr.reps}"); }
        }
        p.missed.Clear();
        return l;
    }

    /// <summary>Volumen-carga semanal del plan (Σ series × reps × kg; el peso corporal cuenta 1 kg): la medida de la progresión.</summary>
    public static float WeeklyTonnage(TrainingPlan p) => p.progress.Values.Sum(x => x.sets * x.reps * Mathf.Max(1f, x.load));

    // ── El entrenador ───────────────────────────────────────────────────────────────────────────
    /// <summary>LA MISIÓN DEL ENTRENADOR (al terminarla): escribe el plan de quien aún no tiene, revisa las series sin acabar
    /// (esas no suben) y avisa de la descarga. Devuelve las líneas del registro.</summary>
    public static List<string> Review(string trainer, int day, IEnumerable<string> people)
    {
        var l = new List<string>();
        foreach (string who in people)
        {
            bool fresh = !_plans.ContainsKey(who);
            TrainingPlan p = For(who, day);
            if (p == null) continue;
            SyncWeek(p, day);
            p.adjustedBy = trainer; p.reviews++;
            if (fresh) l.Add($"escribe el plan de {p}");
            if (p.missed.Count > 0) l.Add($"{who}: mantiene la carga de {string.Join(", ", p.missed.Select(GymStation.Label))} (series sin acabar)");
            if (IsDeload(p.week + 1) && day % 7 == 6) l.Add($"{who}: la semana que viene es de descarga");
            DayRoutine r = Prescribe(who, day + 1);
            if (r != null && !r.recovery) l.Add($"{who}, mañana «{r.name}»: {string.Join(" · ", r.Main.Select(x => $"{x.Name} {x.sets}×{x.reps} {x.LoadText}"))}");
        }
        return l;
    }

    /// <summary>Qué corrige el entrenador en cada máquina (las indicaciones técnicas de §2–§11 y de NASM/ACE).</summary>
    public static string Cue(GymMachineType t)
    {
        switch (t)
        {
            case GymMachineType.SentadillaJaula: case GymMachineType.SentadillaSmith: return "pecho arriba y rodillas hacia fuera, en la línea de los pies";
            case GymMachineType.PesoMuerto: return "espalda neutra y la barra pegada a las piernas";
            case GymMachineType.PressBancaPlano: case GymMachineType.PressBancaInclinado: return "escápulas atrás y codos a unos 45°";
            case GymMachineType.PressPecho: return "agarres a la altura del pecho y sin bloquear los codos";
            case GymMachineType.PressHombros: return "sin arquear la zona lumbar";
            case GymMachineType.Jalon: case GymMachineType.Dominadas: return "baja la barra al pecho sin balancearte";
            case GymMachineType.Remo: case GymMachineType.RemoMancuerna: return "codos atrás y pecho quieto";
            case GymMachineType.Prensa: return "rodillas alineadas con los pies y sin bloquearlas arriba";
            case GymMachineType.HipThrust: return "barbilla recogida y cadera arriba hasta alinear el tronco";
            case GymMachineType.CurlFemoral: case GymMachineType.ExtensionCuadriceps: return "la rodilla en el eje de la máquina; baja en 2 s";
            default: return "rango completo y bajada controlada (2 s)";
        }
    }

    /// <summary>¿Hace de «spotter» (se queda detrás por si falla la última repetición)? Peso libre pesado: banca y sentadillas.</summary>
    public static bool NeedsSpotter(GymMachineType t, float rpe) => rpe >= 7.5f && (t == GymMachineType.PressBancaPlano || t == GymMachineType.PressBancaInclinado || t == GymMachineType.SentadillaJaula);

    // ── Los días de gimnasio en la jornada ──────────────────────────────────────────────────────
    /// <summary>Los DÍAS DEL PLAN en el reparto del entrenamiento de las 14 h: quien tiene gimnasio ese día en su plan va al
    /// gimnasio (y el entrenador, siempre); a quien la rotación mandaba al gimnasio un día sin gimnasio en su plan, se le da el
    /// siguiente entrenamiento abierto. Si el gimnasio no está abierto, no cambia nada.</summary>
    public static int ApplyGymDays(WorkRoster.DayPlan plan, IList<string> open, IList<WorkRoster.Worker> workers)
    {
        if (plan == null || open == null || !open.Contains(GymArea)) return 0;
        int changed = 0;
        for (int i = 0; i < workers.Count; i++)
        {
            WorkRoster.Worker w = workers[i];
            if (w.clone) continue;
            TrainingPlan p = For(w.name, plan.day);
            if (p == null) continue;
            plan.training.TryGetValue(w, out string was);
            string want = was;
            if (w.name == Trainer || p.Trains(plan.day)) want = GymArea;
            else if (was == GymArea)
                for (int k = 1; k <= open.Count; k++) { string o = open[(i + plan.day + k) % open.Count]; if (o != GymArea) { want = o; break; } }
            if (want != was) { plan.training[w] = want; changed++; }
        }
        return changed;
    }
}
