using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PMSystem.Models;
using PMSystem.DataAccess;

namespace PMSystem.Utility
{
    public static class DataAccessUtil
    {
        public static User getAdminInfoByEmail(PMSystemDbContext context, string value)
        {
            return context.Users.FirstOrDefault(u => u.Email == value);
        }
    }
}