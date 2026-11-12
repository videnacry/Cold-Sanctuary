using UnityEngine;

/// <summary>Punto de cuidados de un techo de frutales: la SALA de la experta (Servicios: banco de trasplante, depósito de
/// fertirriego con dosificador, armario de abonos, medidor de pH y EC…) o el punto de riego de los demás techos (depósito y
/// carro). El jardinero empieza ahí la ronda de cada techo.
/// En su propio archivo (2026-10-31): dentro de FruitGardener.cs la escena guardada no encontraba el script al cargarse
/// («Missing script» en los puntos de Producción, Central Nuclear y Ciencias: sin cabezal ni punto en la prueba 55).</summary>
public class FruitCareStation : MonoBehaviour
{
    public string building;
    public bool mainRoom;
    /// <summary>Dónde se pone a trabajar (delante de la línea de equipos).</summary>
    public Vector3 StandPoint => transform.TransformPoint(new Vector3(0f, 1f, 1.6f));
}
