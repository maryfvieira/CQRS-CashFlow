using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CashFlow.Application.Validators;


namespace CashFlow.ConsolidationApi.Models.Requests;

public record ReportDailyRequest(

    [Required(ErrorMessage = "Por favor, informe o identificador da empresa")]
    [ValidateCompanyAccount]
    Guid CompanyAccountId,

    [Required(ErrorMessage = "Por favor, informe a data inicial")]
    //[property: JsonConverter(typeof(BrazilianDateTimeJsonConverter))]
    [ValidateInitialDate]
    DateTime InitialDate,

    [Required(ErrorMessage = "Por favor, informe a data final")]
    //[property: JsonConverter(typeof(BrazilianDateTimeJsonConverter))]
    [ValidateEndDate]
    DateTime EndDate);


    
    
    
    