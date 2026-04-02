using UnityEngine;

public class TimeAdvanceButton : MonoBehaviour
{
    [SerializeField] private DailyTimeController dailyTimeController;

    public void OnClickAdvance()
    {
        if (dailyTimeController == null)
        {
            Debug.LogError("[TimeAdvanceButton] DailyTimeController is not assigned.");
            return;
        }

        dailyTimeController.NextTime();
    }
}