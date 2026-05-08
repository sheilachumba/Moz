using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class KycStatusConverter : JsonConverter<KycStatus>
    {
        public override KycStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var raw = reader.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                    return KycStatus.Open;

                // Normalize: trim, replace spaces and hyphens with underscores
                var norm = raw.Trim()
                              .Replace("-", "_")
                              .Replace(" ", "_");

                if (Enum.TryParse<KycStatus>(norm, true, out var result))
                    return result;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int num) && Enum.IsDefined(typeof(KycStatus), num))
                    return (KycStatus)num;
            }

            // Fallback
            return KycStatus.Open;
        }

        public override void Write(Utf8JsonWriter writer, KycStatus value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
