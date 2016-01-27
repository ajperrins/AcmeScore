using System;
using System.Runtime.InteropServices;

namespace AcmeScore
{
    /// <summary>
    ///     Represents an contact's weighted score, normalized on a 0 to 100 scale, based on the min and max weighted scores
    ///     in a collection of contacts
    /// </summary>
    public class WeightedNormalizedContactScore
    {
        /// <summary>
        ///     Minimum weighed sum across all contacts
        /// </summary>
        public static float MinWeightedSum { get; set; } = 0;

        /// <summary>
        ///     Maximum weighted sum across all contacts
        /// </summary>
        public static float MaxWeightedSum { get; set; } = 0;

        public int ContactId { get; set; }
        public float WeightedSum { get; set; }

        public float NormalizedScore
        {
            get
            {
                var result = (WeightedSum - MinWeightedSum) * (100 / (MaxWeightedSum - MinWeightedSum));

                // When the min/max difference is too small to provide meaningful comparative results,
                // or there is only a single contact (meaning the min/max delta will be zero), we can't
                // provide meaningful results; halt
                if (float.IsNaN(result) || float.IsInfinity(result))
                {
                    throw new ApplicationException("A meaningful normalized score can't be determined. Is only one contact in the list? Is the delta between the min and max scores too small?");
                }

                return (int)Math.Round(result, MidpointRounding.AwayFromZero);
            }
        }

        public string QuartileLabel
        {
            get
            {
                if (NormalizedScore < 25) return "bronze";
                if (NormalizedScore < 50) return "silver";
                if (NormalizedScore < 75) return "gold";
                return "platinum";
            }
        }
    }
}