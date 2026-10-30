using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MEDIDAS DEL ÁRBOL DE LOS CAMPOS (docs/edificio-arbol.md; Berón 2026-10-29). Las comparten el constructor
/// (SampleSceneBuilder.Tree.cs), el árbol en juego (<see cref="ArbolCampos"/>, <see cref="TreeRotator"/>) y las pruebas.
/// - RAÍZ: una columna de agua TRANSPARENTE (como el tanque de virtualización) de Ø 20 m de agua y 1 m de pared de
///   acrílico (Ø 22 m), de 0 a 101 m; la sostiene un exoesqueleto de 16 nervios de acero (CFST) con zunchos. Es el único
///   apoyo de todo el árbol. Cada piso tiene una PUERTA con campo magnético que sujeta el agua; el hueco de cada puerta es
///   a la vez una campana de aire (el campo aparta el agua: se respira).
/// - TRONCO: 8 pisos de cultivo, octógonos de ~51 m (rectángulo de 46 × 46 m), girados 45° cada uno respecto al de abajo
///   e inclinados 3° alternando el sentido (acordeón); su canaleta del borde bajo riega el piso de abajo.
/// - RAMAS: 6 tubos de agua inclinados 35° desde 66 m, de 40 m, con un anillo de nidos de ~13 m en la punta.
/// - COPA: plataforma octogonal plana de 22 m (el ancho de la raíz) con las alfombras; gira con el árbol.
/// </summary>
public static class TreeSpec
{
    public const string RootName = "Arbol_de_los_Campos", Building = "Árbol de los Campos";
    /// <summary>Sitio: lóbulo Sur:Milpa (la milpa, la chacra, el arrozal, el bambusal, el bosque comestible y la rotación se mudan aquí).</summary>
    public static readonly Vector3 Site = new Vector3(-50f, 0f, -84f);
    public const float WaterRadius = 10f, WallThickness = 1f, RootHeight = 101f, WaterTop = 99.6f, WaterBottom = 0.05f;
    public static float OuterRadius => WaterRadius + WallThickness;   // 11 m → Ø 22 m
    /// <summary>Cota de la cara de cada piso (P1…P8). P2→P3 deja 11 m para la guadua (hasta 9 m) y P7→P8, 10 m para el
    /// dosel del bosque comestible; los demás, 7 m.</summary>
    public static readonly float[] FloorY = { 6f, 13f, 24f, 31f, 38f, 45f, 52f, 62f };
    public const float FloorHalf = 23f, FloorEdge = 2.5f;   // rectángulo 46 × 46 m dentro del octógono (+2,5 m y chaflanes)
    public const float Tilt = 3f, Twist = 45f;
    public static float TiltOf(int floor) => floor % 2 == 1 ? Tilt : -Tilt;   // P1 +3°, P2 −3°… (acordeón)
    public static float YawOf(int floor) => floor * Twist;                     // P1 45°, P2 90°…
    /// <summary>La pose de un piso respecto al árbol: primero la inclinación (sobre su eje X local), luego el giro.</summary>
    public static Quaternion PoseOf(int floor) => Quaternion.Euler(0f, YawOf(floor), 0f) * Quaternion.Euler(TiltOf(floor), 0f, 0f);
    public const float BranchBaseY = 66f, BranchLength = 40f, BranchAngle = 35f, BranchRadius = 1.5f, BranchWater = 1.3f;
    public static readonly float[] BranchYaw = { 22.5f, 90f, 135f, 202.5f, 270f, 315f };   // entre los nervios (cada 22,5°) y lejos de la puerta del piso 8 (0°, a 62–65 m)
    public const float RingHub = 7f, RingWidth = 4f;     // RingDeck alrededor de 7 × 7 → 13,4 m de lado
    public const float BulbRadius = 2f, BulbInner = 1.8f;
    public const float DoorWidth = 1.8f, DoorHeight = 2.6f;
    public const float TopWidth = 22f;
    public const int Ribs = 16;
    /// <summary>ZUNCHOS del exoesqueleto (los que sujetan el empuje del agua junto con el acrílico): uno cada <see cref="HoopSpacing"/> m,
    /// planos y hondos (<see cref="HoopHeight"/> de alto × <see cref="HoopDepth"/> hacia fuera: la misma sección de acero, 0,09 m², que
    /// uno de 0,6 × 0,15, pero de canto: tapan cuatro veces menos la vista). Con 6 m, <see cref="StructuralLimits.RootCheck"/> da el
    /// zuncho al 64 % (Berón 2026-10-30: la raíz se tiene que ver transparente, como el tanque: se ve nadar a la gente).</summary>
    public const float HoopSpacing = 6f, HoopHeight = 0.25f, HoopDepth = 0.36f;
    public static float HoopArea => HoopHeight * HoopDepth;
    /// <summary>Lo que se muda al árbol, de abajo arriba (P5 = el área del Huerto, con las hortalizas).</summary>
    public static readonly GardenPlot.Guild[] Guilds =
    {
        GardenPlot.Guild.Arrozal, GardenPlot.Guild.Bambusal, GardenPlot.Guild.Milpa, GardenPlot.Guild.ChacraAndina,
        GardenPlot.Guild.Hortalizas, GardenPlot.Guild.Rotacion, GardenPlot.Guild.BosqueComestible, GardenPlot.Guild.BosqueComestible,
    };
    public static readonly int[] Seeds = { 14, 17, 12, 13, 11, 15, 16, 18 };

