using System.Collections.Generic;
using System.Linq;

/// <summary>
/// LAS MARCAS DE CADA PISO Y LA REORGANIZACIÓN POR CÁLCULO (fase F0 del rediseño de pisos, PB-170; docs/areas/rediseno-pisos-arboles.md;
/// portado de CodeShip `organizacion.ts`, regla 2.13. Berón, en CodeShip, 2026-11-24: «debemos tener reglas que permitan este tipo de
/// cambios que reorganizan a los edificios y pisos, quizás por cálculos? Los pisos deberían tener marcas que nos digan si eran mixtos o
/// puros, los edificios-árbol nos deberían decir a qué categoría pertenecen y su cantidad de pisos»). Datos puros, sin escena.
///
/// MARCAS: cada piso lleva sus COMPONENTES (lo que lo comparte), cada uno con su categoría y su CUOTA (la parte del piso de 40 × 40 m
/// que necesita: 1 = el piso entero). Un piso es PURO con un componente y MIXTO con varios. La categoría de un edificio-árbol es la
/// que más cuota suma en sus pisos. Los componentes de entrenamiento llevan además sus PLAZAS (J16 reescrita: la suma llega a todos).
///
/// LAS REGLAS (<see cref="Review"/>) leen las marcas y dicen qué debe cambiar; <see cref="Reorganize"/> las aplica. Nunca se reorganiza a mano:
///  1. DÓNDE CABE un componente (<see cref="Place"/>): cuota ≤ 1, un piso; ≤ 1 + <see cref="RingShare"/>, un piso con su anillo (como el
///     restaurante); ≤ <see cref="CrownShare"/>, la CIMA (piso gigante en la copa); ≤ cima + 2,5 anillos, la cima con anillo; más, se reparte.
///  2. PISO MIXTO DESBORDADO: si un componente de un mixto pide el piso entero o más, los demás buscan un piso con sitio (primero uno de
///     su categoría; si no, el más vacío), para que ninguno se quede en un piso a medias.
///  3. EDIFICIO SOLO CON CIMA: el que se queda solo con su cima se la cede al edificio con menos pisos que no tenga cima, y desaparece
///     (un árbol menos, viajes más cortos).
///  4. DESEQUILIBRIO: un edificio con menos de la mitad de pisos que el más lleno se señala (para fusionar o mover pisos).
/// <see cref="Proposed"/> son los árboles para los miembros de hoy (§4 del plan); lo que construye la fase F1 es
/// <c>Reorganize(Proposed())</c>.
/// </summary>
public static class FloorOrganization
{
    public enum Category { Servicios, Entrenamiento, Ciencias, Produccion, Campos, Vivienda }

    /// <summary>Medida de un piso (el de la cocina): 40 × 40 m (CodeShip regla 2.9).</summary>
    public const float FloorSide = 40f;
    /// <summary>Lo que añade el anillo de un piso (~1 430 m² frente a 1 600 de un piso de 40 m).</summary>
    public const float RingShare = 0.9f;
    /// <summary>La cima: un piso gigante de hasta 120 × 120 m sobre un árbol de 40 m (9 pisos).</summary>
    public const float CrownShare = 9f;
    /// <summary>La cima de la Residencia del plan: 110 × 110 m (castillos, ring, aula, lectura, cocina y jardín).</summary>
    public const float ResidenceCrownSide = 110f;

    public struct Component
    {
        public string id, name; public Category category; public float quota;
        /// <summary>Plazas de entrenamiento a la vez (0 si no es un centro de entrenamiento).</summary>
        public int seats;
        public Component(string id, string name, Category c, float quota, int seats = 0) { this.id = id; this.name = name; category = c; this.quota = quota; this.seats = seats; }
        public override string ToString() => $"{name} ({category}, {quota:0.##})";
    }

    public class Floor
    {
        public string id; public List<Component> components = new List<Component>();
        public Floor(string id, params Component[] c) { this.id = id; components.AddRange(c); }
        public bool Mixed => components.Count > 1;
        public float Occupancy => components.Sum(c => c.quota);
    }

    public class Building
    {
        public string id, name; public List<Floor> floors = new List<Floor>(); public Floor crown;
        public Building(string id, string name, params Floor[] f) { this.id = id; this.name = name; floors.AddRange(f); }
        public Building Copy() => new Building(id, name, floors.ToArray()) { crown = crown };
        public IEnumerable<Floor> All => crown != null ? floors.Append(crown) : floors;
    }

    /// <summary>La categoría de un edificio: la que más cuota suma en sus pisos (y su cima).</summary>
    public static Category? CategoryOf(Building b)
    {
        var by = new Dictionary<Category, float>();
        foreach (Floor f in b.All) foreach (Component c in f.components) { by.TryGetValue(c.category, out float v); by[c.category] = v + c.quota; }
        if (by.Count == 0) return null;
        return by.OrderByDescending(kv => kv.Value).First().Key;
    }

