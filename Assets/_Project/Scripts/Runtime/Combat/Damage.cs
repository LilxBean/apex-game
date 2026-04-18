using APEX.Tags;

namespace APEX.Combat
{
    /// <summary>
    /// Immutable damage payload passed through the hit pipeline.
    /// </summary>
    public readonly struct Damage
    {
        public readonly float Amount;
        public readonly TagSet Tags;
        public readonly object Source;

        public Damage(float amount, TagSet tags, object source = null)
        {
            Amount = amount;
            Tags = tags;
            Source = source;
        }
    }
}
