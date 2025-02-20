using System;
using System.Collections.Generic;

namespace OnlineLibrary.Data.Entities;

public  class BooksDatum
{
    public long? Id { get; set; }

    public string? Title { get; set; }

    public string? Author { get; set; }

    public string? Text { get; set; }
}
