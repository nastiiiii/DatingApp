namespace API.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var ageDiff = today.Year - date.Year;
        
        if (date > today.AddYears(-ageDiff)) ageDiff--;
        
        return ageDiff;
    }
}