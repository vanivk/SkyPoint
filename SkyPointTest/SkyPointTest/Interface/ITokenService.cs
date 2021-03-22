using SkyPointTest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyPointTest.Interface
{
    public interface ITokenService
    {
        string CreateToken(Users user);

        string ValidateJwtToken(string token);
    }
}
