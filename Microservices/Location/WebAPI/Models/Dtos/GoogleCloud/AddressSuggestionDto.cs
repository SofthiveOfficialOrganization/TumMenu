namespace WebAPI.Models.Dtos.GoogleCloud;

public class AddressSuggestionDto : IDto
{
    public string SuggestedAddress { get; set; } // description değeri
    public string SecondaryText { get; set; } // secondary_text değeri
}