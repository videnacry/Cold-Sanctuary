using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BunnyBehavior : Animal
{
    protected override string SpeciesArchetype => "Bunny";

    #region Family
    /// <summary>
    /// Properties wich determine how is going te be the created family of an instance
    /// </summary>
    /*
    public override char ParentalCare { get; set; } = Family.maternal;
    public override float ParentsRate { get; set; } = 0.14f;
    public override byte FamilySize { get; set; } = 4;
    */

    #endregion

    #region Physiognomy
    /// <summary>
    /// Field with property wich contains the base value for new instances
    /// </summary>
    // Escala medida contra el mesh crudo (ver AnimalPrefabGenerator > Measure Raw Animal Sizes):
    // altura cruda 23.52m (el FBX vino en una unidad ~100x mas grande de lo esperado)
    // -> objetivo realista de altura adulta ~0.25m.
    #endregion

    
    

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
    }

   

    protected override void ConfigureThreat(ThreatResponder t) { t.aggressiveness = 0f; t.canHitAndRun = true; }
}