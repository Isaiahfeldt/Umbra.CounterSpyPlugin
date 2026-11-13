using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Umbra.Common;
using Umbra.Game;

namespace Umbra.CounterSpyPlugin;

[Service]
internal sealed class CounterSpyRepository(
    IObjectTable objectTable,
    IPartyList   party,
    CounterSpySettings settings,
    IPlayer      player
) : IDisposable
{
    private readonly Dictionary<ulong, IGameObject> _objects = [];

    private readonly TargetFilter _filter = new();

    public bool IsPreviewMode { get; set; }

    public List<IGameObject> GetTargets(bool players, bool npcs)
    {
        lock (_objects) {
            return _objects
                .Values.Where(
                    obj => (players && obj is IPlayerCharacter)
                        || (npcs && obj.ObjectKind == ObjectKind.BattleNpc)
                )
                .ToList();
        }
    }

    public bool IsTargetingYou(ulong id)
    {
        lock (_objects) {
            return _objects.ContainsKey(id);
        }
    }

    [OnTick]
    private void OnTick()
    {
        lock (_objects) {
            if (objectTable.LocalPlayer == null || player.IsBetweenAreas || player.IsInCutscene) {
                _objects.Clear();
                return;
            }

            _filter.SetNames(settings.Blacklist);
            var partyIds = party.Select(member => member.EntityId).Where(id => id != 0 && id != 0xE0000000).ToHashSet();

            ulong       playerId  = objectTable.LocalPlayer.GameObjectId;
            Vector3     playerPos = objectTable.LocalPlayer.Position;
            List<ulong> ids       = [];

            // Sort objects by distance to player.
            List<IGameObject> objects =
                objectTable.ToArray().OrderBy(p => Vector3.Distance(playerPos, p.Position)).ToList();

            foreach (var obj in objects) {
                if (obj.IsValid()
                    && (obj is IPlayerCharacter || obj.ObjectKind == ObjectKind.BattleNpc)
                    && obj.TargetObjectId != obj.GameObjectId
                    && obj.GameObjectId != playerId
                    && (IsPreviewMode || obj.TargetObjectId == playerId)
                    && !_filter.ShouldIgnore(obj is IPlayerCharacter, obj.Name.TextValue,
                        settings.IgnoreParty, partyIds.Contains(obj.EntityId))
                    && !obj.IsDead
                   ) {
                    ids.Add(obj.GameObjectId);
                    _objects[obj.GameObjectId] = obj;
                }
            }

            foreach (ulong obj in _objects.Keys.ToArray()) {
                if (!ids.Contains(obj)) {
                    _objects.Remove(obj);
                }
            }
        }
    }

    public void Dispose()
    {
        _objects.Clear();
    }
}
