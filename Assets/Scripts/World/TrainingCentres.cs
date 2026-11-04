using UnityEngine;

/// <summary>
/// CUÁNTOS PISOS NECESITA CADA CENTRO DE ENTRENAMIENTO (regla J16 / S13, Berón 2026-11-04: «los centros de entrenamiento y de ocio
/// deben dar servicio a todos los miembros a la vez»; «algunas áreas tienen más de un piso justo para abastecer a todos los miembros
/// y se pueden agregar pisos, si las 2 de yoga existentes no son suficientes se crea una más y lo mismo con el dojo y demás áreas de
/// estudio y entrenamiento»; docs/centros-para-todos.md).
/// El constructor de la torre del Entrenamiento pide <see cref="FloorsFor"/> pisos de cada centro, ⌈miembros / plazas de un piso⌉, con
/// los miembros de la lista del personal (<see cref="AreaCapacity.DefaultMembers"/>; nunca un número fijo), y los apila en el
/// edificio-árbol como cualquier otro piso (hélice, 22 m entre niveles, anillos, alfombras, máscaras). La comprobación es la de la
/// regla de capacidad (<see cref="AreaCapacity"/>, grupo 75): cuenta las estaciones del gimnasio, las esterillas de cada estilo de
/// yoga y los <see cref="AreaSeat"/> de la calistenia, el ballet y el dojo, en todos sus pisos. El Aula lleva la misma regla en su
/// propio constructor (AulaFloors).
/// </summary>
public static class TrainingCentres
{
    public struct Spec
    {
        public string name, activity; public int perFloor; public string unit, why;
        public Spec(string n, string act, int p, string u, string w) { name = n; activity = act; perFloor = p; unit = u; why = w; }
    }

    /// <summary>Lo que cabe en UN piso de cada centro (de su distribución; docs/centros-para-todos.md). <c>activity</c>: la actividad de
    /// su <see cref="AreaSeat"/> (vacía si AreaCapacity lo cuenta por sí mismo: máquinas del gimnasio, esterillas del yoga).</summary>
    public static readonly Spec[] All =
    {
        new Spec("Gimnasio", "", 30, "estaciones del circuito", "10 tipos de máquina de placas × 3 copias en el piso del gimnasio (más el cardio y el peso libre)"),
        new Spec("Calistenia", "calistenia, parkour y crossfit", 40, "puestos", "calistenia ~18 (dominadas 3, anillas 2, paralelas bajas 4, espalderas 3…), parkour ~6 a la vez, crossfit ~16 (rig, GHD, plataformas, ergómetros)"),
        new Spec("Yoga Hatha", "", 16, "esterillas", "dos salas por piso (occidental y tradicional) de 8 esterillas: con ≥ 2 m entre ellas y a ≤ 9 m del maestro no caben más en 18 × 26 m"),
        new Spec("Yoga Ashtanga", "", 16, "esterillas", "como el de Hatha: dos salas de 8 esterillas por piso"),
        new Spec("Dojo", "artes marciales", 25, "puestos", "tatami de 8 × 8 (16 a 2 m), jaula 2, ring 2, sacos 3, makiwaras 2"),
        new Spec("Ballet", "ballet", 20, "plazas", "suelo de 18 × 8,6 m y barras dobles (16–23 alumnos, docs/obras-dojo-ballet.md)"),
    };

    public static Spec? Of(string centre) { foreach (Spec s in All) if (s.name == centre) return s; return null; }

    /// <summary>Pisos que necesita un centro con <paramref name="perFloor"/> plazas por piso para <paramref name="members"/> miembros.</summary>
    public static int FloorsFor(int perFloor, int members) => Mathf.Max(1, Mathf.CeilToInt(members / (float)Mathf.Max(1, perFloor)));
    public static int FloorsFor(string centre, int members) { Spec? s = Of(centre); return s.HasValue ? FloorsFor(s.Value.perFloor, members) : 1; }
}
