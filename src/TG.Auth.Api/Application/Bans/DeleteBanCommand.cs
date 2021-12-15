using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Db;
using TG.Auth.Api.Errors;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Application.Bans
{
    public record DeleteBanCommand(Guid Id) : IRequest<OperationResult>;
    
    public class DeleteBanCommandHandler : IRequestHandler<DeleteBanCommand, OperationResult>
    {
        private readonly ApplicationDbContext _dbContext;

        public DeleteBanCommandHandler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult> Handle(DeleteBanCommand cmd, CancellationToken cancellationToken)
        {
            var ban = await _dbContext.Bans
                .FirstOrDefaultAsync(u => u.Id == cmd.Id, cancellationToken);
            if (ban is null)
            {
                return AppErrors.NotFound;
            }

            if (ban.UnbannedTime is null)
            {
                return AppErrors.BanActive;
            }

            _dbContext.Remove(ban);
            return OperationResult.Success();
        }
    }
}