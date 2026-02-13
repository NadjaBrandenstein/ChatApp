using System;
using System.Collections.Generic;

namespace efscaffold.Entities;

public partial class Room
{
    public int Roomid { get; set; }

    public string Roomname { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
