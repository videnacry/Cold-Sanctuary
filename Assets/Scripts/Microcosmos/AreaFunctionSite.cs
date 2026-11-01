using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UN EDIFICIO QUE HACE FUNCIONES (docs/microcosmos-civilizaciones.md §2): dice qué plantilla del registro es
/// (<see cref="AreaFunctions.Buildings"/>: «Cocina», «Restaurante», «Hospital», «Dojo», «Sala de Yoga», «Huerto», «Taller»…)
/// y sus muebles (<see cref="AreaFunctionStation"/>) qué puesto cubre cada uno. Lo que se puede hacer en él sale del registro
/// filtrado por el año de la escena (<see cref="Eras.Now"/>). Lo usan los insectos-humanos en su jornada
/// (<see cref="MicroJornada"/>) y el jugador (interactuando con un puesto).
/// </summary>
public class AreaFunctionSite : MonoBehaviour
{
    [Tooltip("La plantilla del registro (AreaFunctions.Buildings).")]
    public string building = "Casa";
    [Tooltip("Su nombre de verdad («Restaurante de Ruby Chow, Seattle»).")]
    public string title = "";

    public static readonly List<AreaFunctionSite> All = new List<AreaFunctionSite>();
    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    /// <summary>Las funciones que se hacen aquí en el año de la escena.</summary>
    public List<AreaFunctions.AreaFunction> Functions(int? year = null) => AreaFunctions.ForSite(building, year ?? Eras.Now);

    public AreaFunctionStation[] Stations => GetComponentsInChildren<AreaFunctionStation>(true);

    /// <summary>Un puesto libre que sirva para <paramref name="f"/> (o null).</summary>
    public AreaFunctionStation StationFor(AreaFunctions.AreaFunction f, Object who = null)
    {
        AreaFunctionStation any = null;
        foreach (AreaFunctionStation s in Stations)
        {
            if (f != null && f.stations.Length > 0 && System.Array.IndexOf(f.stations, s.station) < 0) continue;
            if (s.occupant == null || s.occupant == who) return s;
            if (any == null) any = s;
        }
        return any;
    }

    /// <summary>Una función de aquí para el papel <paramref name="role"/> que tenga puesto (la que toca según la hora).</summary>
    public AreaFunctions.AreaFunction Pick(string role, int salt)
    {
        var fs = Functions();
        var ok = new List<AreaFunctions.AreaFunction>();
        foreach (var f in fs)
        {
            if (StationFor(f) == null) continue;
            if (string.IsNullOrEmpty(role) || f.roles.Length == 0 || System.Array.IndexOf(f.roles, role) >= 0 || System.Array.IndexOf(f.roles, "todos") >= 0) ok.Add(f);
        }
        if (ok.Count == 0) foreach (var f in fs) if (StationFor(f) != null) ok.Add(f);
        return ok.Count > 0 ? ok[Mathf.Abs(salt) % ok.Count] : null;
    }
}
