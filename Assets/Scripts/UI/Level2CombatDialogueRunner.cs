using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 第二关战斗场景对话：博士与科普面板复用。
/// </summary>
public static class Level2CombatDialogueRunner
{
    private const string DoctorPrefabResourcesPath = "Dialogue/conversation1";
    private const string SciencePrefabResourcesPath = "Dialogue/conversation3";
    private const string DoctorPrefabEditorPath = "Assets/perfab/conversation1.prefab";
    private const string SciencePrefabEditorPath = "Assets/perfab/conversation3.prefab";

    private const int DialogueFontSize = 18;

    public readonly struct ScriptLine
    {
        public readonly bool IsScience;
        public readonly string Content;

        public ScriptLine(bool isScience, string content)
        {
            IsScience = isScience;
            Content = content;
        }

        public static ScriptLine Doctor(string content) => new ScriptLine(false, content);
        public static ScriptLine Science(string content) => new ScriptLine(true, content);
    }

    public static void Play(ScriptLine[] script, Action onComplete)
    {
        if (script == null || script.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        GameObject host = new GameObject("Level2CombatDialogue");
        Level2CombatDialogueHost runner = host.AddComponent<Level2CombatDialogueHost>();
        runner.Run(script, onComplete);
    }

    private sealed class Level2CombatDialogueHost : MonoBehaviour
    {
        private Action onComplete;
        private GameObject doctorPanel;
        private GameObject sciencePanel;

        public void Run(ScriptLine[] script, Action completeCallback)
        {
            onComplete = completeCallback;
            LevelGameFlow.SetIntroActive(true);
            Time.timeScale = 0f;

            Canvas canvas = CombatUiCanvas.GetOrCreate(280);
            Font font = DialogueFont.Get();
            GameObject doctorPrefab = StoryDialogueUiHelper.LoadPrefab(DoctorPrefabResourcesPath, DoctorPrefabEditorPath);
            GameObject sciencePrefab = StoryDialogueUiHelper.LoadPrefab(SciencePrefabResourcesPath, SciencePrefabEditorPath);
            if (doctorPrefab == null || sciencePrefab == null || font == null)
            {
                Debug.LogError("Level2CombatDialogueRunner: 缺少对话预制体或字体资源。");
                Finish();
                return;
            }

            doctorPanel = StoryDialogueUiHelper.InstantiatePanel(doctorPrefab, canvas.transform, "conversation1_level2");
            sciencePanel = StoryDialogueUiHelper.InstantiatePanel(sciencePrefab, canvas.transform, "conversation3_level2");
            Text doctorText = ConversationDialogueHelper.EnsureDialogueText(
                doctorPanel.transform, font, DialogueFontSize, Color.white);
            Text scienceText = ConversationDialogueHelper.EnsureDialogueText(
                sciencePanel.transform, font, DialogueFontSize, Color.white);

            DialogueLine[] lines = BuildDialogueLines(script, doctorText, scienceText);

            StreamingDialogueController dialogueController = gameObject.AddComponent<StreamingDialogueController>();
            dialogueController.Configure(lines);
            dialogueController.OnDialogueComplete += HandleDialogueComplete;
            dialogueController.StartDialogue();
        }

        private DialogueLine[] BuildDialogueLines(ScriptLine[] script, Text doctorText, Text scienceText)
        {
            DialogueLine[] lines = new DialogueLine[script.Length];
            for (int i = 0; i < script.Length; i++)
            {
                ScriptLine line = script[i];
                lines[i] = new DialogueLine
                {
                    panel = line.IsScience ? sciencePanel : doctorPanel,
                    dialogueText = line.IsScience ? scienceText : doctorText,
                    content = line.Content
                };
            }

            return lines;
        }

        private void HandleDialogueComplete()
        {
            Finish();
            Destroy(gameObject);
        }

        private void Finish()
        {
            if (doctorPanel != null)
                Destroy(doctorPanel);

            if (sciencePanel != null)
                Destroy(sciencePanel);

            doctorPanel = null;
            sciencePanel = null;

            LevelGameFlow.SetIntroActive(false);
            Time.timeScale = 1f;
            onComplete?.Invoke();
            onComplete = null;
        }
    }
}
