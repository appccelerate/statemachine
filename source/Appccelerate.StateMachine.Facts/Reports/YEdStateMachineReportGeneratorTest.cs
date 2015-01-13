//-------------------------------------------------------------------------------
// <copyright file="YEdStateMachineReportGeneratorTest.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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

namespace Appccelerate.StateMachine.Reports
{
    using System.IO;

    using FluentAssertions;

    using Xunit;

    public class YEdStateMachineReportGeneratorTest
    {
        /// <summary>
        /// Some test states for simulating an elevator.
        /// </summary>
        private enum States
        {
            /// <summary>Elevator has an Error</summary>
            Error,

            /// <summary>Elevator is healthy, i.e. no error</summary>
            Healthy,

            /// <summary>The elevator is moving (either up or down)</summary>
            Moving,

            /// <summary>The elevator is moving down.</summary>
            MovingUp,

            /// <summary>The elevator is moving down.</summary>
            MovingDown,

            /// <summary>The elevator is standing on a floor.</summary>
            OnFloor,

            /// <summary>The door is closed while standing still.</summary>
            DoorClosed,

            /// <summary>The door is open while standing still.</summary>
            DoorOpen
        }

        /// <summary>
        /// Some test events for the elevator
        /// </summary>
        private enum Events
        {
            /// <summary>An error occurred.</summary>
            ErrorOccured,

            /// <summary>Reset after error.</summary>
            Reset,

            /// <summary>Open the door.</summary>
            OpenDoor,

            /// <summary>Close the door.</summary>
            CloseDoor,

            /// <summary>Move elevator up.</summary>
            GoUp,

            /// <summary>Move elevator down.</summary>
            GoDown,

            /// <summary>Stop the elevator.</summary>
            Stop
        }

