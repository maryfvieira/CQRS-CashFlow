﻿using MediatR;

namespace CashFlow.Application.Commands
{
    public sealed record OutFlowCommand(decimal Amount, Guid CompanyAccountId = default!, string Description = default!) : IRequest<Guid>;
}
