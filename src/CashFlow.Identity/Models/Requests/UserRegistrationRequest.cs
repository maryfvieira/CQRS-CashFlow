using System.ComponentModel.DataAnnotations;

namespace CashFlow.Identity.Models.Requests;

public record UserRegistrationRequest(
    
    [Required(ErrorMessage = "Por favor informe a senha")]
    [StringLength(10, MinimumLength = 8, ErrorMessage = "A senha deve ter de 8 a 10 caracteres")]
    string Password,

    [Required(ErrorMessage = "Por favor informe o usuario")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "O usuario deve ter entre 3 e 30 caracteres")]
    string Username,
    
    [Required(ErrorMessage = "Por favor informe o email")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "O email deve ter entre  e 100 caracteres")]
    string Email);