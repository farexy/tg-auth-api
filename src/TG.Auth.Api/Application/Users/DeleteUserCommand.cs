using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Db;
using TG.Auth.Api.Errors;
using TG.Core.App.OperationResults;
using TG.Core.Db.Postgres.Extensions;
using TG.Core.ServiceBus;
using TG.Core.ServiceBus.Messages;

namespace TG.Auth.Api.Application.Users
{
    public record DeleteUserCommand(Guid UserId) : IRequest<OperationResult>;
    
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, OperationResult>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITopicProducer<UserDeletedMessage> _topicProducer;
        private readonly IMapper _mapper;

        public DeleteUserCommandHandler(ApplicationDbContext dbContext, ITopicProducer<UserDeletedMessage> topicProducer, IMapper mapper)
        {
            _dbContext = dbContext;
            _topicProducer = topicProducer;
            _mapper = mapper;
        }

        public async Task<OperationResult> Handle(DeleteUserCommand cmd, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == cmd.UserId, cancellationToken);
            if (user is null)
            {
                return AppErrors.NotFound;
            }
            _dbContext.Remove(user);

            await _dbContext.SaveChangesAtomicallyAsync(() =>
                _topicProducer.SendMessageAsync(_mapper.Map<UserDeletedMessage>(user)));
            return OperationResult.Success();
        }
    }
}