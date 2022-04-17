using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    public Questions[] questions;
    private static List<Questions> unansweredQuestion;
    public Questions currentQuestion;
    public int Score = 0;
    public int click =0;
    public GameObject button;

    [SerializeField]
    private Text factText;
    
    [SerializeField]
    private Text ScoreText;
    void Start()
    {
         if(unansweredQuestion==null || unansweredQuestion.Count==0){
            unansweredQuestion=questions.ToList<Questions>();
        }
        RandomQuestion();
        
    }

    public void RandomQuestion(){
        int randomQuestionIndex = Random.Range(0, unansweredQuestion.Count);
        currentQuestion = unansweredQuestion[randomQuestionIndex];
        factText.text = currentQuestion.fact;
        print(currentQuestion.fact+currentQuestion.isTrue);
        unansweredQuestion.RemoveAt(randomQuestionIndex);
        
    }
   
    
    // Update is called once per frame
    void Update()
    {
            
        ScoreText.text = Score.ToString();
        //factText.text = currentQuestion.fact;
        factText.text = currentQuestion.fact;

    }

    
    public void addScore(){
    
       click++;
        var go = EventSystem.current.currentSelectedGameObject;
        if (go != null){
            if(bool.Parse(go.name)==currentQuestion.isTrue){
                ++Score;
            }
           
         }
     
      
        
        
    }
}


  
