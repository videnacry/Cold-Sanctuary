using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EDIFICIO EN HÉLICE (modelo genérico, Berón 2026-10-30; docs/edificio-arbol.md): un TRONCO CUADRADO de agua (se nada por
/// dentro de nivel a nivel) del que salen plataformas CUADRADAS por sus lados. Lados a (+Z), b (+X), c (−Z), d (−X) en el
/// marco del edificio. En cada NIVEL de la hélice:
/// - dos PISOS pegados al tronco en dos lados opuestos (nivel impar: a y c; par: b y d), en MOLINETE: cada piso arranca en
///   su cara del tronco y se corre hacia la derecha (mirando hacia fuera), así ningún piso tapa a otro en planta;
/// - en los otros dos lados, una RAMA (tubo cuadrado de agua: sale recta de la cara y gira hacia la derecha) que lleva a un
///   CAMPO DE PUNTA cuadrado en la esquina libre de ese lado, con una COLUMNA de agua en su esquina de dentro (puerta al campo
///   y puerta al ANILLO que va encima del campo).
/// Todo se mide en el MARCO DEL LADO s (girado s·90°): +Z hacia fuera de la cara, +X a la derecha mirando hacia fuera.
/// Pensado para reutilizarlo: el Árbol de los Campos es <see cref="TreeSpec.Helix"/>; las torres podrán migrar a este modelo
/// con otro juego de medidas.
/// </summary>
[System.Serializable]
public class HelixTower
{
    public float waterHalf = 10f, wall = 1f;          // tronco: agua de 2·waterHalf, pared de acrílico de «wall»
    public float firstY = 64f, levelGap = 11f;         // cara del primer piso y de un nivel al siguiente
    public int levels = 2;
    public float floorSize = 32f;                      // piso pegado al tronco (cuadrado)
    public float tipSize = 16f, tipGap = 4f, tipRise = 6f;   // campo de punta (cuadrado), su hueco al piso y cuánto sube sobre su nivel
    public float ringH = 4f, ringWidth = 4f, ringInside = 0.2f;   // anillo encima del campo (RingDeck: 20 % sobre el campo)
    public float tubeHalf = 2.8f, tubeWall = 0.3f, mouthX = 6.5f;   // rama: tubo cuadrado (hueco de 5 × 5 m: caben varios a la vez; Berón 2026-10-31) y su boca en la cara
    public float colHalf = 2.5f, colWall = 0.3f, colMargin = 1f;  // columna de la punta: agua de 5 × 5 m, a 1 m del borde del campo
    public float doorWidth = 3f, doorHeight = 3f, doorX = 5f;     // puertas de 3 × 3 m (Berón 2026-10-31: más anchas)

    public float OuterHalf => waterHalf + wall;
    public static Quaternion SideRot(int side) => Quaternion.Euler(0f, side * 90f, 0f);
    /// <summary>Un punto del marco del lado <paramref name="side"/> en el marco del edificio.</summary>
    public static Vector3 FromSide(int side, Vector3 p) => SideRot(side) * p;
    public float LevelY(int level) => firstY + (level - 1) * levelGap;
    /// <summary>¿El lado tiene PISO en ese nivel? (impar: a y c; par: b y d). Si no, tiene RAMA.</summary>
    public static bool HasFloor(int level, int side) => (side % 2 == 0) == (level % 2 == 1);
    public float TipY(int level) => LevelY(level) + tipRise;
    public float RingY(int level) => TipY(level) + ringH;

    // ── Huella de cada pieza en el marco de su lado ──
    /// <summary>Centro (marco del lado) de un piso pegado al tronco: x de −h a −h+S (molinete), z de h a h+S.</summary>
    public Vector3 FloorCenterS => new Vector3(floorSize * 0.5f - OuterHalf, 0f, OuterHalf + floorSize * 0.5f);
    /// <summary>Centro (marco del lado) del campo de punta: en la esquina libre, pasado el borde del piso de ese lado.</summary>
    public Vector3 TipCenterS => new Vector3(floorSize - OuterHalf + tipGap + tipSize * 0.5f, 0f, OuterHalf + tipGap + tipSize * 0.5f);
    public float ColOuter => colHalf + colWall;
    /// <summary>Alto del hueco del tubo de una rama (cuadrado, como su ancho).</summary>
    public float TubeInnerH => 2f * (tubeHalf - tubeWall);
    /// <summary>Centro (marco del lado) de la columna de agua de la punta: dentro del campo, en su esquina de dentro (fuera del
    /// anillo, que entra 0,8 m sobre el campo).</summary>
    public Vector3 ColumnS => TipCenterS + new Vector3(-tipSize * 0.5f + colMargin + ColOuter, 0f, -tipSize * 0.5f + colMargin + ColOuter);
    /// <summary>La puerta de un piso en la cara (marco del lado, en el hueco de la pared): frente al centro del piso.</summary>
    public Vector3 DoorS(float y) => new Vector3(doorX, y, waterHalf + wall * 0.5f);
    public float RingOuterHalf => tipSize * 0.5f - ringWidth * ringInside + ringWidth;
    public float RingInnerHalf => tipSize * 0.5f - ringWidth * ringInside;

    /// <summary>Rectángulo en planta (marco del edificio) de una pieza cuadrada de lado <paramref name="size"/> centrada en
    /// <paramref name="centerS"/> del lado <paramref name="side"/>.</summary>
    public static Rect PlanRect(int side, Vector3 centerS, float size)
    {
        Vector3 c = FromSide(side, centerS);
        return new Rect(c.x - size * 0.5f, c.z - size * 0.5f, size, size);
    }
    public static float Overlap(Rect a, Rect b)
    {
        float w = Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin), h = Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin);
        return w > 0f && h > 0f ? w * h : 0f;
    }
    /// <summary>Alcance (radio en planta) de lo más lejano: la esquina del anillo de una punta (gira con el árbol).</summary>
    public float Reach
    {
        get
        {
            Vector3 t = TipCenterS; float r = RingOuterHalf;
            Vector3 f = FloorCenterS; float s = floorSize * 0.5f;
            return Mathf.Max(new Vector2(t.x + r, t.z + r).magnitude, new Vector2(f.x + s, f.z + s).magnitude);
        }
    }
}

