using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ConferenceProgram;

// Records used to deserialize the JSON data in session.json

public record Expert(
    [property: JsonPropertyName("surname")] string Surname,
    [property: JsonPropertyName("forename")] string Forename,
    [property: JsonPropertyName("company")] string Company,
    [property: JsonPropertyName("bio")] string Bio,
    [property: JsonPropertyName("slug")] string Slug
);

public record ExpertSummary(
    [property: JsonPropertyName("surname")] string Surname,
    [property: JsonPropertyName("forename")] string Forename,
    [property: JsonPropertyName("company")] string Company
);

public record PrimaryTrack(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug
);

public record Session(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("localizedStartDate")]
    [property: JsonConverter(typeof(LocalizedDateTimeJsonConverter))] DateTimeOffset LocalizedStartDate,
    [property: JsonPropertyName("localizedEndDate")]
    [property: JsonConverter(typeof(LocalizedDateTimeJsonConverter))] DateTimeOffset LocalizedEndDate,
    [property: JsonPropertyName("longAbstract")]
    [property: JsonConverter(typeof(HtmlToMarkdownJsonConverter))] string LongAbstract,
    [property: JsonPropertyName("workshopShortLabel")] string WorkshopShortLabel,
    [property: JsonPropertyName("roomName")] string RoomName,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("slugNames")] IReadOnlyList<string> SlugNames,
    [property: JsonPropertyName("workshopRequirements")] string WorkshopRequirements,
    [property: JsonPropertyName("details")]
    [property: JsonConverter(typeof(HtmlToMarkdownJsonConverter))] string Details,
    [property: JsonPropertyName("sessionType")] SessionType SessionType,
    [property: JsonPropertyName("primaryTrack")] PrimaryTrack PrimaryTrack,
    [property: JsonPropertyName("tracks")] IReadOnlyList<Track> Tracks,
    [property: JsonPropertyName("experts")] IReadOnlyList<Expert> Experts
);

public record SessionSummary(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("localizedStartDate")]
    [property: JsonConverter(typeof(LocalizedDateTimeJsonConverter))] DateTimeOffset LocalizedStartDate,
    [property: JsonPropertyName("localizedEndDate")]
    [property: JsonConverter(typeof(LocalizedDateTimeJsonConverter))] DateTimeOffset LocalizedEndDate
);

public record SessionType(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug
);

public record Track(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug
);

public class HtmlToMarkdownJsonConverter : JsonConverter<string>
{
    public override string Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var html = reader.GetString();
        if (html is null) { return string.Empty; }
        var converter = new ReverseMarkdown.Converter();
        return converter.Convert(html);
    }

    public override void Write(
        Utf8JsonWriter writer,
        string markdownValue,
        JsonSerializerOptions options)
        // We do not need to convert markdown back to HTML for this sample
        => writer.WriteStringValue(markdownValue);
}

public partial class LocalizedDateTimeJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var html = reader.GetString();
        if (html is null || !LocalizedDateTimeRegex().IsMatch(html)) { return DateTimeOffset.MinValue; }
        var match = LocalizedDateTimeRegex().Match(html);
        return new DateTimeOffset(new DateTime(
            int.Parse(match.Groups[3].Value),
            int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[4].Value) + 1,
            int.Parse(match.Groups[5].Value),
            int.Parse(match.Groups[6].Value)
        ), TimeSpan.FromHours(1));
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset dateTimeValue,
        JsonSerializerOptions options) => writer.WriteStringValue(dateTimeValue.ToString("o"));

    [GeneratedRegex(@"^(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})$")]
    private static partial Regex LocalizedDateTimeRegex();
}