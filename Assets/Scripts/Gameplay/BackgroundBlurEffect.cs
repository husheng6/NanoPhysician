using UnityEngine;

/// <summary>
/// 给关卡背景施加虚化效果。
/// </summary>
public static class BackgroundBlurEffect
{
    private static Material sharedMaterial;

    public static void ApplyToMap(Transform mapRoot, float blurSize = 1.6f, float dimAlpha = 0.94f)
    {
        if (mapRoot == null)
            return;

        Material material = GetOrCreateMaterial(blurSize, dimAlpha);
        SpriteRenderer[] renderers = mapRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.material = material;
            renderer.color = new Color(0.9f, 0.9f, 0.95f, dimAlpha);
        }
    }

    private static Material GetOrCreateMaterial(float blurSize, float dimAlpha)
    {
        if (sharedMaterial == null)
        {
            Shader shader = Shader.Find("NanoPhysician/BackgroundBlur");
            if (shader == null)
                return null;

            sharedMaterial = new Material(shader);
        }

        sharedMaterial.SetFloat("_BlurSize", blurSize);
        sharedMaterial.SetColor("_Color", new Color(0.88f, 0.88f, 0.92f, dimAlpha));
        return sharedMaterial;
    }
}
