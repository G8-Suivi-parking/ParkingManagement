using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ParkingManagement.API.AI;

namespace ParkingManagement.API.Services;

public class VlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public VlmService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    // ============================================================
    // ANALYSER L'IMAGE DU PARKING
    // ============================================================

    public async Task<string> AnalyzeImageAsync(
        byte[] imageBytes,
        List<ParkingZone> zones)
    {
        // --------------------------------------------------------
        // 1. Récupérer la clé OpenAI depuis appsettings.json
        // --------------------------------------------------------

        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception(
                "La clé OpenAI n'est pas configurée. " +
                "Ajoutez OpenAI:ApiKey dans appsettings.json."
            );
        }

        if (imageBytes == null || imageBytes.Length == 0)
        {
            throw new Exception(
                "L'image du parking est vide."
            );
        }

        if (zones == null || zones.Count == 0)
        {
            throw new Exception(
                "Aucune zone de calibration n'est configurée."
            );
        }

        // --------------------------------------------------------
        // 2. Convertir l'image en Base64
        // --------------------------------------------------------

        var base64Image =
            Convert.ToBase64String(imageBytes);

        // --------------------------------------------------------
        // 3. Préparer les zones
        // --------------------------------------------------------

        var zonesText = string.Join(
            "\n",
            zones.Select(z =>
                $"- Zone {z.Id} ({z.Nom}) : " +
                $"X={z.X}, Y={z.Y}, " +
                $"Largeur={z.Largeur}, Hauteur={z.Hauteur}"
            )
        );

        // --------------------------------------------------------
        // 4. Prompt
        // --------------------------------------------------------

        var prompt =
            "Tu es un système intelligent d'analyse de parking. " +
            "Analyse l'image fournie avec les zones de stationnement calibrées. " +
            "Pour chaque zone, détermine si elle contient un véhicule. " +
            "Une zone occupée contient un véhicule. " +
            "Une zone libre ne contient aucun véhicule. " +
            "Compte les véhicules présents et les places libres. " +
            "Le nombre de véhicules plus le nombre de places libres " +
            "doit être égal au nombre total de zones. " +
            "Réponds UNIQUEMENT avec un objet JSON valide, sans markdown " +
            "et sans explication. " +
            "Le format doit être exactement : " +
            "{\"vehicles\":0,\"freeSpaces\":0}" +
            "\n\n" +
            "Zones de calibration :\n" +
            zonesText;

        // --------------------------------------------------------
        // 5. Préparer la requête OpenAI
        // --------------------------------------------------------

        var requestBody = new
        {
            model = "gpt-4o",

            input = new object[]
            {
                new
                {
                    role = "user",

                    content = new object[]
                    {
                        new
                        {
                            type = "input_text",
                            text = prompt
                        },

                        new
                        {
                            type = "input_image",

                            image_url =
                                $"data:image/jpeg;base64,{base64Image}"
                        }
                    }
                }
            }
        };

        var json =
            JsonSerializer.Serialize(requestBody);

        // --------------------------------------------------------
        // 6. Créer la requête HTTP
        // --------------------------------------------------------

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/responses"
            );

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey
            );

        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

        // --------------------------------------------------------
        // 7. Appeler OpenAI
        // --------------------------------------------------------

        var response =
            await _httpClient.SendAsync(request);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        // --------------------------------------------------------
        // 8. Vérifier la réponse
        // --------------------------------------------------------

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Erreur OpenAI : " +
                $"{response.StatusCode} - " +
                $"{responseContent}"
            );
        }

        return responseContent;
    }

    // ============================================================
    // EXTRAIRE LES VÉHICULES ET PLACES LIBRES
    // ============================================================

    public (int vehicles, int freeSpaces) ExtractOccupancy(
        string response)
    {
        try
        {
            using var document =
                JsonDocument.Parse(response);

            // ----------------------------------------------------
            // Récupérer "output"
            // ----------------------------------------------------

            if (!document.RootElement.TryGetProperty(
                    "output",
                    out var output))
            {
                return (0, 0);
            }

            // ----------------------------------------------------
            // Parcourir les éléments de output
            // ----------------------------------------------------

            foreach (var outputItem
                     in output.EnumerateArray())
            {
                if (!outputItem.TryGetProperty(
                        "content",
                        out var content))
                {
                    continue;
                }

                // ------------------------------------------------
                // Parcourir content
                // ------------------------------------------------

                foreach (var contentItem
                         in content.EnumerateArray())
                {
                    if (!contentItem.TryGetProperty(
                            "text",
                            out var textElement))
                    {
                        continue;
                    }

                    var text =
                        textElement.GetString();

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    // ------------------------------------------------
                    // Nettoyer éventuellement les ```json
                    // ------------------------------------------------

                    text = text
                        .Replace("```json", "")
                        .Replace("```", "")
                        .Trim();

                    // ------------------------------------------------
                    // Parser le JSON retourné par le VLM
                    // ------------------------------------------------

                    using var resultJson =
                        JsonDocument.Parse(text);

                    var root =
                        resultJson.RootElement;

                    var vehicles = 0;
                    var freeSpaces = 0;

                    if (root.TryGetProperty(
                            "vehicles",
                            out var vehiclesElement))
                    {
                        vehicles =
                            vehiclesElement.GetInt32();
                    }

                    if (root.TryGetProperty(
                            "freeSpaces",
                            out var freeSpacesElement))
                    {
                        freeSpaces =
                            freeSpacesElement.GetInt32();
                    }

                    return (
                        Math.Max(vehicles, 0),
                        Math.Max(freeSpaces, 0)
                    );
                }
            }

            return (0, 0);
        }
        catch
        {
            return (0, 0);
        }
    }
}