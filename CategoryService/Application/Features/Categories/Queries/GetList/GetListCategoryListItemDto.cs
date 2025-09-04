
namespace Application.Features.Categories.Queries.GetList;

public class GetListCategoryListItemDto 
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public string Status { get; set; }
}