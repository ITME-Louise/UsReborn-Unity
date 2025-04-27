using UnityEditor;
using UnityEngine;

public class URPMaterialConverter
{
    [MenuItem("Tools/Convert Materials to URP")]
    static void ConvertMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.shader.name == "Standard")
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                Debug.Log("Converted: " + mat.name);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Material conversion to URP complete!");
    }
}
