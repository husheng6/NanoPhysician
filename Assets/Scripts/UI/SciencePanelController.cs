using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 科普页面：关闭页面返回主菜单。
/// 挂载到 SciencePanel 根物体上，默认隐藏。
/// </summary>
public class SciencePanelController : MonoBehaviour
{
    [Header("按钮引用")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
    }
}
