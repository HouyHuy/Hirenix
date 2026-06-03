using System.Text.Json;
using System.Text.Json.Serialization;
using Hirenix.Domain.Enums;

namespace Hirenix.Infrastructure.Converters;

/// <summary>
/// JSON converter for CompanySize enum to support user-friendly string values
/// Converts between "51-200" (JSON) ↔ CompanySize.Size_51_200 (Enum)
/// </summary>
public class CompanySizeJsonConverter : JsonConverter<CompanySize?>
{
    private static readonly Dictionary<string, CompanySize> StringToEnum = new()
    {
        { "1-10", CompanySize.Size_1_10 },
        { "11-50", CompanySize.Size_11_50 },
        { "51-200", CompanySize.Size_51_200 },
        { "201-500", CompanySize.Size_201_500 },
        { "500+", CompanySize.Size_500Plus }
    };

    private static readonly Dictionary<CompanySize, string> EnumToString = 
        StringToEnum.ToDictionary(x => x.Value, x => x.Key);

    public override CompanySize? Read(
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            
            // Try user-friendly format first (e.g., "51-200")
            if (value != null && StringToEnum.TryGetValue(value, out var enumValue))
                return enumValue;

            // Fallback: try parse as enum name (e.g., "Size_51_200")
            if (value != null && Enum.TryParse<CompanySize>(value, true, out var parsedEnum))
                return parsedEnum;

            // Provide helpful error message
            throw new JsonException(
                $"Invalid company size '{value}'. " +
                $"Valid values: 1-10, 11-50, 51-200, 201-500, 500+"
            );
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing CompanySize");
    }

    public override void Write(
        Utf8JsonWriter writer, 
        CompanySize? value, 
        JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // Write user-friendly format (e.g., "51-200")
        if (EnumToString.TryGetValue(value.Value, out var stringValue))
        {
            writer.WriteStringValue(stringValue);
        }
        else
        {
            // Fallback to enum name
            writer.WriteStringValue(value.ToString());
        }
    }
}
