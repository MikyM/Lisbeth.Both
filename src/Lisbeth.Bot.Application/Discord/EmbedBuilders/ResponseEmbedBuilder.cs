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

using MikyM.Discord.EmbedBuilders.Builders;
using MikyM.Discord.EmbedBuilders.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedBuilders;

public class ResponseEmbedBuilder : EnrichedEmbedBuilder, IResponseEmbedBuilder
{
    public virtual DiscordResponse? Response { get; private set; }

    protected internal ResponseEmbedBuilder(EnhancedDiscordEmbedBuilder enhancedEmbedBuilder,
        DiscordResponse? response = null) : base(enhancedEmbedBuilder) =>
        this.Response = response;

    public IResponseEmbedBuilder WithType(DiscordResponse response)
    {
        this.Response = response;
        return this;
    }

    protected override void Evaluate()
    {
        if (this.Response is not null or 0) // if not default
            base.WithAction(this.Response.Value);
        base.WithActionType(DiscordEmbedEnhancement.Response);

        this.EnhancedBuilder.Evaluate();
    }

    public override ResponseEmbedBuilder EnrichFrom<TEnricher>(TEnricher enricher)
    {
        enricher.Enrich(this);
        return this;
    }
}
