/// <summary>
/// KillDisplay - Displays kill/death/streak counters 
/// 
/// Shows live statistics 
/// Updates on events 
/// </summary> 
using UnityEngine; 
using TMPro; 
public class KillDisplay : MonoBehaviour {
     [SerializeField] private TextMeshProUGUI killsText;
     [SerializeField] private TextMeshProUGUI deathsText; 
     [SerializeField] private TextMeshProUGUI streakText; 
     private void Start() {
         KillCounter.OnKillScored += UpdateKills; 
         KillCounter.OnDeathOccurred += UpdateDeaths; 
         KillCounter.OnKillStreakChanged += UpdateStreak; 
         // Initialize displays 
         UpdateKills(0); 
         UpdateDeaths(0); 
         UpdateStreak(0); 
         Debug.Log("[KillDisplay] Initialized"); 
         } 
         private void UpdateKills(int kills) { 
            if (killsText != null) killsText.text = $"Kills: {kills}"; 
            } 
        private void UpdateDeaths(int deaths) { 
            if (deathsText != null) deathsText.text = $"Deaths: {deaths}"; 
            }
         private void UpdateStreak(int streak) { 
            if (streakText != null) streakText.text = $"Streak: {streak}"; 
            } 
        private void OnDestroy() {
             KillCounter.OnKillScored -= UpdateKills; 
             KillCounter.OnDeathOccurred -= UpdateDeaths; 
             KillCounter.OnKillStreakChanged -= UpdateStreak; 
 } 
}