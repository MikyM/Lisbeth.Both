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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.Ticket;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[SlashModuleLifespan(SlashModuleLifespan.Transient)]
[UsedImplicitly]
public class TicketSlashCommands : ExtendedApplicationCommandModule
{
    public TicketSlashCommands(IDiscordTicketService discordTicketService)
    {
        _discordTicketService = discordTicketService;
    }

    private readonly IDiscordTicketService _discordTicketService;

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [SlashCommand("ticket", "A command that allows managing tickets", false)]
    public async Task TicketHandlerCommand(InteractionContext ctx,
        [Option("action", "Type of action to perform")]
        TicketActionType action,
        [Option("target", "A user or a role to add")]
        SnowflakeObject target)
    {
        if (target is null) throw new ArgumentNullException(nameof(target));

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        Result<DiscordEmbed> result;
        switch (action)
        {
            case TicketActionType.Add:
                var addReq = new TicketAddReqDto(null, null, ctx.Guild.Id, ctx.Channel.Id, ctx.User.Id, target.Id);
                var addReqValidator = new TicketAddReqValidator(ctx.Client);
                await addReqValidator.ValidateAndThrowAsync(addReq);
                result = await this._discordTicketService!.AddToTicketAsync(ctx, addReq);
                break;
            case TicketActionType.Remove:
                var removeReq = new TicketRemoveReqDto(null, null, ctx.Guild.Id, ctx.Channel.Id, ctx.User.Id,
                    target.Id);
                var removeReqValidator = new TicketRemoveReqValidator(ctx.Client);
                await removeReqValidator.ValidateAndThrowAsync(removeReq);
                result = await this._discordTicketService!.RemoveFromTicketAsync(ctx, removeReq);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }

        if (result.IsDefined())
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(result.Entity)
                .AsEphemeral(true));
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral(true));
    }
}
