//-------------------------------------------------------------------------------
// <copyright file="EquivalenceExtensions.cs" company="Appccelerate">
//   Copyright (c) 2008-2019 Appccelerate
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.Specs
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Collections;

    public static class EquivalenceExtensions
    {
        public static void IsEquivalentInOrder<T>(this GenericCollectionAssertions<T> genericCollectionAssertions, IList<T> other)
            where T : class
        {
            var listToAssert = ConvertOrCastToList(genericCollectionAssertions.Subject);

            listToAssert
                .Count
                .Should()
                .Be(other.Count);

            for (var i = 0; i < listToAssert.Count; i++)
            {
                listToAssert[i]
                    .Should()
                    .BeEquivalentTo(other[i]);
            }
        }

        private static IList<T> ConvertOrCastToList<T>(IEnumerable<T> source)
        {
            return source as IList<T> ?? source.ToList();
        }
    }
}
