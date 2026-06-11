namespace Amane.Echo
{
    // BattleController が未言界バトルか否かを知るための共有コンテキスト
    public static class DungeonBattleContext
    {
        public static bool IsInDungeon { get; set; }
        public static bool BattleWon { get; set; }
        public static RoomType CurrentRoomType { get; set; }

        public static void Reset()
        {
            IsInDungeon = false;
            BattleWon = false;
            CurrentRoomType = RoomType.Empty;
        }
    }
}
