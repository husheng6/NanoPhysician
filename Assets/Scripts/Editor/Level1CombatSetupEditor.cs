#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 编辑器菜单：一键修复第一关战斗脚本挂载。
/// </summary>
public static class Level1CombatSetupEditor
{
    [MenuItem("NanoPhysician/Setup Level 1 Combat")]
    public static void SetupLevel1Combat()
    {
        if (EditorSceneManager.GetActiveScene().name != "level1Scence")
        {
            EditorUtility.DisplayDialog(
                "Setup Level 1 Combat",
                "请先打开场景：Assets/Scenes/level1Scence.unity",
                "OK");
            return;
        }

        SetupPlayer();
        SetupEnemySpawner();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Setup Level 1 Combat",
            "已完成！\n\nPlayer：Health + PlayerStats + PlayerShooting\nLevel/EnemySpawner：GlucoseEnemySpawner\n\n可在 Resources/Player/DefaultPlayerStats 或 PlayerStats 组件上配置基础属性。\n记得 Ctrl+S 保存场景。",
            "OK");
    }

    private static void SetupPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[Level1CombatSetup] 找不到 Tag 为 Player 的对象。");
            return;
        }

        RemoveMissingScripts(player);

        if (player.GetComponent<Health>() == null)
            Undo.AddComponent<Health>(player);

        if (player.GetComponent<PlayerStats>() == null)
            Undo.AddComponent<PlayerStats>(player);

        if (player.GetComponent<PlayerShooting>() == null)
            Undo.AddComponent<PlayerShooting>(player);
    }

    [MenuItem("NanoPhysician/Setup Level 2 Combat")]
    public static void SetupLevel2Combat()
    {
        if (EditorSceneManager.GetActiveScene().name != "level2Scence")
        {
            EditorUtility.DisplayDialog(
                "Setup Level 2 Combat",
                "请先打开场景：Assets/Scenes/level2Scence.unity",
                "OK");
            return;
        }

        SetupPlayer();
        SetupLevel2EnemySpawner();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Setup Level 2 Combat",
            "已完成！\n\nPlayer：Health + PlayerStats + PlayerShooting\nLevel/EnemySpawner：ImmuneCellEnemySpawner\n\n记得 Ctrl+S 保存场景。",
            "OK");
    }

    private static void SetupLevel2EnemySpawner()
    {
        GameObject level = GameObject.Find("Level");
        if (level == null)
        {
            Debug.LogError("[Level2CombatSetup] 找不到 Level 对象。");
            return;
        }

        Transform existing = level.transform.Find("EnemySpawner");
        if (existing != null)
            Undo.DestroyObjectImmediate(existing.gameObject);

        GameObject spawnerObject = new GameObject("EnemySpawner");
        Undo.RegisterCreatedObjectUndo(spawnerObject, "Create EnemySpawner");
        spawnerObject.transform.SetParent(level.transform);
        spawnerObject.transform.localPosition = Vector3.zero;
        Undo.AddComponent<ImmuneCellEnemySpawner>(spawnerObject);

        Transform mapRoot = level.transform.Find("BackgroundGroup");
        ImmuneCellEnemySpawner spawner = spawnerObject.GetComponent<ImmuneCellEnemySpawner>();
        if (spawner != null && mapRoot != null)
        {
            SerializedObject so = new SerializedObject(spawner);
            so.FindProperty("mapRoot").objectReferenceValue = mapRoot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetupEnemySpawner()
    {
        GameObject level = GameObject.Find("Level");
        if (level == null)
        {
            Debug.LogError("[Level1CombatSetup] 找不到 Level 对象。");
            return;
        }

        Transform existing = level.transform.Find("EnemySpawner");
        if (existing != null)
        {
            RemoveMissingScripts(existing.gameObject);
            if (existing.GetComponent<GlucoseEnemySpawner>() == null)
            {
                Undo.AddComponent<GlucoseEnemySpawner>(existing.gameObject);
            }
        }
        else
        {
            GameObject spawnerObject = new GameObject("EnemySpawner");
            Undo.RegisterCreatedObjectUndo(spawnerObject, "Create EnemySpawner");
            spawnerObject.transform.SetParent(level.transform);
            spawnerObject.transform.localPosition = Vector3.zero;
            Undo.AddComponent<GlucoseEnemySpawner>(spawnerObject);

            Transform mapRoot = level.transform.Find("BackgroundGroup");
            GlucoseEnemySpawner spawner = spawnerObject.GetComponent<GlucoseEnemySpawner>();
            if (spawner != null && mapRoot != null)
            {
                SerializedObject so = new SerializedObject(spawner);
                so.FindProperty("mapRoot").objectReferenceValue = mapRoot;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    private static void RemoveMissingScripts(GameObject target)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(target);
        if (removed > 0)
            Debug.Log($"[Level1CombatSetup] 已从 {target.name} 移除 {removed} 个丢失的脚本引用。");
    }
}
#endif
