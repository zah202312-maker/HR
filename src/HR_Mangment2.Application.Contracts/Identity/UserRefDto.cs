using System;
using System.Collections.Generic;

namespace HR_Mangment2.Identity;

public class UserRefDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public Guid? PhotoId { get; set; }
}

public class UserRefPagedResultDto
{
    public long TotalCount { get; set; }
    public List<UserRefDto> Items { get; set; } = new();
}
