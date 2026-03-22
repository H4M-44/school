using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Transform cameraTransform;

    private void Awake()
    {
        Hide();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
            return;

        Vector3 direction = transform.position - cameraTransform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void Show(string text)
    {
        root.SetActive(true);
        promptText.text = text;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}