using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueChecker : MonoBehaviour
{

    // Update is called once per frame
    public void Check()
    {
        string s = this.GetComponent<InputField>().text;
        bool sucess = true;
        int x = 0;

        try
        {
            x = int.Parse(s);
        }
        catch (System.Exception)
        {

            sucess = false;
        }
        if(sucess && x > 0 && x %2== 0 && x <= 500)
        {
            this.GetComponent<Image>().color = Color.green;
            
        }else
            this.GetComponent<Image>().color = Color.red;

    }
}
