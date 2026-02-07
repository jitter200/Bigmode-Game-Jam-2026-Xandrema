using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class UpgradeUI : MonoBehaviour
{
    
    public GameObject panel;
    public TMP_Text titleText;

    public Button btn1;
    public Button btn2;
    public Button btn3;

    public TMP_Text txt1;
    public TMP_Text txt2;
    public TMP_Text txt3;

    private bool _chosen;
    private Action _applyChosen;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);

        btn1.onClick.AddListener(() => Choose(0));
        btn2.onClick.AddListener(() => Choose(1));
        btn3.onClick.AddListener(() => Choose(2));
    }

    private List<Action> _applies = new List<Action>();

    public void Show(List<string> labels, List<Action> applies)
    {
        _chosen = false;
        _applyChosen = null;
        _applies = applies;

        if (titleText != null) titleText.text = "CHOOSE UPGRADE";

        txt1.text = labels[0];
        txt2.text = labels[1];
        txt3.text = labels[2];

        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Choose(int index)
    {
        if (_chosen) return;
        _chosen = true;

        _applyChosen = _applies[index];
        _applyChosen?.Invoke();

        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    
    public bool Chosen => _chosen;
}
