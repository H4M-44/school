using UnityEngine;
using WorldTime;

public class TimeAdvanceButton : MonoBehaviour
{
    [SerializeField] private WorldTime.WorldTime worldTime; // drag your timesys reference here
    [SerializeField] private int advanceMinutes = 15;       // set per button in Inspector

    public void OnClickAdvance()
    {
        if (worldTime == null)
        {
            Debug.LogError("WorldTime reference is missing on TimeAdvanceButton.");
            return;
        }

        worldTime.AdvanceMinutes(advanceMinutes);
    }
}
