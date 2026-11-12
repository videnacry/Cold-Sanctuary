using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL CATÁLOGO DE ESTILOS DE ÉPOCA Y LUGAR de los muebles de los microcosmos (docs/microcosmos-reglas-edificacion.md §Catálogo;
/// Berón 2026-11-02: «asegúrate de estar usando camas y demás servicios con el estilo de época/lugar, deberíamos tener una lista
/// así podemos reutilizar para épocas/lugares similares y el template de microcosmos podría escoger automáticamente»).
/// Cada <see cref="Entry"/> es un intervalo de años y una o varias <see cref="Region"/> (región o cultura) y dice, para cada
/// mueble de la casa, QUÉ VERSIÓN se construye: cama, asiento, mesa, hogar, retrete, lavamanos, almacén y luz, con los colores
/// de sus materiales. <c>MicroFurniture.Build</c> la pide con <see cref="For"/> (el año del pueblo + su región); la región de
/// cada microcosmos sale de <see cref="RegionOf"/> (su nombre) y va en su raíz (<see cref="MicroEra.region"/>) y en cada
/// edificio (<see cref="AreaFunctionSite.region"/>). Épocas y lugares parecidos REUTILIZAN la misma entrada: un pueblo nuevo
/// solo necesita su región (o una nueva entrada si su cultura no está).
/// </summary>
public static partial class MicroStyles
{
    /// <summary>Regiones o culturas materiales (lo que decide cómo se duerme, se come, se cocina y se hace de vientre).</summary>
    public enum Region
    {
        Prehistoria, Mesopotamia, Egipto, Egeo, Roma, SurAsia, China, HongKong, Japon, Islam, Otomano,
        EuropaMedieval, EuropaNorte, Alpes, EuropaUrbana, FranciaRural, Norteamerica, Hispanoamerica, Sovietico, Frente,
        Maya,   // (2026-11-03) Mesoamérica maya clásica (Palenque)
        // (2026-11-05) Las historias de la tanda 3 (docs/microcosmos-historias-3.md; nombres y frutales en MicroStyles.Historias.cs).
        Andes, Mapuche, Inca, Haudenosaunee, EgiptoPtolemaico, VirginiaTidewater, Viena, ProvinciasUnidas, Amazonia, EspanaActual, Exposicion,
        // (2026-11-04) Los famosos (docs/microcosmos-famosos.md): el Sahel (Uagadugú) y el interior de Norteamérica (Albuquerque,
        // Omaha, Prairie View: frutales de clima continental, no los naranjos de California).
        Sahel, NorteamericaInterior,
        // (2026-11-05) La música (docs/microcosmos-musica.md): el Caribe hispano (Barranquilla, Vega Baja), Jamaica, Corea, las islas
        // Británicas (Belfast, Ascot), el noreste de Norteamérica y los Grandes Lagos (Montreal, Gary, Long Island) y Sudáfrica.
        CaribeHispano, Jamaica, Corea, IslasBritanicas, NorteamericaNorte, Sudafrica,
    }

    public enum Bed { Estera, LechoJuncos, Reposacabezas, Kline, Lectus, Charpai, Kang, Futon, Minder, Jergon, Dosel, Madera, Hierro, Litera, Hamaca, Moderna, SofaCama }
    public enum Seat { Suelo, Taburete, Diphros, Klismos, Banco, Cojines, Windsor, Thonet, Butaca, Banqueta, Caja, Cromado, Sovietica }
    public enum Table { EsteraDeComer, Baja, Caballete, Trapeza, Refectorio, Redonda, Formica }
    public enum Hearth { Suelo, Tannur, Chulha, Brasero, Caldero, Ocak, KangWok, Irori, Kachelofen, Topias, Hierro, GasWok, Gas, Electrica }
    public enum Toilet { Pozo, Vasija, Comunal, Bacinilla, Turca, Caseta, CisternaAlta, CisternaBaja, Cubo }
    public enum Basin { Pila, Cantaro, Louterion, Aguamanil, Ibrik, Soporte, Tarai, Tinajero, Pedestal }
    public enum Storage { Cesto, Tinaja, Arcon, Nicho, Rollos, Alacena, Comoda, Estanteria, Stenka }
    public enum Light { Fuego, Concha, Lucerna, Diya, Vela, Candil, Andon, Quinque, Gas, Bombilla }

    /// <summary>Una entrada del catálogo: un estilo de época y lugar para cada mueble.</summary>
    public sealed class Entry
    {
        public string id, name;
        public int from, to;
        public Region[] regions;
        public Bed bed; public Seat seat; public Table table; public Hearth hearth; public Toilet toilet; public Basin basin; public Storage storage; public Light light;
        public Color wood, textile, metal;
        public string sources;
        public bool FloorSeating => seat == Seat.Suelo || seat == Seat.Cojines;
        public bool Covers(int year) => year >= from && year <= to;
        public bool Has(Region r) { foreach (Region x in regions) if (x == r) return true; return false; }
        public override string ToString() => $"{name} ({id})";
    }

    static Color H(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out Color c); return c; }

    static Entry E(string id, string name, int from, int to, Region[] regions, Bed bed, Seat seat, Table table, Hearth hearth, Toilet toilet, Basin basin, Storage storage, Light light,
                   string wood, string textile, string metal, string sources)
        => new Entry { id = id, name = name, from = from, to = to, regions = regions, bed = bed, seat = seat, table = table, hearth = hearth, toilet = toilet, basin = basin,
                       storage = storage, light = light, wood = H(wood), textile = H(textile), metal = H(metal), sources = sources };

    static Region[] R(params Region[] r) => r;

    /// <summary>EL CATÁLOGO. Orden: de lo más específico a lo más general dentro de cada región (gana la primera que cubre el año).</summary>
    public static readonly Entry[] All =
    {
        E("prehistoria", "Prehistoria (cazadores y primeras aldeas)", -3000000, -4001, R(Region.Prehistoria),
          Bed.Estera, Seat.Suelo, Table.EsteraDeComer, Hearth.Suelo, Toilet.Pozo, Basin.Pila, Storage.Cesto, Light.Fuego,
          "6B4F35", "8A5A44", "8C857A", "Esteras y pieles junto al hogar (Ohalo II, ~21 000 a. C.: camas de hierba; Natufiense)"),
        E("mesopotamia", "Mesopotamia (Uruk, Ur)", -5000, -500, R(Region.Mesopotamia),
          Bed.LechoJuncos, Seat.Taburete, Table.Baja, Hearth.Tannur, Toilet.Vasija, Basin.Cantaro, Storage.Tinaja, Light.Concha,
          "8A6A44", "C9B07A", "B0714A", "Esteras y armazones de cama de madera con cuerdas de juncos (tumbas de Ur); tannur de pan; letrinas sobre tubos de cerámica (Tell Asmar, ~2200 a. C.)"),
        E("egipto", "Egipto faraónico", -3100, -30, R(Region.Egipto),
          Bed.Reposacabezas, Seat.Diphros, Table.Baja, Hearth.Tannur, Toilet.Vasija, Basin.Aguamanil, Storage.Arcon, Light.Lucerna,
          "A0703A", "EDE6D0", "C9A24A", "Cama con patas de león, inclinada, con reposacabezas (Tutankamón, Museo Egipcio); asiento de caliza sobre vasija de arena (Amarna); jarra y jofaina"),
        E("egeo", "Grecia (micénica, arcaica y helenística)", -1600, -30, R(Region.Egeo),
          Bed.Kline, Seat.Klismos, Table.Trapeza, Hearth.Brasero, Toilet.Bacinilla, Basin.Louterion, Storage.Arcon, Light.Lucerna,
          "7A5232", "B5462F", "B08D57", "Kliné con correas y almohadón, klismós y trapeza de tres patas (Richter, The Furniture of the Greeks); orinal (amis); louterion; bañera de barro de Pilos"),
        E("roma", "Roma", -500, 476, R(Region.Roma),
          Bed.Lectus, Seat.Taburete, Table.Redonda, Hearth.Brasero, Toilet.Comunal, Basin.Louterion, Storage.Arcon, Light.Lucerna,
          "6E4B2E", "8E2A26", "B08D57", "Lectus, sella y mensa redonda; culina con trébede; letrinas comunales con xylospongium (Ostia); labrum; lucerna"),
        E("sur_asia", "Sur de Asia antigua (Kapilavastu, el Ganges)", -2600, 1200, R(Region.SurAsia),
          Bed.Charpai, Seat.Taburete, Table.Baja, Hearth.Chulha, Toilet.Vasija, Basin.Cantaro, Storage.Tinaja, Light.Diya,
          "7A5A3A", "D9C9A0", "B0714A", "Cama de cuerdas (khaṭvā/mañca de los Vinaya) y taburete bajo (pīṭha); chulha de barro; pozos-letrina de anillos de terracota; lota de agua; diya"),
        E("islam_abasi", "Islam abasí (Bagdad)", 600, 1499, R(Region.Islam),
          Bed.Minder, Seat.Cojines, Table.Baja, Hearth.Tannur, Toilet.Turca, Basin.Ibrik, Storage.Nicho, Light.Candil,
          "7A5232", "2F5E7A", "C9A24A", "Majlis de colchones y cojines sobre un estrado; sufra y bandeja; tannur; letrina a la turca con ibrīq y ṭast; candil de bronce"),
        E("otomano", "Otomano (Üsküdar, 1500–1923)", 1500, 1923, R(Region.Otomano),
          Bed.Minder, Seat.Cojines, Table.Baja, Hearth.Ocak, Toilet.Turca, Basin.Ibrik, Storage.Nicho, Light.Candil,
          "6E4B2E", "8E2A26", "B87333", "Sedir y minder, sini (bandeja redonda baja) sobre su pie, ocak y mangal; hela a la turca con ibrik; leğen-ibrik de cobre"),
        E("china_tradicional", "China tradicional", -200, 1944, R(Region.China, Region.HongKong),
          Bed.Kang, Seat.Banqueta, Table.Baja, Hearth.KangWok, Toilet.Bacinilla, Basin.Soporte, Storage.Arcon, Light.Andon,
          "6B2E22", "3A5A8A", "8C857A", "Kang de ladrillo calentado por la cocina, mesa de kang, taburetes; matong (bacín) y palangana sobre su soporte de madera; farol de papel"),
        E("hongkong_tonglau", "Hong Kong de los tong lau (1945–1979)", 1945, 1979, R(Region.HongKong),
          Bed.Litera, Seat.Banqueta, Table.Redonda, Hearth.GasWok, Toilet.Turca, Basin.Soporte, Storage.Estanteria, Light.Bombilla,
          "7A4A2A", "C8D0C8", "9AA0A6", "Literas de madera de los pisos subdivididos, taburetes y mesa redonda plegable; wok en fogón de queroseno o gas; retrete a la turca compartido; palangana esmaltada en su soporte"),
        E("japon_edo_meiji", "Japón Edo y Meiji", 1603, 1925, R(Region.Japon),
          Bed.Futon, Seat.Suelo, Table.Baja, Hearth.Irori, Toilet.Turca, Basin.Tarai, Storage.Arcon, Light.Andon,
          "8A6A44", "3A4A6A", "4A4440", "Futón sobre el tatami, zabutón, chabudai u ozen; irori con su gancho; kawaya a la turca; tarai; tansu; andon"),
        E("europa_medieval", "Europa medieval", 476, 1499, R(Region.EuropaMedieval, Region.EuropaNorte, Region.EuropaUrbana, Region.FranciaRural, Region.Alpes),
          Bed.Jergon, Seat.Banco, Table.Caballete, Hearth.Caldero, Toilet.Bacinilla, Basin.Aguamanil, Storage.Arcon, Light.Vela,
          "5A4030", "B5A27A", "3B3B3E", "Jergón de paja, bancos y mesa de caballetes; hogar abierto con caldero; orinal y garderobe; aguamanil; arcón"),
        E("alpino", "Alpes (casa engadinesa, 1600–1918)", 1500, 1918, R(Region.Alpes),
          Bed.Madera, Seat.Banco, Table.Refectorio, Hearth.Kachelofen, Toilet.Bacinilla, Basin.Aguamanil, Storage.Arcon, Light.Quinque,
          "B08850", "9A3030", "3B3B3E", "Stube de pino: banco corrido, mesa de pino y cama de madera tallada; estufa de azulejos (Kachelofen); orinal y aguamanil; quinqué de petróleo"),
        E("estuardo", "Inglaterra y Europa del norte (1500–1719)", 1500, 1719, R(Region.EuropaNorte, Region.EuropaUrbana, Region.FranciaRural),
          Bed.Dosel, Seat.Taburete, Table.Refectorio, Hearth.Caldero, Toilet.Bacinilla, Basin.Aguamanil, Storage.Arcon, Light.Vela,
          "5A3A22", "6B2E2E", "3B3B3E", "Cama de dosel de roble, taburetes ensamblados y mesa larga; hogar con espetón y caldero; silla-retrete (close stool) y orinal; aguamanil; vela de junco (Woolsthorpe Manor, National Trust)"),
        E("georgiano", "Georgiano (1720–1836)", 1720, 1836, R(Region.EuropaNorte, Region.EuropaUrbana, Region.FranciaRural),
          Bed.Dosel, Seat.Windsor, Table.Redonda, Hearth.Caldero, Toilet.Bacinilla, Basin.Aguamanil, Storage.Comoda, Light.Vela,
          "6E3B22", "4A6A8A", "C9A24A", "Cama de cuatro postes, sillas Windsor, mesa redonda; cocina abierta con su espetón; cómoda-orinal (night commode) y lavamanos de trípode; candelero (Jenner's House, Berkeley)"),
        E("europa_romantica", "Europa urbana (1837–1879)", 1837, 1879, R(Region.EuropaUrbana, Region.EuropaNorte, Region.FranciaRural),
          Bed.Hierro, Seat.Thonet, Table.Refectorio, Hearth.Hierro, Toilet.Bacinilla, Basin.Aguamanil, Storage.Comoda, Light.Quinque,
          "6E4B2E", "EDE6D0", "3B3B3E", "Cama de hierro, sillas de Thonet (n.º 14, 1859), cocina económica de hierro; orinal y lavamanos de jarra y palangana; quinqué"),
        E("belle_epoque", "Europa urbana (1880–1918)", 1880, 1918, R(Region.EuropaUrbana, Region.EuropaNorte),
          Bed.Hierro, Seat.Thonet, Table.Redonda, Hearth.Hierro, Toilet.CisternaAlta, Basin.Aguamanil, Storage.Comoda, Light.Gas,
          "6E4B2E", "EDE6D0", "2F2F33", "Cama de hierro con colcha, Thonet, cocina de carbón; inodoro con cisterna alta (París: tout-à-l'égout, 1894); lavamanos de mármol con jarra; luz de gas"),
        E("entreguerras", "Europa urbana (1919–1959)", 1919, 1959, R(Region.EuropaUrbana, Region.EuropaNorte),
          Bed.Hierro, Seat.Thonet, Table.Redonda, Hearth.Hierro, Toilet.CisternaAlta, Basin.Aguamanil, Storage.Alacena, Light.Quinque,
          "6E4B2E", "D9D2C2", "3B3B3E", "Cama de hierro con edredón (pierzyna), cocina de carbón y estufa de azulejos, retrete compartido de cisterna alta; palangana; lámpara de petróleo por los cortes de luz (gueto de Varsovia, 1942)"),
        E("francia_rural", "Francia rural (1850–1960)", 1850, 1960, R(Region.FranciaRural),
          Bed.Madera, Seat.Banco, Table.Refectorio, Hearth.Hierro, Toilet.Caseta, Basin.Aguamanil, Storage.Alacena, Light.Quinque,
          "6E4B2E", "B5462F", "3B3B3E", "Cama de madera alta con edredón, mesa larga con bancos, cuisinière de leña; la caseta del fondo del huerto; broc et cuvette; lampe à pétrole (granjas del plateau Vivarais-Lignon)"),
        E("frente_1914", "El frente (1914–1918)", 1914, 1918, R(Region.Frente),
          Bed.Litera, Seat.Caja, Table.Caballete, Hearth.Brasero, Toilet.Cubo, Basin.Soporte, Storage.Estanteria, Light.Vela,
          "6B5A40", "6B6A4A", "4A4A4A", "Literas de tela metálica, cajas de munición como asientos, brasero de cubo agujereado, letrina de cubo; palangana de lata (Imperial War Museums)"),
        E("hispanoamerica", "Hispanoamérica colonial y republicana (1500–1899)", 1500, 1899, R(Region.Hispanoamerica),
          Bed.Hamaca, Seat.Butaca, Table.Refectorio, Hearth.Topias, Toilet.Caseta, Basin.Tinajero, Storage.Arcon, Light.Vela,
          "6E4B2E", "D9A441", "8C857A", "Chinchorro (hamaca) del Orinoco, butaca de cuero, fogón de topias (tres piedras), tinajero de piedra para filtrar el agua, letrina del patio"),
        E("maya_clasico", "Maya clásico (Palenque, 250–900)", 250, 900, R(Region.Maya),
          Bed.Estera, Seat.Banco, Table.Baja, Hearth.Topias, Toilet.Pozo, Basin.Cantaro, Storage.Tinaja, Light.Fuego,
          "8A5A3A", "C8B48A", "6E8A7A", "Banca de mampostería enlucida con petate (estera de palma) para dormir y sentarse; el k'oben (fogón de tres piedras) con su comal; ollas y cántaros de barro; tinajas; tea de ocote. Letrinas y baños de vapor en el Palacio de Palenque (Barnhart 2001; Stuart y Stuart 2008)"),
        // (2026-11-05) LAS HISTORIAS DE LA TANDA 3 (docs/microcosmos-historias-3.md; sus nombres de edificios y frutales, en MicroStyles.Historias.cs).
        E("andes_republicano", "Andes republicanos (Quito, 1820–1960)", 1820, 1960, R(Region.Andes),
          Bed.Hierro, Seat.Butaca, Table.Refectorio, Hearth.Topias, Toilet.Caseta, Basin.Aguamanil, Storage.Arcon, Light.Quinque,
          "6E4B2E", "B5462F", "8C857A", "Casa quiteña de patio: cama de hierro, sillas de cuero, mesa larga; tulpa (fogón de tres piedras) en la cocina del fondo; letrina del patio, palangana y jarra; lámpara de queroseno"),
        E("mapuche", "Mapuche (ruka, 1000–1900)", 1000, 1900, R(Region.Mapuche),
          Bed.Estera, Seat.Taburete, Table.EsteraDeComer, Hearth.Suelo, Toilet.Pozo, Basin.Cantaro, Storage.Cesto, Light.Fuego,
          "6B4F35", "7A2E2A", "B8BDC2", "La ruka de paja y coligüe con el kütral (fuego) en el centro; esteras y mantas de lana (makuñ); wanco (taburete de una pieza de madera); metawe (cántaro de barro); chaiwe y llepü (cestos) (Museo Chileno de Arte Precolombino; Bengoa 2000)"),
        E("inca", "Inca (Tawantinsuyu, 1400–1572)", 1400, 1572, R(Region.Inca),
          Bed.Estera, Seat.Taburete, Table.EsteraDeComer, Hearth.Chulha, Toilet.Pozo, Basin.Cantaro, Storage.Tinaja, Light.Fuego,
          "7A5232", "C23B22", "C9A24A", "Mantas de qompi y awasqa sobre esteras; tiana (asiento bajo de la nobleza); q'oncha (fogón de barro); urpu o aríbalo y qero; colcas y tinajas (D'Altroy 2014, The Incas; Morris y Thompson 1985)"),
        E("haudenosaunee", "Haudenosaunee (casa larga, 1000–1700)", 1000, 1700, R(Region.Haudenosaunee),
          Bed.Estera, Seat.Banco, Table.EsteraDeComer, Hearth.Suelo, Toilet.Pozo, Basin.Cantaro, Storage.Cesto, Light.Fuego,
          "6E5238", "8A6A44", "8C857A", "Casa larga de corteza de olmo: plataformas corridas con esteras y pieles, fuegos en fila por el pasillo central; ollas de barro; cestos y trenzas de maíz colgadas (Snow 1994, The Iroquois; Ganondagan State Historic Site)"),
        E("egipto_ptolemaico", "Egipto ptolemaico (Alejandría, −332 a −30)", -332, -30, R(Region.EgiptoPtolemaico),
          Bed.Kline, Seat.Klismos, Table.Trapeza, Hearth.Brasero, Toilet.Vasija, Basin.Louterion, Storage.Arcon, Light.Lucerna,
          "7A5232", "2F5E7A", "C9A24A", "Casa griega en tierra egipcia: kliné y klismós, trapeza, brasero y fogón portátil, jarra y louterion, lucerna de barro de Alejandría (Fraser 1972, Ptolemaic Alexandria)"),
        E("virginia_1960", "Virginia (costa, 1945–1979)", 1945, 1979, R(Region.VirginiaTidewater),
          Bed.Moderna, Seat.Cromado, Table.Formica, Hearth.Electrica, Toilet.CisternaBaja, Basin.Pedestal, Storage.Alacena, Light.Bombilla,
          "8A6A44", "6E9CC0", "C0C4C8", "La casa de clase media de Hampton y Newport News: dinette de fórmica y cromo, cocina eléctrica, inodoro de tanque bajo (Shetterly 2016, Hidden Figures)"),
        E("viena_biedermeier", "Viena (1780–1848)", 1780, 1848, R(Region.Viena),
          Bed.Madera, Seat.Butaca, Table.Redonda, Hearth.Kachelofen, Toilet.Bacinilla, Basin.Aguamanil, Storage.Comoda, Light.Vela,
          "8A5A32", "B5A27A", "C9A24A", "Biedermeier: cama de cerezo, sillas tapizadas, mesa redonda, estufa de azulejos (Kachelofen); orinal y aguamanil; velas de sebo y de cera"),
        E("provincias_1600", "Provincias Unidas (1580–1700)", 1580, 1700, R(Region.ProvinciasUnidas),
          Bed.Madera, Seat.Banco, Table.Refectorio, Hearth.Caldero, Toilet.Bacinilla, Basin.Aguamanil, Storage.Alacena, Light.Vela,
          "5A3A22", "3A4A6A", "B8A070", "Casa holandesa de ladrillo: cama-armario (bedstede), banco y mesa de roble, hogar con caldero y azulejos, kast (armario grande), orinal y jarra (Rijksmuseum)"),
        E("amazonia_1950", "Amazonía peruana (1900–1980)", 1900, 1980, R(Region.Amazonia),
          Bed.Hamaca, Seat.Banco, Table.Caballete, Hearth.Topias, Toilet.Caseta, Basin.Tinajero, Storage.Estanteria, Light.Quinque,
          "6E4B2E", "D9C9A0", "8C857A", "Casas de madera sobre pilotes con techo de hojas de palma: hamaca y mosquitero, banco de tablas, fogón de tres piedras, letrina aparte, tinaja de agua y lámpara de queroseno (colonia de San Pablo, Loreto)"),
        E("espana_actual", "España actual (2000–)", 2000, 2100, R(Region.EspanaActual),
          Bed.Moderna, Seat.Cromado, Table.Formica, Hearth.Electrica, Toilet.CisternaBaja, Basin.Pedestal, Storage.Alacena, Light.Bombilla,
          "A08058", "6E8EA0", "C0C4C8", "Piso de Madrid: mobiliario de serie, vitrocerámica, baño completo"),
        E("exposicion_mundial", "Feria internacional (2000–)", 2000, 2100, R(Region.Exposicion),
          Bed.Moderna, Seat.Cromado, Table.Formica, Hearth.Electrica, Toilet.CisternaBaja, Basin.Pedestal, Storage.Estanteria, Light.Bombilla,
          "A08058", "D9A441", "C0C4C8", "Pabellones de una feria inventada: cada uno lleva el aire de su país; el mobiliario de servicio es el de cualquier feria"),
        // (2026-11-04) LOS FAMOSOS (docs/microcosmos-famosos.md).
        E("india_moderna", "India moderna (Tamil Nadu, 1900–)", 1900, 2100, R(Region.SurAsia),
          Bed.Charpai, Seat.Suelo, Table.Baja, Hearth.Gas, Toilet.Turca, Basin.Cantaro, Storage.Estanteria, Light.Bombilla,
          "7A5A3A", "F2EEE2", "B0714A", "Ashram: esterilla y cojín para sentarse en el suelo, catre de cuerdas (kattil) o colchón fino; cocina de gas con ollas de acero y de barro; letrina a la turca con su jarra; vasija de agua de barro; estante de acero (Isha Yoga Center, Velliangiri)"),
        E("sahel_moderno", "Sahel (Burkina Faso, 1960–)", 1960, 2100, R(Region.Sahel),
          Bed.Moderna, Seat.Taburete, Table.Baja, Hearth.Brasero, Toilet.Turca, Basin.Cantaro, Storage.Tinaja, Light.Bombilla,
          "7A5232", "D9A441", "8C857A", "La cour familiale: cama de hierro con colchón de espuma, taburetes y mesa baja; foyer amélioré (hornillo de barro) y brasero de carbón con su olla de aluminio; letrina a la turca; el canari (tinaja de barro que enfría el agua); estera de paja (Ouagadougou)"),
        E("norteamerica_interior", "Norteamérica interior (1920–)", 1920, 2100, R(Region.NorteamericaInterior),
          Bed.Moderna, Seat.Cromado, Table.Formica, Hearth.Gas, Toilet.CisternaBaja, Basin.Pedestal, Storage.Alacena, Light.Bombilla,
          "8A6A44", "6E9CC0", "C0C4C8", "Cama con colcha, mesa de cocina de fórmica y sillas de cromo, cocina de gas o eléctrica, inodoro de tanque bajo y lavabo de pie (las casas de posguerra de Albuquerque, Omaha y el este de Texas)"),
        // (2026-11-05) LA MÚSICA (docs/microcosmos-musica.md).
        E("caribe_hispano", "Caribe hispano (Barranquilla, Vega Baja, 1950–)", 1950, 2100, R(Region.CaribeHispano),
          Bed.Moderna, Seat.Butaca, Table.Formica, Hearth.Gas, Toilet.CisternaBaja, Basin.Pedestal, Storage.Alacena, Light.Bombilla,
          "8A6A44", "E8B04A", "C0C4C8", "La casa de bloque del Caribe: cama con ventilador, mecedora o butaca en la terraza, mesa de fórmica, cocina de gas de bombona, inodoro y lavadero en el patio (Barranquilla, Vega Baja)"),
        E("jamaica_moderna", "Jamaica (Kingston, 1950–)", 1950, 2100, R(Region.Jamaica),
          Bed.Hierro, Seat.Banco, Table.Caballete, Hearth.Brasero, Toilet.Comunal, Basin.Cantaro, Storage.Estanteria, Light.Bombilla,
          "7A5232", "3E7A4E", "8C857A", "Los Government Yards de Trench Town: cama de hierro, bancos y mesa de tablas, el coal pot (brasero de carbón) en la cocina común, baños comunes en el patio y agua del grifo del patio"),
        E("corea_moderna", "Corea del Sur (Seúl, 1980–)", 1980, 2100, R(Region.Corea),
          Bed.Moderna, Seat.Banqueta, Table.Baja, Hearth.Gas, Toilet.CisternaBaja, Basin.Pedestal, Storage.Estanteria, Light.Bombilla,
          "8A6A44", "E4D8C8", "C0C4C8", "El piso de Seúl: cama o colchón yo sobre el suelo radiante (ondol), mesa baja, cocina de gas y baño con ducha"),
        E("britanico_moderno", "Islas Británicas (1945–)", 1945, 2100, R(Region.IslasBritanicas),
          Bed.Moderna, Seat.Butaca, Table.Formica, Hearth.Electrica, Toilet.CisternaBaja, Basin.Pedestal, Storage.Comoda, Light.Bombilla,
          "6E4B2E", "8A5A44", "C0C4C8", "La casa adosada de ladrillo de Belfast y de Inglaterra: butaca junto a la chimenea, cocina eléctrica, cómoda y aseo con cisterna"),
        E("norteamerica_norte", "Norteamérica del noreste y Grandes Lagos (1920–)", 1920, 2100, R(Region.NorteamericaNorte),
          Bed.Moderna, Seat.Cromado, Table.Formica, Hearth.Gas, Toilet.CisternaBaja, Basin.Pedestal, Storage.Alacena, Light.Bombilla,
          "8A6A44", "6E9CC0", "C0C4C8", "Casa de madera o de ladrillo con sótano, dinette de cromo y fórmica, cocina de gas (Montreal, Gary, Long Island)"),
        E("sudafrica_moderna", "Sudáfrica (1950–)", 1950, 2100, R(Region.Sudafrica),
          Bed.Hierro, Seat.Banco, Table.Caballete, Hearth.Brasero, Toilet.Caseta, Basin.Cantaro, Storage.Alacena, Light.Vela,
          "7A5232", "C8A24A", "8C857A", "La casa de cuatro habitaciones de un township de los años del apartheid: cama de hierro, mesa de tablas, brasero de carbón, letrina en el patio y velas o parafina"),
        E("norteamerica_victoriana", "Norteamérica (1840–1919)", 1840, 1919, R(Region.Norteamerica),
          Bed.Hierro, Seat.Windsor, Table.Redonda, Hearth.Hierro, Toilet.Caseta, Basin.Aguamanil, Storage.Comoda, Light.Quinque,
          "6E4B2E", "D9D2C2", "2F2F33", "Cama de hierro o de latón, sillas de respaldo prensado, cocina de hierro de leña o carbón; la caseta (outhouse) y el lavamanos de jarra y palangana; lámpara de queroseno (casa de los Wright, Dayton)"),
        E("norteamerica_moderna", "Norteamérica (1920–1979)", 1920, 1979, R(Region.Norteamerica, Region.Hispanoamerica),
          Bed.Moderna, Seat.Cromado, Table.Formica, Hearth.Gas, Toilet.CisternaBaja, Basin.Pedestal, Storage.Alacena, Light.Bombilla,
          "8A6A44", "6E9CC0", "C0C4C8", "Dinette de cromo y fórmica, cocina de gas o eléctrica, inodoro de tanque bajo y lavabo de pie (Pasadena 1953, Seattle 1960)"),
        E("sovietico", "Soviético (1960–1991)", 1960, 1991, R(Region.Sovietico),
          Bed.SofaCama, Seat.Sovietica, Table.Formica, Hearth.Electrica, Toilet.CisternaBaja, Basin.Pedestal, Storage.Stenka, Light.Bombilla,
          "7A4A2A", "8A3A30", "C0C4C8", "Sofá-cama (divan-krovat), mueble de pared (stenka), taburetes de cocina, cocina eléctrica (los pisos de Prípiat), inodoro compacto"),
        E("occidental_moderna", "Occidental moderna (1960–)", 1960, 2100, R(Region.EuropaUrbana, Region.EuropaNorte, Region.FranciaRural, Region.Alpes, Region.Norteamerica, Region.Hispanoamerica, Region.Frente),
          Bed.Moderna, Seat.Cromado, Table.Formica, Hearth.Electrica, Toilet.CisternaBaja, Basin.Pedestal, Storage.Alacena, Light.Bombilla,
          "8A6A44", "6E9CC0", "C0C4C8", "Mobiliario de serie de la segunda mitad del siglo XX"),
    };

    /// <summary>Si la región no tiene entrada para ese año, sus vecinas culturales (en orden).</summary>
    static readonly Dictionary<Region, Region[]> Neighbours = new Dictionary<Region, Region[]>
    {
        [Region.HongKong] = new[] { Region.China },
        [Region.Alpes] = new[] { Region.EuropaNorte, Region.EuropaUrbana },
        [Region.FranciaRural] = new[] { Region.EuropaNorte, Region.EuropaUrbana },
        [Region.EuropaNorte] = new[] { Region.EuropaUrbana, Region.EuropaMedieval },
        [Region.EuropaUrbana] = new[] { Region.EuropaNorte, Region.EuropaMedieval },
        [Region.EuropaMedieval] = new[] { Region.EuropaNorte },
        [Region.Otomano] = new[] { Region.Islam },
        [Region.Islam] = new[] { Region.Otomano, Region.Mesopotamia },
        [Region.Roma] = new[] { Region.Egeo },
        [Region.Egeo] = new[] { Region.Roma, Region.Egipto },
        [Region.Egipto] = new[] { Region.Egeo, Region.Mesopotamia },
        [Region.Mesopotamia] = new[] { Region.Egipto, Region.Islam },
        [Region.Frente] = new[] { Region.EuropaUrbana },
        [Region.Sovietico] = new[] { Region.EuropaUrbana },
        [Region.Hispanoamerica] = new[] { Region.EuropaMedieval, Region.Norteamerica },
        [Region.Norteamerica] = new[] { Region.EuropaNorte },
        [Region.Japon] = new[] { Region.China },
        [Region.China] = new[] { Region.Japon },
        [Region.SurAsia] = new[] { Region.Islam },
        [Region.NorteamericaInterior] = new[] { Region.Norteamerica },
        [Region.Sahel] = new[] { Region.Islam },
        [Region.CaribeHispano] = new[] { Region.Hispanoamerica },
        [Region.Jamaica] = new[] { Region.CaribeHispano, Region.Hispanoamerica },
        [Region.Corea] = new[] { Region.Japon, Region.China },
        [Region.IslasBritanicas] = new[] { Region.EuropaNorte, Region.EuropaUrbana },
        [Region.NorteamericaNorte] = new[] { Region.NorteamericaInterior, Region.Norteamerica },
        [Region.Sudafrica] = new[] { Region.EuropaUrbana },
    };

    /// <summary>
    /// El estilo de un pueblo del año <paramref name="year"/> en la región <paramref name="region"/>: la primera entrada de esa
    /// región que cubre el año; si no hay, la de sus vecinas culturales; antes de las primeras ciudades (o sin nada), la
    /// prehistoria; y si no, la más cercana en el tiempo de su región.
    /// </summary>
    public static Entry For(int year, Region region)
    {
        if (year < -4000 && region != Region.Mesopotamia && region != Region.Egipto && region != Region.SurAsia) return All[0];
        foreach (Entry e in All) if (e.Has(region) && e.Covers(year)) return e;
        if (Neighbours.TryGetValue(region, out Region[] near))
            foreach (Region r in near) foreach (Entry e in All) if (e.Has(r) && e.Covers(year)) return e;
        Entry best = null; int bd = int.MaxValue;
        foreach (Entry e in All)
        {
            if (!e.Has(region)) continue;
            int d = year < e.from ? e.from - year : year - e.to;
            if (d < bd) { bd = d; best = e; }
        }
        return best ?? All[0];
    }

    /// <summary>Lo mismo con la región por su nombre (<see cref="MicroEra.region"/>, <see cref="AreaFunctionSite.region"/>); vacío o
    /// desconocido → la prehistoria antes de −4000 y, después, Europa del norte.</summary>
    public static Entry For(int year, string region) => For(year, Parse(region, year));

    public static Region Parse(string region, int year = 0)
    {
        if (!string.IsNullOrEmpty(region) && System.Enum.TryParse(region, out Region r)) return r;
        return year < -4000 ? Region.Prehistoria : Region.EuropaNorte;
    }

    /// <summary>La REGIÓN de cada microcosmos por su nombre (el de <c>Eras.OfMicrocosmos</c>): el lugar real de su historia.</summary>
    public static Region RegionOf(string display)
    {
        switch (display)
        {
            case "Mesopotamia": return Region.Mesopotamia;
            case "Kapilavastu": case "Patanjali": return Region.SurAsia;
            case "Egina": case "Crotona": case "Alejandría": return Region.Egeo;   // Alejandría: la ciudad griega de los Ptolomeos
            case "Menfis": return Region.Egipto;
            case "Bagdad": return Region.Islam;
            case "Escútari": return Region.Otomano;
            case "Woolsthorpe": case "Berkeley": return Region.EuropaNorte;
            case "Sils Maria": return Region.Alpes;
            case "París 1898": case "Berna": case "Planta Atómica": case "Irena Sendler": return Region.EuropaUrbana;
            case "Le Chambon-sur-Lignon": return Region.FranciaRural;
            case "Angostura": return Region.Hispanoamerica;
            case "Dayton": case "Colorado Springs": case "Pasadena": return Region.Norteamerica;
            case "Tregua 1914": return Region.Frente;
            case "Bruce Lee": return Region.HongKong;   // (Seattle, Oakland y Los Ángeles: CivicBuilder los pone en Norteamérica)
            case "Chernóbil": return Region.Sovietico;
            case "Palenque": return Region.Maya;
            // Los famosos (docs/microcosmos-famosos.md)
            case "Velliangiri": return Region.SurAsia;   // 1999: la entrada india_moderna
            case "Hawthorne": return Region.Norteamerica;
            case "Aldea de la montaña": return Region.China;
            case "Albuquerque": case "Omaha": case "Prairie View": return Region.NorteamericaInterior;
            case "Uagadugú": return Region.Sahel;
            // La música (docs/microcosmos-musica.md)
            case "Barranquilla": case "Vega Baja": return Region.CaribeHispano;
            case "Kingston": return Region.Jamaica;
            case "Seúl": return Region.Corea;
            case "Belfast": return Region.IslasBritanicas;
            case "Montreal": case "Gary": case "Long Island": return Region.NorteamericaNorte;
            case "Grandes aportes de las canciones": return Region.Hispanoamerica;   // el pueblo inventado; cada rincón lleva la suya (CivicBuilder.Musica)
            // Las historias de la tanda 3 (docs/microcosmos-historias-3.md)
            case "Chimbacalle": return Region.Andes;
            case "Lagunillas": return Region.Mapuche;
            case "Cajamarca": return Region.Inca;
            case "Onondaga": return Region.Haudenosaunee;
            case "Alejandría de Cleopatra": return Region.EgiptoPtolemaico;
            case "Langley": return Region.VirginiaTidewater;
            case "Viena 1824": return Region.Viena;
            case "Leiden": return Region.ProvinciasUnidas;
            case "San Pablo": return Region.Amazonia;
            case "Madrid 2018": return Region.EspanaActual;
            case "Gettysburg": return Region.Norteamerica;
            case "Feria de las Hazañas": return Region.Exposicion;
            default: return Region.Prehistoria;   // las áreas del santuario: Sakshi, Ambrosio, Cocina, Huerto, Yoga, Textil, Mecánica
        }
    }
}
