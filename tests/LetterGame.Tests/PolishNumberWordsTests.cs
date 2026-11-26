using LetterGame.Domain.Services;

namespace LetterGame.Tests;

public sealed class PolishNumberWordsTests
{
    [Theory]
    [InlineData(0, "zero")]
    [InlineData(5, "piêæ")]
    [InlineData(11, "jedenaœcie")]
    [InlineData(21, "dwadzieœcia jeden")]
    [InlineData(100, "sto")]
    [InlineData(101, "sto jeden")]
    [InlineData(999, "dziewiêæset dziewiêædziesi¹t dziewiêæ")]
    [InlineData(1000, "tysi¹c")]
    public void Converts_supported_numbers_to_polish_words(int number, string expected)
    {
        Assert.Equal(expected, PolishNumberWords.ToWords(number));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Rejects_numbers_outside_supported_range(int number)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PolishNumberWords.ToWords(number));
    }
}
