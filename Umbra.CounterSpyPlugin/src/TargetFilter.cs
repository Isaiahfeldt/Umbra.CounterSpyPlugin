using System;
using System.Collections.Generic;

namespace Umbra.CounterSpyPlugin;

internal sealed class TargetFilter
{
    private string _source = "";
    private readonly HashSet<string> _names = new(StringComparer.OrdinalIgnoreCase);

    public void SetNames(string? value)
    {
        value ??= "";
        if (_source == value) return;
        _source = value;
        _names.Clear();
        foreach (var name in value.Split(['\r', '\n', ',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            _names.Add(name);
    }

    public bool ShouldIgnore(bool isPlayer, string name, bool ignoreParty, bool isPartyMember) =>
        isPlayer && ((ignoreParty && isPartyMember) || _names.Contains(name.Trim()));
}
