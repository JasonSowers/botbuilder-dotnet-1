// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class ChoiceFactory
    {
        public static IMessageActivity ForChannel(TurnContext context, Choice[] choices, string text, string speak, ChoiceFactoryOptions options)
        {
            return ForChannel(Channel.GetChannelId(context), choices, text, speak, options);
        }
        public static IMessageActivity ForChannel(TurnContext context, string[] choices, string text, string speak, ChoiceFactoryOptions options)
        {
            return ForChannel(Channel.GetChannelId(context), ToChoices(choices), text, speak, options);
        }
        public static IMessageActivity ForChannel(string channelId, string[] choices, string text, string speak, ChoiceFactoryOptions options)
        {
            return ForChannel(channelId, ToChoices(choices), text, speak, options);
        }
        public static IMessageActivity ForChannel(string channelId, Choice[] list, string text, string speak, ChoiceFactoryOptions options)
        {
            // Find maximum title length
            var maxTitleLength = 0;
            foreach (var choice in list)
            {
                var l = choice.Action != null && string.IsNullOrEmpty(choice.Action.Title) ? choice.Action.Title.Length : choice.Value.Length;
                if (l > maxTitleLength)
                {
                    maxTitleLength = l;
                }
            };

            // Determine list style
            var supportsSuggestedActions = Channel.SupportsSuggestedActions(channelId, list.Length);
            var supportsCardActions = Channel.SupportsCardActions(channelId, list.Length);
            var maxActionTitleLength = Channel.MaxActionTitleLength(channelId);
            var hasMessageFeed = Channel.HasMessageFeed(channelId);
            var longTitles = maxTitleLength > maxActionTitleLength;

            if (!longTitles && (supportsSuggestedActions || (!hasMessageFeed && supportsCardActions)))
            {
                // We always prefer showing choices using suggested actions. If the titles are too long, however,
                // we'll have to show them as a text list.
                return SuggestedAction(list, text, speak, options);
            }
            else if (!longTitles && list.Length <= 3)
            {
                // If the titles are short and there are 3 or less choices we'll use an inline list.
                return Inline(list, text, speak, options);
            }
            else
            {
                // Show a numbered list.
                return List(list, text, speak, options);
            }
        }

        public static Activity Inline(Choice[] choices, string text, string speak, ChoiceFactoryOptions options)
        {
            var opt = new ChoiceFactoryOptions
            {
                InlineSeparator = options.InlineSeparator ?? ", ",
                InlineOr = options.InlineOr ?? " or ",
                InlineOrMore = options.InlineOrMore ?? ", or ",
                IncludeNumbers = options.IncludeNumbers ?? true
            };

            // Format list of choices
            var connector = string.Empty;
            var txt = text ?? string.Empty;
            txt += " ";

            for (var index = 0; index < choices.Length; index++)
            {
                var choice = choices[index];

                var title = choice.Action != null && choice.Action.Title != null ? choice.Action.Title : choice.Value;

                txt += $"{connector}";
                if (opt.IncludeNumbers.Value)
                {
                    txt += "(" + (index + 1).ToString() + ") ";
                }
                txt += $"{title}";
                if (index == (choices.Length - 2))
                {
                    connector = (index == 0 ? opt.InlineOr : opt.InlineOrMore) ?? string.Empty;
                }
                else
                {
                    connector = opt.InlineSeparator ?? string.Empty;
                }
            }
            txt += "";

            // Return activity with choices as an inline list.
            return MessageFactory.Text(txt, speak, InputHints.ExpectingInput);
        }

        public static Activity List(Choice[] choices, string text, string speak, ChoiceFactoryOptions options)
        {
            bool includeNumbers = options.IncludeNumbers ?? true;

            // Format list of choices
            var connector = string.Empty;
            var txt = (text ?? string.Empty);
            txt += "\n\n  ";

            for (var index = 0; index < choices.Length; index++)
            {
                var choice = choices[index];

                var title = choice.Action != null && choice.Action.Title != null ? choice.Action.Title : choice.Value;

                txt += connector;
                if (includeNumbers)
                {
                    txt += (index + 1).ToString() + ". ";
                }
                else
                {
                    txt += "- ";
                }
                txt += title;
                connector =  "\n   ";
            }

            // Return activity with choices as a numbered list.
            return MessageFactory.Text(txt, speak, InputHints.ExpectingInput);
        }

        public static IMessageActivity SuggestedAction(Choice[] choices, string text, string speak, ChoiceFactoryOptions options)
        {
            // Map choices to actions
            var actions = choices.Select((choice) => {
                if (choice.Action != null)
                {
                    return choice.Action;
                }
                else
                {
                    return new CardAction
                    {
                        Type = ActionTypes.ImBack,
                        Value = choice.Value,
                        Title = choice.Value
                    };
                }
            }).ToList();

            // Return activity with choices as suggested actions
            return MessageFactory.SuggestedActions(actions, text, speak, InputHints.ExpectingInput);
        }

        public static Choice[] ToChoices(string[] choices)
        {
            if (choices == null)
            {
                return new Choice[0];
            }
            return choices.Select(choice => new Choice { Value = choice }).ToArray();
        }
    }
}
