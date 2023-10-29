using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
    Animator anim;
    public float speed;
    public float jumpHeight;
    public Transform groundCheck;
    bool isGrounded;
    int curHp;
    int maxHp = 3;
    bool isHit = false;
    public Main main;
    public bool key = false;
    bool canTP = true;
    public bool inWater = false;
    bool isClimbing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        curHp = maxHp;
    }

    void Update()
    {
        if (inWater && !isClimbing)
        {
            anim.SetInteger("State", 4);
            //isGrounded = true;
            if (Input.GetAxis("Horizontal") != 0)
                Flip();
        }
        else
        {
            //jump
            CheckGround();
            if (Input.GetAxis("Horizontal") == 0 && (isGrounded) && !isClimbing)
            {
                anim.SetInteger("State", 1);
            }
            else
            {
                Flip();
                if (isGrounded && !isClimbing)
                    anim.SetInteger("State", 2);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
                rb.AddForce(transform.up * jumpHeight/5*3, ForceMode2D.Impulse);
 
    }

    void FixedUpdate()
    { 
        //move
        rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, rb.velocity.y);
    }

    void Flip()
    {
        if (Input.GetAxis("Horizontal") > 0)
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        if (Input.GetAxis("Horizontal") < 0)
            transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    void CheckGround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, 0.2f);
        isGrounded = colliders.Length > 1;

        if(!isGrounded && !isClimbing)
        {
            anim.SetInteger("State", 3);
        }
    }

    public void RecountHp(int deltaHp)
    {
        curHp = curHp + deltaHp;
        if (deltaHp < 0)
        {
            StopCoroutine(OnHit());
            isHit = true;
            StartCoroutine(OnHit());
        }

        print(curHp);
        if(curHp <= 0)
        {
            GetComponent<CapsuleCollider2D>().enabled = false;
            Invoke("Lose", 1.5f);
        }
    }

    IEnumerator OnHit()
    {
        if(isHit)
            GetComponent<SpriteRenderer>().color = new Color(1f, GetComponent<SpriteRenderer>().color.g - 0.04f, GetComponent<SpriteRenderer>().color.b - 0.04f);
        else
            GetComponent<SpriteRenderer>().color = new Color(1f, GetComponent<SpriteRenderer>().color.g + 0.04f, GetComponent<SpriteRenderer>().color.b + 0.04f);
        if (GetComponent<SpriteRenderer>().color.g == 1f)
            StopCoroutine(OnHit());
        if (GetComponent<SpriteRenderer>().color.g <= 0)
            isHit = false;

        yield return new WaitForSeconds(0.02f);
        StartCoroutine(OnHit());
    }

    void Lose()
    {
        main.GetComponent<Main>().Lose();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Key")
        {
            Destroy(collision.gameObject);
            key = true; 
        }

        if(collision.gameObject.tag == "Door")
        {
            if (collision.gameObject.GetComponent<Door>().isOpen && canTP)
            {
                collision.gameObject.GetComponent<Door>().Teleport(gameObject);
                canTP = false;
                StartCoroutine(TPwait());
            }
            else if (key)
                collision.gameObject.GetComponent<Door>().Unlock();  
        }
    }

    IEnumerator TPwait()
    {
        yield return new WaitForSeconds(1f);
        canTP = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Ladder")
        {
            isClimbing = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            if (Input.GetAxis("Vertical") == 0)
            {
                anim.SetInteger("State", 5);
            }
            else
            {
                anim.SetInteger("State", 6);
                transform.Translate(Vector3.up * Input.GetAxis("Vertical") * speed * 0.5f * Time.deltaTime);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ladder")
        {
            isClimbing = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
    public class FlagController : MonoBehaviour
    {
        private bool levelCompleted = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && !levelCompleted)
            {
                levelCompleted = true;
                Debug.Log("Уровень завершен");
            }
        }
    }
}
