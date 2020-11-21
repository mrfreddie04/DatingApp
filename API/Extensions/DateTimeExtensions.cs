using System;

namespace API.Extensions
{
    public static class DateTimeExtensions    
    {
        public static int CalculateAge(this DateTime date)
        {
            var today = DateTime.Today;
            var age = today.Year -  date.Year;
            if(today < date.AddYears(age))
                age--;
            return age;            
        }
    }
}