using UnityEngine;
using System.Collections;

public class SceneBootstrapper : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;
        BindSceneUI();
    }

    private void BindSceneUI()
    {
        WorldTimeDisplay[] displays = FindObjectsByType<WorldTimeDisplay>(FindObjectsSortMode.None);

        foreach (WorldTimeDisplay display in displays)
        {
            display.TryBind();
        }
    }
}