using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Db;
using TG.Auth.Api.Errors;
using TG.Auth.Api.Models.Response;
using TG.Core.App.OperationResults;
using TG.Core.App.Services;
using TG.Core.Db.Postgres.Extensions;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;

namespace TG.Auth.Api.Application.Bans
{
    public record UnbanUserCommand(Guid Id, string AdminUserLogin)
        : IRequest<OperationResult<BanResponse>>;
    
    public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, OperationResult<BanResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITopicProducer<UserCancellationMessage> _topicProducer;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UnbanUserCommandHandler(ApplicationDbContext dbContext, ITopicProducer<UserCancellationMessage> topicProducer,
            IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _dbContext = dbContext;
            _topicProducer = topicProducer;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<BanResponse>> Handle(UnbanUserCommand cmd, CancellationToken cancellationToken)
        {
            var ban = await _dbContext.Bans
                .Include(b => b.User)
                .FirstOrDefaultAsync(u => u.Id == cmd.Id, cancellationToken);
            if (ban is null)
            {
                return AppErrors.NotFound;
            }

            ban.User!.BanId = null;
            ban.UnbannedTime = _dateTimeProvider.UtcNow;

            var mqMessage = _mapper.Map<UserCancellationMessage>(ban.User);
            mqMessage.Type = UserCancellationType.Unbanned;
            await _dbContext.SaveChangesAtomicallyAsync(() => _topicProducer.SendMessageAsync(mqMessage));

            return _mapper.Map<BanResponse>(ban);
        }
    }
}