using Microsoft.AspNetCore.Mvc;
using System;

namespace SportsPortal.Controllers
{
    public class BaseController : Controller
    {
        protected int GetSelectedYear()
        {
            if (int.TryParse(HttpContext.Request.Query["year"], out int year))
            {
                return year;
            }
            return DateTime.Now.Year; // Default to current year
        }
    }
}
