using System.Collections.Generic;

/// <summary>
/// LAS PROFESIONES DEL PERSONAL (Berón 2026-11-04; docs/personajes-mesocosmos.md §Profesiones y parejas).
/// La profesión da a cada uno su papel en la jornada sin quitarle la rotación (regla J2, todos aprenden todo):
///  · <see cref="WorkRoster"/> le da cada día, ANTES de las vueltas, UNA misión de cada área de su profesión (la que menos ha hecho; si
///    la profesión nombra misiones —«rep_» las reparaciones, «obra_» la obra nueva—, de esas);
///  · en una misión de GRUPO de su profesión (la clase de yoga, una obra), el profesional, si está libre, entra en el grupo y, si hay
///    papel que dirige, lo dirige;
///  · su área de salida (<see cref="SanctuaryDirector"/>) y su rótulo.
/// Las áreas son las de las misiones (<see cref="AreaFunctions"/>, <see cref="AreaWork"/>): «Sala de Yoga», «Construcción», «Cocina»,
/// «Huerto», «Química», «Aula», «Mecánica», «Hospital», «Guardia», «Estudio Textil»…
/// </summary>
public static class Professions
{
    public struct Entry
    {
        public string who, title;
        /// <summary>«Área» o «Área:prefijo1,prefijo2» (solo esas misiones del área).</summary>
        public string[] areas;
        public Entry(string w, string t, params string[] a) { who = w; title = t; areas = a; }
    }

    public static readonly Entry[] All =
    {
        // Josedrilo: maestro de yoga (YogaTeacher), JEFE DE OBRA NUEVA (las obras: «obra_») y bioconstrucción (el material: «mat_»).
        new Entry("Josedrilo", "maestro de yoga, jefe de obra nueva y bioconstrucción", "Sala de Yoga", "Construcción:obra_,mat_"),
        // Tote: JEFE DE REPARACIONES («rep_») y guardia.
        new Entry("JefeConstruccion", "jefe de reparaciones y guardia", "Construcción:rep_", "Guardia"),
        // Los guardias: Gohageneis (y las artes marciales), Nesvitor (y agricultor) y Tote.
        new Entry("Gohageneis", "guardia, artes marciales y maestro de baile", "Guardia"),
        // Panterilia: MAESTRA DE BAILE y FINANZAS (la mesa de cuentas del restaurante), y segunda maestra de yoga (SecondYogaTeacher).
        new Entry("Panterilia", "maestra de baile, finanzas y segunda maestra de yoga", "Cocina:coc_cuentas"),
        // Julian: DIRECCIÓN DE PROYECTOS, con Alicita (con_proyectos y con_proyectos2: uno cada uno).
        new Entry("Julian", "dirección de proyectos", "Construcción:con_proyectos"),
        new Entry("Nesvitor", "guardia y agricultor", "Guardia", "Huerto"),
        // Maite: jefa de cocina con Amykhuz, y CURANDERA (las plantas medicinales y las curas del hospital).
        new Entry("Maite", "jefa de cocina y curandera", "Cocina", "Hospital:hos_plantas,enf_visita"),
        new Entry("Amykhuz", "jefe de cocina y química", "Cocina", "Química"),
        new Entry("Shalva", "cultivo y bioconstrucción", "Huerto", "Construcción:mat_"),
        // Submarinismo: los submarinos (el de obra y la ambulancia) y las tareas bajo el agua de los árboles salen de Construcción.
        new Entry("Adaguchi", "submarinismo y cocina", "Construcción", "Cocina"),
        // Asuntos legales y lenguas: aún no hay área propia; su mesa está en la sala de estudio del Aula, donde da clase de lenguas.
        new Entry("Karen", "asuntos legales y lenguas", "Aula:aul_legal,aul_lenguas"),
        new Entry("Alicita", "contabilidad y dirección de proyectos", "Cocina:coc_cuentas", "Construcción:con_proyectos"),
        // Ducristin: GERENTE DE BANCA Y PROFESOR. No hay banco: lleva las finanzas en la mesa de cuentas del restaurante (donde entra el
        // dinero del santuario) y da clase en el Aula. Su mecánica pasa a Fausto.
        new Entry("Ducristin", "gerente de banca y profesor", "Cocina:coc_cuentas", "Aula:aul_clase"),
        new Entry("Fausto", "mecánico", "Mecánica"),
        // Andrea: COMUNICACIÓN (los avisos del santuario y la radio, en el comedor del restaurante —la torre de Servicios—, donde coinciden
        // todos a la hora de comer). Su papel de entrenador del gimnasio lo pone la rama del entrenamiento.
        new Entry("Andrea", "comunicación", "Cocina:coc_avisos"),
        // Irosene: LETRAS (clases de poesía y narración y lecturas en la sala de estudio del Aula) y su textil.
        new Entry("Irosene", "letras (poesía y narración) y textil", "Aula:aul_letras", "Estudio Textil"),
        // LOS SEIS NUEVOS (Berón 2026-11-04). Informática: el clúster de IA (sistemas, refrigeración y energía).
        new Entry("David", "informática", "Informática"),
        new Entry("Adonis", "informática", "Informática"),
        // Biotecnología: el laboratorio de carne cultivada y la biología (con los fotobiorreactores del árbol, que se cuidan desde Biología).
        new Entry("Julito", "biotecnología", "Carne Cultivada", "Biología"),
        new Entry("Ale", "biotecnología", "Carne Cultivada", "Biología"),
        // Pediatra: en el Hospital (pediatría de los niños de las personas; la Cría es de animales y le toca en la rotación como a todos).
        new Entry("Ivelyna", "pediatría", "Hospital:hos_pediatria,enf_"),
        // Piloto: la flota de alfombras voladoras y el transporte aéreo (en la Mecánica) y el vuelo y el control de misión de la lanzadera.
        // Los submarinos siguen con Adaguchi.
        new Entry("Luciana", "piloto: alfombras, transporte aéreo y lanzadera", "Mecánica:mec_alfombras", "Control de misión"),
    };

