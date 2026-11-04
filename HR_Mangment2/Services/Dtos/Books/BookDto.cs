using System;
using Volo.Abp.Application.Dtos;
using HR_Mangment2.Entities.Books;
namespace HR_Mangment2.Services.Dtos.Books;
public class BookDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; }
    public BookType Type { get; set; }
    public DateTime PublishDate { get; set; }
    public float Price { get; set; }
}