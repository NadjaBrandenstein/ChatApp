using System;
using System.Collections.Generic;

namespace efscaffold.Entities;

public partial class Message
{
    public int Messageid { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Sentat { get; set; }

    public int Senderuserid { get; set; }

    public int Roomid { get; set; }

    public int? Recipientuserid { get; set; }

    public virtual Login? Recipientuser { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual Login Senderuser { get; set; } = null!;
}
