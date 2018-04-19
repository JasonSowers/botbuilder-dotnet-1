// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public static class Find
    {
        public static List<ModelResult<FoundChoice>> FindChoices(string utterance, List<string> choices, FindChoicesOptions options)
        {
            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            return FindChoices(utterance, choices.Select(s => new Choice { Value = s }).ToList(), options);
        }

        public static List<ModelResult<FoundChoice>> FindChoices(string utterance, List<Choice> choices, FindChoicesOptions options)
        {
            if (string.IsNullOrEmpty(utterance))
                throw new ArgumentNullException(nameof(utterance));
            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            var opt = options ?? new FindChoicesOptions();

            // Build up full list of synonyms to search over.
            // - Each entry in the list contains the index of the choice it belongs to which will later be
            //   used to map the search results back to their choice.
            var synonyms = new List<SortedValue>();

            for (int index=0; index<choices.Count; index++)
            {
                var choice = choices[index];

                if (!opt.NoValue)
                {
                    synonyms.Append(new SortedValue { Value = choice.Value, Index = index });
                }
                if (choice.Action != null && choice.Action.Title != null && !opt.NoAction)
                {
                    synonyms.Append(new SortedValue { Value = choice.Action.Title, Index = index });
                }

                if (choice.Synonyms != null)
                {
                    foreach (var synonym in choice.Synonyms)
                    {
                        synonyms.Append(new SortedValue { Value = synonym, Index = index });
                    }
                }
            }

            return new List<ModelResult<FoundChoice>>();

            /*
            // Find synonyms in utterance and map back to their choices
            return FindValues(utterance, synonyms, options).Select((v) =>
             {
                 var choice = choices[v.Resolution.Index];
                 return new ModelResult<FoundChoice>
                 {
                     Start = v.Start,
                     End = v.End,
                     TypeName = "choice",
                     Text = v.Text,
                     Resolution = new FoundChoice
                     {
                         Value = choice.Value,
                         Index = v.Resolution.Index,
                         Score = v.Resolution.Score,
                         Synonym = v.Resolution.Value
                     }
                 };
             }).ToList();
             */
        }

        /*
        public static List<ModelResult<FoundValue>> FindValues(string utterance, List<SortedValue> values, FindValuesOptions options)
        {
            // Sort values in descending order by length so that the longest value is searched over first.
            var list = values;
            list.Sort((a, b) => b.Value.Length - a.Value.Length);

            // Search for each value within the utterance.
            var matches = new List<ModelResult<FoundValue>>();
            var opt = options ?? new FindValuesOptions();
            var tokenizer = (opt.Tokenizer || DefaultTokenizer);
            var tokens = tokenizer(utterance, opt.Locale);
            var maxDistance = opt.MaxTokenDistance ?? 2;

            for (var index=0; index<list.Count; index++)
            {
                var entry = list[index];
                // Find all matches for a value
                // - To match "last one" in "the last time I chose the last one" we need 
                //   to re-search the string starting from the end of the previous match.
                // - The start & end position returned for the match are token positions.
                let startPos = 0;
                const vTokens = tokenizer(entry.value.trim(), opt.locale);
                while (startPos < tokens.length)
                {
                    const match = matchValue(entry.index, entry.value, vTokens, startPos);
                    if (match)
                    {
                        startPos = match.end + 1;
                        matches.push(match);
                    }
                    else
                    {
                        break;
                    }
                }
            });

            // Sort matches by score descending
            matches = matches.sort((a, b) => b.resolution.score - a.resolution.score);

            // Filter out duplicate matching indexes and overlapping characters.
            // - The start & end positions are token positions and need to be translated to 
            //   character positions before returning. We also need to populate the "text"
            //   field as well. 
            const results: ModelResult<FoundValue>[] = [];
            const foundIndexes: { [index: number]: boolean
        } = {};
    const usedTokens: { [index: number]: boolean } = {};
        matches.forEach((match) => {
            // Apply filters
            let add = !foundIndexes.hasOwnProperty(match.resolution.index);
            for (let i = match.start; i <= match.end; i++) {
                if (usedTokens[i]) {
                    add = false;
                    break;
                }
            }

            // Add to results
            if (add) {
                // Update filter info
                foundIndexes[match.resolution.index] = true;
                for (let i = match.start; i <= match.end; i++) { usedTokens[i] = true }

                // Translate start & end and populate text field
                match.start = tokens[match.start].start;
                match.end = tokens[match.end].end;
                match.text = utterance.substring(match.start, match.end + 1);
                results.push(match);
            }
        });

            // Return the results sorted by position in the utterance
            return results.sort((a, b) => a.start - b.start);
        }
        */

        private static int IndexOfToken(List<Token> tokens, Token token, int startPos)
        {
            for (var i = startPos; i < tokens.Count; i++)
            {
                if (tokens[i].Normalized == token.Normalized) {
                    return i;
                }
            }
            return -1;
        }

        private static ModelResult<FoundValue> MatchValue(List<Token> tokens, int maxDistance, FindValuesOptions options, int index, string value, List<Token> vTokens, int startPos)
        {
            // Match value to utterance and calculate total deviation.
            // - The tokens are matched in order so "second last" will match in 
            //   "the second from last one" but not in "the last from the second one".
            // - The total deviation is a count of the number of tokens skipped in the 
            //   match so for the example above the number of tokens matched would be
            //   2 and the total deviation would be 1. 
            var matched = 0;
            var totalDeviation = 0;
            var start = -1;
            var end = -1;
            foreach (var token in vTokens)
            {
                // Find the position of the token in the utterance.
                var pos = IndexOfToken(tokens, token, startPos);
                if (pos >= 0)
                {
                    // Calculate the distance between the current tokens position and the previous tokens distance.
                    var distance = matched > 0 ? pos - startPos : 0;
                    if (distance <= maxDistance)
                    {
                        // Update count of tokens matched and move start pointer to search for next token after
                        // the current token.
                        matched++;
                        totalDeviation += distance;
                        startPos = pos + 1;

                        // Update start & end position that will track the span of the utterance that's matched.
                        if (start< 0)
                        {
                            start = pos;
                        }
                        end = pos;
                    }
                }
            }

            // Calculate score and format result
            // - The start & end positions and the results text field will be corrected by the caller.
            ModelResult<FoundValue> result = null;

            if (matched > 0 && (matched == vTokens.Count || options.AllowPartialMatches))
            {
                // Percentage of tokens matched. If matching "second last" in 
                // "the second from last one" the completeness would be 1.0 since
                // all tokens were found.
                var completeness = matched / vTokens.Count;

                // Accuracy of the match. The accuracy is reduced by additional tokens
                // occurring in the value that weren't in the utterance. So an utterance
                // of "second last" matched against a value of "second from last" would
                // result in an accuracy of 0.5. 
                var accuracy = (matched / (matched + totalDeviation));

                // The final score is simply the completeness multiplied by the accuracy.
                var score = completeness * accuracy;

                // Format result
                result = new ModelResult<FoundValue>
                {
                    Start = start,
                    End = end,
                    TypeName = "value",
                    Resolution = new FoundValue
                    {
                        Value = value,
                        Index = index,
                        Score = score
                    } 
                };
            }
            return result;
        }


    }
}
