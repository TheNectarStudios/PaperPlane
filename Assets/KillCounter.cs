using UnityEngine;
using TMPro;

public class KillCounter : MonoBehaviour
{
    public TextMeshProUGUI killCountText;
    private int killCount = 0;

    private void Start()
    {
        UpdateKillCountText();
    }

    public void EnemyKilled()
    {
        killCount++;
        UpdateKillCountText();
    }

    private void UpdateKillCountText()
    {
        killCountText.text = "Kills: " + killCount.ToString();
    }
}
