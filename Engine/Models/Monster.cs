namespace Engine.Models
{
    public class Monster : LivingEntity
    {
        private int _hitPoints;

        public string ImageName { get; set; }

        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }

        public int RewardExperience { get; private set; }

        public Monster (string name, string imageName,
            int maximumHitPoints, int hitPoints,
            int minimumDamage, int maximumDamage,
            int rewardExperience, int rewardGold)
        {
            Name = name;
            ImageName = $"/Engine;component/Images/Monsters/{imageName}";
            MaximumHitPoints = maximumHitPoints;
            CurrentHitPoints = hitPoints;
            MinimumDamage = minimumDamage;
            MaximumDamage = maximumDamage;
            RewardExperience = rewardExperience;
            Gold = rewardGold;
        }
    }
}
