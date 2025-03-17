using UnityEngine;

public class CalculatePearlsManager : MonoBehaviour
{
    //Prob could be a static, not MonoBehaviour




    public int GetCalculatePearls(bool win)
    {
        //Calculate pearls, change latter to calculate when game ended in other method, this one will just return the value calculated

        if (win)
            return Random.Range(10, 100); //DEBUG
        else
            return Random.Range(-10, -100); //DEBUG
    }
}
