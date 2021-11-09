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

using AutoMapper;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Results.Errors;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Services.Database
{
    [UsedImplicitly]
    public class MuteService : CrudService<Mute, LisbethBotDbContext>, IMuteService
    {
        public MuteService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
        {
        }

        public async Task<Result<(long Id, Mute? FoundEntity)>> AddOrExtendAsync(MuteReqDto req, bool shouldSave = false)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var result = await base.GetSingleBySpecAsync<Mute>(new Specification<Mute>(x =>
                x.UserId == req.TargetUserId && x.GuildId == req.GuildId && !x.IsDisabled && !x.Guild.IsDisabled));
            if (!result.IsSuccess)
            {
                var partial = await base.AddAsync(req, shouldSave);
                return Result<(long Id, Mute? FoundEntity)>.FromSuccess((partial.Entity, null));
            }

            var entity = result.Entity;

            if (entity.AppliedUntil > req.AppliedUntil) return (entity.Id, entity);

            var shallowCopy = entity.ShallowCopy();

            base.BeginUpdate(entity);
            entity.AppliedById = req.RequestedOnBehalfOfId;
            entity.AppliedUntil = req.AppliedUntil;
            entity.Reason = req.Reason;

            if (shouldSave) await base.CommitAsync();

            return (entity.Id, shallowCopy);
        }

        public async Task<Result<Mute>> DisableAsync(MuteDisableReqDto entry, bool shouldSave = false)
        {
            if (entry is null) throw new ArgumentNullException(nameof(entry));

            if (!entry.TargetUserId.HasValue || !entry.GuildId.HasValue) throw new InvalidOperationException();

            var result =
                await base.GetSingleBySpecAsync<Mute>(
                    new ActiveExpiredMutePerGuildSpec(entry.TargetUserId.Value, entry.GuildId.Value));
            if (!result.IsSuccess) return Result<Mute>.FromError(new NotFoundError());

            var entity = result.Entity;

            base.BeginUpdate(entity);
            entity.IsDisabled = true;
            entity.LiftedOn = DateTime.UtcNow;
            entity.LiftedById = entry.RequestedOnBehalfOfId;

            if (shouldSave) await base.CommitAsync();

            return entity;
        }
    }
}