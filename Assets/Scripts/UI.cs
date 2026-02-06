using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UI : MonoBehaviour
{
    public Player player;
    public Waves waves;

    public Image portrait;
    public Sprite portraitNormal;
    public Sprite portraitCritical;

    public Image heart;
    public Sprite heartFull;
    public Sprite heartHalf;
    public Sprite heartLow;

    public TMP_Text hpText;
    public TMP_Text waveText;

    public Image windTimer;
    

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<Player>();
        if (waves == null) waves = FindFirstObjectByType<Waves>();
    }

    private void Update()
    {
        
        portrait.sprite = player.Hp01 < 0.3f ? portraitCritical : portraitNormal;

        
        float hp = player.Hp01;
        if (hp > 0.7f) heart.sprite = heartFull;
        else if (hp > 0.4f) heart.sprite = heartHalf;
        else heart.sprite = heartLow;

        
        hpText.text = $"{Mathf.CeilToInt(player.Hp01 * player.maxHp)} / {player.maxHp}";

        
        windTimer.fillAmount = player.Wind01;

        
        waveText.text = $"WAVE {waves.currentWaveIndex + 1}";
    }
}
