using CashFlow.Identity.Models.Requests;
using CashFlow.Identity.Models.Responses;

namespace CashFlow.Identity.Services;

public interface ITokenizationService
{
    public Task<Result<TokenizationResponse>> GenerateToken(AuthRequest authRequest);
    public Task<Result<UserDetailResponse>> ValidateToken(string token);
}