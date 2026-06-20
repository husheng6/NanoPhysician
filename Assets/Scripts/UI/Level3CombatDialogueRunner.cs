using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 第三关战斗场景对话：博士、小雨与科普面板复用。
/// </summary>
public static class Level3CombatDialogueRunner
{
    private const string DoctorPrefabResourcesPath = "Dialogue/conversation1";
    private const string GirlPrefabResourcesPath = "Dialogue/conversation2";
    private const string SciencePrefabResourcesPath = "Dialogue/conversation3";
    private const string DoctorPrefabEditorPath = "Assets/perfab/conversation1.prefab";
    private const string GirlPrefabEditorPath = "Assets/perfab/conversation2.prefab";
    private const string SciencePrefabEditorPath = "Assets/perfab/conversation3.prefab";

    private const int DialogueFontSize = 18;

    public enum Speaker
    {
        Doctor,
        Girl,
        Science
    }

    public readonly struct ScriptLine
    {
        public readonly Speaker Speaker;
        public readonly string Content;

        public ScriptLine(Speaker speaker, string content)
        {
            Speaker = speaker;
            Content = content;
        }

        public static ScriptLine Doctor(string content) => new ScriptLine(Speaker.Doctor, content);
        public static ScriptLine Girl(string content) => new ScriptLine(Speaker.Girl, content);
        public static ScriptLine Science(string content) => new ScriptLine(Speaker.Science, content);
    }

    public static void Play(ScriptLine[] script, Action onComplete)
    {
        if (script == null || script.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        GameObject host = new GameObject("Level3CombatDialogue");
        Level3CombatDialogueHost runner = host.AddComponent<Level3CombatDialogueHost>();
        runner.Run(script, onComplete);
    }

    private sealed class Level3CombatDialogueHost : MonoBehaviour
    {
        private Action onComplete;
        private GameObject doctorPanel;
        private GameObject girlPanel;
        private GameObject sciencePanel;

        public void Run(ScriptLine[] script, Action completeCallback)
        {
            onComplete = completeCallback;
            LevelGameFlow.SetIntroActive(true);
            Time.timeScale = 0f;

            Canvas canvas = CombatUiCanvas.GetOrCreate(280);
            Font font = DialogueFont.Get();
            GameObject doctorPrefab = StoryDialogueUiHelper.LoadPrefab(DoctorPrefabResourcesPath, DoctorPrefabEditorPath);
            GameObject girlPrefab = StoryDialogueUiHelper.LoadPrefab(GirlPrefabResourcesPath, GirlPrefabEditorPath);
            GameObject sciencePrefab = StoryDialogueUiHelper.LoadPrefab(SciencePrefabResourcesPath, SciencePrefabEditorPath);
            if (doctorPrefab == null || girlPrefab == null || sciencePrefab == null || font == null)
            {
                Debug.LogError("Level3CombatDialogueRunner: 缺少对话预制体或字体资源。");
                Finish();
                return;
            }

            doctorPanel = StoryDialogueUiHelper.InstantiatePanel(doctorPrefab, canvas.transform, "conversation1_level3");
            girlPanel = StoryDialogueUiHelper.InstantiatePanel(girlPrefab, canvas.transform, "conversation2_level3");
            sciencePanel = StoryDialogueUiHelper.InstantiatePanel(sciencePrefab, canvas.transform, "conversation3_level3");
            Text doctorText = ConversationDialogueHelper.EnsureDialogueText(
                doctorPanel.transform, font, DialogueFontSize, Color.white);
            Text girlText = ConversationDialogueHelper.EnsureDialogueText(
                girlPanel.transform, font, DialogueFontSize, Color.white);
            Text scienceText = ConversationDialogueHelper.EnsureDialogueText(
                sciencePanel.transform, font, DialogueFontSize, Color.white);

            DialogueLine[] lines = BuildDialogueLines(script, doctorText, girlText, scienceText);

            StreamingDialogueController dialogueController = gameObject.AddComponent<StreamingDialogueController>();
            dialogueController.Configure(lines);
            dialogueController.OnDialogueComplete += HandleDialogueComplete;
            dialogueController.StartDialogue();
        }

        private DialogueLine[] BuildDialogueLines(ScriptLine[] script, Text doctorText, Text girlText, Text scienceText)
        {
            DialogueLine[] lines = new DialogueLine[script.Length];
            for (int i = 0; i < script.Length; i++)
            {
                ScriptLine line = script[i];
                GameObject panel;
                Text text;
                switch (line.Speaker)
                {
                    case Speaker.Girl:
                        panel = girlPanel;
                        text = girlText;
                        break;
                    case Speaker.Science:
                        panel = sciencePanel;
                        text = scienceText;
                        break;
                    default:
                        panel = doctorPanel;
                        text = doctorText;
                        break;
                }

                lines[i] = new DialogueLine
                {
                    panel = panel,
                    dialogueText = text,
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

            if (girlPanel != null)
                Destroy(girlPanel);

            if (sciencePanel != null)
                Destroy(sciencePanel);

            doctorPanel = null;
            girlPanel = null;
            sciencePanel = null;

            LevelGameFlow.SetIntroActive(false);
            Time.timeScale = 1f;
            onComplete?.Invoke();
            onComplete = null;
        }
    }
}
