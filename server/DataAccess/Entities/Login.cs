using System;
using System.Collections.Generic;

namespace efscaffold.Entities;

public partial class Login
{
    public int Userid { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Roleid { get; set; }

    public virtual ICollection<Message> MessageRecipientusers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenderusers { get; set; } = new List<Message>();

    public virtual Role Role { get; set; } = null!;
}
