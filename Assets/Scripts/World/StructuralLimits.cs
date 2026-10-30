/// <summary>
/// LÍMITES DE LA ESTRUCTURA (Berón 2026-10-28; docs/columnas-sismicas.md §Límite de diseño): las megacolumnas de las
/// 8 puntas de cada plataforma están dimensionadas para edificios de hasta <see cref="MaxFloors"/> pisos. Si algún día un
/// edificio tiene más, la construcción lo avisa (error en consola) y la prueba de la vida del santuario falla: hay que
/// reforzar las columnas (más diámetro o más chapa, ver el doc) antes de subir más pisos.
/// LA RAÍZ DEL ÁRBOL DE LOS CAMPOS (docs/edificio-arbol.md) tiene su propia comprobación (<see cref="RootCheck"/>): es la
/// ÚNICA columna de 8 pisos en voladizo, 6 ramas con sus anillos y su copa, y además es un depósito de 100 m de agua.
/// </summary>
public static class StructuralLimits
{
    /// <summary>Pisos (contando el techo de alfombras) que aguantan las columnas actuales.</summary>
    public const int MaxFloors = 30;

    /// <summary>Diámetro de la columna (tubo de acero relleno de hormigón, chapa de 40 mm, relleno C80) para 30 pisos:
    /// 1,6 m en las plantas grandes (≥ 30 m de lado) y 1,3 m en las demás.</summary>
    public static float ColumnDiameter(float longestSide) => longestSide >= 30f ? 1.6f : 1.3f;

    // ── RAÍZ DEL ÁRBOL ────────────────────────────────────────────────────────────────────────────────────────

    /// <summary>Diámetro mínimo de la raíz (Berón 2026-10-29: es el único apoyo de todo el árbol, y al menos tan ancha como
    /// la plataforma de alfombras de su copa).</summary>
    public const float RootMinDiameter = 20f;
    /// <summary>Por encima de este uso (carga / capacidad) la construcción avisa; por encima de 1, error.</summary>
    public const float RootWarnUse = 0.8f;

    /// <summary>Resultado de la comprobación de la raíz (MN, MN/m de altura para el zuncho).</summary>
    public struct RootReport
    {
        public float floorsMN, branchesMN, topMN, selfMN, axialMN, axialCapacityMN;
        public float waterMN, groundMN;           // el agua apoya en el fondo (no en la pared): va al cimiento
        public float hoopDemand, hoopCapacity;    // empuje del agua en el fondo (p·r) y lo que resisten acrílico + zunchos
        public float AxialUse => axialMN / UnityEngine.Mathf.Max(1e-3f, axialCapacityMN);
        public float HoopUse => hoopDemand / UnityEngine.Mathf.Max(1e-3f, hoopCapacity);
        public float Use => UnityEngine.Mathf.Max(AxialUse, HoopUse);
        public bool Warn => Use > RootWarnUse;
        public bool Fail => Use > 1f;
        public override string ToString() =>
            $"carga {axialMN:0} MN (pisos {floorsMN:0}, ramas {branchesMN:0}, copa {topMN:0}, pared {selfMN:0}) / {axialCapacityMN:0} MN = {AxialUse:P0}; " +
            $"agua {waterMN:0} MN al cimiento; zuncho {hoopDemand:0.0}/{hoopCapacity:0.0} MN/m = {HoopUse:P0}";
    }

    /// <summary>
    /// Comprueba la raíz con <paramref name="floors"/> pisos (de <paramref name="floorSide"/> m), <paramref name="branches"/> ramas y
    /// su agua. Modelo sencillo y del lado de la seguridad:
    /// - PISO: losa ligera de 0,28 m (2 t/m³) en el octógono, tierra de cultivo 0,4 m (1,6 t/m³) en 320 m² y uso 3 kN/m².
    /// - RAMA: tubo de 40 m (acrílico 2,3 t/m), su agua (Ø 2,6 m) y el anillo de nidos con sus frutales (~160 t).
    /// - COPA: 22 × 22 × 0,3 m (2 t/m³) y las alfombras.
    /// - PARED de acrílico (1,19 t/m³) de <paramref name="wall"/> m: su peso. No cuenta como portante (fluye con el tiempo).
    /// - CAPACIDAD: <paramref name="ribs"/> nervios CFST Ø 0,8 m (chapa 30 mm S460 + C80), φ = 0,6 → ~38 MN cada uno.
    /// - ZUNCHO: el agua empuja p·r en el fondo (ρ g h · r); lo resisten el acrílico (10 MPa a largo plazo) y los zunchos de
    ///   acero (<paramref name="hoopArea"/> m² cada <paramref name="hoopSpacing"/> m, 350 MPa; el árbol: 0,25 × 0,36 m cada 6 m).
    /// </summary>
    public static RootReport RootCheck(int floors, int branches, float waterRadius = 10f, float height = 100f, float wall = 1f,
                                       int ribs = 16, float floorSide = 51f,
                                       float hoopSpacing = 2f, float hoopArea = 0.09f)
    {
        const float g = 9.81f;
        var r = new RootReport();
        float octo = floorSide * floorSide * 0.92f;                               // octógono casi cuadrado (chaflanes de 5 m)
        float floorT = octo * 0.28f * 2f + 320f * 0.4f * 1.6f + octo * 3f / g;    // toneladas (el uso, en t equivalentes)
        r.floorsMN = floors * floorT * g / 1000f;
        float branchT = 40f * 2.3f + UnityEngine.Mathf.PI * 1.3f * 1.3f * 40f + 160f;
        r.branchesMN = branches * branchT * g / 1000f;
        r.topMN = (22f * 22f * 0.3f * 2f + 20f) * g / 1000f;
        float ro = waterRadius + wall;
        r.selfMN = UnityEngine.Mathf.PI * (ro * ro - waterRadius * waterRadius) * height * 1.19f * g / 1000f;
        r.axialMN = r.floorsMN + r.branchesMN + r.topMN + r.selfMN;
        float steel = UnityEngine.Mathf.PI * 0.8f * 0.03f * 460f, concrete = UnityEngine.Mathf.PI * 0.25f * 0.74f * 0.74f * 80f * 0.85f;
        r.axialCapacityMN = ribs * (steel + concrete) * 0.6f;
        r.waterMN = UnityEngine.Mathf.PI * waterRadius * waterRadius * height * g / 1000f;
        r.groundMN = r.axialMN + r.waterMN;
        float p = 1000f * g * height / 1e6f;                                      // MPa en el fondo
        r.hoopDemand = p * waterRadius;                                           // MN/m
        r.hoopCapacity = wall * 10f + (hoopArea / UnityEngine.Mathf.Max(0.1f, hoopSpacing)) * 350f;
        return r;
    }
}
