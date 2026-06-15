#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 编辑器菜单：修复并绑定关卡一剧情场景的 conversation 面板。
/// </summary>
public static class Level1PlotSetupEditor
{
    private const string ScenePath = "Assets/Scenes/leve1polt.unity";

    [MenuItem("NanoPhysician/Setup Level 1 Plot Scene")]
    public static void SetupLevel1PlotScene()
    {
        if (!PreparePlotScene(showDialogs: true))
            return;

        BuildPlotScene();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Setup Level 1 Plot",
            "剧情场景已绑定 conversation 面板。\n\n请 Ctrl+S 保存场景后运行测试。\n操作：鼠标左键或空格跳过打字 / 下一句。",
            "OK");
    }

    public static void SetupLevel1PlotSceneBatch()
    {
        if (!PreparePlotScene(showDialogs: false))
        {
            Debug.LogError("[Level1PlotSetup] 无法打开剧情场景。");
            EditorApplication.Exit(1);
            return;
        }

        BuildPlotScene();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Level1PlotSetup] 剧情场景绑定完成。");
        EditorApplication.Exit(0);
    }

    private static bool PreparePlotScene(bool showDialogs)
    {
        if (EditorSceneManager.GetActiveScene().name != "leve1polt")
        {
            if (showDialogs)
            {
                if (!EditorUtility.DisplayDialog(
                        "Setup Level 1 Plot",
                        "将打开剧情场景：Assets/Scenes/leve1polt.unity\n是否继续？",
                        "继续",
                        "取消"))
                    return false;
            }

            EditorSceneManager.OpenScene(ScenePath);
        }

        return EditorSceneManager.GetActiveScene().name == "leve1polt";
    }

    private static void BuildPlotScene()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Level1PlotSetup] 场景中找不到 Canvas。");
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect.localScale == Vector3.zero)
            canvasRect.localScale = Vector3.one;

        Level1PlotBootstrap bootstrap = canvas.GetComponent<Level1PlotBootstrap>();
        if (bootstrap == null)
            bootstrap = Undo.AddComponent<Level1PlotBootstrap>(canvas.gameObject);

        Font font = DialogueFont.Get();
        SerializedObject bootstrapSo = new SerializedObject(bootstrap);
        bootstrapSo.FindProperty("dialogueFont").objectReferenceValue = font;
        bootstrapSo.FindProperty("autoSetupOnAwake").boolValue = true;
        bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
#endif
