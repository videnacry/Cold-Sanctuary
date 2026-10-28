/// <summary>
/// LÍMITES DE LA ESTRUCTURA (Berón 2026-10-28; docs/columnas-sismicas.md §Límite de diseño): las megacolumnas de las
/// 8 puntas de cada plataforma están dimensionadas para edificios de hasta <see cref="MaxFloors"/> pisos. Si algún día un
/// edificio tiene más, la construcción lo avisa (error en consola) y la prueba de la vida del santuario falla: hay que
/// reforzar las columnas (más diámetro o más chapa, ver el doc) antes de subir más pisos.
/// </summary>
public static class StructuralLimits
{
    /// <summary>Pisos (contando el techo de alfombras) que aguantan las columnas actuales.</summary>
    public const int MaxFloors = 30;

    /// <summary>Diámetro de la columna (tubo de acero relleno de hormigón, chapa de 40 mm, relleno C80) para 30 pisos:
    /// 1,6 m en las plantas grandes (≥ 30 m de lado) y 1,3 m en las demás.</summary>
    public static float ColumnDiameter(float longestSide) => longestSide >= 30f ? 1.6f : 1.3f;
}
