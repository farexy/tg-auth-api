using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TG.Auth.Api.Db;
using TG.Auth.Api.Models.Response;
using TG.Core.App.OperationResults;

namespace TG.Auth.Api.Application.Bans
{
    public record GetBansQuery(Guid? UserId, string? UserLogin) : IRequest<OperationResult<IReadOnlyList<BanResponse>>>;
    
    public class GetBansQueryHandler : IRequestHandler<GetBansQuery, OperationResult<IReadOnlyList<BanResponse>>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetBansQueryHandler(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<OperationResult<IReadOnlyList<BanResponse>>> Handle(GetBansQuery request, CancellationToken cancellationToken)
        {
            var bans = await _dbContext.Bans
                .Include(b => b.User)
                .Where(b => request.UserId == null || b.UserId == request.UserId)
                .Where(b => request.UserLogin == null || b.User!.Login == request.UserLogin)
                .OrderByDescending(b => b.BanTime)
                .ToListAsync(cancellationToken);
            return new OperationResult<IReadOnlyList<BanResponse>>(_mapper.Map<IReadOnlyList<BanResponse>>(bans));
        }
    }
} 