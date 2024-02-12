namespace ConferenceProgram.Tests;

public class ConferenceProgramReaderTests
{
    [Fact]
    public async Task ReadSessionsFile()
    {
        // Arrange
        var reader = new ConferenceProgramReader();

        // Act
        var sessions = await reader.ReadSessionsAsync(/* use default file sessions.json */);

        // Assert
        Assert.NotNull(sessions);
        Assert.NotEmpty(sessions);
    }

    [Fact]
    public async Task HtmlConverter()
    {
        // Arrange
        var reader = new ConferenceProgramReader();

        // Act
        var sessions = await reader.ReadSessionsAsync(/* use default file sessions.json */);

        // Assert
        Assert.DoesNotContain(sessions, s => (s.LongAbstract?.Contains("<p>") ?? false) || (s.Details?.Contains("<p>") ?? false));
    }

    [Fact]
    public async Task Dates()
    {
        // Arrange
        var reader = new ConferenceProgramReader();

        // Act
        var sessions = await reader.ReadSessionsAsync(/* use default file sessions.json */);

        // Assert
        Assert.Equal(8, sessions.Select(s => s.LocalizedStartDate.LocalDateTime.Hour).Min());
    }
}