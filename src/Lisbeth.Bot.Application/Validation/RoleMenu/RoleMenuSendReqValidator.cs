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
using FluentValidation;
using Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Validation.RoleMenu;

public class RoleMenuSendReqValidator : AbstractValidator<RoleMenuSendReqDto>
{
    public RoleMenuSendReqValidator(IDiscordService discordService) : this(discordService.Client)
    {
    }

    public RoleMenuSendReqValidator(DiscordClient discord)
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id).NotEmpty().When(x => x.Name is not null && !x.GuildId.HasValue);
        RuleFor(x => x.Name).NotEmpty().When(x => !x.Id.HasValue);
        RuleFor(x => x.GuildId)
            .NotEmpty()
            .When(x => !x.Id.HasValue)
            .DependentRules(x => x.SetAsyncValidator(new DiscordGuildIdValidator<RoleMenuSendReqDto>(discord)));
        RuleFor(x => x.RequestedOnBehalfOfId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordUserIdValidator<RoleMenuSendReqDto>(discord)));
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .DependentRules(x => x.SetAsyncValidator(new DiscordChannelIdValidator<RoleMenuSendReqDto>(discord)));
    }
}