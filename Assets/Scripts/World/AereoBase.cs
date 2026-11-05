using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL SANTUARIO AÉREO, la escena (docs/sitio-nuclear.md §23): la lanzadera entera y lo que se ve del parque. La lógica de la energía vive abajo (<see cref="EnergySite"/>), porque
/// da luz a todo el santuario esté esto cargado o no. Al cargarse, le presta sus piezas (<see cref="EnergySite.Attach"/>); al descargarse, se las quita. Nada de abajo apunta aquí.
/// </summary>
public class AereoBase : MonoBehaviour
{
    public static AereoBase Instance { get; private set; }

    [Tooltip("Donde aparece Kushal en cada casquete.")] public Transform launchArrival, energyArrival;
    public List<Transform> tables = new List<Transform>();
    public List<Transform> figures = new List<Transform>();
    public List<Vector3> figureSpots = new List<Vector3>();
    public DroneDragon dragon;
    public Transform storage;
    [Tooltip("Por turbina, en el mismo orden que abajo: rotor, pala negra y detector.")] public List<Transform> rotors = new List<Transform>(), blades = new List<Transform>(), detectors = new List<Transform>();
    [Tooltip("Por turbina mareal: rotor y sonar.")] public List<Transform> tidalRotors = new List<Transform>(), tidalSonars = new List<Transform>();

    void OnEnable()
    {
        Instance = this;
        if (EnergySite.Instance != null) EnergySite.Instance.Attach(this);
    }

    void OnDisable()
    {
        if (EnergySite.Instance != null) EnergySite.Instance.Detach(this);
        if (Instance == this) Instance = null;
    }
}
