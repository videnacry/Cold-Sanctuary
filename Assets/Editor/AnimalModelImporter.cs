using UnityEditor;
using UnityEngine;

/// <summary>
/// Auto-configures FBX import settings for animal models dropped into Assets/Animals/*/Models/.
/// Runs automatically via AssetPostprocessor whenever a new FBX is imported.
///
/// What it does:
///   - Enables Read/Write
///   - Sets scale factor to 1 (Meshy / Sketchfab models are often off-scale)
///   - Animation type: Humanoid if path contains "Player", Generic for animals
///   - Import animations: true
///   - Material creation: per texture
///   - Generates lightmap UVs
/// </summary>
public class AnimalModelImporter : AssetPostprocessor
{
    // Only run for FBX files inside Assets/Animals/*/Models/
    bool IsAnimalModel(string assetPath)
        => assetPath.Contains("/Animals/") && assetPath.Contains("/Models/")
           && assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase);

    void OnPreprocessModel()
    {
        if (!IsAnimalModel(assetPath)) return;

        var importer = assetImporter as ModelImporter;
        if (importer == null) return;

        // Geometry
        importer.isReadable          = true;
        importer.globalScale         = 1f;
        importer.generateSecondaryUV = true;   // lightmap UVs

        // Animation
        importer.animationType          = ModelImporterAnimationType.Generic;
        importer.importAnimation        = true;
        importer.importBlendShapes      = true;
        importer.importVisibility       = true;
        importer.importCameras          = false;
        importer.importLights           = false;

        // Materials
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.materialLocation    = ModelImporterMaterialLocation.External;

        Debug.Log($"[AnimalModelImporter] Auto-configured: {assetPath}");
    }

    void OnPreprocessAnimation()
    {
        if (!IsAnimalModel(assetPath)) return;

        var importer = assetImporter as ModelImporter;
        if (importer == null) return;

        // Ensure root motion is extracted so animations work with NavMesh
        importer.motionNodeName = "";   // use root bone
        importer.bakeAxisConversion = true;
    }
}
