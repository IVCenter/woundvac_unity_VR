

using UnityEngine;
using System.Collections;

public class SixenseHand : MonoBehaviour
{

    Animator 	m_animator;
	float 		m_fLastTriggerVal;
	Vector3		m_initialPosition;
	Quaternion 	m_initialRotation;


	protected void Start() 
	{
		// get the Animator
		m_animator = gameObject.GetComponent<Animator>();
		m_initialRotation = transform.localRotation;
		m_initialPosition = transform.localPosition;
	}


	protected void Update()
	{

		if ( m_animator != null )
		{
		}

        // Test
        if (Input.GetKeyDown(KeyCode.A))
        {
            m_animator.SetBool("Idle", true); 
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            m_animator.SetBool("Idle", false);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            m_animator.SetBool("GripBall", true);
        }
        else if(Input.GetKeyUp(KeyCode.B))
        {
            m_animator.SetBool("GripBall", false);
        }

    }

    // Updates the animated object from controller input.
 //   protected void UpdateHandAnimation()
	//{
 //       //Point on Bumper press: only functions if Point SetBool code lines are commented out
 //       /*if (m_hand == SixenseHands.RIGHT ? m_controller.GetButton(SixenseButtons.BUMPER) : m_controller.GetButton(SixenseButtons.BUMPER))
 //       {
 //           Debug.Log("Bumper");
 //           m_animator.SetBool("Point", true);
 //       }
 //       else
 //       {
 //           m_animator.SetBool("Point", false);
 //       } */
    
 //       // Point: If button one is pressed on the left controller, or the second button on the right controller, point
 //       if ( m_hand == SixenseHands.RIGHT ? m_controller.GetButton(SixenseButtons.ONE) : m_controller.GetButton(SixenseButtons.TWO) )
	//	{
	//		m_animator.SetBool("Point", true );
 //           Debug.Log("Pointing");
	//	}
	//	else
	//	{
	//		m_animator.SetBool("Point", false );
	//	} 
		

	//	// Grip Ball: second button on left controller, first button on right controller
	//	if ( m_hand == SixenseHands.RIGHT ? m_controller.GetButton(SixenseButtons.TWO) : m_controller.GetButton(SixenseButtons.ONE)  )
	//	{
 //           m_animator.SetBool( "GripBall", true );
	//	}
	//	else
	//	{
 //           m_animator.SetBool( "GripBall", false );
 //       }
				
	//	/* Hold Book: third button on left controller, fourth button on right controller
	//	if ( m_hand == SixenseHands.RIGHT ? m_controller.GetButton(SixenseButtons.THREE) : m_controller.GetButton(SixenseButtons.FOUR) )
	//	{
	//		m_animator.SetBool("HoldBook", true );
 //           Debug.Log("Holding book");
 //       }
	//	else
	//	{
	//		m_animator.SetBool("HoldBook", false );
	//	} */

 //       // Fist
 //       float fTriggerVal = m_controller.Trigger;
	//	fTriggerVal = Mathf.Lerp( m_fLastTriggerVal, fTriggerVal, 0.1f );
	//	m_fLastTriggerVal = fTriggerVal;
		
	//	if ( fTriggerVal > 0.01f )
	//	{
 //           m_animator.SetBool("Fist", true);
 //       }
	//	else
	//	{
	//		m_animator.SetBool("Fist", false );
	//	}
		
	//	m_animator.SetFloat("FistAmount", fTriggerVal);
		
	//	// Idle
	//	if ( m_animator.GetBool("Fist") == false &&  
	//		 m_animator.GetBool("HoldBook") == false && 
	//		 m_animator.GetBool("GripBall") == false && 
	//		 m_animator.GetBool("Point") == false )
	//	{
	//		m_animator.SetBool("Idle", true);
	//	}
	//	else
	//	{
	//		m_animator.SetBool("Idle", false);
	//	}
	//}


	public Quaternion InitialRotation
	{
		get { return m_initialRotation; }
	}
	
	public Vector3 InitialPosition
	{
		get { return m_initialPosition; }
	}
}

