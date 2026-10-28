using UnityEngine;

/// <summary>
/// LÍNEA DE MUEBLES que van UNIDOS (Berón 2026-10-28: «lo único que no se separa es lo que puede ir pegado»): los muebles de
/// dentro de este objeto forman un solo tramo corrido — encimeras alineadas con su fregadero y su máquina, las cámaras frías
/// pegadas, el obrador de una tarea contra la pared, la cocina de leche de la Cría —, así que entre ellos no se guarda el paso
/// de <see cref="FurnitureSpacing.Standard"/> m; el tramo entero sí lo guarda con todo lo demás. Lo ponen los constructores
/// de la escena en el contenedor del tramo.
/// </summary>
public class FurnitureLine : MonoBehaviour { }
