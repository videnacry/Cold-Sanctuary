using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// DÓNDE VIVE CADA UNO (Berón 2026-11-08; docs/jornada-y-rotacion.md §Santuarios y niveles; manual S20–S23). Cada humano tiene su CASA:
/// un santuario y un nivel. Santuario 1 = la tierra (SampleScene), 2 = el Marino (docs/sitio-nuclear.md §20), 3 = el Aéreo (§23).
///  · Los nueve de siempre (<see cref="Originals"/>) empiezan en santuario 1, nivel 1.
///  · Los demás, por sus estadísticas: por defecto la SUMA de los niveles de sus margas del alma (Stats + Yoga + Vínculos, <see cref="CharacterLevel"/>),
///    con los umbrales de UNA tabla (<see cref="PositionThresholds"/>). Berón puede elegir otras estadísticas cambiando <see cref="Score"/>.
///  · ACCESO: se puede ir a santuarios y niveles ANTERIORES, nunca a los siguientes. El nivel sube al terminar una vuelta por todos los santuarios.
///    Por eso basta un número, la POSICIÓN: (nivel − 1) × 3 + (santuario − 1). s1n1 = 0, s2n1 = 1, s3n1 = 2, s1n2 = 3… Se puede ir a un sitio si su
///    posición no pasa de la propia: nivel menor, o el mismo nivel y santuario menor o igual (<see cref="CanAccess"/>).
///  · Se GUARDA entre partidas (PlayerPrefs, como <see cref="Progress"/>): la posición de cada uno, que nunca baja. Mientras corren las pruebas no se guarda.
/// Lógica pura salvo <see cref="Score"/> (lee el cuerpo) y el guardado.
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

    /// <summary>Los nueve de siempre: santuario 1, nivel 1 (Berón 2026-11-08).</summary>
    public static readonly string[] Originals = { "Player", "Linduarda", "Lizmibet", "Goluis", "Panterilia", "Gohageneis", "Irosene", "JefeConstruccion", "Andrea" };
    public static bool IsOriginal(string name) => System.Array.IndexOf(Originals, name) >= 0;

    /// <summary>LA TABLA DE UMBRALES (para Berón): la puntuación mínima para cada posición. Índice = posición: 0 = s1n1, 1 = s2n1, 2 = s3n1,
    /// 3 = s1n2, 4 = s2n2, 5 = s3n2. Con la puntuación por defecto (suma de niveles de las tres margas, 3 al empezar), cada 3 niveles ganados
    /// es un santuario más. Para más niveles, añadir filas.</summary>
    public static readonly int[] PositionThresholds = { 3, 6, 9, 12, 15, 18 };

    /// <summary>PUNTUACIÓN INICIAL de quien aún no tiene margas en el cuerpo (el personal no lleva <see cref="CharacterLevel"/>; solo Kushal).
    /// INFERIDO, para que Berón lo cambie: Fernando (paneles) y Luciana (piloto, lanzadera) tienen su trabajo en el Aéreo → s3n1; el resto → s2n1.</summary>
    public static readonly Dictionary<string, int> StartingScore = new Dictionary<string, int> { { "Fernando", 9 }, { "Luciana", 9 } };
    /// <summary>La de quien no está en <see cref="StartingScore"/> ni tiene margas (INFERIDO: s2n1).</summary>
    public const int DefaultStartingScore = 6;

    /// <summary>La puntuación de una persona (cámbiese para usar otras estadísticas). Por defecto: la suma de los niveles de sus margas.</summary>
    public static System.Func<GameObject, string, int> Score = SoulSum;

    public static int SoulSum(GameObject g, string name)
    {
        CharacterLevel cl = g != null ? g.GetComponent<CharacterLevel>() : null;
        if (cl != null) return cl.stats.level + cl.yoga.level + cl.bonds.level;
        return name != null && StartingScore.TryGetValue(name, out int s) ? s : DefaultStartingScore;
    }

    /// <summary>La posición que da una puntuación: la última fila de la tabla que alcanza.</summary>
    public static int PositionFor(int score)
    {
        int p = 0;
        for (int i = 0; i < PositionThresholds.Length; i++) if (score >= PositionThresholds[i]) p = i;
        return p;
    }

    /// <summary>¿Puede quien vive en <paramref name="own"/> ir a <paramref name="target"/>? A lo anterior sí; a lo siguiente, no.</summary>
    public static bool CanAccess(Home own, Home target) => target.level < own.level || (target.level == own.level && target.sanctuary <= own.sanctuary);
    public static bool CanAccess(int ownPosition, Home target) => CanAccess(Home.At(ownPosition), target);

    /// <summary>EL SANTUARIO DE CADA ÁREA (una tabla, para Berón). Aéreo: la lanzadera y el parque energético (<see cref="AereoRule.IsArea"/>). Marino:
    /// la línea de las cavernas, que baja a él (INFERIDO: la central, la planta atómica y el TRIGA siguen en el santuario 1, §20). Lo demás, la tierra.</summary>
    public static int SanctuaryOfArea(string area)
    {
        if (AereoRule.IsArea(area)) return Aerial;
        if (area == SiteLine.AreaName) return Marine;
        return Land;
    }
    /// <summary>El nivel de cada área: hoy todo es nivel 1 (aún no hay pisos de nivel 2).</summary>
    public static int LevelOfArea(string area) => 1;
    public static Home PlaceOfArea(string area) => new Home(SanctuaryOfArea(area), LevelOfArea(area));

    // ── La casa de cada uno, guardada ──
    const string Key = "cs_santuarios";
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

    /// <summary>La posición de <paramref name="name"/> con esa puntuación: la guardada o la de la tabla, la mayor (nunca baja). Los nueve de siempre
    /// empiezan en s1n1 y solo suben con <see cref="Advance"/>. Si sube, se guarda.</summary>
    public static int PositionOf(string name, int score)
    {
        int saved = Saved(name), p = System.Math.Max(saved, IsOriginal(name) ? 0 : PositionFor(score));
        if (p != saved && !Off) { _saved[name] = p; Save(); }
        return p;
    }

    /// <summary>Terminó el santuario de su casa en esta vuelta: pasa al siguiente; tras el último, al santuario 1 del nivel siguiente. Se guarda.</summary>
    public static int Advance(string name)
    {
        int p = Saved(name) + 1;
        if (!Off) { _saved[name] = p; Save(); }
        return p;
    }

    /// <summary>Lo que lleva de la vuelta del nivel: cuántos santuarios ya ha pasado (0 = está en el primero).</summary>
    public static int RoundProgress(int position) => System.Math.Max(0, position) % Count;

    /// <summary>(Pruebas y «empezar de cero») Borra lo guardado.</summary>
    public static void Reset() { Load(); _saved.Clear(); Save(); }
}
