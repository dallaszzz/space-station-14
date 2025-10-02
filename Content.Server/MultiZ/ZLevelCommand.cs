using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Content.Shared.MultiZ;

namespace Content.Server.MultiZ
{
    [AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
    public sealed class ZLevelCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly SharedMapSystem _mapSystem = default!;
        [Dependency] private readonly IEntityManager _entity = default!;

        public override string Command => "zlevel";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length == 0)
            {
                shell.WriteError(Loc.GetString("cmd-zlevel-not-enough-args"));
                return;
            }

            if (!int.TryParse(args[0], out var targetMap))
            {
                shell.WriteError(Loc.GetString("cmd-zlevel-map-not-int"));
                return;
            }

            MapId targetMapId;
            targetMapId = new MapId(targetMap);

            if (!_mapSystem.MapExists(targetMapId))
            {
                shell.WriteError(Loc.GetString("cmd-zlevel-map-doesnt-exist"));
                return;
            }

            if (args.Length == 1)
            {
                NewMap(shell, targetMapId, 1);
            }

            if (args.Length == 2)
            {
                if (!int.TryParse(args[1], out var count))
                {
                    shell.WriteError(Loc.GetString("cmd-zlevel-count-not-int"));
                    return;
                }

                NewMap(shell, targetMapId, count);
            }

            shell.WriteLine(Loc.GetString("cmd-hint-mapping-path"));
            return;
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
                shell.WriteError(Loc.GetString("cmd-zlevel-too-many-args"));
                return;
            }

            if (!_entity.EnsureComponent<ZLevelComponent>(uid.Value, out var primaryComp))
            {
                primaryComp.IsPrimary = true;
                primaryComp.LevelList.Add(primary);
            }

            bool init = _mapSystem.IsInitialized(primary);
            for (var i = 0; i < count; i++)
            {
                var newUid = _mapSystem.CreateMap(out var newMapId, init);
                primaryComp.LevelList.Add(newMapId);

                var newComp = _entity.AddComponent<ZLevelComponent>(newUid);
                newComp.PrimaryMapId = primary;
            }

            // Update the levellist on the comp for each map
            for (var i = 0; i < primaryComp.LevelList.Count; i++)
            {
                if (!_mapSystem.TryGetMap(primaryComp.LevelList[i], out var listMap))
                {
                    shell.WriteError(Loc.GetString("cmd-zlevel-invalid-map-with-list"));
                    return;
                }
                var listMapComp = _entity.GetComponent<ZLevelComponent>(listMap.Value);
                listMapComp.LevelList = primaryComp.LevelList;
            }

            shell.WriteLine(Loc.GetString("cmd-zlevel-command-finished", ("count", count)));
            return;
        }
    }
}
