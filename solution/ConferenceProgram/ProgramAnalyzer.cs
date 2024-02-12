namespace ConferenceProgram;

public static class ProgramAnalyzer
{
    public static IEnumerable<ExpertSummary> GetExperts(this IEnumerable<Session> sessions)
    {
        var result = sessions
            .Where(s => s.Experts != null)
            .SelectMany(s => s.Experts)
            .Distinct()
            .Select(e => new ExpertSummary(e.Surname, e.Forename, e.Company));
        return result;
    }

    public static IEnumerable<SessionSummary> GetSessionsByExpert(this IEnumerable<Session> sessions, string forename, string surname)
        => sessions
            .Where(s => s.Experts != null)
            .Where(s => s.Experts.Any(e => e.Forename == forename && e.Surname == surname))
            .Select(s => new SessionSummary(s.Name, s.LocalizedStartDate, s.LocalizedEndDate));

    public static Session? GetSessionByName(this IEnumerable<Session> sessions, string name)
        => sessions.FirstOrDefault(s => s.Name == name);

    public static IEnumerable<Session> GetAllWorkshops(this IEnumerable<Session> sessions)
        => sessions.Where(s => s.SessionType.Type == "WORKSHOP");

    public static IEnumerable<Session> GetAllKeynotes(this IEnumerable<Session> sessions)
        => sessions.Where(s => s.SessionType.Type == "KEYNOTE");

    public static IEnumerable<Session> GetAllSessions(this IEnumerable<Session> sessions)
        => sessions.Where(s => s.SessionType.Type == "SESSION");

    public static IEnumerable<string> GetAllTracks(this IEnumerable<Session> sessions)
        => sessions.SelectMany(s => s.Tracks).Select(t => t.Name).Distinct();

    public static IEnumerable<Session> GetSessionsAtTime(this IEnumerable<Session> sessions, DateTimeOffset time)
        => sessions.Where(s => s.LocalizedStartDate.LocalDateTime <= time.LocalDateTime
            && s.LocalizedEndDate.LocalDateTime >= time.LocalDateTime);
}