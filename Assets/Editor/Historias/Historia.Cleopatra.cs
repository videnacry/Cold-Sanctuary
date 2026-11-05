using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CLEOPATRA Y LOS AÑOS SIN CRECIDA (docs/microcosmos-historias-3.md). Alejandría, verano del 42 a. C.: el Nilo lleva dos años sin crecer y una
/// reina que habla la lengua de cada uno de sus pueblos tiene que dar de comer a la mayor ciudad del Mediterráneo. La historia va de gobernar,
/// no de los romanos ni de su muerte (una línea de la ficha). Las frases son de juego; los hechos, de Plutarco, Séneca, Plinio y Roller (2010).
/// </summary>
public static partial class StorySceneBuilder
{
    static int Cleopatra(Transform t)
    {
        Material marble = M("Marmol_de_Alejandria", 0.92f, 0.9f, 0.84f), limestone = M("Caliza_egipcia", 0.82f, 0.74f, 0.6f), gold = M("Dorado_real", 0.85f, 0.68f, 0.3f), grain = M("Saco_de_trigo", 0.78f, 0.66f, 0.42f);
        Material water = M("Agua_del_Gran_Puerto", 0.25f, 0.48f, 0.58f), wood = M("Cedro", 0.5f, 0.34f, 0.22f), papyrus = M("Papiro", 0.86f, 0.8f, 0.62f), blue = M("Azul_egipcio", 0.15f, 0.35f, 0.65f);

        // 1 · EL MUELLE REAL: el Gran Puerto, a lo lejos el Faro.
        Transform quay = Spot(t, "Muelle del Gran Puerto", new Vector3(0f, 0f, 18f), "El Gran Puerto: el Faro, al fondo", 3f);
        Prop(quay, "Muelle de piedra", new Vector3(0f, 0.2f, 0f), new Vector3(18f, 0.4f, 3f), limestone);
        Prop(quay, "Mar", new Vector3(0f, 0.02f, 5f), new Vector3(30f, 0.04f, 7f), water, false);
        Prop(quay, "Barca del grano", new Vector3(-5f, 0.5f, 3f), new Vector3(6f, 1f, 2f), wood);

        // 2 · EL NILÓMETRO: la escala de la crecida (Alejandría tiene el suyo en el templo; aquí, la copia del de Menfis).
        Transform nilometer = Spot(t, "Nilómetro", new Vector3(-15f, 0f, 9f), "La escala del Nilo: este año no ha llegado a la marca", 3f);
        Prop(nilometer, "Columna con la escala", new Vector3(0f, 1.6f, 0f), new Vector3(0.5f, 3.2f, 0.5f), limestone);
        for (int k = 0; k < 8; k++) Prop(nilometer, "Marca del codo", new Vector3(0f, 0.3f + k * 0.38f, -0.27f), new Vector3(0.4f, 0.04f, 0.02f), blue, false);
        Prop(nilometer, "Pila con poca agua", new Vector3(1.8f, 0.15f, 0f), new Vector3(2f, 0.3f, 2f), water, false);

        // 3 · LOS GRANEROS REALES: tres sacos que hay que llevar al reparto.
        Transform dole = Spot(t, "Puesto del reparto", new Vector3(14f, 0f, 9f), "El reparto de grano de la reina", 2.4f);
        Prop(dole, "Mesa del escriba", new Vector3(0f, 0.4f, 0f), new Vector3(2.4f, 0.8f, 1f), wood);
        Prop(dole, "Rollo de cuentas", new Vector3(0f, 0.84f, 0f), new Vector3(1.4f, 0.06f, 0.3f), papyrus, false);
        var sacks = Items(t, grain, new Vector3(0.7f, 0.6f, 0.5f), ("Saco de trigo de los graneros reales", new Vector3(19f, 0.3f, 3f)), ("Saco de trigo de los graneros reales", new Vector3(21f, 0.3f, -2f)), ("Saco de trigo de los graneros reales", new Vector3(16f, 0.3f, -6f)));

        // 4 · LOS TRES BARRIOS: griegos, egipcios y judíos.
        Transform greek = Spot(t, "Barrio griego", new Vector3(-6f, 0f, -11f), "El Brucheion: griego", 2.2f);
        Prop(greek, "Columna jónica", new Vector3(0f, 1.3f, 0f), new Vector3(0.5f, 2.6f, 0.5f), marble);
        Transform rhakotis = Spot(t, "Barrio egipcio", new Vector3(-17f, 0f, -5f), "Rhakotis: egipcio", 2.2f);
        Prop(rhakotis, "Pequeño obelisco", new Vector3(0f, 1.3f, 0f), new Vector3(0.5f, 2.6f, 0.5f), limestone);
        Transform delta = Spot(t, "Barrio judío", new Vector3(8f, 0f, -13f), "El barrio Delta: hebreo y arameo", 2.2f);
        Prop(delta, "Mesa de la sinagoga", new Vector3(0f, 0.4f, 0f), new Vector3(1.8f, 0.8f, 1f), wood);
        var quarters = new List<Transform> { greek, rhakotis, delta };

        // 5 · EL MOUSEION: la mesa de Sosígenes y su calendario, y los rollos.
        Transform museum = Spot(t, "Pórtico del Mouseion", new Vector3(16f, 0f, 17f), "El Mouseion: el saber vive de que haya pan", 3f);
        for (int k = 0; k < 3; k++) Prop(museum, "Columna del pórtico", new Vector3(-3f + k * 3f, 1.5f, 1.5f), new Vector3(0.5f, 3f, 0.5f), marble);
        Prop(museum, "Mesa con rollos y esfera", new Vector3(0f, 0.4f, -0.5f), new Vector3(2.6f, 0.8f, 1f), wood);
        Prop(museum, "Esfera armilar", new Vector3(0f, 1.1f, -0.5f), new Vector3(0.6f, 0.6f, 0.6f), gold, false);

        // 6 · EL SOL DE LA MAÑANA: la terraza de la reina.
        Transform terrace = Spot(t, "Terraza de Lochias", new Vector3(5f, 0f, 11f), "La terraza del cabo Lochias", 2.2f);
        Prop(terrace, "Asiento de mármol", new Vector3(0f, 0.25f, 0f), new Vector3(1.8f, 0.5f, 0.7f), marble);

        // La gente: insectos-humanos. Cleopatra, una ABEJA (el signo del rey del Bajo Egipto es la abeja; la colmena guarda para los años
        // malos), con túnica clara y diadema griega.
        Color white = new Color(0.94f, 0.92f, 0.86f), blueC = new Color(0.15f, 0.35f, 0.65f), linen = new Color(0.86f, 0.8f, 0.68f), wine = new Color(0.45f, 0.14f, 0.2f), ochre = new Color(0.7f, 0.5f, 0.25f);
        GameObject queen = Ancient(t, "Cleopatra", new Vector3(3f, 0f, 9f), true, white, false, 1.6f, "SnowBee");
        Prop(queen.transform, "Diadema", new Vector3(0f, 1.62f, 0f), new Vector3(0.34f, 0.04f, 0.34f), gold, false);
        GameObject priest = Ancient(t, "Pshereniptah, sacerdote de Ptah (Atlas)", new Vector3(-13f, 0f, 7f), false, linen, false, 1.68f, "DungBeetle");
        GameObject scribe = Ancient(t, "Escriba del reparto (Ruth)", new Vector3(12f, 0f, 7f), true, ochre, false, 1.6f, "Ant"); Routine(scribe, dole);
        GameObject sosigenes = Ancient(t, "Sosígenes", new Vector3(14f, 0f, 15f), false, wine, true, 1.7f, "Spider"); Routine(sosigenes, museum);
        GameObject baker = Ancient(t, "Panadera de Rhakotis (Medea)", new Vector3(-15f, 0f, -7f), true, blueC, false, 1.58f, "Ant"); Routine(baker, rhakotis, delta);
        GameObject iras = Ancient(t, "Iras", new Vector3(7f, 0f, 9f), true, blueC, false, 1.6f, "SnowBee");
        int n = 6;

        Mission(t, "Cleopatra en Alejandría: los años sin crecida (42 a. C.)", StoryMission.Marga.Bonds, 60f,
            new[]
            {
                Talk("Habla con Cleopatra en el muelle real", queen.transform, "Cleopatra", "Dos años sin crecida. Sin crecida no hay limo; sin limo no hay trigo; y sin trigo, en esta ciudad de cientos de miles, hay motín. Mi padre habría pedido ayuda a Roma. Yo prefiero contar lo que tengo."),
                Reach("Ve al nilómetro con el sacerdote", nilometer, "Sacerdote", "La reina me ha hablado en egipcio. Doscientos cincuenta años de reyes griegos, y es la primera de su casa que se ha molestado en aprender nuestra lengua. Mira la marca: el río no llega ni a doce codos."),
                Collect("Lleva tres sacos de los graneros reales al reparto", sacks, dole, "Escriba del reparto", "Cada saco, apuntado. La reina manda que el trigo de los nomos venga aquí y se reparta, y que los campesinos no paguen lo que no pueden. Las cuentas se hacen en griego y en demótico."),
                Visit("Ve con la reina a los tres barrios: griego, egipcio y judío", quarters, "Panadera", "A cada barrio le habla en su lengua, y no le hace falta intérprete. Por eso la escuchan. El pan sale igual de pequeño, pero sale."),
                Talk("Habla con Iras, la consejera de la reina", iras.transform, "Iras", "Desde que murió César no duerme. Tiene un hijo pequeño que proteger, un reino que Roma mira con hambre, y una ciudad que comer. Y aún saca una hora para leer."),
                Reach("Ve al Mouseion", museum, "Sosígenes", "Yo ayudé a César a arreglar el calendario: 365 días y uno más cada cuatro años. Ahora la reina me pregunta por otra cosa: cuántos días de trigo quedan. Un astrónomo también sabe contar eso."),
                Hold("En la terraza de Lochias, mira cómo entran las barcas", terrace, 10f, "Cleopatra", "Allí vienen. No es mucho, pero es a tiempo. Gobernar es esto: contar, hablar con todos, y que mañana haya pan."),
                Talk("Vuelve con Cleopatra", queen.transform, "Cleopatra", "Los romanos escribirán de mí muchas cosas. Tú has visto un día de trabajo. Cuéntalo."),
            },
            "Alejandría, verano del 42 a. C., sobre la Cocina del santuario a escala de hormiga. El Nilo lleva dos años sin crecer y Cleopatra VII, última reina de los Ptolomeos, tiene que dar de comer a su capital. Acompáñala un día de gobierno. E para hablar y recoger; para mirar, quédate quieto.",
            new[]
            {
                "Cleopatra VII Filopátor (69–30 a. C.) nació en Alejandría, en una dinastía griega, la de los Ptolomeos, que reinaba en Egipto desde Alejandro Magno. Subió al trono en 51 a. C. Según Plutarco, «su lengua era como un instrumento de muchas cuerdas»: hablaba griego, egipcio (la primera de su casa que lo aprendió), etíope, hebreo, arameo, árabe, medo y parto, y trataba a la mayoría de los pueblos sin intérprete. Su primer acto conocido fue viajar a Hermontis para instalar al toro sagrado Buquis.",
                "Su reinado tuvo crecidas muy malas. Séneca cuenta que el Nilo no creció en los años décimo y undécimo de su reinado (43–41 a. C.). Un decreto del 50 a. C. mandaba enviar a Alejandría todo el grano del Egipto medio, y una estela de Tebas del 39 a. C. honra a un gobernador que salvó a la región del hambre. Esta escena reúne esos datos: no se conserva el relato de un día concreto. Josefo la acusa de no dar grano a los judíos de Alejandría; es una fuente muy hostil a ella, y no se ha podido comprobar.",
                "Alejandría tenía el Faro, una de las maravillas del mundo, la Biblioteca y el Mouseion, y barrios griegos, egipcios y judíos. Sosígenes, astrónomo de Alejandría, ayudó a Julio César a crear el calendario juliano, del que sale el nuestro (Plinio). Cleopatra tuvo un hijo con César, Cesarión, y tres con Marco Antonio. Tras la derrota de Accio (31 a. C.), murió en Alejandría en el 30 a. C. y Egipto pasó a ser provincia romana.",
                "Casi todo lo que sabemos viene de sus enemigos romanos, que la pintaron como seductora. No conservamos sus palabras, y su aspecto se discute. Aquí se la muestra por lo que se sabe de su trabajo: gobernar. Pshereniptah fue sumo sacerdote de Ptah en Menfis en su tiempo; Iras aparece en Plutarco como una de sus damas; los demás son inventados.",
            },
            new[]
            {
                "Plutarco, Vida de Antonio, 27 (las lenguas).",
                "Séneca, Cuestiones naturales, IV a, 2, 16 (el Nilo sin crecida); Plinio, Historia natural, XVIII, 57 (Sosígenes); Josefo, Contra Apión, II, 60.",
                "Roller, D. W. (2010). Cleopatra: A Biography. Oxford University Press.",
                "Tyldesley, J. (2008). Cleopatra: Last Queen of Egypt. Profile.",
                "Estela de Calímaco (Tebas, 39 a. C.); Museo Egizio de Turín, Rivista del Museo Egizio; decreto de 50 a. C. (BGU VIII 1730).",
                "Fraser, P. M. (1972). Ptolemaic Alexandria. Clarendon Press.",
            },
            "Cleopatra", "Una reina que no entiende a su pueblo gobierna solo a sus intérpretes.");
        return n;
    }
}

