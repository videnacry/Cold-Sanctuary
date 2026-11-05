using System.Collections.Generic;

/// <summary>
/// HISTORIA NATURAL de las especies del santuario (docs/ciclo-de-vida.md; Berón 2026-11-01: «que realice todos sus
/// procesos naturales»): lo que NO estaba en ninguna tabla y faltaba para cerrar el ciclo de vida de verdad —
///   · de dónde sacan el AGUA (beben, comen nieve o la sacan de la comida: <see cref="Hydration"/>),
///   · cómo CRÍAN (lactancia, regurgitación, guardar el saco de huevos; quién: madre o los dos),
///   · si la cría SIGUE a la madre en tierra (deseo «heel» de <see cref="DesireCatalog"/>),
///   · si la hembra pare en una GUARIDA (<see cref="MaternityDen"/>: entra antes del parto y sale con las crías),
///   · si pone HUEVOS (<see cref="Egg"/>: quién incuba y cuánto).
/// Las duraciones van en MESES REALES y se escalan al año del santuario (<see cref="TimeController.SecondsForMonths"/>), igual
/// que la gestación (<see cref="Reproduction.Calendar"/>) y las etapas de vida (<see cref="StageProfile"/>, en días reales).
/// Lo de cada especie sale de su biología real (fuentes en docs/ciclo-de-vida.md); [A] = valor de referencia general.
/// Una especie sin entrada no tiene estos procesos (los insectos del microcosmos siguen como estaban).
/// </summary>
public class LifeHistory
{
    /// <summary>De dónde saca el agua.</summary>
    public enum Water : byte
    {
        None,
        /// <summary>Bebe agua dulce (un bebedero, un charco) y, si no hay, come nieve.</summary>
        Drinks,
        /// <summary>En el mar bebe agua salada (glándula de la sal) y en tierra come nieve.</summary>
        SeaOrSnow,
        /// <summary>No bebe: el agua le llega con la comida (agua de las presas y agua metabólica de la grasa).</summary>
        FromFood,
        /// <summary>El agua de la miel/néctar (y la condensación del racimo en el panal).</summary>
        FromHoney,
    }

    /// <summary>Cómo alimenta (o protege) a la cría.</summary>
    public enum Care : byte { None, Lactation, Regurgitation, GuardsEggSac, Hive }

    /// <summary>Quién incuba el huevo.</summary>
    public enum Incubator : byte { Female, Male, Both }

    public Water water;
    /// <summary>Horas de juego entre una toma de agua y la siguiente (solo si bebe o come nieve).</summary>
    public float drinkHours = 8f;
    public string waterLabel, waterSource;

    public Care care;
    /// <summary>Los dos progenitores alimentan a la cría (si no, solo la madre).</summary>
    public bool biparental;
    public string careLabel;

    /// <summary>La cría sigue a su madre (o a su padre) en tierra mientras es cría; a esta distancia (m).</summary>
    public bool youngFollow;
    public float followDistance = 3f;
    public string followLabel;

    /// <summary>La hembra preñada entra en una guarida <see cref="denBeforeMonths"/> antes del parto, pare dentro y sale
    /// con las crías <see cref="denAfterMonths"/> después.</summary>
    public bool den;
    public float denBeforeMonths, denAfterMonths;
    /// <summary>En la guarida no come: vive de su grasa (la osa ayuna hasta 8 meses).</summary>
    public bool denFasts;
    public string denLabel, denExitLabel;

    /// <summary>Pone huevos: la «gestación» de <see cref="Reproduction.Calendar"/> es de la cópula a la puesta; luego el huevo
    /// se incuba <see cref="incubationMonths"/>.</summary>
    public bool eggs;
    public float incubationMonths;
    public Incubator incubator;
    /// <summary>El huevo va sobre las patas del que incuba (pingüino emperador: no hay nido de piedras); si no, queda en su
    /// sitio (saco de huevos en la tela).</summary>
    public bool eggOnFeet;
    /// <summary>El huevo queda en un NIDO y quien incuba se queda encima (pingüino de Adelia, sobre su nido de guijarros): ni va sobre las
    /// patas ni es un saco que se desarrolla solo.</summary>
    public bool eggNest;
    /// <summary>Horas de juego que el huevo aguanta sin nadie encima antes de helarse (0 = no se hiela: el saco de la araña).</summary>
    public float eggChillHours;
    public string eggLabel, layLabel, incubateLabel, hatchLabel;

