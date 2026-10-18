using UnityEngine;

/// <summary>
/// `GetComponent&lt;T&gt;() ?? AddComponent&lt;T&gt;()` NO funciona en el editor: si falta el componente, GetComponent
/// devuelve un "null falso" (un objeto que == null pero que `??` no considera nulo) y nunca se añade. Usar esto.
/// </summary>
public static class ComponentExt
{
    public static T GetOrAdd<T>(this GameObject g) where T : Component => g.TryGetComponent(out T c) ? c : g.AddComponent<T>();
    public static T GetOrAdd<T>(this Component c) where T : Component => c.gameObject.GetOrAdd<T>();
}