/// <summary>
/// MEDIDAS DEL ÁRBOL DE LOS CAMPOS v2 (docs/edificio-arbol.md; Berón 2026-10-30). Las comparten el constructor
/// (SampleSceneBuilder.Tree.cs), el árbol en juego (<see cref="ArbolCampos"/>, <see cref="TreeRotator"/>) y las pruebas.
/// - TRONCO CUADRADO de agua TRANSPARENTE (el cristal y el agua del tanque de virtualización): 20 × 20 m de agua, pared de
///   acrílico de 1 m (22 m), de 0 a 92 m; 24 nervios CFST y zunchos. Puerta abajo (suelo) y entrada por el TECHO (una columna de agua que
///   asoma, con su puerta de frente, junto a las alfombras). Cada piso tiene su puerta con campo magnético (el hueco es una campana de aire).
/// - HÉLICE de 2 niveles (<see cref="Helix"/>): en cada nivel, 2 pisos pegados al tronco (lados alternos) y 2 ramas a campos de
///   punta con su anillo encima. 8 plataformas de cultivo.
/// - EL PRIMER PISO, por encima del árbol más alto que crece en el santuario (<see cref="TallestTree"/>): el suelo bajo el árbol
///   queda libre para poblarlo de árboles.
/// </summary>
public static class TreeSpec
{
    public const string RootName = "Arbol_de_los_Campos", Building = "Árbol de los Campos";
    /// <summary>Sitio: lóbulo Sur:Milpa.</summary>
    public static readonly Vector3 Site = new Vector3(-50f, 0f, -84f);
    /// <summary>EL ÁRBOL MÁS ALTO QUE CRECE en el santuario (medido 2026-10-30 en los constructores): un árbol de hielo VIEJO del
    /// bosque mide hasta 40 m (PolarLandscapeBuilder.HeightFor) ×1,3 (CubeTree) = 52 m de tronco, y su copa llega a ~55,5 m.
    /// (El árbol de la guardia, de 83 m, es único y fijo, en su isla: no crece bajo este árbol.)</summary>
    public const float TallestTree = 55.5f;
    /// <summary>Lo más bajo del árbol (las tornapuntas de los pisos) queda al menos esto sobre la copa más alta.</summary>
    public const float TreeClearance = 4f;
    public const float FirstFloorMin = 50f;
    /// <summary>Cuánto bajan las tornapuntas bajo la cara de un piso.</summary>
    public const float StrutDrop = 4f;
    public static readonly HelixTower Helix = new HelixTower
    {
        waterHalf = 10f, wall = 1f,
        firstY = Mathf.Max(FirstFloorMin, Mathf.Ceil(TallestTree + TreeClearance + StrutDrop)),   // 64 m
        levelGap = 11f, levels = 2, floorSize = 32f, tipSize = 16f, tipGap = 4f, tipRise = 6f,
    };

