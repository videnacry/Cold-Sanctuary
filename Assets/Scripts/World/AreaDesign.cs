/// <summary>
/// DE DÓNDE SALE EL DISEÑO DE CADA ÁREA DEL SANTUARIO (Berón 2026-11-05: «en el santuario se crean las áreas como mix de lo mejor del mundo con lo que puede tener
/// maquinaria/diseño con componentes de diversos países y culturas»; reglas S14 y F14 del manual; docs/piso-asambleas.md §Auditoría). Una fila por área: si su diseño está
/// investigado y de qué culturas viene lo que ya tiene. <see cref="State.Cumple"/> = investigado y ≥ 3 culturas; lo demás está pendiente y es la lista de trabajo.
/// </summary>
public static class AreaDesign
{
    public enum State { Cumple, Parcial, Falta }

    public struct Row
    {
        public string area, doc, note; public State state; public string[] cultures;
        public Row(string area, State state, string doc, string note, params string[] cultures) { this.area = area; this.state = state; this.doc = doc; this.note = note; this.cultures = cultures; }
    }

    public static readonly Row[] All =
    {
        new Row("Sala de asambleas", State.Cumple, "piso-asambleas.md", "cada pieza con su origen", "Islandia (Lögberg)", "Haudenosaunee (círculo y archivo)", "Botsuana (kgotla)", "Mali (toguna)", "Atenas (kleroterion y clepsidra)", "Alemania (cono del Reichstag)", "ONU (cabina de intérpretes)", "Euskadi (roble de Gernika)", "India (panchayat)", "Andes (quipu)"),
        new Row("Cocina", State.Cumple, "cocina-profesional.md", "wok, comal y brigada de cocina", "China (wok)", "México (comal)", "Francia (brigada)"),
        new Row("Dojo (artes marciales)", State.Cumple, "obras-dojo-ballet.md", "tatami, karate, kickboxing y MMA", "Japón (tatami, karate)", "Tailandia y Países Bajos (kickboxing)", "EE. UU. y Brasil (MMA)"),
        new Row("Música", State.Cumple, "piso-musica.md", "acústica investigada; instrumentos del mundo en el banco del técnico (2026-11-05)", "Alemania y Austria (piano)", "India (sitar)", "Andes (charango)", "Mali (kora)", "Japón (shamisen)"),
        new Row("Aula", State.Cumple, "piso-aula.md", "instrumentos del mundo en la mesa del profesor (2026-11-05)", "China (ábaco)", "Al-Ándalus y Persia (astrolabio)", "Andes (quipu)", "Grecia (esfera)"),
        new Row("Sala de Yoga", State.Parcial, "yoga-pisos.md", "dos culturas: falta una tercera", "India (kund, linga, kusha)", "Occidente (estudio)"),
        new Row("Gimnasio", State.Parcial, "gimnasio-maquinas.md", "máquinas investigadas; mezcla por añadir (mazas persas e indias, girya rusa, ya en los microcosmos)", "Rusia (kettlebell)", "EE. UU. (máquinas de placas)"),
        new Row("Sala de Ballet", State.Parcial, "obras-dojo-ballet.md", "solo ballet clásico", "Francia, Italia y Rusia (ballet)"),
        new Row("Estudio Textil", State.Parcial, "", "sin investigación propia: telares y rueca sin origen", "Sin identificar"),
        new Row("Planta Atómica", State.Parcial, "laboratorios-investigacion.md", "investigada; una sola tradición", "EE. UU. (TRIGA)"),
        new Row("Central Nuclear", State.Parcial, "sitio-nuclear.md", "seguridad investigada; origen por repartir", "Sin repartir"),
        new Row("Control de misión", State.Parcial, "lanzadera-de-cohetes.md", "regla de altura investigada; diseño sin mezcla", "Sin repartir"),
        new Row("Informática", State.Parcial, "piso-informatica.md", "técnica investigada; sin mezcla cultural", "Sin repartir"),
        new Row("Hospital", State.Falta, "hospital.md", "muy investigado en medicina; sin mezcla de culturas de curar"),
        new Row("Química", State.Falta, "laboratorios-investigacion.md", "técnica; sin mezcla"),
        new Row("Biología", State.Falta, "laboratorios-investigacion.md", "técnica; sin mezcla"),
        new Row("Construcción", State.Cumple, "construccion-sostenible.md", "muestrario de técnicas en el borde +Z (2026-11-05)", "Colombia (guadua y bahareque)", "Iberia, Magreb y Andes (tapia)", "Japón (kigumi)", "Nubia y Egipto (bóveda sin cimbra)", "Perú (sillería inca)"),
        new Row("Mecánica", State.Falta, "", "sin investigación de diseño"),
        new Row("Huerto", State.Cumple, "huertos-y-cultivos.md", "seis métodos del mundo junto al cultivo (2026-11-05)", "México (chinampa)", "Burkina Faso (zaï)", "Andes (andén)", "Europa central (hügelkultur)", "Corea (batdam)", "Bali (subak)"),
        new Row("Calistenia", State.Falta, "", "sin investigación de diseño"),
        new Row("Cría", State.Falta, "cria-simulation.md", "sin investigación de diseño"),
        new Row("Carne Cultivada", State.Falta, "carne-cultivada.md", "técnica; sin mezcla"),
    };

    public static Row? Of(string area) { foreach (Row r in All) if (r.area == area) return r; return null; }
}
