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
using Microsoft.EntityFrameworkCore;
using MikyM.Common.Application.Interfaces;
using MikyM.Common.DataAccessLayer.Repositories;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using MikyM.Common.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MikyM.Common.Application.Results;

namespace MikyM.Common.Application.Services
{
    public class CrudService<TEntity, TContext> : ReadOnlyService<TEntity, TContext>, ICrudService<TEntity, TContext>
        where TEntity : AggregateRootEntity where TContext : DbContext
    {
        public CrudService(IMapper mapper, IUnitOfWork<TContext> uof) : base(mapper, uof)
        {
        }

        public virtual async Task<Result<long>> AddAsync<TPost>(TPost entry, bool shouldSave = false) where TPost : class
        {
            if (entry  is null) throw new ArgumentNullException(nameof(entry));

            TEntity entity;
            if (entry is TEntity rootEntity)
            {
                entity = rootEntity;
                UnitOfWork.GetRepository<Repository<TEntity>>()?.Add(entity);
            }
            else
            {
                entity = Mapper.Map<TEntity>(entry);
                UnitOfWork.GetRepository<Repository<TEntity>>()?.Add(entity);
            }

            if (!shouldSave) return 0;
            await CommitAsync();
            return Result<long>.FromSuccess(entity.Id);
        }

        public virtual async Task<Result<IEnumerable<long>>> AddRangeAsync<TPost>(IEnumerable<TPost> entries,
            bool shouldSave = false) where TPost : class
        {
            if (entries  is null) throw new ArgumentNullException(nameof(entries));

            IEnumerable<TEntity> entities;

            if (entries is IEnumerable<TEntity> rootEntities)
            {
                entities = rootEntities;
                UnitOfWork.GetRepository<Repository<TEntity>>()?.AddRange(entities);
            }
            else
            {
                entities = Mapper.Map<IEnumerable<TEntity>>(entries);
                UnitOfWork.GetRepository<Repository<TEntity>>()?.AddRange(entities);
            }

            if (!shouldSave) return new List<long>();
            await CommitAsync();
            return Result<IEnumerable<long>>.FromSuccess(entities.Select(e => e.Id).ToList());
        }

        public virtual Result BeginUpdate<TPatch>(TPatch entry) where TPatch : class
        {
            if (entry  is null) throw new ArgumentNullException(nameof(entry));

            if (entry is TEntity rootEntity)
                UnitOfWork.GetRepository<Repository<TEntity>>()?.BeginUpdate(rootEntity);
            else
                UnitOfWork.GetRepository<Repository<TEntity>>()?.BeginUpdate(Mapper.Map<TEntity>(entry));

            return Result.FromSuccess();
        }

        public virtual Result BeginUpdateRange<TPatch>(IEnumerable<TPatch> entries) where TPatch : class
        {
            if (entries  is null) throw new ArgumentNullException(nameof(entries));

            if (entries is IEnumerable<TEntity> rootEntities)
                UnitOfWork.GetRepository<Repository<TEntity>>()?.BeginUpdateRange(rootEntities);
            else
                UnitOfWork.GetRepository<Repository<TEntity>>()
                    ?.BeginUpdateRange(Mapper.Map<IEnumerable<TEntity>>(entries));

            return Result.FromSuccess();
        }

        public virtual async Task<Result<long>> AddOrUpdateAsync<TPut>(TPut entry, bool shouldSave = false) where TPut : class
        {
            if (entry  is null) throw new ArgumentNullException(nameof(entry));

            TEntity entity;

            if (entry is TEntity rootEntity)
            {
                entity = rootEntity;
                UnitOfWork.GetRepository<Repository<TEntity>>()?.AddOrUpdate(entity);
            }
            else
            {
                entity = Mapper.Map<TEntity>(entry);
                UnitOfWork.GetRepository<Repository<TEntity>>()?.AddOrUpdate(entity);
            }

            if (!shouldSave) return 0;
            await CommitAsync();
            return Result<long>.FromSuccess(entity.Id);
        }

        public virtual async Task<Result<IEnumerable<long>>> AddOrUpdateRangeAsync<TPut>(IEnumerable<TPut> entries,
            bool shouldSave = false) where TPut : class
        {
            if (entries  is null) throw new ArgumentNullException(nameof(entries));

            IEnumerable<TEntity> entities;

            if (entries is IEnumerable<TEntity> rootEntities)
            {
                entities = rootEntities;
                UnitOfWork.GetRepository<Repository<TEntity>>()?.AddOrUpdateRange(entities);
            }
            else
            {
                entities = Mapper.Map<IEnumerable<TEntity>>(entries);
                UnitOfWork.GetRepository<Repository<TEntity>>()
                    ?.AddOrUpdateRange(Mapper.Map<IEnumerable<TEntity>>(entities));
            }

            if (!shouldSave) return new List<long>();
            await CommitAsync();
            return Result<IEnumerable<long>>.FromSuccess(entities.Select(e => e.Id).ToList());
        }

        public virtual async Task<Result> DeleteAsync<TDelete>(TDelete entry, bool shouldSave = false) where TDelete : class
        {
            if (entry  is null) throw new ArgumentNullException(nameof(entry));

            if (entry is TEntity rootEntity)
                UnitOfWork.GetRepository<Repository<TEntity>>()?.Delete(rootEntity);
            else
                UnitOfWork.GetRepository<Repository<TEntity>>()?.Delete(Mapper.Map<TEntity>(entry));

            if (shouldSave) await CommitAsync();

            return Result.FromSuccess();
        }

        public virtual async Task<Result> DeleteAsync(long id, bool shouldSave = false)
        {
            UnitOfWork.GetRepository<Repository<TEntity>>()?.Delete(id);

            if (shouldSave) await CommitAsync();

            return Result.FromSuccess();
        }

        public virtual async Task<Result> DeleteRangeAsync(IEnumerable<long> ids, bool shouldSave = false)
        {
            if (ids  is null) throw new ArgumentNullException(nameof(ids));

            UnitOfWork.GetRepository<Repository<TEntity>>()?.DeleteRange(ids);

            if (shouldSave) await CommitAsync();

            return Result.FromSuccess();
        }

        public virtual async Task<Result> DeleteRangeAsync<TDelete>(IEnumerable<TDelete> entries, bool shouldSave = false)
            where TDelete : class
        {
            if (entries  is null) throw new ArgumentNullException(nameof(entries));

            if (entries is IEnumerable<TEntity> rootEntities)
                UnitOfWork.GetRepository<Repository<TEntity>>()?.DeleteRange(rootEntities);
            else
                UnitOfWork.GetRepository<Repository<TEntity>>()?
                    .DeleteRange(Mapper.Map<IEnumerable<TEntity>>(entries));

            if (shouldSave) await CommitAsync();

            return Result.FromSuccess();
        }

        public virtual async Task<Result> DisableAsync(long id, bool shouldSave = false)
        {
            await UnitOfWork.GetRepository<Repository<TEntity>>()?
                .DisableAsync(id)!;

            if (shouldSave) await CommitAsync();

            return Result.FromSuccess();
        }

        public virtual async Task<Result> DisableAsync<TDisable>(TDisable entry, bool shouldSave = false) where TDisable : class
        {
            if (entry  is null) throw new ArgumentNullException(nameof(entry));

            if (entry is TEntity rootEntity)
                UnitOfWork.GetRepository<Repository<TEntity>>()?.Disable(rootEntity);
            else
                UnitOfWork.GetRepository<Repository<TEntity>>()?.Disable(Mapper.Map<TEntity>(entry));

            if (shouldSave) await CommitAsync();

            return Result.FromSuccess();
        }

        public virtual async Task<Result> DisableRangeAsync(IEnumerable<long> ids, bool shouldSave = false)
        {
            if (ids  is null) throw new ArgumentNullException(nameof(ids));

            await UnitOfWork.GetRepository<Repository<TEntity>>()
                ?.DisableRangeAsync(ids)!;

            if (shouldSave) await CommitAsync();

            return Result.FromSuccess();
        }

        public virtual async Task<Result> DisableRangeAsync<TDisable>(IEnumerable<TDisable> entries, bool shouldSave = false)
            where TDisable : class
        {
            if (entries  is null) throw new ArgumentNullException(nameof(entries));

            if (entries is IEnumerable<TEntity> rootEntities)
                UnitOfWork.GetRepository<Repository<TEntity>>()?.DisableRange(rootEntities);
            else
                UnitOfWork.GetRepository<Repository<TEntity>>()?
                    .DeleteRange(Mapper.Map<IEnumerable<TEntity>>(entries));

            if (shouldSave) await CommitAsync();

            return Result.FromSuccess();
        }
    }
}