﻿using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Interfaces
{
    public interface IMuteService : ICrudService<Mute, LisbethBotDbContext>
    {
        Task<(long Id, Mute FoundEntity)> AddOrExtendAsync(MuteReqDto req, bool shouldSave = false);
        Task<Mute> DisableAsync(MuteDisableReqDto entry, bool shouldSave = false);
    }
}
