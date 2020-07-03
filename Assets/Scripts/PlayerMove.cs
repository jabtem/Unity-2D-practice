using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    //other Script
    public GameManager gameManager;

    //Componet
    Rigidbody2D rigid;
    SpriteRenderer spr;
    CapsuleCollider2D CapCollider;
    Animator anim;
    AudioSource audioSource;

    //Parameter
    public float maxSpeed;//최대속도
    public float jumpPower;
    public int jumpCount;//최대 점프횟수제한
    int firstJumpCount;
    public Vector2 startPos;

    //Sound
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    //Mobile Key var
    public float L_Value;
    public float R_Value;
    public float J_Value;

    public bool L_Down;
    public bool L_Up;
    public bool R_Down;
    public bool R_Up;
    public bool J_Down;


    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        CapCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();

        startPos = this.gameObject.transform.position;//캐릭터의시작위치를 저장
        firstJumpCount = jumpCount;//최대 점프횟수를 저장
    }



    void Update()
    {
        //Jump
        if ((Input.GetButtonDown("Jump") || J_Down) &&jumpCount>0)
        {
            J_Down = false;
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount--;
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }
        
        //Stop Speed 버튼에서 손땔떼
        if (Input.GetButtonUp("Horizontal")|| L_Up || R_Up){
            if (L_Up)
                L_Up = false;
            else if (R_Up)
                R_Up = false;

            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);//손떼면감속
            //rigid.velocity.normalized.x * 0.5f

        }

        //Sprite Direction 스프라이트 방향전환
        if (Input.GetButton("Horizontal") || L_Down || R_Down)
            //PC
            spr.flipX = Input.GetAxisRaw("Horizontal") == -1;

            //Mobile
            if (L_Down)
                spr.flipX = true;
            else if(R_Down)
                spr.flipX = false;

        //애니메이션 전환
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);

    }
    void FixedUpdate()
    {
        //Move Speed
        //PC +Mobile
        float h = Input.GetAxisRaw("Horizontal") + (R_Value + L_Value)*maxSpeed;//좌우로 키보드입력인식
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);//누르는 지속만큼 힘증가


        if (rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);//Right Maxspeed
        else if (rigid.velocity.x < maxSpeed * (-1))
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);//Left Maxspeed

        //Lading Platform
        if (rigid.velocity.y < 0)//캐릭터가 떨어지고있을때
        {
            Debug.DrawRay(rigid.position, Vector3.down *0.5f, new Color(1, 0, 0));
            RaycastHit2D rayhit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

            if (rayhit.collider != null)
            {//레이캐스트와 충돌판정이 있는경우
                if (rayhit.distance < 0.5f)
                    anim.SetBool("isJumping", false);
                    jumpCount = firstJumpCount;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.tag == "Enemy")
        {
            //Attack
            if(rigid.velocity.y<0 && transform.position.y > collision.transform.position.y)
            {
                if (collision.gameObject.layer == 9)// layer 9 -> Eenemy
                {
                    OnAttack(collision.transform);
                }
                else
                    OnDamaged(collision.transform.position);

            }
            else
                OnDamaged(collision.transform.position);
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            //Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if(isBronze)
                gameManager.stagePoint +=50;

            else if(isSilver)
                gameManager.stagePoint += 100;

            else if (isGold)
                gameManager.stagePoint += 300;

            //Deactive Item
            collision.gameObject.SetActive(false);

            //SOUND
            PlaySound("ITEM");
        }
        else if(collision.gameObject.tag == "Finish")
        {
            //SOUND
            PlaySound("FINISH");

            //next Stage
            gameManager.NextStage();
        }
    }
    
    void OnAttack(Transform enemy)
    {
        //SOUND
        PlaySound("ATTACK");

        //Point
        gameManager.stagePoint += 100;

        //Reaction
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        //Eenmy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.Ondamaged();
    }

    void OnDamaged(Vector2 targetPos)
    {
        //SOUND
        PlaySound("DAMAGED");

        //Change Layer(immortal Active)
        gameObject.layer = 11; // layer -> playerdamaged

        //Minus Point
        if (gameManager.stagePoint - 100 < 0)
            gameManager.stagePoint = 0;
        else
            gameManager.stagePoint -= 100;

        //Minus Life
        gameManager.lifeDown();


        if (gameManager.life == 0)
            OnDie();

        spr.color = new Color(1, 1, 1, 0.4f); // R G B 투명도

        //Reaction
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1)*7, ForceMode2D.Impulse);
        anim.SetTrigger("doDamaged");
        Invoke("OffDamaged", 1);
    }

    void OffDamaged()
    {
        gameObject.layer = 10;//Layer -> player
        spr.color = new Color(1f, 1f, 1f, 1); // R G B 투명도
    }

    public void OnDie()
    {
        //SOUND
        PlaySound("DIE");

        //Sprite Color Change
        spr.color = new Color(1, 1, 1, 0.4f);

        //Sprite Flip
        spr.flipY = true;

        //Collider Disable
        CapCollider.enabled = false;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 20, ForceMode2D.Impulse);

        //Invoke("OnReposition", 2);

    }

    //Revive
    /*
    void OnReposition()
    {


        OffDamaged();
        //Sprite Flip
        spr.flipY = false;

        //Collider Disable
        CapCollider.enabled = true;

        rigid.velocity = Vector2.zero;
        transform.position = new Vector3(startPos.x,startPos.y,0);
        gameManager.life = 3;

    }
    */

    //스테이지 변경시 시작위치로 이동 
    public void OnStageChange()
    {
        rigid.velocity = Vector2.zero;
        transform.position = new Vector3(startPos.x, startPos.y, 0);
        //gameManager.life = 3;
    }

    public void PlaySound(string action)
    {
        if (action == "JUMP")
        {
            audioSource.clip = audioJump;
        }
        else if (action == "ATTACK")
        {
            audioSource.clip = audioAttack;
        }
        else if (action == "DAMAGED")
        {
            audioSource.clip = audioDamaged;
        }
        else if (action == "ITEM")
        {
            audioSource.clip = audioItem;
        }
        else if (action == "DIE")
        {
            audioSource.clip = audioDie;
        }
        else if (action == "FINISH")
        {
            audioSource.clip = audioFinish;
        }
        audioSource.Play();//오디오실행
    }

    //Mobile Button Control
    public void ButtonDown(string type)
    {
        switch (type)
        {
            case "L":
                L_Value = -1;
                L_Down = true;
                L_Up = false;
                break;
            case "R":
                R_Value = 1;
                R_Down = true;
                R_Up = false;
                break;
            case "J":
                J_Down = true;
                break;
        }

    }
    public void ButtonUp(string type)
    {
        switch (type)
        {
            case "L":
                L_Value = 0;
                L_Up = true;
                L_Down = false;
                break;
            case "R":
                R_Value = 0;
                R_Up = true;
                R_Down = false;
                break;
        }
    }
}
