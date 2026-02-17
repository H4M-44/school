#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;
using UnityEditor;
using UnityEngine;

public static class DailySystemExcelImporter
{
    // ---- Configure paths here ----
    private const string ExcelPath = "Assets/Data/SourceTables/日常系统.xlsx";
    private const string OutputFolder = "Assets/Data/GeneratedAssets/";

    // ---- Sheet name candidates (you don't need to change Excel) ----
    private static readonly string[] ScheduleSheetNames = { "日程系统", "日程", "Schedule", "ScheduleSystem" };
    private static readonly string[] NpcLocationSheetNames = { "NPC位置", "Npc位置", "NPC", "NpcLocation", "NpcLocationSystem" };
    private static readonly string[] DialogueSheetNames = { "剧情", "对话", "剧情系统", "Dialogue", "Dialog" };

    [MenuItem("Tools/Config/Import Daily System (XLSX Robust)")]
    public static void ImportAll()
    {
        if (!File.Exists(ExcelPath))
        {
            Debug.LogError($"Excel not found: {ExcelPath}");
            return;
        }

        Directory.CreateDirectory(OutputFolder);

        using var stream = File.Open(ExcelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var ds = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
        });

        var scheduleTable = FindSheet(ds, ScheduleSheetNames);
        var npcTable = FindSheet(ds, NpcLocationSheetNames);
        var dialogueTable = FindSheet(ds, DialogueSheetNames);

        if (scheduleTable == null) Debug.LogError($"Cannot find schedule sheet. Existing: {ListSheets(ds)}");
        if (npcTable == null) Debug.LogError($"Cannot find npc-location sheet. Existing: {ListSheets(ds)}");
        if (dialogueTable == null) Debug.LogError($"Cannot find dialogue sheet. Existing: {ListSheets(ds)}");

        // Parse + Save
        var scheduleDb = ParseSchedule(scheduleTable);
        SaveOrOverwrite(scheduleDb, "ScheduleDatabase.asset");

        var npcDb = ParseNpcLocation(npcTable);
        SaveOrOverwrite(npcDb, "NpcLocationDatabase.asset");

