using System;
using System.Numerics;
using System.IO;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using Umbra.Common;

namespace Umbra.CounterSpyPlugin;

[Service]
internal sealed class CounterSpySettings : IDisposable
{
    public bool IgnoreParty => _data.IgnoreParty;
    public string Blacklist => _data.Blacklist ?? "";
    private SettingsData _data = new();
    private readonly string _path;
    private readonly IPluginLog _log;
    private string? _error;

    public sealed class SettingsData
    {
        public bool IgnoreParty { get; set; }
        public string Blacklist { get; set; } = "";
    }

    private readonly ICommandManager _commands;
    private bool _open;

    public CounterSpySettings(ICommandManager commands, IPluginLog log)
    {
        _commands = commands;
        _log = log;
        _path = Path.Combine(Framework.DalamudPlugin.GetPluginConfigDirectory(), "CounterSpy.settings.json");
        try {
            if (File.Exists(_path))
                _data = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_path)) ?? new();
        } catch (Exception ex) {
            _error = "Could not load Counter Spy settings. Changes will not be saved until the file is repaired.";
            log.Error(ex, "Unable to load Counter Spy settings");
        }
        commands.AddHandler("/counterspy", new CommandInfo(OnCommand) {
            HelpMessage = "Configure Counter Spy's ignored players and party filter.",
        });
    }

    public void Open() => _open = true;
    private void OnCommand(string command, string args) => Open();

    [OnDraw]
    private void OnDraw()
    {
        if (!_open) return;
        ImGui.SetNextWindowSize(new Vector2(460, 360), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Counter Spy - Targeting filters", ref _open)) {
            var ignoreParty = IgnoreParty;
            if (ImGui.Checkbox("Ignore party members", ref ignoreParty)) {
                _data.IgnoreParty = ignoreParty;
                Save();
            }
            ImGui.TextWrapped("Ignored players do not appear in Counter Spy markers, glow effects or the toolbar list. NPCs are unaffected.");
            ImGui.Separator();
            ImGui.TextUnformatted("Ignored character names (one full name per line)");
            var names = Blacklist;
            if (ImGui.InputTextMultiline("##Blacklist", ref names, 16384, new Vector2(-1, 150))) {
                _data.Blacklist = names;
                Save();
            }
            if (_error != null) ImGui.TextWrapped(_error);
            ImGui.TextWrapped("Names match exactly, ignoring capitalization and surrounding spaces. A name applies on all worlds. Commas and semicolons also separate names. Changes save automatically and apply on the next targeting update, including in preview mode.");
        }
        ImGui.End();
    }

    private void Save()
    {
        if (_error != null) return;
        try {
            File.WriteAllText(_path + ".tmp", JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(_path + ".tmp", _path, true);
        } catch (Exception ex) {
            _error = "Could not save Counter Spy settings. See the Dalamud log; reload after correcting the file access issue.";
            _log.Error(ex, "Unable to save Counter Spy settings");
        }
    }

    public void Dispose() => _commands.RemoveHandler("/counterspy");
}
