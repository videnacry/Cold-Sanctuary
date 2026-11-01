using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LOS EDIFICIOS REALES DE CADA ERA en los microcosmos civilizados (docs/microcosmos-civilizaciones.md §8; Berón 2026-11-01:
/// «se deben crear edificaciones reales, debes investigar planos e imágenes para poder recrear las estructuras de la era e
/// historia, y deben ser interactuables, por ejemplo restaurantes o cocinas o dojos o salas de yoga o hospitales o jardines»).
/// Cada edificio es un <see cref="AreaFunctionSite"/> con una plantilla del REGISTRO (<see cref="AreaFunctions.Buildings"/>) y
/// sus muebles son <see cref="AreaFunctionStation"/> (interactuables): se reutilizan las funciones del santuario (el servicio de
/// la carta, pasar visita, la clase de yoga, el riego…) a la escala de los insectos-humanos (~1,7 m de pie: puertas de 2,4 m,
/// salas de 6–20 m). Los habita gente de la era con su JORNADA (<see cref="MicroJornada"/>) y familias de especies mezcladas que
/// tienen hijos (<see cref="FolkCouple"/>, <see cref="InsectGenealogy"/>).
/// Los edificios van alrededor del claro de la historia (sus estaciones están a menos de ~24 m del centro): a ±31 m en x y a
/// +36 / −26 m en z, sin tapar la entrada ni el portal (0, −14…−20).
/// </summary>
internal static class CivicBuilder
{
    internal enum Roof { None, Flat, Gable }

    internal sealed class Style
    {
        public Color wall, floor, roof; public Roof roofKind = Roof.Flat; public float wallH = 3.4f; public bool open; public bool columns;
        public Style(Color wall, Color floor, Color roof, Roof kind, float h = 3.4f) { this.wall = wall; this.floor = floor; this.roof = roof; roofKind = kind; wallH = h; }
    }

    // ── Estilos de cada era (colores de sus materiales reales) ──
    static Color C(float r, float g, float b) => new Color(r, g, b);
    static readonly Style TongLau = new Style(C(0.86f, 0.82f, 0.7f), C(0.55f, 0.5f, 0.45f), C(0.6f, 0.58f, 0.55f), Roof.Flat, 3.4f);          // Hong Kong: hormigón enlucido, azotea
    static readonly Style ChaChaanTeng = new Style(C(0.62f, 0.78f, 0.7f), C(0.85f, 0.85f, 0.8f), C(0.55f, 0.55f, 0.52f), Roof.Flat, 3.4f);  // azulejo verde claro y terrazo
    static readonly Style SeattleBrick = new Style(C(0.6f, 0.3f, 0.22f), C(0.45f, 0.32f, 0.22f), C(0.25f, 0.24f, 0.24f), Roof.Gable, 3.8f);
    static readonly Style Selimiye = new Style(C(0.92f, 0.84f, 0.6f), C(0.6f, 0.55f, 0.48f), C(0.7f, 0.35f, 0.25f), Roof.Gable, 4.2f);      // enlucido amarillo y teja
    static readonly Style FieldTimber = new Style(C(0.55f, 0.42f, 0.28f), C(0.45f, 0.38f, 0.3f), C(0.35f, 0.3f, 0.25f), Roof.Gable, 3f);
    static readonly Style DaytonBrick = new Style(C(0.62f, 0.32f, 0.24f), C(0.5f, 0.38f, 0.28f), C(0.3f, 0.28f, 0.27f), Roof.Flat, 6.5f);  // dos plantas, dos escaparates
    static readonly Style Clapboard = new Style(C(0.93f, 0.92f, 0.88f), C(0.5f, 0.4f, 0.3f), C(0.3f, 0.3f, 0.32f), Roof.Gable, 3.6f);     // casa de tablas blancas
    static readonly Style BurntBrick = new Style(C(0.66f, 0.38f, 0.26f), C(0.6f, 0.5f, 0.38f), C(0.55f, 0.45f, 0.3f), Roof.Flat, 3.2f);    // ladrillo cocido (Tilaurakot)
    static readonly Style Limestone = new Style(C(0.92f, 0.9f, 0.82f), C(0.8f, 0.76f, 0.66f), C(0.85f, 0.82f, 0.74f), Roof.Flat, 4.5f) { columns = true };
    static readonly Style MudThatch = new Style(C(0.62f, 0.5f, 0.36f), C(0.55f, 0.45f, 0.32f), C(0.75f, 0.65f, 0.38f), Roof.Gable, 2.8f);   // adobe y paja
    static readonly Style Pavilion = new Style(C(0.5f, 0.38f, 0.25f), C(0.7f, 0.6f, 0.45f), C(0.75f, 0.65f, 0.38f), Roof.Gable, 3f) { open = true };
    static readonly Style Sandbag = new Style(C(0.62f, 0.55f, 0.4f), C(0.33f, 0.27f, 0.2f), C(0.4f, 0.33f, 0.25f), Roof.Flat, 2.2f);       // refugio de trinchera

    /// <summary>Añade a la historia los edificios de su era (por su nombre de microcosmos). Devuelve cuántos insectos-humanos puso.</summary>
    internal static int Civilize(Transform t, string display)
    {
        switch (display)
        {
            case "Bruce Lee": return BruceLeeEra(t);
            case "Escútari": return Scutari(t);
            case "Dayton": return Dayton(t);
            case "Kapilavastu": return Kapilavastu(t);
            case "Alejandría": return Alexandria(t);
            case "Patanjali": return Ashram(t);
            case "Tregua 1914": return Trenches1914(t);
            default: return 0;
        }
    }

    // ══ BRUCE LEE: Hong Kong (años 50) y Seattle (1959–64) ════════════════════════════════════════════════════════
    // · La escuela de Yip Man: las clases eran en pisos y azoteas de edificios tong lau de Kowloon, en salas pequeñas con el
    //   muñeco de madera (HistoryLink 3999; Polly 2018). · Un cha chaan teng (casa de té de Hong Kong: barra, cocina vista,
    //   mesas con bancos). · El restaurante de Ruby Chow en Seattle, donde trabajó de camarero y vivió encima (HistoryLink 3999).
    static int BruceLeeEra(Transform t)
    {
        var school = Building(t, "Escuela de Yip Man (tong lau, Kowloon)", "Dojo", "Escuela de Wing Chun de Yip Man", new Vector3(-31f, 0f, 4f), 0f, new Vector2(12f, 10f), TongLau,
            S("tatami", 0f, 1f, 6f, 5f, "Tatami"), S("muneco", -4f, 3.5f, 0.5f, 0.5f, "Muñeco de madera"), S("muneco", 4f, 3.5f, 0.5f, 0.5f, "Muñeco de madera"));
        var cafe = Building(t, "Cha chaan teng (casa de té de Hong Kong)", "Restaurante", "Cha chaan teng", new Vector3(31f, 0f, 4f), 0f, new Vector2(14f, 10f), ChaChaanTeng,
            S("fogon", -4f, 3.5f, 3f, 1f, "Cocina vista (fogones)"), S("barra", 2f, 3f, 4f, 0.8f, "Barra"),
            S("mesa", -4f, -1.5f, 1.4f, 1.4f, "Mesa con bancos"), S("mesa", 0f, -1.5f, 1.4f, 1.4f, "Mesa con bancos"), S("mesa", 4f, -1.5f, 1.4f, 1.4f, "Mesa con bancos"));
        var ruby = Building(t, "Restaurante de Ruby Chow (Seattle)", "Restaurante", "Restaurante de Ruby Chow (Seattle, 1959)", new Vector3(0f, 0f, 38f), 0f, new Vector2(16f, 10f), SeattleBrick,
            S("fogon", -5f, 3.5f, 3f, 1f, "Cocina"), S("barra", 3f, 3.2f, 4f, 0.8f, "Barra"),
            S("mesa", -4f, -1.5f, 1.4f, 1.4f, "Mesa"), S("mesa", 0f, -1.5f, 1.4f, 1.4f, "Mesa"), S("mesa", 4f, -1.5f, 1.4f, 1.4f, "Mesa"));
        var home = Building(t, "Pisos de un tong lau", "Casa", "Casa (tong lau)", new Vector3(-31f, 0f, -26f), 0f, new Vector2(12f, 9f), TongLau,
            S("cama", -3.5f, 2.5f, 1.2f, 2f, "Cama"), S("cama", 3.5f, 2.5f, 1.2f, 2f, "Cama"), S("fogon", 0f, 3f, 1.4f, 0.8f, "Fogón"));
        int n = 0;
        n += Worker(t, "Alumno de Wing Chun 1", school, "alumno", cafe, school, home);
        n += Worker(t, "Alumno de Wing Chun 2", school, "alumno", cafe, school, home);
        n += Worker(t, "Wong Shun-leung", school, "maestro", cafe, school, home);
        n += Worker(t, "Cocinero del cha chaan teng", cafe, "cocinero", cafe, school, home);
        n += Worker(t, "Camarera del cha chaan teng", cafe, "camarero", cafe, school, home);
        n += Worker(t, "Ruby Chow", ruby, "camarero", ruby, null, home);
        n += Worker(t, "Cocinero de Ruby Chow", ruby, "cocinero", ruby, null, home);
        n += Couple(t, "Vecina del tong lau", "Vecino del tong lau", home, cafe, school);
        return n;
    }

    // ══ ESCÚTARI (1854–55): el cuartel de Selimiye (finales de la década de 1820): cuadrilátero con patio y torres en las
    //    esquinas, alas de tres plantas, salas-pasillo con camas a los dos lados; Nightingale vivía en una torre (hoy museo).
    //    La cocina de campaña de Alexis Soyer (1855) y la lavandería. (Wikipedia «Selimiye Barracks»; Royal Collection Trust:
    //    «The Military Hospital, Scutari» y «Miss Nightingale's Tower», J. Robertson 1855; Soyer 1857.)
    static int Scutari(Transform t)
    {
        var ward = Building(t, "Ala del cuartel de Selimiye (sala de camas)", "Hospital", "Hospital del cuartel de Selimiye", new Vector3(-33f, 0f, 8f), 0f, new Vector2(12f, 22f), Selimiye,
            S("cama", -4f, 7f, 1.2f, 2.2f, "Cama"), S("cama", 4f, 7f, 1.2f, 2.2f, "Cama"), S("cama", -4f, 2f, 1.2f, 2.2f, "Cama"), S("cama", 4f, 2f, 1.2f, 2.2f, "Cama"),
            S("cama", -4f, -3f, 1.2f, 2.2f, "Cama"), S("cama", 4f, -3f, 1.2f, 2.2f, "Cama"), S("mesa", 0f, 9.5f, 1.6f, 0.9f, "Mesa de los registros"),
            S("camilla", 0f, -7f, 1f, 2f, "Camilla de curas"), S("armario", 5f, -8.5f, 1.2f, 0.6f, "Armario de vendas"), S("lavadero", -5f, -8.5f, 1.4f, 0.8f, "Barreño"));
        Tower(t, "Torre de la esquina (la de Nightingale)", new Vector3(-33f, 0f, -8f), 6f, 13f, Selimiye);
        var kitchen = Building(t, "Cocina de campaña de Soyer (1855)", "Cocina", "Cocina de Alexis Soyer", new Vector3(32f, 0f, 8f), 0f, new Vector2(12f, 9f), FieldTimber,
            S("fogon", -3f, 2.5f, 1.4f, 1.4f, "Fogón de campaña de Soyer"), S("fogon", 1f, 2.5f, 1.4f, 1.4f, "Fogón de campaña de Soyer"), S("horno", 4.5f, 2.5f, 1.4f, 1.4f, "Horno"));
        var laundry = Building(t, "Lavandería del hospital", "Lavandería", "Lavandería (ropa de cama hervida)", new Vector3(32f, 0f, -24f), 0f, new Vector2(10f, 8f), FieldTimber,
            S("lavadero", -2.5f, 2f, 2f, 1f, "Caldera de lavar"), S("lavadero", 2.5f, 2f, 2f, 1f, "Pila"));
        var home = Building(t, "Barracones del personal", "Casa", "Barracones", new Vector3(0f, 0f, 38f), 0f, new Vector2(14f, 8f), Selimiye,
            S("cama", -4f, 2f, 1.2f, 2f, "Catre"), S("cama", 0f, 2f, 1.2f, 2f, "Catre"), S("cama", 4f, 2f, 1.2f, 2f, "Catre"), S("fogon", 5.5f, -2f, 1f, 0.8f, "Estufa"));
        int n = 0;
        n += Worker(t, "Enfermera Mary Clarke", ward, "enfermera", kitchen, null, home);
        n += Worker(t, "Enfermera de Escútari", ward, "enfermera", kitchen, null, home);
        n += Worker(t, "Cirujano del ejército", ward, "medico", kitchen, null, home);
        n += Worker(t, "Ayudante de Soyer", kitchen, "cocinero", kitchen, null, home);
        n += Worker(t, "Lavandera de Escútari", laundry, "", kitchen, null, home);
        n += Worker(t, "Lavandera de la ribera", laundry, "", kitchen, null, home);
        n += Couple(t, "Esposa del sargento", "Sargento convaleciente", home, kitchen, null);
        return n;
    }

    // ══ DAYTON (1892–1903): la Wright Cycle Company (1127 W. Third St.), edificio de ladrillo de DOS plantas y dos escaparates
    //    (en Greenfield Village desde 1937): tienda delante, taller al fondo (torno, motor), allí el túnel de viento de 1901. La casa
    //    de tablas de 7 Hawthorn St. y el cobertizo de Kitty Hawk (1902). (The Henry Ford, «Wright Cycle Shop… circa 1910»; NPS,
    //    Wright Cycle Company Historic Furnishings Report.)
    static int Dayton(Transform t)
    {
        var shop = Building(t, "Wright Cycle Company (1127 West Third Street)", "Taller", "Wright Cycle Company", new Vector3(-31f, 0f, 4f), 0f, new Vector2(9f, 18f), DaytonBrick,
            S("banco_taller", -2.5f, 5f, 2.4f, 0.9f, "Banco de reparar bicicletas"), S("banco_taller", 2.5f, 5f, 2.4f, 0.9f, "Banco de montar"),
            S("torno", 2.5f, 1f, 1.6f, 0.8f, "Torno"), S("tunel", -2f, 0f, 0.6f, 1.8f, "Túnel de viento (1901)"), S("mesa", 0f, -6f, 3f, 0.8f, "Mostrador de la tienda"));
        Upper(t, shop.transform, new Vector2(9f, 18f), DaytonBrick, "Imprenta (planta alta)");
        var home = Building(t, "Casa de los Wright (7 Hawthorn Street)", "Casa", "Casa de los Wright", new Vector3(31f, 0f, 4f), 0f, new Vector2(10f, 10f), Clapboard,
            S("cama", -3f, 3f, 1.2f, 2f, "Cama"), S("cama", 3f, 3f, 1.2f, 2f, "Cama"), S("fogon", 0f, -3.5f, 1.4f, 0.8f, "Cocina de leña"));
        var shed = Building(t, "Cobertizo de Kitty Hawk (1902)", "Taller", "Campamento de Kill Devil Hills", new Vector3(31f, 0f, -26f), 0f, new Vector2(14f, 8f), FieldTimber,
            S("banco_taller", -4f, 2f, 2.4f, 0.9f, "Banco del planeador"), S("banco_taller", 3f, 2f, 2.4f, 0.9f, "Banco del motor"));
        int n = 0;
        n += Worker(t, "Katharine Wright", home, "", home, null, home);
        n += Worker(t, "Aprendiz del taller", shop, "mecanico", home, null, home);
        n += Worker(t, "Mecánico de Dayton", shop, "mecanico", home, null, home);
        n += Worker(t, "Salvavidas de Kill Devil Hills", shed, "mecanico", home, null, home);
        n += Couple(t, "Vecina de West Third Street", "Vecino de West Third Street", home, home, null);
        return n;
    }

    // ══ KAPILAVASTU (s. V a. C.): Tilaurakot, ciudad en cuadrícula con casas y tiendas de LADRILLO, puertas a los puntos
    //    cardinales, muralla de ladrillo cocido sobre una empalizada de madera, estanques revestidos de ladrillo (Durham Univ.,
    //    proyecto Tilaurakot-Kapilavastu desde 2014; Wikipedia «Tilaurakot»).
    static int Kapilavastu(Transform t)
    {
        Wall(t, "Muralla de ladrillo cocido (puerta norte)", new Vector3(0f, 0f, 46f), 40f, 4f, BurntBrick, gate: 5f);
        var h1 = Building(t, "Casa de ladrillo (calle este-oeste)", "Casa", "Casa de Kapilavastu", new Vector3(-31f, 0f, 0f), 0f, new Vector2(10f, 8f), BurntBrick,
            S("cama", -2.5f, 2f, 1.2f, 2f, "Lecho"), S("fogon", 3f, 2.5f, 1.2f, 0.8f, "Hogar de barro"));
        var h2 = Building(t, "Casa de ladrillo (calle norte-sur)", "Casa", "Casa de Kapilavastu", new Vector3(-31f, 0f, 16f), 0f, new Vector2(10f, 8f), BurntBrick,
            S("cama", -2.5f, 2f, 1.2f, 2f, "Lecho"), S("fogon", 3f, 2.5f, 1.2f, 0.8f, "Hogar de barro"));
        var market = Building(t, "Calle de tiendas", "Mercado", "Mercado de Kapilavastu", new Vector3(31f, 0f, 4f), 0f, new Vector2(14f, 8f), BurntBrick,
            S("puesto", -4.5f, 0f, 2f, 1.2f, "Puesto de grano"), S("puesto", 0f, 0f, 2f, 1.2f, "Puesto de telas"), S("puesto", 4.5f, 0f, 2f, 1.2f, "Puesto de cerámica"));
        var garden = Building(t, "Huerto junto al estanque", "Huerto", "Huerto y estanque revestido de ladrillo", new Vector3(31f, 0f, -26f), 0f, new Vector2(14f, 10f), Pavilion,
            S("bancal", -4f, 1f, 3f, 5f, "Bancal de arroz"), S("bancal", 0.5f, 1f, 3f, 5f, "Bancal de legumbres"), S("lavadero", 5f, 0f, 2.5f, 6f, "Estanque de ladrillo"));
        int n = 0;
        n += Worker(t, "Alfarero de Kapilavastu", market, "comerciante", h1, null, h1);
        n += Worker(t, "Tejedora de Kapilavastu", market, "comerciante", h2, null, h2);
        n += Worker(t, "Campesino del Terai", garden, "hortelano", h1, null, h1);
        n += Couple(t, "Madre de Kapilavastu", "Padre de Kapilavastu", h2, h2, null);
        return n;
    }

    // ══ ALEJANDRÍA (~240 a. C.): el Museion y la Biblioteca (salas con columnas y armarios de rollos: sin planos; según
    //    Estrabón XVII.1.8, el Museion tenía paseo, exedra con asientos y una gran sala común para comer los sabios) y una casa
    //    griega con patio.
    static int Alexandria(Transform t)
    {
        var mus = Building(t, "Museion (sala de los rollos)", "Escuela", "Museion de Alejandría", new Vector3(-32f, 0f, 4f), 0f, new Vector2(16f, 12f), Limestone,
            S("estante", -6f, 4.8f, 3f, 0.8f, "Armario de rollos"), S("estante", -1.5f, 4.8f, 3f, 0.8f, "Armario de rollos"), S("estante", 3f, 4.8f, 3f, 0.8f, "Armario de rollos"),
            S("pizarra", 6.5f, 2f, 0.4f, 2f, "Tablilla de cera"), S("mesa", 0f, -1f, 3f, 1.2f, "Mesa de copia"));
        var dining = Building(t, "Sala común del Museion", "Restaurante", "Comedor de los sabios (Museion)", new Vector3(0f, 0f, 38f), 0f, new Vector2(14f, 9f), Limestone,
            S("fogon", -5f, 3f, 1.6f, 1f, "Hogar"), S("barra", 0f, 3.2f, 3f, 0.8f, "Aparador"), S("mesa", -3f, -1.5f, 2f, 1.2f, "Lecho de banquete"), S("mesa", 3f, -1.5f, 2f, 1.2f, "Lecho de banquete"));
        var home = Building(t, "Casa griega con patio", "Casa", "Casa de Alejandría", new Vector3(32f, 0f, 4f), 0f, new Vector2(12f, 12f), Limestone,
            S("cama", -4f, 4f, 1.2f, 2f, "Lecho"), S("fogon", 4f, 4f, 1.4f, 0.8f, "Hogar"), S("altar", 0f, 0f, 0.8f, 0.8f, "Altar del patio"));
        int n = 0;
        n += Worker(t, "Bibliotecario del Museion", mus, "sabio", dining, null, home);
        n += Worker(t, "Copista del Museion", mus, "sabio", dining, null, home);
        n += Worker(t, "Cocinero del Museion", dining, "cocinero", dining, null, home);
        n += Couple(t, "Madre alejandrina", "Padre alejandrino", home, dining, null);
        return n;
    }

    // ══ PATANJALI (s. II a. C.): el āśrama junto al río: cabañas de adobe y paja, el patio de práctica bajo cubierta, la cocina de
    //    leña y el huerto (Olivelle 1993, The Āśrama System; cuevas de meditación como las de Barabar, s. III a. C.).
    static int Ashram(Transform t)
    {
        var hut = Building(t, "Cabaña del āśrama", "Casa", "Cabaña del āśrama", new Vector3(-32f, 0f, 2f), 0f, new Vector2(9f, 7f), MudThatch,
            S("cama", -2f, 1.5f, 1.2f, 2f, "Estera de dormir"), S("cama", 2f, 1.5f, 1.2f, 2f, "Estera de dormir"));
        var yoga = Building(t, "Patio de práctica (pabellón)", "Sala de Yoga", "Patio de práctica del āśrama", new Vector3(32f, 0f, 4f), 0f, new Vector2(14f, 10f), Pavilion,
            S("esterilla", -4f, 2f, 1f, 2f, "Esterilla"), S("esterilla", 0f, 2f, 1f, 2f, "Esterilla"), S("esterilla", 4f, 2f, 1f, 2f, "Esterilla"), S("esterilla", 0f, -2.5f, 1f, 2f, "Esterilla del maestro"));
        var kitchen = Building(t, "Cocina de leña", "Cocina", "Cocina del āśrama", new Vector3(32f, 0f, -26f), 0f, new Vector2(9f, 7f), MudThatch,
            S("fogon", 0f, 1.5f, 1.6f, 1.2f, "Fogón de leña (chulha)"));
        var garden = Building(t, "Huerto del āśrama", "Huerto", "Huerto del āśrama", new Vector3(0f, 0f, 38f), 0f, new Vector2(16f, 9f), Pavilion,
            S("bancal", -5f, 0f, 3f, 5f, "Bancal"), S("bancal", 0f, 0f, 3f, 5f, "Bancal"), S("bancal", 5f, 0f, 3f, 5f, "Bancal"));
        int n = 0;
        n += Worker(t, "Discípula del āśrama", yoga, "alumno", kitchen, yoga, hut);
        n += Worker(t, "Discípulo del āśrama", garden, "hortelano", kitchen, yoga, hut);
        n += Worker(t, "Cocinera del āśrama", kitchen, "cocinero", kitchen, yoga, hut);
        n += Couple(t, "Madre del pueblo", "Padre del pueblo", hut, kitchen, yoga);
        return n;
    }

    // ══ TREGUA DE 1914: en el frente no hay edificios; hay refugios (dugouts) de tablas y sacos y la cocina de campaña
    //    (Imperial War Museums, «The Christmas Truce»).
    static int Trenches1914(Transform t)
    {
        var bri = Building(t, "Refugio británico (dugout)", "Trinchera", "Refugio británico", new Vector3(-30f, 0f, -12f), 0f, new Vector2(10f, 6f), Sandbag,
            S("parapeto", 0f, -2.2f, 6f, 0.6f, "Aspillera"), S("cama", -3f, 1.5f, 1.2f, 2f, "Litera"), S("fogon", 3f, 1.5f, 1f, 0.8f, "Brasero"));
        var ger = Building(t, "Refugio alemán (Unterstand)", "Trinchera", "Refugio alemán", new Vector3(-30f, 0f, 34f), 180f, new Vector2(10f, 6f), Sandbag,
            S("parapeto", 0f, -2.2f, 6f, 0.6f, "Aspillera"), S("cama", -3f, 1.5f, 1.2f, 2f, "Litera"), S("fogon", 3f, 1.5f, 1f, 0.8f, "Estufa"));
        var cook = Building(t, "Cocina de campaña", "Trinchera", "Cocina de campaña británica", new Vector3(30f, 0f, -12f), 0f, new Vector2(8f, 6f), FieldTimber,
            S("fogon", 0f, 1f, 2f, 1.2f, "Marmita de campaña"), S("cama", -2.5f, -1.2f, 1.2f, 2f, "Catre"));
        int n = 0;
        n += Worker(t, "Cocinero del batallón", cook, "", cook, null, bri, "Ant", new Color(0.55f, 0.22f, 0.12f));
        n += Worker(t, "Centinela británico", bri, "soldado", cook, null, bri, "Ant", new Color(0.55f, 0.22f, 0.12f));
        n += Worker(t, "Centinela alemán", ger, "soldado", ger, null, ger, "Ant", new Color(0.12f, 0.11f, 0.1f));
        return n;
    }

    // ── Piezas ──────────────────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Un puesto: clave del registro, posición local (x, z) dentro del edificio, tamaño (ancho, fondo) y rótulo.</summary>
    static (string key, Vector2 at, Vector2 size, string label) S(string key, float x, float z, float w, float d, string label) => (key, new Vector2(x, z), new Vector2(w, d), label);

    /// <summary>Un EDIFICIO: suelo, cuatro muros con la puerta en el frente (−z local, 2,4 m), cubierta (sin colisión: no tapa la
    /// navegación) y sus puestos interactuables. Es un <see cref="AreaFunctionSite"/> de la plantilla <paramref name="template"/>.</summary>
    internal static AreaFunctionSite Building(Transform parent, string name, string template, string title, Vector3 pos, float yaw, Vector2 size, Style st,
                                              params (string key, Vector2 at, Vector2 size, string label)[] stations)
    {
        Transform b = new GameObject(name).transform; b.SetParent(parent, false); b.localPosition = pos; b.localRotation = Quaternion.Euler(0f, yaw, 0f);
        Material wall = AreaMicrocosmos.Mat("Civ_Muro_" + Hex(st.wall), st.wall), floor = AreaMicrocosmos.Mat("Civ_Suelo_" + Hex(st.floor), st.floor), roof = AreaMicrocosmos.Mat("Civ_Cubierta_" + Hex(st.roof), st.roof);
        float w = size.x, d = size.y, h = st.wallH, th = 0.3f, door = 2.4f;
        AreaMicrocosmos.Box(b, "Suelo", new Vector3(0f, 0.04f, 0f), new Vector3(w, 0.08f, d), floor, false);
        if (st.open)
        {
            foreach (var (x, z) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) AreaMicrocosmos.Box(b, "Poste", new Vector3(x * (w * 0.5f - 0.2f), h * 0.5f, z * (d * 0.5f - 0.2f)), new Vector3(0.3f, h, 0.3f), wall, true);
        }
        else
        {
            AreaMicrocosmos.Box(b, "Muro_fondo", new Vector3(0f, h * 0.5f, d * 0.5f), new Vector3(w, h, th), wall, true);
            AreaMicrocosmos.Box(b, "Muro_izquierdo", new Vector3(-w * 0.5f, h * 0.5f, 0f), new Vector3(th, h, d), wall, true);
            AreaMicrocosmos.Box(b, "Muro_derecho", new Vector3(w * 0.5f, h * 0.5f, 0f), new Vector3(th, h, d), wall, true);
            float side = (w - door) * 0.5f;
            AreaMicrocosmos.Box(b, "Muro_frente", new Vector3(-(door + side) * 0.5f, h * 0.5f, -d * 0.5f), new Vector3(side, h, th), wall, true);
            AreaMicrocosmos.Box(b, "Muro_frente", new Vector3((door + side) * 0.5f, h * 0.5f, -d * 0.5f), new Vector3(side, h, th), wall, true);
            if (h > 2.7f) AreaMicrocosmos.Box(b, "Dintel", new Vector3(0f, (h + 2.6f) * 0.5f, -d * 0.5f), new Vector3(door, h - 2.6f, th), wall, false);
            // Ventanas pintadas a los lados (un poco más oscuras que el muro).
            Material win = AreaMicrocosmos.Mat("Civ_Ventana", new Color(0.25f, 0.3f, 0.35f));
            for (int k = -1; k <= 1; k += 2) AreaMicrocosmos.Box(b, "Ventana", new Vector3(k * (w * 0.5f + 0.17f), h * 0.6f, 0f), new Vector3(0.04f, 1.1f, Mathf.Min(1.6f, d * 0.3f)), win, false);
        }
        if (st.columns) for (int k = 0; k < 6; k++) AreaMicrocosmos.Box(b, "Columna", new Vector3(-w * 0.5f + 0.4f + k * (w - 0.8f) / 5f, h * 0.5f, -d * 0.5f - 1.2f), new Vector3(0.5f, h, 0.5f), wall, true);
        if (st.roofKind == Roof.Flat) AreaMicrocosmos.Box(b, "Cubierta", new Vector3(0f, h + 0.1f, 0f), new Vector3(w + 0.4f, 0.2f, d + 0.4f), roof, false);
        else if (st.roofKind == Roof.Gable)
        {
            float rise = Mathf.Min(2.4f, d * 0.3f), half = d * 0.5f + 0.3f, ang = Mathf.Atan2(rise, half) * Mathf.Rad2Deg, len = Mathf.Sqrt(rise * rise + half * half);
            AreaMicrocosmos.Box(b, "Faldon", new Vector3(0f, h + rise * 0.5f, -half * 0.5f), new Vector3(w + 0.6f, 0.18f, len), Quaternion.Euler(-ang, 0f, 0f), roof, false);
            AreaMicrocosmos.Box(b, "Faldon", new Vector3(0f, h + rise * 0.5f, half * 0.5f), new Vector3(w + 0.6f, 0.18f, len), Quaternion.Euler(ang, 0f, 0f), roof, false);
        }
        var site = b.gameObject.AddComponent<AreaFunctionSite>(); site.building = template; site.title = title;
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = title; wl.height = h + 2.5f;
        Material furn = AreaMicrocosmos.Mat("Civ_Mueble", new Color(0.5f, 0.38f, 0.26f));
        foreach (var s in stations)
        {
            float sh = StationHeight(s.key);
            Transform p = AreaMicrocosmos.Box(b, "Puesto_" + s.key, new Vector3(s.at.x, sh * 0.5f + 0.08f, s.at.y), new Vector3(s.size.x, sh, s.size.y), StationMat(s.key, furn), true);
            var fs = p.gameObject.AddComponent<AreaFunctionStation>(); fs.station = s.key;
            var l = p.gameObject.AddComponent<WorldLabel>(); l.text = s.label; l.height = 1.2f;
        }
        return site;
    }

    static float StationHeight(string key)
    {
        switch (key)
        {
            case "tatami": case "esterilla": case "bancal": return 0.1f;
            case "cama": case "camilla": return 0.55f;
            case "muneco": return 1.7f;
            case "estante": case "armario": return 2.2f;
            case "pizarra": case "tunel": return 1.8f;
            case "parapeto": return 1.3f;
            default: return 0.9f;   // fogón, barra, mesa, banco, puesto, lavadero, altar…
        }
    }

    static Material StationMat(string key, Material furn)
    {
        switch (key)
        {
            case "tatami": return AreaMicrocosmos.Mat("Civ_Tatami", new Color(0.78f, 0.72f, 0.5f));
            case "esterilla": return AreaMicrocosmos.Mat("Civ_Esterilla", new Color(0.6f, 0.45f, 0.25f));
            case "bancal": return AreaMicrocosmos.Mat("Civ_Bancal", new Color(0.3f, 0.45f, 0.2f));
            case "cama": case "camilla": return AreaMicrocosmos.Mat("Civ_Sabana", new Color(0.93f, 0.92f, 0.88f));
            case "fogon": case "horno": return AreaMicrocosmos.Mat("Civ_Fogon", new Color(0.3f, 0.28f, 0.26f));
            case "lavadero": return AreaMicrocosmos.Mat("Civ_Agua", new Color(0.35f, 0.55f, 0.7f));
            case "parapeto": return AreaMicrocosmos.Mat("Civ_Sacos", new Color(0.62f, 0.55f, 0.4f));
            default: return furn;
        }
    }

    // La planta alta (la imprenta de los Wright): una losa y un rótulo, sin entrar.
    static void Upper(Transform t, Transform b, Vector2 size, Style st, string label)
    {
        Transform u = new GameObject(label).transform; u.SetParent(b, false); u.localPosition = new Vector3(0f, st.wallH * 0.55f, 0f);
        var wl = u.gameObject.AddComponent<WorldLabel>(); wl.text = label; wl.height = 1f;
    }

    // Una torre de esquina (Selimiye): cuatro muros altos y una cubierta en pirámide (dos losas cruzadas).
    static void Tower(Transform t, string name, Vector3 pos, float side, float h, Style st)
    {
        Transform b = new GameObject(name).transform; b.SetParent(t, false); b.localPosition = pos;
        Material wall = AreaMicrocosmos.Mat("Civ_Muro_" + Hex(st.wall), st.wall), roof = AreaMicrocosmos.Mat("Civ_Cubierta_" + Hex(st.roof), st.roof);
        AreaMicrocosmos.Box(b, "Fuste", new Vector3(0f, h * 0.5f, 0f), new Vector3(side, h, side), wall, true);
        for (int k = 0; k < 3; k++) AreaMicrocosmos.Box(b, "Chapitel", new Vector3(0f, h + 0.6f + k * 0.8f, 0f), new Vector3(side * (1f - k * 0.3f), 0.8f, side * (1f - k * 0.3f)), roof, false);
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = h + 4f;
    }

    // Un tramo de muralla con su puerta (Tilaurakot).
    static void Wall(Transform t, string name, Vector3 pos, float len, float h, Style st, float gate)
    {
        Transform b = new GameObject(name).transform; b.SetParent(t, false); b.localPosition = pos;
        Material wall = AreaMicrocosmos.Mat("Civ_Muro_" + Hex(st.wall), st.wall);
        float side = (len - gate) * 0.5f;
        AreaMicrocosmos.Box(b, "Muralla", new Vector3(-(gate + side) * 0.5f, h * 0.5f, 0f), new Vector3(side, h, 1.6f), wall, true);
        AreaMicrocosmos.Box(b, "Muralla", new Vector3((gate + side) * 0.5f, h * 0.5f, 0f), new Vector3(side, h, 1.6f), wall, true);
        foreach (float x in new[] { -gate * 0.5f - 1f, gate * 0.5f + 1f }) AreaMicrocosmos.Box(b, "Torre_de_la_puerta", new Vector3(x, h * 0.65f, 0f), new Vector3(2f, h * 1.3f, 2.4f), wall, true);
        var wl = b.gameObject.AddComponent<WorldLabel>(); wl.text = name; wl.height = h + 2f;
    }

    // Un insecto-humano de la era con su jornada (trabajo, comida, práctica, casa).
    static int Worker(Transform t, string name, AreaFunctionSite work, string role, AreaFunctionSite dine, AreaFunctionSite practice, AreaFunctionSite home, string species = null, Color? body = null)
    {
        Vector3 at = t.InverseTransformPoint(work.transform.TransformPoint(new Vector3(0f, 0f, -2f)));
        GameObject g = AreaMicrocosmos.Folk(t, name, at, Random.Range(1.6f, 1.85f), 1f, species, body);
        CubeInsect.WearShirt(g, Color.HSVToRGB(Mathf.Abs(name.GetHashCode() % 100) / 100f, 0.35f, 0.75f));
        var j = g.AddComponent<MicroJornada>(); j.work = work; j.role = role; j.dine = dine; j.practice = practice; j.home = home;
        return 1;
    }

    // Una PAREJA de especies distintas (con abuelos de otras especies), que tendrá hijos: el árbol que se mezcla.
    static int Couple(Transform t, string she, string he, AreaFunctionSite home, AreaFunctionSite dine, AreaFunctionSite practice)
    {
        Worker(t, she, home, "", dine, practice, home); Worker(t, he, home, "", dine, practice, home);
        FolkLineage a = null, b = null;
        foreach (FolkLineage l in t.GetComponentsInChildren<FolkLineage>(true)) { if (l.name == she) a = l; if (l.name == he) b = l; }
        if (a == null || b == null) return 2;
        var rnd = new System.Random(she.GetHashCode());
        string R() => InsectCast.Folk[rnd.Next(InsectCast.Folk.Length)];
        foreach (FolkLineage l in new[] { a, b })
        {
            l.ancestors = new List<InsectGenealogy.Ancestor> { new InsectGenealogy.Ancestor("madre", R(), 1), new InsectGenealogy.Ancestor("padre", R(), 1),
                new InsectGenealogy.Ancestor("abuela", R(), 2), new InsectGenealogy.Ancestor("abuelo", R(), 2), new InsectGenealogy.Ancestor("bisabuelo", R(), 3) };
        }
        a.female = true; b.female = false; a.partner = b; b.partner = a;
        a.gameObject.AddComponent<FolkCouple>();
        return 2;
    }

    static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
}
