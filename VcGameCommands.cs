using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCord;
using NetCord.Services.ApplicationCommands;
using Newtonsoft.Json;



public class VcGameCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("mute_everyone_in_call_with_me", "mutes everyone in call (including you)", Contexts = [InteractionContextType.Guild])]
    public async Task<string> MuteAllInCall()
    {
        var guildId = Context.Interaction.GuildId!.Value;
        var userId = Context.User.Id;

        if (!Context.Client.Cache.Guilds.TryGetValue(guildId, out var guild))
            return "`ERROR:` Couldn't find guild!";

        if (!guild.VoiceStates.TryGetValue(userId, out var myVoiceState) || myVoiceState.ChannelId is null)
            return "`ERROR:` You need to be in a voice channel first!";

        var channelId = myVoiceState.ChannelId.Value;

        var muteTargets = guild.VoiceStates.Values
            .Where(vs => vs.ChannelId == channelId)
            .ToList();


        foreach (var vs in muteTargets)
            await Context.Client.Rest.ModifyGuildUserAsync(guildId, vs.UserId, x => x.Muted = true);

        return $"`Task Compleate:` Muted {muteTargets.Count} users in <#{channelId}>!";
    }

    [SlashCommand("unmute_everyone_in_call_with_me", "unmutes everyone in call (including you)", Contexts = [InteractionContextType.Guild])]
    public async Task<string> UnMuteAllInCall()
    {
        var guildId = Context.Interaction.GuildId!.Value;
        var userId = Context.User.Id;

        if (!Context.Client.Cache.Guilds.TryGetValue(guildId, out var guild))
            return "`ERROR:` Couldn't find guild!";

        if (!guild.VoiceStates.TryGetValue(userId, out var myVoiceState) || myVoiceState.ChannelId is null)
            return "`ERROR:` You need to be in a voice channel first!";

        var channelId = myVoiceState.ChannelId.Value;

        var muteTargets = guild.VoiceStates.Values
            .Where(vs => vs.ChannelId == channelId && vs.IsMuted == true)
            .ToList();


        foreach (var vs in muteTargets)
            await Context.Client.Rest.ModifyGuildUserAsync(guildId, vs.UserId, x => x.Muted = false);

        return $"`Task Compleate:` Unmuted {muteTargets.Count} users in <#{channelId}>!";
    }

}