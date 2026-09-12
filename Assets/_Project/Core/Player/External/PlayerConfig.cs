namespace Core.Player.External
{
    public class PlayerConfig
    {
        public float MaxFallGravity { get; set; } = -35f;
        public float GroundedGravity = -0.6f;
        public float MoveSpeed = 6f;
        public float MaxJumpHeight = 3.5f;
        public float MaxJumpTime = 1f;
        public float Gravity { get; set; } = -20;
        public float JumpGravity { get; set; } = -9.8f;
    }
}