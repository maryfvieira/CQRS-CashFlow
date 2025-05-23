using System.Security.Claims;
using CashFlow.Application.Services;
using CashFlow.Identity.Models.Requests;
using CashFlow.Identity.Models.Responses;

namespace CashFlow.Identity.Services;

public class TokenizationService: ITokenizationService
{
    private IUserService _userService;
    private IAuthService _authService;
    
    public TokenizationService(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }
    
    public async Task<Result<TokenizationResponse>> GenerateToken(AuthRequest authRequest)
    {
        try
        {
            var user = await _userService.ValidateCredentialsAsync(authRequest.Username, authRequest.Password);
        
            if(user == null)
                return await Task.FromResult(Result<TokenizationResponse>
                    .InternalServerError(["Usuario ou senha invalidos"]));
        
            string? token = _authService.GenerateToken(user);
        
            if (token == null)
                return await Task.FromResult(Result<TokenizationResponse>
                    .InternalServerError(["Ocorreu um problema ao generar o token, tente mais tarde"]));
            
            return await Task.FromResult(Result<TokenizationResponse>.Created(new TokenizationResponse(token)));
        }
        catch (Exception ex)
        {
            return await Task.FromResult(Result<TokenizationResponse>
                .InternalServerError(["Ocorreu um erro ao generar o token: " + ex.Message]));
        }
    }

    public async Task<Result<UserDetailResponse>> ValidateToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                
                return await Task.FromResult(Result<UserDetailResponse>
                    .BadRequest(["Token não fornecido"]));

            var principal = _authService.ValidateToken(token);
            if (principal == null)
            {
                return await Task.FromResult(Result<UserDetailResponse>
                    .Unauthorized(["Token inválido ou expirado"]));
            }
            
            var claims = principal.Claims as Claim[] ?? principal.Claims.ToArray();

            return await Task.FromResult(Result<UserDetailResponse>.Accepted(new UserDetailResponse(
                Email: claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty,
                UserName: claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty,
                Roles: claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
                new List<string>()
            )));
            
        }
        catch (Exception ex)
        {
            return await Task.FromResult(Result<UserDetailResponse>
                .InternalServerError(["Ocorreu um erro ao generar o token: " + ex.Message]));
        }
    }
}