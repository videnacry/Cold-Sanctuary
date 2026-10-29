using UnityEngine;

/// <summary>Hechizo «piso»: anda (con <see cref="PathMover"/>, que entra en la raíz cuando toca) hasta la puerta del nivel
/// pedido. Sujeta el cuerpo del jugador mientras va (la cámara sigue libre). Con límite de tiempo.</summary>
public class TreeWalk : MonoBehaviour
{
    ArbolCampos _tree; TreeLevel _to; float _until; bool _held;

    public static void Go(GameObject who, ArbolCampos tree, TreeLevel to)
    {
        TreeWalk w = who.GetOrAdd<TreeWalk>();
        w._tree = tree; w._to = to; w._until = Time.time + 300f; w.enabled = true;
        if (!w._held && who.TryGetComponent(out PlayerController pc)) { pc.HoldBody(true); w._held = true; }
        PathMover.Of(who.transform).Restart();
    }

    void Update()
    {
        if (_tree == null || _to == null || Time.time > _until) { Stop(); return; }
        PathMover pm = PathMover.Of(transform);
        Vector3 target = _tree.World(_to.outside) + Vector3.up * pm.PivotHeight;
        if (_tree.LevelAt(transform.position) == _to && !TreeTrip.Busy(gameObject)) { Stop(); return; }
        pm.StepToward(target, 2.2f);
    }

    void Stop()
    {
        if (_held && TryGetComponent(out PlayerController pc)) pc.HoldBody(false);
        _held = false; enabled = false;
    }
}

