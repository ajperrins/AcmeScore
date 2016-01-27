using System;
using System.Collections.Generic;
using System.Configuration;

namespace AcmeScore
{
    /// <summary>
    ///     Represents a score for an event, providing both the raw event score, and its value weighted based on the event type
    /// </summary>
    public class ScoreRecord
    {
        // Score weighting has defaults in code to allow running absent a configuration file, but their values can be 
        // updated in appSettings configuration without recompilation
        private static readonly Dictionary<Event, float> EventWeights = new Dictionary<Event, float>
        {
            {Event.Web, float.Parse(ConfigurationManager.AppSettings[$"Weight{Event.Web}"] ?? "1")},
            {Event.Email, float.Parse(ConfigurationManager.AppSettings[$"Weight{Event.Email}"] ?? "1.2")},
            {Event.Social, float.Parse(ConfigurationManager.AppSettings[$"Weight{Event.Social}"] ?? "1.5")},
            {Event.Webinar, float.Parse(ConfigurationManager.AppSettings[$"Weight{Event.Webinar}"] ?? "2")}
        };

        public int ContactId { get; set; }
        public Event Event { get; set; }
        public float Score { get; set; }
        public float WeightedScore => EventWeights[Event]*Score;

        public override string ToString()
        {
            return $"[{GetType().Name}] {nameof(ContactId)}={ContactId}, {nameof(Event)}={Event}, {nameof(Score)}={Score}, {nameof(WeightedScore)}={WeightedScore}";
        }

        /// <summary>
        ///     Returns an instance, given a comma-separated string where the fields represent,
        ///     in order, the contact ID, the event, and the score. Returns null if the argument
        ///     could not be parsed
        /// </summary>
        /// <param name="line">A comma separated string</param>
        /// <returns>An instnace, or null if the provided string could not be interpreted</returns>
        public static ScoreRecord Parse(string line)
        {
            ScoreRecord result = null;
            var parts = line?.Split(',');

            if (parts?.Length >= 3)
            {
                int contactId;
                Event channel;
                float score;

                if (int.TryParse(parts[0], out contactId)
                    && Enum.TryParse(parts[1], true, out channel)
                    && float.TryParse(parts[2], out score))
                {
                    result = new ScoreRecord
                    {
                        ContactId = contactId,
                        Event = channel,
                        Score = score
                    };
                }
            }
            return result;
        }
    }
}