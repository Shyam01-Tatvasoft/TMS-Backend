using System;
using System.Collections.Generic;

namespace TMS.Repository.Data;

public partial class UserOtp
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string OtpHash { get; set; } = null!;

    public DateTime ExpiryTime { get; set; }

    public DateTime? CreatedAt { get; set; }
}
