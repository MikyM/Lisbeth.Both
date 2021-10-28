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

using System.Collections.Generic;
using System.Linq;

namespace MikyM.Common.DataAccessLayer.Specifications.Evaluators
{
    public class GroupByEvaluator : IEvaluator, IInMemoryEvaluator
    {
        private GroupByEvaluator()
        {
        }

        public static GroupByEvaluator Instance { get; } = new();

        public bool IsCriteriaEvaluator { get; } = false;

        public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification) where T : class
        {
            return specification.GroupByExpression is null
                ? query
                : query.GroupBy(specification.GroupByExpression).SelectMany(x => x);
        }

        public IEnumerable<T> Evaluate<T>(IEnumerable<T> query, ISpecification<T> specification) where T : class
        {
            return specification.GroupByExpression is null
                ? query
                : query.GroupBy(specification.GroupByExpression.Compile()).SelectMany(x => x);
        }
    }
}