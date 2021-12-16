using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TG.Auth.Api.Db;
using TG.Auth.Api.Entities;
using TG.Core.App.Services;
using TG.Core.Db.Postgres.Extensions;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;

namespace TG.Auth.Api.Application.Bans
{
    public record UserBanExpiredEvent(Ban Ban) : INotification;
    
    public class UserBanExpiredEventHandler : INotificationHandler<UserBanExpiredEvent>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITopicProducer<UserCancellationMessage> _topicProducer;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UserBanExpiredEventHandler(ApplicationDbContext dbContext, ITopicProducer<UserCancellationMessage> topicProducer,
            IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _dbContext = dbContext;
            _topicProducer = topicProducer;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task Handle(UserBanExpiredEvent notification, CancellationToken cancellationToken)
        {
            var ban = notification.Ban;
            var user = await _dbContext.Users.FindAsync(notification.Ban.UserId);
            user.BanId = null;

            ban.UnbannedTime = _dateTimeProvider.UtcNow;

            var mqMessage = _mapper.Map<UserCancellationMessage>(user);
            mqMessage.Type = UserCancellationType.Unbanned;
            await _dbContext.SaveChangesAtomicallyAsync(() => _topicProducer.SendMessageAsync(mqMessage));
        }
    }
}