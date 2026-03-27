using System.Collections;
using UnityEditor.Callbacks;
using UnityEngine;

public class BrokeTiles : MonoBehaviour
{
    [SerializeField] private GameObject tile;
    private Sprite currentile;
    private Rigidbody2D rb;
    private Collider2D collider;
    [SerializeField] private Sprite broken_one;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        StartCoroutine(startBroke());
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }
    public IEnumerator startBroke()
    {
        yield return new WaitForSeconds(5);
        tile.GetComponent<SpriteRenderer>().sprite = broken_one;
        yield return new WaitForSeconds(5);

        Destroy(tile);

    }
}
