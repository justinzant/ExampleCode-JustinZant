// All "Views" inhertit from an Abstract base class. This class caches components and stores base variables.

// The "View" for the player. This MonoBehaviour is attached to the player prefab inside Unity.
// It contains 2 movement types for both player controlled movement and cinematic movement.
// It sents signals to its mediator, who will decide what to do with it.
// it has a pre-determined turret (fire abilty) that can be accessed with other game signals.
// All base stats required to function and feel good are handled in this view.
// All gameplay influenced variables are handled in the config and/or gameModel.

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace virum.game
{
	public class PlayerView : View, IEntity
	{
		private PlayerMediator mediator { get; set; }

		// I consider View injection a bad thing, however; 
		// Since either dependend on the player they are injected in here
		[Inject (GenericElement.BACKGROUND)]
		public BackgroundView background { get; set; }

		[Inject]
		public FireProjectileSignal fireTurretSignal{ get; set; }

		//Internal Signal that notify the mediator
		internal Signal<GameObject> collisionSignal = new Signal<GameObject> ();
		internal Signal<GameObject> triggerSignal = new Signal<GameObject> ();
		internal Signal deathSignal = new Signal();

		//Camera
		private Transform camT;

		//Serilized within Unity - base settings
		[SerializeField] private float playerSpeed = 5F;
		[SerializeField] private float acceleration = 0.1F;
		[SerializeField] private float deceleration = 0.2F;
		[SerializeField] private float camSmoothTime = 0.165F;

		//Public
		public bool isPlayerControlled { get; set; }
		public bool isFiring { get; set; }
		public ITurret turretBehaviour{ get; set; }
		public GatewayView currentGateway { get; set; }

		//Private
		private int input;
		private float curSpeed;
		private Vector3 lastMousePos;


		//Fire when mediator is attached
		internal void Init()
		{
			//Set mediator
			mediator = GetComponent<PlayerMediator>();
			if (mediator == null){
				Debug.Log( "Mediator was not set on: " + gameObject );
				return;
			}

			//Movement related
			camT = mediator.cam.transform;
			StartCoroutine( MovementUpdate());

			//Set Turret behaviour, weapon type and listener
			turretBehaviour = new TurretBehaviour();
			turretBehaviour.StartTurret(this, TurretElement.BASIC_POOL);
			turretBehaviour.fireSignal.AddListener( OnFire );
			turretBehaviour.RetrieveManualControl();

			isPlayerControlled = true; //Movement Related
		}

		protected override void OnDestroy ()
		{
			turretBehaviour.fireSignal.RemoveListener( OnFire );
		}

		//Set the IME value
		internal void SetAction(int evt)
		{
			input = evt;
		}

		#region Player Movements
		//When the player follows a fixed line; Get the path connnected to it & information regarding the path 
		public void MoveOnLine( Vector2[] path , int startPoint, Vector2 endPoint )
		{
			if(!isPlayerControlled){
				isPlayerControlled = true;
				StartCoroutine( MoveAcrossLine(path, startPoint, endPoint) );
			}
		}

		/// <summary>
		/// Gets the current Gateway; This is called on Gateway selection by the player.
		/// </summary>
		public GatewayView GetCurrentGateWay()
		{
			Collider2D[] col = Physics2D.OverlapCircleAll((Vector2)_t.position, 2.5F);
			if(col == null){ return null; }

			int i = 0;
			while(i < col.Length)
			{
				GatewayView gv = col[i].GetComponent<GatewayView>();
				if(gv != null)
				{
					currentGateway = gv;
					return currentGateway;
				}
				i++;
			}
			return currentGateway;
		}

		private IEnumerator MoveAcrossLine(Vector2[] path, int startIndex, Vector2 endPoint)
		{
			Vector2 playerPos = _t.position;
			int curPoint = startIndex;
			int nPoints = path.Length;
			int step = 1;
			if(startIndex > 0 ){ step = -step; }

			Vector2 nextPoint = path[curPoint];

			while( playerPos != endPoint )
			{
				playerPos = _t.position;

				if(curPoint < nPoints && curPoint >= 0){
					//Increase or decrease
					nextPoint = path[curPoint];
					curPoint += step;
				}else {
					//Set Endpoint
					nextPoint = endPoint;
				}

				//After point is set for smooth transitions
				_t.position = Vector2.MoveTowards (playerPos, nextPoint, Time.deltaTime * curSpeed );
				yield return null;
			}
			isPlayerControlled = false;
			GetCurrentGateWay();
			currentGateway.tb.RetrieveManualControl();
		}

		private IEnumerator MovementUpdate()
		{
			lastMousePos = _t.position;
			curSpeed = 0;
			while( gameObject.activeSelf )
			{
				//Is played controlled by cinematic?
				if(!isPlayerControlled){ 
					yield return null;
				}else{
					Vector3 playerPos = _t.position;

					//Fire before moving
					if(isFiring)
					{
						//Sent the ITurret to command for Firing
						turretBehaviour.isFiring = true;

						//Set firing for single salvo shots - remove for manual control
						isFiring = false;
						curSpeed = 0;

						yield return new WaitForSeconds(0.05F);
					}else if((input & GameInputEvent.MOUSEHELDDOWN) > 0)
					{
						if(curSpeed < playerSpeed)
						{
							curSpeed += acceleration; 
						}

						lastMousePos = mediator.cam.ScreenToWorldPoint((Vector2)Input.mousePosition);
					}else{
						if(curSpeed - deceleration > 0)
						{
							curSpeed -= deceleration; 
						}else{
							curSpeed = 0;
						}
					}

					_t.position = Vector2.MoveTowards( playerPos, lastMousePos, Time.deltaTime * curSpeed);

					//Dispatch bitwise signal from here to ensure we are in the right frame
					mediator.gameInputSignal.Dispatch( input );
					yield return null;
				}
			}
		}
		#endregion

		private void OnFire()
		{
			fireTurretSignal.Dispatch( turretBehaviour, turretBehaviour.turretType);
		}

		private void CamUpdate(float smoothTime)
		{
			if(camT != null)
			{
				Vector3 cPos = camT.position;
				Vector2 pPos = _t.position;
				
				cPos.x = Mathf.SmoothDamp( cPos.x, pPos.x, ref curSpeed, smoothTime);
				cPos.y = Mathf.SmoothDamp( cPos.y, pPos.y, ref curSpeed, smoothTime);
				camT.position = cPos;
			}
		}

		void LateUpdate()
		{
			CamUpdate(camSmoothTime);
		}
	
		//Collision2D & Trigger2D Signals
		//Filtered by collision layers
		void OnTriggerEnter2D( Collider2D other ){
			GameObject triggeredObject = other.gameObject;

			if( triggeredObject.tag == "DeathZone" ){
				deathSignal.Dispatch();
			}else{
				triggerSignal.Dispatch( triggeredObject );
			}
		}

		void OnCollisionEnter2D(Collision2D other)
		{
			collisionSignal.Dispatch( other.gameObject );
		}
	}
}

