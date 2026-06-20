using System;
using UnityEngine;

/// <summary>
/// 第三关：玩家进入指定地图片段时触发剧情，每段仅触发一次。
/// </summary>
public class Level3SegmentDialogueTrigger : MonoBehaviour
{
    private sealed class SegmentEntry
    {
        public string SegmentName;
        public Bounds Bounds;
        public bool HasBounds;
        public bool Played;
        public Action<Action> PlayDialogue;
    }

    private static bool cooperationDialoguePlayed;
    private static bool replicationDialoguePlayed;

    private Transform player;
    private SegmentEntry[] segments;
    private bool dialogueActive;

    public static void ResetState()
    {
        cooperationDialoguePlayed = false;
        replicationDialoguePlayed = false;
    }

    public static void Setup()
    {
        GameObject host = GameObject.Find("Level3SegmentDialogueTrigger");
        if (host == null)
        {
            host = new GameObject("Level3SegmentDialogueTrigger");
            host.AddComponent<Level3SegmentDialogueTrigger>();
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        segments = new[]
        {
            CreateSegment("Background_2", cooperationDialoguePlayed, Level3CooperationDialogueRunner.Play),
            CreateSegment("Background_3", replicationDialoguePlayed, Level3ReplicationDialogueRunner.Play)
        };
    }

    private SegmentEntry CreateSegment(
        string segmentName,
        bool played,
        Action<Action> playDialogue)
    {
        SegmentEntry entry = new SegmentEntry
        {
            SegmentName = segmentName,
            Played = played,
            PlayDialogue = playDialogue
        };

        Transform mapRoot = GameObject.Find("BackgroundGroup")?.transform;
        if (mapRoot == null)
            return entry;

        Transform segment = mapRoot.Find(segmentName);
        if (segment == null)
        {
            Debug.LogWarning($"Level3SegmentDialogueTrigger: 找不到地图片段 {segmentName}。");
            return entry;
        }

        SpriteRenderer renderer = segment.GetComponent<SpriteRenderer>();
        if (renderer == null)
            return entry;

        entry.Bounds = renderer.bounds;
        entry.HasBounds = true;
        return entry;
    }

    private void Update()
    {
        if (dialogueActive || player == null || segments == null)
            return;

        if (LevelGameFlow.IsGameplayFrozen || LevelGameFlow.IsLevelEnded)
            return;

        foreach (SegmentEntry segment in segments)
        {
            if (segment.Played || !segment.HasBounds)
                continue;

            if (!segment.Bounds.Contains(player.position))
                continue;

            TriggerSegmentDialogue(segment);
            break;
        }
    }

    private void TriggerSegmentDialogue(SegmentEntry segment)
    {
        segment.Played = true;
        if (segment.SegmentName == "Background_2")
            cooperationDialoguePlayed = true;
        else if (segment.SegmentName == "Background_3")
            replicationDialoguePlayed = true;

        dialogueActive = true;
        segment.PlayDialogue?.Invoke(() => dialogueActive = false);
    }
}
