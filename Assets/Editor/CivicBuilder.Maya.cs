using UnityEngine;

/// <summary>
/// LAKAMHA' (PALENQUE), 683 (docs/microcosmos-maya-pakal.md). La ciudad de K'inich Janaab Pakal sobre su plano real
/// (CivicBuilder.Plans, «Palenque»): el PALACIO en terrazas (la Casa E, la Casa Blanca donde está el Tablero Oval de su coronación,
/// y la torre), el TEMPLO DE LAS INSCRIPCIONES (nueve cuerpos escalonados; aquí cuatro terrazas con su rampa: los 617 glifos de sus
/// tableros arriba), el JUEGO DE PELOTA, la casa de los escribas, la cocina del Palacio, el Grupo de las Cruces (la Cruz Foliada:
/// el maíz), la casa de la familia real; y el pueblo: casas de mampostería enlucida con su huerto de cacao, talleres de jade y
/// obsidiana, milpas. Todo abierto (las terrazas no tapan nada), con muebles mayas (banca con petate, k'oben de tres piedras, ollas)
/// a la medida de los insectos-humanos y de las personas, y frutales mayas (cacao, aguacate, ramón, zapote, papaya).
/// (Barnhart 2001, The Palenque Mapping Project; Stuart y Stuart 2008, Palenque: Eternal City of the Maya; Martin y Grube 2008.)
/// </summary>
internal static partial class CivicBuilder
{
    static readonly Style MayaPlaster = new Style(C(0.78f, 0.36f, 0.28f), C(0.88f, 0.85f, 0.78f), C(0.85f, 0.82f, 0.74f), Roof.None, 3.2f);   // estuco rojo y suelo de cal
    static readonly Style MayaStone = new Style(C(0.8f, 0.77f, 0.68f), C(0.84f, 0.81f, 0.72f), C(0.8f, 0.77f, 0.68f), Roof.None, 3.2f);     // caliza de los templos

