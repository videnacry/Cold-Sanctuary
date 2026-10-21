using UnityEngine;

/// <summary>
/// ARRANQUE DE UNA ESCENA DE VIRTUALIZACIÓN (Virtual_&lt;clave&gt;): la escena trae una COPIA del área real del santuario
/// (sus muebles, con los mismos nombres) y, al cargarse, monta la misión sobre ella (pasos guiados, el turista de la
/// iniciación, el servicio del restaurante…) y trae COPIAS de la gente del santuario que trabaja en esa área. Cada
/// virtualización tiene así su propia escena (se desarrolla por separado) pero reutiliza el área y los personajes.
/// </summary>
public class VirtualSceneBootstrap : MonoBehaviour
{
    public string key, title, area;
    public Transform areaRoot, spawn;
    public string[] staff = new string[0];   // quién trabaja en esta área (se copian sus cuerpos)

    void Start()
    {
        WorkRoster.Mission m = VirtualMissionRunner.Pending;
        if (m == null || VirtualMissions.Key(m.id) != key)
        {
            foreach (WorkRoster.Mission c in AreaWork.Catalog) if (VirtualMissions.Key(c.id) == key) { m = c.Copy(); break; }
            if (m == null) m = new WorkRoster.Mission(area, key, title, 1f, false);
        }
        Transform where = areaRoot != null ? areaRoot : transform;
        if (m.id == "coc_servicio")
        {
            RestaurantServiceGame g = RestaurantServiceGame.StartInRestaurant(where, null, m);
            if (g != null) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(g.gameObject, gameObject.scene);   // se va con la escena
        }
        else
        {
            VirtualInPlace.Begin(m, where, spawn != null ? spawn : transform);
            if (VirtualMissionRunner.Active != null)
            {
                VirtualMissionRunner.Active.inPlace = false;   // al terminar: salir de la escena
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(VirtualMissionRunner.Active.gameObject, gameObject.scene);
            }
        }
        // La gente del área (copias de sus cuerpos), en su sitio de trabajo.
        for (int i = 0; i < staff.Length; i++)
        {
            GameObject c = WorkdayDirector.SpawnBodyCopy(staff[i], staff[i]);
            if (c == null) continue;
            c.transform.SetParent(transform, true);
            float a = i * 2.4f;
            c.transform.position = where.position + new Vector3(Mathf.Cos(a) * 4f, 0.1f, Mathf.Sin(a) * 4f);
            c.transform.rotation = Quaternion.LookRotation(new Vector3(-Mathf.Cos(a), 0f, -Mathf.Sin(a)));
        }
        Debug.Log($"[Virtualización] Escena «{key}»: copia del área {area} y {staff.Length} compañeros del santuario.");
    }
}
