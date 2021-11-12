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

using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.Application.Results;

namespace Lisbeth.Bot.Application.Services.Database.Interfaces;

public interface ITicketService : ICrudService<Ticket, LisbethBotDbContext>
{
    Task<Result<Ticket>> CloseAsync(TicketCloseReqDto req, bool shouldSave = false);
    Task<Result<Ticket>> CloseAsync(TicketCloseReqDto req, Ticket ticket);
    Task<Result<Ticket>> OpenAsync(TicketOpenReqDto req);
    Task<Result<Ticket>> ReopenAsync(TicketReopenReqDto req, Ticket ticket);
    Task<Result<Ticket>> ReopenAsync(TicketReopenReqDto req);
    Task<Result> SetAddedUsersAsync(Ticket ticket, IEnumerable<ulong> userIds, bool shouldSave = false);
    Task<Result> SetAddedRolesAsync(Ticket ticket, IEnumerable<ulong> roleIds, bool shouldSave = false);
    Task<Result<bool>> IsTicketPrivateAsync(Ticket ticket, DiscordGuild guild);
    Task<Result> CheckAndSetPrivacyAsync(Ticket ticket, DiscordGuild guild);
    Task<Result> CheckForDeletedTicketChannelAsync(ulong channelId, ulong guildId, ulong requestedOnBehalfOfId);
}