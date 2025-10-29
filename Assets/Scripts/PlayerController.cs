using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//=============================================================================
// Controla o movemento dun obxecto mediante entrada do usuario
// Permite mover o obxecto cara adiante/atrás e rotalo á esquerda/dereita
//=============================================================================
public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f; //a velocidade base á que o obxecto se move cara adiante ou cara atrás
    public float rotationSpeed = 100.0f; //a velocidade base á que o obxecto rota á esquerda ou á dereita
    public float currentSpeed = 0; //o desprazamento real aplicado neste frame (accesible por outros scripts)

    //=============================================================================
    //actualiza o movemento e rotación do obxecto cada frame en base á entrada do usuario
    //=============================================================================
    void Update()
    {
        //obter o eixe horizontal e vertical da entrada do usuario
        //por defecto están mapeados ás teclas de frecha
        //o valor está no rango -1 a 1
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;

        //facer que se mova por segundo en lugar de por frame multiplicando por Time.deltaTime
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;

        transform.Translate(0, 0, translation); //mover a translación ao longo do eixe z do obxecto (adiante/atrás)
        currentSpeed = translation; //gardar a velocidade actual para que outros scripts poidan acceder

        transform.Rotate(0, rotation, 0); //rotar ao redor do eixe y do obxecto (esquerda/dereita)
    }
}
