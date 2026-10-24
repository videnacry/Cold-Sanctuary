using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SEÑAL DE CORTEJO a distancia (docs/era-species-behavior.md §3): la LUCIÉRNAGA destella y la CIGARRA canta. La da el
/// hábito «Signal» del <see cref="Ethogram"/> (posado en un tallo/tronco) y SOLO si el ser está en celo
/// (<see cref="EstrusState"/>): un macho saciado y en celo llama; uno hambriento come; uno joven no llama. Así el coro sale
/// de los stats de cada individuo, no de un temporizador global.
///
/// - **Destellos** (Luciola): el macho emite un destello corto ~1 por segundo; la HEMBRA en celo que ve uno contesta con
///   un destello propio tras un retardo fijo, y el macho va hacia ella (Lewis &amp; Cratsley 2008, Annu Rev Entomol 53:293;
///   el órgano de luz adulto: Fu &amp; Zhu 2024, Nat Commun 15:1736, doi 10.1038/s41467-024-45559-7). Se ve: una luz
///   verde-amarilla (~560 nm) en el abdomen. La luz artificial apaga los cortejos (Firebaugh &amp; Haynes 2016, Oecologia
///   182:1203, doi 10.1007/s00442-016-3723-1).
/// - **Canto** (Cicada orni): el macho canta con los timbales en las horas de calor desde los troncos, posado bajo
///   (Sueur &amp; Aubin 2003, doi 10.1007/s00114-003-0432-5); la hembra tiene el tímpano afinado a la frecuencia del canto
///   del macho (Sueur et al. 2008, J Exp Biol 211:2379, doi 10.1242/jeb.018804) y va hacia él. Se oye: un zumbido de
///   timbal sintetizado (sin assets de audio), en 3D.
///
/// Las hembras en celo buscan al que llama con <see cref="NearestCaller"/> (deseo «mate» de <see cref="DesireCatalog"/>).
/// </summary>
[RequireComponent(typeof(Animal))]
public class MatingSignal : MonoBehaviour
{
    public enum Kind { None, Flash, Song }

    public static Kind KindOf(string species) => species switch
    {
        "Firefly" => Kind.Flash,
        "Cicada"  => Kind.Song,
        _         => Kind.None,
    };

    static readonly List<MatingSignal> _callers = new List<MatingSignal>();

    public Kind Mode { get; private set; }
    /// <summary>Llamando ahora (el hábito «Signal» lo renueva cada frame).</summary>
    public bool Calling => Time.time < _until;

    Animal _a;
    EstrusState _estrus;
    float _until, _phase, _answerAt = -1f;
    Light _light;
    AudioSource _audio;
    static AudioClip _song;

    const float FLASH_PERIOD = 1.0f, FLASH_ON = 0.2f, ANSWER_DELAY = 0.5f;   // Luciola: ~1 destello/s; la hembra contesta con retardo [A]

    void Awake()
    {
        _a = GetComponent<Animal>(); _estrus = GetComponent<EstrusState>(); Mode = KindOf(_a != null ? _a.SpeciesName : null);
        Transform old = FindDeep(transform, "Destello");   // una cría es copia de su madre: no hereda su farolillo encendido
        if (old != null) Destroy(old.gameObject);
    }
    void OnEnable() { if (!_callers.Contains(this)) _callers.Add(this); }
    void OnDisable() { _callers.Remove(this); SetLight(false); if (_audio != null) _audio.Stop(); }

    bool InEstrus => _estrus != null && _estrus.InEstrus && _a != null && _a.lifeStage == LifeStage.adult && !_a.death;

    /// <summary>Lo llama el hábito «Signal» cada frame. Devuelve false si no toca llamar (no está en celo o es hembra que
    /// solo contesta): entonces el hábito cuenta como descanso.</summary>
    public bool Display()
    {
        if (Mode == Kind.None || !InEstrus || _a.sex != Sex.male) return false;
        _until = Time.time + 0.5f;
        if (Time.frameCount % 20 == 0) TraceField.Leave(transform.position, TraceChannel.Estrus, 1f);   // también deja el rastro del celo
        return true;
    }

    /// <summary>El macho de su especie que llama más cerca (dentro de lo que <paramref name="female"/> percibe), o null.</summary>
    public static MatingSignal NearestCaller(Animal female, float radius)
    {
        if (female == null) return null;
        MatingSignal best = null; float bd = radius * radius;
        foreach (MatingSignal m in _callers)
        {
            if (m == null || !m.Calling || m._a == female || m._a.SpeciesName != female.SpeciesName) continue;
            float d = (m.transform.position - female.transform.position).sqrMagnitude;
            if (d < bd) { bd = d; best = m; }
        }
        return best;
    }

