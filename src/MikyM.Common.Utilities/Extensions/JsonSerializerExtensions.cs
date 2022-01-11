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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MikyM.Common.Utilities.Extensions;

public static class JsonSerializerExtensions
{
    /// <summary>
    /// Adds a JSON converter to the given json options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <typeparam name="TConverter">The converter type.</typeparam>
    /// <returns>The added converter.</returns>
    public static JsonSerializerOptions AddConverter<TConverter>(this JsonSerializerOptions options)
        where TConverter : JsonConverter, new()
    {
        options.Converters.Insert(0, new TConverter());

        return options;
    }
}
