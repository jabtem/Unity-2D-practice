using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public int nextMove;
    public int nowSpeed;

    SpriteRenderer spr;
    Rigidbody2D rigid;
    Animator anim;
    CircleCollider2D CirCollider;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spr = GetComponent<SpriteRenderer>();
        CirCollider = GetComponent<CircleCollider2D>();
        //nextMove = -1;
        Invoke("Think", 3);//3초후 호출
    }

    // Update is called once per frame
    
    void Update()
    {
    }
   
    void FixedUpdate()
    {
        //Move 이동
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);
        //Platform Check 지형체크
        Vector2 frontVector = new Vector2(rigid.position.x + (nextMove*0.5f),rigid.position.y);
        Debug.DrawRay(frontVector, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayhit = Physics2D.Raycast(frontVector, Vector3.down, 1, LayerMask.GetMask("Platform"));
        //anim.SetInteger("WalkSpeed", nextMove);
        if (rayhit.collider == null)
        {//레이캐스트와 충돌판정이 없을경우 -> 낭떠러지의경우
            //nowSpeed = nextMove;//현재의 이동방향을 저장
            //nextMove = 0;
            //Invoke("Turn2", 2);
            Turn();
        }

        
    }
    void Turn2()//낭떠러지에가면 2초 정지후 반대방향으로 이동
    {
        nextMove = nowSpeed;
        nextMove *= -1;
        spr.flipX = nextMove == 1;

    }
    void Turn()
    {
        nextMove *= -1;
        spr.flipX = nextMove == 1;
        CancelInvoke();//작동중인 모든 invoke함수 정지
        Invoke("Think", 2);

    }
    void Think()//3초마다 재귀함수로 다시호출
    { 
        nextMove = Random.Range(-1, 2);//-1~1 사이의 난수형성 최대값은 랜덤범위에 포함x
        
        //-1 좌로이동 , 0 정지, 1 우로이동
        //애니메이션 전환
        anim.SetInteger("WalkSpeed", nextMove);

        //Flip Sprite
        if(nextMove !=0)
            spr.flipX = nextMove == 1;
        
        Invoke("Think", 3);//3초후 Think함수 호출
    }

    public void Ondamaged()
    {

        CancelInvoke();//이동모션 정지
        nextMove = 0;

        //Sprite Color Change
        spr.color = new Color(1,1,1,0.4f);

        //Sprite Flip
        spr.flipY = true;

        //Collider Disable
        CirCollider.enabled = false;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        //Destroy
        Invoke("DeActive", 5);
    }
    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
