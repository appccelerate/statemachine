//-------------------------------------------------------------------------------
// <copyright file="Transition.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System;
    using States;
    using LiteGuard = Guard;

    public class TransitionLogic<TState, TEvent>
        : ITransitionLogic<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IExtensionHost<TState, TEvent> extensionHost;
        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;

        private IStateLogic<TState, TEvent> stateLogic;

        public TransitionLogic(
            IExtensionHost<TState, TEvent> extensionHost,
            IStateMachineInformation<TState, TEvent> stateMachineInformation)
        {
            this.extensionHost = extensionHost;
            this.stateMachineInformation = stateMachineInformation;
        }

        public void SetStateLogic(IStateLogic<TState, TEvent> stateLogicToSet)
        {
            this.stateLogic = stateLogicToSet;
        }

        public ITransitionResult<TState> Fire(TransitionNew<TState, TEvent> transitionDefinition, ITransitionContext<TState, TEvent> context)
        {
            LiteGuard.AgainstNullArgument("context", context);

            if (!this.ShouldFire(transitionDefinition, context))
            {
                this.extensionHost.ForEach(extension => extension.SkippedTransition(
                    this.stateMachineInformation,
                    null,
                    context));

                return TransitionResult<TState>.NotFired;
            }

            context.OnTransitionBegin();

            this.extensionHost.ForEach(extension => extension.ExecutingTransition(
                this.stateMachineInformation,
                null,
                context));

            var newState = context.StateDefinition.Id;

            if (!transitionDefinition.IsInternalTransition)
            {
                this.UnwindSubStates(transitionDefinition, context);

                this.Fire(transitionDefinition, transitionDefinition.Source, transitionDefinition.Target, context);

                newState = this.stateLogic.EnterByHistory(transitionDefinition.Target, context);
            }
            else
            {
                this.PerformActions(transitionDefinition, context);
            }

            this.extensionHost.ForEach(extension => extension.ExecutedTransition(
                this.stateMachineInformation,
                null,
                context));

            return new TransitionResult<TState>(true, newState);
        }

        private static void HandleException(Exception exception, ITransitionContext<TState, TEvent> context)
        {
            context.OnExceptionThrown(exception);
        }

        /// <summary>
        /// Recursively traverses the state hierarchy, exiting states along
        /// the way, performing the action, and entering states to the target.
        /// </summary>
        /// <remarks>
        /// There exist the following transition scenarios:
        /// 0. there is no target state (internal transition)
        ///    --> handled outside this method.
        /// 1. The source and target state are the same (self transition)
        ///    --> perform the transition directly:
        ///        Exit source state, perform transition actions and enter target state
        /// 2. The target state is a direct or indirect sub-state of the source state
        ///    --> perform the transition actions, then traverse the hierarchy
        ///        from the source state down to the target state,
        ///        entering each state along the way.
        ///        No state is exited.
        /// 3. The source state is a sub-state of the target state
        ///    --> traverse the hierarchy from the source up to the target,
        ///        exiting each state along the way.
        ///        Then perform transition actions.
        ///        Finally enter the target state.
        /// 4. The source and target state share the same super-state
        /// 5. All other scenarios:
        ///    a. The source and target states reside at the same level in the hierarchy
        ///       but do not share the same direct super-state
        ///    --> exit the source state, move up the hierarchy on both sides and enter the target state
        ///    b. The source state is lower in the hierarchy than the target state
        ///    --> exit the source state and move up the hierarchy on the source state side
        ///    c. The target state is lower in the hierarchy than the source state
        ///    --> move up the hierarchy on the target state side, afterward enter target state.
        /// </remarks>
        /// <param name="source">The source state.</param>
        /// <param name="target">The target state.</param>
        /// <param name="context">The event context.</param>
        private void Fire(
            TransitionNew<TState, TEvent> transitionDefinition,
            IStateDefinition<TState, TEvent> source,
            IStateDefinition<TState, TEvent> target,
            ITransitionContext<TState, TEvent> context)
        {
            if (source == transitionDefinition.Target)
            {
                // Handles 1.
                // Handles 3. after traversing from the source to the target.
                this.stateLogic.Exit(source, context);
                this.PerformActions(transitionDefinition, context);
                this.stateLogic.Entry(transitionDefinition.Target, context);
            }
            else if (source == target)
            {
                // Handles 2. after traversing from the target to the source.
                this.PerformActions(transitionDefinition, context);
            }
            else if (source.SuperState == target.SuperState)
            {
                //// Handles 4.
                //// Handles 5a. after traversing the hierarchy until a common ancestor if found.
                this.stateLogic.Exit(source, context);
                this.PerformActions(transitionDefinition, context);
                this.stateLogic.Entry(target, context);
            }
            else
            {
                // traverses the hierarchy until one of the above scenarios is met.

                // Handles 3.
                // Handles 5b.
                if (source.Level > target.Level)
                {
                    this.stateLogic.Exit(source, context);
                    this.Fire(transitionDefinition, source.SuperState, target, context);
                }
                else if (source.Level < target.Level)
                {
                    // Handles 2.
                    // Handles 5c.
                    this.Fire(transitionDefinition, source, target.SuperState, context);
                    this.stateLogic.Entry(target, context);
                }
                else
                {
                    // Handles 5a.
                    this.stateLogic.Exit(source, context);
                    this.Fire(transitionDefinition, source.SuperState, target.SuperState, context);
                    this.stateLogic.Entry(target, context);
                }
            }
        }

        private bool ShouldFire(
            TransitionNew<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            try
            {
                return transitionDefinition.Guard == null || transitionDefinition.Guard.Execute(context.EventArgument);
            }
            catch (Exception exception)
            {
                this.extensionHost.ForEach(extension =>
                    extension.HandlingGuardException(this.stateMachineInformation, null, context, ref exception));

                HandleException(exception, context);

                this.extensionHost.ForEach(extension => 
                    extension.HandledGuardException(this.stateMachineInformation, null, context, exception));

                return false;
            }
        }

        private void PerformActions(TransitionNew<TState, TEvent> transitionDefinition, ITransitionContext<TState, TEvent> context)
        {
            foreach (var action in transitionDefinition.Actions)
            {
                try
                {
                    action.Execute(context.EventArgument);
                }
                catch (Exception exception)
                {
                    this.extensionHost.ForEach(extension => 
                        extension.HandlingTransitionException(this.stateMachineInformation, null, context, ref exception));

                    HandleException(exception, context);

                    this.extensionHost.ForEach(extension => 
                        extension.HandledTransitionException(this.stateMachineInformation, null, context, exception));
                }
            }
        }

        private void UnwindSubStates(
            TransitionNew<TState, TEvent> transitionDefinition,
            ITransitionContext<TState, TEvent> context)
        {
            var o = context.StateDefinition;
            while (o != transitionDefinition.Source)
            {
                this.stateLogic.Exit(o, context);
                o = o.SuperState;
            }
        }
    }
}