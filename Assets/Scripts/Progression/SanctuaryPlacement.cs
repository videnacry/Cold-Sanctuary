using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// DÓNDE VIVE CADA UNO (Berón 2026-11-08; docs/progresion-santuarios.md, docs/jornada-y-rotacion.md §Reparto por santuarios; manual J33). Cada humano tiene
/// su CASA: un santuario y un nivel. Santuario 1 = la tierra (SampleScene), 2 = el Marino (docs/sitio-nuclear.md §20), 3 = el Aéreo (§23).
///  · SE SUBE POR PODER: un humano sale de su santuario solo cuando su poder supera al JEFE de ese santuario, el animal más poderoso de su fauna
///    (<see cref="SanctuaryPower"/>). Entonces pasa al siguiente santuario del mismo nivel, o al primero del nivel siguiente tras el último.
///    Se prepara con tareas, entrenamiento, yoga y meditación; después se le enseña magia, y magia y bonusPacks son lo que le deja superar a una ballena.
///  · LA CASA DE INICIO (<see cref="StartingHome"/>): Irosene y Lizmibet en s2n1 y Linduarda en s3n1 (Berón); los demás, por su poder (un humano
///    corriente, s1n1).
///  · ACCESO: se puede ir a santuarios y niveles ANTERIORES, nunca a los siguientes. Basta un número, la POSICIÓN: (nivel − 1) × 3 + (santuario − 1).
///    s1n1 = 0, s2n1 = 1, s3n1 = 2, s1n2 = 3… Se va a un sitio si su posición no pasa de la propia (<see cref="CanAccess"/>).
///  · Se GUARDA entre partidas (PlayerPrefs, como <see cref="Progress"/>): la posición de cada uno, que nunca baja. Mientras corren las pruebas no se guarda.
/// Lógica pura salvo el guardado.
/// </summary>
public static class SanctuaryPlacement
{
    public const int Land = 1, Marine = 2, Aerial = 3, Count = 3;

    /// <summary>Un sitio: santuario y nivel. También la casa de una persona.</summary>
    public struct Home : System.IEquatable<Home>
    {
        public int sanctuary, level;
        public Home(int sanctuary, int level) { this.sanctuary = sanctuary; this.level = level; }
        public int Position => (level - 1) * Count + (sanctuary - 1);
        public static Home At(int position) { int p = System.Math.Max(0, position); return new Home(p % Count + 1, p / Count + 1); }
        public bool Equals(Home o) => o.sanctuary == sanctuary && o.level == level;
        public override bool Equals(object o) => o is Home h && Equals(h);
        public override int GetHashCode() => Position;
        public override string ToString() => $"santuario {sanctuary} nivel {level}";
    }

    public static readonly Home LandFirst = new Home(Land, 1);

    /// <summary>Los nueve de siempre (Berón 2026-11-08). Ya no fijan la casa: la da el poder, salvo <see cref="StartingHome"/>.</summary>
    public static readonly string[] Originals = { "Player", "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Andrea" };
    public static bool IsOriginal(string name) => System.Array.IndexOf(Originals, name) >= 0;

    /// <summary>LA CASA DE INICIO que no sale del poder (Berón 2026-11-08): Irosene y Lizmibet, planeadas con la magia de s3n1, empiezan en s2n1;
    /// Linduarda, hechicera de nivel 4, en s3n1 (el santuario más alto que hay). El poder puede subirlos más, nunca bajarlos.</summary>
    public static readonly Dictionary<string, int> StartingHome = new Dictionary<string, int> { { "Irosene", 1 }, { "Lizmibet", 1 }, { "Linduarda", 2 } };

    /// <summary>¿Puede quien vive en <paramref name="own"/> ir a <paramref name="target"/>? A lo anterior sí; a lo siguiente, no.</summary>
    public static bool CanAccess(Home own, Home target) => target.level < own.level || (target.level == own.level && target.sanctuary <= own.sanctuary);
    public static bool CanAccess(int ownPosition, Home target) => CanAccess(Home.At(ownPosition), target);

