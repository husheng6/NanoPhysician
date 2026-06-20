using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 商店界面：支持主页预览与关卡胜利后升级。
/// </summary>
[DisallowMultipleComponent]
public class ShopUI : MonoBehaviour
{
    private const string PrefabResourcesPath = "Shop/shop page";
    private const string PrefabEditorPath = "Assets/perfab/shop page.prefab";

    private enum ShopMode
    {
        Menu,
        Victory
    }

    private static ShopUI instance;
    private static string pendingNextScene;
    private static ShopMode currentMode;

    [Header("预制体节点（可留空，运行时自动查找）")]
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button healthUpgradeButton;
    [SerializeField] private Button meleeUpgradeButton;
    [SerializeField] private Button rangedUpgradeButton;
    [SerializeField] private Button manaUpgradeButton;
    [SerializeField] private TMP_Text healthLevelText;
    [SerializeField] private TMP_Text meleeLevelText;
    [SerializeField] private TMP_Text rangedLevelText;
    [SerializeField] private TMP_Text manaLevelText;

    private Canvas shopCanvas;
    private bool initialized;

    public static void ResetState()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }

        pendingNextScene = null;
        currentMode = ShopMode.Menu;
    }

    public static void ShowForMenu()
    {
        ShowInternal(null, ShopMode.Menu);
    }

    public static void Show(string nextSceneName)
    {
        ShowInternal(nextSceneName, ShopMode.Victory);
    }

    private static void ShowInternal(string nextSceneName, ShopMode mode)
    {
        pendingNextScene = nextSceneName;
        currentMode = mode;
        EnsureEventSystem();

        if (instance != null)
        {
            instance.gameObject.SetActive(true);
            instance.EnsureInitialized();
            instance.Refresh();
            OnShopOpened(mode);
            return;
        }

        GameObject prefab = LoadPrefab();
        if (prefab == null)
        {
            Debug.LogError("ShopUI: 找不到商店预制体。");
            return;
        }

        GameObject shopObject = Instantiate(prefab);
        shopObject.name = "ShopPanel";
        instance = shopObject.GetComponent<ShopUI>();
        if (instance == null)
            instance = shopObject.AddComponent<ShopUI>();

        instance.EnsureInitialized();
        instance.Refresh();
        OnShopOpened(mode);
    }

    private static void OnShopOpened(ShopMode mode)
    {
        if (mode == ShopMode.Victory)
            CombatCurrencyHUD.SetVisible(false);
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        ResolveReferences();
        ApplyTextStyles();
        BindButtons();
        EnsureCanvasOnTop();
        RunProgression.OnChanged += Refresh;
        initialized = true;
    }

    private void OnDestroy()
    {
        RunProgression.OnChanged -= Refresh;

        if (instance == this)
            instance = null;
    }

    private void ResolveReferences()
    {
        Transform root = transform;

        if (currencyText == null)
            currencyText = root.Find("money/Text (TMP)")?.GetComponent<TMP_Text>();

        if (healthUpgradeButton == null)
            healthUpgradeButton = root.Find("health points/Button")?.GetComponent<Button>();

        if (meleeUpgradeButton == null)
            meleeUpgradeButton = root.Find("close/Button")?.GetComponent<Button>();

        if (rangedUpgradeButton == null)
            rangedUpgradeButton = root.Find("remote/Button")?.GetComponent<Button>();

        if (manaUpgradeButton == null)
            manaUpgradeButton = root.Find("blue/Button")?.GetComponent<Button>();

        if (closeButton == null)
            closeButton = FindCloseButton(root);

        if (healthLevelText == null)
            healthLevelText = root.Find("health points")?.GetComponent<TMP_Text>();

        if (meleeLevelText == null)
            meleeLevelText = root.Find("close")?.GetComponent<TMP_Text>();

        if (rangedLevelText == null)
            rangedLevelText = root.Find("remote")?.GetComponent<TMP_Text>();

        if (manaLevelText == null)
            manaLevelText = root.Find("blue")?.GetComponent<TMP_Text>();

        DisableRaycast(healthLevelText);
        DisableRaycast(meleeLevelText);
        DisableRaycast(rangedLevelText);
        DisableRaycast(manaLevelText);
        DisableRaycast(currencyText);

        if (closeButton == null)
            Debug.LogWarning("ShopUI: 未找到关闭按钮，请在根节点下放置名为 closeButton 或 Button 的关闭按钮。");
    }

    private void ApplyTextStyles()
    {
        TMP_Text[] labels = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text label in labels)
        {
            if (label == null)
                continue;

            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
        }
    }

    private static void DisableRaycast(TMP_Text label)
    {
        if (label != null)
            label.raycastTarget = false;
    }

    private static Button FindCloseButton(Transform root)
    {
        string[] preferredNames = { "closeButton", "exitButton", "close button", "exit", "关闭" };
        foreach (string name in preferredNames)
        {
            Button button = FindChildButton(root, name);
            if (button != null)
                return button;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name != "Button")
                continue;

            Button button = child.GetComponent<Button>();
            if (button != null)
                return button;
        }

        return null;
    }

    private static Button FindChildButton(Transform root, string childName)
    {
        Transform target = root.Find(childName);
        if (target == null)
            return null;

        Button button = target.GetComponent<Button>();
        if (button != null)
            return button;

        return target.GetComponentInChildren<Button>(true);
    }

    private void BindButtons()
    {
        BindButton(healthUpgradeButton, () => HandleUpgrade(ShopUpgradeType.Health));
        BindButton(meleeUpgradeButton, () => HandleUpgrade(ShopUpgradeType.MeleeAttack));
        BindButton(rangedUpgradeButton, () => HandleUpgrade(ShopUpgradeType.RangedAttack));
        BindButton(manaUpgradeButton, () => HandleUpgrade(ShopUpgradeType.Mana));
        BindButton(closeButton, CloseShop);
    }

    private static void BindButton(Button button, Action action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action());
    }

    private void EnsureCanvasOnTop()
    {
        shopCanvas = GetComponent<Canvas>();
        if (shopCanvas == null)
            return;

        shopCanvas.overrideSorting = true;
        shopCanvas.sortingOrder = 300;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private void HandleUpgrade(ShopUpgradeType type)
    {
        if (!RunProgression.TryUpgrade(type))
            return;

        PlayerStats stats = FindPlayerStats();
        if (stats != null)
            RunProgression.ApplyToPlayer(stats);

        Refresh();
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);

        if (currentMode == ShopMode.Menu)
            return;

        CombatCurrencyHUD.SetVisible(true);
        ShowPostShopFlow();
    }

    private static void ShowPostShopFlow()
    {
        if (Level1VictoryDialogueRunner.ShouldPlayForCurrentScene())
        {
            Level1VictoryDialogueRunner.Play(() => LevelVictoryUI.Show(pendingNextScene));
            return;
        }

        if (Level2VictoryDialogueRunner.ShouldPlayForCurrentScene())
        {
            Level2VictoryDialogueRunner.Play(() => LevelVictoryUI.Show(pendingNextScene));
            return;
        }

        if (Level3VictoryDialogueRunner.ShouldPlayForCurrentScene())
        {
            Level3VictoryDialogueRunner.Play(() => LevelVictoryUI.Show(pendingNextScene));
            return;
        }

        LevelVictoryUI.Show(pendingNextScene);
    }

    private void Refresh()
    {
        if (currencyText != null)
            currencyText.text = RunProgression.Currency.ToString();

        UpdateRowLabel(healthLevelText, ShopUpgradeType.Health);
        UpdateRowLabel(meleeLevelText, ShopUpgradeType.MeleeAttack);
        UpdateRowLabel(rangedLevelText, ShopUpgradeType.RangedAttack);
        UpdateRowLabel(manaLevelText, ShopUpgradeType.Mana);

        SetUpgradeButtonState(healthUpgradeButton, ShopUpgradeType.Health);
        SetUpgradeButtonState(meleeUpgradeButton, ShopUpgradeType.MeleeAttack);
        SetUpgradeButtonState(rangedUpgradeButton, ShopUpgradeType.RangedAttack);
        SetUpgradeButtonState(manaUpgradeButton, ShopUpgradeType.Mana);
    }

    private static void UpdateRowLabel(TMP_Text label, ShopUpgradeType type)
    {
        if (label == null)
            return;

        int level = RunProgression.GetUpgradeLevel(type);
        label.text = $"{RunProgression.GetDisplayName(type)}：{level}/{RunProgression.MaxUpgradeLevel}";
    }

    private static void SetUpgradeButtonState(Button button, ShopUpgradeType type)
    {
        if (button == null)
            return;

        bool canUpgrade = RunProgression.Currency >= RunProgression.UpgradeCost
            && RunProgression.GetUpgradeLevel(type) < RunProgression.MaxUpgradeLevel;
        button.interactable = canUpgrade;
    }

    private static PlayerStats FindPlayerStats()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.GetComponent<PlayerStats>() : null;
    }

    private static GameObject LoadPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>(PrefabResourcesPath);
#if UNITY_EDITOR
        if (prefab == null)
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PrefabEditorPath);
#endif
        return prefab;
    }
}
