﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HodHod.Otp.Dto;

public class OtpLoginInput
{
    public string PhoneNumber { get; set; }
    public string Code { get; set; }
    public bool? SingleSignIn { get; set; }
    public string ReturnUrl { get; set; }
}
