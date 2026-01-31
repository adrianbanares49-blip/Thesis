using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterTile : MonoBehaviour
{
    [System.Serializable] //enable to Serialize Board
    public class State
    {
        public Color fillColor;
        public Color outlineColor;
    }
    private TextMeshProUGUI text;
    private UnityEngine.UI.Image fill;
    private Outline outline;

    public State state { get; private set; }
    public char letter { get; private set; }

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        fill = GetComponent<UnityEngine.UI.Image>();
        outline = GetComponent<Outline>();
    }

    public void SetLetter(char letter)
    {
        this.letter = letter;
        text.text = letter.ToString().ToUpper();
    }
    public void SetState(State state)
    {
        this.state = state;
        fill.color = state.fillColor;
        outline.effectColor = state.outlineColor;
    }
}
