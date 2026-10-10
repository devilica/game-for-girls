using System.Collections.Generic;

namespace DressUpGame.MysteryBox
{
    public enum MysteryOpenStatus
    {
        Success,
        AllUnlocked,
        Failed
    }

    public readonly struct MysteryOpenResult
    {
        public MysteryOpenStatus Status { get; }
        public IReadOnlyList<MysteryLootEntry> Rewards { get; }
        public string Message { get; }

        public MysteryOpenResult(MysteryOpenStatus status, IReadOnlyList<MysteryLootEntry> rewards, string message)
        {
            Status = status;
            Rewards = rewards ?? System.Array.Empty<MysteryLootEntry>();
            Message = message ?? string.Empty;
        }

        public static MysteryOpenResult Success(IReadOnlyList<MysteryLootEntry> rewards) =>
            new MysteryOpenResult(MysteryOpenStatus.Success, rewards, string.Empty);

        public static MysteryOpenResult AllUnlocked(string message) =>
            new MysteryOpenResult(MysteryOpenStatus.AllUnlocked, null, message);

        public static MysteryOpenResult Failed(string message) =>
            new MysteryOpenResult(MysteryOpenStatus.Failed, null, message);
    }
}