    public enum Where { Floor, FloorRing, Crown, CrownRing, Split }
    public struct Placement { public Where where; public int floors; public override string ToString() => where == Where.Split ? $"repartir en {floors} pisos" : where.ToString(); }

    /// <summary>Regla 1: dónde cabe un componente con esa cuota.</summary>
    public static Placement Place(float quota)
    {
        if (quota <= 1f) return new Placement { where = Where.Floor, floors = 1 };
        if (quota <= 1f + RingShare) return new Placement { where = Where.FloorRing, floors = 1 };
        if (quota <= CrownShare) return new Placement { where = Where.Crown };
        if (quota <= CrownShare + RingShare * 2.5f) return new Placement { where = Where.CrownRing };
        return new Placement { where = Where.Split, floors = (int)System.Math.Ceiling(quota / (1f + RingShare)) };
    }

    public enum Rule { Crown, Overflow, OnlyCrown, Imbalance }
    public struct Finding
    {
        public Rule rule; public string building, floor, component, grows, to; public Placement placement; public int floors, max;
        public override string ToString() => rule switch
        {
            Rule.Crown => $"«{component}» no cabe en un piso de {building}: {placement}",
            Rule.Overflow => $"piso mixto «{floor}» desbordado por «{grows}»: «{component}» se va a {to}",
            Rule.OnlyCrown => $"{building} se queda solo con su cima: pasa a {to}",
            _ => $"{building} tiene {floors} pisos y el más lleno {max}: desequilibrado",
        };
    }

    /// <summary>Todas las reglas sobre unos edificios: lo que encuentran.</summary>
    public static List<Finding> Review(IList<Building> buildings)
    {
        var o = new List<Finding>();
        var all = buildings.SelectMany(q => q.floors).ToList();
        foreach (Building b in buildings)
            foreach (Floor f in b.floors)
            {
                foreach (Component c in f.components)
                {
                    Placement p = Place(c.quota);
                    if (p.where != Where.Floor && p.where != Where.FloorRing) o.Add(new Finding { rule = Rule.Crown, building = b.id, floor = f.id, component = c.id, placement = p });
                }
                if (f.Mixed && f.Occupancy > 1f)
                {
                    Component grows = f.components.OrderByDescending(c => c.quota).First();
                    foreach (Component c in f.components)
                    {
                        if (c.id == grows.id) continue;
                        Floor dest = all.Where(q => q != f && q.Occupancy + c.quota <= 1f)
                                        .OrderByDescending(q => q.components.Any(k => k.category == c.category)).ThenBy(q => q.Occupancy).FirstOrDefault();
                        o.Add(new Finding { rule = Rule.Overflow, building = b.id, floor = f.id, component = c.id, grows = grows.id, to = dest != null ? dest.id : "piso nuevo" });
                    }
                }
            }
        foreach (Building b in buildings)
            if (b.crown != null && b.floors.Count == 0)
            {
                Building to = buildings.Where(q => q != b && q.crown == null && q.floors.Count > 0).OrderBy(q => q.floors.Count).FirstOrDefault();
                if (to != null) o.Add(new Finding { rule = Rule.OnlyCrown, building = b.id, to = to.id });
            }
        int max = buildings.Count == 0 ? 0 : buildings.Max(q => q.floors.Count);
        foreach (Building b in buildings)
            if (b.floors.Count > 0 && b.floors.Count < max / 2f) o.Add(new Finding { rule = Rule.Imbalance, building = b.id, floors = b.floors.Count, max = max });
        return o;
    }

    /// <summary>Aplica las reglas hasta que nada se mueve: lo que no cabe en un piso sube a la cima de su árbol con los pisos de su
    /// categoría; un árbol que se queda solo con su cima la cede y desaparece. Devuelve los edificios nuevos y lo hecho, en orden.</summary>
    public static List<Building> Reorganize(IList<Building> buildings, out List<string> done)
    {
        var bs = buildings.Select(q => q.Copy()).ToList();
        done = new List<string>();
        for (int guard = 0; guard < 20; guard++)
        {
            var h = Review(bs);
            Finding up = h.FirstOrDefault(x => x.rule == Rule.Crown && (x.placement.where == Where.Crown || x.placement.where == Where.CrownRing) && bs.First(q => q.id == x.building).crown == null);
            if (up.building != null)
            {
                Building b = bs.First(q => q.id == up.building);
                Component comp = b.floors.SelectMany(f => f.components).First(c => c.id == up.component);
                var same = b.floors.Where(f => f.components.All(c => c.category == comp.category)).ToList();
                b.crown = new Floor(comp.id + "-cima", comp);
                b.floors.RemoveAll(same.Contains);
                done.Add($"«{comp.name}» no cabe en un piso (cuota {comp.quota:0.0}): pasa a la cima de {b.name}{(up.placement.where == Where.CrownRing ? " con anillo" : "")}, con {same.Count} piso(s) de su categoría");
                continue;
            }
            Finding only = h.FirstOrDefault(x => x.rule == Rule.OnlyCrown);
            if (only.building != null)
            {
                Building from = bs.First(q => q.id == only.building), to = bs.First(q => q.id == only.to);
                done.Add($"{from.name} se queda solo con su cima: la cima pasa a {to.name}, el edificio con menos pisos, y {from.name} desaparece");
                to.crown = from.crown; bs.Remove(from);
                continue;
            }
            break;
        }
        return bs;
    }

