// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodNameExtractor.cs" company="Appccelerate">
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
// --------------------------------------------------------------------------------------------------------------------

namespace Appccelerate.StateMachine
{
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class MethodNameExtractor
    {
        public static string ExtractMethodNameOrAnonymous(MethodInfo methodInfo)
        {
            return
                IsLambda(methodInfo)
                    ? "anonymous"
                    : methodInfo.Name;
        }

        private static bool IsLambda(MethodInfo methodInfo)
        {
            return methodInfo
                .DeclaringType
                .GetTypeInfo()
                .GetCustomAttributes(typeof(CompilerGeneratedAttribute), false)
                .Any();
        }
    }
}
