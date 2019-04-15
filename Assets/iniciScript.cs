using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class iniciScript : MonoBehaviour
{

    public GameObject roberto;
    public GameObject studios;
    public GameObject logo;
    public GameObject canvas;
    public GameObject[] objetos;
    public GameObject mainMenu;

    // Start is called before the first frame update
    void Start()
    {
        objetos = new GameObject[4];
        objetos[0] = roberto;
        objetos[1] = studios;
        objetos[2] = logo;
        objetos[3] = canvas;

        Activar1();
    }

    // Update is called once per frame
    void Activar1()
    {
        objetos[0].SetActive(true);
        Invoke("Activar2", 1f);
    }
    void Activar2()
    {
        objetos[1].SetActive(true);
        Invoke("Activar3", 1f);
    }
    void Activar3()
    {
        objetos[2].SetActive(true);
        Invoke("Desactivar", 1f);
    }
    void Desactivar()
    {
        objetos[3].SetActive(false);
        mainMenu.SetActive(true);
    }

}