    /// <summary>Las plazas de entrenamiento de unos edificios (J16 reescrita: su suma debe llegar a los miembros).</summary>
    public static int TrainingSeats(IEnumerable<Building> buildings) => buildings.SelectMany(b => b.All).SelectMany(f => f.components).Sum(c => c.seats);

    static Component C(string id, string name, Category c, float q, int seats = 0) => new Component(id, name, c, q, seats);
    static Floor Pure(string id, string name, Category c, int seats = 0) => new Floor(id, C(id, name, c, 1f, seats));

    /// <summary>LOS ÁRBOLES PROPUESTOS para los miembros de hoy (§4 del plan, D1 de Berón 2026-11-25: un piso por actividad y por
    /// afinidad), con sus marcas, ANTES de aplicar las reglas: la Residencia aún es su propio árbol. Plazas de entrenamiento: gimnasio
    /// 20 (10 máquinas × 2, F1b), funcional 40, combate 25, cada sala 8.</summary>
    public static List<Building> Proposed()
    {
        const Category S = Category.Servicios, E = Category.Entrenamiento, Ci = Category.Ciencias, P = Category.Produccion, Ca = Category.Campos, V = Category.Vivienda;
        float residence = ResidenceCrownSide * ResidenceCrownSide / (FloorSide * FloorSide);   // ≈ 7,6 pisos: no cabe en un piso
        var l = new List<Building>
        {
            new Building("servicios", "Servicios",
                Pure("restaurante", "Restaurante", S), Pure("hospital", "Hospital", S), Pure("veterinario", "Hospital veterinario", S),
                new Floor("cria-carne-cultivada", C("cria", "Cría", S, 0.6f), C("carne-cultivada", "Carne cultivada", Ci, 0.4f)),
                new Floor("asambleas-guardia", C("asambleas", "Asambleas", S, 0.75f), C("guardia", "Sala de guardia", S, 0.25f))),
            new Building("entrenamiento", "Entrenamiento",
                Pure("gimnasio", "Gimnasio", E, 20),
                new Floor("funcional", C("calistenia", "Calistenia", E, 0.35f, 16), C("parkour", "Parkour", E, 0.3f, 8), C("crossfit", "CrossFit", E, 0.35f, 16)),
                new Floor("combate", C("judo", "Judo", E, 0.25f, 7), C("karate", "Karate", E, 0.25f, 6), C("kickboxing", "Kickboxing", E, 0.25f, 6), C("mma", "MMA", E, 0.25f, 6)),
                new Floor("salas", C("yoga", "Yoga", E, 0.25f, 8), C("musica", "Música", E, 0.25f, 8), C("ballet", "Ballet", E, 0.25f, 8), C("bailes", "Bailes sociales", E, 0.25f, 8))),
            new Building("ciencias", "Ciencias",
                new Floor("laboratorios", C("quimica", "Química", Ci, 0.5f), C("biologia", "Biología", Ci, 0.5f)),
                new Floor("informatica", C("informatica", "Informática", Ci, 0.8f), C("pecera", "Pecera de simulación", Ci, 0.15f))),
            new Building("produccion", "Producción",
                Pure("construccion", "Construcción", P),
                new Floor("mecanica-garaje", C("mecanica", "Mecánica", P, 0.5f), C("garaje", "Garaje del submarino", P, 0.5f)),
                new Floor("textil-huerto", C("textil", "Textil", P, 0.5f), C("huerto-vertical", "Huerto vertical", Ca, 0.5f))),
            new Building("campos", "Árbol de los Campos",
                Pure("campos-1", "Cultivos 1", Ca), Pure("campos-2", "Cultivos 2", Ca), Pure("campos-3", "Cultivos 3", Ca), Pure("campos-4", "Cultivos 4", Ca)),
        };
        var res = new Building("residencia", "Residencia", new Floor("residencia-plaza", C("residencia", "Residencia: castillos y ring de almohadas", V, residence)));
        for (int i = 1; i <= 5; i++) res.floors.Add(Pure($"habitaciones-{i}", $"Habitaciones ({i})", V));
        l.Add(res);
        return l;
    }
}
