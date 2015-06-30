using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using virum.game;


public class TurretBehaviour : ITurret {
	//UNDONE: Setdata objects
	private SpriteData spriteData { get; set; }

	#region ITurret implementation
	public Signal fireSignal{ get; set; }
	public TurretElement turretType { get;set; }
	public Transform _t{ get; set; }
	public IEntity parentEntity { get; set; }
	public bool isFiring{ get; set; }
	public float Damage{ get; set; }
	public float Radius{ get; set; }
	public float FireDelay { get; set; }
	public int Salvo { get; set; }

	public bool IsPlayerControled { get; set; }
	public IRotate rotation{ get; set; }
	public EnemyView enemy { get; set; }
	#endregion

	protected IEnumerator fireMechanism;
	protected IEnumerator manualControl;

	protected Transform targetT;
	protected ITurret turretEntity;
	
	//Set turret element for variables
	public void StartTurret( IEntity _parentEntity, TurretElement _turretType ){
		rotation = new RotateToPoint2D();
		fireSignal = new Signal();

		parentEntity = _parentEntity;
		View view = parentEntity.view;
		turretEntity = view.GetComponent<PlayerView>().turretBehaviour;

		if( view == null || turretEntity == null ){
			Debug.Log( "Selected Entity does not not meet requirement, entity null " + (turretEntity == null) );
			return;
		}

		_t = view._t; 
		isFiring = true;
		SetNewTurretType( _turretType );
		fireMechanism = FireMechanism();
		parentEntity.view.StartCoroutine(fireMechanism);

	}
	
	//Gets nearest target
	private EnemyView GetTarget()
	{
		Collider2D _enemyCollider = Physics2D.OverlapCircle( _t.position, Radius, 1 << 8 );

		if(_enemyCollider != null)
		{
			enemy = _enemyCollider.GetComponent<EnemyView>();
			targetT = enemy._t;
			parentEntity.target = enemy;
			return enemy;
		}else{
			return null;
		}
	}

	//When loading data, load ID
	public void SetNewTurretType( TurretElement _turretType )
	{
		turretType = _turretType;
		Sprite[] sprite = Resources.LoadAll<Sprite>( "art/textures/SpriteSheetGeneric" );

		switch(turretType)
		{
		case TurretElement.BASIC_POOL:

			parentEntity.view._sr.sprite = sprite[5];
			Damage = 5;
			Radius = 10;
			FireDelay = 0.5F;
			Salvo = 3;
			break;

		case TurretElement.FIELD_POOL:
			
			parentEntity.view._sr.sprite = sprite[6];
			Damage = 5;
			Radius = 10;
			FireDelay = 1F;
			Salvo = 3;
			break;

		case TurretElement.LASER_POOL:
			
			parentEntity.view._sr.sprite = sprite[7];
			Damage = 5;
			Radius = 10;
			FireDelay = 5F;
			Salvo = 1;
			break;
		}
	}

	public void RetrieveManualControl()
	{
		if(fireMechanism == null){ return; }
		parentEntity.target = null;
		IsPlayerControled = true;
		parentEntity.view.StopCoroutine(fireMechanism);
		manualControl = ManualControl();
		parentEntity.view.StartCoroutine(manualControl);
	}
	
	public void ReturnAutomatedControl()
	{
		if(manualControl == null){ return; }
		IsPlayerControled = false;
		parentEntity.view.StopCoroutine(manualControl);
		fireMechanism = FireMechanism();
		parentEntity.view.StartCoroutine(fireMechanism);
	}
	
	private IEnumerator FireMechanism()
	{
		while( !IsPlayerControled )
		{
			if(enemy == null)
			{
				enemy = GetTarget();
			}

			if(enemy != null && !IsPlayerControled)
			{
				if( (targetT.position - _t.position).sqrMagnitude > Radius*Radius )
				{
					enemy = null;
					parentEntity.target = null;
				}else{
					rotation.RotateFixed( _t, targetT.position );
					for(int i = 0; i < Salvo; i++){
						fireSignal.Dispatch();
						yield return new WaitForSeconds(0.2F);
					}
				}
			}
			yield return new WaitForSeconds(FireDelay);
		}
	}

	//Player controlled
	private IEnumerator ManualControl()
	{
		isFiring = false;
		while( IsPlayerControled )
		{
			if(isFiring)
			{
				for(int i = 0; i < Salvo; i++){
					fireSignal.Dispatch();
					yield return new WaitForSeconds(0.2F);
				}
				isFiring = false;
				yield return new WaitForSeconds (FireDelay);
			}
			yield return null;
		}
	}
}
