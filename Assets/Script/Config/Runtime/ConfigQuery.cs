using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ConfigQuery
{
    private static Dictionary<int, ScheduleDay> _dayByNumber;
    private static Dictionary<int, NpcLocationSet> _npcSetById;
    private static Dictionary<int, List<DialogueLine>> _dialogueLinesById;
    private static Dictionary<int, GameEventDefinition> _eventById;

    public static void BuildCache()
    {
        var cfg = ConfigService.I;

        _dayByNumber = cfg.Schedule.days.ToDictionary(d => d.dayNumber, d => d);
        _npcSetById = cfg.NpcLocation.sets.ToDictionary(s => s.id, s => s);
        _eventById = cfg.Events != null
            ? cfg.Events.events.Where(e => e.id != 0).ToDictionary(e => e.id, e => e)
            : new Dictionary<int, GameEventDefinition>();

        _dialogueLinesById = new Dictionary<int, List<DialogueLine>>();
        foreach (var line in cfg.Dialogue.lines)
        {
            if (!_dialogueLinesById.TryGetValue(line.dialogueId, out var list))
            {
                list = new List<DialogueLine>();
                _dialogueLinesById.Add(line.dialogueId, list);
            }
            list.Add(line);
        }
    }

    public static ScheduleDay GetScheduleDay(int dayNumber)
        => _dayByNumber != null && _dayByNumber.TryGetValue(dayNumber, out var d) ? d : null;

    public static NpcLocationSet GetNpcLocationSet(int id)
        => _npcSetById != null && _npcSetById.TryGetValue(id, out var s) ? s : null;

    public static List<DialogueLine> GetDialogue(int dialogueId)
        => _dialogueLinesById != null && _dialogueLinesById.TryGetValue(dialogueId, out var lines) ? lines : null;

    public static GameEventDefinition GetEvent(int eventId)
        => _eventById != null && _eventById.TryGetValue(eventId, out var e) ? e : null;
}
