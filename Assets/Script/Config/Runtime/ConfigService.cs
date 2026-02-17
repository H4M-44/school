using UnityEngine;

public class ConfigService : MonoBehaviour
{
    public static ConfigService I { get; private set; }

    [Header("Generated Config Assets")]
    [SerializeField] private ScheduleDatabase schedule;
    [SerializeField] private NpcLocationDatabase npcLocation;
    [SerializeField] private DialogueDatabase dialogue;

    public ScheduleDatabase Schedule => schedule;
    public NpcLocationDatabase NpcLocation => npcLocation;
    public DialogueDatabase Dialogue => dialogue;

    private void Awake()
    
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        Debug.Assert(schedule != null, "ScheduleDatabase is not assigned!");
        Debug.Assert(npcLocation != null, "NpcLocationDatabase is not assigned!");
        Debug.Assert(dialogue != null, "DialogueDatabase is not assigned!");
        ConfigQuery.BuildCache();


    }
    
}
