namespace BareBones.Services.ObjectPool
{
    /** 
     * Predefined pool ids
     */
    public enum PoolIdEnum
    {
        // some standard pool ids, add or remove as needed
        Players = 0,
        PlayerBullets = 1,
        Enemies = 2,
        EnemyBullets = 3,
        // Id will be mapped to the first available slot after
        // the AutoIndex value
        AutoIndex = 4,
    }
}