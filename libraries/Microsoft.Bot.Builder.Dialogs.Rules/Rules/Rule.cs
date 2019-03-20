﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Expressions;
using Microsoft.Bot.Builder.Dialogs.Expressions;

namespace Microsoft.Bot.Builder.Dialogs.Rules.Rules
{
    /// <summary>
    /// Defines basic Rule contract
    /// </summary>
    public abstract class Rule : IRule
    {
        private IExpression expression;

        public Rule(string constraint = null, List<IDialog> steps = null, PlanChangeTypes changeType = PlanChangeTypes.DoSteps)
        {
            this.Constraint = constraint;
            this.Steps = steps;
            this.ChangeType = changeType;
        }

        /// <summary>
        /// Gets or sets the constraint to apply to the rule (OPTIONAL) 
        /// </summary>
        public string Constraint { get; set; }

        /// <summary>
        /// Gets or sets the steps to add to the plan when the rule constraints are met
        /// </summary>
        public List<IDialog> Steps { get; set; } = new List<IDialog>();

        /// <summary>
        /// Gets or sets the Policy for the changing the plan
        /// </summary>
        public PlanChangeTypes ChangeType { get; set; }

        /// <summary>
        /// Get the expression for this rule by calling GatherConstraints()
        /// </summary>
        public virtual IExpression GetExpressionEval(PlanningContext planningContext, DialogEvent dialogEvent)
        {
            var expressionFactory = planningContext.Context.TurnState.Get<IExpressionFactory>();

            if (expressionFactory != null && this.expression == null)
            {
                List<String> expressions = new List<string>();

                // get constraints from children
                GatherConstraints(expressions);

                if (expressions.Any())
                {
                    this.expression = expressionFactory.CreateExpression(String.Join(" && ", expressions));
                }
            }

            return new FunctionExpression(async (vars) =>
            {
                // TODO figure out cleaner way to do this, we need access to the dialogevent in the variable space
                planningContext.State.Dialog["DialogEvent"] = dialogEvent;

                if (this.expression != null)
                {
                    return await this.expression.Evaluate(vars);
                }
                return true;
            });
        }

        /// <summary>
        /// Method called to execute the rule's steps
        /// </summary>
        /// <param name="planningContext"></param>
        /// <param name="dialogEvent"></param>
        /// <returns></returns>
        public async Task<List<PlanChangeList>> ExecuteAsync(PlanningContext planningContext)
        {
            return await OnExecuteAsync(planningContext).ConfigureAwait(false);
        }


        /// <summary>
        /// Method called to process the request to execute the steps
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dialogEvent"></param>
        /// <returns></returns>
        public async virtual Task<List<PlanChangeList>> OnExecuteAsync(PlanningContext planning)
        {
            return new List<PlanChangeList>()
            {
                this.OnCreateChangeList(planning)
            };
        }

        protected virtual PlanChangeList OnCreateChangeList(PlanningContext planning, object dialogOptions = null)
        {
            var changeList = new PlanChangeList()
            {
                ChangeType = this.ChangeType,
                Steps = new List<PlanStepState>()
            };

            Steps.ForEach(s =>
            {
                var stepState = new PlanStepState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id
                };

                if (dialogOptions != null)
                {
                    stepState.Options = dialogOptions;
                }

                changeList.Steps.Add(stepState);
            });

            return changeList;
        }

        protected virtual void GatherConstraints(List<string> constraints)
        {
            if (!String.IsNullOrEmpty(this.Constraint))
            {
                constraints.Add(this.Constraint);
            }
        }
    }
}
