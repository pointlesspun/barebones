namespace BareBones.Services.ObjectPool
{
    public enum PoolIdEnum
    {
        Players = 0,
        PlayerBullets = 1,
        EnemyDrones = 2,
        Other = 3,
        // Id will be mapped to the first available slot after
        // the AutoIndex value
        AutoIndex = 4,
    }
}