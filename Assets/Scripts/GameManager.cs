using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int life;
    public PlayerMove player;
    public GameObject[] Stages;

    //UI
    public Image[] UILife;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;
    

    // Start is called before the first frame update
    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }



    public void NextStage()
    {
        //Stage Change
        if(stageIndex < Stages.Length-1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            player.OnStageChange();

            UIStage.text = "STAGE" + (stageIndex+1);
        }
        else//Game Clear
        {
            //Player Control Lock
            Time.timeScale = 0;
            //Result
            Debug.Log("게임 클리어");

            //Restart Button UI
            Text btnText = UIRestartBtn.GetComponentInChildren<Text>();
            btnText.text = "Clear!";
            UIRestartBtn.SetActive(true);
        }

        totalPoint += stagePoint;
        stagePoint = 0;
    }
    public void lifeDown()
    {
        if(life > 1)
        {
            life--;
            UILife[life].color = new Color(1, 0, 0, 0.4f);
        }
        else
        {
            //All Life UI Off
            UILife[0].color = new Color(1, 0, 0, 0.4f);

            //Plyaer Die
            player.OnDie();

            //Result UI

            //Retry
            UIRestartBtn.SetActive(true);
            Invoke("Puase", 1);
        }



    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {


            //Player Reposition
            if (life > 1)
            {
                collision.attachedRigidbody.velocity = Vector2.zero;
                collision.transform.position = new Vector3(player.startPos.x, player.startPos.y, -1);
                player.PlaySound("DAMAGED");
            }

            lifeDown();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }

    void Puase()
    {
        Time.timeScale = 0;
    }

}
