using System.Collections.Generic;
using UnityEngine;

// (En su propio archivo: Unity solo serializa en la escena un MonoBehaviour cuyo archivo se llama como la clase; si no, en Play queda «Missing Script».)
/// <summary>Lo que el anima sostiene en las manos al cocinar (una cosa a la vez). Se añade solo.</summary>
public class KitchenHands : MonoBehaviour
{
    public string held;
    public static KitchenHands Of(Anima self) => self.GetOrAdd<KitchenHands>();
}
