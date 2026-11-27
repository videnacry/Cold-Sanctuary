using UnityEngine;

/// <summary>
/// CUÁNTOS PISOS NECESITA CADA CENTRO DE ENTRENAMIENTO (regla J16 / S13; docs/areas/centros-para-todos.md y
/// docs/areas/rediseno-pisos-arboles.md).
/// J16 REESCRITA (D1 de Berón 2026-11-25: «uno por actividad, por plazas»): cada centro lleva UN piso, y la SUMA de las plazas de
/// todos llega a los miembros; el entrenamiento de las 8 y las 14 reparte a la gente por plazas (<see cref="WorkRoster.SeatTraining"/>).
/// Solo si la suma no llega se añade un piso, al centro con más plazas por piso (<see cref="FloorsFor(string,int)"/>).
/// LOS EDIFICIOS DE HOY («amplios», <see cref="Amplios"/>): la J16 antigua (Berón 2026-11-04: «si las 2 de yoga existentes no son
/// suficientes se crea una más…»): cada centro con ⌈miembros / plazas de un piso⌉ pisos, para todos a la vez. Se guardan para cuando
/// haya más gente. La fase F1 del rediseño (2026-11-25) apagó <see cref="Amplios"/>: el constructor levanta un piso por centro.
/// Los miembros salen de la lista del personal (<see cref="AreaCapacity.DefaultMembers"/>; nunca un número fijo). La comprobación es
/// la de la regla de capacidad (<see cref="AreaCapacity"/>, grupo 75). El Aula lleva su propia regla (AulaFloors: estudio para todos).
/// </summary>
public static class TrainingCentres
{
    public struct Spec
    {
        public string name, activity; public int perFloor; public string unit, why;
        public Spec(string n, string act, int p, string u, string w) { name = n; activity = act; perFloor = p; unit = u; why = w; }
    }

    /// <summary>Lo que cabe en UN piso de cada centro (de su distribución; docs/areas/centros-para-todos.md). <c>activity</c>: la actividad de
    /// su <see cref="AreaSeat"/> (vacía si AreaCapacity lo cuenta por sí mismo: máquinas del gimnasio, esterillas del yoga).</summary>
    public static readonly Spec[] All =
    {
        new Spec("Gimnasio", "", 20, "estaciones del circuito", "10 tipos de máquina de placas × 2 copias (F1b, 2026-11-27: gimnasio y cardio en un piso de 40 × 40; más el cardio y el peso libre)"),
        new Spec("Calistenia", "calistenia, parkour y crossfit", 40, "puestos", "calistenia ~18 (dominadas 3, anillas 2, paralelas bajas 4, espalderas 3…), parkour ~6 a la vez, crossfit ~16 (rig, GHD, plataformas, ergómetros)"),
        new Spec("Yoga Hatha", "", 16, "esterillas", "dos salas por piso (occidental y tradicional) de 8 esterillas: con ≥ 2 m entre ellas y a ≤ 9 m del maestro no caben más en 18 × 26 m"),
        new Spec("Yoga Ashtanga", "", 16, "esterillas", "como el de Hatha: dos salas de 8 esterillas por piso"),
        new Spec("Dojo", "artes marciales", 25, "puestos", "tatami de 8 × 8 (16 a 2 m), jaula 2, ring 2, sacos 3, makiwaras 2"),
        new Spec("Ballet", "ballet", 20, "plazas", "suelo de 18 × 8,6 m y barras dobles (16–23 alumnos, docs/areas/obras-dojo-ballet.md)"),
    };

    public static Spec? Of(string centre) { foreach (Spec s in All) if (s.name == centre) return s; return null; }

    /// <summary>Los EDIFICIOS AMPLIOS (los de antes, J16 antigua: cada centro para todos a la vez). APAGADO desde la fase F1 del rediseño
    /// de pisos (2026-11-25): un piso por actividad y el entrenamiento repartido por plazas (docs/areas/rediseno-pisos-arboles.md §6 ter).
    /// Se enciende solo cuando haya más gente de la que caben, sumando, en un piso de cada centro.</summary>
    public static bool Amplios = false;

    /// <summary>Pisos que necesita UN centro con <paramref name="perFloor"/> plazas por piso para que quepan, él solo, <paramref name="members"/>
    /// miembros (la cuenta de los edificios amplios).</summary>
    public static int FloorsFor(int perFloor, int members) => Mathf.Max(1, Mathf.CeilToInt(members / (float)Mathf.Max(1, perFloor)));

    /// <summary>Las plazas de todos los centros con un piso cada uno.</summary>
    public static int SeatsOnePerActivity() { int n = 0; foreach (Spec s in All) n += s.perFloor; return n; }

    /// <summary>Pisos de un centro con la J16 reescrita (por plazas): uno; si la suma de todos no llega a <paramref name="members"/>, los
    /// pisos que faltan van, uno a uno, al centro con más plazas por piso (así se añaden los menos pisos posibles).</summary>
    public static int FloorsBySeats(string centre, int members)
    {
        Spec? me = Of(centre);
        if (!me.HasValue) return 1;
        int[] floors = new int[All.Length]; int seats = 0;
        for (int i = 0; i < All.Length; i++) { floors[i] = 1; seats += All[i].perFloor; }
        int best = 0;
        for (int i = 1; i < All.Length; i++) if (All[i].perFloor > All[best].perFloor) best = i;
        while (seats < members) { floors[best]++; seats += All[best].perFloor; }
        for (int i = 0; i < All.Length; i++) if (All[i].name == centre) return floors[i];
        return 1;
    }

    /// <summary>Los pisos que levanta el constructor para un centro: los amplios (cada uno para todos) o por plazas (J16 reescrita).</summary>
    public static int FloorsFor(string centre, int members)
    {
        if (!Amplios) return FloorsBySeats(centre, members);
        Spec? s = Of(centre); return s.HasValue ? FloorsFor(s.Value.perFloor, members) : 1;
    }
}
