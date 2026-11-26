namespace LetterGame;

internal static class MathNumberWords
{
    private static readonly string[] Units = ["", "jeden", "dwa", "trzy", "cztery", "piêæ", "szeœæ", "siedem", "osiem", "dziewiêæ"];
    private static readonly string[] Teens = ["dziesiêæ", "jedenaœcie", "dwanaœcie", "trzynaœcie", "czternaœcie", "piêtnaœcie", "szesnaœcie", "siedemnaœcie", "osiemnaœcie", "dziewiêtnaœcie"];
    private static readonly string[] Tens = ["", "", "dwadzieœcia", "trzydzieœci", "czterdzieœci", "piêædziesi¹t", "szeœædziesi¹t", "siedemdziesi¹t", "osiemdziesi¹t", "dziewiêædziesi¹t"];
    private static readonly string[] Hundreds = ["", "sto", "dwieœcie", "trzysta", "czterysta", "piêæset", "szeœæset", "siedemset", "osiemset", "dziewiêæset"];

    public static string ToWords(int number)
    {
        if (number is < 0 or > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(number));
        }

        if (number == 0)
        {
            return "zero";
        }

        if (number == 1000)
        {
            return "tysi¹c";
        }

        var parts = new List<string>(3);
        var remainder = number;
        var hundreds = remainder / 100;
        if (hundreds > 0)
        {
            parts.Add(Hundreds[hundreds]);
            remainder %= 100;
        }

        if (remainder is >= 10 and < 20)
        {
            parts.Add(Teens[remainder - 10]);
        }
        else
        {
            var tens = remainder / 10;
            if (tens > 0)
            {
                parts.Add(Tens[tens]);
            }

            var units = remainder % 10;
            if (units > 0)
            {
                parts.Add(Units[units]);
            }
        }

        return string.Join(' ', parts);
    }
}
