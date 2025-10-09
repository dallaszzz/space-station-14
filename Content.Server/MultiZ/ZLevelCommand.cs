using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Content.Shared.MultiZ;

namespace Content.Server.MultiZ;

[AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
public sealed class ZLevelCommand : LocalizedEntityCommands
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IEntityManager _entity = default!;

    public override string Command => "zlevel";
    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
                return CompletionResult.FromHint(Loc.GetString("cmd-zlevel-hint-mapid"));
            case 2:
                return CompletionResult.FromHint(Loc.GetString("cmd-zlevel-hint-count"));
        }
        return CompletionResult.Empty;
    }
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0)
        {
            shell.WriteError(Loc.GetString("cmd-zlevel-help"));
            return;
        }

        if (!int.TryParse(args[0], out var targetMap))
        {
            shell.WriteError(Loc.GetString("cmd-zlevel-map-not-int"));
            return;
        }

        var targetMapId = new MapId(targetMap);

        if (!_mapSystem.MapExists(targetMapId))
        {
            shell.WriteError(Loc.GetString("cmd-zlevel-map-doesnt-exist"));
            return;
        }

        var count = 1;
        if (args.Length >= 2 && !int.TryParse(args[1], out count))
        {
            shell.WriteError(Loc.GetString("cmd-zlevel-count-not-int"));
            return;
        }

        NewMap(shell, targetMapId, count);
    }

    /// <summary>
    /// Creates new maps with z-levels setup automaticly from a primary map and a count of zlevels.
    /// </summary>
    /// <param name="shell"></param>
    /// <param name="primary"></param>
    /// <param name="count"></param>
    public void NewMap(IConsoleShell shell, MapId primary, int count)
    {
        if (!_mapSystem.TryGetMap(primary, out var uid))
        {
            shell.WriteError(Loc.GetString("cmd-zlevel-uid-fail"));
            return;
        }

        if (!_entity.EnsureComponent<ZLevelComponent>(uid.Value, out var primaryComp))
        {
            primaryComp.IsPrimary = true;
            primaryComp.LevelList.Add(uid.Value);
        }

        // Creates the maps and gives them the zlevel component
        var init = _mapSystem.IsInitialized(primary);
        for (var i = 0; i < count; i++)
        {
            var newUid = _mapSystem.CreateMap(out var newMapId, init);
            primaryComp.LevelList.Add(newUid);

            var newComp = _entity.AddComponent<ZLevelComponent>(newUid);
            newComp.PrimaryMapId = uid.Value;
        }

        // Update the levellist on the comp for each map
        foreach (var listMapUid in primaryComp.LevelList)
        {
            var listMapComp = _entity.GetComponent<ZLevelComponent>(listMapUid);
            listMapComp.LevelList = primaryComp.LevelList;
        }

        shell.WriteLine(Loc.GetString("cmd-zlevel-command-finished", ("count", count)));
    }
}
