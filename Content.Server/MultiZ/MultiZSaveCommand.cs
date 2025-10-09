using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.MultiZ;

[AdminCommand(AdminFlags.Server | AdminFlags.Mapping)]
public sealed class MultiZSaveCommand : LocalizedEntityCommands
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IResourceManager _resourceMgr = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    public override string Command => "multizsave";
    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
                var opts = CompletionHelper.UserFilePath(args[0], _resourceMgr.UserData)
                .Concat(CompletionHelper.ContentFilePath(args[0], _resourceMgr));
                return CompletionResult.FromHintOptions(opts, Loc.GetString("cmd-multizsave-hint-path"));
            case 2:

                return CompletionResult.FromHintOptions(["false", "true"], Loc.GetString("cmd-multizsave-hint-force"));
        }
        if (args.Length >= 3)
        {
            return CompletionResult.FromHint(Loc.GetString("cmd-multizsave-hint-map"));
        }
        return CompletionResult.Empty;
    }

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 3)
        {
            shell.WriteLine(Loc.GetString("cmd-multizsave-help"));
            return;
        }

        if (!ResPath.IsValidPath(args[0]))
        {
            shell.WriteError(Loc.GetString("cmd-multizsave-error-path"));
            return;
        }
        var path = new ResPath(args[0]);

        if (!bool.TryParse(args[1], out var force))
        {
            shell.WriteError(Loc.GetString("cmd-multizsave-error-force"));
            return;
        }

        var mapArgs = args[2..];
        var mapList = new HashSet<EntityUid>();
        foreach (var map in mapArgs)
        {
            if (!int.TryParse(map, out var mapInt))
            {
                shell.WriteError(Loc.GetString("cmd-multizsave-error-map"));
                return;
            }

            var mapId = new MapId(mapInt);
            if (!_mapSystem.TryGetMap(mapId, out var mapUid))
            {
                shell.WriteError(Loc.GetString("cmd-multizsave-error-map"));
                return;
            }

            if (_mapSystem.IsInitialized(mapId) && !force)
            {
                shell.WriteError(Loc.GetString("cmd-multizsave-initialized"));
                return;
            }

            mapList.Add(mapUid.Value);
        }

        shell.WriteLine(Loc.GetString("cmd-multizsave-attempt"));
        var saveSuccess = _mapLoader.TrySaveGeneric(mapList, path, out _);

        if (saveSuccess)
        {
            shell.WriteLine(Loc.GetString("cmd-multizsave-success"));
            return;
        }
        else
        {
            shell.WriteError(Loc.GetString("cmd-multizsave-fail"));
            return;
        }
    }
}
