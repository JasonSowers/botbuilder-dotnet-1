// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// Represents recognition result for the prompt.
    /// </summary>
    public class ChoiceResult : PromptResult
    {
        /// <summary>
        /// Creates a <see cref="ChoiceResult"/> object.
        /// </summary>
        public ChoiceResult() { }

        /// <summary>
        /// The value recognized; or <c>null</c>, if recognition fails.
        /// </summary>
        public FoundChoice Value { get; set; }
    }

    public class ChoicePrompt : BasePrompt<ChoiceResult>
    {
        public ChoicePrompt()
        {
        }

        public async Task Prompt(ITurnContext context, List<Choice> choices, string prompt, string speak)
        {
            //
            //TODO: call ChoiceFactory
            //
            IMessageActivity activity = Activity.CreateMessageActivity();
            activity.Text = !string.IsNullOrWhiteSpace(prompt) ? prompt : null;
            activity.Speak = !string.IsNullOrWhiteSpace(speak) ? speak : null;
            activity.InputHint = InputHints.ExpectingInput;
            //
            //
            await context.SendActivity(activity);
        }

        public override Task<ChoiceResult> Recognize(ITurnContext context)
        {
            BotAssert.ContextNotNull(context);
            BotAssert.ActivityNotNull(context.Activity);
            if (context.Activity.Type != ActivityTypes.Message)
                throw new InvalidOperationException("No Message to Recognize");
            //
            //TODO: call RecognizeChoices
            //
            throw new NotImplementedException();
        }
    }
}
