﻿using System.Collections.Generic;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;


[UsedImplicitly]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class ModUtilSlashCommands : ExtendedApplicationCommandModule
{
    private readonly IGuildDataService _guildDataService;

    public ModUtilSlashCommands(IGuildDataService guildDataService)
    {
        _guildDataService = guildDataService;

    }
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [SlashCommand("identity", "Command that allows checking information about a member.", false)]
    [UsedImplicitly]
    public async Task IdentityCommand(InteractionContext ctx,
        [Option("user", "User to identify")] DiscordUser user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        var res = await _guildDataService!.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(ctx.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException("Guild not found in database");

        var member = (DiscordMember)user;

        var embed = new DiscordEmbedBuilder();
        embed.WithThumbnail(member.AvatarUrl);
        embed.WithTitle("Member information");
        embed.AddField("Member's identity", $"{user.GetFullUsername()}", true);
        embed.AddField("Joined guild", $"{member.JoinedAt.ToString(CultureInfo.CurrentCulture)}");
        embed.AddField("Account created", $"{member.CreationTimestamp.ToString(CultureInfo.CurrentCulture)}");
        embed.WithColor(new DiscordColor(res.Entity.EmbedHexColor));
        embed.WithFooter($"Member Id: {member.Id}");

        await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed.Build())
            .AsEphemeral());
    }
    
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [SlashCommand("booster", "Command that allows checking information about boosters.", false)]
    [UsedImplicitly]
    public async Task BoosterCommand(InteractionContext ctx,
        [Option("action", "Action type")] BoosterActionType actionType,
        [Option("user", "User to check if any")] DiscordUser? user = null)
    {
        _ = ctx.DeferAsync();

        var res = await _guildDataService.GetSingleBySpecAsync(
            new ActiveGuildByDiscordIdWithBoostersSpecifications(ctx.Guild.Id));

        if (!res.IsDefined(out var guild)) 
            throw new ArgumentException("Guild not found in database");

        var member = (DiscordMember?)user;

        switch (actionType)
        {
            case BoosterActionType.Check:
                if (member is null)
                    throw new ArgumentException("User must be supplied if using Check action");
                var embed = new DiscordEmbedBuilder();

                embed.WithTitle("Booster information");
                embed.WithThumbnail(member.AvatarUrl);
                embed.WithColor(new DiscordColor(guild.EmbedHexColor));
                var isBoosting = member.Roles.Any(x => x.Tags.IsPremiumSubscriber);
                var discordBoostDate = member.PremiumSince;
                var dbBooster = (await _guildDataService.GetServerBoosterAsync(ctx.Guild.Id, member.Id)).Entity;

                var date = discordBoostDate is not null && discordBoostDate != DateTimeOffset.MinValue
                    ? discordBoostDate
                    : dbBooster is null 
                        ? null
                        : new DateTimeOffset(dbBooster.BoostingSince, TimeSpan.Zero);
                
                embed.AddField("Currently boosting", isBoosting ? "Yes" : "No", true);
                
                switch (isBoosting)
                {
                    case false:
                        embed.AddField("Boosted previously", dbBooster is not null ? "Yes" : "Unknown", true);
                        break;
                    case true:
                        embed.AddField("Boosting since", date.HasValue ? date.Value.ToString() : "Unknown");
                        embed.AddField("Boosting for", date.HasValue
                            ? $"{Math.Round(DateTime.UtcNow.Subtract(date.Value.UtcDateTime).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days"
                            : "Unknown");
                        break;
                }
                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed.Build()));
                break;
            case BoosterActionType.History:
                var intr = ctx.Client.GetInteractivity();
                var pages = new List<Page>();
                var chunked = (guild.ServerBoosters ?? throw new ArgumentNullException()).Where(x => !x.IsDisabled)
                    .OrderBy(x => x.BoostingSince)
                    .Chunk(10)
                    .OrderByDescending(x => x.Length)
                    .ToList();

                int pageNumber = 1;
                
                foreach (var chunk in chunked)
                {
                    var embedBuilder = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                        .WithTitle("Booster history")
                        .WithFooter($"Current page: {pageNumber} | Total pages: {chunked.Count}");

                    foreach (var booster in chunk)
                    {
                        var memberHistory = await ctx.Guild.GetMemberAsync(booster.UserId);
                        embedBuilder.AddField(memberHistory.GetFullUsername(),
                            $"Is boosting: {!booster.IsDisabled}\nLast boost date: {booster.BoostingSince.ToString(CultureInfo.InvariantCulture)}{(booster.IsDisabled ? string.Empty : $"\nBoosting for: {Math.Round(DateTime.UtcNow.Subtract(booster.BoostingSince.ToUniversalTime()).TotalDays, 2).ToString(CultureInfo.InvariantCulture)}")}");
                        await Task.Delay(500);
                    }

                    pages.Add(new Page("", embedBuilder));
                    pageNumber++;
                }
                
                
                if (pages.Any())
                    await intr.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages, null,
                        null, null, default, true);
                else
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Booster history").WithDescription("No history entries available.").WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            case BoosterActionType.Active:
                var intrActive = ctx.Client.GetInteractivity();
                var pagesActive = new List<Page>();
                var chunkedActive = (guild.ServerBoosters?.Where(x => !x.IsDisabled) ?? throw new ArgumentNullException()).Where(x => !x.IsDisabled)
                    .OrderBy(x => x.BoostingSince)
                    .Chunk(10)
                    .OrderByDescending(x => x.Length)
                    .ToList();

                int pageNumberActive = 1;
                
                foreach (var chunk in chunkedActive)
                {
                    var embedBuilderActive = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                        .WithTitle("Boosters")
                        .WithFooter($"Current page: {pageNumberActive} | Total pages: {chunkedActive.Count}");

                    foreach (var booster in chunk)
                    {
                        var memberActive = await ctx.Guild.GetMemberAsync(booster.UserId);
                        embedBuilderActive.AddField(memberActive.GetFullUsername(),
                            $"Last boost date: {booster.BoostingSince.ToString(CultureInfo.InvariantCulture)}\nBoosting for: {Math.Round(DateTime.UtcNow.Subtract(booster.BoostingSince.ToUniversalTime()).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days");
                        await Task.Delay(500);
                    }

                    pagesActive.Add(new Page("", embedBuilderActive));
                    pageNumberActive++;
                }

                if (pagesActive.Any())
                    await intrActive.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pagesActive, null,
                        null, null, default, true);
                else
                    _ = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Boosters").WithDescription("No active boosters found.").WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            case BoosterActionType.ActiveByDiscord:
                var intrActiveDisc = ctx.Client.GetInteractivity();
                var pagesActiveDisc = new List<Page>();
                var membersActiveDisc = await ctx.Guild.GetAllMembersAsync();
                var chunkedActiveDisc = membersActiveDisc.Where(x => x.Roles.Any(y => y.Tags.IsPremiumSubscriber))
                    .OrderBy(x => x.PremiumSince!.Value)
                    .Chunk(10)
                    .OrderByDescending(x => x.Length)
                    .ToList();
                
                int pageNumberActiveDisc = 1;
                
                foreach (var chunk in chunkedActiveDisc)
                {
                    var embedBuilderActiveDisc = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                        .WithTitle("Boosters")
                        .WithFooter($"Current page: {pageNumberActiveDisc} | Total pages: {chunkedActiveDisc.Count}");

                    foreach (var booster in chunk)
                    {
                        embedBuilderActiveDisc.AddField(booster.GetFullUsername(),
                            $"Last boost date: {booster.PremiumSince!.Value.ToString(CultureInfo.InvariantCulture)}\nBoosting for: {Math.Round(DateTime.UtcNow.Subtract(booster.PremiumSince!.Value.UtcDateTime).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days");
                    }

                    pagesActiveDisc.Add(new Page("", embedBuilderActiveDisc));
                    pageNumberActiveDisc++;
                }
                
                if (pagesActiveDisc.Any())
                    await intrActiveDisc.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pagesActiveDisc, null,
                      null, null, default, true);
                else
                    _ = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Boosters").WithDescription("No active boosters found.").WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            case BoosterActionType.Backtrack:
                var members = await ctx.Guild.GetAllMembersAsync();
                foreach (var memberBacktrack in members)
                {
                    if (!memberBacktrack.Roles.Any(x => x.Tags.IsPremiumSubscriber))
                        continue;
                    
                    var dateBacktrack = memberBacktrack.PremiumSince is not null &&
                                        memberBacktrack.PremiumSince != DateTimeOffset.MinValue
                        ? memberBacktrack.PremiumSince.Value.UtcDateTime
                        : DateTime.UtcNow;
                    
                    _ = await _guildDataService.AddServerBoosterAsync(ctx.Guild.Id, memberBacktrack.Id, dateBacktrack);
                }

                _ = await _guildDataService.CommitAsync();

                _ = await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(
                    new DiscordEmbedBuilder().WithDescription("Backtracking server boosters has finished successfully!")
                        .WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }
    }
}
