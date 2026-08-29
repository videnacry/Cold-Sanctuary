using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Family
{
    // Parental care
    public const char paternal = 'p', maternal = 'm', biparental = 'b';

    public Animal[] members;
    public Animal[] fed;
    public Animal[] feeders;
    public Animal alphaMale;
    public Animal alphaFemale;
    public char parentalCare;
    public float parentsRate;
    public byte familySize;
    public Family(byte pFamilySize, float pParentsRate, char pParentalCare)
    {
        this.familySize = pFamilySize;
        this.parentsRate = pParentsRate;
        this.parentalCare = pParentalCare;
    }

    // Estructura familiar por especie (etapa 5): antes un `defaultGroup` estático por clase, ahora DATA. Devuelve una
    // COPIA (cada ser arranca con la suya; RenderFamily la reemplaza). Desconocida → familia maternal pequeña.
    static readonly Family _default = new Family(4, 0.3f, maternal);
    static Dictionary<string, Family> _catalog;
    static void BuildCatalog() => _catalog = new Dictionary<string, Family>
    {
        { "Fox",  new Family(7, 0.4f,  biparental) }, { "Bear",     new Family(3, 0.4f,  maternal) },
        { "Wolf", new Family(6, 0.3f,  biparental) }, { "Malamute", new Family(6, 0.3f,  maternal) },
        { "Bunny",new Family(5, 0.4f,  maternal)   }, { "Whale",    new Family(10, 0.25f, maternal) },
        { "Seal", new Family(8, 0.3f,  maternal)   }, { "Deer",     new Family(6, 0.3f,  maternal) },
        // Insectos
        { "Ant",     new Family(6, 0.8f, maternal) },  // colonia; reinas = parents
        { "Aphid",   new Family(4, 0.6f, maternal) },  // racimo de clones (partenogénesis aproximada)
        { "Ladybug", new Family(2, 0.3f, maternal) },  // semi-solitaria
        { "Spider",  new Family(1, 0.1f, maternal) },  // solitaria (canibalismo mata al macho)
        { "Cricket", new Family(3, 0.3f, maternal) },  // grupos pequeños
    };

    public static Family Of(string species)
    {
        if (_catalog == null) BuildCatalog();
        Family f = species != null && _catalog.TryGetValue(species, out Family v) ? v : _default;
        return new Family(f.familySize, f.parentsRate, f.parentalCare);
    }

    public static GameObject[] RenderGroup(GameObject animal, int quantity, Vector3 position, float height, float radius = 0)
    {
        if (radius == 0) radius = quantity * 2;
        Vector3 maxPos = new Vector3(position.x + radius, position.y + radius, position.z + radius);
        Vector3 minPos = new Vector3(position.x - radius, position.y - radius, position.z - radius);
        GameObject[] creatures = new GameObject[quantity];
        Vector3[] positions = new Vector3[quantity];
        int counter = 0;
        for (int idx = -(quantity / 2); quantity / 2 > idx; idx += 1)
        {
            float xPos = Random.Range(minPos.x, maxPos.x);
            float zPos = Random.Range(minPos.z, maxPos.z);
            positions[counter] = new Vector3(xPos, height, zPos);
            counter++;
        }
        for (int idx = 0; quantity > idx; idx++)
        {
            GameObject creature = MonoBehaviour.Instantiate(animal, positions[idx], animal.transform.rotation);
            creatures[idx] = creature;
        }
        return creatures;
    }
    public static Animal[] SetGendersRate (GameObject[] creatures, float rate, char sex)
    {
        Animal[] scripts = new Animal[creatures.Length];
        for (int idx = 0; creatures.Length > idx; idx++)
        {
            scripts[idx] = creatures[idx].GetComponent<Animal>();
            scripts[idx].sex = Sex.SwitchSex(sex);
            if (Random.Range(0.0f, 1.0f) < rate)
                scripts[idx].sex = sex;
        }
        return scripts;
    }
    public static Animal[] SetParents(Animal[] scripts, byte familySize, float parentsRandomRate, int minParentsCount, char parentalCare)
    {
        float parentsCount = 0;
        char parentalSex = Sex.female;
        bool alphaMaleSetted = false, alphaFemaleSetted = false;
        // familySize/parentalCare vienen por parámetro (no de scripts[0].Group): las criaturas recién
        // Instantiate()adas todavía no corrieron su propio Start()/Init() (Unity lo difiere), así que su
        // Group sigue sin fijar en este mismo frame — leerlo acá tiraba NullReferenceException.
        Family family = new Family(familySize, parentsRandomRate, parentalCare);
        HashSet<Animal> adults = new HashSet<Animal>();
        HashSet<Animal> children = new HashSet<Animal>();
        foreach (Animal script in scripts)
        {
            script.Group = family;
            if (minParentsCount > parentsCount || parentsRandomRate > (parentsCount / scripts.Length))
            {
                adults.Add(script);
                parentsCount++;
                script.lifeStage = LifeStage.adult;
                script.sex = parentalSex;
                if (!alphaFemaleSetted) 
                { 
                    family.alphaFemale = script;
                    alphaFemaleSetted = true;
                } else if (!alphaMaleSetted)
                {
                    family.alphaMale = script;
                    alphaMaleSetted = true;
                }
                parentalSex = Sex.SwitchSex(parentalSex);
            } else
            {
                script.lifeStage = LifeStage.child;
                children.Add(script);
            }
        }
        family.members = scripts;
        family.fed = children.ToArray();
        family.feeders = adults.ToArray();
        return scripts;
    }
    public static Animal[] RenderFamily(GameObject animal, int quantity, float parentsRandomRate, int minParentsCount, char parentalCare, Vector3 position, float height, float radius = 0)
    {
        Animal[] scripts = SetGendersRate(RenderGroup(animal, quantity, position, height, radius), 0.5f, Sex.female);
        scripts = SetParents(scripts, (byte)quantity, parentsRandomRate, minParentsCount, parentalCare);
        return scripts;
    }
}