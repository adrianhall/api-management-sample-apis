using System.ComponentModel.DataAnnotations;

namespace Todo.RestApi.DataTransferObjects;

public class TodoListDto
{
    [Required]
    public Guid? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    public string? Description { get; set; }
    [Required]
    public DateTimeOffset? CreatedDate { get; set; }
    [Required]
    public DateTimeOffset? UpdatedDate { get; set; }
}
