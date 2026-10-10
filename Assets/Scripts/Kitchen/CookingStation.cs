using UnityEngine;

/// <summary>
/// ESTACIÓN DE COCINA de UNA anima (docs/kitchen-simulation.md): toma un platillo asignado por el
/// <see cref="RestaurantKitchen"/> y **recorre sus pasos**. Si `playerDriven`, el jugador **avanza cada paso con una
/// tecla** (jugable, con HUD); si no, el NPC avanza solo por tiempo. Al terminar un platillo lo **entrega al stock** y
/// pide el siguiente que el restaurante necesite (equilibrio). Varias estaciones = varias animas cocinando a la vez.
/// Prototipo: pasos por tecla/tiempo + HUD OnGUI; la versión con **estaciones manipulables reales** (Virtualization
/// `StationPart`: ir a la nevera, agarrar…) necesita prefabs (Unity) — ver docs/unity-editor-manual.md.
/// </summary>
public class CookingStation : MonoBehaviour
{
    public RestaurantKitchen kitchen;
    [Tooltip("true = el JUGADOR avanza cada paso con `stepKey` (jugable). false = NPC automático por tiempo.")]
    public bool playerDriven = false;
    public KeyCode stepKey = KeyCode.E;
    [Tooltip("Segundos por paso cuando es NPC (automático).")]
    [Min(0.1f)] public float npcStepSeconds = 1.2f;
    [Tooltip("HUD OnGUI para el cocinero jugador.")]
    public Vector2 hudOrigin = new Vector2(360f, 12f);

    CookingRecipe _recipe;
    int _step;
    float _next;
    GUIStyle _style;

    void Start()
    {
        if (kitchen == null) kitchen = FindObjectOfType<RestaurantKitchen>();
        TakeAssignment();
    }

    void TakeAssignment()
    {
        _recipe = kitchen != null ? kitchen.AssignNext(this) : null;
        _step = 0;
        _next = Time.time + npcStepSeconds;
    }

    void Update()
    {
        if (kitchen == null) return;
        if (_recipe == null)                      // todo cubierto (o aún no asignado): reintenta de vez en cuando
        {
            if (Time.time >= _next) { _next = Time.time + 2f; TakeAssignment(); }
            return;
        }

        bool advance = playerDriven ? Input.GetKeyDown(stepKey) : Time.time >= _next;
        if (advance) AdvanceStep();
    }

    /// <summary>Ejecuta el paso actual (lo llama la tecla, el tiempo del NPC, o la consola de hechizos "step"/"cook").
    /// Al terminar la receta, deposita en el stock y pide el siguiente platillo que haga falta.</summary>
    public void AdvanceStep()
    {
        if (_recipe == null) return;
        if (_step < _recipe.steps.Length)
            Debug.Log($"[Cocina] «{name}» {_recipe.dishName}: {_recipe.steps[_step]}");
        _step++;
        _next = Time.time + npcStepSeconds;

        if (_step >= _recipe.steps.Length)        // emplatado → al stock → siguiente platillo que haga falta
        {
            kitchen.Deliver(_recipe.dishName, this);
            TakeAssignment();
        }
    }

    void OnGUI()
    {
        if (!playerDriven) return;
        if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true, wordWrap = true };

        var sb = new System.Text.StringBuilder();
        if (_recipe == null) sb.AppendLine("<b>Restaurante</b>\nTodo el stock está cubierto. ¡Misión lista!");
        else
        {
            sb.AppendLine($"<b>Cocinando: {_recipe.dishName}</b>  <size=11>(pulsa {stepKey} para el siguiente paso)</size>");
            for (int i = 0; i < _recipe.steps.Length; i++)
            {
                string mark = i < _step ? "✔" : (i == _step ? "➤" : "·");
                string color = i < _step ? "7CFC7C" : (i == _step ? "FFD700" : "AAAAAA");
                sb.AppendLine($"<color=#{color}>{mark} {_recipe.steps[i]}</color>");
            }
        }
        // Stock del restaurante
        if (kitchen != null && kitchen.dishes != null)
        {
            sb.AppendLine("<b>Stock</b>");
            foreach (RestaurantKitchen.DishStock d in kitchen.dishes)
            {
                string dn = d.container != null ? d.container.dishName : (d.recipe != null ? d.recipe.dishName : "?");
                sb.AppendLine($"  {dn}: {d.Have}/{d.target}");
            }
        }
        GUI.Label(new Rect(hudOrigin.x, hudOrigin.y, 320f, 320f), sb.ToString(), _style);
    }
}
