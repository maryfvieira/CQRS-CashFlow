using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using AutoFixture.Xunit2;
using CashFlow.Identity.Extensions;
using CashFlow.Identity.Models.Requests;
using CashFlow.Identity.Models.Responses;
using CashFlow.Identity.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Xunit;

namespace CashFlow.Identity.UnitTests;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Fixture _fixture;
    private readonly Mock<ITokenizationService> _tokenServiceMock = new();

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_tokenServiceMock.Object);
            });
        });

        _fixture = new Fixture();
        _fixture.Customize<AuthRequest>(c => c
            .With(x => x.Username, "validuser")
            .With(x => x.Password, "validpass12"));
    }

    // [Fact]
    // public async Task POST_login_should_return_created_with_token_when_valid_credentials()
    // {
    //     // Arrange
    //     var client = _factory.CreateClient();
    //     var request = _fixture.Create<AuthRequest>();
    //     var expectedToken = new UserRegistrationResponse("generated_jwt_token");
    //
    //     // _tokenServiceMock.Setup(x => x.GenerateToken(request))
    //     //     .ReturnsAsync(new Result<UserRegistrationResponse>(expectedToken, HttpStatusCode.Created));
    //     
    //     _tokenServiceMock.Setup(x => x.GenerateToken(request))
    //         .ReturnsAsync(new Result<UserRegistrationResponse>());
    //
    //     // Act
    //     var response = await client.PostAsJsonAsync("/api/v1/identity/login", request);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.Created);
    //     var result = await response.Content.ReadFromJsonAsync<UserRegistrationResponse>();
    //     result.Should().BeEquivalentTo(expectedToken);
    // }

    [Theory]
    [InlineData("usr", "short", "usuario deve ter entre 3 e 30 caracteres")]
    [InlineData(null, "password", "informe o usuario")]
    [InlineData("validuser", "1234567", "senha deve ter de 8 a 10 caracteres")]
    public async Task POST_login_should_validate_request_model(
        string username, 
        string password,
        string expectedError)
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidRequest = new AuthRequest(password, username);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/identity/login", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(expectedError);
    }

    // [Fact]
    // public async Task POST_login_should_return_bad_request_when_invalid_credentials()
    // {
    //     // Arrange
    //     var client = _factory.CreateClient();
    //     var request = _fixture.Create<AuthRequest>();
    //
    //     _tokenServiceMock.Setup(x => x.GenerateToken(request)).ReturnsAsync()
    //     _tokenServiceMock.Setup(x => x.GenerateToken(request))
    //         .ReturnsAsync(new Result<UserRegistrationResponse>(new Error("Credenciais inválidas"), HttpStatusCode.BadRequest));
    //
    //     // Act
    //     var response = await client.PostAsJsonAsync("/api/v1/identity/login", request);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //     var error = await response.Content.ReadFromJsonAsync<Error>();
    //     error.Message.Should().Be("Credenciais inválidas");
    // }

    // [Fact]
    // public async Task POST_validate_token_should_return_user_details_when_valid_token()
    // {
    //     // Arrange
    //     var client = _factory.CreateClient();
    //     var validToken = "valid.jwt.token";
    //     var expectedUser = new UserDetailResponse("user@email.com", "validuser", ["Admin"]);
    //     
    //     _tokenServiceMock.Setup(x => x.ValidateToken(validToken)).ReturnsAsync(Result<UserDetailResponse>.Created(new UserDetailResponse(expectedUser))
    //         //Result<UserRegistrationResponse>.Created(new UserRegistrationResponse(token)
    //     
    //     _tokenServiceMock.Setup(x => x.ValidateToken(validToken))
    //         .ReturnsAsync(new Result<UserDetailResponse>(expectedUser, HttpStatusCode.OK));
    //
    //     // Act
    //     var response = await client.PostAsJsonAsync("/api/v1/identity/validateToken", validToken);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.OK);
    //     var user = await response.Content.ReadFromJsonAsync<UserDetailResponse>();
    //     user.Should().BeEquivalentTo(expectedUser);
    // }
    //
    // [Fact]
    // public async Task POST_validate_token_should_return_unauthorized_when_invalid_token()
    // {
    //     // Arrange
    //     var client = _factory.CreateClient();
    //     var invalidToken = "invalid.token";
    //
    //     _tokenServiceMock.Setup(x => x.ValidateToken(invalidToken))
    //         .ReturnsAsync(new Result<UserDetailResponse>(new Error("Token inválido"), HttpStatusCode.Unauthorized));
    //
    //     // Act
    //     var response = await client.PostAsJsonAsync("/api/v1/identity/validateToken", invalidToken);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    // }
}