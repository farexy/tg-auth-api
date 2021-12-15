using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Constants;
using TG.Auth.Api.Db;
using TG.Auth.Api.Entities;
using TG.Auth.Api.Errors;
using TG.Auth.Api.Models.Response;
using TG.Core.App.Exceptions;
using TG.Core.App.OperationResults;
using TG.Core.App.Services;
using TG.Core.Db.Postgres.Extensions;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;

namespace TG.Auth.Api.Application.Bans
{
    public record BanUserCommand(Guid UserId, DateTime? BanTill, string? Comment, string Reason, string AdminUserLogin)
        : IRequest<OperationResult<BanResponse>>;
    
    public class BanUserCommandHandler : IRequestHandler<BanUserCommand, OperationResult<BanResponse>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITopicProducer<UserCancellationMessage> _topicProducer;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        public BanUserCommandHandler(ApplicationDbContext dbContext, ITopicProducer<UserCancellationMessage> topicProducer,
            IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _dbContext = dbContext;
            _topicProducer = topicProducer;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<OperationResult<BanResponse>> Handle(BanUserCommand cmd, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == cmd.UserId, cancellationToken);
            if (user is null)
            {
                return AppErrors.NotFound;
            }

            if (!Enum.TryParse(cmd.Reason, out BanReason reason))
            {
                throw new BusinessLogicException("Invalid reason");
            }
            
            var ban = new Ban
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BanTime = _dateTimeProvider.UtcNow,
                BannedTill = cmd.BanTill,
                AdminUserLogin = cmd.AdminUserLogin,
                Comment = cmd.Comment,
                Reason = reason,
            };
            user.BanId = ban.Id;

            await _dbContext.AddAsync(ban, cancellationToken);

            var mqMessage = _mapper.Map<UserCancellationMessage>(user);
            mqMessage.Type = UserCancellationType.Banned;
            await _dbContext.SaveChangesAtomicallyAsync(() => _topicProducer.SendMessageAsync(mqMessage));

            return _mapper.Map<BanResponse>(ban);
        }
    }
}