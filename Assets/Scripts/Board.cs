using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS =
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E,
        KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
        KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O,
        KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
        KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y,
        KeyCode.Z
    };

    private BoardRow[] rows;

    private string[] solutions;
    private string[] validWords;

    private string word;
    private int rowIndex;
    private int columnIndex;
    private bool lastSubmissionInvalid;
    private bool wordFound = false;


    [Header("States")]
    public LetterTile.State emptyState;
    public LetterTile.State occupiedState;
    public LetterTile.State correctState;
    public LetterTile.State wrongState;
    public LetterTile.State incorrectState;


    [Header("UI")]
    public Image invalidWordImage;
    public TextMeshProUGUI invalidWordText;
    public Image wordFoundTextImage;
    public TextMeshProUGUI wordFoundText;
    public GameObject retryButton;


    private void Awake()
    {
        rows = GetComponentsInChildren<BoardRow>();
    }

    private void Start()
    {
        LoadData();

        if (solutions == null || solutions.Length == 0)
        {
            Debug.LogError("Solutions not loaded!");
            enabled = false;
            return;
        }
        invalidWordImage.gameObject.SetActive(false);
        invalidWordText.gameObject.SetActive(false); //  hide on game start

        wordFoundTextImage.gameObject.SetActive(false);
        wordFoundText.gameObject.SetActive(false);
        retryButton.SetActive(false);
        lastSubmissionInvalid = false;


        SetRandomWord();
    }

    private void LoadData()
    {
        TextAsset allWords = Resources.Load<TextAsset>("official_wordle_all");
        TextAsset commonWords = Resources.Load<TextAsset>("official_wordle_common");

        if (allWords == null || commonWords == null)
        {
            Debug.LogError("Word list files not found in Resources folder!");
            return;
        }

        validWords = allWords.text
        .ToLower()
        .Split('\n');
        solutions = commonWords.text
        .ToLower()
        .Split('\n');

    }

    private void SetRandomWord()
    {
        word = solutions[UnityEngine.Random.Range(0, solutions.Length)]
            .ToLower()
            .Trim();
        //convert to small cases for comparison
    }

    public void RetryGame()
    {
        rowIndex = 0;
        columnIndex = 0;
        wordFound = false;
        lastSubmissionInvalid = false;

        //hide text/
        invalidWordImage.gameObject.SetActive(false);
        invalidWordText.gameObject.SetActive(false);


        wordFoundTextImage.gameObject.SetActive(false);
        wordFoundText.gameObject.SetActive(false);
        retryButton.SetActive(false);

        foreach (BoardRow row in rows)
        {
            foreach (LetterTile tile in row.tiles)
            {
                tile.SetLetter('\0');
                tile.SetState(emptyState);
            }
        }

        SetRandomWord();
        enabled = true;
    }

    private void Update()
    {
        //stop input if the correct word is already inputted
        if (wordFound)
            return;

        if (rowIndex >= rows.Length)
            return;

        BoardRow currentRow = rows[rowIndex];

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            //backspacing
            if (columnIndex > 0)
            {
                columnIndex--;
                currentRow.tiles[columnIndex].SetLetter('\0');
                currentRow.tiles[columnIndex].SetState(emptyState);

                if (lastSubmissionInvalid)
                {
                    invalidWordImage.gameObject.SetActive(false);
                    invalidWordText.gameObject.SetActive(false);
                    lastSubmissionInvalid = false;
                }
            }
            return;
        }

        if (columnIndex >= currentRow.tiles.Length)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SubmitRow(currentRow);
            }
            return;
        }

        for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
        {
            if (Input.GetKeyDown(SUPPORTED_KEYS[i]))
            {

                currentRow.tiles[columnIndex]
                    .SetLetter(SUPPORTED_KEYS[i].ToString()[0]);
                currentRow.tiles[columnIndex]
                    .SetState(occupiedState);

                columnIndex++;
                break;
            }
        }
    }

    private void SubmitRow(BoardRow row)
    {
        // Clear previous error ONLY when Enter is pressed
        invalidWordImage.gameObject.SetActive(false);
        invalidWordText.gameObject.SetActive(false);
        lastSubmissionInvalid = false;

        if (!IsValidWord(row.word))
        {
            invalidWordImage.gameObject.SetActive(true);
            invalidWordText.gameObject.SetActive(true);
            lastSubmissionInvalid = true;
            return;
        }

        string remaining = word;

        for (int i = 0; i < row.tiles.Length; i++)
        {
            LetterTile tile = row.tiles[i];
            char letter = char.ToLower(tile.letter);

            if (letter == word[i])
            {
                tile.SetState(correctState);
                remaining = remaining.Remove(i, 1).Insert(i, " ");
            }
        }

        for (int i = 0; i < row.tiles.Length; i++)
        {
            LetterTile tile = row.tiles[i];
            char letter = char.ToLower(tile.letter);

            if (tile.state == correctState)
                continue;

            if (remaining.Contains(letter))
            {
                tile.SetState(wrongState);
                int index = remaining.IndexOf(letter);
                remaining = remaining.Remove(index, 1).Insert(index, " ");
            }
            else
            {
                tile.SetState(incorrectState);
            }
        }


        /* cannot check multiple same letters 
                => cause confusion to players

        for (int i = 0; i < row.tiles.Length; i++)
        {
            LetterTile tile = row.tiles[i];
            char letter = char.ToLower(tile.letter);

            if (letter == word[i])
            {
                tile.SetState(correctState); 
            }
            else if (word.Contains(letter))
            {
                tile.SetState(wrongState);     
            }
            else
            {
                tile.SetState(incorrectState); 
            }
        }
        */

        if (row.word == word)
        {
            //checking if the word is correect
            wordFound = true;
            wordFoundTextImage.gameObject.SetActive(true);
            wordFoundText.gameObject.SetActive(true);
            enabled = false;
        }

        rowIndex++;
        columnIndex = 0;

        //disable next row input when invalid
        if (rowIndex >= rows.Length)
        {
            enabled = false;
            retryButton.SetActive(true);
        }
    }
    //check if the word is valid
    private bool IsValidWord(string word)
    {
        for (int i = 0; i < validWords.Length; i++)
        {
            if (validWords[i].Trim() == word)
            {
                return true;
            }
        }
        return false;
    }


}
