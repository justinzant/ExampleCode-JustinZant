//This mediates between the app and the PlayerView.

using System;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace virum.game
{
	public class PlayerMediator : Mediator
	{
		//View
		[Inject]
		public PlayerView view { get; set; }

		//Generic element Injections
		[Inject (GenericElement.GAME_CAMERA)]
		public Camera cam { get; set; }

		//Game Signal injections
		[Inject]
		public GameInputSignal gameInputSignal{ get; set; }

		[Inject]
		public DestroyPlayerSignal destroyPlayerSignal{ get; set; }

		//Much like start but called when mediator is attached
		public override void OnRegister ()
		{
			//Internal View signals
			view.triggerSignal.AddListener( OnTrigger );
			view.collisionSignal.AddListener ( OnCollision );
			view.deathSignal.AddListener( KillPlayer );

			gameInputSignal.AddListener( OnGameInput );

			view.requiresContext = false;
			view.Init ();
		}

		//OnRemove() replaces OnDestroy. Used for clean up.
		public override void OnRemove ()
		{
			view.triggerSignal.RemoveListener( OnTrigger );
			view.collisionSignal.RemoveListener ( OnCollision );
			view.deathSignal.RemoveListener( KillPlayer );

			gameInputSignal.RemoveListener( OnGameInput );
		}

		//Receive a signal from Updating GameInput
		private void OnGameInput(int input)
		{
			//Notify the change ( susch as Mousedown )
			view.SetAction (input);
		
			if ((input & GameInputEvent.FIRE) > 0)
			{
				view.isFiring = true;
				input |= GameInputEvent.FIRE;
			}

			//A MOUSEDOWN with restrictions
			//Are we above a gamobject with the interface IEntity?
			if ((input & GameInputEvent.ONTARGET) > 0)
			{
				view.isFiring = true;
				input |= GameInputEvent.FIRE;
			}
		}

		private void KillPlayer()
		{
			this.gameObject.SetActive(false);
		}

		//When the View collides with something, dispatch the appropriate signal
		private void OnCollision(GameObject _colObject)
		{
			//For more complex situations a hit a hitCommand would be ideal
			if(_colObject.layer != 10)
			{
				destroyPlayerSignal.Dispatch (view, false);
			}
		}
		
		//When the View collides with something, dispatch the appropriate signal
		private void OnTrigger(GameObject _triggerObject)
		{
		}
	}
}

