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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Results.Errors;

namespace Lisbeth.Bot.Application.Services;

[UsedImplicitly]
public class MuteCheckService : IMuteCheckService
{
    private readonly IGuildService _guildService;
    private readonly IMuteService _muteService;

    public MuteCheckService(IMuteService muteService, IGuildService guildService)
    {
        _muteService = muteService;
        _guildService = guildService;
    }

    public async Task<Result> CheckForNonBotMuteActionAsync(ulong targetId, ulong guildId,
        ulong requestedOnBehalfOfId,
        IReadOnlyList<DiscordRole> rolesBefore, IReadOnlyList<DiscordRole> rolesAfter)
    {
        await Task.Delay(1000);

        var result = await _guildService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpecifications(guildId));

        if (!result.IsDefined() || result.Entity.ModerationConfig is null)
            return Result.FromError(new NotFoundError());

        bool wasMuted = rolesBefore.Any(x => x.Id == result.Entity.ModerationConfig.MuteRoleId);
        bool isMuted = rolesAfter.Any(x => x.Id == result.Entity.ModerationConfig.MuteRoleId);

        switch (wasMuted)
        {
            case true when !isMuted:
                await _muteService.DisableAsync(new MuteDisableReqDto(targetId, guildId, requestedOnBehalfOfId));
                break;
            case false when isMuted:
                await _muteService.AddOrExtendAsync(new MuteReqDto(targetId, guildId, requestedOnBehalfOfId,
                    DateTime.MaxValue));
                break;
        }

        return Result.FromSuccess();
    }
}