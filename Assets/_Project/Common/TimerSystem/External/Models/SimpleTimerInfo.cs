namespace Common.TimerSystem.External.Models
{
    public class SimpleTimerInfo
    {
        public float Duration { get; set; }
        public float Time { get; set; }
        public float TimeLeft { get; set; }
        public float Delta { get; set; }
        
        public float Progress 
        { 
            get { return Duration > 0 ? Time / Duration : 1f; } 
        }
        
        public float ProgressPercent 
        { 
            get { return Progress * 100f; } 
        }
        
        public bool IsCompleted 
        { 
            get { return TimeLeft <= 0f; } 
        }
        
        public bool IsMoreThanHalfway 
        { 
            get { return Progress > 0.5f; } 
        }
    }
}