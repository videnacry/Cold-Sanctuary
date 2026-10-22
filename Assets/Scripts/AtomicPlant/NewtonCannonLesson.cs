using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EL CAÑÓN DE NEWTON (su «Tratado del sistema del mundo»; gravitación de los Principia, 1687): desde una montaña muy alta
/// se dispara una bala cada vez más rápido. Si va despacio, cae; más deprisa, cae más lejos; a ~7,8 km/s la Tierra se
/// curva bajo ella tan deprisa como cae: está en ÓRBITA (cae siempre sin llegar nunca); a 11,2 km/s escapa.
/// La trayectoria se calcula con la gravedad de verdad (a = −GM·r/|r|³, GM = 3,986e14 m³/s²) sobre una Tierra
/// a escala, acelerando el tiempo para verla en segundos. ↑/↓ cambia la velocidad, E dispara.
/// </summary>
public class NewtonCannonLesson : PhysicsLesson
{
    public Transform earth;          // esfera de la Tierra a escala (radio = earthRadiusModel)
    public float earthRadiusModel = 4f;
    public float timeScale = 600f;   // un periodo orbital bajo (~88 min) se ve en ~9 s

    const double GM = 3.986004e14, R = 6.371e6, Altitude = 2e5;   // m; la montaña de Newton, por encima del aire

    float _speedKms = 5f;
    bool _flying;
    double _x, _y, _vx, _vy, _t, _angle;
    Transform _ball;
    LineRenderer _trail;
    readonly List<Vector3> _pts = new List<Vector3>();
    string _result = "Elige la velocidad (↑/↓) y dispara (E).";

    protected override void OnBegin()
    {
        if (_ball == null)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(b.GetComponent<Collider>());
            b.name = "Bala_de_Newton"; b.transform.SetParent(transform, false); b.transform.localScale = Vector3.one * 0.25f;
            _ball = b.transform;
            _trail = b.AddComponent<LineRenderer>();
            _trail.widthMultiplier = 0.06f; _trail.useWorldSpace = true;
            _trail.material = new Material(Shader.Find("Sprites/Default"));
            _trail.startColor = _trail.endColor = new Color(1f, 0.85f, 0.3f);
        }
        _ball.gameObject.SetActive(false);
    }

    protected override void Tick()
    {
        if (!_flying)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) _speedKms = Mathf.Min(12f, _speedKms + 0.25f);
            if (Input.GetKeyDown(KeyCode.DownArrow)) _speedKms = Mathf.Max(1f, _speedKms - 0.25f);
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)) Fire();
            return;
        }
        double simDt = Time.deltaTime * timeScale;
        int n = Mathf.Max(1, Mathf.CeilToInt((float)(simDt / 2.0)));
        double h = simDt / n;
        for (int i = 0; i < n && _flying; i++) Integrate(h);
        Place();
    }

    void Fire()
    {
        _x = 0; _y = R + Altitude; _vx = _speedKms * 1000.0; _vy = 0; _t = 0; _angle = 0;
        _flying = true; _pts.Clear();
        _ball.gameObject.SetActive(true);
        _result = $"¡Fuego a {_speedKms:0.00} km/s!";
        Place();
    }

    void Integrate(double h)
    {
        // Verlet de velocidad: estable para órbitas.
        double r3 = System.Math.Pow(_x * _x + _y * _y, 1.5);
        double ax = -GM * _x / r3, ay = -GM * _y / r3;
        double prevAng = System.Math.Atan2(_x, _y);
        _x += _vx * h + 0.5 * ax * h * h; _y += _vy * h + 0.5 * ay * h * h;
        double r3b = System.Math.Pow(_x * _x + _y * _y, 1.5);
        _vx += 0.5 * (ax - GM * _x / r3b) * h; _vy += 0.5 * (ay - GM * _y / r3b) * h;
        _t += h;
        double ang = System.Math.Atan2(_x, _y), d = ang - prevAng;
        if (d < -System.Math.PI) d += 2 * System.Math.PI;
        _angle += d;
        double r = System.Math.Sqrt(_x * _x + _y * _y);
        if (r <= R)
        {
            _flying = false;
            double km = _angle * R / 1000.0;
            _result = $"Cayó a {km:0} km de la montaña, tras {_t / 60.0:0} min. Más rápido: cae más lejos, porque la Tierra es redonda y se curva bajo la bala.";
        }
        else if (_angle >= 2 * System.Math.PI)
        {
            _flying = false;
            _result = $"¡Dio la vuelta entera sin tocar el suelo en {_t / 60.0:0} min! Está en ÓRBITA: cae todo el rato, pero la Tierra se curva igual de deprisa. Así está la Luna (y ahora sabes por qué no se cae).";
            Complete("«Si el disparo es lo bastante rápido, la bala nunca vuelve a la Tierra: gira alrededor como la Luna.»");
        }
        else if (r > 6 * R)
        {
            _flying = false;
            double v2 = _vx * _vx + _vy * _vy, energy = 0.5 * v2 - GM / r;   // energía por kg: ≥ 0 = no vuelve nunca
            if (energy >= 0)
                _result = $"Se fue: a {_speedKms:0.00} km/s supera la velocidad de escape (11,2 km/s en el suelo; algo menos aquí arriba). La gravedad nunca la frena del todo.";
            else
            {
                double a = -GM / (2 * energy), period = 2 * System.Math.PI * System.Math.Sqrt(a * a * a / GM);
                _result = $"Una ÓRBITA muy alargada (una elipse, como las de Kepler): sube lejísimos y volvería a la montaña en {period / 3600.0:0.0} h. Prueba más despacio para verla entera.";
                Complete("«Las órbitas son elipses: la misma gravedad explica la manzana, la Luna y los planetas.»");
            }
        }
    }

    void Place()
    {
        if (earth == null || _ball == null) return;
        float s = (float)(earthRadiusModel / R);
        Vector3 p = earth.position + earth.right * (float)_x * s + earth.up * (float)_y * s;
        _ball.position = p;
        if (_pts.Count == 0 || (_pts[_pts.Count - 1] - p).sqrMagnitude > 0.01f) _pts.Add(p);
        _trail.positionCount = _pts.Count;
        _trail.SetPositions(_pts.ToArray());
    }

    protected override void Draw(Rect a)
    {
        GUILayout.BeginArea(a);
        GUILayout.Label("Newton imagina una montaña tan alta que asoma por encima del aire, con un cañón en la cima.", Wrap);
        GUILayout.Label($"Velocidad de la bala: <b>{_speedKms:0.00} km/s</b>  (↑/↓ cambia · E dispara · Esc cierra)", Wrap);
        GUILayout.Label(_result, Wrap);
        if (Done) GUILayout.Label("<i>Lo mismo que hace caer la manzana sostiene a la Luna: la gravitación universal, F = G·m·M/r².</i>", Wrap);
        GUILayout.EndArea();
    }
}
