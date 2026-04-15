using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// XMLファイルを読み込み、数値を有効数字5桁に丸めるスクリプト
const string inputFilePath = "DefaultSettings.xml";
const string outputFilePath = "DefaultSettings_rounded.xml";

if (!File.Exists(inputFilePath))
{
    Console.WriteLine($"Error: Input file not found at '{inputFilePath}'");
    return;
}

try
{
    var doc = XDocument.Load(inputFilePath);
    
    var numberRegex = NumberRegex();

    string ProcessValue(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        return numberRegex.Replace(input, match =>
            double.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var num)
                ? num.ToString("G5", CultureInfo.InvariantCulture)
                : match.Value);
    }

    foreach (var element in doc.Descendants())
    {
        if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
        {
            element.Value = ProcessValue(element.Value);
        }
    }

    doc.Save(outputFilePath);
    Console.WriteLine($"Successfully processed '{inputFilePath}' and saved to '{outputFilePath}'");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

internal partial class Program
{
    [GeneratedRegex(@"[-+]?\d*\.?\d+(?:[eE][-+]?\d+)?", RegexOptions.Compiled)]
    private static partial Regex NumberRegex();
}