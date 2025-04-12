using System;
using System.Collections.Generic;

namespace OnlineLibrary.Data.Entities;

public partial class BooksDatum: BaseEntity
{
    public long? Id { get; set; }

    public string? Title { get; set; }

    public string? Category { get; set; }

    public string? Author { get; set; }

    public string? Summary { get; set; }

    public string? Text { get; set; }

    public string? Cover { get; set; }
}
