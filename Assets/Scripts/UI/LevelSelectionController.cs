using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡选择界面：点击关卡按钮进入对应场景。
/// 挂载到 Canvas 上。
/// </summary>
public class LevelSelectionController : MonoBehaviour
{
    [Header("关卡按钮")]
    [SerializeField] private Button level1Button;

    [Header("场景名称")]
    [SerializeField] private string level1SceneName = "level1Scence";

    private void Awake()
    {
        if (level1Button != null)
            level1Button.onClick.AddListener(LoadLevel1);
    }

    public void LoadLevel1()
    {
        SceneLoader.Load(level1SceneName);
    }

    private void OnDestroy()
    {
        if (level1Button != null)
            level1Button.onClick.RemoveListener(LoadLevel1);
    }
}
