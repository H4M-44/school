using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DialogueSource
{
    Unknown,
    Schedule,
    Interaction
}

public class DialogueUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;

    [Header("Database")]
    [SerializeField] private DialogueDatabase dialogueDatabase;

    private readonly List<DialogueLine> currentLines = new();
    private int currentIndex = 0;
    private bool isPlaying = false;
    private DialogueSource currentSource = DialogueSource.Unknown;

    public Action OnDialogueFinished;
    public Action<DialogueSource> OnDialogueFinishedWithSource;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        AutoBindMissingReferences();
    }

    private void Start()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        HideDialogue();

        //test StartDialogueByIdRange(900001, 900003);
    }

    public void StartDialogue(List<DialogueLine> lines, DialogueSource source = DialogueSource.Unknown)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("StartDialogue called with no dialogue lines.");
            return;
        }

        currentLines.Clear();
        currentLines.AddRange(lines);

        currentIndex = 0;
        isPlaying = true;
        currentSource = source;

        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    public void StartDialogueByIdRange(int startId, int endId, DialogueSource source = DialogueSource.Unknown)
    {
        if (dialogueDatabase == null)
        {
            Debug.LogError("DialogueDatabase is not assigned.");
            return;
        }

        if (startId <= 0 || endId <= 0)
        {
            Debug.LogWarning($"Invalid dialogue range: startId={startId}, endId={endId}");
            return;
        }

        if (endId < startId)
        {
            Debug.LogError($"Dialogue range is invalid: endId({endId}) < startId({startId})");
            return;
        }

        List<DialogueLine> result = new();

        foreach (var line in dialogueDatabase.lines)
        {
            if (line.dialogueId >= startId && line.dialogueId <= endId)
            {
                result.Add(line);
            }
        }

        result.Sort((a, b) => a.dialogueId.CompareTo(b.dialogueId));

        if (result.Count == 0)
        {
            Debug.LogWarning($"No dialogue lines found in range {startId} - {endId}");
            return;
        }

        StartDialogue(result, source);
    }

    private void ShowCurrentLine()
    {
        if (currentIndex < 0 || currentIndex >= currentLines.Count)
        {
            Debug.LogError("Current dialogue index is out of range.");
            return;
        }

        DialogueLine line = currentLines[currentIndex];
        speakerText.text = line.speaker;
        contentText.text = line.text;

        backButton.interactable = currentIndex > 0;
    }

    private void OnNextClicked()
    {
        if (!isPlaying) return;
        if (currentLines.Count == 0) return;

        currentIndex++;

        if (currentIndex >= currentLines.Count)
        {
            FinishDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void OnBackClicked()
    {
        if (!isPlaying) return;
        if (currentLines.Count == 0) return;
        if (currentIndex <= 0) return;

        currentIndex--;
        ShowCurrentLine();
    }

    private void FinishDialogue()
    {
        DialogueSource finishedSource = currentSource;
        isPlaying = false;
        currentLines.Clear();
        currentIndex = 0;
        currentSource = DialogueSource.Unknown;

        HideDialogue();
        OnDialogueFinishedWithSource?.Invoke(finishedSource);
        OnDialogueFinished?.Invoke();
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void AutoBindMissingReferences()
    {
        if (dialoguePanel == null)
            dialoguePanel = gameObject;

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (speakerText == null && text.name == "SpeakerText")
                speakerText = text;
            else if (contentText == null && text.name == "ContentText")
                contentText = text;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (nextButton == null && button.name == "NextButton")
                nextButton = button;
            else if (backButton == null && button.name == "BackButton")
                backButton = button;
        }

        if (dialogueDatabase == null && ConfigService.I != null)
            dialogueDatabase = ConfigService.I.Dialogue;
    }
}
