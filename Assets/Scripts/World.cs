using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//=============================================================================
// Singleton que almacena un array con todos os lugares de agocho no entorno do xogo
// Proporciona acceso global aos puntos onde os axentes poden esconderse
//=============================================================================
public sealed class World
{
    private static readonly World instance = new World(); //instancia única do singleton
    private static GameObject[] hidingSpots; //array cos obxectos de agocho do entorno

    //=============================================================================
    // constrúe o singleton e inicializa o array de lugares de agocho
    //=============================================================================
    static World()
    {
        //encher o array de lugares de agocho cos obxectos do entorno
        //que coincidan coa etiqueta "hide"
        hidingSpots = GameObject.FindGameObjectsWithTag("hide");
    }

    //=============================================================================
    // construtor privado para previr instanciación externa
    //=============================================================================
    private World() { }

    //=============================================================================
    // propiedade para acceder á instancia única do singleton
    //=============================================================================
    public static World Instance
    {
        get { return instance; }
    }

    //=============================================================================
    // devolve o array con todos os lugares de agocho dispoñibles no entorno
    //=============================================================================
    public GameObject[] GetHidingSpots()
    {
        return hidingSpots;
    }
}