internal static partial class CivicBuilder
{
    static readonly Style PtolemaicStone = new Style(C(0.9f, 0.86f, 0.76f), C(0.76f, 0.7f, 0.58f), C(0.72f, 0.42f, 0.28f), Roof.Flat, 4f) { columns = true };   // caliza enlucida y mármol
    static readonly Style AlexandrianHouse = new Style(C(0.86f, 0.8f, 0.68f), C(0.65f, 0.55f, 0.42f), C(0.72f, 0.42f, 0.28f), Roof.Flat, 3.4f);              // casa de ciudad, con terraza

    // ALEJANDRÍA (42 a. C.): el mar al norte con el Gran Puerto, el Heptastadion y la isla de Faro; la Vía Canópica de este a oeste, la calle del
    // Soma de norte a sur (cuadrícula de Dinócrates), el Brucheion y los palacios al noreste, el cabo Lochias, Rhakotis y el Serapeo al suroeste,
    // el barrio Delta al este y el lago Mareotis al sur. Plano aproximado y ACORTADO de sur a norte (Mahmoud-Bey 1872; McKenzie 2007, The
    // Architecture of Alexandria and Egypt; Fraser 1972).
    const string PlanAlejandria = @"
mar Mediterráneo | 2800 700 | 0 1050
calle Vía Canópica | 24 | -1400 -260, 1400 -260
calle Calle del Soma | 14 | 60 -900, 60 620
calle Calle del Brucheion | 10 | -300 160, 1000 160
calle Calle del puerto | 10 | -900 440, 900 440
calle Calle de Rhakotis | 8 | -1300 -560, -300 -560
calle Calle del Mareotis | 8 | -800 -860, 800 -860
calle Canal del Nilo a Canopo | 12 | -1400 -760, 1400 -700
colina Paneo (la colina de Pan) | 110 40 | 340 -440
colina Colina del Serapeo | 150 30 | -780 -520
campo Jardines del Paneo | 240 150 | 700 -460
campo Palmerales del Mareotis | 500 180 | -700 -1150
lago Lago Mareotis | 900 170 | 300 -1250
hito 0 | 60 100
hito 1 | 330 220
hito 2 | -260 400
hito 3 | -520 -420
hito 4 | 200 60
hito 5 | -800 -520
hito 6 | -460 330
hito 7 | 720 280
Plaza | 240 -170
Pileta | 330 -120
Mercado | -220 300
Parque | 720 -460
Estudio | 120 -420
Entrenamiento | 180 -180
Baile | 480 80
Piscina | -120 -340
Museo | -880 -300
Investigacion | 500 300
Sanacion | -360 -180
Hotel | -300 180
Taller | -620 -220
Carpinteria | -860 200
Tecnologia | 560 -80
Mecanica | -500 -700
Estacion | 300 -1020
Guarderia | 620 -220
Edificio | -60 -150
Obra | 1000 -300
Policia | 40 -560
relleno | 60 | 300 -300";

