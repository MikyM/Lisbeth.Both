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

namespace MikyM.Common.Utilities.Autofac.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class AutofacLifetimeAttribute : Attribute
{
    public Lifetime Scope { get; private set; }
    public Type? Owned { get; private set; }
    public IEnumerable<object> Tags { get; private set; } = new List<string>();

    public AutofacLifetimeAttribute(Lifetime scope)
    {
        Scope = scope;
    }

    public AutofacLifetimeAttribute(Lifetime scope, Type owned)
    {
        Scope = scope;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    public AutofacLifetimeAttribute(Lifetime scope, IEnumerable<object> tags)
    {
        Scope = scope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }

    public AutofacLifetimeAttribute(Type owned)
    {
        Scope = Lifetime.InstancePerOwned;
        Owned = owned ?? throw new ArgumentNullException(nameof(owned));
    }

    public AutofacLifetimeAttribute(IEnumerable<object> tags)
    {
        Scope = Lifetime.InstancePerMatchingLifetimeScope;
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        if (!tags.Any())
            throw new ArgumentException("You must pass at least one tag");
    }
}