    public static float WaterHalf => Helix.waterHalf;
    public static float WallThickness => Helix.wall;
    public static float OuterHalf => Helix.OuterHalf;   // 11 m → 22 m de lado
    public const float RootHeight = 92f, WaterTop = 91.5f, WaterBottom = 0.05f;   // el agua del tronco, bajo la losa del techo
    public const float TopWidth = 22f;           // el techo de alfombras: tan ancho como el tronco
    /// <summary>La PUERTA DEL TECHO (Berón 2026-10-31: como la de los anillos, se entra de frente): una columna de agua de 5 × 5 m
    /// que asoma sobre el techo (centro, a ras de la cubierta), con su puerta en la cara +Z (hacia las alfombras).</summary>
    public static readonly Vector3 HatchLocal = new Vector3(0f, RootHeight, -6f);
    public const float HatchHalf = 2.5f;   // medio lado del agua de la columna del techo (pared de 0,3 m)
    public const int Ribs = 24;                  // 4 esquinas + 5 por cara: uno cada ~3,7 m
    public const float HoopSpacing = 6f, HoopHeight = 0.25f, HoopDepth = 0.36f;
    public static float HoopArea => HoopHeight * HoopDepth;
    public const float DoorWidth = 3f, DoorHeight = 3f;
    /// <summary>El lado de la puerta del suelo (a = +Z: hacia las alfombras del pie, al NE).</summary>
    public const int GroundSide = 0;

    /// <summary>Plataformas (P1…P8) en orden de altura: nivel 1 (pisos a y c, puntas b y d), nivel 2 (pisos b y d, puntas a y c).</summary>
    public static (int level, int side, bool tip) Platform(int k)
    {
        int lv = (k - 1) / 4 + 1, j = (k - 1) % 4;
        bool tip = j >= 2;
        int side = (lv % 2 == 1) == !tip ? (j % 2 == 0 ? 0 : 2) : (j % 2 == 0 ? 1 : 3);
        return (lv, side, tip);
    }
    public const int Platforms = 8;
    /// <summary>Qué crece en cada plataforma (P1…P8). En los pisos grandes (32 m), lo alto (bosque, bambú) y el Huerto (P5,
    /// con su tanque y su iniciación); en las puntas (16 m), lo bajo.</summary>
    public static readonly GardenPlot.Guild[] Guilds =
    {
        GardenPlot.Guild.BosqueComestible, GardenPlot.Guild.Bambusal, GardenPlot.Guild.Arrozal, GardenPlot.Guild.Milpa,
        GardenPlot.Guild.Hortalizas, GardenPlot.Guild.BosqueComestible, GardenPlot.Guild.ChacraAndina, GardenPlot.Guild.Rotacion,
    };
    public static readonly int[] Seeds = { 16, 17, 14, 12, 11, 18, 13, 15 };
    /// <summary>Cara de arriba de la plataforma k.</summary>
    public static float PlatformY(int k) { var p = Platform(k); return p.tip ? Helix.TipY(p.level) : Helix.LevelY(p.level); }

    /// <summary>Dirección horizontal (local del árbol) de un ángulo de brújula (0 = +Z, 90 = +X).</summary>
    public static Vector3 Dir(float deg) => new Vector3(Mathf.Sin(deg * Mathf.Deg2Rad), 0f, Mathf.Cos(deg * Mathf.Deg2Rad));
    public static float Angle(Vector3 v) => Mathf.Repeat(Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg, 360f);
}

/// <summary>Un NIVEL del árbol al que se llega nadando: el suelo, cada piso o campo de punta, cada anillo y el techo. Los
/// puntos van en coordenadas LOCALES del árbol (gira) y a ras del suelo (los pies).</summary>
[System.Serializable]
public class TreeLevel
{
    public enum Kind { Ground, Floor, Ring, Top }
    public string name;
    public Kind kind;
    public int index;               // piso 1…8 (en orden de altura), anillo 1…4, suelo 0, techo 9
    public bool tip;                // campo de punta (al final de una rama)
    public int side = -1, level;    // lado del tronco (0 a, 1 b, 2 c, 3 d) y nivel de la hélice
    public Transform deck;          // el piso, el campo, el anillo o el techo (null = el suelo de hielo)
    public float doorAngle;         // brújula, local del árbol: hacia dónde se sale por la puerta
    public float half;              // medio lado de la cubierta (para saber si alguien está en ella)
    public Vector3 outside;         // donde se espera para entrar (pies, sobre la cubierta)
    public Vector3 door;            // en el hueco de la puerta (campana de aire)
    public Vector3 inside;          // ya en el agua, junto a la puerta
    public Vector3[] via = new Vector3[0];   // del tronco a «inside» (las puntas: por la rama y la columna)
    public Transform field;         // el campo magnético de su puerta
}
