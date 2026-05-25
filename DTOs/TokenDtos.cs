namespace ATS.API.DTOs;
public class TokenRequestDto
{
    public string  GrantType  { get; set; } = "password";
    public string Username { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}

public class TokenResponseDto
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("scope")]
    public string? Scope { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; }= null;
}