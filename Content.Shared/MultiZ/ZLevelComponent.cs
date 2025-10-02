using Robust.Shared.Map;

namespace Content.Shared.MultiZ;
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class ZLevelComponent : Component
{
    /// <summary>
    /// Indicates if this map is the primary zlevel.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public bool IsPrimary = false;

    /// <summary>
    /// The MapId of the primary map.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public MapId PrimaryMapId;

    /// <summary>
    /// A list of maps thats part of a multi-z map and their order. Should be the same on all maps in a multi-z map.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public List<MapId> LevelList = [];
}
