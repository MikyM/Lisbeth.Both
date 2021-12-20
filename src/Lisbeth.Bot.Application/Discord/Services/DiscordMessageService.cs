﻿// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Prune;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request;
using Microsoft.Extensions.Logging;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;
using System.Collections.Generic;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
public class DiscordMessageService : IDiscordMessageService
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly IPruneService _pruneService;
    private readonly ILogger<DiscordMessageService> _logger;
    private readonly IResponseDiscordEmbedBuilder _embedBuilder;
    private readonly IDiscordGuildLoggerService _discordGuildLogger;

    public DiscordMessageService(IDiscordService discord, IPruneService pruneService, IGuildDataService guildDataService,
        ILogger<DiscordMessageService> logger, IResponseDiscordEmbedBuilder embedBuilder,
        IDiscordGuildLoggerService discordGuildLogger)
    {
        _pruneService = pruneService;
        _discord = discord;
        _guildDataService = guildDataService;
        _logger = logger;
        _embedBuilder = embedBuilder;
        _discordGuildLogger = discordGuildLogger;
    }


    public async Task<Result<DiscordEmbed>> PruneAsync(InteractionContext ctx, PruneReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await PruneAsync(req, ctx.Channel, ctx.Guild, ctx.Member, ctx.InteractionId);
    }

    public async Task<Result<DiscordEmbed>> PruneAsync(ContextMenuContext ctx, PruneReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await PruneAsync(req, ctx.Channel, ctx.Guild, ctx.Member, ctx.InteractionId);
    }

    public async Task<Result<DiscordEmbed>> PruneAsync(PruneReqDto req, DiscordChannel channel, DiscordGuild guild,
        DiscordMember moderator, ulong? interactionId = null)
    {
        if (channel is null) throw new ArgumentNullException(nameof(channel));
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (moderator is null) throw new ArgumentNullException(nameof(moderator));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (!moderator.IsModerator())
            return new DiscordNotAuthorizedError();

        await _discordGuildLogger.LogToDiscordAsync(guild, req, DiscordModeration.Prune, moderator);

        int count = 0;

        if (req.Count is not null)
        {
            var messages = await channel.GetMessagesAsync(req.Count.Value);
            await channel.DeleteMessagesAsync(interactionId is null
                ? messages
                : messages.TakeWhile(x => x.Interaction is null || x.Interaction.Id != interactionId));
            count = messages.Count;
        }
        else if (req.IsTargetedMessageDelete.HasValue && req.IsTargetedMessageDelete.Value && req.MessageId is not null)
        {
            await channel.DeleteMessageAsync(await channel.GetMessageAsync(req.MessageId.Value));
            count++;
        }
        else if (req.MessageId is not null)
        {
            List<DiscordMessage> messagesToDelete = new();
            int cycles = 0;
            bool shouldStop = false;
            var targetMessage = await channel.GetMessageAsync(req.MessageId.Value);
            if (targetMessage is null) return new DiscordNotFoundError("Message with given Id was not found");
            var messages = await channel.GetMessagesAsync(1);
            var lastMessage = messages[0];

            while (cycles <= 10 && !shouldStop)
            {
                messagesToDelete.Clear();
                messagesToDelete.AddRange(
                    await channel.GetMessagesBeforeAsync(lastMessage.Id));
                await Task.Delay(300);
                var target = messagesToDelete.FirstOrDefault(x => x.Id == req.MessageId);
                if (target is not null)
                {
                    var index = messagesToDelete.IndexOf(target);
                    messagesToDelete.RemoveRange(index + 1, messagesToDelete.Count - index - 1);
                    shouldStop = true;
                }
                lastMessage = messagesToDelete.Last();
                await Task.Delay(600);
                // keep interaction message
                if (interactionId.HasValue)
                    messagesToDelete.RemoveAll(x => x.Interaction is not null && x.Interaction.Id == interactionId.Value);
                await channel.DeleteMessagesAsync(messagesToDelete);
                count += messagesToDelete.Count;
                cycles++;
            }
        }
        else if (req.TargetAuthorId.HasValue)
        {
            var messages = await channel.GetMessagesAsync();
            var messagesToDelete = messages.Where(x => x.Author.Id == req.TargetAuthorId.Value);

            await channel.DeleteMessagesAsync(messagesToDelete);
        }

        var res = await _pruneService.AddAsync(req, true);
        if (!res.IsDefined(out var id)) return Result<DiscordEmbed>.FromError(res);

        return _embedBuilder.WithType(DiscordModeration.Prune)
            .EnrichFrom(new PruneReqResponseEnricher(req, count))
            .WithCase(id)
            .Build();
    }

    public async Task LogMessageUpdatedEventAsync(MessageUpdateEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        if (args.Author.IsBot || args.MessageBefore.Content == args.Message.Content &&
            args.MessageBefore.Attachments.Count == args.Message.Attachments.Count) return;

        var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(args.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException();

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageUpdatedEventsLogChannelId is null) return;

        DiscordChannel logChannel = args.Guild.Channels
            .FirstOrDefault(x => x.Key == guild.ModerationConfig.MessageUpdatedEventsLogChannelId)
            .Value;

        if (logChannel is null) return;

        string oldContent = args.MessageBefore.Content;
        string newContent = args.Message.Content;
        string oldAttachmentsString = "No attachments";
        string newAttachmentsString = "No attachments";

        var oldAttachments = args.MessageBefore.Attachments;
        var newAttachments = args.Message.Attachments;

        List<string> oldAttachmentUrls = oldAttachments.Select(attachment => attachment.ProxyUrl).ToList();
        List<string> newAttachmentUrls = newAttachments.Select(attachment => attachment.ProxyUrl).ToList();

        if (oldAttachmentUrls.Count != 0)
            oldAttachmentsString = string.Join(Environment.NewLine, oldAttachmentUrls);
        if (newAttachmentUrls.Count != 0)
            newAttachmentsString = string.Join(Environment.NewLine, newAttachmentUrls);

        if (oldContent == "") oldContent = "No content";
        if (newContent == "") newContent = "No content";

        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("Message has been edited");
        embed.WithThumbnail(args.Author.AvatarUrl);
        embed.AddField("Author", $"{args.Author.GetFullUsername()}", true);
        embed.AddField("Author mention", $"{args.Message.Author.Mention}", true);
        embed.AddField("Channel", $"{args.Channel.Mention}", true);
        embed.AddField("Date sent", $"{args.Message.Timestamp}");
        embed.AddField("Old content", oldContent);
        embed.AddField("Old attachments", oldAttachmentsString);
        embed.AddField("New content", newContent);
        embed.AddField("New attachments", newAttachmentsString);
        embed.WithFooter($"Message Id: {args.Message.Id} || Author Id: {args.Message.Author.Id}");
        embed.WithColor(new DiscordColor(guild.EmbedHexColor));

        try
        {
            await _discord.Client.SendMessageAsync(logChannel, embed.Build());
        }
        catch (Exception)
        {
            //log something
        }
    }

    public async Task LogMessageDeletedEventAsync(MessageDeleteEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        if (args.Message.Author.IsBot) return;

        var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(args.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException();

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageDeletedEventsLogChannelId is null) return;

        DiscordChannel logChannel = args.Guild.Channels
            .FirstOrDefault(x => x.Key == guild.ModerationConfig.MessageDeletedEventsLogChannelId)
            .Value;

        if (logChannel is null) return;

        string content = args.Message.Content;
        string attachmentsString = "No attachments";
        var attachments = args.Message.Attachments;
        DiscordUser deletedBy = args.Message.Author;

        await Task.Delay(500);

        var auditLogs = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.MessageDelete);
        var auditLogsBans = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
        var filtered = auditLogs
            .Where(m => m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4))).ToList();
        var filteredBans = auditLogsBans
            .Where(m => m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4))).ToList();

        if (filtered.Count() != 0)
        {
            var deletedLog = (DiscordAuditLogMessageEntry)filtered[0];
            if (deletedLog.Channel == args.Channel && args.Message.Author.Id == deletedLog.Target.Id)
                deletedBy = deletedLog.UserResponsible;
        }

        var attachmentUrls = attachments.Select(attachment => attachment.ProxyUrl).ToList();

        if (content == "") content = "No content";
        if (attachmentUrls.Count != 0) attachmentsString = string.Join(Environment.NewLine, attachmentUrls);

        var embed = new DiscordEmbedBuilder();

        embed.WithThumbnail(args.Message.Author.AvatarUrl);
        embed.AddField("Author", $"{args.Message.Author.GetFullUsername()}", true);
        embed.AddField("Author mention", $"{args.Message.Author.Mention}", true);
        embed.AddField("Channel", $"{args.Channel.Mention}", true);
        if (filteredBans.Count() != 0)
        {
            embed.WithTitle("Message has been deleted due to ban prune");
            embed.AddField("Deleted by", $"{filteredBans[0].UserResponsible.Mention}");
        }
        else
        {
            embed.WithTitle("Message has been deleted");
            embed.AddField("Deleted by", $"{deletedBy.Mention}");
        }

        embed.AddField("Date sent", $"{args.Message.Timestamp}");
        embed.AddField("Content", content);
        embed.AddField("Attachments", attachmentsString);
        embed.WithFooter($"Message Id: {args.Message.Id} || Author Id: {args.Message.Author.Id}");
        embed.WithColor(new DiscordColor(guild.EmbedHexColor));

        try
        {
            await _discord.Client.SendMessageAsync(logChannel, embed.Build());
        }
        catch (Exception)
        {
            //log something
        }
    }

    public async Task LogMessageBulkDeletedEventAsync(MessageBulkDeleteEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(args.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException();

        var guild = res.Entity;

        if (guild.ModerationConfig?.MessageDeletedEventsLogChannelId is null) return;

        DiscordChannel logChannel = args.Guild.Channels
            .FirstOrDefault(x => x.Key == guild.ModerationConfig.MessageDeletedEventsLogChannelId)
            .Value;

        if (logChannel is null) return;

        await Task.Delay(500);

        var auditLogs = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
        var auditBulkLogs = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.MessageBulkDelete);
        var filtered = auditLogs
            .Where(m => m.CreationTimestamp.LocalDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4)))
            .ToList();
        var filteredBulk = auditBulkLogs
            .Where(m => m.CreationTimestamp.LocalDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4)))
            .ToList();

        foreach (var msg in args.Messages)
        {
            if (msg?.Author is null || msg.Author.IsBot) continue;

            var embed = new DiscordEmbedBuilder();
            var attachments = msg.Attachments;
            var attachmentsString = "No attachments";

            var attachmentUrls = attachments.Select(attachment => attachment.ProxyUrl).ToList();

            if (attachmentUrls.Count != 0) attachmentsString = string.Join(Environment.NewLine, attachmentUrls);

            var content = msg.Content;
            if (content == "") content = "No content";

            embed.WithThumbnail(msg.Author.AvatarUrl);
            embed.AddField("Author", $"{msg.Author.Username}#{msg.Author.Discriminator}", true);
            embed.AddField("Author mention", $"{msg.Author.Mention}", true);
            embed.AddField("Channel", $"{args.Channel.Mention}", true);

            if (filtered.Count() != 0)
            {
                embed.WithTitle("Message has been deleted due to ban prune");
                embed.AddField("Pruned by", $"{filtered[0].UserResponsible.Mention}");
            }
            else if (filteredBulk.Count() != 0)
            {
                embed.WithTitle("Message has been deleted via prune command");
                embed.AddField("Pruned by", $"{filteredBulk[0].UserResponsible.Mention}");
            }
            else
            {
                embed.WithTitle("Message has been deleted in a bulk deletion action");
                embed.AddField("Pruned by", "Unknown");
            }

            embed.AddField("Date sent", $"{msg.Timestamp}");
            embed.AddField("Content", content);
            embed.AddField("Attachments", attachmentsString);
            embed.WithFooter($"Message ID: {msg.Id} || Author ID: {msg.Author.Id}");
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));

            try
            {
                await _discord.Client.SendMessageAsync(logChannel, embed.Build());
            }
            catch (Exception)
            {
                //log something
            }

            await Task.Delay(800);
        }
    }
}