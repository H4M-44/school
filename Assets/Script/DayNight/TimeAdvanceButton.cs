using UnityEngine;
using UnityEngine.UI;

public class TimeAdvanceButton : MonoBehaviour
{
    [SerializeField] private DailyTimeController dailyTimeController;

    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button != null && !HasValidPersistentNextTimeListener(button))
            button.onClick.AddListener(OnClickAdvance);
    }

    private bool HasValidPersistentNextTimeListener(Button button)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            Object target = button.onClick.GetPersistentTarget(i);
            string methodName = button.onClick.GetPersistentMethodName(i);

            if (target is DailyTimeController && methodName == nameof(DailyTimeController.NextTime))
                return true;
        }

        return false;
    }

    public void OnClickAdvance()
    {
        if (dailyTimeController == null)
            dailyTimeController = FindFirstObjectByType<DailyTimeController>();

        if (dailyTimeController == null)
        {
            Debug.LogError("[TimeAdvanceButton] DailyTimeController is not assigned.");
            return;
        }

        dailyTimeController.NextTime();
    }
}
