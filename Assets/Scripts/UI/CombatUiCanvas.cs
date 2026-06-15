using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗/对话 UI 共用 Canvas，与剧情场景保持一致的缩放基准。
/// </summary>
public static class CombatUiCanvas
{
    public static readonly Vector2 ReferenceResolution = new Vector2(968f, 544.67f);

    public static Canvas GetOrCreate(int sortingOrder = 100)
    {
        Canvas canvas = FindCombatCanvas();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("CombatUI");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        ConfigureScaler(canvas);
        canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, sortingOrder);
        return canvas;
    }

    private static Canvas FindCombatCanvas()
    {
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.gameObject.name == "CombatUI")
                return canvas;
        }

        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
        }

        return null;
    }

    public static void ConfigureScaler(Canvas canvas)
    {
        if (canvas == null)
            return;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight = 0.5f;
    }
}
