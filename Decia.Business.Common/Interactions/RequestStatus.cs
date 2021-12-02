﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Interactions
{
    public enum RequestStatus
    {
        Waiting,
        Expired,
        Rejected,
        AcceptedByNewUser,
        AcceptedByExistingUser
    }
}