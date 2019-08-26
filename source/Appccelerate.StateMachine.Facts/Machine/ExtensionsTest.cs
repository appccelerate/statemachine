//-------------------------------------------------------------------------------
// <copyright file="ExceptionCasesTest.cs" company="Appccelerate">
//   Copyright (c) 2008-2017 Appccelerate
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

namespace Appccelerate.StateMachine.Facts.Machine
{
    using FakeItEasy;
    using StateMachine.Machine;
    using Xunit;

    public class ExtensionsTest
    {
        [Fact]
        public void ForEachExecutesActionOnEveryAddedExtension()
        {
            var extensionA = A.Fake<IExtension<string, string>>();
            var extensionB = A.Fake<IExtension<string, string>>();
            var stateMachineInformation = A.Fake<IStateMachineInformation<string, string>>();
            const string InitialState = "init";

            var extensions = new Extensions<string, string>();
            extensions.Add(extensionA);
            extensions.Add(extensionB);

            extensions.ForEach(ext => ext.InitializedStateMachine(stateMachineInformation, InitialState));

            A.CallTo(() => extensionA.InitializedStateMachine(stateMachineInformation, InitialState))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => extensionB.InitializedStateMachine(stateMachineInformation, InitialState))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void ForEachExecutesNoAction_WhenExtensionsWereCleared()
        {
            var extension = A.Fake<IExtension<string, string>>();
            var stateMachineInformation = A.Fake<IStateMachineInformation<string, string>>();
            const string InitialState = "init";

            var extensions = new Extensions<string, string>();
            extensions.Add(extension);

            extensions.Clear();
            extensions.ForEach(ext => ext.InitializedStateMachine(stateMachineInformation, InitialState));

            A.CallTo(() => extension.InitializedStateMachine(A<IStateMachineInformation<string, string>>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }
    }
}