        var dialogueDb = ParseDialogue(dialogueTable);
        SaveOrOverwrite(dialogueDb, "DialogueDatabase.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Import done. ScheduleDays={scheduleDb.days.Count}, NpcSets={npcDb.sets.Count}, DialogueLines={dialogueDb.lines.Count}");
    }

    // =========================
    // Parse: 日程系统
    // =========================
    private static ScheduleDatabase ParseSchedule(DataTable t)
    {
        var db = ScriptableObject.CreateInstance<ScheduleDatabase>();
        if (t == null) return db;

        int headerRow = DetectHeaderRow(t, requiredKeywords: new[] { "ID", "天数", "时间段" });
        if (headerRow < 0)
        {
            Debug.LogError("Schedule: cannot detect header row.");
            return db;
        }

        var headerMap = BuildHeaderMap(t, headerRow);

        // Mandatory
        int colId = FindCol(headerMap, "ID");
        int colDay = FindCol(headerMap, "天数");

        if (colId < 0 || colDay < 0)
        {
            Debug.LogError($"Schedule: missing columns. ID={colId}, 天数={colDay}");
            return db;
        }

        // Time blocks A~Z (we'll dynamically find all '时间段X')
        var blockKeys = FindTimeBlockKeys(headerMap.Keys); // returns list like A,B,C...
        // For each key, we try to find:
        // 时间段A, 名字, NPC位置ID, 起始剧情ID, 结束剧情ID (your sheet uses these exact headers)
        // Note: sometimes columns might be named like "NPC位置ID" or "npc位置ID" etc. We'll use contains matching.

        for (int r = headerRow + 1; r < t.Rows.Count; r++)
        {
            var row = t.Rows[r];
            if (IsRowEmpty(row)) continue;

            int id = ToInt(row, colId);
            int dayNumber = ToInt(row, colDay);
            if (id == 0 && dayNumber == 0) continue;

            var day = new ScheduleDay
            {
                id = id,
                dayNumber = dayNumber,
                blocks = new List<TimeBlock>()
            };

            foreach (var key in blockKeys)
            {
                int colTime = FindCol(headerMap, $"时间段{key}");
                if (colTime < 0) continue;

                string time = NormalizeTime(ToCellString(row, colTime));
                if (string.IsNullOrWhiteSpace(time)) continue;

                int colName = FindCol(headerMap, $"名字{key}");
                if (colName < 0) colName = FindCol(headerMap, "名字"); // fallback if not suffixed

                int colNpc =
                    FindColContains(headerMap, $"NPC位置ID{key}") ??
                    FindColContains(headerMap, "NPC位置ID") ??
                    FindColContains(headerMap, "Npc位置ID") ??
                    FindColContains(headerMap, "NPC位置") ??
                    -1;

                int colStart =
                    FindColContains(headerMap, $"起始剧情ID{key}") ??
                    FindColContains(headerMap, "起始剧情ID") ??
                    FindColContains(headerMap, "开始剧情ID") ??
                    FindColContains(headerMap, "起始剧情") ??
                    -1;

                int colEnd =
                    FindColContains(headerMap, $"结束剧情ID{key}") ??
                    FindColContains(headerMap, "结束剧情ID") ??
                    FindColContains(headerMap, "结束剧情") ??
                    -1;


                var block = new TimeBlock
                {
                    time = time,
                    name = colName >= 0 ? ToCellString(row, colName) : "",
                    npcLocationId = colNpc >= 0 ? ToInt(row, colNpc) : 0,
                    startDialogueId = colStart >= 0 ? ToInt(row, colStart) : 0,
                    endDialogueId = colEnd >= 0 ? ToInt(row, colEnd) : 0,
                };

                day.blocks.Add(block);
            }

            // Sort blocks by time (HH:mm)
            day.blocks.Sort((a, b) => ParseMinutes(a.time).CompareTo(ParseMinutes(b.time)));

            db.days.Add(day);
        }

        return db;
    }

    // Try to detect keys A,B,C... by scanning headers that start with "时间段"
    private static List<string> FindTimeBlockKeys(IEnumerable<string> headers)
    {
        var keys = new HashSet<string>();
        foreach (var h in headers)
        {
            var s = NormalizeHeader(h);
            if (!s.StartsWith("时间段")) continue;

            // "时间段A" -> key = "A"
            var key = s.Substring("时间段".Length).Trim();
            if (string.IsNullOrWhiteSpace(key)) continue;
            keys.Add(key);
        }

        // If none found, fallback to A~F
        if (keys.Count == 0)
            return new List<string> { "A", "B", "C", "D", "E", "F" };

        // Stable ordering: A,B,C... then others
        return keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
    }

    // =========================
    // Parse: NPC位置
    // =========================
    private static NpcLocationDatabase ParseNpcLocation(DataTable t)
    {
        var db = ScriptableObject.CreateInstance<NpcLocationDatabase>();
        if (t == null) return db;

        int headerRow = DetectHeaderRow(t, requiredKeywords: new[] { "ID" });
        if (headerRow < 0)
        {
            Debug.LogError("NPC位置: cannot detect header row.");
            return db;
        }

        var headerMap = BuildHeaderMap(t, headerRow);
        int colId = FindCol(headerMap, "ID");
        int colNote = FindCol(headerMap, "备注");

        if (colId < 0)
        {
            Debug.LogError("NPC位置: missing ID column.");
            return db;
        }

        // Your NPC位置 header is like:
        // ID, 备注, A, 事件ID, B, 事件ID, C, 事件ID...
        // We'll parse pairs by scanning columns after ID/备注:
        // a "slot" column then next "eventId" column (header contains 事件ID)
        var colCount = t.Columns.Count;

        for (int r = headerRow + 1; r < t.Rows.Count; r++)
        {
            var row = t.Rows[r];
            if (IsRowEmpty(row)) continue;

            int id = ToInt(row, colId);
            if (id == 0) continue;

            var set = new NpcLocationSet
            {
                id = id,
                note = colNote >= 0 ? ToCellString(row, colNote) : "",
                slotEvents = new List<NpcSlotEvent>()
            };

            // Start scanning from the first column after "备注" if it exists, else after ID
            int startCol = (colNote >= 0 ? Math.Max(colNote + 1, colId + 1) : colId + 1);

            for (int c = startCol; c < colCount - 1; c++)
            {
                string header = GetHeaderByCol(headerMap, c);

                // Detect slot columns: header is "A"/"B"/"C" OR contains "槽位" etc.
                // In your sheet, header itself might be "A", and the next header is "事件ID".
                string slotHeader = NormalizeHeader(header);
                if (string.IsNullOrWhiteSpace(slotHeader)) continue;

                // If next column header contains "事件ID", treat current as slot
                string nextHeader = GetHeaderByCol(headerMap, c + 1);
                if (!NormalizeHeader(nextHeader).Contains("事件id") && !NormalizeHeader(nextHeader).Contains("事件ID".ToLower()))
                    continue;

                string slotValue = ToCellString(row, c); // usually "A"/"B"/...
                if (string.IsNullOrWhiteSpace(slotValue))
                {
                    // If the cell is empty, you may still want slot from header (A/B/C). We'll fallback.
                    slotValue = slotHeader;
                }

                int eventId = ToInt(row, c + 1);

                // Allow empty eventId (0), still store slot so you can debug
                set.slotEvents.Add(new NpcSlotEvent
                {
                    slot = slotValue.Trim(),
                    eventId = eventId
                });

                c++; // skip eventId column
            }

            db.sets.Add(set);
        }

        return db;
    }

    // =========================
    // Parse: 剧情
    // =========================
    private static DialogueDatabase ParseDialogue(DataTable t)
    {
        var db = ScriptableObject.CreateInstance<DialogueDatabase>();
        if (t == null) return db;

        int headerRow = DetectHeaderRow(t, requiredKeywords: new[] { "ID", "文本" });
        if (headerRow < 0)
        {
            Debug.LogError("剧情: cannot detect header row.");
            return db;
        }

        var headerMap = BuildHeaderMap(t, headerRow);

        int colTime = FindCol(headerMap, "时间段"); // optional
        int colId = FindCol(headerMap, "ID");
        int colSpeaker = FindCol(headerMap, "对话角色");
        if (colSpeaker < 0) colSpeaker = FindCol(headerMap, "角色");
        int colText = FindCol(headerMap, "文本");
        if (colText < 0) colText = FindCol(headerMap, "内容");

        if (colId < 0 || colText < 0)
        {
            Debug.LogError($"剧情: missing columns. ID={colId}, 文本/内容={colText}");
            return db;
        }

        for (int r = headerRow + 1; r < t.Rows.Count; r++)
        {
            var row = t.Rows[r];
            if (IsRowEmpty(row)) continue;

            int dialogueId = ToInt(row, colId);
            if (dialogueId == 0) continue;

            string text = ToCellString(row, colText);
            if (string.IsNullOrWhiteSpace(text)) continue;

            db.lines.Add(new DialogueLine
            {
                dialogueId = dialogueId,
                time = colTime >= 0 ? NormalizeTime(ToCellString(row, colTime)) : "",
                speaker = colSpeaker >= 0 ? ToCellString(row, colSpeaker) : "",
                text = text
            });
        }

        return db;
    }

    // =========================
    // Excel & Asset Helpers
    // =========================
    private static DataTable FindSheet(DataSet ds, string[] candidates)
    {
        if (ds == null) return null;

        // Exact match first
        foreach (var name in candidates)
        {
            foreach (DataTable t in ds.Tables)
                if (t.TableName == name) return t;
        }

        // Contains match fallback (case-insensitive)
        foreach (var name in candidates)
        {
            var n = name.ToLowerInvariant();
            foreach (DataTable t in ds.Tables)
            {
                var tn = (t.TableName ?? "").ToLowerInvariant();
                if (tn.Contains(n)) return t;
            }
        }

        return null;
    }

    private static string ListSheets(DataSet ds)
    {
        if (ds == null) return "(null)";
        return string.Join(", ", ds.Tables.Cast<DataTable>().Select(x => x.TableName));
    }

    private static void SaveOrOverwrite(UnityEngine.Object asset, string fileName)
    {
        var path = Path.Combine(OutputFolder, fileName).Replace("\\", "/");
        var existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

        if (existing != null)
        {
            EditorUtility.CopySerialized(asset, existing);
            UnityEngine.Object.DestroyImmediate(asset);
            EditorUtility.SetDirty(existing);
        }
        else
        {
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
        }
    }

    // Detect which row is header by checking if it contains required keywords across columns
    private static int DetectHeaderRow(DataTable t, string[] requiredKeywords)
    {
        // scan first 10 rows (enough for your use)
        int maxScan = Math.Min(10, t.Rows.Count);
        for (int r = 0; r < maxScan; r++)
        {
            var row = t.Rows[r];
            if (IsRowEmpty(row)) continue;

            // build row strings
            var cells = new List<string>();
            for (int c = 0; c < t.Columns.Count; c++)
                cells.Add(NormalizeHeader(CellRawToString(row[c])));

            bool ok = true;
            foreach (var k in requiredKeywords)
            {
                var kk = NormalizeHeader(k);
                if (!cells.Any(s => s.Contains(kk)))
                {
                    ok = false;
                    break;
                }
            }
            if (ok) return r;
        }
        return -1;
    }

    private static Dictionary<string, int> BuildHeaderMap(DataTable t, int headerRowIndex)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headerRow = t.Rows[headerRowIndex];

        for (int c = 0; c < t.Columns.Count; c++)
        {
            var raw = CellRawToString(headerRow[c]);
            var key = NormalizeHeader(raw);

            if (string.IsNullOrWhiteSpace(key)) continue;

            // If duplicates, keep the first
            if (!map.ContainsKey(key))
                map[key] = c;
        }
        return map;
    }

    private static int FindCol(Dictionary<string, int> headerMap, string exactName)
    {
        if (headerMap == null) return -1;
        var key = NormalizeHeader(exactName);
        return headerMap.TryGetValue(key, out var c) ? c : -1;
    }

    private static int? FindColContains(Dictionary<string, int> headerMap, string containsName)
    {
        if (headerMap == null) return null;
        var key = NormalizeHeader(containsName);
        foreach (var kv in headerMap)
        {
            if (kv.Key.Contains(key)) return kv.Value;
        }
        return null;
    }

    private static string GetHeaderByCol(Dictionary<string, int> headerMap, int col)
    {
        foreach (var kv in headerMap)
            if (kv.Value == col) return kv.Key;
        return "";
    }

    private static bool IsRowEmpty(DataRow row)
    {
        for (int i = 0; i < row.Table.Columns.Count; i++)
        {
            var v = row[i];
            if (v != null && v != DBNull.Value && !string.IsNullOrWhiteSpace(v.ToString()))
                return false;
        }
        return true;
    }

    // ===== Cell conversion =====
    private static string ToCellString(DataRow row, int col)
    {
        if (col < 0 || col >= row.Table.Columns.Count) return "";
        var v = row[col];
        return CellToString(v);
    }

    private static string CellToString(object v)
    {
        if (v == null || v == DBNull.Value) return "";

        // Excel time/date often becomes DateTime
        if (v is DateTime dt)
            return dt.ToString("HH:mm");

        // Sometimes time is double (fraction of day)
        if (v is double d)
        {
            // If it looks like a time fraction (0..1), format to HH:mm
            if (d >= 0 && d < 1.5)
            {
                var ts = TimeSpan.FromDays(d);
                return $"{ts.Hours:D2}:{ts.Minutes:D2}";
            }
            // Otherwise keep numeric
            return d.ToString(CultureInfo.InvariantCulture);
        }

        return v.ToString().Trim();
    }

    private static int ToInt(DataRow row, int col)
    {
        if (col < 0 || col >= row.Table.Columns.Count) return 0;
        var v = row[col];
        if (v == null || v == DBNull.Value) return 0;

        if (v is int i) return i;
        if (v is long l) return (int)l;
        if (v is double d) return (int)d;

        var s = v.ToString().Trim();
        if (int.TryParse(s, out var vi)) return vi;
        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var vd)) return (int)vd;

        return 0;
    }

    private static string CellRawToString(object v)
    {
        if (v == null || v == DBNull.Value) return "";
        return v.ToString().Trim();
    }

    // ===== Time normalization =====
    private static string NormalizeTime(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";

        raw = raw.Trim();

        // "10.15" -> "10:15"
        if (raw.Contains("."))
            raw = raw.Replace('.', ':');

        // "10:00:00" -> "10:00"
        var parts = raw.Split(':');
        if (parts.Length >= 2)
        {
            if (int.TryParse(parts[0], out var h) && int.TryParse(parts[1], out var m))
                return $"{h:D2}:{m:D2}";
        }

        // If it's already "HH:mm" return raw (best effort)
        return raw;
    }

    private static int ParseMinutes(string hhmm)
    {
        if (string.IsNullOrWhiteSpace(hhmm)) return int.MaxValue;
        var parts = hhmm.Split(':');
        if (parts.Length < 2) return int.MaxValue;
        if (!int.TryParse(parts[0], out var h)) return int.MaxValue;
        if (!int.TryParse(parts[1], out var m)) return int.MaxValue;
        return h * 60 + m;
    }

    private static string NormalizeHeader(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim();
        s = s.Replace(" ", "");
        s = s.Replace("\u3000", ""); // full-width space
        return s.ToLowerInvariant();
    }
}
#endif
