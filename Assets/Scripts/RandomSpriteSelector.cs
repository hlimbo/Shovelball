using UnityEngine;
using System.Collections;

public class RandomSpriteSelector : MonoBehaviour {

    public int numOfSprites;
    public Sprite[] sprites;

    private SpriteRenderer render;

    void Awake()
    {
        render = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Debug.Log("hello");
        SetRandomSprite();
    }

    private void SetRandomSprite()
    {
        int spriteIndex = Random.Range(0, sprites.Length);
        render.sprite = sprites[spriteIndex];
        Debug.Log(spriteIndex);
    }

    void OnEnable()
    {
        SetRandomSprite();
    }


    
}