    static int AlejandriaTown(Transform t)
    {
        var s = Town(t, "Alejandría de Cleopatra", new[]
        {
            new Lot("Palacio de Lochias", "Taller de papeles", "Palacio de Lochias (la cancillería de la reina)", 18f, 12f, PtolemaicStone,
                S("mesa", -3f, 2f, 2.4f, 1f, "Mesa de la reina (mapas de los nomos)"), S("estante", 4f, 2f, 1.2f, 2f, "Rollos de cuentas"), S("mesa", 0f, -4f, 2.4f, 1f, "Mesa de los escribas")),
            new Lot("Mouseion", "Escuela", "El Mouseion y la Biblioteca", 20f, 14f, PtolemaicStone,
                S("estante", -6f, 4f, 2f, 0.6f, "Rollos de la Biblioteca"), S("estante", 0f, 4f, 2f, 0.6f, "Rollos de la Biblioteca"), S("mesa", -3f, -3f, 2.4f, 1f, "Mesa de estudio"), S("pizarra", 5f, -3f, 2f, 0.4f, "Tablilla de cálculo")),
            new Lot("Graneros reales", "Granero", "Graneros reales del Puerto", 18f, 12f, PtolemaicStone,
                S("granero", -4f, 2f, 2f, 2f, "Silo de trigo"), S("granero", 4f, 2f, 2f, 2f, "Silo de cebada"), S("mesa", 0f, -4f, 2f, 1f, "Mesa del recuento")),
            new Lot("Panadería de Rhakotis", "Cocina", "Panadería de Rhakotis (pan de la reina)", 14f, 10f, AlexandrianHouse,
                S("horno", -3f, 2f, 1.6f, 1.6f, "Horno de pan"), S("mesa", 2f, -1f, 2f, 1f, "Mesa de amasar"), S("estante", 4f, 3f, 0.8f, 1.6f, "Sacos de harina")),
            new Lot("Casa de la reina", "Casa", "Aposentos de Cleopatra y Cesarión", 16f, 12f, PtolemaicStone,
                S("cama", -4f, 2f, 1.6f, 2f, "Kliné de Cleopatra"), S("cama", 0f, 2f, 1.2f, 2f, "Kliné de Cesarión"), S("estante", 4f, 2f, 0.8f, 1.6f, "Rollos de lectura")),
            new Lot("Serapeo", "Templo", "Serapeo (el templo de Serapis)", 16f, 12f, PtolemaicStone,
                S("altar", 0f, 3f, 2.4f, 1f, "Altar de Serapis"), S("banco", -3f, -2f, 2f, 0.6f, "Banco"), S("banco", 3f, -2f, 2f, 0.6f, "Banco")),
            new Lot("Mercado del Gran Puerto", "Mercado", "Emporion del Gran Puerto", 18f, 12f, AlexandrianHouse,
                S("puesto", -5f, 2f, 2f, 1f, "Puesto de pescado salado"), S("puesto", 0f, 2f, 2f, 1f, "Puesto de papiro y vidrio"), S("puesto", 5f, 2f, 2f, 1f, "Puesto de especias")),
            new Lot("Taberna del barrio Delta", "Restaurante", "Comedor del barrio Delta", 14f, 10f, AlexandrianHouse,
                S("fogon", -3f, 2f, 1.6f, 0.8f, "Olla de lentejas"), S("mesa", 2f, -1f, 2f, 1f, "Mesa con bancos"), S("barra", 3f, 3f, 2f, 0.8f, "Ánforas de vino de Mareotis")),
        }, new[]
        {
            new Lot("Casa de Alejandría", "Casa", "Casa de patio de Alejandría", 11f, 9f, AlexandrianHouse),
            new Lot("Taller de vidrio", "Taller", "Taller de vidrio moldeado", 10f, 8f, AlexandrianHouse, S("horno", 0f, 0f, 1.4f, 1.4f, "Horno de vidrio"), S("estante", 0f, 0f, 0.8f, 1.6f, "Cuencos de vidrio")),
            new Lot("Taller de papiro", "Taller", "Taller de papiro", 10f, 8f, AlexandrianHouse, S("banco_taller", 0f, 0f, 2f, 0.9f, "Prensa de hojas de papiro"), S("estante", 0f, 0f, 0.8f, 1.6f, "Rollos en blanco")),
            new Lot("Huerto de palmeras", "Huerto", "Huerto de palmeras y hortalizas", 12f, 10f, AlexandrianHouse, S("bancal", 0f, 0f, 4f, 6f, "Cebollas y lentejas"), S("pozo", 0f, 0f, 1f, 1f, "Saqiya (noria)")),
        }, C(0.6f, 0.55f, 0.46f));
        int n = 0;
        n += W(t, "Secretaria de la reina", s[0], "sabio", s[7], null, s[4]);
        n += W(t, "Bibliotecario del Mouseion", s[1], "sabio", s[7], null, null);
        n += W(t, "Alumna del Mouseion", s[1], "alumno", s[7], s[1], null);
        n += W(t, "Guardián de los graneros", s[2], "", s[7], null, null);
        n += W(t, "Panadero de Rhakotis", s[3], "cocinero", s[3], null, null);
        n += W(t, "Sacerdote de Serapis", s[5], "", s[7], null, null);
        n += W(t, "Vendedora del puerto", s[6], "comerciante", s[7], null, null);
        n += W(t, "Cocinero del barrio Delta", s[7], "cocinero", s[7], null, null);
        n += Townsfolk(t, s, 8, "Vecina de Alejandría", "Vecino de Alejandría", s[7], s[5]);
        return n;
    }
}

