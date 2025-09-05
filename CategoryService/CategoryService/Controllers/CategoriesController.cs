using Application.Features.Categories.Commands.Create;
using Application.Features.Categories.Commands.Delete;
using Application.Features.Categories.Commands.Update;
using Application.Features.Categories.Queries.GetById;
using Application.Features.Categories.Queries.GetList;
using Application.Features.Categories.Queries.GetListByDynamic;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;

namespace WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<CreatedCategoryResponse>> Add([FromBody] CreateCategoryCommand command)
    {
        CreatedCategoryResponse response = await Mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { response.Id }, response);
    }

    [HttpPut]
    public async Task<ActionResult<UpdatedCategoryResponse>> Update([FromBody] UpdateCategoryCommand command)
    {
        UpdatedCategoryResponse response = await Mediator.Send(command);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<DeletedCategoryResponse>> Delete([FromRoute] Guid id)
    {
        DeleteCategoryCommand command = new() { Id = id };

        DeletedCategoryResponse response = await Mediator.Send(command);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetByIdCategoryResponse>> GetById([FromRoute] Guid id)
    {
        GetByIdCategoryQuery query = new() { Id = id };

        GetByIdCategoryResponse response = await Mediator.Send(query);

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<GetListResponse<GetListCategoryListItemDto>>> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListCategoryQuery query = new() { PageRequest = pageRequest };

        GetListResponse<GetListCategoryListItemDto> response = await Mediator.Send(query);

        return Ok(response);
    }

    [HttpPost("GetListByDynamic")]
    public async Task<IActionResult> GetListByDynamic([FromQuery] PageRequest pageRequest, [FromBody] DynamicQuery dynamic)
    {
        GetListByDynamicCategoryQuery getListByDynamicCategoryQuery = new() { PageRequest = pageRequest, Dynamic = dynamic };
        GetListResponse<GetListByDynamicCategoryListItemDto> response = await Mediator.Send(getListByDynamicCategoryQuery);
        return Ok(response);
    }
}