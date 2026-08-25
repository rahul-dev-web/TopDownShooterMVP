/// <summary> 
/// HealthDisplay - Displays player health bar and text 
/// 
///  Subscribes to Health events 
///  Updates bar and text in real-time /// 
/// </summary> 
using UnityEngine; 
using UnityEngine.UI; 
using TMPro; 

public class HealthDisplay : MonoBehaviour 
{
     [SerializeField] private Image healthBar; 
     [SerializeField] private TextMeshProUGUI healthText; 
     [SerializeField] private Color fullHealthColor = Color.green; 
     [SerializeField] private Color lowHealthColor = Color.red; 
     
     private void Start() { 
        if (healthBar == null || healthText == null) 
        {
             Debug.LogError("[HealthDisplay] Bar or Text not assigned!"); 
             return; 
             } 
             Health.OnHealthChanged += UpdateDisplay; 
             Debug.Log("[HealthDisplay] Initialized"); 
             } 
             private void UpdateDisplay(float currentHealth, float maxHealth) { 
                if (healthBar == null || healthText == null) 
                return; 

                // Update bar 
                float healthPercent = currentHealth / maxHealth; 
                healthBar.fillAmount = healthPercent; 
                
                // Color based on health 
                healthBar.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent); 
                
                // Update text 
                healthText.text = $"{currentHealth:F0}/{maxHealth:F0}"; 
                } 
                private void OnDestroy() {
                     Health.OnHealthChanged -= UpdateDisplay; 
                     } 
}