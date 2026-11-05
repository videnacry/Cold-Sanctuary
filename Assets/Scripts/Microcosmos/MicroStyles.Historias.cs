using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS ESTILOS DE LA TANDA 3 DE HISTORIAS (docs/microcosmos-historias-3.md, 2026-11-05): los nombres de época y lugar de cada edificio
/// del pueblo y los frutales de sus entradas nuevas del catálogo (<see cref="MicroStyles.All"/>): Andes republicanos, mapuche, inca,
/// haudenosaunee, Egipto ptolemaico, Virginia de 1960, Viena de 1824, las Provincias Unidas de 1637, la Amazonía peruana, España de hoy y la
/// feria inventada. En una entrada de esta tabla, LO QUE NO ESTÁ NO EXISTÍA (<see cref="MicroStyles.CivicName"/> da null y no se construye).
/// </summary>
public static partial class MicroStyles
{
    static readonly Dictionary<string, Dictionary<Civic, string>> HistoNames = new Dictionary<string, Dictionary<Civic, string>>
    {
        ["andes_republicano"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Escuela Normal", [Civic.Entrenamiento] = "Cancha de pelota nacional", [Civic.Baile] = "Salón de baile (pasillo y sanjuanito)", [Civic.Plaza] = "Plaza del barrio",
            [Civic.Estacion] = "Parada del ferrocarril", [Civic.Parque] = "Alameda", [Civic.Museo] = "Museo de la escuela", [Civic.Investigacion] = "Observatorio Astronómico",
            [Civic.Guarderia] = "Casa cuna", [Civic.Monumento] = "Monumento", [Civic.Policia] = "Policía del barrio", [Civic.Bomberos] = "Cuerpo de bomberos", [Civic.Mercado] = "Mercado",
            [Civic.Mecanica] = "Taller del carretero", [Civic.Taller] = "Taller de artesanos", [Civic.Piscina] = "Baños públicos", [Civic.Hotel] = "Hotel", [Civic.Carpinteria] = "Carpintería",
            [Civic.Pileta] = "Pila del barrio", [Civic.Obra] = "Obra: casa en construcción", [Civic.Tecnologia] = "Oficina de telégrafos", [Civic.Edificio] = "Casa de dos plantas con balcón", [Civic.Sanacion] = "Hospital",
        },
        ["mapuche"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Ruka del kimche (enseñanza oral)", [Civic.Entrenamiento] = "Paliwe (cancha de palín)", [Civic.Baile] = "Lepün de la danza (choike purrun)", [Civic.Plaza] = "Lepün (campo de reunión)",
            [Civic.Estacion] = "Paso del río (wampo, la canoa)", [Civic.Parque] = "Menoko (humedal del lawen)", [Civic.Guarderia] = "Ruka de los niños", [Civic.Monumento] = "Chemamull (figura de madera)",
            [Civic.Mercado] = "Trafkintu (intercambio)", [Civic.Taller] = "Taller de cestos y piedra", [Civic.Carpinteria] = "Taller de madera (wanco y wiño)", [Civic.Pileta] = "Vertiente del lof",
            [Civic.Obra] = "Rukatun (levantar una ruka entre todos)", [Civic.Sanacion] = "Ruka de la machi",
        },
        ["inca"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Yachay wasi (casa del saber)", [Civic.Entrenamiento] = "Campo del warachikuy", [Civic.Baile] = "Patio de taki (canto y danza)", [Civic.Plaza] = "Haukaypata (la gran plaza)",
            [Civic.Estacion] = "Tambo del Qhapaq Ñan (posta de chasquis)", [Civic.Parque] = "Jardín del Inca", [Civic.Investigacion] = "Ushnu (mirar el sol)", [Civic.Guarderia] = "Casa de cuidado de los niños",
            [Civic.Monumento] = "Huanca (piedra sagrada)", [Civic.Policia] = "Casa del tukuy rikuq (el que todo lo ve)", [Civic.Mercado] = "Qhatu (trueque del valle)", [Civic.Taller] = "Taller de orfebres y ceramistas",
            [Civic.Carpinteria] = "Taller de madera (qeros y tianas)", [Civic.Pileta] = "Paqcha (fuente del canal)", [Civic.Obra] = "Obra de la mit'a (muro de piedra)", [Civic.Piscina] = "Pultumarca (baños termales)",
            [Civic.Hotel] = "Tambo (posada del camino)", [Civic.Edificio] = "Kallanka (gran sala)", [Civic.Sanacion] = "Casa del hampikamayuq (el curandero)",
        },
        ["haudenosaunee"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Casa larga de los relatos (enseñanza oral)", [Civic.Entrenamiento] = "Campo de tewaarathon", [Civic.Baile] = "Claro de la danza", [Civic.Plaza] = "Claro del consejo",
            [Civic.Estacion] = "Embarcadero de canoas de corteza", [Civic.Parque] = "Arboleda de arces y nogales", [Civic.Guarderia] = "Casa de las abuelas", [Civic.Monumento] = "Poste del clan",
            [Civic.Mercado] = "Intercambio de la aldea", [Civic.Taller] = "Taller de conchas y cestos", [Civic.Carpinteria] = "Taller de corteza y canoas", [Civic.Pileta] = "Manantial de la aldea",
            [Civic.Obra] = "Casa larga en construcción", [Civic.Piscina] = "Cabaña de sudar", [Civic.Sanacion] = "Casa de las medicinas",
        },
        ["egipto_ptolemaico"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Escuela (didaskaleion)", [Civic.Entrenamiento] = "Gimnasio (palestra y xisto)", [Civic.Baile] = "Odeón (música y danza)", [Civic.Plaza] = "Ágora", [Civic.Estacion] = "Puerto del lago Mareotis",
            [Civic.Parque] = "Jardines del Paneo", [Civic.Museo] = "Tesoro del Serapeo", [Civic.Investigacion] = "Observatorio (gnomon y esfera)", [Civic.Guarderia] = "Casa de cuidado de los niños", [Civic.Monumento] = "Estatua real",
            [Civic.Policia] = "Puesto de los phylakitai", [Civic.Mercado] = "Emporion (mercado del puerto)", [Civic.Mecanica] = "Taller del carretero (ruedas y ejes)", [Civic.Taller] = "Taller de artesanos (vidrio y papiro)",
            [Civic.Piscina] = "Baños (balaneion)", [Civic.Hotel] = "Katagogion (posada)", [Civic.Carpinteria] = "Carpintería", [Civic.Pileta] = "Fuente (krene)", [Civic.Obra] = "Obra: casa en construcción",
            [Civic.Tecnologia] = "Taller de mecanismos (engranajes y relojes de agua)", [Civic.Edificio] = "Casa de dos plantas con terraza", [Civic.Sanacion] = "Asclepeion (santuario de curación)",
        },
        ["virginia_1960"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Escuela", [Civic.Entrenamiento] = "Gimnasio", [Civic.Baile] = "Salón de baile", [Civic.Plaza] = "Plaza (town square)", [Civic.Estacion] = "Estación de ferrocarril", [Civic.Parque] = "Parque",
            [Civic.Museo] = "Museo", [Civic.Investigacion] = "Laboratorio de investigación", [Civic.Guarderia] = "Guardería", [Civic.Monumento] = "Monumento", [Civic.Policia] = "Comisaría de policía", [Civic.Bomberos] = "Firehouse (parque de bomberos)",
            [Civic.Mercado] = "Supermercado", [Civic.Mecanica] = "Taller mecánico (garaje)", [Civic.Taller] = "Taller de artesanos", [Civic.Piscina] = "Piscina municipal", [Civic.Hotel] = "Hotel", [Civic.Carpinteria] = "Carpintería",
            [Civic.Pileta] = "Fuente", [Civic.Obra] = "Obra: casa en construcción", [Civic.Tecnologia] = "Centro de cálculo (ordenadores)", [Civic.Edificio] = "Edificio de apartamentos", [Civic.Sanacion] = "Hospital",
        },
        ["viena_biedermeier"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Escuela", [Civic.Entrenamiento] = "Sala de esgrima", [Civic.Baile] = "Salón de baile", [Civic.Plaza] = "Plaza mayor", [Civic.Estacion] = "Posta de diligencias", [Civic.Parque] = "Jardín público",
            [Civic.Museo] = "Museo", [Civic.Investigacion] = "Gabinete científico", [Civic.Guarderia] = "Casa de cuidado de los niños", [Civic.Monumento] = "Monumento", [Civic.Policia] = "Comisaría de policía", [Civic.Bomberos] = "Parque de bomberos",
            [Civic.Mercado] = "Mercado", [Civic.Mecanica] = "Taller del carretero (ruedas y ejes)", [Civic.Taller] = "Taller de pianos y violines", [Civic.Piscina] = "Baños del Danubio", [Civic.Hotel] = "Posada", [Civic.Carpinteria] = "Carpintería",
            [Civic.Pileta] = "Fuente y lavadero", [Civic.Obra] = "Obra: casa en construcción", [Civic.Edificio] = "Inmueble de varias plantas", [Civic.Sanacion] = "Hospital general (Allgemeines Krankenhaus)",
        },
        ["provincias_1600"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Escuela latina", [Civic.Entrenamiento] = "Sala de esgrima", [Civic.Plaza] = "Plaza del mercado", [Civic.Estacion] = "Embarcadero de la barca de sirga", [Civic.Parque] = "Jardín público",
            [Civic.Investigacion] = "Observatorio de la Universidad", [Civic.Guarderia] = "Hofje (casa de acogida)", [Civic.Monumento] = "Monumento", [Civic.Policia] = "Casa de la guardia cívica (schutterij)", [Civic.Bomberos] = "Cubos y escalas del barrio",
            [Civic.Mercado] = "Mercado de la Pesa", [Civic.Mecanica] = "Taller del carretero (ruedas y ejes)", [Civic.Taller] = "Taller de paños", [Civic.Hotel] = "Posada", [Civic.Carpinteria] = "Carpintería", [Civic.Pileta] = "Pozo y bomba del barrio",
            [Civic.Obra] = "Obra: casa en construcción", [Civic.Edificio] = "Casa de dos plantas", [Civic.Sanacion] = "Hospital de Santa Cecilia",
        },
        ["amazonia_1950"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Escuela de la colonia", [Civic.Entrenamiento] = "Cancha de fútbol de la colonia", [Civic.Baile] = "Salón de baile de la colonia", [Civic.Plaza] = "Plaza de la colonia", [Civic.Estacion] = "Embarcadero del río",
            [Civic.Parque] = "Jardín de la colonia", [Civic.Guarderia] = "Casa de cuidado de los niños", [Civic.Monumento] = "Cruz de la plaza", [Civic.Mercado] = "Bodega de la colonia", [Civic.Taller] = "Taller de la colonia",
            [Civic.Carpinteria] = "Carpintería de la colonia", [Civic.Pileta] = "Tanque de agua de lluvia", [Civic.Obra] = "Obra: casa sobre pilotes", [Civic.Piscina] = "Baño del río", [Civic.Sanacion] = "Hospital de la colonia",
        },
        ["espana_actual"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Colegio público", [Civic.Entrenamiento] = "Polideportivo municipal", [Civic.Baile] = "Escuela de baile", [Civic.Plaza] = "Plaza", [Civic.Estacion] = "Estación de metro", [Civic.Parque] = "Parque",
            [Civic.Museo] = "Museo", [Civic.Investigacion] = "Centro de investigación", [Civic.Guarderia] = "Escuela infantil", [Civic.Monumento] = "Monumento", [Civic.Policia] = "Comisaría de policía", [Civic.Bomberos] = "Parque de bomberos",
            [Civic.Mercado] = "Mercado municipal", [Civic.Mecanica] = "Taller mecánico", [Civic.Taller] = "Taller de artesanos", [Civic.Piscina] = "Piscina municipal", [Civic.Hotel] = "Hotel", [Civic.Carpinteria] = "Carpintería",
            [Civic.Pileta] = "Fuente", [Civic.Obra] = "Obra (bloque nuevo)", [Civic.Tecnologia] = "Centro de datos", [Civic.Edificio] = "Edificio de viviendas", [Civic.Sanacion] = "Centro de salud",
        },
        ["exposicion_mundial"] = new Dictionary<Civic, string>
        {
            [Civic.Estudio] = "Aula de la Feria", [Civic.Entrenamiento] = "Gimnasio de la Feria", [Civic.Baile] = "Escenario de bailes del mundo", [Civic.Plaza] = "Plaza de las Hazañas", [Civic.Estacion] = "Estación de la Feria", [Civic.Parque] = "Jardín de la Feria",
            [Civic.Museo] = "Museo de la Feria", [Civic.Guarderia] = "Guardería de la Feria", [Civic.Monumento] = "Monumento a los ciudadanos anónimos", [Civic.Mercado] = "Mercado de la Feria", [Civic.Taller] = "Taller de la Feria", [Civic.Piscina] = "Piscina de la Feria",
            [Civic.Hotel] = "Hotel de la Feria", [Civic.Carpinteria] = "Carpintería de la Feria", [Civic.Pileta] = "Fuente", [Civic.Obra] = "Obra: el próximo pabellón", [Civic.Tecnologia] = "Centro de datos de la Feria", [Civic.Sanacion] = "Puesto de salud de la Feria",
        },
    };

