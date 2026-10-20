using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CACHÉ DE BÚSQUEDAS EN ESCENA. <c>FindObjectsOfType</c> recorre TODOS los objetos (la escena tiene decenas de miles):
/// hecho en cada frame por varios scripts era lo que hundía el FPS. Aquí la lista de cada tipo se comparte y se refresca
/// como mucho cada <c>maxAge</c> segundos (los objetos destruidos entre medias llegan como null: comprobarlo).
/// </summary>
public static class SceneCache
{
    static readonly Dictionary<Type, (UnityEngine.Object[] items, float at, int frame)> _cache = new Dictionary<Type, (UnityEngine.Object[], float, int)>();

    public static T[] All<T>(float maxAge = 1f) where T : UnityEngine.Object
    {
        if (_cache.TryGetValue(typeof(T), out var e) && Time.time - e.at < maxAge && e.frame <= Time.frameCount && e.items is T[] typed)
            return typed;
        T[] found = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        _cache[typeof(T)] = (found, Time.time, Time.frameCount);
        return found;
    }

    /// <summary>Fuerza a re-buscar el tipo la próxima vez (al crear/destruir algo que importa ya).</summary>
    public static void Invalidate<T>() => _cache.Remove(typeof(T));
}
