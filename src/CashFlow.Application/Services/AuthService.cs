using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CashFlow.Application.Dtos;
using CashFlow.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace CashFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(JwtSettings jwtSettings, IConfiguration config, ILogger<AuthService> logger)
    {
        _jwtSettings = jwtSettings;
        _config = config;
        _logger = logger;
    }
    
    public string? GenerateToken(UserDto user)
    {
        try
        {
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            if (key.KeySize < 256)
                throw new ArgumentException("Chave JWT muito curta");

            var now = DateTime.UtcNow;
            //var expires = now.AddHours(_config.GetValue<double>("Jwt:ExpiryInHours"));
            var expires = now.AddHours(_jwtSettings.ExpiryInHours);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                //new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expires).ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
                //new Claim(JwtRegisteredClaimNames.Iss, _config["Jwt:Issuer"])  // Adicionando o Issuer no token
                new Claim(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer)
            };
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                //issuer: _config["Jwt:Issuer"],
                //audience: _config["Jwt:Audience"],
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials
            );
            

            _logger.LogDebug("""
                Token gerado:
                - Emitido em (UTC): {IssueDate}
                - Expira em (UTC): {ExpirationDate}
                - Duração total: {TotalHours} horas
                """, 
                now.ToString("yyyy-MM-dd HH:mm:ss"), 
                expires.ToString("yyyy-MM-dd HH:mm:ss"), 
                //_config.GetValue<double>("Jwt:ExpiryInHours"
                _jwtSettings.ExpiryInHours);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar token");
            return null;
        }
    }
    
    public ClaimsPrincipal? ValidateToken(string token)
    {
        //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        if (key.KeySize < 256)
            throw new ArgumentException("Chave JWT muito curta");

        var tokenHandler = new JwtSecurityTokenHandler();
        
        if (!tokenHandler.CanReadToken(token))
        {
            _logger.LogWarning("Token não pode ser lido");
            return null;
        }

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            //ValidIssuer = _config["Jwt:Issuer"],
            //ValidAudience = _config["Jwt:Audience"],
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewInMinutes) // Tolerância ajustada aqui
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);
            
            _logger.LogDebug("""
                Token validado com sucesso:
                - Valido de (UTC): {ValidFrom}
                - Valido até (UTC): {ValidTo}
                - Tempo restante: {Remaining} minutos
                """, 
                validatedToken.ValidFrom.ToString("yyyy-MM-dd HH:mm:ss"), 
                validatedToken.ValidTo.ToString("yyyy-MM-dd HH:mm:ss"), 
                (validatedToken.ValidTo - DateTime.UtcNow).TotalMinutes.ToString("N2"));

            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("""
                Token expirado:
                - Data atual (UTC): {CurrentUtc}
                - Data expiração (UTC): {ExpirationUtc}
                - Tolerância usada: {ClockSkewInMinutes} segundos
                """, 
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), 
                ex.Expires.ToString("yyyy-MM-dd HH:mm:ss"), 
                TimeSpan.FromMinutes(_jwtSettings.ClockSkewInMinutes).TotalSeconds);
            
            return null;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogWarning("Assinatura do token inválida");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Erro de segurança: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado: {Message}", ex.Message);
            return null;
        }
    }
}