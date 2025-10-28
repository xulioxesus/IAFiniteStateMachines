using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//=========================================================================
// Clase AI (Intelixencia Artificial)
// Controlador principal da máquina de estados finitos dun NPC.
// Xestiona a transición entre estados e actualiza o comportamento cada frame.
//=========================================================================
public class AI : MonoBehaviour {

    NavMeshAgent agent;         // Axente de navegación do NPC
    Animator animator;              // Controlador de animacións do NPC
    State currentState;         // Estado actual da máquina de estados

    public Transform player;    // Referencia á transformada do xogador

    //=========================================================================
    // Método Start - Inicialización do compoñente
    // Chámase unha vez ao iniciar o GameObject
    //=========================================================================
    void Start() {

        agent = GetComponent<NavMeshAgent>();       // Obtén o compoñente NavMeshAgent do GameObject
        animator = GetComponent<Animator>();            // Obtén o compoñente Animator do GameObject
        currentState = new Idle(gameObject, agent, animator, player);   // Inicializa no estado Idle
    }

    //=========================================================================
    // Método Update - Actualización cada frame
    // Procesa o estado actual e actualiza a transicións entre estados
    //=========================================================================
    void Update() {

        // Procesa o estado actual e actualiza ao novo estado se hai transición
        currentState = currentState.Process();
    }
}