    /// <summary>(2026-11-04) Los MAESTROS de una clase de entrenamiento o de ocio que no es misión de grupo: el baile (Berón: «Panterilia y
    /// Gohageneis, maestros de baile»). Se turnan por días (el día d, el d mod n). En la jornada dan el entrenamiento de la Sala de Ballet
    /// cuando está terminada; en el horario libre guían el baile de las 20 h en su sitio (la Sala de Ballet o, mientras está en obras, la
    /// calistenia o el gimnasio).</summary>
    public static readonly Dictionary<string, string[]> ClassLeads = new Dictionary<string, string[]>
    {
        { "Sala de Ballet", new[] { "Panterilia", "Gohageneis" } },
        { "baile", new[] { "Panterilia", "Gohageneis" } },
    };
    /// <summary>El maestro de <paramref name="cls"/> el día <paramref name="day"/> (null si no tiene).</summary>
    public static string LeadOn(string cls, int day) => ClassLeads.TryGetValue(cls, out string[] l) && l.Length > 0 ? l[((day % l.Length) + l.Length) % l.Length] : null;

    static Dictionary<string, Entry> _by;
    public static Entry? Of(string who)
    {
        if (_by == null) { _by = new Dictionary<string, Entry>(); foreach (Entry e in All) _by[e.who] = e; }
        return who != null && _by.TryGetValue(who.Replace("_Post", ""), out Entry x) ? x : (Entry?)null;
    }

    /// <summary>Las áreas de su profesión (sin los prefijos).</summary>
    public static string[] AreasOf(string who)
    {
        Entry? e = Of(who);
        if (!e.HasValue) return new string[0];
        var l = new string[e.Value.areas.Length];
        for (int i = 0; i < l.Length; i++) { string s = e.Value.areas[i]; int c = s.IndexOf(':'); l[i] = c < 0 ? s : s.Substring(0, c); }
        return l;
    }

    /// <summary>¿Es de su profesión el área (y, si se da, la misión <paramref name="missionId"/>)?</summary>
    public static bool Practises(string who, string area, string missionId = null)
    {
        Entry? e = Of(who);
        if (!e.HasValue) return false;
        foreach (string s in e.Value.areas)
        {
            int c = s.IndexOf(':');
            string a = c < 0 ? s : s.Substring(0, c);
            if (a != area) continue;
            if (c < 0 || missionId == null) return true;
            foreach (string p in s.Substring(c + 1).Split(',')) if (missionId.StartsWith(p)) return true;
        }
        return false;
    }
}
