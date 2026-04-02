using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStatePrefab;
    [SerializeField] private WorldTime worldTimePrefab;

    private void Awake()
    {
        EnsureGameState();
        EnsureWorldTime();
    }

    private void EnsureGameState()
    {
        if (GameStateManager.Instance != null)
            return;

        if (gameStatePrefab != null)
        {
            Instantiate(gameStatePrefab);
        }
        else
        {
            GameObject go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
        }
    }

    private void EnsureWorldTime()
    {
        if (WorldTime.Instance != null)
            return;

        if (worldTimePrefab != null)
        {
            Instantiate(worldTimePrefab);
        }
        else
        {
            GameObject go = new GameObject("WorldTime");
            go.AddComponent<WorldTime>();
        }
    }
}