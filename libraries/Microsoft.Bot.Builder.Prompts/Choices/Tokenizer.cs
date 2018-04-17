// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class Token
    {
        int Start { get; set; }
        int End { get; set; }
        string Text { get; set; }
        string Normalized { get; set; }
    }

    public delegate Token[] TokenizerFunction(string text, string locale);

    public class Tokenizer
    {
    }
}