        [Fact]
        public void YEdGraphMl()
        {
            var elevator = new PassiveStateMachine<States, Events>("Elevator");

            elevator.DefineHierarchyOn(States.Healthy)
                .WithHistoryType(HistoryType.Deep)
                .WithInitialSubState(States.OnFloor)
                .WithSubState(States.Moving);

            elevator.DefineHierarchyOn(States.Moving)
                .WithHistoryType(HistoryType.Shallow)
                .WithInitialSubState(States.MovingUp)
                .WithSubState(States.MovingDown);

            elevator.DefineHierarchyOn(States.OnFloor)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(States.DoorClosed)
                .WithSubState(States.DoorOpen);

            elevator.In(States.Healthy)
                .On(Events.ErrorOccured).Goto(States.Error);

            elevator.In(States.Error)
                .On(Events.Reset).Goto(States.Healthy)
                .On(Events.ErrorOccured);

            elevator.In(States.OnFloor)
                .ExecuteOnEntry(AnnounceFloor)
                .ExecuteOnExit(Beep)
                .ExecuteOnExit(Beep)
                .On(Events.CloseDoor).Goto(States.DoorClosed)
                .On(Events.OpenDoor).Goto(States.DoorOpen)
                .On(Events.GoUp)
                    .If(CheckOverload).Goto(States.MovingUp)
                    .Otherwise()
                        .Execute(AnnounceOverload)
                        .Execute(Beep)
                .On(Events.GoDown)
                    .If(CheckOverload).Goto(States.MovingDown)
                    .Otherwise().Execute(AnnounceOverload);

            elevator.In(States.Moving)
                .On(Events.Stop).Goto(States.OnFloor);

            elevator.Initialize(States.OnFloor);

            var stream = new MemoryStream();
            var textWriter = new StreamWriter(stream);
            var testee = new YEdStateMachineReportGenerator<States, Events>(textWriter);

            elevator.Report(testee);

            stream.Position = 0;
            var reader = new StreamReader(stream);
            var report = reader.ReadToEnd();

            const string ExpectedReport = "<?xml version=\"1.0\" encoding=\"utf-8\"?><graphml xmlns:y=\"http://www.yworks.com/xml/graphml\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:yed=\"http://www.yworks.com/xml/yed/3\" xmlns:schemaLocation=\"http://graphml.graphdrawing.org/xmlns http://www.yworks.com/xml/schema/graphml/1.1/ygraphml.xsd\" xmlns=\"http://graphml.graphdrawing.org/xmlns\">  <!--Created by Appccelerate.StateMachine.YEdStateMachineReportGenerator-->  <key for=\"graphml\" id=\"d0\" yfiles.type=\"resources\" />  <key for=\"port\" id=\"d1\" yfiles.type=\"portgraphics\" />  <key for=\"port\" id=\"d2\" yfiles.type=\"portgeometry\" />  <key for=\"port\" id=\"d3\" yfiles.type=\"portuserdata\" />  <key attr.name=\"url\" attr.type=\"string\" for=\"node\" id=\"d4\" />  <key attr.name=\"description\" attr.type=\"string\" for=\"node\" id=\"d5\" />  <key for=\"node\" id=\"d6\" yfiles.type=\"nodegraphics\" />  <key attr.name=\"Beschreibung\" attr.type=\"string\" for=\"graph\" id=\"d7\">    <default />  </key>  <key attr.name=\"url\" attr.type=\"string\" for=\"edge\" id=\"d8\" />  <key attr.name=\"description\" attr.type=\"string\" for=\"edge\" id=\"d9\" />  <key for=\"edge\" id=\"d10\" yfiles.type=\"edgegraphics\" />  <graph edgedefault=\"directed\" id=\"G\">    <node id=\"Healthy\">      <data key=\"d6\">        <y:ProxyAutoBoundsNode>          <y:Realizers active=\"0\">            <y:GroupNode>              <y:NodeLabel alignment=\"right\" autoSizePolicy=\"node_width\" backgroundColor=\"#EBEBEB\" modelName=\"internal\" modelPosition=\"t\">Healthy</y:NodeLabel>              <y:State closed=\"false\" innerGraphDisplayEnabled=\"true\" />            </y:GroupNode>          </y:Realizers>        </y:ProxyAutoBoundsNode>      </data>      <graph edgedefault=\"directed\" id=\"Healthy:\">        <node id=\"OnFloor\">          <data key=\"d6\">            <y:ProxyAutoBoundsNode>              <y:Realizers active=\"0\">                <y:GroupNode>                  <y:NodeLabel alignment=\"right\" autoSizePolicy=\"node_width\" backgroundColor=\"#EBEBEB\" modelName=\"internal\" modelPosition=\"t\">(AnnounceFloor)OnFloor(Beep, Beep)</y:NodeLabel>                  <y:State closed=\"false\" innerGraphDisplayEnabled=\"true\" />                  <y:BorderStyle width=\"2.0\" />                </y:GroupNode>              </y:Realizers>            </y:ProxyAutoBoundsNode>          </data>          <graph edgedefault=\"directed\" id=\"OnFloor:\">            <node id=\"DoorClosed\">              <data key=\"d6\">                <y:ShapeNode>                  <y:NodeLabel>DoorClosed</y:NodeLabel>                  <y:Shape type=\"ellipse\" />                  <y:BorderStyle width=\"2.0\" />                </y:ShapeNode>              </data>            </node>            <node id=\"DoorOpen\">              <data key=\"d6\">                <y:ShapeNode>                  <y:NodeLabel>DoorOpen</y:NodeLabel>                  <y:Shape type=\"ellipse\" />                </y:ShapeNode>              </data>            </node>          </graph>        </node>        <node id=\"Moving\">          <data key=\"d6\">            <y:ProxyAutoBoundsNode>              <y:Realizers active=\"0\">                <y:GroupNode>                  <y:NodeLabel alignment=\"right\" autoSizePolicy=\"node_width\" backgroundColor=\"#EBEBEB\" modelName=\"internal\" modelPosition=\"t\">Moving</y:NodeLabel>                  <y:State closed=\"false\" innerGraphDisplayEnabled=\"true\" />                </y:GroupNode>              </y:Realizers>            </y:ProxyAutoBoundsNode>          </data>          <graph edgedefault=\"directed\" id=\"Moving:\">            <node id=\"MovingUp\">              <data key=\"d6\">                <y:ShapeNode>                  <y:NodeLabel>MovingUp</y:NodeLabel>                  <y:Shape type=\"ellipse\" />                  <y:BorderStyle width=\"2.0\" />                </y:ShapeNode>              </data>            </node>            <node id=\"MovingDown\">              <data key=\"d6\">                <y:ShapeNode>                  <y:NodeLabel>MovingDown</y:NodeLabel>                  <y:Shape type=\"ellipse\" />                </y:ShapeNode>              </data>            </node>          </graph>        </node>      </graph>    </node>    <node id=\"Error\">      <data key=\"d6\">        <y:ShapeNode>          <y:NodeLabel>Error</y:NodeLabel>          <y:Shape type=\"ellipse\" />        </y:ShapeNode>      </data>    </node>    <edge id=\"ErrorOccured0\" source=\"Healthy\" target=\"Error\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"line\" />          <y:Arrows source=\"none\" target=\"standard\" />          <y:EdgeLabel>ErrorOccured</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"CloseDoor1\" source=\"OnFloor\" target=\"DoorClosed\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"line\" />          <y:Arrows source=\"none\" target=\"standard\" />          <y:EdgeLabel>CloseDoor</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"OpenDoor2\" source=\"OnFloor\" target=\"DoorOpen\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"line\" />          <y:Arrows source=\"none\" target=\"standard\" />          <y:EdgeLabel>OpenDoor</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"GoUp3\" source=\"OnFloor\" target=\"MovingUp\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"line\" />          <y:Arrows source=\"none\" target=\"standard\" />          <y:EdgeLabel>[CheckOverload]GoUp</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"GoUp4\" source=\"OnFloor\" target=\"OnFloor\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"dashed\" />          <y:Arrows source=\"none\" target=\"plain\" />          <y:EdgeLabel>GoUp(AnnounceOverload, Beep)</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"GoDown5\" source=\"OnFloor\" target=\"MovingDown\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"line\" />          <y:Arrows source=\"none\" target=\"standard\" />          <y:EdgeLabel>[CheckOverload]GoDown</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"GoDown6\" source=\"OnFloor\" target=\"OnFloor\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"dashed\" />          <y:Arrows source=\"none\" target=\"plain\" />          <y:EdgeLabel>GoDown(AnnounceOverload)</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"Stop7\" source=\"Moving\" target=\"OnFloor\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"line\" />          <y:Arrows source=\"none\" target=\"standard\" />          <y:EdgeLabel>Stop</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"Reset8\" source=\"Error\" target=\"Healthy\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"line\" />          <y:Arrows source=\"none\" target=\"standard\" />          <y:EdgeLabel>Reset</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>    <edge id=\"ErrorOccured9\" source=\"Error\" target=\"Error\">      <data key=\"d10\">        <y:PolyLineEdge>          <y:LineStyle type=\"dashed\" />          <y:Arrows source=\"none\" target=\"plain\" />          <y:EdgeLabel>ErrorOccured</y:EdgeLabel>        </y:PolyLineEdge>      </data>    </edge>  </graph>  <data key=\"d0\">    <y:Resources />  </data></graphml>";

            var cleanedReport = report.Replace("\n", string.Empty).Replace("\r", string.Empty);

            cleanedReport.Should().Be(ExpectedReport.Replace("\n", string.Empty).Replace("\r", string.Empty));
        }

        private static void Beep()
        {
        }

        private static bool CheckOverload()
        {
            return true;
        }

        private static void AnnounceFloor()
        {
        }

        private static void AnnounceOverload()
        {
        }
    }
}