    static int PalenqueTown(Transform t)
    {
        var s = Town(t, "Palenque", new[]
        {
            new Lot("Palacio de Palenque (Casa E, la Casa Blanca)", "Escuela", "Palacio: la Casa Blanca (Sak Nuk Naah) y la torre", 0f, 0f, MayaPlaster)
            { floors = new[] {
                new[] { S("mesa", 0, 0, 1.6f, 1f, "Trono de la Casa E (el Tablero Oval: Sak K'uk' corona a Pakal, 615)"), S("estante", 0, 0, 0.8f, 2f, "Códices de papel amate") },
                new[] { S("pizarra", 0, 0, 1.6f, 0.4f, "Tablero de estuco (cuentas del calendario)"), S("mesa", 0, 0, 1.6f, 1f, "Mesa de los consejeros") },
                new[] { S("consola", 0, 0, 1.6f, 0.7f, "La torre: mirar el sol del solsticio") } } },
            new Lot("Templo de las Inscripciones", "Templo", "Templo de las Inscripciones (la tumba de Pakal)", 0f, 0f, MayaStone)
            { floors = new[] {
                new[] { S("altar", 0, 0, 1.6f, 1f, "Altar al pie de la escalinata") },
                new[] { S("estatua", 0, 0, 1.4f, 1.4f, "Incensario de barro (el dios del sol)") },
                new[] { S("estatua", 0, 0, 1.4f, 1.4f, "Incensario de barro (el dios jaguar)") },
                new[] { S("altar", 0, 0, 1.6f, 1f, "Santuario: los tres tableros (617 glifos)") } } },
            new Lot("Juego de pelota", "Centro de entrenamiento", "Juego de pelota de Lakamha'", 16f, 10f, MayaStone,
                S("pista", 0f, 0f, 6f, 6f, "Cancha del juego de pelota (la pelota de hule)"), S("armario", 0f, 0f, 1.2f, 0.6f, "Yugos y protectores de cadera")),
            new Lot("Casa de los escribas", "Escuela", "Casa de los escribas (aj tz'ib)", 14f, 10f, MayaPlaster,
                S("mesa", 0f, 0f, 1.6f, 1f, "Mesa del escriba: pinceles, conchas de tinta y papel amate"), S("estante", 0f, 0f, 0.8f, 2f, "Códices plegados"), S("pizarra", 0f, 0f, 1.6f, 0.4f, "Tablero de glifos (aprender a escribir)")),
            new Lot("Cocina del Palacio", "Cocina", "Cocina del Palacio (tamales y cacao)", 14f, 10f, MayaPlaster,
                S("fogon", 0f, 0f, 1.6f, 0.8f, "Comal sobre las tres piedras (k'oben)"), S("granero", 0f, 0f, 2f, 2f, "Metate y ollas del nixtamal"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa baja (jícaras de cacao)")),
            new Lot("Templo de la Cruz Foliada", "Templo", "Templo de la Cruz Foliada (el maíz que renace)", 12f, 10f, MayaStone,
                S("altar", 0f, 0f, 1.6f, 1f, "Tablero de la Cruz Foliada (la planta de maíz con rostros)")),
            new Lot("Casa de la familia real", "Casa", "Casa de Pakal y Tz'akbu Ajaw", 14f, 10f, MayaPlaster,
                S("cama", 0f, 0f, 1.2f, 2f, "Banca con petate de Pakal"), S("cama", 0f, 0f, 1.2f, 2f, "Banca con petate de Tz'akbu Ajaw"), S("cama", 0f, 0f, 1.2f, 2f, "Banca de los hijos")),
        }, new[]
        {
            new Lot("Casa de Lakamha'", "Casa", "Casa maya (mampostería enlucida y su huerto de cacao)", 10f, 9f, MayaPlaster),
            new Lot("Taller de jade y obsidiana", "Taller", "Taller de jade y obsidiana", 10f, 8f, MayaPlaster, S("banco_taller", 0f, 0f, 2f, 0.9f, "Banco del lapidario")),
            new Lot("Milpa", "Huerto", "Milpa (maíz, frijol y calabaza)", 12f, 10f, MayaPlaster, S("bancal", 0f, 0f, 4f, 6f, "Milpa: maíz, frijol y calabaza")),
            new Lot("Alfarería", "Alfarería", "Alfarería (ollas y vasos de cacao)", 10f, 8f, MayaPlaster, S("torno_alfarero", 0f, 0f, 1f, 1f, "Moldes y ollas (sin torno: se levantan a mano)")),
        }, C(0.82f, 0.8f, 0.72f));
        int n = 0;
        n += W(t, "Escriba de Pakal", s[3], "sabio", s[4], s[0], null);
        n += W(t, "Escriba aprendiz", s[3], "alumno", s[4], s[0], null);
        n += W(t, "Cocinera del Palacio", s[4], "cocinero", s[4], null, null);
        n += W(t, "Jugador de pelota", s[2], "alumno", s[4], s[2], null);
        n += W(t, "Sacerdote del Templo", s[1], "", s[4], null, null);
        n += W(t, "Sacerdotisa de la Cruz Foliada", s[5], "", s[4], null, null);
        n += C2(t, "Tz'akbu Ajaw", "Consejero de Pakal", s[6], s[4], s[1]);
        n += Townsfolk(t, s, 7, "Vecina de Lakamha'", "Vecino de Lakamha'", s[4], s[1]);
        return n;
    }
}

/// <summary>
/// EGINA (la Grecia heroica del mito de los mirmidones, ~1250 a. C.; docs/microcosmos-myrmidons.md): la ciudad de Éaco sobre su
/// plano (CivicBuilder.Plans, «Egina»): el puerto y la colina de Kolona (el santuario de Apolo, el asentamiento de la Edad del
/// Bronce) al oeste, el monte Oros con el santuario de Zeus Helenio al sureste, el palacio de Éaco, el taller del alfarero y los
/// olivares. (Walter y Felten 1981, Alt-Ägina; Pausanias II.29–30.)
/// </summary>
internal static partial class CivicBuilder
{
    static int EginaTown(Transform t)
    {
        var s = Town(t, "Egina", new[]
        {
            new Lot("Palacio de Éaco", "Casa", "Palacio de Éaco (el rey de los mirmidones)", 14f, 10f, GreekStone,
                S("cama", 0f, 0f, 1.2f, 2f, "Kliné del rey"), S("fogon", 0f, 0f, 1.6f, 0.8f, "Hogar del palacio"), S("mesa", 0f, 0f, 1.6f, 1f, "Mesa del consejo")),
            new Lot("Santuario de Zeus Helenio", "Templo", "Santuario de Zeus Helenio (monte Oros)", 12f, 10f, GreekStone, S("altar", 0f, 0f, 2f, 1f, "Altar de Zeus (la lluvia)")),
            new Lot("Puerto de Kolona", "Mercado", "Puerto de Kolona", 14f, 10f, GreekStone, S("puesto", 0f, 0f, 2f, 1f, "Barca con ánforas"), S("puesto", 0f, 0f, 2f, 1f, "Pescado y sal")),
            new Lot("Taller del alfarero", "Alfarería", "Taller del alfarero de Egina", 10f, 8f, GreekStone, S("torno_alfarero", 0f, 0f, 1f, 1f, "Torno de alfarero")),
        }, new[]
        {
            new Lot("Casa de Egina", "Casa", "Casa de Egina", 10f, 9f, GreekStone),
            new Lot("Olivar", "Huerto", "Olivar de Egina", 12f, 10f, GreekStone, S("bancal", 0f, 0f, 4f, 6f, "Olivos y cebada")),
            new Lot("Taller de bronce", "Taller", "Taller del broncista", 10f, 8f, GreekStone, S("banco_taller", 0f, 0f, 2f, 0.9f, "Yunque y crisol")),
        }, C(0.7f, 0.62f, 0.5f));
        int n = 0;
        n += W(t, "Alfarero de Egina", s[3], "alfarero", s[0], null, null);
        n += W(t, "Pescador de Kolona", s[2], "comerciante", s[0], null, null);
        n += W(t, "Sacerdotisa de Zeus Helenio", s[1], "", s[0], null, null);
        n += Townsfolk(t, s, 4, "Vecina de Egina", "Vecino de Egina", s[0], s[1]);
        return n;
    }
}
