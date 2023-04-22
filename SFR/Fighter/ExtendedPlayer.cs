﻿using System;
using System.Collections.Generic;
using SFD;
using SFD.Sounds;
using SFDGameScriptInterface;
using SFR.Fighter.Jetpacks;
using SFR.Sync.Generic;

namespace SFR.Fighter;

/// <summary>
///     Since we need to save additional data into the player instance
///     we use this file to "extend" the player class.
/// </summary>
internal sealed class ExtendedPlayer : IEquatable<Player>
{
    internal static readonly List<ExtendedPlayer> ExtendedPlayers = new();
    internal readonly Player Player;
    internal readonly TimeSequence Time = new();
    internal GenericJetpack GenericJetpack;

    internal JetpackType JetpackType = JetpackType.None;
    internal bool PrepareJetpack = false;

    internal bool Stickied;

    internal ExtendedPlayer(Player player) => Player = player;

    internal bool RageBoost
    {
        get => Time.RageBoost > 0f;
        set => Time.RageBoost = value ? TimeSequence.RageBoostTime : 0f;
    }

    public bool Equals(Player other) => other?.ObjectID == Player.ObjectID;

    internal void ApplyStickiedBoost()
    {
        SoundHandler.PlaySound("StrengthBoostStart", Player.Position, Player.GameWorld);
        var modifiers = Player.GetModifiers();
        modifiers.SprintSpeedModifier = 1.6f;
        modifiers.RunSpeedModifier = 1.6f;
        Player.SetModifiers(modifiers);

        // avoid the server from reposition the player due to excessive speed.
        // TODO: Implement a proper mechanics in Player.GetTopSpeed()
        // _player.SpeedBoostActive = true;
        Stickied = true;
    }

    internal void DisableStickiedBoost()
    {
        SoundHandler.PlaySound("StrengthBoostStop", Player.Position, Player.GameWorld);
        var modifiers = Player.GetModifiers();
        modifiers.SprintSpeedModifier = 1f;
        modifiers.RunSpeedModifier = 1f;
        Player.SetModifiers(modifiers);

        // _player.SpeedBoostActive = false; // temp
        Stickied = false;
    }

    // TODO: Change other methods instead of using modifiers, like strength boost & speed boost do
    internal void ApplyRageBoost()
    {
        // var modifiers = _player.GetModifiers();
        var modifiers = new PlayerModifiers(true)
        {
            SprintSpeedModifier = 1.3f,
            RunSpeedModifier = 1.3f,
            SizeModifier = 1.05f,
            MeleeForceModifier = 1.2f
        };
        Player.SetModifiers(modifiers);
        RageBoost = true;
        GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, Player.ObjectID, GetStates()));
    }

    public object[] GetStates()
    {
        object[] states = new object[4];
        states[0] = RageBoost;
        states[1] = PrepareJetpack;
        states[2] = (int)JetpackType;
        states[3] = (int)(GenericJetpack?.Fuel?.CurrentValue ?? 100);

        return states;
    }

    // TODO: Change other methods instead of using modifiers, like strength boost & speed boost do
    internal void DisableRageBoost()
    {
        SoundHandler.PlaySound("StrengthBoostStop", Player.Position, Player.GameWorld);
        // var modifiers = _player.GetModifiers();
        // modifiers.SprintSpeedModifier = 1f;
        // modifiers.RunSpeedModifier = 1f;
        // modifiers.SizeModifier = 1f;
        // modifiers.MeleeForceModifier = 1f;
        var modifiers = new PlayerModifiers(true);
        Player.SetModifiers(modifiers);
        GenericData.SendGenericDataToClients(new GenericData(DataType.ExtraClientStates, Player.ObjectID, GetStates()));
    }

    internal class TimeSequence
    {
        internal const float RageBoostTime = 16000f;
        internal float RageBoost;
    }
}