    /// <summary>EL SANTUARIO DE CADA ÁREA (una tabla, para Berón). Aéreo: la lanzadera y el parque energético (<see cref="AereoRule.IsArea"/>). Marino:
    /// la línea de las cavernas, que baja a él, y la PLANTA NUCLEAR (la Planta Atómica y la Central Nuclear: Berón 2026-11-08, «lejos del santuario y de la vida»; dónde está y su estación de alfombras lo mira otra tarea). Lo demás, la tierra.</summary>
    public static int SanctuaryOfArea(string area)
    {
        if (AereoRule.IsArea(area)) return Aerial;
        if (area == SiteLine.AreaName || area == NuclearSite.PlantArea || area == NuclearSite.PowerArea) return Marine;
        return Land;
    }
    /// <summary>El nivel de cada área: hoy todo es nivel 1 (aún no hay pisos de nivel 2).</summary>
    public static int LevelOfArea(string area) => 1;
    public static Home PlaceOfArea(string area) => new Home(SanctuaryOfArea(area), LevelOfArea(area));

    // ── La casa de cada uno, guardada ──
    const string Key = "cs_santuarios_poder";   // (2026-11-08: clave nueva; la de antes guardaba las casas de la tabla de puntuación, que ya no vale)
    static readonly Dictionary<string, int> _saved = new Dictionary<string, int>();
    static bool _loaded;
    static bool Off => ColdSanctuary.TestRunner.Running;

    static void Load()
    {
        if (_loaded) return;
        _loaded = true;
        foreach (string e in PlayerPrefs.GetString(Key, "").Split('\n'))
        {
            int eq = e.LastIndexOf('=');
            if (eq > 0 && int.TryParse(e.Substring(eq + 1), out int p)) _saved[e.Substring(0, eq)] = p;
        }
    }

    static void Save()
    {
        if (Off) return;
        PlayerPrefs.SetString(Key, string.Join("\n", _saved.Select(kv => $"{kv.Key}={kv.Value}")));
        PlayerPrefs.Save();
    }

    /// <summary>La posición guardada (0 si nunca se guardó).</summary>
    public static int Saved(string name) { Load(); return _saved.TryGetValue(name, out int p) ? p : 0; }

    /// <summary>LA CASA DE <paramref name="name"/> con su poder de hoy (<see cref="SanctuaryPower.OfHuman"/>) y los jefes (<see cref="SanctuaryPower.Bosses"/>).
    /// Ver <see cref="Next"/>. Se guarda. En las pruebas, siempre como la primera vez y sin guardar.</summary>
    public static int PositionOf(string name, float power, List<(string who, float power)> bosses, int day)
    {
        Load();
        bool known = !Off && _saved.ContainsKey(name);
        int saved = known ? _saved[name] : 0;
        bool counted = known && _saved.TryGetValue(name + "@", out int last) && last >= day;
        int p = Next(known, saved, counted, StartingHome.TryGetValue(name, out int s) ? s : 0, power, bosses);
        if (!Off) { _saved[name] = p; _saved[name + "@"] = day; Save(); }
        return p;
    }

    /// <summary>Lógica pura de la subida. La primera vez (<paramref name="known"/> falso): la mayor entre su casa de inicio y lo que su poder ya supera.
    /// Después, como mucho UN santuario por día (si ese día no contó ya): si su poder supera al jefe de su casa, pasa al siguiente. Nunca baja.</summary>
    public static int Next(bool known, int saved, bool countedToday, int start, float power, List<(string who, float power)> bosses)
    {
        int p = known ? saved : System.Math.Max(start, SanctuaryPower.PositionFor(power, bosses));
        if (known && !countedToday && p < bosses.Count && power > bosses[p].power) p++;
        return System.Math.Max(p, start);
    }

    /// <summary>Lo que lleva de la vuelta del nivel: cuántos santuarios ya ha pasado (0 = está en el primero).</summary>
    public static int RoundProgress(int position) => System.Math.Max(0, position) % Count;

    /// <summary>(Pruebas y «empezar de cero») Borra lo guardado.</summary>
    public static void Reset() { Load(); _saved.Clear(); Save(); }
}
