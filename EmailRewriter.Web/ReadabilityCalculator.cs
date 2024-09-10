using Microsoft.SemanticKernel;
using System.Text.RegularExpressions;

namespace EmailRewriter.Web;

public static class ReadabilityCalculator
{
    // Method to calculate the Gunning Fog Index
    public static double CalculateGunningFogIndex(string text)
    {
        // Step 1: Calculate the number of words and sentences
        int wordCount = CountWords(text);
        int sentenceCount = CountSentences(text);

        // Step 2: Calculate the number of complex words (words with 3+ syllables)
        int complexWordCount = CountComplexWords(text);

        // Step 3: Apply the Gunning Fog Index formula
        double fogIndex = 0.4 * ((double)wordCount / sentenceCount + 100.0 * (double)complexWordCount / wordCount);

        return fogIndex;
    }

    public static async Task<double> CalculateGunningFogIndexThroughAI(Kernel semanticKernel, string text)
    {
        //var prompt = $"""
        //    Calculate the Gunning Fog Index of the following text:
        //    {text}

        //    Return the result in as a double.
        //    """;

        ////semanticKernel.Crea
        //var result= await semanticKernel.InvokePromptAsync(prompt);
        //return Convert.ToDouble(result);

        var prompt = $$"""
            Calculate the number of words, number of complex words and number of sentences in the following text:
            {{text}}

            Return the result as JSON. Here is an example response:
            {
               words: 100,
               complexWords: 10,
               sentences: 5
            }
            """;

        var result= await semanticKernel.InvokePromptAsync(prompt);
        return 0;
        //double fogIndex = 0.4 * ((double)result.words / result.sentences + 100.0 * (double)result.complexWords / result.words);
        //return fogIndex;
        
        //string promptYaml = File.ReadAllText("CalculateReadabilityPrompt.yml");
        //var function = semanticKernel.CreateFunctionFromPromptYaml(promptYaml);
        
        //KernelArguments arguments = new KernelArguments
        //{
        //    { "text", text }
        //};

        //var result = await function.InvokeAsync<Readability>(semanticKernel, arguments);

        //return result.readability;
    }

    public record Readability(int words, int sentences, int complexWords, double readability);

    // Helper method to count words
    static int CountWords(string text)
    {
        return text.Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    // Helper method to count sentences
    static int CountSentences(string text)
    {
        // Count sentences using punctuation marks as delimiters
        return Regex.Matches(text, @"[.!?]").Count;
    }

    // Helper method to count complex words (words with 3 or more syllables)
    static int CountComplexWords(string text)
    {
        string[] words = text.Split(new char[] { ' ', '\t', '\n', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        int complexWordCount = 0;

        foreach (var word in words)
        {
            if (CountSyllables(word) >= 3)
            {
                complexWordCount++;
            }
        }

        return complexWordCount;
    }

    // Helper method to count syllables in a word
    static int CountSyllables(string word)
    {
        // Regex pattern to match vowel groups (approximates syllables)
        string pattern = @"[aeiouyAEIOUY]+";
        var matches = Regex.Matches(word, pattern);

        int syllableCount = matches.Count;

        // Handle silent 'e' at the end of a word
        if (word.EndsWith("e", StringComparison.OrdinalIgnoreCase) && syllableCount > 1)
        {
            syllableCount--;
        }

        return syllableCount > 0 ? syllableCount : 1; // Ensure at least 1 syllable
    }
}
