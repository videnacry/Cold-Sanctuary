/// <summary>
/// LÍMITES DE LA ESTRUCTURA (Berón 2026-10-28; docs/columnas-sismicas.md §Límite de diseño): las megacolumnas de las
/// 8 puntas de cada plataforma están dimensionadas para edificios de hasta <see cref="MaxFloors"/> pisos. Si algún día un
/// edificio tiene más, la construcción lo avisa (error en consola) y la prueba de la vida del santuario falla: hay que
/// reforzar las columnas (más diámetro o más chapa, ver el doc) antes de subir más pisos.
/// LA RAÍZ DEL ÁRBOL DE LOS CAMPOS (docs/edificio-arbol.md) tiene su propia comprobación (<see cref="RootCheck"/>): es la
/// ÚNICA columna (un tronco CUADRADO de agua) de los pisos en voladizo, las ramas con sus campos de punta y su techo.
/// </summary>
public static class StructuralLimits
{
    /// <summary>Pisos (contando el techo de alfombras) que aguantan las columnas actuales.</summary>
    public const int MaxFloors = 30;

    /// <summary>Diámetro de la columna (tubo de acero relleno de hormigón, chapa de 40 mm, relleno C80) para 30 pisos:
    /// 1,6 m en las plantas grandes (≥ 30 m de lado) y 1,3 m en las demás.</summary>
    public static float ColumnDiameter(float longestSide) => longestSide >= 30f ? 1.6f : 1.3f;

    // ── TRONCO DEL ÁRBOL (EDIFICIO EN HÉLICE) ──────────────────────────────────────────────────────────────────

    /// <summary>Lado mínimo del tronco (Berón 2026-10-29: es el único apoyo de todo el árbol, y al menos tan ancho como la
    /// plataforma de alfombras de su techo).</summary>
    public const float RootMinDiameter = 20f;
    /// <summary>Por encima de este uso (carga / capacidad) la construcción avisa; por encima de 1, error.</summary>
    public const float RootWarnUse = 0.8f;

    /// <summary>Resultado de la comprobación del tronco (MN, MN/m, MPa, MN·m).</summary>
    public struct RootReport
    {
        public float floorsMN, tipsMN, topMN, selfMN, axialMN, axialCapacityMN;
        public float waterMN, groundMN;           // el agua apoya en el fondo (no en la pared): va al cimiento
        public float hoopDemand, hoopCapacity;    // el agua tira de la pared en el fondo (p·a) y lo que resisten acrílico + zunchos
        public float panelStress, panelCapacity;  // flexión del acrílico entre nervios y zunchos (MPa)
        public float floorMoment, floorMomentCapacity, tipMoment, tipMomentCapacity;   // voladizos (MN·m)
        public float AxialUse => axialMN / UnityEngine.Mathf.Max(1e-3f, axialCapacityMN);
        public float HoopUse => hoopDemand / UnityEngine.Mathf.Max(1e-3f, hoopCapacity);
        public float PanelUse => panelStress / UnityEngine.Mathf.Max(1e-3f, panelCapacity);
        public float FloorUse => floorMoment / UnityEngine.Mathf.Max(1e-3f, floorMomentCapacity);
        public float TipUse => tipMoment / UnityEngine.Mathf.Max(1e-3f, tipMomentCapacity);
        public float Use => UnityEngine.Mathf.Max(UnityEngine.Mathf.Max(AxialUse, HoopUse), UnityEngine.Mathf.Max(PanelUse, UnityEngine.Mathf.Max(FloorUse, TipUse)));
        public bool Warn => Use > RootWarnUse;
        public bool Fail => Use > 1f;
        public override string ToString() =>
            $"carga {axialMN:0} MN (pisos {floorsMN:0}, puntas {tipsMN:0}, techo {topMN:0}, pared {selfMN:0}) / {axialCapacityMN:0} MN = {AxialUse:P0}; " +
            $"agua {waterMN:0} MN al cimiento; zuncho {hoopDemand:0.0}/{hoopCapacity:0.0} MN/m = {HoopUse:P0}; acrílico {panelStress:0.0}/{panelCapacity:0} MPa = {PanelUse:P0}; " +
            $"voladizo piso {floorMoment:0}/{floorMomentCapacity:0} MN·m = {FloorUse:P0}; rama {tipMoment:0}/{tipMomentCapacity:0} MN·m = {TipUse:P0}";
    }