    public string sources;

    static Dictionary<string, LifeHistory> _catalog;

    public static bool Has(string species) { if (_catalog == null) Build(); return species != null && _catalog.ContainsKey(species); }
    public static LifeHistory Of(string species)
    {
        if (_catalog == null) Build();
        return species != null && _catalog.TryGetValue(species, out LifeHistory h) ? h : _none;
    }
    static readonly LifeHistory _none = new LifeHistory();

    static void Build() => _catalog = new Dictionary<string, LifeHistory>
    {
        // OSO POLAR. Agua: no bebe; la saca de la grasa de foca (agua metabólica) [A]. Lactancia: los oseznos maman solo de
        // la madre los 3 primeros meses y siguen con ella hasta ~2,5 años (Derocher et al. 1993, Can J Zool). Guarida
        // de maternidad: la hembra preñada entra en noviembre (Amstrup & Gardner 1994, J Wildl Manage 58:1: entrada media
        // 11–22 nov), pare a principios de diciembre–enero y sale a mediados–finales de marzo (Smith et al. 2007, Arctic 60:187;
        // emergencia media 16 mar, JMammal 105:gyae010) → ~1,2 meses antes del parto y ~3 meses después. Ayuna todo ese tiempo.
        { "Bear", new LifeHistory {
            water = Water.FromFood, waterLabel = "Agua: no bebe; la saca de la grasa de foca (agua metabólica)",
            care = Care.Lactation, careLabel = "Crianza: la osa amamanta a sus oseznos",
            youngFollow = true, followDistance = 3f, followLabel = "Los oseznos van pegados a su madre",
            den = true, denBeforeMonths = 1.2f, denAfterMonths = 3f, denFasts = true,
            denLabel = "La osa preñada entra en su guarida de nieve y pare dentro", denExitLabel = "Sale de la guarida con los oseznos (≈3 meses)",
            sources = "Derocher, Andriashek & Arnould 1993 (Can J Zool, lactancia); Amstrup & Gardner 1994 (J Wildl Manage 58:1); Smith et al. 2007 (Arctic 60:187); J Mammal 105:gyae010 (2024)" } },
        // FOCA COMÚN (Phoca vitulina). Agua de la presa y metabólica; los pinnípedos casi no beben (Ortiz 2001, J Exp Biol
        // 204:1831). Lactancia ~24 días (Boness et al. 1994; Thompson et al. 1994; destete: Muelbert & Bowen 1993, Can J Zool
        // 71:1405, doi 10.1139/z93-194); la cría nada con la madre desde que nace (MarineLife).
        { "Seal", new LifeHistory {
            water = Water.FromFood, waterLabel = "Agua: no bebe; la saca de los peces que come (y de su grasa)",
            care = Care.Lactation, careLabel = "Crianza: la foca amamanta a su cría (~24 días)",
            youngFollow = true, followDistance = 2f, followLabel = "La cría nada (o se tumba) junto a su madre",
            sources = "Ortiz 2001 (J Exp Biol 204:1831); Muelbert & Bowen 1993 (doi 10.1139/z93-194); Boness et al. 1994" } },
        // BALLENA JOROBADA. Agua de la presa (krill) y metabólica (Ortiz 2001). Lactancia ~11–12 meses; la cría acompaña a la
        // madre en la migración [A].
        { "Whale", new LifeHistory {
            water = Water.FromFood, waterLabel = "Agua: no bebe; la saca del krill y los peces (y de su grasa)",
            care = Care.Lactation, careLabel = "Crianza: la ballena amamanta a su ballenato (~1 año)",
            youngFollow = true, followDistance = 2f, followLabel = "El ballenato nada junto a su madre",
            sources = "Ortiz 2001 (J Exp Biol 204:1831); lactancia de la jorobada ≈1 año [A]" } },
        // ORCA. Agua de la presa (Ortiz 2001). Las crías maman 1–2 años y viven con la madre (sociedad matrilineal) [A].
        { "Orca", new LifeHistory {
            water = Water.FromFood, waterLabel = "Agua: no bebe; la saca de sus presas",
            care = Care.Lactation, careLabel = "Crianza: la orca amamanta a su cría (1–2 años)",
            youngFollow = true, followDistance = 2f, followLabel = "La cría nada junto a su madre",
            sources = "Ortiz 2001 (J Exp Biol 204:1831); lactancia 1–2 años [A]" } },
        // PINGÜINO EMPERADOR (Aptenodytes forsteri). Agua: bebe agua de mar y expulsa la sal por la glándula supraorbitaria
        // (Schmidt-Nielsen 1960, Circulation 21:955); en tierra come nieve [A]. No hay nido: la hembra pone UN huevo y se lo
        // pasa al macho, que lo incuba sobre las patas 62–67 días (media 64, récord Guinness; 65 d en Groscolas) ayunando, y
        // ella se va dos meses al mar; vuelve justo antes de la eclosión. Si llega tarde, el macho alimenta al pollito con una
        // secreción del esófago. Luego lo alimentan los dos regurgitando; el pollo va sobre las patas de un padre ~45 días y
        // se independiza con el plumaje impermeable (~150 días). Un huevo al aire a −40 °C se hiela en minutos.
        { "EmperorPenguin", new LifeHistory {
            water = Water.SeaOrSnow, drinkHours = 6f, waterLabel = "Bebe agua de mar (glándula de la sal) o come nieve",
            care = Care.Regurgitation, biparental = true, careLabel = "Crianza: los padres alimentan al polluelo (regurgitan)",
            youngFollow = true, followDistance = 1.2f, followLabel = "El polluelo, al abrigo de sus padres (sobre sus patas)",
            eggs = true, incubationMonths = 2.1f, incubator = Incubator.Male, eggOnFeet = true, eggChillHours = 2f,
            eggLabel = "La hembra forma el huevo (cópula → puesta)", layLabel = "Pone el huevo (sobre las patas: no hay nido)",
            incubateLabel = "El macho incuba el huevo (ella se lo pasa y se va al mar)", hatchLabel = "Eclosión (nace el polluelo)",
            sources = "Schmidt-Nielsen 1960 (Circulation 21:955); Prévost 1961; Groscolas (ayuno de 115 d: cortejo 45 + incubación 65); Guinness (incubación 62–67 d)" } },
        // PINGÜINO DE ADELIA (Pygoscelis adeliae): el «Penguin» del catálogo desde 2026-11-05 (antes mezclaba al emperador con los
        // guijarros de Adelia). Nido de piedras; 2 huevos (aquí 1, como todo parto); incubación 32–37 d y el macho hace el primer
        // turno largo, ~2 semanas, en el 85 % de los nidos, mientras ella come en el mar (Davis 1988, Auk 105:428): aquí ese
        // primer turno vale por toda la incubación. Guardería a las 2–4 semanas y emplume a los ~53 d (NZ Birds Online).
        { "Penguin", new LifeHistory {
            water = Water.SeaOrSnow, drinkHours = 6f, waterLabel = "Bebe agua de mar (glándula de la sal) o come nieve",
            care = Care.Regurgitation, biparental = true, careLabel = "Crianza: los padres alimentan al polluelo (regurgitan)",
            youngFollow = true, followDistance = 2f, followLabel = "El polluelo, junto al nido y a sus padres",
            eggs = true, incubationMonths = 1.15f, incubator = Incubator.Male, eggNest = true, eggChillHours = 2f,
            eggLabel = "La hembra forma el huevo (cópula → puesta)", layLabel = "Pone el huevo en el nido de guijarros",
            incubateLabel = "El macho hace el primer turno largo en el nido (ella se va al mar)", hatchLabel = "Eclosión (nace el polluelo)",
            sources = "Davis 1988 (Auk 105:428); NZ Birds Online (Adelie penguin); Animal Diversity Web" } },
        // PETREL DE LAS NIEVES (Pagodroma nivea). Un huevo en grieta o repisa de roca, incubación 41–49 d por los dos (aquí el macho hace el turno que vale por todos,
        // como en Adelia), pollo ~46 d, primera cría a los ~11 años (WorldSpecies; BirdForum).
        { "SnowPetrel", new LifeHistory {
            water = Water.SeaOrSnow, drinkHours = 8f, waterLabel = "Bebe agua de mar o come nieve",
            care = Care.Regurgitation, biparental = true, careLabel = "Crianza: los padres van y vienen a alimentar al pollo",
            youngFollow = true, followDistance = 1.5f, followLabel = "El pollo, en el nido, con sus padres",
            eggs = true, incubationMonths = 1.5f, incubator = Incubator.Male, eggNest = true, eggChillHours = 4f,
            eggLabel = "La hembra forma el huevo", layLabel = "Pone un huevo en su grieta de roca",
            incubateLabel = "El macho hace el turno en el nido", hatchLabel = "Eclosión (nace el pollo)",
            sources = "Australian Antarctic Program; WorldSpecies (Pagodroma nivea); BirdForum Opus" } },
        // MORSA (Odobenus rosmarus). No bebe: agua de la presa (Ortiz 2001). Gestación ~15 meses con 4–5 de implantación diferida, una
        // cría cada ~2,6 años (BMC Zoology 2022, doi 10.1186/s40850-022-00158-1), lactancia 12–36 meses (ADW): la cría nada con la madre.
        { "Walrus", new LifeHistory {
            water = Water.FromFood, waterLabel = "Agua: no bebe; la saca de los bivalvos que come (y de su grasa)",
            care = Care.Lactation, careLabel = "Crianza: la morsa amamanta a su cría (hasta ~2 años)",
            youngFollow = true, followDistance = 2f, followLabel = "La cría nada (o se tumba) junto a su madre",
            sources = "doi 10.1186/s40850-022-00158-1 (BMC Zoology 2022); Ortiz 2001 (J Exp Biol 204:1831); Animal Diversity Web" } },
        // RENO (Rangifer tarandus). Come nieve o bebe de charcas; gestación 228–234 d, una cría al año; la cría pasta desde ~45 d
        // y mama hasta el otoño, siguiendo a la madre (ADW; Wikipedia).
        { "Reindeer", new LifeHistory {
            water = Water.Drinks, drinkHours = 10f, waterLabel = "Bebe de las charcas o come nieve",
            care = Care.Lactation, careLabel = "Crianza: la hembra amamanta a su cría",
            youngFollow = true, followDistance = 3f, followLabel = "La cría corre junto a su madre",
            sources = "Animal Diversity Web (Rangifer tarandus); doi 10.1016/j.theriogenology.2009.01.022" } },
        // BUEY ALMIZCLERO (Ovibos moschatus). Gestación ~8 meses, una cría cada 1–2 años, destete 10–14 meses (ADW; GBIF).
        { "MuskOx", new LifeHistory {
            water = Water.Drinks, drinkHours = 10f, waterLabel = "Bebe de las charcas o come nieve",
            care = Care.Lactation, careLabel = "Crianza: la hembra amamanta a su ternero",
            youngFollow = true, followDistance = 2.5f, followLabel = "El ternero va pegado a su madre",
            sources = "Animal Diversity Web (Ovibos moschatus); GBIF" } },
        // LOBO ÁRTICO (Canis lupus arctos). Hay pocas medidas de esta subespecie; gestación 63 d de Canis lupus [A]. Guarida en roca o
        // cueva: el suelo helado no se puede excavar (Mech & Packard 1990: una usada >700 años). NE de Groenlandia: ~2 cachorros.
        { "Wolf", new LifeHistory {
            water = Water.Drinks, drinkHours = 10f, waterLabel = "Bebe de las charcas o come nieve",
            care = Care.Lactation, careLabel = "Crianza: la loba amamanta a sus cachorros",
            youngFollow = true, followDistance = 3f, followLabel = "Los cachorros van detrás de sus padres",
            den = true, denBeforeMonths = 0.05f, denAfterMonths = 0.8f, denFasts = false,
            denLabel = "La loba entra en su guarida de roca y pare dentro", denExitLabel = "Los cachorros salen de la guarida (≈3 semanas)",
            sources = "Mech & Packard 1990 (Can Field-Nat); Marquard-Petersen 2008 (Can Field-Nat 122, doi 10.22621/cfn.v122i2.573); gestación [A]" } },
        // LIEBRE ÁRTICA (Lepus arcticus). Come nieve; gestación 50–53 d, 2–8 lebratos (aquí 1 por parto), destete 8–9 semanas (ADW).
        { "ArcticHare", new LifeHistory {
            water = Water.Drinks, drinkHours = 8f, waterLabel = "Come nieve o bebe de las charcas",
            care = Care.Lactation, careLabel = "Crianza: la liebre amamanta a sus lebratos",
            youngFollow = true, followDistance = 3f, followLabel = "Los lebratos se quedan cerca de su madre",
            sources = "Animal Diversity Web (Lepus arcticus)" } },
        // ZORRO ÁRTICO (Vulpes lagopus). Gestación ~52 d (46–58), camada 5–8; madriguera en montículos de 1–4 m con 4–8 bocas, usada
        // décadas o siglos; crían los dos padres (ADW; Mammalian Species 713).
        { "ArcticFox", new LifeHistory {
            water = Water.Drinks, drinkHours = 8f, waterLabel = "Bebe de las charcas o come nieve",
            care = Care.Lactation, biparental = true, careLabel = "Crianza: la zorra amamanta y el macho trae comida",
            youngFollow = true, followDistance = 3f, followLabel = "Los cachorros van detrás de sus padres",
            den = true, denBeforeMonths = 0.05f, denAfterMonths = 0.7f, denFasts = false,
            denLabel = "La zorra entra en su madriguera y pare dentro", denExitLabel = "Los cachorros salen de la madriguera",
            sources = "Animal Diversity Web (Vulpes lagopus); Audet et al. 2002 (Mammalian Species 713)" } },
        // MALAMUTE (perro). Bebe ~50–60 ml/kg/día; los perros de trineo comen nieve si no tienen agua [A]. Gestación 63 d
        // (Reproduction.Calendar); 12–24 h antes del parto la perra hace el nido; los cachorros maman ~7–8 semanas y no salen
        // del nido hasta las ~3 semanas (abren los ojos a las 2) [A].
        { "Malamute", new LifeHistory {
            water = Water.Drinks, drinkHours = 8f, waterLabel = "Bebe agua (bebedero) o come nieve",
            care = Care.Lactation, careLabel = "Crianza: la perra amamanta a sus cachorros",
            youngFollow = true, followDistance = 3f, followLabel = "Los cachorros van detrás de su madre",
            den = true, denBeforeMonths = 0.04f, denAfterMonths = 0.7f, denFasts = false,
            denLabel = "La perra hace el nido en la caseta y pare dentro", denExitLabel = "Los cachorros salen del nido (≈3 semanas)",
            sources = "Gestación 63 ± 2 d (doi 10.1111/j.1439-0531.2011.01773.x); desarrollo del cachorro [A]" } },
        // ABEJA DE LAS NIEVES (abeja melífera modificada). Agua: la del néctar/miel y la condensación del racimo [A]; cría en
        // el panal (Hive): huevo 3 d, larva 6, pupa 12 (obrera) o 14,5 (zángano); la reina se aparea en vuelos nupciales.
        { "SnowBee", new LifeHistory {
            water = Water.FromHoney, waterLabel = "Agua: la de la miel y el néctar (y la condensación del racimo)",
            care = Care.Hive, careLabel = "Las nodrizas alimentan a las larvas",
            sources = "docs/abejas-y-panal.md; Winston 1987, The Biology of the Honey Bee [A]" } },
        // ARAÑA DE LAS NIEVES (ficticia: araña orbicular de ~30 cm adaptada al frío; ver docs/ciclo-de-vida.md). Agua: de la
        // hemolinfa de sus presas (y gotas en la tela) [A]. La madre teje un saco de huevos en su tela y lo guarda hasta la
        // eclosión (como Theraphosa: 6–8 semanas); las arañitas viven del vitelo y no se alimentan: luego se dispersan.
        { "SnowSpider", new LifeHistory {
            water = Water.FromFood, waterLabel = "Agua: la de sus presas (y las gotas de su tela)",
            care = Care.GuardsEggSac, careLabel = "La madre guarda el saco de huevos en su tela",
            youngFollow = false, followLabel = "Las arañitas se quedan junto a la madre y luego se dispersan",
            eggs = true, incubationMonths = 1.75f, incubator = Incubator.Female, eggOnFeet = false, eggChillHours = 0f,
            eggLabel = "La hembra forma los huevos (cópula → puesta)", layLabel = "Teje el saco de huevos en su tela",
            incubateLabel = "La madre guarda el saco de huevos", hatchLabel = "Eclosión: salen las arañitas",
            sources = "Theraphosa blondi (Britannica; Striffler 2005); Bowden & Buddle 2012 (Can J Zool 90:714, doi 10.1139/z2012-038)" } },
    };
}