    /// <summary>Dirección horizontal (local del árbol) de un ángulo de brújula (0 = +Z, 90 = +X).</summary>
    public static Vector3 Dir(float deg) => new Vector3(Mathf.Sin(deg * Mathf.Deg2Rad), 0f, Mathf.Cos(deg * Mathf.Deg2Rad));
    public static float Angle(Vector3 v) => Mathf.Repeat(Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg, 360f);
    /// <summary>Ángulo de la PUERTA del piso k (0 = suelo): el lado +Z local del piso (el de su cultivo: se sale de la raíz al bancal), girado con él.</summary>
    public static float DoorAngle(int floor) => Mathf.Repeat(YawOf(floor), 360f);
    /// <summary>Cota del umbral de la puerta del piso k: su cara a 11 m del eje (inclinada).</summary>
    public static float DoorBottom(int floor) => floor == 0 ? 0f : FloorY[floor - 1] + (PoseOf(floor) * new Vector3(0f, 0f, OuterRadius)).y;
    /// <summary>Punto (local del árbol) del piso k a <paramref name="r"/> m del eje por el lado de su puerta, en su suelo.</summary>
    public static Vector3 OnFloorAtDoor(int floor, float r) => floor == 0 ? Dir(DoorAngle(0)) * r
        : new Vector3(0f, FloorY[floor - 1], 0f) + PoseOf(floor) * new Vector3(0f, 0f, r);
    /// <summary>Punta de la rama b (el final del tubo) y su dirección.</summary>
    public static Vector3 BranchDir(int b) => Dir(BranchYaw[b]) * Mathf.Cos(BranchAngle * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(BranchAngle * Mathf.Deg2Rad);
    public static Vector3 BranchStart(int b) => Dir(BranchYaw[b]) * OuterRadius + Vector3.up * BranchBaseY;
    public static Vector3 BranchEnd(int b) => BranchStart(b) + BranchDir(b) * BranchLength;
    /// <summary>Cara de arriba del anillo de nidos de la rama b (la punta del tubo queda 2,5 m por debajo, dentro de su campana).</summary>
    public static float RingDeckY(int b) => BranchEnd(b).y + 2.5f;
}

/// <summary>Un NIVEL del árbol al que se llega nadando por la raíz: el suelo, cada piso y cada anillo de nidos. Los puntos
/// van en coordenadas LOCALES del árbol (gira) y a ras del suelo (los pies).</summary>
[System.Serializable]
public class TreeLevel
{
    public enum Kind { Ground, Floor, Ring, Top }
    public string name;
    public Kind kind;
    public int index;
    public Transform deck;          // el piso, el anillo o la copa (null = el suelo de hielo)
    public float doorAngle;         // brújula, local del árbol
    public Vector3 outside;         // donde se espera para entrar (pies, sobre la cubierta)
    public Vector3 door;            // en el hueco de la puerta (campana de aire)
    public Vector3 inside;          // ya en el agua, junto a la puerta
    public Vector3[] via = new Vector3[0];   // del eje de la raíz a «inside» (las ramas: por el tubo y la campana)
    public Transform field;         // el campo magnético de su puerta
}

