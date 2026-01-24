using System.Text;
using System.Text.Json;
using ETNA.Api.Models.DTOs;
using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// OCR service implementation using Google Gemini API
/// </summary>
public class GeminiOcrService : IOcrService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiOcrService> _logger;

    // Gemini API endpoint
    private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public GeminiOcrService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiOcrService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<VehicleDataDto> ProcessImageAsync(string base64Image, string mode)
    {
        try
        {
            var apiKey = _configuration["GeminiSettings:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Gemini API Key not configured");
                return CreateErrorResponse("OCR service not configured. Please contact support.");
            }

            // Clean up base64 string (remove data URL prefix if present)
            var cleanBase64 = base64Image;
            if (base64Image.Contains(","))
            {
                cleanBase64 = base64Image.Split(',')[1];
            }

            // Get the appropriate prompt based on mode
            var prompt = GetPromptForMode(mode);

            // Build the request payload for Gemini
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = "image/jpeg",
                                    data = cleanBase64
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1,
                    topK = 1,
                    topP = 1,
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Make API request
            var response = await _httpClient.PostAsync($"{GEMINI_API_URL}?key={apiKey}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
                return CreateErrorResponse("Failed to process image. Please try again.");
            }

            // Parse the response
            return ParseGeminiResponse(responseContent, mode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image with Gemini OCR");
            return CreateErrorResponse($"Error processing image: {ex.Message}");
        }
    }

    private string GetPromptForMode(string mode)
    {
        if (mode.Equals("rc", StringComparison.OrdinalIgnoreCase))
        {
            return @"You are an expert at reading Indian vehicle RC (Registration Certificate) cards.
Analyze this image of an RC card and extract the following information.
Return ONLY a valid JSON object with these exact fields (use null for any field you cannot find):

{
  ""plateNumber"": ""The vehicle registration number (e.g., MH 12 AB 1234)"",
  ""ownerName"": ""Name of the registered owner"",
  ""vehicleBrand"": ""Make/Manufacturer of the vehicle (e.g., Toyota, Honda, Maruti Suzuki)"",
  ""vehicleModel"": ""Model name (e.g., Innova, City, Swift)"",
  ""year"": 2020,
  ""variant"": ""Variant/trim level if visible"",
  ""chassisNumber"": ""Chassis number"",
  ""engineNumber"": ""Engine number"",
  ""fuelType"": ""Petrol/Diesel/CNG/Electric"",
  ""registrationDate"": ""Registration date in DD/MM/YYYY format""
}

Important:
- Return ONLY the JSON object, no other text or markdown
- Use null (not string ""null"") for missing fields
- Year should be a number, not a string
- Clean up any OCR artifacts in the text";
        }
        else // plate mode
        {
            return @"You are an expert at reading Indian vehicle number plates.
Analyze this image and extract the vehicle registration number (number plate).
Return ONLY a valid JSON object with this format:

{
  ""plateNumber"": ""The vehicle registration number (e.g., MH 12 AB 1234)""
}

Important:
- Return ONLY the JSON object, no other text or markdown
- Format the plate number with proper spacing (State Code, District Code, Series, Number)
- Examples: ""MH 12 AB 1234"", ""DL 4C AD 1234"", ""KA 01 MK 5678""
- If you cannot read the plate clearly, return {""plateNumber"": null}";
        }
    }

    private VehicleDataDto ParseGeminiResponse(string responseContent, string mode)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            // Navigate to the text content in Gemini's response structure
            // Response structure: { candidates: [{ content: { parts: [{ text: "..." }] } }] }
            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var firstPart = parts[0];
                    if (firstPart.TryGetProperty("text", out var textElement))
                    {
                        var text = textElement.GetString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            // Clean up the text (remove markdown code blocks if present)
                            text = text.Trim();
                            if (text.StartsWith("```json"))
                            {
                                text = text.Substring(7);
                            }
                            if (text.StartsWith("```"))
                            {
                                text = text.Substring(3);
                            }
                            if (text.EndsWith("```"))
                            {
                                text = text.Substring(0, text.Length - 3);
                            }
                            text = text.Trim();

                            // Parse the JSON from Gemini's response
                            return ParseExtractedJson(text, mode);
                        }
                    }
                }
            }

            _logger.LogWarning("Could not parse Gemini response structure: {Response}", responseContent);
            return CreateErrorResponse("Could not extract data from image. Please try again.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response JSON: {Response}", responseContent);
            return CreateErrorResponse("Error parsing OCR response.");
        }
    }

    private VehicleDataDto ParseExtractedJson(string jsonText, string mode)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            string? GetStringOrNull(string propertyName)
            {
                if (root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
                {
                    return prop.GetString();
                }
                return null;
            }

            int? GetIntOrNull(string propertyName)
            {
                if (root.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number)
                    {
                        return prop.GetInt32();
                    }
                    if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var val))
                    {
                        return val;
                    }
                }
                return null;
            }

            var plateNumber = GetStringOrNull("plateNumber");

            if (string.IsNullOrEmpty(plateNumber))
            {
                return CreateErrorResponse("Could not read the number plate. Please try again with a clearer image.");
            }

            return new VehicleDataDto(
                PlateNumber: plateNumber,
                OwnerName: GetStringOrNull("ownerName"),
                VehicleBrand: GetStringOrNull("vehicleBrand"),
                VehicleModel: GetStringOrNull("vehicleModel"),
                Year: GetIntOrNull("year"),
                Variant: GetStringOrNull("variant"),
                ChassisNumber: GetStringOrNull("chassisNumber"),
                EngineNumber: GetStringOrNull("engineNumber"),
                FuelType: GetStringOrNull("fuelType"),
                RegistrationDate: GetStringOrNull("registrationDate"),
                Success: true,
                ErrorMessage: null
            );
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing extracted JSON: {Json}", jsonText);
            return CreateErrorResponse("Error parsing vehicle data.");
        }
    }

    private static VehicleDataDto CreateErrorResponse(string message)
    {
        return new VehicleDataDto(
            PlateNumber: null,
            OwnerName: null,
            VehicleBrand: null,
            VehicleModel: null,
            Year: null,
            Variant: null,
            ChassisNumber: null,
            EngineNumber: null,
            FuelType: null,
            RegistrationDate: null,
            Success: false,
            ErrorMessage: message
        );
    }
}
