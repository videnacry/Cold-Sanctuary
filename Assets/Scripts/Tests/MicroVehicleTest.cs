using System.Collections;
using UnityEngine;

/// <summary>
/// PRUEBA DEL TRANSPORTE DE ÉPOCA QUE SE USA (docs/plan-microcosmos-realismo.md §3.3, P6; <see cref="MicroVehicle"/>), sin cargar escenas:
/// 1. alguien con CharacterController sube (queda en el asiento, con su controlador apagado) y nadie más puede subir a la vez;
/// 2. con el acelerador avanza hacia delante, sin pasar de su velocidad máxima, y con la dirección gira; marcha atrás, más despacio;
/// 3. baja a un lado, con su controlador encendido otra vez, y el vehículo frena solo;
/// 4. carga un palé en las horquillas (viaja con él) y lo deja delante.
/// </summary>
public class MicroVehicleTest : MonoBehaviour, ITestUnit
{
    public int Group => 123;   // (119 y 120: otras ramas; 121 escala, 122 vista de corte)
    public bool ParallelSafe => true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Register()
    {
        ColdSanctuary.TestRunner tr = FindAnyObjectByType<ColdSanctuary.TestRunner>();
        if (tr != null && tr.GetComponentInChildren<MicroVehicleTest>() == null) tr.gameObject.AddComponent<MicroVehicleTest>();
    }

    public IEnumerator Run()
    {
        TestProbe.Begin("Transporte de época: subir, conducir, bajar y llevar carga");
        var car = GameObject.CreatePrimitive(PrimitiveType.Cube); car.name = "Coche_prueba_transporte";
        Destroy(car.GetComponent<Collider>());
        car.transform.position = new Vector3(-9200f, -700f, -9200f); car.transform.localScale = Vector3.one;
        var seat = new GameObject("Asiento").transform; seat.SetParent(car.transform, false); seat.localPosition = new Vector3(0f, 0.4f, 0f);
        var forks = new GameObject("Horquillas").transform; forks.SetParent(car.transform, false); forks.localPosition = new Vector3(0f, 0.1f, 1.2f);
        var driver = new GameObject("Conductor_prueba"); driver.transform.position = car.transform.position + new Vector3(2f, 0f, 0f);
        CharacterController cc = driver.AddComponent<CharacterController>();
        var other = new GameObject("Otro_prueba");
        var pallet = GameObject.CreatePrimitive(PrimitiveType.Cube); pallet.name = "Palé_prueba"; Destroy(pallet.GetComponent<Collider>());
        pallet.transform.position = car.transform.position + new Vector3(0f, 0f, 5f);
        try
        {
            MicroVehicle v = car.AddComponent<MicroVehicle>();
            v.title = "Coche de prueba"; v.seat = seat; v.cargoPoint = forks; v.maxSpeed = 4f; v.accel = 4f; v.turn = 60f;
            yield return null;

            // 1 · Subir.
            bool up = v.Board(driver);
            TestProbe.Check("sube: queda en su asiento, con el controlador apagado", up && v.Driver == driver && driver.transform.parent == seat && !cc.enabled && (driver.transform.position - seat.position).sqrMagnitude < 1e-4f);
            TestProbe.Check("nadie más sube a la vez", !v.Board(other));

            // 2 · Conducir.
            Vector3 p0 = car.transform.position; float y0 = car.transform.eulerAngles.y;
            for (int i = 0; i < 40; i++) v.Drive(1f, 0f, 0.1f);   // 4 s a fondo
            Vector3 d = car.transform.position - p0;
            TestProbe.Check("avanza hacia delante, sin pasar de su máxima", d.z > 8f && Mathf.Abs(d.x) < 0.01f && v.Speed <= v.maxSpeed + 1e-3f, $"{d.z:0.0} m, {v.Speed:0.0} m/s");
            for (int i = 0; i < 10; i++) v.Drive(1f, 1f, 0.1f);
            TestProbe.Check("gira con la dirección", Mathf.Abs(Mathf.DeltaAngle(y0, car.transform.eulerAngles.y)) > 20f, $"{Mathf.DeltaAngle(y0, car.transform.eulerAngles.y):0}°");
            for (int i = 0; i < 60; i++) v.Drive(-1f, 0f, 0.1f);
            TestProbe.Check("marcha atrás, más despacio", v.Speed < 0f && -v.Speed <= v.maxSpeed * 0.4f + 1e-3f, $"{v.Speed:0.0} m/s");
            TestProbe.Check("el conductor viaja con él", (driver.transform.position - seat.position).sqrMagnitude < 1e-4f);

            // 3 · Bajar.
            v.Leave();
            TestProbe.Check("baja a un lado, con el controlador encendido", v.Driver == null && driver.transform.parent == null && cc.enabled && (driver.transform.position - car.transform.position).magnitude > 0.5f);
            float sp = Mathf.Abs(v.Speed);
            for (int i = 0; i < 30; i++) v.Drive(1f, 0f, 0.1f);   // sin conductor, el acelerador no hace nada: frena
            TestProbe.Check("sin conductor, frena solo", Mathf.Abs(v.Speed) < 1e-3f && sp > 0f, $"de {sp:0.0} a {v.Speed:0.0} m/s");

            // 4 · La carga.
            bool loaded = v.Load(pallet.transform);
            Vector3 c0 = car.transform.position;
            v.Board(driver); for (int i = 0; i < 10; i++) v.Drive(1f, 0f, 0.1f); v.Leave();
            TestProbe.Check("carga el palé en las horquillas y viaja con él", loaded && v.Cargo == pallet.transform && (pallet.transform.position - forks.position).sqrMagnitude < 1e-4f && (car.transform.position - c0).magnitude > 1f);
            Transform dropped = v.Unload();
            TestProbe.Check("lo deja en el suelo, delante", dropped == pallet.transform && v.Cargo == null && pallet.transform.parent != forks);
        }
        finally { Destroy(car); Destroy(driver); Destroy(other); Destroy(pallet); }
        TestProbe.End();
    }
}
