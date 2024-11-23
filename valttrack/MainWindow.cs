using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace AltTrack;

public class MainWindow(string dbPath) : Window("Alt Tracker"), IDisposable
{
    private Database _db = new(dbPath);
    private ulong[] _contentIds = new ulong[100];

    public void Dispose()
    {
        _db.Dispose();
    }

    public unsafe override void PreOpenCheck()
    {
        var gom = GameObjectManager.Instance();
        for (int i = 0; i < _contentIds.Length; i++)
        {
            var chr = GetIfPlayer(gom->Objects.IndexSorted[i * 2].Value);
            if (chr == null || _contentIds[i] == chr->ContentId)
                continue;

            _contentIds[i] = chr->ContentId;
            _db.Update(chr->AccountId, chr->ContentId, chr->NameString, chr->HomeWorld);
        }
    }

    public unsafe override void Draw()
    {
        var target = GetIfPlayer(TargetSystem.Instance()->GetTargetObject());
        if (target != null)
        {
            var alts = _db.Entries(target->AccountId);
            alts.Reverse();
            DrawCharacter(alts, target->ContentId);

            HashSet<ulong> visited = [target->ContentId];
            foreach (var alt in alts)
            {
                if (visited.Contains(alt.ContentId))
                    continue;
                visited.Add(alt.ContentId);
                DrawCharacter(alts, alt.ContentId);
            }
        }
    }

    public void DrawCharacter(List<DBCharacter> characters, ulong contentId)
    {
        bool first = true;
        foreach (var prev in characters.Where(prev => prev.ContentId == contentId))
        {
            ImGui.TextUnformatted($"{(first ? $"{prev.AccountId:X}.{prev.ContentId:X}:" : "-")} {prev.Name} @ {Service.LuminaRow<World>(prev.HomeWorld)?.Name}");
            first = false;
        }
    }

    private static unsafe Character* GetIfPlayer(GameObject* obj) => obj != null && obj->ObjectKind == ObjectKind.Pc && obj->IsCharacter() ? (Character*)obj : null;
}