    /// <summary>Los frutales de las entradas de la tanda 3 (null: no es una de ellas). Cada uno, de su lugar y su época: en la Onondaga de
    /// 1450 no hay manzanos (llegaron de Europa) y en la Cajamarca de 1533, ni naranjos.</summary>
    static Fruit[] HistoFruits(Entry e, Color leaf, Color dark, Color grey)
    {
        switch (e.id)
        {
            case "andes_republicano": return new[] { new Fruit("Capulí (Prunus serotina)", 7f, leaf, F(0.25f, 0.08f, 0.12f)), new Fruit("Tomate de árbol (Solanum betaceum)", 3f, leaf, F(0.8f, 0.25f, 0.15f)), new Fruit("Chirimoya (Annona cherimola)", 5f, leaf, F(0.55f, 0.65f, 0.35f)) };
            case "mapuche": return new[] { new Fruit("Maqui (Aristotelia chilensis)", 3f, leaf, F(0.15f, 0.1f, 0.2f)), new Fruit("Avellano chileno (Gevuina avellana)", 8f, dark, F(0.55f, 0.2f, 0.15f)), new Fruit("Murta (Ugni molinae)", 1.4f, dark, F(0.75f, 0.2f, 0.25f), bush: true) };
            case "inca": return new[] { new Fruit("Lúcuma (Pouteria lucuma)", 8f, dark, F(0.85f, 0.6f, 0.15f)), new Fruit("Chirimoya (Annona cherimola)", 5f, leaf, F(0.55f, 0.65f, 0.35f)), new Fruit("Pacay (Inga feuilleei)", 8f, leaf, F(0.5f, 0.6f, 0.3f)) };
            case "haudenosaunee": return new[] { new Fruit("Ciruelo americano (Prunus americana)", 5f, leaf, F(0.75f, 0.25f, 0.2f)), new Fruit("Nogal americano (Carya ovata)", 9f, leaf, F(0.55f, 0.5f, 0.3f)), new Fruit("Arándano (Vaccinium corymbosum)", 1.6f, leaf, F(0.2f, 0.25f, 0.5f), bush: true) };
            case "egipto_ptolemaico": return new[] { new Fruit("Palmera datilera (Phoenix dactylifera)", 9f, grey, F(0.6f, 0.35f, 0.15f), palm: true), new Fruit("Higuera (Ficus carica)", 4f, leaf, F(0.45f, 0.3f, 0.4f)), new Fruit("Granado (Punica granatum)", 3.5f, leaf, F(0.75f, 0.15f, 0.15f)), new Fruit("Sicómoro (Ficus sycomorus)", 7f, leaf, F(0.8f, 0.55f, 0.35f)) };
            case "virginia_1960": return new[] { new Fruit("Melocotonero (Prunus persica)", 3.5f, leaf, F(0.95f, 0.6f, 0.4f)), new Fruit("Caqui de Virginia (Diospyros virginiana)", 7f, leaf, F(0.9f, 0.5f, 0.15f)), new Fruit("Higuera (Ficus carica)", 4f, leaf, F(0.45f, 0.3f, 0.4f)) };
            case "viena_biedermeier": return new[] { new Fruit("Albaricoquero (Prunus armeniaca)", 4f, leaf, F(0.95f, 0.65f, 0.3f)), new Fruit("Cerezo (Prunus avium)", 5f, leaf, F(0.7f, 0.05f, 0.1f)), new Fruit("Peral en espaldera (Pyrus communis)", 3f, leaf, F(0.8f, 0.75f, 0.3f)) };
            case "provincias_1600": return new[] { new Fruit("Manzano (Malus domestica)", 4.5f, leaf, F(0.75f, 0.15f, 0.12f)), new Fruit("Peral en espaldera (Pyrus communis)", 3f, leaf, F(0.8f, 0.75f, 0.3f)), new Fruit("Ciruelo (Prunus domestica)", 4f, leaf, F(0.4f, 0.2f, 0.45f)) };
            case "amazonia_1950": return new[] { new Fruit("Aguaje (Mauritia flexuosa)", 12f, F(0.3f, 0.5f, 0.25f), F(0.6f, 0.25f, 0.15f), palm: true), new Fruit("Plátano (Musa)", 4f, F(0.3f, 0.55f, 0.25f), F(0.9f, 0.85f, 0.3f), palm: true), new Fruit("Papayo (Carica papaya)", 4f, F(0.3f, 0.55f, 0.25f), F(0.95f, 0.6f, 0.15f), palm: true), new Fruit("Camu camu (Myrciaria dubia)", 2f, dark, F(0.65f, 0.1f, 0.2f), bush: true) };
            case "espana_actual": return new[] { new Fruit("Almendro (Prunus dulcis)", 4f, grey, F(0.7f, 0.75f, 0.5f)), new Fruit("Olivo (Olea europaea)", 5f, grey, F(0.25f, 0.3f, 0.15f)), new Fruit("Granado (Punica granatum)", 3.5f, leaf, F(0.75f, 0.15f, 0.15f)) };
            case "exposicion_mundial": return new[] { new Fruit("Naranjo (Citrus sinensis)", 4f, dark, F(0.95f, 0.55f, 0.1f)), new Fruit("Mango (Mangifera indica)", 8f, dark, F(0.95f, 0.65f, 0.15f)), new Fruit("Olivo (Olea europaea)", 5f, grey, F(0.25f, 0.3f, 0.15f)) };
            default: return null;
        }
    }
}
