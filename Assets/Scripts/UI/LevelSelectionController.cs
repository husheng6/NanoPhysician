using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡选择界面：点击关卡按钮进入对应场景。
/// </summary>
public class LevelSelectionController : MonoBehaviour
{
    [Header("关卡按钮")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    [Header("场景名称")]
    [SerializeField] private string level1SceneName = "leve1polt";
    [SerializeField] private string level2SceneName = "level2Scence";
    [SerializeField] private string level3SceneName = "level3Scence";

    private void Awake()
    {
        ResolveMissingButtons();

        BindButton(level1Button, LoadLevel1);
        BindButton(level2Button, LoadLevel2);
        BindButton(level3Button, LoadLevel3);
    }

    /// <summary>
    /// Inspector 未拖引用时，按子物体名称自动查找按钮。
    /// </summary>
    private void ResolveMissingButtons()
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            switch (button.gameObject.name)
            {
                case "level1":
                    if (level1Button == null)
                        level1Button = button;
                    break;
                case "level2":
                    if (level2Button == null)
                        level2Button = button;
                    break;
                case "level3":
                    if (level3Button == null)
                        level3Button = button;
                    break;
            }
        }
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    public void LoadLevel1()
    {
        SceneLoader.LoadNewRun(level1SceneName);
    }

    public void LoadLevel2()
    {
        SceneLoader.LoadNewRun(level2SceneName);
    }

    public void LoadLevel3()
    {
        SceneLoader.LoadNewRun(level3SceneName);
    }

    private void OnDestroy()
    {
        if (level1Button != null)
            level1Button.onClick.RemoveListener(LoadLevel1);

        if (level2Button != null)
            level2Button.onClick.RemoveListener(LoadLevel2);

        if (level3Button != null)
            level3Button.onClick.RemoveListener(LoadLevel3);
    }
}