public static partial class MicroBiome
{
    // Alejandría, verano seco del 42 a. C.: costa mediterránea de Egipto. Palmeras, sicómoros, olivos y vides del Mareotis, tamariscos y
    // salicornia en las orillas, papiro y juncos en el lago. Escarabajo pelotero, mantis, cigarra, abeja; gatos y asnos de la ciudad.
    // (Täckholm y Drar 1941–1969, Flora of Egypt; Fraser 1972.)
    static Biome AlejandriaBiome() => new Biome { title = "Alejandría, costa de Egipto (verano del 42 a. C.)", ground = C(0.76f, 0.68f, 0.52f), plants = new[] {
            new Plant(Kind.Shrub, 6, "Tamarisco (Tamarix nilotica)", 400, C(0.45f, 0.55f, 0.42f), C(0.85f, 0.6f, 0.7f)),
            new Plant(Kind.Grass, 10, "Salicornia (Salicornia europaea)", 30, C(0.5f, 0.6f, 0.35f), C(0.7f, 0.4f, 0.3f)),
            new Plant(Kind.Grass, 12, "Papiro (Cyperus papyrus), del Mareotis", 300, C(0.4f, 0.6f, 0.3f), C(0.7f, 0.65f, 0.35f)),
            new Plant(Kind.Shrub, 5, "Vid de Mareotis (Vitis vinifera)", 180, C(0.35f, 0.5f, 0.28f), C(0.35f, 0.15f, 0.4f)),
            new Plant(Kind.Sand, 8, "Arena de la costa", 0, C(0.85f, 0.78f, 0.6f), C(0.8f, 0.72f, 0.55f)),
            new Plant(Kind.Stone, 6, "Caliza oolítica", 0, C(0.82f, 0.76f, 0.62f), Stone) },
          town = new[] {
            new Plant(Kind.Shrub, 7, "Palmera datilera (Phoenix dactylifera), plantada", 1500, C(0.45f, 0.52f, 0.4f), C(0.6f, 0.35f, 0.15f)),
            new Plant(Kind.Shrub, 4, "Sicómoro (Ficus sycomorus), de patio", 900, C(0.25f, 0.45f, 0.22f), C(0.8f, 0.55f, 0.35f)),
            new Plant(Kind.Shrub, 4, "Olivo (Olea europaea), de los jardines", 500, C(0.45f, 0.52f, 0.4f), C(0.25f, 0.3f, 0.15f)),
            new Plant(Kind.Flower, 4, "Loto azul (Nymphaea caerulea), en las pilas", 30, C(0.3f, 0.5f, 0.3f), C(0.4f, 0.5f, 0.9f)) },
          bugs = new[] {
            new Bug("DungBeetle", 3, 25, C(0.15f, 0.14f, 0.12f), "Escarabajo sagrado (Scarabaeus sacer)"),
            new Bug("Mantis", 2, 60, C(0.6f, 0.6f, 0.4f), "Mantis (Sphodromantis viridis)"),
            new Bug("Cicada", 3, 25, C(0.4f, 0.38f, 0.28f), "Cigarra de los olivos"),
            new Bug("SnowBee", 3, 14, C(0.8f, 0.6f, 0.2f), "Abeja egipcia (Apis mellifera lamarckii)") },
          townFauna = new[] {
            new Pet("Woodlouse", 3, 0.5f, C(0.75f, 0.65f, 0.45f), "Gato del barrio (en el microcosmos, una cochinilla hormiguera)"),
            new Pet("Aphid", 3, 1.6f, C(0.55f, 0.5f, 0.45f), "Asno de carga (en el microcosmos, un pulgón de carga)") } };
}
