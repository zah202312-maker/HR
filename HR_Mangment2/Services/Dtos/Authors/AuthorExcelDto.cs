using System;

namespace HR_Mangment2.Services.Dtos.Authors;

public class AuthorExcelDto
{
    public string Name { get; set; }

    public DateTime BirthDate { get; set; }

    public string? ShortBio { get; set; }
}
