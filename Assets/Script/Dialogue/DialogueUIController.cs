using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public Action OnDialogueFinished;

    public bool IsPlaying => isPlaying;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        backButton.onClick.AddListener(OnBackClicked);
        HideDialogue();

        //test StartDialogueByIdRange(900001, 900003);
    }

    public void StartDialogue(List<DialogueLine> lines)
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

        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    public void StartDialogueByIdRange(int startId, int endId)
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

        StartDialogue(result);
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
        isPlaying = false;
        currentLines.Clear();
        currentIndex = 0;

        HideDialogue();
        OnDialogueFinished?.Invoke();
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}