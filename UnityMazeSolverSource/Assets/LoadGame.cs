using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGame : MonoBehaviour
{


    public GameObject widthField;
    public GameObject heightField;

    [SerializeField] private SizeData size;

    public void Load()
    {


        if (Check(widthField) && Check(heightField))
        {
            SetHeigth();
            SetWidth();
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);

        }
    }


    public bool Check(GameObject g)
    {
        string s = g.GetComponent<InputField>().text;
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
        if (sucess && x > 0 && x % 2 == 0 && x <= 2000)
        {
            g.GetComponent<Image>().color = Color.green;
            return true;

        }
        else
        {
            g.GetComponent<Image>().color = Color.red;
            return false;

        }
    }

    public void SetHeigth()
    {
        string s = heightField.GetComponent<InputField>().text;
        if (Check(heightField))
            size.height = int.Parse(s);
    }
    public void SetWidth()
    {
        string s = widthField.GetComponent<InputField>().text;
        if (Check(widthField))
            size.width = int.Parse(s);
    }
}