    /// <summary>
    /// Comprueba el TRONCO CUADRADO de un edificio en hélice (<paramref name="h"/>) con <paramref name="floors"/> pisos pegados y
    /// <paramref name="tips"/> campos de punta, <paramref name="height"/> m de agua y <paramref name="ribs"/> nervios. Modelo
    /// sencillo y del lado de la seguridad:
    /// - PISO (S × S): losa de 0,28 m (2 t/m³), tierra de cultivo 0,4 m (1,6 t/m³) en el 45 % y uso 3 kN/m².
    /// - PUNTA (T × T): lo mismo, más el anillo con sus frutales (~60 t), la rama (agua 5 × 5 m y acrílico, ~30 m) y la
    ///   columna de agua (5 × 5 m, de la rama al anillo).
    /// - TECHO: lado del tronco × 0,3 m (2 t/m³) y las alfombras. PARED de acrílico (1,19 t/m³): su peso; no cuenta como portante.
    /// - CAPACIDAD: nervios CFST Ø 0,8 m (chapa 30 mm S460 + C80), φ = 0,6 → ~38 MN cada uno.
    /// - ZUNCHO: el agua tira de cada cara p·a en el fondo (a = medio lado del agua); lo resisten el acrílico (10 MPa a largo
    ///   plazo) y los zunchos de acero (<paramref name="hoopArea"/> m² cada <paramref name="hoopSpacing"/> m, 350 MPa).
    /// - ACRÍLICO entre nervios y zunchos: placa de luz s = mín(paso de nervios, paso de zunchos), M = p·s²/10, σ = 6M/t² ≤ 10 MPa.
    /// - VOLADIZO DE UN PISO: su peso a S/2 de la cara; lo sostienen 4 tornapuntas CFST Ø 0,7 m (~29 MN) que bajan
    ///   <paramref name="strutDrop"/> m hasta la cara del tronco y apoyan a 0,625·S.
    /// - VOLADIZO DE UNA RAMA: la punta y su rama a la distancia de la punta; lo sostiene la X    ///   Ø 0,8 m arriba y 2 abajo, de 5 m de canto).
    /// </summary>
    public static RootReport RootCheck(HelixTower h, int floors, int tips, float height = 92f, int ribs = 24,
                                       float hoopSpacing = 6f, float hoopArea = 0.09f, float strutDrop = 4f)
    {
        const float g = 9.81f;
        var r = new RootReport();
        float S = h.floorSize, T = h.tipSize, a = h.waterHalf, side = 2f * h.OuterHalf;
        float floorT = S * S * 0.28f * 2f + 0.45f * S * S * 0.4f * 1.6f + S * S * 3f / g;
        float tubeW = h.TubeInnerH, colW = 2f * h.colHalf, colH = h.tipRise + h.ringH + h.doorHeight + 1.5f;   // hueco del tubo y de la columna (agua)
        float tubeL = (h.TipCenterS - new UnityEngine.Vector3(h.mouthX, 0f, h.OuterHalf)).magnitude;   // ~30 m: de la cara a la punta
        float tipDeckT = T * T * 0.28f * 2f + 0.45f * T * T * 0.4f * 1.6f + T * T * 3f / g + 60f + colW * colW * colH + 4f * (colW + 2f * h.colWall) * h.colWall * colH * 1.19f;
        float tubeT = tubeW * tubeW * tubeL + 4f * 2f * h.tubeHalf * h.tubeWall * tubeL * 1.19f, tipT = tipDeckT + tubeT;
        r.floorsMN = floors * floorT * g / 1000f;
        r.tipsMN = tips * tipT * g / 1000f;
        r.topMN = (side * side * 0.3f * 2f + 20f) * g / 1000f;
        r.selfMN = 4f * (side - h.wall) * h.wall * height * 1.19f * g / 1000f;
        r.axialMN = r.floorsMN + r.tipsMN + r.topMN + r.selfMN;
        float steel = UnityEngine.Mathf.PI * 0.8f * 0.03f * 460f, concrete = UnityEngine.Mathf.PI * 0.25f * 0.74f * 0.74f * 80f * 0.85f;
        float rib = (steel + concrete) * 0.6f;
        r.axialCapacityMN = ribs * rib;
        r.waterMN = 4f * a * a * height * g / 1000f;
        r.groundMN = r.axialMN + r.waterMN;
        float p = 1000f * g * height / 1e6f;                                      // MPa en el fondo
        r.hoopDemand = p * a;                                                     // MN/m
        r.hoopCapacity = h.wall * 10f + (hoopArea / UnityEngine.Mathf.Max(0.1f, hoopSpacing)) * 350f;
        float span = UnityEngine.Mathf.Min(side / UnityEngine.Mathf.Max(1f, ribs / 4f), hoopSpacing);
        r.panelStress = 6f * (p * span * span / 10f) / (h.wall * h.wall);
        r.panelCapacity = 10f;
        r.floorMoment = floorT * g / 1000f * S * 0.5f;
        float x = 0.625f * S, sin = strutDrop / UnityEngine.Mathf.Sqrt(strutDrop * strutDrop + x * x);
        float strut = (UnityEngine.Mathf.PI * 0.7f * 0.025f * 460f + UnityEngine.Mathf.PI * 0.25f * 0.65f * 0.65f * 80f * 0.85f) * 0.6f;
        r.floorMomentCapacity = 4f * strut * sin * x;
        r.tipMoment = (tipDeckT * tubeL + tubeT * tubeL * 0.5f) * g / 1000f;
        float chord = (UnityEngine.Mathf.PI * 1.0f * 0.04f * 460f + UnityEngine.Mathf.PI * 0.25f * 0.92f * 0.92f * 80f * 0.85f) * 0.6f;   // CFST Ø 1 m, chapa 40 mm
        r.tipMomentCapacity = 2f * chord * (h.TubeInnerH + 1.35f);   // 2 cordones comprimidos; canto = el tubo y sus cordones
        return r;
    }
}
