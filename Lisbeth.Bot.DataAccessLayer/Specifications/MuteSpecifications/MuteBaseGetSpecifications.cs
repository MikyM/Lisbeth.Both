﻿// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.DataAccessLayer.Specifications.MuteSpecifications
{
    public class MuteBaseGetSpecifications : Specifications<Mute>
    {
        public MuteBaseGetSpecifications(long? id = null, ulong? userId = null, ulong? guildId = null, ulong? appliedById = null, DateTime? liftedOn = null, DateTime? appliedOn = null, ulong? liftedById = null, int limit = 0)
        {
            if(id != null)
                ApplyFilterCondition(x => x.Id == id);
            if (userId != null)
                ApplyFilterCondition(x => x.UserId == userId);
            if (guildId != null)
                ApplyFilterCondition(x => x.GuildId == guildId);
            if (appliedById != null)
                ApplyFilterCondition(x => x.AppliedById == appliedById);
            if (liftedById != null)
                ApplyFilterCondition(x => x.LiftedById == liftedById);
            if (liftedOn != null)
                ApplyFilterCondition(x => x.LiftedOn == liftedOn);
            if (appliedOn != null)
                ApplyFilterCondition(x => x.AppliedOn == appliedOn);
            if (liftedById != null)
                ApplyFilterCondition(x => x.LiftedById == liftedById);

            ApplyOrderByDescending(x => x.AppliedOn);

            ApplyLimit(limit);
        }
    }
}
