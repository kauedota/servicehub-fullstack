using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceHub.Api.Configuration;

// Gravamos todas as datas em UTC (SYSUTCDATETIME / DateTime.UtcNow). Mas o SQL Server (DATETIME2)
// não guarda fuso horário: ao ler, EF Core e Dapper devolvem DateTime com Kind = Unspecified,
// e o JSON sai sem o "Z" no final ("2026-09-24T15:00:00"). O navegador interpretaria como horário LOCAL.
// Este conversor marca a data como UTC na serialização => "2026-09-24T15:00:00Z".
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetDateTime().ToUniversalTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
        writer.WriteStringValue(utc);
    }
}
