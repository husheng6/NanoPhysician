using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗界面货币显示。挂在货币预制体根节点（或含货币文本的 Canvas）上，自动刷新数量。
/// </summary>
[DisallowMultipleComponent]
public class CombatCurrencyHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyTmpText;
    [SerializeField] private Text currencyText;
    [SerializeField] private GameObject currencyRoot;

    private bool initialized;

    public static void TryInitialize()
    {
        CombatCurrencyHUD hud = FindSceneInstance();
        if (hud == null)
            return;

        hud.EnsureInitialized();
    }

    public static void SetVisible(bool visible)
    {
        CombatCurrencyHUD hud = FindSceneInstance();
        if (hud != null)
            hud.SetCurrencyVisible(visible);
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        if (initialized)
            Refresh();
    }

    private void OnDestroy()
    {
        RunProgression.OnChanged -= Refresh;
    }

    private static CombatCurrencyHUD FindSceneInstance()
    {
        CombatCurrencyHUD hud = Object.FindObjectOfType<CombatCurrencyHUD>();
        if (hud != null)
            return hud;

        Transform currencyRootTransform = FindCurrencyRootTransform();
        if (currencyRootTransform == null)
        {
            Debug.LogWarning("CombatCurrencyHUD: 场景中未找到货币预制体，请在关卡放置 shop 预制体（根节点挂 CombatCurrencyHUD）。");
            return null;
        }

        hud = currencyRootTransform.GetComponent<CombatCurrencyHUD>();
        if (hud == null)
            hud = currencyRootTransform.gameObject.AddComponent<CombatCurrencyHUD>();

        return hud;
    }

    private static Transform FindCurrencyRootTransform()
    {
        Transform[] transforms = Object.FindObjectsOfType<Transform>(true);

        foreach (Transform transform in transforms)
        {
            if (transform == null || !transform.gameObject.scene.IsValid())
                continue;

            if (IsExcludedCurrencyRoot(transform.name))
                continue;

            if (transform.name.Equals("shop", System.StringComparison.OrdinalIgnoreCase))
                return transform;
        }

        string[] rootNames =
        {
            "CombatCurrencyHUD", "CurrencyHUD", "CurrencyDisplay",
            "货币显示", "货币HUD", "货币", "money"
        };

        foreach (string rootName in rootNames)
        {
            foreach (Transform transform in transforms)
            {
                if (transform == null || !transform.gameObject.scene.IsValid())
                    continue;

                if (IsExcludedCurrencyRoot(transform.name))
                    continue;

                if (!NameMatches(transform.name, rootName))
                    continue;

                return transform;
            }
        }

        return null;
    }

    private static bool IsExcludedCurrencyRoot(string objectName)
    {
        return objectName.IndexOf("shop page", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void EnsureInitialized()
    {
        if (initialized)
        {
            Refresh();
            return;
        }

        FixCanvasTransform();
        ResolveReferences();

        if (currencyTmpText == null && currencyText == null)
        {
            Debug.LogWarning($"CombatCurrencyHUD: 未找到货币数字文本，对象={name}");
            return;
        }

        RunProgression.OnChanged -= Refresh;
        RunProgression.OnChanged += Refresh;
        Refresh();
        initialized = true;
    }

    private void FixCanvasTransform()
    {
        RectTransform selfRect = transform as RectTransform;
        if (selfRect != null && selfRect.localScale == Vector3.zero)
            selfRect.localScale = Vector3.one;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null && canvasRect.localScale == Vector3.zero)
            canvasRect.localScale = Vector3.one;
    }

    private void ResolveReferences()
    {
        if (currencyTmpText == null)
            currencyTmpText = FindCurrencyText<TMP_Text>();

        if (currencyText == null)
            currencyText = FindCurrencyText<Text>();

        if (currencyRoot == null)
            currencyRoot = gameObject;
    }

    private T FindCurrencyText<T>() where T : Component
    {
        string[] preferredNames =
        {
            "Text (TMP)", "CurrencyText", "CurrencyAmount", "Amount",
            "Count", "value", "数量", "货币数量"
        };

        T[] labels = GetComponentsInChildren<T>(true);
        foreach (string preferredName in preferredNames)
        {
            foreach (T label in labels)
            {
                if (label == null || IsStatusBarLabel(label))
                    continue;

                if (NameMatches(label.name, preferredName))
                    return label;
            }
        }

        T singleCandidate = null;
        foreach (T label in labels)
        {
            if (label == null || IsStatusBarLabel(label))
                continue;

            if (singleCandidate != null)
                return null;

            singleCandidate = label;
        }

        return singleCandidate;
    }

    private static bool NameMatches(string objectName, string pattern)
    {
        return objectName.Equals(pattern, System.StringComparison.OrdinalIgnoreCase)
            || objectName.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void SetCurrencyVisible(bool visible)
    {
        if (currencyRoot != null)
        {
            currencyRoot.SetActive(visible);
            return;
        }

        if (currencyTmpText != null)
            currencyTmpText.gameObject.SetActive(visible);

        if (currencyText != null)
            currencyText.gameObject.SetActive(visible);
    }

    private static bool IsStatusBarLabel(Component label)
    {
        string[] excludedNames =
        {
            "HP", "MP", "Health", "Mana", "Label",
            "PlayerHealth", "PlayerMana", "PlayerHealthBar", "PlayerManaBar",
            "PlayerStatusBars", "StatusBar", "HealthBar", "ManaBar"
        };

        Transform current = label.transform;
        while (current != null)
        {
            foreach (string excludedName in excludedNames)
            {
                if (current.name.IndexOf(excludedName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void Refresh()
    {
        string amount = RunProgression.Currency.ToString();

        if (currencyTmpText != null)
            currencyTmpText.text = amount;

        if (currencyText != null)
            currencyText.text = amount;
    }
}
