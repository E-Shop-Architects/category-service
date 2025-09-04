using Application.Features.Categories.Constants;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Core.Application.Pipelines.Caching;
using Core.Application.Pipelines.Logging;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;

namespace Application.Features.Categories.Queries.GetListByDynamic;

public class GetListByDynamicCategoryQuery : IRequest<GetListResponse<GetListByDynamicCategoryListItemDto>>
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery Dynamic { get; set; }

        
    public bool BypassCache { get; }
        public string CacheKey => $"GetListByDynamicCategory-{PageRequest.PageIndex}-{PageRequest.PageSize}";
        public string CacheGroupKey => "GetCategories";
        public TimeSpan? SlidingExpiration { get; }
        

    public class GetListByDynamicCategoryQueryHandler : IRequestHandler<GetListByDynamicCategoryQuery, GetListResponse<GetListByDynamicCategoryListItemDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetListByDynamicCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetListByDynamicCategoryListItemDto>> Handle(GetListByDynamicCategoryQuery request, CancellationToken cancellationToken)
        {
            Paginate<Category> categories = await _categoryRepository.GetListByDynamicAsync(
                dynamic: request.Dynamic,
                index: request.PageRequest.PageIndex,
                size: request.PageRequest.PageSize,
                cancellationToken: cancellationToken
            );

            GetListResponse<GetListByDynamicCategoryListItemDto> response = _mapper.Map<GetListResponse<GetListByDynamicCategoryListItemDto>>(categories);
            return response;
        }
    }
}