    void Update()
    {
        if (Mode == Kind.Flash) UpdateFlash();
        else if (Mode == Kind.Song) UpdateSong();
    }

    void UpdateFlash()
    {
        if (Calling)
        {
            _phase = (_phase + Time.deltaTime) % FLASH_PERIOD;
            SetLight(_phase < FLASH_ON);
            return;
        }
        // HEMBRA en celo: contesta al destello de un macho que ve (no vuela tras él: él viene).
        if (InEstrus && _a.sex == Sex.female)
        {
            MatingSignal m = NearestCaller(_a, 40f * Mathf.Max(0.3f, _a.perception));
            if (m != null && m.FlashOn && _answerAt < 0f) _answerAt = Time.time + ANSWER_DELAY;
            if (_answerAt > 0f && Time.time >= _answerAt) { _answerAt = -1f; Invoke(nameof(LightOff), FLASH_ON); SetLight(true); }
            return;
        }
        if (_light != null && _light.enabled && !IsInvoking(nameof(LightOff))) SetLight(false);
    }

    void LightOff() => SetLight(false);
    bool FlashOn => _light != null && _light.enabled;

    void SetLight(bool on)
    {
        if (on && _light == null)
        {
            var g = new GameObject("Destello");
            Transform lantern = FindDeep(transform.Find("Insecto"), "Linterna");   // el órgano de luz del cuerpo de cubos
            g.transform.SetParent(lantern != null ? lantern : transform, false);
            g.transform.localPosition = lantern != null ? Vector3.zero : new Vector3(0f, 0.2f, -0.35f);   // si no, el abdomen
            _light = g.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = new Color(0.75f, 1f, 0.25f);   // ~560 nm, verde-amarillo de Luciola
            _light.intensity = 3f;
            _light.shadows = LightShadows.None;
            var bulb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(bulb.GetComponent<Collider>());
            bulb.name = "Organo_luminoso";
            bulb.transform.SetParent(g.transform, false);
            bulb.transform.localScale = Vector3.one * 0.15f;
            var r = bulb.GetComponent<Renderer>();
            r.material.color = _light.color;
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", _light.color * 4f);
        }
        if (_light == null) return;
        _light.range = 6f * Mathf.Max(0.1f, transform.lossyScale.y) + 1f;
        _light.enabled = on;
        _light.transform.GetChild(0).gameObject.SetActive(on);
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root == null) return null;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
        return null;
    }

    void UpdateSong()
    {
        if (!Calling) { if (_audio != null && _audio.isPlaying) _audio.Stop(); return; }
        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.clip = SongClip();
            _audio.loop = true;
            _audio.spatialBlend = 1f;
            _audio.rolloffMode = AudioRolloffMode.Logarithmic;
            _audio.minDistance = 2f; _audio.maxDistance = 60f;
            _audio.volume = 0.25f;
        }
        if (!_audio.isPlaying) { _audio.pitch = Random.Range(0.95f, 1.05f); _audio.Play(); }
    }

    // Canto de timbal sintetizado: ráfagas de pulsos (cada pulso = un clic del timbal) con portadora aguda y ruido, en
    // «equemas» separados por pausas cortas. Frecuencias y ritmos orientativos [A]: el canto real de C. orni se calibra con
    // grabaciones (Sueur & Aubin 2003; Fonseca & Revez 2002, J Exp Biol 205:1285, doi 10.1242/jeb.205.9.1285).
    static AudioClip SongClip()
    {
        if (_song != null) return _song;
        const int rate = 22050; int n = rate * 2;
        var data = new float[n];
        var rng = new System.Random(7);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float echeme = (t % 0.25f) < 0.17f ? 1f : 0f;               // equema ~0,17 s, pausa ~0,08 s
            float pulse = Mathf.Exp(-((t * 220f) % 1f) * 6f);           // ~220 clics de timbal por segundo
            float carrier = Mathf.Sin(2f * Mathf.PI * 3800f * t) * 0.6f + ((float)rng.NextDouble() * 2f - 1f) * 0.4f;
            data[i] = echeme * pulse * carrier * 0.5f;
        }
        _song = AudioClip.Create("CantoCigarra", n, 1, rate, false);
        _song.SetData(data, 0);
        return _song;
    }
}
