using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 剧情对话 UI 共用：全屏背景、预制体加载等。
/// </summary>
public static class StoryDialogueUiHelper
{
    public static GameObject CreateFullScreenBackground(Transform parent, Sprite sprite)
    {
        GameObject background = new GameObject("StoryBackground");
        background.transform.SetParent(parent, false);
        background.transform.SetAsFirstSibling();

        RectTransform rect = background.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = background.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = false;
        image.color = sprite != null ? Color.white : Color.black;
        image.raycastTarget = false;
        return background;
    }

    public static void UpdateBackgroundImage(GameObject backgroundObject, Sprite sprite)
    {
        if (backgroundObject == null)
            return;

        Image image = backgroundObject.GetComponent<Image>();
        if (image == null)
            return;

        image.sprite = sprite;
        image.color = sprite != null ? Color.white : Color.black;
    }

    public static Sprite LoadSprite(string resourcesPath, string editorPath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcesPath);
#if UNITY_EDITOR
        if (sprite == null)
            sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
#endif
        return sprite;
    }

    public static GameObject LoadPrefab(string resourcesPath, string editorPath)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcesPath);
#if UNITY_EDITOR
        if (prefab == null)
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(editorPath);
#endif
        return prefab;
    }

    public static GameObject InstantiatePanel(GameObject prefab, Transform parent, string objectName)
    {
        GameObject instance = Object.Instantiate(prefab, parent);
        instance.name = objectName;
        instance.SetActive(false);
        return instance;
    }
}
