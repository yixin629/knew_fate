using System;
using System.Threading.Tasks;

namespace KnewFate.Services
{
    public interface INumerologyService
    {
        Task<string> CalculateLifePathNumberAsync(DateTime birthDate);
    }

    public class NumerologyService : INumerologyService
    {
        public Task<string> CalculateLifePathNumberAsync(DateTime birthDate)
        {
            // Placeholder logic for Numerology calculation
            int lifePathNumber = 0;
            foreach (var digit in birthDate.ToString("yyyyMMdd"))
            {
                if (char.IsDigit(digit))
                {
                    lifePathNumber += int.Parse(digit.ToString());
                }
            }
            while (lifePathNumber > 9)
            {
                lifePathNumber = (lifePathNumber / 10) + (lifePathNumber % 10);
            }
            return Task.FromResult($"Life Path Number: {lifePathNumber}");
        }
    }
}
