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
using MikyM.Discord.EmbedBuilders.Enrichers;

namespace MikyM.Discord.EmbedBuilders.Builders;

public abstract class EnrichedEmbedBuilder<TBuilder> : IEnrichedEmbedBuilder<TBuilder> where TBuilder : IBaseEmbedBuilder
{
    public DiscordEmbedBuilder Base { get; protected set; }

    public IEnhancedDiscordEmbedBuilder PreviousBuilder { get; }

    protected EnrichedEmbedBuilder(IEnhancedDiscordEmbedBuilder previousEmbedBuilder)
    {
        this.Base = previousEmbedBuilder.BaseBuild();
        this.PreviousBuilder = previousEmbedBuilder;
    }

    public virtual DiscordEmbed Build()
    {
        this.BaseBuild();
        return this.Base.Build();
    }

    public virtual DiscordEmbedBuilder BaseBuild()
    {
        return this.Base;
    }

    public abstract IEnrichedEmbedBuilder<TBuilder> EnrichFrom<TEnricher>(TEnricher enricher)
        where TEnricher : IEmbedEnricher;
}
