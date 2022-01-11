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
using Lisbeth.Bot.Domain.DTOs.Request.Prune;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces;

public interface IDiscordMessageService
{
    Task<Result<DiscordEmbed>> PruneAsync(InteractionContext ctx, PruneReqDto req);
    Task<Result<DiscordEmbed>> PruneAsync(ContextMenuContext ctx, PruneReqDto req);
    Task<Result<DiscordEmbed>> PruneAsync(PruneReqDto req, DiscordChannel channel, DiscordGuild discordGuild,
        DiscordMember moderator, ulong? interactionId = null);

    Task LogMessageUpdatedEventAsync(MessageUpdateEventArgs args);
    Task LogMessageDeletedEventAsync(MessageDeleteEventArgs args);
    Task LogMessageBulkDeletedEventAsync(MessageBulkDeleteEventArgs args);
}