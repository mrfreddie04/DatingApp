using System;

namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateTime dob) 
        {            
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (today.Month < dob.Month || today.Month == dob.Month && today.Day < dob.Month) 
                age--;

            return age;
        }
    }
}