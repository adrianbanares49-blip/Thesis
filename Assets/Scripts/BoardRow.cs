using System.Data;
using JetBrains.Annotations;
using UnityEngine;

public class BoardRow : MonoBehaviour
{
    public LetterTile[] tiles { get; private set; }

        public string word{
        get
        {
            string result = "";

            for( int i = 0; i < tiles.Length; i++)
            {
                if(tiles[i].letter == '\0')
                    return "";
                result += char.ToLower(tiles[i].letter);
            }
            return result.Trim();
        }
    }
    private void Awake()
    {
        tiles = GetComponentsInChildren<LetterTile>();
    }
    
}
