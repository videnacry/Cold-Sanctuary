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
    public static Animal[] SetParents(Animal[] scripts,float parentsRandomRate, int minParentsCount)
    {
        float parentsCount = 0;
        char parentalSex = Sex.female;
        bool alphaMaleSetted = false, alphaFemaleSetted = false;
        Family family = new Family(scripts[0].Group.familySize, scripts[0].Group.parentsRate, scripts[0].Group.parentalCare);
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
    public static Animal[] RenderFamily(GameObject animal, int quantity, float parentsRandomRate, int minParentsCount, char parentalCare, Vector3 position, float height)
    {
        Animal[] scripts = SetGendersRate(RenderGroup(animal, quantity, position, height), 0.5f, Sex.female);
        scripts = SetParents(scripts, parentsRandomRate, minParentsCount);
        return scripts;
    }
}