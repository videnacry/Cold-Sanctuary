using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ARTICULACIÓN de un cuerpo de cubos: el Transform de este GameObject es el PIVOTE (hombro, codo, rodilla…), así que
/// rotarlo gira el segmento desde su articulación y arrastra a todo lo que cuelga de él. El cubo visible (`segment`)
/// es su hijo. <see cref="SetStretch"/> ESTIRA o COMPRIME el segmento a lo largo de su eje (Y local) y desplaza las
/// articulaciones hijas para que el miembro siga unido. Lo crea <see cref="CubeHumanoid"/>.
/// </summary>
public class CubeJoint : MonoBehaviour
{
    public BodyPart part;
    public Transform segment;
    [SerializeField] Vector3 _segBaseScale, _segBasePos;
    [SerializeField] List<Transform> _childJoints = new List<Transform>();
    [SerializeField] List<Vector3> _childBasePos = new List<Vector3>();
    [SerializeField] Quaternion _restRotation = Quaternion.identity;
    float _stretch = 1f;

    public float Stretch => _stretch;
    public Quaternion RestRotation => _restRotation;

    /// <summary>Registra el estado de reposo (tras construir el cuerpo).</summary>
    public void Capture()
    {
        _restRotation = transform.localRotation;
        if (segment != null) { _segBaseScale = segment.localScale; _segBasePos = segment.localPosition; }
        _childJoints.Clear(); _childBasePos.Clear();
        foreach (Transform c in transform)
            if (c.GetComponent<CubeJoint>() != null) { _childJoints.Add(c); _childBasePos.Add(c.localPosition); }
    }

    static Vector3 AlongY(Vector3 v, float s) => new Vector3(v.x, v.y * s, v.z);   // solo a lo largo del segmento

    /// <summary>1 = longitud normal; &gt;1 estira, &lt;1 comprime (0.5–1.6). Las articulaciones hijas se desplazan con él.</summary>
    public void SetStretch(float factor)
    {
        _stretch = Mathf.Clamp(factor, 0.5f, 1.6f);
        if (segment != null)
        {
            segment.localScale = new Vector3(_segBaseScale.x, _segBaseScale.y * _stretch, _segBaseScale.z);
            segment.localPosition = AlongY(_segBasePos, _stretch);
        }
        for (int i = 0; i < _childJoints.Count; i++)
            if (_childJoints[i] != null) _childJoints[i].localPosition = AlongY(_childBasePos[i], _stretch);
    }
}
